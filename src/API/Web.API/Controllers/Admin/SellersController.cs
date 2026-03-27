using Identity.Application.DTOs.Seller;
using Identity.Application.Services;
using Identity.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        #region Create

        /// <summary>
        /// Create new seller
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateSellerDto command,
            CancellationToken cancellationToken)
        {
            var id = await _sellerService.CreateAsync(command, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        #endregion

        #region Get

        /// <summary>
        /// Get seller by Id
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _sellerService.GetByIdAsync(id, cancellationToken);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// Get all sellers
        /// </summary>
        [HttpGet("GetPaged")]
        public async Task<IActionResult> GetAll(
             int page = 1, int pageSize = 20,
             string? status = null,string? search = null,
             string? sortBy = null, string? sortDirection = null,
             CancellationToken ct = default)
        {
            var result = await _sellerService.GetPagedAsync(
                page, pageSize, status, search, sortBy, sortDirection, ct);
            return HandleResult(result);
        }

        #endregion

        #region Update

        /// <summary>
        /// Update seller profile
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateSellerDto command,
            CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest("Id mismatch");

            await _sellerService.UpdateAsync(command, cancellationToken);

            return NoContent();
        }

        #endregion

        #region Status Management

        /// <summary>
        /// Approve seller (Admin)
        /// </summary>
        [HttpPost("{id:guid}/approve")]
        public async Task<IActionResult> Approve(
            Guid id,
            [FromQuery] Guid adminId,
            CancellationToken cancellationToken)
        {
            await _sellerService.ApproveAsync(id, adminId, cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Reject seller (Admin)
        /// </summary>
        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> Reject(
            Guid id,
            [FromQuery] Guid adminId,
            [FromBody] RejectSellerRequest request,
            CancellationToken cancellationToken)
        {
            await _sellerService.RejectAsync(id, adminId, request.Reason, cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Suspend seller
        /// </summary>
        [HttpPost("{id:guid}/suspend")]
        public async Task<IActionResult> Suspend(Guid id, CancellationToken cancellationToken)
        {
            await _sellerService.SuspendAsync(id, cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Activate seller
        /// </summary>
        [HttpPost("{id:guid}/activate")]
        public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
        {
            await _sellerService.ActivateAsync(id, cancellationToken);

            return NoContent();
        }

        #endregion
    }
}
