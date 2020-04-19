using SpotifyAPI.Web;

namespace Cww.Core.Factories
{
    public interface ISpotifyApiFactory
    {
        SpotifyWebAPI GetSpotifyApi();
    }
}