using GeoTagMap.Models.Common;
using GeoTagMap.Repository.Common;
using GeoTagMap.Service.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GeoTagMap.Service
{
    public class LikeService : ILikeService
    {
        private readonly ILikeRepository _likeRepository; 

        public LikeService(ILikeRepository likeRepository)
        {
            _likeRepository = likeRepository;
        }

        public async Task<List<ILikeModel>> GetAllLikesAsync()
        {
            return await _likeRepository.GetAllLikesAsync();
        }
        public async Task<ILikeModel> GetLikeAsync(Guid id)
        {
            return await _likeRepository.GetLikeAsync(id);
        }
        public async Task<ILikeModel> GetLikeByCommentAndUserIdAsync(Guid commentId)
        {
            Guid userId = GetUserId();
            return await _likeRepository.GetLikeByCommentAndUserIdAsync(commentId, userId);
        }
        public async Task<ILikeModel> GetLikeByEventAndUserIdAsync(Guid eventId)
        {

            Guid userId = GetUserId();
            return await _likeRepository.GetLikeByEventAndUserIdAsync(eventId, userId);
        }
        public async Task<ILikeModel> GetLikeByTouristSiteAndUserIdAsync(Guid touristSiteId)
        {

            Guid userId = GetUserId();
            return await _likeRepository.GetLikeByTouristSiteAndUserIdAsync(touristSiteId, userId);
        }
        public async Task<ILikeModel> GetLikeByStoryAndUserIdAsync(Guid storyId)
        {
            Guid userId = GetUserId();
            return await _likeRepository.GetLikeByStoryAndUserIdAsync(storyId, userId);
        }
        public async Task<List<ILikeModel>> GetAllLikesByCommentIdAsync(Guid commentId)
        {
            return await _likeRepository.GetAllLikesByCommentIdAsync(commentId);
        }
        public async Task<List<ILikeModel>> GetAllLikesByUserIdAsync(string likesOf)
        {
            Guid userId = GetUserId();
            return await _likeRepository.GetAllLikesByUserIdAsync(userId, likesOf);
        }
        public async Task<List<ILikeModel>> GetAllLikesByEventIdAsync(Guid eventId)
        {
            return await _likeRepository.GetAllLikesByEventIdAsync(eventId);
        }
        public async Task<List<ILikeModel>> GetAllLikesByStoryIdAsync(Guid storyId)
        {
            return await _likeRepository.GetAllLikesByStoryIdAsync(storyId);
        }
        public async Task<List<ILikeModel>> GetAllLikesByTouristSiteIdAsync(Guid touristSiteId)
        {
            return await _likeRepository.GetAllLikesByTouristSiteIdAsync(touristSiteId);
        }
        public async Task AddLikeAsync(ILikeModel like)
        {
            like.UserId = GetUserId();
            like.CreatedBy = GetUserId();
            like.UpdatedBy = GetUserId();
            await _likeRepository.AddLikeAsync(like);
        }
        public async Task DeleteLikeAsync(Guid id)
        {
            await _likeRepository.DeleteLikeAsync(id);
        }
        public Guid GetUserId()
        {
            var identity = ClaimsPrincipal.Current.Identity as ClaimsIdentity;
            return Guid.Parse(identity.FindFirst("userId")?.Value);
        }
    }
}
