using GeoTagMap.Common;
using GeoTagMap.Common.Filtering;
using GeoTagMap.Common.Paging;
using GeoTagMap.Common.Sorting;
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
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }
        public async Task<PagingInfo<ICommentModel>> GetAllCommentsAsync(Paging paging, Sorting sort, CommentFiltering filtering)
        {
            return await _commentRepository.GetAllCommentsAsync(paging, sort, filtering);
        }
        public async Task<List<ICommentModel>> GetAllParentCommentsAsync(Guid? eventId = null, Guid? touristSiteId = null, Guid? storyId = null)
        {
            return await _commentRepository.GetAllParentCommentsAsync(eventId, touristSiteId, storyId);
        }
        public async Task<List<ICommentModel>> GetAllChildCommentsAsync(Guid parentCommentId)
        {
            return await _commentRepository.GetAllChildCommentsAsync(parentCommentId);
        }
        public async Task<List<ICommentModel>> GetCommentsByEventIdAsync(Guid eventId)
        {
            return await _commentRepository.GetCommentsByEventIdAsync(eventId);
        }
        public async Task<List<ICommentModel>> GetCommentsByUserIdAsync()
        {
            Guid userId = GetUserId();
            return await _commentRepository.GetCommentsByUserIdAsync(userId);
        }
        public async Task<List<ICommentModel>> GetCommentsByTouristSiteIdAsync(Guid touristSiteId)
        {
            return await _commentRepository.GetCommentsByTouristSiteIdAsync(touristSiteId);
        }
        public async Task<List<ICommentModel>> GetCommentsByStoryIdAsync(Guid storyId)
        {
            return await _commentRepository.GetCommentsByStoryIdAsync(storyId);
        }
        public async Task<ICommentModel> GetCommentAsync(Guid id)
        {
            return await _commentRepository.GetCommentAsync(id);
        }
        public async Task AddCommentAsync(ICommentModel comment)
        {
            comment.UserId = GetUserId();
            comment.CreatedBy = GetUserId();
            comment.UpdatedBy = GetUserId();
            await _commentRepository.AddCommentAsync(comment);
        }
        public async Task UpdateCommentAsync(Guid id, ICommentModel comment)
        {
            comment.UpdatedBy = GetUserId();
            await _commentRepository.UpdateCommentAsync(id, comment);
        }
        public async Task DeleteCommentAsync(Guid id)
        {
            await _commentRepository.DeleteCommentAsync(id);
        }
        public Guid GetUserId()
        {
            var identity = ClaimsPrincipal.Current.Identity as ClaimsIdentity;
            return Guid.Parse(identity.FindFirst("userId")?.Value);
        }
    }
}
