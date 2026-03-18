using Identity.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Extensions;

namespace Web.API.Controllers.Admin
{
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
            => _userService = userService;

        // GET api/admin/users?page=1&pageSize=20&role=Seller&status=Active&search=john
        [HttpGet]
        public async Task<IActionResult> GetAll(
            int page = 1, int pageSize = 20,
            string? role = null, string? status = null,
            string? search = null,
            CancellationToken ct = default)
        {
            var result = await _userService.GetPagedAsync(
                page, pageSize, role, status, search, ct);
            return HandleResult(result);
        }

        // GET api/admin/users/{id}
        [HttpGet("{id:guid}", Name = "GetUserById")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await _userService.GetByIdAsync(id, ct);
            return HandleResult(result);
        }

        // POST api/admin/users/{id}/suspend
        [HttpPost("{id:guid}/suspend")]
        public async Task<IActionResult> Suspend(Guid id, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _userService.SuspendAsync(id, adminId, ct);
            return HandleResult(result);
        }

        // POST api/admin/users/{id}/activate
        [HttpPost("{id:guid}/activate")]
        public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _userService.ActivateAsync(id, adminId, ct);
            return HandleResult(result);
        }
    }
}