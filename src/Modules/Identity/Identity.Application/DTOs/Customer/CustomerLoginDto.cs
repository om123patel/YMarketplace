// src/Modules/Identity/Identity.Application/DTOs/Customer/CustomerLoginDto.cs
namespace Identity.Application.DTOs.Customer
{
    public class CustomerLoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}