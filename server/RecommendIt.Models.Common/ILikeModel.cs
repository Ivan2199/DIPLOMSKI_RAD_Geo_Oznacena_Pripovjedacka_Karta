using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoTagMap.Models.Common
{
    public interface ILikeModel
    {
        Guid Id { get; set; }
        bool IsLike { get; set; }
        Guid? CommentId { get; set; }
        Guid? EventId { get; set; }
        Guid? TouristSiteId { get; set; }
        Guid? StoryId { get; set; }
        Guid UserId { get; set; }

        DateTime DateCreated { get; set; }
        DateTime DateUpdated { get; set; }
        Guid CreatedBy { get; set; }
        Guid UpdatedBy { get; set; }
        bool? IsActive { get; set; }
    }
}
