using Payments.Application.DTOs.Checkout;
using Payments.Application.DTOs.Gateway;
using Payments.Application.DTOs.Transactions;
using Payments.Application.Interfaces;
using Payments.Application.Services.Interface;
using Payments.Domain.Enums;
using Shared.Application.Models;

namespace Payments.Application.Services
{
    public class CheckoutService : ICheckoutService
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly ICommissionRuleRepository _commissionRepo;
        private readonly IPaymentGatewayFactory _gatewayFactory;
        private readonly IPaymentsUnitOfWork _unitOfWork;

        public CheckoutService(
            ITransactionRepository transactionRepo,
            ICommissionRuleRepository commissionRepo,
            IPaymentGatewayFactory gatewayFactory,
            IPaymentsUnitOfWork unitOfWork)
        {
            _transactionRepo = transactionRepo;
            _commissionRepo = commissionRepo;
            _gatewayFactory = gatewayFactory;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CheckoutResponseDto>> InitiateAsync(
            InitiateCheckoutDto dto,
            Guid buyerId,
            string buyerName,
            string buyerEmail,
            string buyerPhone,
            string orderNumber,
            decimal orderAmount,
            int? categoryId,
            CancellationToken ct = default)
        {
            // 1. Resolve or create transaction
            var existingTx = await _transactionRepo.GetByOrderIdAsync(dto.OrderId, ct);

            if (existingTx != null && existingTx.Status != Payments.Domain.Enums.TransactionStatus.Pending)
                return Result<CheckoutResponseDto>.Failure(
                    "This order already has a completed or failed payment.",
                    "TRANSACTION_EXISTS");

            Payments.Domain.Entities.Transaction tx;

            if (existingTx == null)
            {
                // Resolve commission
                var commissionRule = categoryId.HasValue
                    ? await _commissionRepo.GetByCategoryIdAsync(categoryId.Value, ct)
                    : null;
                commissionRule ??= await _commissionRepo.GetDefaultAsync(ct);

                var rate = commissionRule?.RatePercent ?? 0m;
                var commission = Math.Round(orderAmount * rate / 100m, 2);

                if (!Enum.TryParse<PaymentMethod>(dto.Gateway == "COD" ? "COD" : "Card", out var method))
                    method = PaymentMethod.Card;

                tx = Payments.Domain.Entities.Transaction.Create(
                    orderId: dto.OrderId,
                    buyerId: buyerId,
                    sellerId: Guid.Empty,  // will be updated if known
                    storeId: Guid.Empty,
                    amount: orderAmount,
                    commissionAmount: commission,
                    currencyCode: "INR",
                    method: method,
                    createdBy: buyerId,
                    gatewayProvider: dto.Gateway);

                await _transactionRepo.AddAsync(tx, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }
            else
            {
                tx = existingTx;
            }

            // 2. Call gateway
            IPaymentGateway gateway;
            try { gateway = _gatewayFactory.GetGateway(dto.Gateway); }
            catch (InvalidOperationException ex)
            { return Result<CheckoutResponseDto>.Failure(ex.Message, "GATEWAY_NOT_FOUND"); }

            var gatewayRequest = new GatewayOrderRequest
            {
                TransactionId = tx.Id,
                OrderNumber = orderNumber,
                Amount = tx.Amount,
                CurrencyCode = "INR",
                BuyerName = buyerName,
                BuyerEmail = buyerEmail,
                BuyerPhone = buyerPhone,
                SuccessUrl = dto.SuccessUrl,
                FailureUrl = dto.FailureUrl,
                WebhookUrl = $"/api/payments/webhook/{dto.Gateway.ToLower()}",
                PreferredMethod = dto.PreferredMethod
            };

            var gatewayResult = await gateway.CreateOrderAsync(gatewayRequest, ct);

            if (!gatewayResult.Success)
                return Result<CheckoutResponseDto>.Failure(
                    $"Gateway error: {gatewayResult.Error}", "GATEWAY_ERROR");

            // 3. Store gateway order ID on transaction
            tx.SetGatewayOrderId(gatewayResult.GatewayOrderId!, gatewayResult.CheckoutUrl, buyerId);
            _transactionRepo.Update(tx);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<CheckoutResponseDto>.Success(new CheckoutResponseDto
            {
                TransactionId = tx.Id,
                GatewayOrderId = gatewayResult.GatewayOrderId!,
                Gateway = dto.Gateway,
                CheckoutUrl = gatewayResult.CheckoutUrl,
                SdkOptions = gatewayResult.Metadata
            });
        }

        public async Task<Result<bool>> VerifyAsync(
            VerifyPaymentDto dto, CancellationToken ct = default)
        {
            var tx = await _transactionRepo.GetByIdAsync(dto.TransactionId, ct);
            if (tx is null)
                return Result<bool>.Failure("Transaction not found.", "TRANSACTION_NOT_FOUND");

            if (tx.Status != Payments.Domain.Enums.TransactionStatus.Pending)
                return Result<bool>.Success(tx.Status == Payments.Domain.Enums.TransactionStatus.Completed);

            IPaymentGateway gateway;
            try { gateway = _gatewayFactory.GetGateway(tx.GatewayProvider ?? "Razorpay"); }
            catch (InvalidOperationException ex)
            { return Result<bool>.Failure(ex.Message, "GATEWAY_NOT_FOUND"); }

            var verifyRequest = new GatewayVerifyRequest
            {
                TransactionId = tx.Id,
                GatewayOrderId = dto.GatewayOrderId,
                GatewayPaymentId = dto.GatewayPaymentId,
                GatewaySignature = dto.GatewaySignature,
                CallbackParams = dto.CallbackParams
            };

            var verifyResult = await gateway.VerifyPaymentAsync(verifyRequest, ct);

            if (verifyResult.Success)
                tx.MarkCompleted(verifyResult.GatewayTransactionId!, verifyResult.GatewayResponse, tx.BuyerId);
            else
                tx.MarkFailed(verifyResult.Error, tx.BuyerId);

            _transactionRepo.Update(tx);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<bool>.Success(verifyResult.Success);
        }

        public async Task<Result> HandleWebhookAsync(
            string gatewayName,
            string rawBody,
            IReadOnlyDictionary<string, string> headers,
            CancellationToken ct = default)
        {
            IPaymentGateway gateway;
            try { gateway = _gatewayFactory.GetGateway(gatewayName); }
            catch (InvalidOperationException ex)
            { return Result.Failure(ex.Message, "GATEWAY_NOT_FOUND"); }

            var webhookResult = gateway.ParseWebhook(rawBody, headers);
            if (webhookResult is null)
                return Result.Failure("Webhook signature validation failed.", "INVALID_WEBHOOK");

            // Find transaction by gateway order ID
            // GatewayOrderId is stored in Transaction.GatewayOrderId
            var transactions = (await _transactionRepo.GetAllAsync(ct)).ToList();
            var tx = transactions.FirstOrDefault(t => t.GatewayOrderId == webhookResult.GatewayOrderId);

            if (tx is null)
                return Result.Failure($"No transaction found for gateway order {webhookResult.GatewayOrderId}.", "TRANSACTION_NOT_FOUND");

            if (tx.Status != Payments.Domain.Enums.TransactionStatus.Pending)
                return Result.Success();  // idempotent — already processed

            if (webhookResult.IsSuccess)
                tx.MarkCompleted(webhookResult.GatewayPaymentId, webhookResult.RawBody, Guid.Empty);
            else if (webhookResult.IsFailure)
                tx.MarkFailed(webhookResult.Status, Guid.Empty);

            _transactionRepo.Update(tx);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}