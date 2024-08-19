using GeoTagMap.Common;
using GeoTagMap.Common.Filtering;
using GeoTagMap.Common.Paging;
using GeoTagMap.Common.Sorting;
using GeoTagMap.Models;
using GeoTagMap.Models.Common;
using GeoTagMap.Repository.Common;
using GeoTagMap.Service.Common;
using System;
using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GeoTagMap.Service
{
    public class LocationService : ILocationService
    {
        private static readonly string AdminId = ConfigurationManager.AppSettings["AdminId"];
        private readonly ILocationRepository _locationRepository;

        public LocationService(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }

        public async Task<PagingInfo<ILocationModel>> GetAllLocationsAsync(Paging paging, Sorting sort, LocationFiltering filtering)
        {
            return await _locationRepository.GetAllLocationsAsync(paging, sort, filtering);
        }

        public async Task<ILocationModel> GetLocationAsync(Guid id)
        {
            return await _locationRepository.GetLocationAsync(id);
        }

        public async Task AddLocationAsync(ILocationModel location)
        {
            Guid userId = GetUserId() ?? Guid.Parse(AdminId);
            location.CreatedBy = userId;
            location.UpdatedBy = userId;
            await _locationRepository.AddLocationAsync(location);
        }

        public async Task UpdateLocationAsync(Guid id, ILocationModel locationData)
        {
            Guid userId = GetUserId() ?? Guid.Parse(AdminId);
            locationData.UpdatedBy = userId;
            await _locationRepository.UpdateLocationAsync(id, locationData);
        }

        public async Task<ILocationModel> GetLocationByJambaseIdentifierAsync(string jambaseIdentifier)
        {
            return await _locationRepository.GetLocationByJambaseIdentifierAsync(jambaseIdentifier);
        }

        public async Task<ILocationModel> GetLocationByAddressAsync(string address)
        {
            return await _locationRepository.GetLocationByAddressAsync(address);
        }

        public Guid? GetUserId()
        {
            var identity = ClaimsPrincipal.Current.Identity as ClaimsIdentity;
            var userIdClaim = identity?.FindFirst("userId")?.Value;
            return userIdClaim != null ? Guid.Parse(userIdClaim) : (Guid?)null;
        }
    }
}
