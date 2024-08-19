using GeoTagMap.Common;
using GeoTagMap.Common.Filtering;
using GeoTagMap.Common.Paging;
using GeoTagMap.Common.Sorting;
using GeoTagMap.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace GeoTagMap.Service.Common
{
    public interface ICommentService
    {
        Task<PagingInfo<ICommentModel>> GetAllCommentsAsync(Paging paging, Sorting sort, CommentFiltering filtering);
        Task<List<ICommentModel>> GetAllParentCommentsAsync(Guid? eventId = null, Guid? touristSiteId = null, Guid? storyId = null);
        Task<List<ICommentModel>> GetAllChildCommentsAsync(Guid parentCommentId);
        Task<List<ICommentModel>> GetCommentsByEventIdAsync(Guid eventId);
        Task<List<ICommentModel>> GetCommentsByUserIdAsync();
        Task<List<ICommentModel>> GetCommentsByTouristSiteIdAsync(Guid touristSiteId);
        Task<List<ICommentModel>> GetCommentsByStoryIdAsync(Guid storyId);
        Task<ICommentModel> GetCommentAsync(Guid id);
        Task AddCommentAsync(ICommentModel comment);
        Task UpdateCommentAsync(Guid id, ICommentModel comment);
        Task DeleteCommentAsync(Guid id);
    }
}
