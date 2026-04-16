// src/API/Web.API/Controllers/Customer/CustomerWalletController.cs

using Identity.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Extensions;

namespace Web.API.Controllers.Customer
{
    [Route("api/buyer/wallet")]
    [Authorize(Policy = "BuyerOnly")]
    public class CustomerWalletController : BaseController
    {
        private readonly IWalletService _walletService;

        public CustomerWalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        // GET api/buyer/wallet
        [HttpGet]
        public async Task<IActionResult> GetBalance(CancellationToken ct)
        {
            var userId = User.GetUserId();
            var result = await _walletService.GetBalanceAsync(userId, ct);
            return HandleResult(result);
        }

        // GET api/buyer/wallet/history?page=1&pageSize=20
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory(
            int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            var userId = User.GetUserId();
            var result = await _walletService.GetHistoryAsync(userId, page, pageSize, ct);
            return HandleResult(result);
        }

        // POST api/buyer/wallet/add-money
        [HttpPost("add-money")]
        public async Task<IActionResult> AddMoney(
            [FromBody] AddMoneyRequest req, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var result = await _walletService.AddMoneyAsync(
                userId, req.Amount, req.ReferenceId, ct);
            return HandleResult(result);
        }
    }

    public record AddMoneyRequest(decimal Amount, string? ReferenceId);
}