using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeoTagMap.WebApi.RestViewModels.Rest
{
    public class LikeRest
    {
        public bool IsLike { get; set; }
        public Guid? CommentId { get; set; }
        public Guid? EventId { get; set; }
        public Guid? TouristSiteId { get; set; }
        public Guid? StoryId { get; set; }
    }
}