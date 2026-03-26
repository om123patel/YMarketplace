using Catalog.Application.DTOs.Categories;
using Catalog.Application.DTOs.Tags;
using Catalog.Application.Services;
using Catalog.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Extensions;

namespace Web.API.Controllers.Admin
{
    [Route("api/admin/tags")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminTagsController : BaseController
    {
        private readonly ITagService _tagService;

        public AdminTagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        [HttpGet("GetPaged")]
        public async Task<IActionResult> GetPaged(
           [FromQuery] TagFilterRequest filter,
           CancellationToken ct)
        {
            var result = await _tagService.GetPagedAsync(filter, ct);
            return HandleResult(result);
        }

        // GET api/admin/tags
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _tagService.GetAllAsync(ct);
            return HandleResult(result);
        }

        // GET api/admin/tags/{id}
        [HttpGet("{id:int}", Name = "GetTagById")]
        public async Task<IActionResult> GetById(
            int id, CancellationToken ct)
        {
            var result = await _tagService.GetByIdAsync(id, ct);
            return HandleResult(result);
        }

        // POST api/admin/tags
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateTagDto dto,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _tagService.CreateAsync(dto, adminId, ct);
            return HandleCreated(result, "GetTagById",
                new { id = result.Value?.Id });
        }

        // PUT api/admin/tags/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateTagDto dto,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _tagService.UpdateAsync(id, dto, adminId, ct);
            return HandleResult(result);
        }

        // DELETE api/admin/tags/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(
            int id, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _tagService.DeleteAsync(id, adminId, ct);
            return HandleResult(result);
        }
    }
}
