using Catalog.Application.DTOs;
using Catalog.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Extensions;

namespace Web.API.Controllers.Admin
{
    [Route("api/admin/attribute-templates")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminAttributeTemplatesController : BaseController
    {
        private readonly IAttributeTemplateService _templateService;

        public AdminAttributeTemplatesController(
            IAttributeTemplateService templateService)
        {
            _templateService = templateService;
        }

        // GET api/admin/attribute-templates
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _templateService.GetAllAsync(ct);
            return HandleResult(result);
        }

        // GET api/admin/attribute-templates/{id}
        [HttpGet("{id:int}", Name = "GetTemplateById")]
        public async Task<IActionResult> GetById(
            int id, CancellationToken ct)
        {
            var result = await _templateService.GetByIdAsync(id, ct);
            return HandleResult(result);
        }

        // GET api/admin/attribute-templates/by-category/{categoryId}
        [HttpGet("by-category/{categoryId:int}")]
        public async Task<IActionResult> GetByCategoryId(
            int categoryId, CancellationToken ct)
        {
            var result = await _templateService
                .GetByCategoryIdAsync(categoryId, ct);
            return HandleResult(result);
        }

        // POST api/admin/attribute-templates
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateAttributeTemplateDto dto,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _templateService.CreateAsync(dto, adminId, ct);
            return HandleCreated(result, "GetTemplateById",
                new { id = result.Value?.Id });
        }

        // PUT api/admin/attribute-templates/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateAttributeTemplateDto dto,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _templateService.UpdateAsync(id, dto, adminId, ct);
            return HandleResult(result);
        }

        // DELETE api/admin/attribute-templates/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(
            int id, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _templateService.DeleteAsync(id, adminId, ct);
            return HandleResult(result);
        }
    }
}
