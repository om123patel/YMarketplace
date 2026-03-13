namespace AdminPanel.Dtos.Auth
{
    public class LoginResult
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; set; }
        public DateTime RefreshTokenExpiresAt { get; set; }

        // Nested user object — NOT flat properties
        public UserResult User { get; set; } = new();
    }
   

  

}
