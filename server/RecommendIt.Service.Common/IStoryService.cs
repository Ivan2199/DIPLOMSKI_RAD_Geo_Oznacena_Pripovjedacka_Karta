using GeoTagMap.Common.Filtering;
using GeoTagMap.Common.Paging;
using GeoTagMap.Common.Sorting;
using GeoTagMap.Common;
using GeoTagMap.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoTagMap.Service.Common
{
    public interface IStoryService
    {
        Task<PagingInfo<IStoryModel>> GetAllStoriesAsync(Paging paging, Sorting sort, StoryFiltering filtering);
        Task<List<IStoryModel>> GetAllStoriesByUserIdAsync();
        Task<IStoryModel> GetStoryAsync(Guid id);
        Task AddStoryAsync(IStoryModel story);
        Task UpdateStoryAsync(Guid id, IStoryModel story);
        Task DeleteStoryAsync(Guid id);
    }
}
