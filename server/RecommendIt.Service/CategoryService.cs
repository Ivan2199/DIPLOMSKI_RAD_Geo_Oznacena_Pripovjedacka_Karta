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
    public class CategoryService : ICategoryService
    {
        private static readonly string AdminId = ConfigurationManager.AppSettings["AdminId"];
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<List<ICategoryModel>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllCategoriesAsync();
        }

        public async Task<ICategoryModel> GetCategoryAsync(Guid id)
        {
            return await _categoryRepository.GetCategoryAsync(id);
        }

        public async Task AddCategoryAsync(ICategoryModel categoryModel)
        {
            Guid userId = GetUserId() ?? Guid.Parse(AdminId);
            categoryModel.CreatedBy = userId;
            categoryModel.UpdatedBy = userId;
            await _categoryRepository.AddCategoryAsync(categoryModel);
        }

        public async Task UpdateCategoryAsync(Guid id, ICategoryModel categoryData)
        {
            Guid userId = GetUserId() ?? Guid.Parse(AdminId);
            categoryData.UpdatedBy = userId;
            await _categoryRepository.UpdateCategoryAsync(id, categoryData);
        }

        public async Task<ICategoryModel> GetCategoryByFsqIdAsync(string id)
        {
            return await _categoryRepository.GetCategoryByFsqIdAsync(id);
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            await _categoryRepository.DeleteCategoryAsync(id);
        }

        public Guid? GetUserId()
        {
            var identity = ClaimsPrincipal.Current.Identity as ClaimsIdentity;
            var userIdClaim = identity?.FindFirst("userId")?.Value;
            return userIdClaim != null ? Guid.Parse(userIdClaim) : (Guid?)null;
        }
    }
}
