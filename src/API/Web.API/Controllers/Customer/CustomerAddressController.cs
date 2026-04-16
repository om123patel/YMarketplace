// src/API/Web.API/Controllers/Customer/CustomerAddressController.cs

using Identity.Application.DTOs.Address;
using Identity.Application.Services;
using Identity.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Extensions;

namespace Web.API.Controllers.Customer
{
    [Route("api/buyer/addresses")]
    [Authorize(Policy = "BuyerOnly")]
    public class CustomerAddressController : BaseController
    {
        private readonly ICustomerAddressService _addressService;

        public CustomerAddressController(ICustomerAddressService addressService)
        {
            _addressService = addressService;
        }

        // GET api/buyer/addresses
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var userId = User.GetUserId();
            var result = await _addressService.GetAllAsync(userId, ct);
            return HandleResult(result);
        }

        // GET api/buyer/addresses/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var result = await _addressService.GetByIdAsync(id, userId, ct);
            return HandleResult(result);
        }

        // POST api/buyer/addresses
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateAddressDto dto, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var result = await _addressService.CreateAsync(dto, userId, ct);
            return HandleResult(result);
        }

        // PUT api/buyer/addresses/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id, [FromBody] CreateAddressDto dto, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var result = await _addressService.UpdateAsync(id, dto, userId, ct);
            return HandleResult(result);
        }

        // DELETE api/buyer/addresses/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var result = await _addressService.DeleteAsync(id, userId, ct);
            return HandleResult(result);
        }

        // PATCH api/buyer/addresses/{id}/set-default
        [HttpPatch("{id:guid}/set-default")]
        public async Task<IActionResult> SetDefault(Guid id, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var result = await _addressService.SetDefaultAsync(id, userId, ct);
            return HandleResult(result);
        }
    }
}