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
        // Converts Result<T> to appropriate HTTP response
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
                return Ok(ApiResponse<T>.Ok(result.Value!));

            return result.ErrorCode switch
            {
                "NOT_FOUND" or
                "PRODUCT_NOT_FOUND" or
                "CATEGORY_NOT_FOUND" or
                "BRAND_NOT_FOUND" or
                "TAG_NOT_FOUND" or
                "USER_NOT_FOUND" or
                "TEMPLATE_NOT_FOUND" or
                "VARIANT_NOT_FOUND" or
                "IMAGE_NOT_FOUND"
                    => NotFound(ApiResponse<T>.Fail(
                        result.Error!, result.ErrorCode, 404)),

                "VALIDATION_FAILED"
                    => BadRequest(ApiResponse<T>.Fail(
                        result.Error!, result.ErrorCode, 400)),

                "INVALID_CREDENTIALS" or
                "INVALID_REFRESH_TOKEN"
                    => Unauthorized(ApiResponse<T>.Fail(
                        result.Error!, result.ErrorCode, 401)),

                "ACCOUNT_LOCKED" or
                "ACCOUNT_INACTIVE" or
                "ACCOUNT_SUSPENDED"
                    => StatusCode(403, ApiResponse<T>.Fail(
                        result.Error!, result.ErrorCode, 403)),

                "SLUG_EXISTS" or
                "SKU_EXISTS" or
                "NAME_EXISTS" or
                "EMAIL_EXISTS" or
                "USER_ALREADY_EXISTS" or
                "TEMPLATE_EXISTS" or
                "VARIANT_NAME_EXISTS"
                    => Conflict(ApiResponse<T>.Fail(
                        result.Error!, result.ErrorCode, 409)),

                "INVALID_STATUS_TRANSITION" or
                "PRODUCT_NOT_ACTIVE" or
                "HAS_CHILDREN" or
                "HAS_PRODUCTS" or
                "LAST_VARIANT" or
                "MAX_IMAGES_REACHED"
                    => UnprocessableEntity(ApiResponse<T>.Fail(
                        result.Error!, result.ErrorCode, 422)),

                _ => BadRequest(ApiResponse<T>.Fail(
                        result.Error!, result.ErrorCode, 400))
            };
        }

        // Non-generic version for commands
        protected IActionResult HandleResult(Result result)
        {
            if (result.IsSuccess)
                return Ok(ApiResponse.Ok());

            return result.ErrorCode switch
            {
                "NOT_FOUND" or
                "PRODUCT_NOT_FOUND" or
                "CATEGORY_NOT_FOUND" or
                "BRAND_NOT_FOUND" or
                "TAG_NOT_FOUND" or
                "USER_NOT_FOUND" or
                "TEMPLATE_NOT_FOUND" or
                "VARIANT_NOT_FOUND" or
                "IMAGE_NOT_FOUND"
                    => NotFound(ApiResponse.Fail(
                        result.Error!, result.ErrorCode, 404)),

                "VALIDATION_FAILED"
                    => BadRequest(ApiResponse.Fail(
                        result.Error!, result.ErrorCode, 400)),

                "INVALID_CREDENTIALS" or
                "INVALID_REFRESH_TOKEN"
                    => Unauthorized(ApiResponse.Fail(
                        result.Error!, result.ErrorCode, 401)),

                "SLUG_EXISTS" or
                "SKU_EXISTS" or
                "NAME_EXISTS" or
                "EMAIL_EXISTS" or
                "VARIANT_NAME_EXISTS"
                    => Conflict(ApiResponse.Fail(
                        result.Error!, result.ErrorCode, 409)),

                "INVALID_STATUS_TRANSITION" or
                "PRODUCT_NOT_ACTIVE" or
                "HAS_CHILDREN" or
                "HAS_PRODUCTS" or
                "LAST_VARIANT" or
                "MAX_IMAGES_REACHED"
                    => UnprocessableEntity(ApiResponse.Fail(
                        result.Error!, result.ErrorCode, 422)),

                _ => BadRequest(ApiResponse.Fail(
                        result.Error!, result.ErrorCode, 400))
            };
        }

        // Created response for POST endpoints
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
