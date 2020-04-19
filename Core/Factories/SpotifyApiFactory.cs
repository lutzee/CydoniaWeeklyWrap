using Microsoft.Extensions.Configuration;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace Cww.Core.Factories
{
    public class SpotifyApiFactory : ISpotifyApiFactory
    {
        private readonly IConfiguration configuration;

        public SpotifyApiFactory(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public SpotifyWebAPI GetSpotifyApi()
        {

            var auth = new CredentialsAuth(configuration["Spotify:ClientId"], configuration["Spotify:ClientSecret"]);
            var token = auth.GetToken().GetAwaiter().GetResult();

            return new SpotifyWebAPI
            {
                AccessToken = token.AccessToken,
                TokenType = "Bearer"
            };
        }
    }
}
