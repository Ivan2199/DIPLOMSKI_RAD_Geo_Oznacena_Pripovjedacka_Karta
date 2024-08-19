using GeoTagMap.Models;
using GeoTagMap.Models.Common;
using GeoTagMap.Repository.Common;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoTagMap.Repository
{
    public class TouristSiteCategoryRepository : ITouristSiteCategoryRepository
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["CONNECTION_STRING"].ToString();

        public async Task AddTouristSiteCategoryAsync(ITouristSiteCategoryModel touristSiteCategoryModel)
        {
            using (var con = new NpgsqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;

                    StringBuilder query = new StringBuilder();
                    query.Append("INSERT INTO \"TouristSiteCategory\" (\"Id\", \"CategoryId\", \"TouristSiteId\", \"DateCreated\", \"DateUpdated\", \"CreatedBy\", \"UpdatedBy\", \"IsActive\") ");
                    query.Append("VALUES (@id, @categoryId, @touristSiteId, @dateCreated, @dateUpdated, @createdBy, @updatedBy, @isActive)");

                    cmd.CommandText = query.ToString();

                    cmd.Parameters.AddWithValue("@id", touristSiteCategoryModel.Id);
                    cmd.Parameters.AddWithValue("@categoryId", touristSiteCategoryModel.CategoryId);
                    cmd.Parameters.AddWithValue("@touristSiteId", touristSiteCategoryModel.TouristSiteId);
                    cmd.Parameters.AddWithValue("@dateCreated", touristSiteCategoryModel.DateCreated);
                    cmd.Parameters.AddWithValue("@dateUpdated", touristSiteCategoryModel.DateUpdated);
                    cmd.Parameters.AddWithValue("@createdBy", touristSiteCategoryModel.CreatedBy);
                    cmd.Parameters.AddWithValue("@updatedBy", touristSiteCategoryModel.UpdatedBy);
                    cmd.Parameters.AddWithValue("@isActive", touristSiteCategoryModel.IsActive);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task<ITouristSiteCategoryModel> GetTouristSiteCategoryByCategoryAndTouristSiteIdAsync(Guid categoryId, Guid touristSiteId)
        {
            ITouristSiteCategoryModel siteCategory = null;
            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;

                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT \"Id\", \"CategoryId\", \"TouristSiteId\", \"IsActive\" ");
                    query.Append("FROM \"TouristSiteCategory\" ");
                    query.Append("WHERE \"CategoryId\" = @categoryId ");
                    query.Append("AND \"TouristSiteId\" = @touristSiteId ");

                    cmd.Parameters.AddWithValue("@categoryId", categoryId);
                    cmd.Parameters.AddWithValue("@touristSiteId", touristSiteId);

                    cmd.CommandText = query.ToString();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {

                            if (siteCategory == null)
                            {
                                siteCategory = MapSiteCategory(reader);
                            }
                        }
                    }
                }
            }
            return siteCategory;
        }

        private ITouristSiteCategoryModel MapSiteCategory(NpgsqlDataReader reader)
        {
            return new TouristSiteCategoryModel
            {
                Id = (Guid)reader["Id"],
                CategoryId = (Guid)reader["CategoryId"],
                TouristSiteId = (Guid)reader["TouristSiteId"]
            };
        }
    }
}
