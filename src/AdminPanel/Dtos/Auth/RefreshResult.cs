namespace AdminPanel.Dtos.Auth
{
    public class RefreshResult
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
