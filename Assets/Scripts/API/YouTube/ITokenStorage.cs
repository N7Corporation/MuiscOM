using System.Threading.Tasks;

namespace MusicOM.API.YouTube
{
    public class AuthToken
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string TokenType { get; set; }
        public long ExpiresAt { get; set; }

        public bool IsExpired => System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= ExpiresAt - 60;
    }

    public interface ITokenStorage
    {
        Task<AuthToken> LoadTokenAsync();
        Task SaveTokenAsync(AuthToken token);
        Task DeleteTokenAsync();
        bool HasToken { get; }
    }
}
