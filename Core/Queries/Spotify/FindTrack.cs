using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cww.Core.Factories;
using MediatR;
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
                var result = await api.SearchItemsAsync($"track:{message.TrackName} artist:{message.ArtistName}", SearchType.Track, 1);

                var i = result.Tracks?.Items?.FirstOrDefault();
                return i;
            }
        }
    }
}
