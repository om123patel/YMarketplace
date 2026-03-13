namespace AdminPanel.Services
{
    public class AuthTokenService
    {
        private const string TokenKey = "jwt_access_token";
        private const string RefreshKey = "jwt_refresh_token";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthTokenService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session
            => _httpContextAccessor.HttpContext!.Session;

        public void StoreTokens(string accessToken, string refreshToken)
        {
            Session.SetString(TokenKey, accessToken);
            Session.SetString(RefreshKey, refreshToken);
        }

        public string? GetAccessToken()
            => Session.GetString(TokenKey);

        public string? GetRefreshToken()
            => Session.GetString(RefreshKey);

        public void ClearTokens()
        {
            Session.Remove(TokenKey);
            Session.Remove(RefreshKey);
            Session.Clear();
        }
    }

}
