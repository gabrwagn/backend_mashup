using System.Collections.Generic;

namespace MusicGenie.Models
{
    public class ArtistInfo
    {
        public string Name { get; set; }
        public string Mbid { get; set; }
        public string Description { get; set; }
        public List<AlbumInfo> Albums { get; set; }
    }

    public struct AlbumInfo
    {
        public string Title { get; set; }
        public string Id { get; set; }
        public string Image { get; set; }
    }
}