﻿using GeoTagMap.Common;
using GeoTagMap.Common.Filtering;
using GeoTagMap.Common.Paging;
using GeoTagMap.Common.Sorting;
using GeoTagMap.Models;
using GeoTagMap.Models.Common;
using GeoTagMap.Repository.Common;
using GeoTagMap.Service.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GeoTagMap.Service
{
    public class EventService : IEventService
    {
        private static readonly string AdminId = ConfigurationManager.AppSettings["AdminId"];
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<PagingInfo<IEventModel>> GetAllEventsAsync(Paging paging, Sorting sort, EventFiltering filtering)
        {
            return await _eventRepository.GetAllEventsAsync(paging, sort, filtering);
        }

        public async Task<List<IEventModel>> GetMostLikedEventsAsync()
        {
            return await _eventRepository.GetMostLikedEventsAsync();
        }

        public async Task<IEventModel> GetEventAsync(Guid id)
        {
            return await _eventRepository.GetEventAsync(id);
        }

        public async Task AddEventAsync(IEventModel eventModel)
        {
            Guid userId = GetUserId() ?? Guid.Parse(AdminId);
            eventModel.CreatedBy = userId;
            eventModel.UpdatedBy = userId;
            await _eventRepository.AddEventAsync(eventModel);
        }

        public async Task UpdateEventAsync(Guid id, IEventModel eventData)
        {
            Guid userId = GetUserId() ?? Guid.Parse(AdminId);
            eventData.UpdatedBy = userId;
            await _eventRepository.UpdateEventAsync(id, eventData);
        }

        public async Task DeleteEventAsync(Guid id)
        {
            await _eventRepository.DeleteEventAsync(id);
        }

        public async Task<IEventModel> GetEventByJambaseIdentifierAsync(string jambaseIdentifier)
        {
            return await _eventRepository.GetEventByJambaseIdentifierAsync(jambaseIdentifier);
        }

        public Guid? GetUserId()
        {
            var identity = ClaimsPrincipal.Current.Identity as ClaimsIdentity;
            var userIdClaim = identity?.FindFirst("userId")?.Value;
            return userIdClaim != null ? Guid.Parse(userIdClaim) : (Guid?)null;
        }
    }
}
