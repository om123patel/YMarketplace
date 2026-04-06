using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payments.Application.DTOs.CommissionRules;
using Payments.Application.Services.Interface;
using Web.API.Controllers;
using Web.API.Extensions;

namespace API.Controllers.Admin
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/commission-rules")]
    public class AdminCommissionController : BaseController
    {
        private readonly ICommissionRuleService _service;

        public AdminCommissionController(ICommissionRuleService service)
            => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _service.GetAllAsync(ct);
            return HandleResult(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            return HandleResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateCommissionRuleDto dto,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _service.CreateAsync(dto, adminId, ct);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById),
                    new { id = result.Value!.Id }, result)
                : HandleResult(result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] UpdateCommissionRuleDto dto,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _service.UpdateAsync(id, dto, adminId, ct);
            return HandleResult(result);
        }

        [HttpPost("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _service.ActivateAsync(id, adminId, ct);
            return HandleResult(result);
        }

        [HttpPost("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _service.DeactivateAsync(id, adminId, ct);
            return HandleResult(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _service.DeleteAsync(id, adminId, ct);
            return HandleResult(result);
        }
    }
}
