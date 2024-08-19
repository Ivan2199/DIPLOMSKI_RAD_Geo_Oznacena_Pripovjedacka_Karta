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
    public class PhotoService : IPhotoService
    {
        private static readonly string AdminId = ConfigurationManager.AppSettings["AdminId"];
        private readonly IPhotoRepository _photoRepository;

        public PhotoService(IPhotoRepository photoRepository)
        {
            _photoRepository = photoRepository;
        }

        public async Task<List<IPhotoModel>> GetPhotosAsync()
        {
            return await _photoRepository.GetPhotosAsync();
        }

        public async Task<List<IPhotoModel>> GetPhotosByTouristSiteIdAsync(Guid touristSiteId)
        {
            return await _photoRepository.GetPhotosByTouristSiteIdAsync(touristSiteId);
        }

        public async Task<List<IPhotoModel>> GetPhotosByStoryIdAsync(Guid storyId)
        {
            return await _photoRepository.GetPhotosByStoryIdAsync(storyId);
        }

        public async Task<IPhotoModel> GetPhotoAsync(Guid id)
        {
            return await _photoRepository.GetPhotoAsync(id);
        }

        public async Task AddPhotoAsync(IPhotoModel photoModel)
        {
            Guid userId = GetUserId() ?? Guid.Parse(AdminId);
            photoModel.CreatedBy = userId;
            photoModel.UpdatedBy = userId;
            await _photoRepository.AddPhotoAsync(photoModel);
        }

        public async Task UpdatePhotoAsync(Guid id, IPhotoModel photoData)
        {
            Guid userId = GetUserId() ?? Guid.Parse(AdminId);
            photoData.UpdatedBy = userId;
            await _photoRepository.UpdatePhotoAsync(id, photoData);
        }

        public async Task DeletePhotoAsync(Guid id)
        {
            await _photoRepository.DeletePhotoAsync(id);
        }

        public Guid? GetUserId()
        {
            var identity = ClaimsPrincipal.Current.Identity as ClaimsIdentity;
            var userIdClaim = identity?.FindFirst("userId")?.Value;
            return userIdClaim != null ? Guid.Parse(userIdClaim) : (Guid?)null;
        }
    }
}
