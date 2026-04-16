// src/Modules/Identity/Identity.Application/DTOs/Customer/CustomerLogoutDto.cs
namespace Identity.Application.DTOs.Customer
{
    public class CustomerLogoutDto
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}