using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IF.Lastfm.Core.Objects;

namespace Cww.Core.Models
{
    public class Track
    {
        public Track()
        {
        }
        public Track(LastTrack lastTrack)
        {
            TrackName = lastTrack.Name;
            ArtistName = lastTrack.ArtistName;
            Mbid = lastTrack.Mbid;
            PlayCount = lastTrack.PlayCount;
            Url = lastTrack.Url;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TrackId { get; set; }

        public string TrackName { get; set; }

        public string ArtistName { get; set; }

        public Uri Url { get; set; }

        public int? PlayCount { get; set; }

        public string Mbid { get; set; }

        public string MostListens { get; set; }

        public string SpotifyUrl { get; set; }

        public string SpotifyUid { get; set; }
    }

    public class UserTrack : Track
    {
        public static UserTrack Create(Track track)
        {
            return new UserTrack
            {
                TrackId = track.TrackId,
                TrackName = track.TrackName,
                ArtistName = track.ArtistName,
                Url = track.Url,
                PlayCount = track.PlayCount,
                Mbid = track.Mbid,
                MostListens = track.MostListens,
                SpotifyUrl = track.SpotifyUrl,
                SpotifyUid = track.SpotifyUid
            };
        }

        public string Username { get; set; }
    }
}