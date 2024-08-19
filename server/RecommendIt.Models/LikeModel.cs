using GeoTagMap.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoTagMap.Models
{
    public class LikeModel : ILikeModel
    {
        public Guid Id { get; set; }
        public bool IsLike { get; set; }  
        public Guid? CommentId { get; set; }
        public Guid? EventId { get; set; }
        public Guid? TouristSiteId { get; set; }
        public Guid? StoryId { get; set; }
        public Guid UserId { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid UpdatedBy { get; set; }
        public bool? IsActive { get; set; }

    }
}
