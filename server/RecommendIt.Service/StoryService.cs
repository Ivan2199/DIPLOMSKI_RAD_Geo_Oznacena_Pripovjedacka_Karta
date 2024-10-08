﻿using GeoTagMap.Common.Filtering;
using GeoTagMap.Common.Paging;
using GeoTagMap.Common.Sorting;
using GeoTagMap.Common;
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
    public class StoryService : IStoryService
    {
        private readonly IStoryRepository _storyRepository;

        public StoryService(IStoryRepository storyRepository)
        {
            _storyRepository = storyRepository;
        }
        public async Task<List<IStoryModel>> GetAllStoriesByUserIdAsync()
        {
            var userId = GetUserId();
            return await _storyRepository.GetAllStoriesByUserIdAsync(userId);
        }
        public async Task<PagingInfo<IStoryModel>> GetAllStoriesAsync(Paging paging, Sorting sort, StoryFiltering filtering)
        {
            return await _storyRepository.GetAllStoriesAsync(paging, sort, filtering);
        }
        public async Task<IStoryModel> GetStoryAsync(Guid id)
        {
            return await _storyRepository.GetStoryAsync(id);
        }
        public async Task AddStoryAsync(IStoryModel story)
        {
            story.UserId = GetUserId();
            story.CreatedBy = GetUserId();
            story.UpdatedBy = GetUserId();
            await _storyRepository.AddStoryAsync(story);
        }
        public async Task UpdateStoryAsync(Guid id, IStoryModel story)
        {
            story.UpdatedBy = GetUserId();
            await _storyRepository.UpdateStoryAsync(id, story);
        }
        public async Task DeleteStoryAsync(Guid id)
        {
            await _storyRepository.DeleteStoryAsync(id);
        }
        public Guid GetUserId()
        {
            var identity = ClaimsPrincipal.Current.Identity as ClaimsIdentity;
            return Guid.Parse(identity.FindFirst("userId")?.Value);
        }
    }
}
