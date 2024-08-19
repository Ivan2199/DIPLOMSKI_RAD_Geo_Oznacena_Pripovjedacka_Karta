using GeoTagMap.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoTagMap.Service.Common
{
    public interface ILikeService
    {
        Task<List<ILikeModel>> GetAllLikesAsync();
        Task<ILikeModel> GetLikeAsync(Guid id);
        Task<ILikeModel> GetLikeByCommentAndUserIdAsync(Guid commentId);
        Task<ILikeModel> GetLikeByEventAndUserIdAsync(Guid eventId);
        Task<ILikeModel> GetLikeByTouristSiteAndUserIdAsync(Guid touristSiteId);
        Task<ILikeModel> GetLikeByStoryAndUserIdAsync(Guid storyId);
        Task<List<ILikeModel>> GetAllLikesByCommentIdAsync(Guid commentId);
        Task<List<ILikeModel>> GetAllLikesByUserIdAsync(string likesOf);
        Task<List<ILikeModel>> GetAllLikesByEventIdAsync(Guid eventId);
        Task<List<ILikeModel>> GetAllLikesByStoryIdAsync(Guid storyId);
        Task<List<ILikeModel>> GetAllLikesByTouristSiteIdAsync(Guid touristSiteId);
        Task AddLikeAsync(ILikeModel like);
        Task DeleteLikeAsync(Guid id);
    }
}
