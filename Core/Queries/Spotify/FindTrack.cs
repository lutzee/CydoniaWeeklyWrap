using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cww.Core.Factories;
using F23.StringSimilarity;
using MediatR;
using Serilog;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

namespace Cww.Core.Queries.Spotify
{
    public class FindTrack
    {
        public class Query : IRequest<FullTrack>
        {
            public string TrackName { get; set; }

            public string ArtistName { get; set; }
        }

        public class Handler : IRequestHandler<Query, FullTrack>
        {
            private readonly ISpotifyApiFactory apiFactory;

            public Handler(ISpotifyApiFactory apiFactory)
            {
                this.apiFactory = apiFactory;
            }

            public async Task<FullTrack> Handle(
                Query message,
                CancellationToken cancellationToken)
            {
                var api = apiFactory.GetSpotifyApi();
                var result = await api.SearchItemsAsync($"{message.TrackName} artist:{message.ArtistName}", SearchType.Track, 1);

                var damerau = new Damerau();
                var i = result.Tracks?.Items?.Where(t => FindBestMatch(t, message, damerau)).FirstOrDefault();
                return i;
            }

            private bool FindBestMatch(FullTrack track, Query message, Damerau damerau)
            {
                var distance = damerau.Distance(track.Name, message.TrackName);
                Log.Logger.Debug($"Checking {track.Name} against {message.TrackName} ({distance})");

                if (distance <= 2)
                {
                    return true;
                }

                return false;
            }
        }
    }
}
