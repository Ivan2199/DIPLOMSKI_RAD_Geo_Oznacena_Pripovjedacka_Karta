using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeoTagMap.WebApi.RestViewModels.View
{
    public class LikeView
    {
        public Guid Id { get; set; }
        public bool IsLike { get; set; }
        public Guid? CommentId { get; set; }
        public Guid? EventId { get; set; }
        public Guid? TouristSiteId { get; set; }
        public Guid? StoryId { get; set; }
        public Guid UserId { get; set; }
        public DateTime DateCreated { get; set; }
    }
}