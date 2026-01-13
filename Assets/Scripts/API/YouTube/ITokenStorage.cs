using System;
using System.Threading.Tasks;

namespace MusicOM.API.YouTube
{
    [Serializable]
    public class AuthToken
    {
        public string accessToken;
        public string refreshToken;
        public string tokenType;
        public long expiresAt;

        public bool IsExpired => DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= expiresAt - 60;
    }

    public interface ITokenStorage
    {
        Task<AuthToken> LoadTokenAsync();
        Task SaveTokenAsync(AuthToken token);
        Task DeleteTokenAsync();
        bool HasToken { get; }
    }
}
