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
    public class TouristSiteCategoryService : ITouristSiteCategoryService
    {
        private static readonly string AdminId = ConfigurationManager.AppSettings["AdminId"];
        private readonly ITouristSiteCategoryRepository _touristSiteCategoryRepository;

        public TouristSiteCategoryService(ITouristSiteCategoryRepository touristSiteCategoryRepository)
        {
            _touristSiteCategoryRepository = touristSiteCategoryRepository;
        }

        public async Task AddTouristSiteCategoryAsync(ITouristSiteCategoryModel touristSiteCategoryModel)
        {
            Guid userId = GetUserId() ?? Guid.Parse(AdminId);
            touristSiteCategoryModel.CreatedBy = userId;
            touristSiteCategoryModel.UpdatedBy = userId;
            await _touristSiteCategoryRepository.AddTouristSiteCategoryAsync(touristSiteCategoryModel);
        }

        public async Task<ITouristSiteCategoryModel> GetTouristSiteCategoryByCategoryAndTouristSiteIdAsync(Guid categoryId, Guid touristSiteId)
        {
            return await _touristSiteCategoryRepository.GetTouristSiteCategoryByCategoryAndTouristSiteIdAsync(categoryId, touristSiteId);
        }

        public Guid? GetUserId()
        {
            var identity = ClaimsPrincipal.Current.Identity as ClaimsIdentity;
            var userIdClaim = identity?.FindFirst("userId")?.Value;
            return userIdClaim != null ? Guid.Parse(userIdClaim) : (Guid?)null;
        }
    }
}
