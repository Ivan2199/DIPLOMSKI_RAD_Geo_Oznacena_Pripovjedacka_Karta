﻿using GeoTagMap.Common;
using GeoTagMap.Common.Filtering;
using GeoTagMap.Common.Paging;
using GeoTagMap.Common.Sorting;
using GeoTagMap.Models;
using GeoTagMap.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoTagMap.Repository.Common
{
    public interface IEventRepository
    {
        Task<PagingInfo<IEventModel>> GetAllEventsAsync(Paging paging, Sorting sort, EventFiltering filtering);
        Task<List<IEventModel>> GetMostLikedEventsAsync();
        Task<IEventModel> GetEventAsync(Guid id);
        Task AddEventAsync(IEventModel eventModel);
        Task UpdateEventAsync(Guid id, IEventModel eventData);
        Task DeleteEventAsync(Guid id);
        Task<IEventModel> GetEventByJambaseIdentifierAsync(string jambaseIdentifier);
    }
}
