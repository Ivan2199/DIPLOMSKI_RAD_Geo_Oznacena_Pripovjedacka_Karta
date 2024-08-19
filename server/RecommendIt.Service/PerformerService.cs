using GeoTagMap.Common;
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
    public class PerformerService : IPerformerService
    {
        private static readonly string AdminId = ConfigurationManager.AppSettings["AdminId"];
        private readonly IPerformerRepository _performerRepository;

        public PerformerService(IPerformerRepository performerRepository)
        {
            _performerRepository = performerRepository;
        }

        public async Task<PagingInfo<IPerformerModel>> GetAllPerformersAsync(Paging paging, Sorting sort, PerformerFiltering filtering)
        {
            return await _performerRepository.GetAllPerformersAsync(paging, sort, filtering);
        }

        public async Task<IPerformerModel> GetPerformerAsync(Guid id)
        {
            return await _performerRepository.GetPerformerAsync(id);
        }

        public async Task AddPerformerAsync(IPerformerModel performerModel)
        {
            Guid userId = GetUserId() ?? Guid.Parse(AdminId);
            performerModel.CreatedBy = userId;
            performerModel.UpdatedBy = userId;
            await _performerRepository.AddPerformerAsync(performerModel);
        }

        public async Task UpdatePerformerAsync(Guid id, IPerformerModel performerData)
        {
            Guid userId = GetUserId() ?? Guid.Parse(AdminId);
            performerData.UpdatedBy = userId;
            await _performerRepository.UpdatePerformerAsync(id, performerData);
        }

        public async Task<IPerformerModel> GetPerformerByJambaseIdentifierAsync(string jambaseIdentifier)
        {
            return await _performerRepository.GetPerformerByJambaseIdentifierAsync(jambaseIdentifier);
        }

        public async Task DeletePerformerAsync(Guid id)
        {
            await _performerRepository.DeletePerformerAsync(id);
        }

        public Guid? GetUserId()
        {
            var identity = ClaimsPrincipal.Current.Identity as ClaimsIdentity;
            var userIdClaim = identity?.FindFirst("userId")?.Value;
            return userIdClaim != null ? Guid.Parse(userIdClaim) : (Guid?)null;
        }
    }
}
