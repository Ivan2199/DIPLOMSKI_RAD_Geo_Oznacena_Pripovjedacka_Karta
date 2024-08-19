using GeoTagMap.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoTagMap.Repository.Common
{
    public interface ILikeRepository
    {
        Task<List<ILikeModel>> GetAllLikesAsync();
        Task<ILikeModel> GetLikeAsync(Guid id);
        Task<ILikeModel> GetLikeByCommentAndUserIdAsync(Guid commentId, Guid userId);
        Task<ILikeModel> GetLikeByEventAndUserIdAsync(Guid eventId, Guid userId);
        Task<ILikeModel> GetLikeByTouristSiteAndUserIdAsync(Guid touristSiteId, Guid userId);
        Task<ILikeModel> GetLikeByStoryAndUserIdAsync(Guid storyId, Guid userId);
        Task<List<ILikeModel>> GetAllLikesByCommentIdAsync(Guid commentId);
        Task<List<ILikeModel>> GetAllLikesByUserIdAsync(Guid userId, string likesOf);
        Task<List<ILikeModel>> GetAllLikesByEventIdAsync(Guid eventId);
        Task<List<ILikeModel>> GetAllLikesByStoryIdAsync(Guid storyId);
        Task<List<ILikeModel>> GetAllLikesByTouristSiteIdAsync(Guid touristSiteId);
        Task AddLikeAsync(ILikeModel like);
        Task DeleteLikeAsync(Guid id);
    }
}
