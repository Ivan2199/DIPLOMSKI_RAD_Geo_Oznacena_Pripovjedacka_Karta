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
    public class GeoLocationService : IGeoLocationService
    {
        private static readonly string AdminId = ConfigurationManager.AppSettings["AdminId"];
        private readonly IGeoLoctionRepositrory _geoLocationRepository;

        public GeoLocationService(IGeoLoctionRepositrory geoLocationRepository)
        {
            _geoLocationRepository = geoLocationRepository;
        }

        public async Task<List<IGeoLocationModel>> GetAllGeoLocationsAsync()
        {
            return await _geoLocationRepository.GetAllGeoLocationsAsync();
        }

        public async Task<IGeoLocationModel> GetGeoLocationAsync(Guid id)
        {
            return await _geoLocationRepository.GetGeoLocationAsync(id);
        }

        public async Task<IGeoLocationModel> GetGeoLocationByLocationIdAsync(Guid locationId)
        {
            return await _geoLocationRepository.GetGeoLocationByLocationIdAsync(locationId);
        }

        public async Task AddGeoLocationAsync(IGeoLocationModel geoLocationModel)
        {
            Guid userId = GetUserId() ?? Guid.Parse(AdminId);
            geoLocationModel.CreatedBy = userId;
            geoLocationModel.UpdatedBy = userId;
            await _geoLocationRepository.AddGeoLocationAsync(geoLocationModel);
        }

        public async Task UpdateGeoLocationAsync(Guid id, IGeoLocationModel geoLocationData)
        {
            Guid userId = GetUserId() ?? Guid.Parse(AdminId);
            geoLocationData.UpdatedBy = userId;
            await _geoLocationRepository.UpdateGeoLocationAsync(id, geoLocationData);
        }

        public Guid? GetUserId()
        {
            var identity = ClaimsPrincipal.Current.Identity as ClaimsIdentity;
            var userIdClaim = identity?.FindFirst("userId")?.Value;
            return userIdClaim != null ? Guid.Parse(userIdClaim) : (Guid?)null;
        }
    }
}
