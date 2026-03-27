namespace Identity.Application.DTOs.User
{
    public class RegisterDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        // Role defaults to Buyer — only internal seeding creates Admin
        public string Role { get; set; } = "Buyer";
    }
}
