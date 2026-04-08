using Microsoft.AspNetCore.Mvc;
using Shared.Application.Models;
using Web.API.Models;

namespace Web.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
                return Ok(ApiResponse<T>.Ok(result.Value!));

            return result.ErrorCode switch
            {
                // ── 404 Not Found ──────────────────────────────────────
                "NOT_FOUND" or
                "PRODUCT_NOT_FOUND" or
                "CATEGORY_NOT_FOUND" or
                "BRAND_NOT_FOUND" or
                "TAG_NOT_FOUND" or
                "USER_NOT_FOUND" or
                "TEMPLATE_NOT_FOUND" or
                "VARIANT_NOT_FOUND" or
                "IMAGE_NOT_FOUND" or
                "ORDER_NOT_FOUND" or
                "DISPUTE_NOT_FOUND" or
                "CART_NOT_FOUND" or
                "CART_ITEM_NOT_FOUND" or
                "SELLER_NOT_FOUND" or 
                "TRANSACTION_NOT_FOUND" or
                "PAYOUT_NOT_FOUND" or
                "COMMISSION_RULE_NOT_FOUND" or
                "TRANSACTION_EXISTS" or
                "PENDING_PAYOUT_EXISTS" or
                "INSUFFICIENT_BALANCE" or
                "INVALID_PAYOUT_STATUS_TRANSITION" or
                "INVALID_STATUS_TRANSITION" or
                "INVALID_PAYOUT_AMOUNT" or
                "INVALID_REFUND_AMOUNT" or
                "STOCK_NOT_FOUND" or
                "STOCK_EXISTS" or
                "INSUFFICIENT_STOCK" 
                    => NotFound(ApiResponse<T>.Fail(
                        result.Error!, result.ErrorCode, 404)),

                // ── 400 Validation ─────────────────────────────────────
                "VALIDATION_FAILED"
                    => BadRequest(ApiResponse<T>.Fail(
                        result.Error!, result.ErrorCode, 400)),

                // ── 401 Unauthorized ───────────────────────────────────
                "INVALID_CREDENTIALS" or
                "INVALID_REFRESH_TOKEN"
                    => Unauthorized(ApiResponse<T>.Fail(
                        result.Error!, result.ErrorCode, 401)),

                // ── 403 Forbidden ──────────────────────────────────────
                "ACCOUNT_LOCKED" or
                "ACCOUNT_INACTIVE" or
                "ACCOUNT_SUSPENDED"
                    => StatusCode(403, ApiResponse<T>.Fail(
                        result.Error!, result.ErrorCode, 403)),

                // ── 409 Conflict ───────────────────────────────────────
                "SLUG_EXISTS" or
                "SKU_EXISTS" or
                "NAME_EXISTS" or
                "EMAIL_EXISTS" or
                "USER_ALREADY_EXISTS" or
                "TEMPLATE_EXISTS" or
                "VARIANT_NAME_EXISTS" or
                "SELLER_ALREADY_EXISTS" or
                "ACTIVE_DISPUTE_EXISTS"
                    => Conflict(ApiResponse<T>.Fail(
                        result.Error!, result.ErrorCode, 409)),

                // ── 422 Unprocessable ──────────────────────────────────
                "INVALID_STATUS_TRANSITION" or
                "PRODUCT_NOT_ACTIVE" or
                "HAS_CHILDREN" or
                "HAS_PRODUCTS" or
                "LAST_VARIANT" or
                "MAX_IMAGES_REACHED" or
                "ORDER_EMPTY" or
                "DISPUTE_ALREADY_RESOLVED" or
                "INVALID_DISPUTE" or
                "INVALID_DISPUTE_STATUS" or
                "INVALID_ORDER"
                    => UnprocessableEntity(ApiResponse<T>.Fail(
                        result.Error!, result.ErrorCode, 422)),

                _ => BadRequest(ApiResponse<T>.Fail(
                        result.Error!, result.ErrorCode, 400))
            };
        }

        protected IActionResult HandleResult(Result result)
        {
            if (result.IsSuccess)
                return Ok(ApiResponse.Ok());

            return result.ErrorCode switch
            {
                // ── 404 Not Found ──────────────────────────────────────
                "NOT_FOUND" or
                "PRODUCT_NOT_FOUND" or
                "CATEGORY_NOT_FOUND" or
                "BRAND_NOT_FOUND" or
                "TAG_NOT_FOUND" or
                "USER_NOT_FOUND" or
                "TEMPLATE_NOT_FOUND" or
                "VARIANT_NOT_FOUND" or
                "IMAGE_NOT_FOUND" or
                "ORDER_NOT_FOUND" or
                "DISPUTE_NOT_FOUND" or
                "CART_NOT_FOUND" or
                "CART_ITEM_NOT_FOUND" or
                "SELLER_NOT_FOUND"
                    => NotFound(ApiResponse.Fail(
                        result.Error!, result.ErrorCode, 404)),

                // ── 400 Validation ─────────────────────────────────────
                "VALIDATION_FAILED"
                    => BadRequest(ApiResponse.Fail(
                        result.Error!, result.ErrorCode, 400)),

                // ── 401 Unauthorized ───────────────────────────────────
                "INVALID_CREDENTIALS" or
                "INVALID_REFRESH_TOKEN"
                    => Unauthorized(ApiResponse.Fail(
                        result.Error!, result.ErrorCode, 401)),

                // ── 409 Conflict ───────────────────────────────────────
                "SLUG_EXISTS" or
                "SKU_EXISTS" or
                "NAME_EXISTS" or
                "EMAIL_EXISTS" or
                "VARIANT_NAME_EXISTS" or
                "SELLER_ALREADY_EXISTS" or
                "ACTIVE_DISPUTE_EXISTS"
                    => Conflict(ApiResponse.Fail(
                        result.Error!, result.ErrorCode, 409)),

                // ── 422 Unprocessable ──────────────────────────────────
                "INVALID_STATUS_TRANSITION" or
                "PRODUCT_NOT_ACTIVE" or
                "HAS_CHILDREN" or
                "HAS_PRODUCTS" or
                "LAST_VARIANT" or
                "MAX_IMAGES_REACHED" or
                "ORDER_EMPTY" or
                "DISPUTE_ALREADY_RESOLVED" or
                "INVALID_DISPUTE" or
                "INVALID_DISPUTE_STATUS" or
                "INVALID_ORDER"
                    => UnprocessableEntity(ApiResponse.Fail(
                        result.Error!, result.ErrorCode, 422)),

                _ => BadRequest(ApiResponse.Fail(
                        result.Error!, result.ErrorCode, 400))
            };
        }

        protected IActionResult HandleCreated<T>(
            Result<T> result, string routeName, object routeValues)
        {
            if (result.IsSuccess)
                return CreatedAtRoute(
                    routeName,
                    routeValues,
                    ApiResponse<T>.Ok(result.Value!, 201));

            return HandleResult(result);
        }
    }
}