using Identity.Application.DTOs;
using Identity.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Extensions;

namespace Web.API.Controllers.Admin
{
    [Route("api/admin/vendors")]
    [Authorize(Roles = "Admin")]
    public class VendorApplicationsController : BaseController
    {
        private readonly IVendorOnboardingService _vendorService;

        public VendorApplicationsController(IVendorOnboardingService vendorService)
            => _vendorService = vendorService;

        // GET api/admin/vendors/applications?page=1&pageSize=20
        [HttpGet("applications")]
        public async Task<IActionResult> GetPending(
            int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            var result = await _vendorService.GetPendingAsync(page, pageSize, ct);
            return HandleResult(result);
        }

        // GET api/admin/vendors/applications/{id}
        [HttpGet("applications/{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await _vendorService.GetByIdAsync(id, ct);
            return HandleResult(result);
        }

        // POST api/admin/vendors/applications/{id}/approve
        [HttpPost("applications/{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _vendorService.ApproveAsync(id, adminId, ct);
            return HandleResult(result);
        }

        // POST api/admin/vendors/applications/{id}/reject
        [HttpPost("applications/{id:guid}/reject")]
        public async Task<IActionResult> Reject(
            Guid id, [FromBody] RejectVendorDto dto, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _vendorService.RejectAsync(
                id, adminId, dto.Reason, ct);
            return HandleResult(result);
        }
    }
}