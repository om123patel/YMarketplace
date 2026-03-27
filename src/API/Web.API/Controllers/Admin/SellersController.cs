// src/API/Web.API/Controllers/Admin/SellersController.cs
using Identity.Application.DTOs.Seller;
using Identity.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Extensions;
using Web.API.Models;

namespace Web.API.Controllers.Admin
{
    [Route("api/admin/Sellers")]
    [Authorize(Roles = "Admin")]
    public class SellersController : BaseController
    {
        private readonly ISellerService _sellerService;

        public SellersController(ISellerService sellerService)
        {
            _sellerService = sellerService;
        }

        // POST api/admin/Sellers
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateSellerDto command,
            CancellationToken cancellationToken)
        {
            var result = await _sellerService.CreateAsync(command, cancellationToken);
            return HandleResult(result);
        }

        // GET api/admin/Sellers/{id}
        [HttpGet("{id:guid}", Name = "GetSellerById")]
        public async Task<IActionResult> GetById(
            Guid id, CancellationToken cancellationToken)
        {
            var result = await _sellerService.GetByIdAsync(id, cancellationToken);
            return HandleResult(result);
        }

        // GET api/admin/Sellers/GetPaged
        [HttpGet("GetPaged")]
        public async Task<IActionResult> GetAll(
            int page = 1, int pageSize = 20,
            string? status = null, string? search = null,
            string? sortBy = null, string? sortDirection = null,
            CancellationToken ct = default)
        {
            var result = await _sellerService.GetPagedAsync(
                page, pageSize, status, search, sortBy, sortDirection, ct);
            return HandleResult(result);
        }

        // PUT api/admin/Sellers/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateSellerDto command,
            CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest(ApiResponse.Fail("Id mismatch", "BAD_REQUEST", 400));

            var result = await _sellerService.UpdateAsync(command, cancellationToken);
            return HandleResult(result);
        }

        // PATCH api/admin/Sellers/{id}/approve
        [HttpPatch("{id:guid}/approve")]
        public async Task<IActionResult> Approve(
            Guid id,
            CancellationToken cancellationToken)
        {
            var adminId = User.GetUserId();
            var result = await _sellerService.ApproveAsync(id, adminId, cancellationToken);
            return HandleResult(result);
        }

        // PATCH api/admin/Sellers/{id}/reject
        [HttpPatch("{id:guid}/reject")]
        public async Task<IActionResult> Reject(
            Guid id,
            [FromBody] RejectSellerRequest request,
            CancellationToken cancellationToken)
        {
            var adminId = User.GetUserId();
            var result = await _sellerService.RejectAsync(
                id, adminId, request.Reason, cancellationToken);
            return HandleResult(result);
        }

        // PATCH api/admin/Sellers/{id}/suspend
        [HttpPatch("{id:guid}/suspend")]
        public async Task<IActionResult> Suspend(
            Guid id,
            [FromBody] SuspendSellerRequest? request,
            CancellationToken cancellationToken)
        {
            var result = await _sellerService.SuspendAsync(id, cancellationToken);
            return HandleResult(result);
        }

        // PATCH api/admin/Sellers/{id}/activate
        [HttpPatch("{id:guid}/activate")]
        public async Task<IActionResult> Activate(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _sellerService.ActivateAsync(id, cancellationToken);
            return HandleResult(result);
        }
    }
}