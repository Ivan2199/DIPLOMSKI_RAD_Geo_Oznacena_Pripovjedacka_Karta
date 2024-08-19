using GeoTagMap.Common;
using GeoTagMap.Common.Filtering;
using GeoTagMap.Common.Paging;
using GeoTagMap.Common.Sorting;
using GeoTagMap.Models;
using GeoTagMap.Models.Common;
using GeoTagMap.Repository.Common;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace GeoTagMap.Repository
{
    public class TouristSiteRepository : ITouristSiteRepository
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["CONNECTION_STRING"].ToString();

        public async Task<PagingInfo<ITouristSitesModel>> GetAllSitesAsync(Paging paging, Sorting sort, TouristSiteFiltering filtering)
        {
            Dictionary<Guid, ITouristSitesModel> sites = new Dictionary<Guid, ITouristSitesModel>();
            int totalTouristSites = 0;

            using (var con = new NpgsqlConnection(_connectionString))
            {
                await con.OpenAsync();

                List<Guid> siteIds = new List<Guid>();
                using (var siteIdCmd = con.CreateCommand())
                {
                    StringBuilder siteIdQuery = new StringBuilder();
                    siteIdQuery.Append("SELECT t.\"Id\" FROM \"TouristSites\" t ");
                    siteIdQuery.Append("LEFT JOIN \"Location\" l ON t.\"LocationId\" = l.\"Id\" ");
                    siteIdQuery.Append("WHERE t.\"IsActive\" = true");

                    if (!string.IsNullOrEmpty(filtering.Name))
                    {
                        siteIdQuery.Append(" AND LOWER(t.\"SiteName\") = LOWER(@Name)");
                        siteIdCmd.Parameters.AddWithValue("@Name", filtering.Name.ToLower());
                    }
                    if (filtering.Popularity != null)
                    {
                        siteIdQuery.Append(" AND t.\"Popularity\" >= @Popularity");
                        siteIdCmd.Parameters.AddWithValue("@Popularity", filtering.Popularity);
                    }
                    if (filtering.Rating != null)
                    {
                        siteIdQuery.Append(" AND t.\"Rating\" >= @Rating");
                        siteIdCmd.Parameters.AddWithValue("@Rating", filtering.Rating);
                    }
                    if (!string.IsNullOrEmpty(filtering.SearchKeyword))
                    {
                        siteIdQuery.Append(" AND LOWER(t.\"SiteName\") LIKE LOWER(@SearchKeyword)");
                        siteIdCmd.Parameters.AddWithValue("@SearchKeyword", "%" + filtering.SearchKeyword.ToLower() + "%");
                    }
                    if (!string.IsNullOrEmpty(filtering.Country))
                    {
                        siteIdQuery.Append(" AND LOWER(l.\"Country\") = LOWER(@Country)");
                        siteIdCmd.Parameters.AddWithValue("@Country", filtering.Country.ToLower());
                    }
                    if (!string.IsNullOrEmpty(filtering.City))
                    {
                        siteIdQuery.Append(" AND LOWER(l.\"City\") = LOWER(@City)");
                        siteIdCmd.Parameters.AddWithValue("@City", filtering.City.ToLower());
                    }

                    if (!string.IsNullOrEmpty(sort.OrderBy))
                    {
                        siteIdQuery.Append($" ORDER BY t.\"{sort.OrderBy}\" {(sort.SortOrder.ToUpper() == "DESC" ? "DESC" : "ASC")}, t.\"Id\"");
                    }
                    else
                    {
                        siteIdQuery.Append(" ORDER BY t.\"Id\"");
                    }

                    siteIdQuery.Append(" LIMIT @Limit OFFSET @Offset");
                    siteIdCmd.Parameters.AddWithValue("@Limit", paging.RRP);
                    siteIdCmd.Parameters.AddWithValue("@Offset", (paging.PageNumber - 1) * paging.RRP);

                    siteIdCmd.CommandText = siteIdQuery.ToString();

                    using (var reader = await siteIdCmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            siteIds.Add(reader.GetGuid(reader.GetOrdinal("Id")));
                        }
                    }
                }

                if (siteIds.Count == 0)
                {
                    return new PagingInfo<ITouristSitesModel>
                    {
                        List = new List<ITouristSitesModel>(),
                        RRP = paging.RRP,
                        PageNumber = paging.PageNumber,
                        TotalSize = 0
                    };
                }

                using (var countCmd = con.CreateCommand())
                {
                    StringBuilder countQuery = new StringBuilder();
                    countQuery.Append("SELECT COUNT(*) FROM \"TouristSites\" t ");
                    countQuery.Append("LEFT JOIN \"Location\" l ON t.\"LocationId\" = l.\"Id\" ");
                    countQuery.Append("WHERE t.\"IsActive\" = true");

                    if (!string.IsNullOrEmpty(filtering.Name))
                    {
                        countQuery.Append(" AND LOWER(t.\"SiteName\") = LOWER(@Name)");
                        countCmd.Parameters.AddWithValue("@Name", filtering.Name.ToLower());
                    }
                    if (filtering.Popularity != null)
                    {
                        countQuery.Append(" AND t.\"Popularity\" >= @Popularity");
                        countCmd.Parameters.AddWithValue("@Popularity", filtering.Popularity);
                    }
                    if (filtering.Rating != null)
                    {
                        countQuery.Append(" AND t.\"Rating\" >= @Rating");
                        countCmd.Parameters.AddWithValue("@Rating", filtering.Rating);
                    }
                    if (!string.IsNullOrEmpty(filtering.SearchKeyword))
                    {
                        countQuery.Append(" AND LOWER(t.\"SiteName\") LIKE LOWER(@SearchKeyword)");
                        countCmd.Parameters.AddWithValue("@SearchKeyword", "%" + filtering.SearchKeyword.ToLower() + "%");
                    }
                    if (!string.IsNullOrEmpty(filtering.Country))
                    {
                        countQuery.Append(" AND LOWER(l.\"Country\") = LOWER(@Country)");
                        countCmd.Parameters.AddWithValue("@Country", filtering.Country.ToLower());
                    }
                    if (!string.IsNullOrEmpty(filtering.City))
                    {
                        countQuery.Append(" AND LOWER(l.\"City\") = LOWER(@City)");
                        countCmd.Parameters.AddWithValue("@City", filtering.City.ToLower());
                    }

                    countCmd.CommandText = countQuery.ToString();
                    totalTouristSites = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
                }

                using (var cmd = con.CreateCommand())
                {
                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT t.\"Id\", t.\"SiteName\", t.\"Link\", t.\"LocationId\" AS TouristSiteLocationId, t.\"Fsq_Id\", t.\"Popularity\", t.\"Rating\", ");
                    query.Append("t.\"Description\", t.\"WebsiteUrl\", t.\"Email\", t.\"HoursOpen\", t.\"FacebookId\", t.\"Instagram\", t.\"Twitter\", t.\"IsActive\", ");
                    query.Append("t.\"NumberOfLikes\" AS TouristSiteNumberOfLikes, ");
                    query.Append("l.\"Id\" AS SiteLocationId, l.\"Country\", l.\"City\", l.\"Village\", l.\"Address\", l.\"NameOfPlace\", l.\"IsActive\" AS LocationIsActive, ");
                    query.Append("l.\"JambaseIdentifier\", ");
                    query.Append("g.\"Id\" AS GeoLocationId, g.\"Latitude\", g.\"Longitude\", g.\"LocationId\" AS GeoLocationLocationId, g.\"IsActive\" AS GeoLocationIsActive, ");
                    query.Append("c.\"Id\" AS CommentId, c.\"CommentText\", c.\"IsActive\" AS CommentIsActive, c.\"UserId\" AS CommentUserId, c.\"EventId\" AS CommentEventId, ");
                    query.Append("c.\"TouristSitesId\" AS CommentTouristSitesId, c.\"StoryId\" AS CommentStoryId, c.\"DateCreated\" AS CommentDateCreated, ");
                    query.Append("c.\"DateUpdated\" AS CommentDateUpdated, c.\"CreatedBy\" AS CommentCreatedBy, c.\"ParentComment\", c.\"NumberOfLikes\", c.\"IsReported\", ");
                    query.Append("p.\"Id\" AS PhotoId, p.\"ImageData\", p.\"ImagePrefix\", p.\"ImageSuffix\", p.\"TouristSiteId\" AS PhotoTouristSiteId, ");
                    query.Append("p.\"StoryId\" AS PhotoStoryId, p.\"UserId\" AS PhotoUserId, p.\"IsActive\" AS PhotoIsActive, p.\"DateCreated\" AS PhotoDateCreated, ");
                    query.Append("p.\"DateUpdated\" AS PhotoDateUpdated, p.\"CreatedBy\" AS PhotoCreatedBy, ");
                    query.Append("tc.\"Id\" AS TouristSiteCategoryId, tc.\"TouristSiteId\", tc.\"CategoryId\", tc.\"IsActive\" AS TouristSiteCategoryIsActive ");
                    query.Append("FROM \"TouristSites\" t ");
                    query.Append("LEFT JOIN \"Location\" l ON t.\"LocationId\" = l.\"Id\" ");
                    query.Append("LEFT JOIN \"GeoLocation\" g ON l.\"Id\" = g.\"LocationId\" ");
                    query.Append("LEFT JOIN \"Comment\" c ON t.\"Id\" = c.\"TouristSitesId\" ");
                    query.Append("LEFT JOIN \"Photo\" p ON t.\"Id\" = p.\"TouristSiteId\" ");
                    query.Append("LEFT JOIN \"TouristSiteCategory\" tc ON t.\"Id\" = tc.\"TouristSiteId\" ");
                    query.Append("WHERE t.\"Id\" = ANY(@SiteIds::uuid[])");

                    cmd.Parameters.Add("@SiteIds", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Uuid).Value = siteIds.ToArray();

                    cmd.CommandText = query.ToString();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ITouristSitesModel currentSite;
                            Guid currentSiteId = reader.GetGuid(reader.GetOrdinal("Id"));

                            if (!sites.TryGetValue(currentSiteId, out currentSite))
                            {
                                currentSite = MapTouristSite(reader);
                                sites.Add(currentSiteId, currentSite);
                            }
                            if (reader["SiteLocationId"] != DBNull.Value)
                            {
                                if (reader["LocationIsActive"] != DBNull.Value && (bool)reader["LocationIsActive"])
                                {
                                    var location = MapLocation(reader);
                                    currentSite.Location = location;
                                }
                            }
                            if (reader["CommentId"] != DBNull.Value)
                            {
                                if (reader["CommentIsActive"] != DBNull.Value && (bool)reader["CommentIsActive"])
                                {
                                    var comment = MapComment(reader);
                                    currentSite.Comments.Add(comment);
                                }
                            }
                            if (reader["PhotoId"] != DBNull.Value)
                            {
                                if (reader["PhotoIsActive"] != DBNull.Value && (bool)reader["PhotoIsActive"])
                                {
                                    var photo = MapPhoto(reader);
                                    currentSite.Photos.Add(photo);
                                }
                            }
                            if (reader["TouristSiteCategoryId"] != DBNull.Value)
                            {
                                if (reader["TouristSiteCategoryIsActive"] != DBNull.Value && (bool)reader["TouristSiteCategoryIsActive"])
                                {
                                    ITouristSiteCategoryModel touristSiteCategory = MapTouristSiteCategory(reader);

                                    if (!currentSite.SiteCategories.Any(c => c.Id == touristSiteCategory.Id))
                                    {
                                        currentSite.SiteCategories.Add(touristSiteCategory);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            PagingInfo<ITouristSitesModel> pagingInfo = new PagingInfo<ITouristSitesModel>
            {
                List = sites.Values.ToList(),
                RRP = paging.RRP,
                PageNumber = paging.PageNumber,
                TotalSize = totalTouristSites
            };
            return pagingInfo;
        }

        public async Task<List<ITouristSitesModel>> GetMostLikedTouristSitesAsync()
        {
            Dictionary<Guid, ITouristSitesModel> sites = new Dictionary<Guid, ITouristSitesModel>();
            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;

                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT t.\"Id\", t.\"SiteName\", t.\"Link\", t.\"LocationId\" AS TouristSiteLocationId, t.\"Fsq_Id\", t.\"Popularity\", t.\"Rating\", t.\"Description\", t.\"WebsiteUrl\", t.\"Email\", t.\"HoursOpen\", t.\"FacebookId\", t.\"Instagram\", t.\"Twitter\", t.\"IsActive\", t.\"NumberOfLikes\" AS TouristSiteNumberOfLikes, ");
                    query.Append("c.\"Id\" AS CommentId, c.\"CommentText\", c.\"IsActive\" AS CommentIsActive, c.\"UserId\" AS CommentUserId, c.\"EventId\" AS CommentEventId, c.\"TouristSitesId\" AS CommentTouristSitesId, c.\"StoryId\" AS CommentStoryId, c.\"DateCreated\" AS CommentDateCreated, c.\"DateUpdated\" AS CommentDateUpdated, c.\"CreatedBy\" AS CommentCreatedBy, c.\"ParentComment\", c.\"NumberOfLikes\", c.\"IsReported\", ");
                    query.Append("g.\"Id\" AS GeoLocationId, g.\"Latitude\", g.\"Longitude\", g.\"LocationId\" AS GeoLocationLocationId, g.\"IsActive\" AS GeoLocationIsActive, ");
                    query.Append("l.\"Id\" AS SiteLocationId, l.\"Country\", l.\"City\", l.\"Village\", l.\"Address\", l.\"NameOfPlace\", l.\"IsActive\" AS LocationIsActive, l.\"JambaseIdentifier\", ");
                    query.Append("p.\"Id\" AS PhotoId, p.\"ImageData\", p.\"ImagePrefix\", p.\"ImageSuffix\", p.\"TouristSiteId\" AS PhotoTouristSiteId, p.\"StoryId\" AS PhotoStoryId, p.\"UserId\" AS PhotoUserId, p.\"IsActive\" AS PhotoIsActive, p.\"DateCreated\" AS PhotoDateCreated, p.\"DateUpdated\" AS PhotoDateUpdated, p.\"CreatedBy\" AS PhotoCreatedBy, ");
                    query.Append("tc.\"Id\" AS TouristSiteCategoryId, tc.\"TouristSiteId\", tc.\"CategoryId\", tc.\"IsActive\" AS TouristSiteCategoryIsActive ");
                    query.Append("FROM \"TouristSites\" t ");
                    query.Append("LEFT JOIN \"Location\" l ON t.\"LocationId\" = l.\"Id\" ");
                    query.Append("LEFT JOIN \"GeoLocation\" g ON l.\"Id\" = g.\"LocationId\" ");
                    query.Append("LEFT JOIN \"Comment\" c ON t.\"Id\" = c.\"TouristSitesId\" ");
                    query.Append("LEFT JOIN \"Photo\" p ON t.\"Id\" = p.\"TouristSiteId\" ");
                    query.Append("LEFT JOIN \"TouristSiteCategory\" tc ON t.\"Id\" = tc.\"TouristSiteId\" ");
                    query.Append("WHERE t.\"NumberOfLikes\" IS NOT NULL ");
                    query.Append("AND t.\"IsActive\" = true ");
                    query.Append("ORDER BY t.\"NumberOfLikes\" DESC ");
                    query.Append("LIMIT 1000");


                    cmd.CommandText = query.ToString();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ITouristSitesModel currentSite;
                            Guid currentSiteId = reader.GetGuid(reader.GetOrdinal("Id"));

                            if (!sites.TryGetValue(currentSiteId, out currentSite))
                            {
                                currentSite = MapTouristSite(reader);
                                sites.Add(currentSiteId, currentSite);
                            }
                            if (reader["SiteLocationId"] != DBNull.Value)
                            {
                                if (reader["LocationIsActive"] != DBNull.Value && (bool)reader["LocationIsActive"])
                                {
                                    var location = MapLocation(reader);
                                    currentSite.Location = location;

                                }
                            }
                            if (reader["CommentId"] != DBNull.Value)
                            {
                                if (reader["CommentIsActive"] != DBNull.Value && (bool)reader["CommentIsActive"])
                                {
                                    var comment = MapComment(reader);
                                    currentSite.Comments.Add(comment);
                                }
                            }
                            if (reader["PhotoId"] != DBNull.Value)
                            {
                                if (reader["PhotoIsActive"] != DBNull.Value && (bool)reader["PhotoIsActive"])
                                {
                                    var photo = MapPhoto(reader);
                                    currentSite.Photos.Add(photo);
                                }
                            }
                            if (reader["TouristSiteCategoryId"] != DBNull.Value)
                            {
                                if (reader["TouristSiteCategoryIsActive"] != DBNull.Value && (bool)reader["TouristSiteCategoryIsActive"])
                                {
                                    ITouristSiteCategoryModel touristSiteCategory = MapTouristSiteCategory(reader);

                                    if (!currentSite.SiteCategories.Any(c => c.Id == touristSiteCategory.Id))
                                    {
                                        currentSite.SiteCategories.Add(touristSiteCategory);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return sites.Values.ToList();
        }

        public async Task<ITouristSitesModel> GetTouristSiteAsync(Guid id)
        {
            ITouristSitesModel site = null;
            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;

                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT t.\"Id\", t.\"SiteName\", t.\"Link\", t.\"LocationId\" AS TouristSiteLocationId, t.\"Fsq_Id\", t.\"Popularity\", t.\"Rating\", t.\"Description\", t.\"WebsiteUrl\", t.\"Email\", t.\"HoursOpen\", t.\"FacebookId\", t.\"Instagram\", t.\"Twitter\", t.\"IsActive\", t.\"NumberOfLikes\" AS TouristSiteNumberOfLikes, ");
                    query.Append("c.\"Id\" AS CommentId, c.\"CommentText\", c.\"IsActive\" AS CommentIsActive, c.\"UserId\" AS CommentUserId, c.\"EventId\" AS CommentEventId, c.\"TouristSitesId\" AS CommentTouristSitesId, c.\"StoryId\" AS CommentStoryId, c.\"DateCreated\" AS CommentDateCreated, c.\"DateUpdated\" AS CommentDateUpdated, c.\"CreatedBy\" AS CommentCreatedBy, c.\"ParentComment\", c.\"NumberOfLikes\", c.\"IsReported\", ");
                    query.Append("g.\"Id\" AS GeoLocationId, g.\"Latitude\", g.\"Longitude\", g.\"LocationId\" AS GeoLocationLocationId, g.\"IsActive\" AS GeoLocationIsActive, ");
                    query.Append("l.\"Id\" AS SiteLocationId, l.\"Country\", l.\"City\", l.\"Village\", l.\"Address\", l.\"NameOfPlace\", l.\"IsActive\" AS LocationIsActive, l.\"JambaseIdentifier\", ");
                    query.Append("p.\"Id\" AS PhotoId, p.\"ImageData\", p.\"ImagePrefix\", p.\"ImageSuffix\", p.\"TouristSiteId\" AS PhotoTouristSiteId, p.\"StoryId\" AS PhotoStoryId, p.\"UserId\" AS PhotoUserId, p.\"IsActive\" AS PhotoIsActive, p.\"DateCreated\" AS PhotoDateCreated, p.\"DateUpdated\" AS PhotoDateUpdated, p.\"CreatedBy\" AS PhotoCreatedBy, ");
                    query.Append("tc.\"Id\" AS TouristSiteCategoryId, tc.\"TouristSiteId\", tc.\"CategoryId\", tc.\"IsActive\" AS TouristSiteCategoryIsActive ");
                    query.Append("FROM \"TouristSites\" t ");
                    query.Append("LEFT JOIN \"Location\" l ON t.\"LocationId\" = l.\"Id\" ");
                    query.Append("LEFT JOIN \"GeoLocation\" g ON l.\"Id\" = g.\"LocationId\" ");
                    query.Append("LEFT JOIN \"Comment\" c ON t.\"Id\" = c.\"TouristSitesId\" ");
                    query.Append("LEFT JOIN \"Photo\" p ON t.\"Id\" = p.\"TouristSiteId\" ");
                    query.Append("LEFT JOIN \"TouristSiteCategory\" tc ON t.\"Id\" = tc.\"TouristSiteId\" ");
                    query.Append("WHERE t.\"Id\" = @siteId ");
                    query.Append("AND t.\"IsActive\" = true");

                    cmd.Parameters.AddWithValue("@siteId", id);
                    cmd.CommandText = query.ToString();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (site == null)
                            {
                                site = MapTouristSite(reader);
                            }
                            if (reader["SiteLocationId"] != DBNull.Value)
                            {
                                if (reader["LocationIsActive"] != DBNull.Value && (bool)reader["LocationIsActive"])
                                {
                                    var location = MapLocation(reader);
                                    site.Location = location;
                                }
                            }
                            if (reader["CommentId"] != DBNull.Value)
                            {
                                if (reader["CommentIsActive"] != DBNull.Value && (bool)reader["CommentIsActive"])
                                {
                                    var comment = MapComment(reader);
                                    site.Comments.Add(comment);
                                }
                            }
                            if (reader["PhotoId"] != DBNull.Value)
                            {
                                if (reader["PhotoIsActive"] != DBNull.Value && (bool)reader["PhotoIsActive"])
                                {
                                    var photo = MapPhoto(reader);
                                    site.Photos.Add(photo);
                                }
                            }
                            if (reader["TouristSiteCategoryId"] != DBNull.Value)
                            {
                                if (reader["TouristSiteCategoryIsActive"] != DBNull.Value && (bool)reader["TouristSiteCategoryIsActive"])
                                {
                                    ITouristSiteCategoryModel touristSiteCategory = MapTouristSiteCategory(reader);
                                    site.SiteCategories.Add(touristSiteCategory);
                                }
                            }
                        }
                    }
                }
            }
            return site;
        }


        public async Task AddTouristSiteAsync(ITouristSitesModel site)
        {
            using(var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                using(var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;

                    StringBuilder query = new StringBuilder();
                    query.Append("INSERT INTO \"TouristSites\" (\"Id\", \"LocationId\", \"SiteName\", \"Link\", \"DateCreated\", \"DateUpdated\", \"CreatedBy\", \"UpdatedBy\", \"IsActive\", \"Fsq_Id\", \"Popularity\", \"Rating\", \"Description\", \"WebsiteUrl\", \"Email\", \"HoursOpen\", \"FacebookId\", \"Instagram\", \"Twitter\", \"NumberOfLike\") ");
                    query.Append("VALUES (@id, @locationId, @siteName, @link, @dateCreated, @dateUpdated, @createdBy, @updatedBy, @isActive, @fsq_Id, @popularity, @rating, @description, @websiteUrl, @email, @hoursOpen, @facebookId, @instagram, @twitter, @numberOfLikes)");

                    cmd.CommandText= query.ToString();

                    cmd.Parameters.AddWithValue("@id", site.Id);
                    cmd.Parameters.AddWithValue("@locationId", site.LocationId);
                    cmd.Parameters.AddWithValue("@siteName", site.Name ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@link", site.Link ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@dateCreated", site.DateCreated);
                    cmd.Parameters.AddWithValue("@dateUpdated", site.DateUpdated);
                    cmd.Parameters.AddWithValue("@createdBy", site.CreatedBy);
                    cmd.Parameters.AddWithValue("@updatedBy", site.UpdatedBy);
                    cmd.Parameters.AddWithValue("@isActive", site.IsActive);
                    cmd.Parameters.AddWithValue("@fsq_Id", site.Fsq_Id ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@popularity", site.Popularity);
                    cmd.Parameters.AddWithValue("@rating", site.Rating);
                    cmd.Parameters.AddWithValue("@description", site.Description ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@websiteUrl", site.WebsiteUrl ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@email", site.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@hoursOpen", site.HoursOpen ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@facebookId", site.FacebookId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@instagram", site.Instagram ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@twitter", site.Twitter ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@numberOfLikes", site.NumberOfLikes ?? (object)DBNull.Value);



                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task UpdateTouristSiteAsync(Guid id, ITouristSitesModel site)
        {
            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;
                    StringBuilder query = new StringBuilder();
                    query.Append("UPDATE \"TouristSites\" SET ");

                    if (!string.IsNullOrEmpty(site.Name))
                    {
                        query.Append("\"SiteName\" = @siteName, ");
                        cmd.Parameters.AddWithValue("@siteName", site.Name);
                    }
                    if (!string.IsNullOrEmpty(site.Link))
                    {
                        query.Append("\"Link\" = @link, ");
                        cmd.Parameters.AddWithValue("@link", site.Link);
                    }
                    if (site.Popularity.HasValue)
                    {
                        query.Append("\"Popularity\" = @popularity, ");
                        cmd.Parameters.AddWithValue("@popularity", site.Popularity.Value);
                    }
                    if (site.Rating.HasValue)
                    {
                        query.Append("\"Rating\" = @rating, ");
                        cmd.Parameters.AddWithValue("@rating", site.Rating.Value);
                    }
                    if (!string.IsNullOrEmpty(site.Description))
                    {
                        query.Append("\"Description\" = @description, ");
                        cmd.Parameters.AddWithValue("@description", site.Description);
                    }
                    if (!string.IsNullOrEmpty(site.WebsiteUrl))
                    {
                        query.Append("\"WebsiteUrl\" = @websiteUrl, ");
                        cmd.Parameters.AddWithValue("@websiteUrl", site.WebsiteUrl);
                    }
                    if (site.NumberOfLikes != null)
                    {
                        if (site.NumberOfLikes == 0)
                        {
                            cmd.Parameters.AddWithValue("@numberOfLikes", DBNull.Value);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@numberOfLikes", site.NumberOfLikes);
                        }
                        query.Append("\"NumberOfLikes\" = @numberOfLikes, ");
                    }
                    if (!string.IsNullOrEmpty(site.Email))
                    {
                        query.Append("\"Email\" = @email, ");
                        cmd.Parameters.AddWithValue("@email", site.Email);
                    }
                    if (!string.IsNullOrEmpty(site.HoursOpen))
                    {
                        query.Append("\"HoursOpen\" = @hoursOpen, ");
                        cmd.Parameters.AddWithValue("@hoursOpen", site.HoursOpen);
                    }
                    if (!string.IsNullOrEmpty(site.FacebookId))
                    {
                        query.Append("\"FacebookId\" = @facebookId, ");
                        cmd.Parameters.AddWithValue("@facebookId", site.FacebookId);
                    }
                    if (!string.IsNullOrEmpty(site.Instagram))
                    {
                        query.Append("\"Instagram\" = @instagram, ");
                        cmd.Parameters.AddWithValue("@instagram", site.Instagram);
                    }
                    if (!string.IsNullOrEmpty(site.Twitter))
                    {
                        query.Append("\"Twitter\" = @twitter, ");
                        cmd.Parameters.AddWithValue("@twitter", site.Twitter);
                    }
                    if (site.UpdatedBy != Guid.Empty)
                    {
                        query.Append("\"UpdatedBy\" = @updatedBy, ");
                        cmd.Parameters.AddWithValue("@updatedBy", site.UpdatedBy);
                    }

                    query.Append("\"DateUpdated\" = @dateUpdated, ");
                    cmd.Parameters.AddWithValue("@dateUpdated", DateTime.Now);

                    query.Length -= 2;
                    query.Append(" WHERE \"Id\" = @id");

                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.CommandText = query.ToString();

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }


        public async Task<ITouristSitesModel> GetPerformerByOpenTripMapIdAsync(string openTripMapId)
        {
            ITouristSitesModel site = null;
            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;

                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT t.\"Id\", t.\"SiteName\", t.\"Link\", t.\"LocationId\" AS TouristSiteLocationId, t.\"Fsq_Id\", t.\"Popularity\", t.\"Rating\", t.\"Description\", t.\"WebsiteUrl\", t.\"Email\", t.\"HoursOpen\", t.\"FacebookId\", t.\"Instagram\", t.\"Twitter\", t.\"IsActive\", t.\"NumberOfLikes\" AS TouristSiteNumberOfLikes, ");
                    query.Append("c.\"Id\" AS CommentId, c.\"CommentText\", c.\"IsActive\", c.\"UserId\" AS CommentUserId, c.\"EventId\" AS CommentEventId, c.\"TouristSitesId\" AS CommentTouristSitesId, c.\"StoryId\" AS CommentStoryId, c.\"DateCreated\" AS CommentDateCreated, c.\"DateUpdated\" AS CommentDateUpdated, c.\"CreatedBy\" AS CommentCreatedBy, c.\"ParentComment\", c.\"NumberOfLikes\", c.\"IsReported\", ");
                    query.Append("l.\"Id\" AS SiteLocationId, l.\"Country\", l.\"City\", l.\"Village\", l.\"Address\", l.\"NameOfPlace\", l.\"IsActive\", l.\"JambaseIdentifier\", ");
                    query.Append("p.\"Id\" AS PhotoId, p.\"ImageData\", p.\"ImagePrefix\", p.\"ImageSuffix\", p.\"TouristSiteId\" AS PhotoTouristSiteId, p.\"StoryId\" AS PhotoStoryId, p.\"UserId\" AS PhotoUserId, p.\"DateCreated\" AS PhotoDateCreated, p.\"DateUpdated\" AS PhotoDateUpdated, p.\"CreatedBy\" AS PhotoCreatedBy, ");
                    query.Append("tc.\"Id\" AS TouristSiteCategoryId, tc.\"TouristSiteId\", tc.\"CategoryId\", tc.\"IsActive\"  ");
                    query.Append("FROM \"TouristSites\" t ");
                    query.Append("LEFT JOIN \"Comment\" c ON t.\"Id\" = c.\"TouristSitesId\" ");
                    query.Append("LEFT JOIN \"Location\" l ON t.\"LocationId\" = l.\"Id\" ");
                    query.Append("LEFT JOIN \"Photo\" p ON t.\"Id\" = p.\"TouristSiteId\" ");
                    query.Append("LEFT JOIN \"TouristSiteCategory\" tc ON t.\"Id\" = tc.\"TouristSiteId\" ");
                    query.Append("WHERE t.\"Fsq_Id\" = @fsq_Id ");
                    query.Append("AND t.\"IsActive\" = true");

                    cmd.Parameters.AddWithValue("@fsq_Id", openTripMapId);
                    cmd.CommandText = query.ToString();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {

                            if (site == null)
                            {
                                site = MapTouristSite(reader);
                            }
                            if (reader["SiteLocationId"] != DBNull.Value)
                            {
                                var location = MapLocation(reader);
                                site.Location = location;
                            }
                            if (reader["CommentId"] != DBNull.Value)
                            {
                                var comment = MapComment(reader);
                                site.Comments.Add(comment);
                            }
                            if (reader["PhotoId"] != DBNull.Value)
                            {
                                var photo = MapPhoto(reader);
                                site.Photos.Add(photo);
                            }
                            if (reader["TouristSiteCategoryId"] != DBNull.Value)
                            {
                                ITouristSiteCategoryModel touristSiteCategory = MapTouristSiteCategory(reader);
                                site.SiteCategories.Add(touristSiteCategory);
                            }
                        }
                    }
                }
            }
            return site;
        }

        public async Task DeleteTouristSiteAsync(Guid id)
        {
            await using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using var cmd = connection.CreateCommand();
                cmd.Connection = connection;
                cmd.Parameters.AddWithValue("@id", id);

                await using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var queryTouristSite = new StringBuilder();
                        queryTouristSite.Append("UPDATE \"TouristSites\" SET \"IsActive\" = false WHERE \"Id\" = @id");
                        cmd.CommandText = queryTouristSite.ToString();
                        await cmd.ExecuteNonQueryAsync();

                        var queryLocation = new StringBuilder();
                        queryLocation.Append("UPDATE \"Location\" SET \"IsActive\" = false WHERE \"Id\" IN (SELECT \"LocationId\" FROM \"TouristSites\" WHERE \"Id\" = @id)");
                        cmd.CommandText = queryLocation.ToString();
                        await cmd.ExecuteNonQueryAsync();

                        var queryGeoLocation = new StringBuilder();
                        queryGeoLocation.Append("UPDATE \"GeoLocation\" SET \"IsActive\" = false WHERE \"LocationId\" IN (SELECT \"LocationId\" FROM \"TouristSites\" WHERE \"Id\" = @id)");
                        cmd.CommandText = queryGeoLocation.ToString();
                        await cmd.ExecuteNonQueryAsync();

                        var queryComment = new StringBuilder();
                        queryComment.Append("UPDATE \"Comment\" SET \"IsActive\" = false WHERE \"TouristSitesId\" = @id");
                        cmd.CommandText = queryComment.ToString();
                        await cmd.ExecuteNonQueryAsync();

                        var queryTouristSiteCategory = new StringBuilder();
                        queryTouristSiteCategory.Append("UPDATE \"TouristSiteCategory\" SET \"IsActive\" = false WHERE \"TouristSiteId\" = @id");
                        cmd.CommandText = queryTouristSiteCategory.ToString();
                        await cmd.ExecuteNonQueryAsync();

                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        private ITouristSitesModel MapTouristSite(NpgsqlDataReader reader)
        {
            ITouristSitesModel touristSite = new TouristSitesModel
            {
                Id = (Guid)reader["Id"],
                LocationId = reader["TouristSiteLocationId"] != DBNull.Value ? (Guid)reader["TouristSiteLocationId"] : null,
                Name = !string.IsNullOrWhiteSpace(Convert.ToString(reader["SiteName"])) ? Convert.ToString(reader["SiteName"]) : null,
                Link = !string.IsNullOrWhiteSpace(Convert.ToString(reader["Link"])) ? Convert.ToString(reader["Link"]) : null,
                Fsq_Id = !string.IsNullOrWhiteSpace(Convert.ToString(reader["Fsq_Id"])) ? Convert.ToString(reader["Fsq_Id"]) : null,
                Popularity = reader["Popularity"] != DBNull.Value ? Convert.ToDouble(reader["Popularity"]) : 0.0,
                Rating = reader["Rating"] != DBNull.Value ? Convert.ToDouble(reader["Rating"]) : 0.0,
                Description = !string.IsNullOrWhiteSpace(Convert.ToString(reader["Description"])) ? Convert.ToString(reader["Description"]) : null,
                WebsiteUrl = !string.IsNullOrWhiteSpace(Convert.ToString(reader["WebsiteUrl"])) ? Convert.ToString(reader["WebsiteUrl"]) : null,
                Email = !string.IsNullOrWhiteSpace(Convert.ToString(reader["Email"])) ? Convert.ToString(reader["Email"]) : null,
                HoursOpen = !string.IsNullOrWhiteSpace(Convert.ToString(reader["HoursOpen"])) ? Convert.ToString(reader["HoursOpen"]) : null,
                FacebookId = !string.IsNullOrWhiteSpace(Convert.ToString(reader["FacebookId"])) ? Convert.ToString(reader["FacebookId"]) : null,
                Instagram = !string.IsNullOrWhiteSpace(Convert.ToString(reader["Instagram"])) ? Convert.ToString(reader["Instagram"]) : null,
                Twitter = !string.IsNullOrWhiteSpace(Convert.ToString(reader["Twitter"])) ? Convert.ToString(reader["Twitter"]) : null,
                NumberOfLikes = reader["TouristSiteNumberOfLikes"] == DBNull.Value ? null : Convert.ToInt32(reader["TouristSiteNumberOfLikes"]),
                IsActive = Convert.ToBoolean(reader["IsActive"]),

                Location = new LocationModel
                {
                    Id = reader["SiteLocationId"] != DBNull.Value ? (Guid)reader["SiteLocationId"] : Guid.Empty,
                    Country = !string.IsNullOrWhiteSpace(Convert.ToString(reader["Country"])) ? Convert.ToString(reader["Country"]) : null,
                    City = !string.IsNullOrWhiteSpace(Convert.ToString(reader["City"])) ? Convert.ToString(reader["City"]) : null,
                    Village = !string.IsNullOrWhiteSpace(Convert.ToString(reader["Village"])) ? Convert.ToString(reader["Village"]) : null,
                    Address = !string.IsNullOrWhiteSpace(Convert.ToString(reader["Address"])) ? Convert.ToString(reader["Address"]) : null,
                    NameOfPlace = !string.IsNullOrWhiteSpace(Convert.ToString(reader["NameOfPlace"])) ? Convert.ToString(reader["NameOfPlace"]) : null,
                    IsActive = reader["LocationIsActive"] != DBNull.Value && Convert.ToBoolean(reader["LocationIsActive"]),
                    JambaseIdentifier = !string.IsNullOrWhiteSpace(Convert.ToString(reader["JambaseIdentifier"])) ? Convert.ToString(reader["JambaseIdentifier"]) : null,
                },

                GeoLocations = new List<IGeoLocationModel>
                {
                    new GeoLocation
                    {
                        Id = reader["GeoLocationId"] != DBNull.Value ? (Guid)reader["GeoLocationId"] : Guid.Empty,
                        Latitude = reader["Latitude"] != DBNull.Value ? Convert.ToDouble(reader["Latitude"]) : 0.0,
                        Longitude = reader["Longitude"] != DBNull.Value ? Convert.ToDouble(reader["Longitude"]) : 0.0,
                        LocationId = reader["GeoLocationLocationId"] != DBNull.Value ? (Guid)reader["GeoLocationLocationId"] : Guid.Empty,
                        IsActive = reader["GeoLocationIsActive"] != DBNull.Value && Convert.ToBoolean(reader["GeoLocationIsActive"]),
                    }
                },

                Comments = new List<ICommentModel>(),
                Photos = new List<IPhotoModel>(),
                SiteCategories = new List<ITouristSiteCategoryModel>(),
            };

            return touristSite;
        }

        private ILocationModel MapLocation(NpgsqlDataReader reader)
        {
            return new LocationModel
            {
                Id = (Guid)reader["SiteLocationId"],
                Country = !string.IsNullOrWhiteSpace(Convert.ToString(reader["Country"])) ? Convert.ToString(reader["Country"]) : null,
                City = !string.IsNullOrWhiteSpace(Convert.ToString(reader["City"])) ? Convert.ToString(reader["City"]) : null,
                Village = !string.IsNullOrWhiteSpace(Convert.ToString(reader["Village"])) ? Convert.ToString(reader["Village"]) : null,
                Address = !string.IsNullOrWhiteSpace(Convert.ToString(reader["Address"])) ? Convert.ToString(reader["Address"]) : null,
                NameOfPlace = !string.IsNullOrWhiteSpace(Convert.ToString(reader["NameOfPlace"])) ? Convert.ToString(reader["NameOfPlace"]) : null,
                IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
                JambaseIdentifier = !string.IsNullOrWhiteSpace(Convert.ToString(reader["JambaseIdentifier"])) ? Convert.ToString(reader["JambaseIdentifier"]) : null,

            };
        }

        private ICommentModel MapComment(NpgsqlDataReader reader)
        {
            return new CommentModel
            {
                Id = (Guid)reader["CommentId"],
                Text = Convert.ToString(reader["CommentText"]),
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                DateCreated = (DateTime)reader["CommentDateCreated"],
                DateUpdated = (DateTime)reader["CommentDateUpdated"],
                CreatedBy = (Guid)reader["CommentCreatedBy"],
                UserId =(Guid)reader["CommentUserId"],
                EventId = Convert.IsDBNull(reader["CommentEventId"]) ? null : (Guid)reader["CommentEventId"],
                StoryId = Convert.IsDBNull(reader["CommentStoryId"]) ? null : (Guid)reader["CommentStoryId"],
                TouristSiteId = Convert.IsDBNull(reader["CommentTouristSitesId"]) ? null : (Guid)reader["CommentTouristSitesId"],
                ParentComment = Convert.IsDBNull(reader["ParentComment"]) ? null : (Guid)reader["ParentComment"],
                NumberOfLikes = reader["NumberOfLikes"] == DBNull.Value ? null : Convert.ToInt32(reader["NumberOfLikes"]),
                IsReported = reader["IsReported"] == DBNull.Value ? null : Convert.ToBoolean(reader["IsReported"]),
            };
        }
        private IPhotoModel MapPhoto(NpgsqlDataReader reader)
        {
            return new PhotoModel
            {
                Id = (Guid)reader["PhotoId"],
                ImageData = !string.IsNullOrWhiteSpace(Convert.ToString(reader["ImageData"])) ? Convert.ToString(reader["ImageData"]) : null,
                ImagePrefix = !string.IsNullOrWhiteSpace(Convert.ToString(reader["ImagePrefix"])) ? Convert.ToString(reader["ImagePrefix"]) : null,
                ImageSuffix = !string.IsNullOrWhiteSpace(Convert.ToString(reader["ImageSuffix"])) ? Convert.ToString(reader["ImageSuffix"]) : null,
                TouristSiteId = reader["PhotoTouristSiteId"] != DBNull.Value ? (Guid)reader["PhotoTouristSiteId"] : null,
                StoryId = reader["PhotoStoryId"] != DBNull.Value ? (Guid)reader["PhotoStoryId"] : null,
                UserId = (Guid)reader["PhotoUserId"],
                DateCreated = (DateTime)reader["PhotoDateCreated"],
                DateUpdated = (DateTime)reader["PhotoDateUpdated"],
                CreatedBy = (Guid)reader["PhotoCreatedBy"],
            };
        }
        private ITouristSiteCategoryModel MapTouristSiteCategory(NpgsqlDataReader reader)
        {
            return new TouristSiteCategoryModel
            {
                Id = (Guid)reader["TouristSiteCategoryId"],
                CategoryId = (Guid)reader["CategoryId"],
                TouristSiteId = (Guid)reader["TouristSiteId"],
                IsActive = reader["IsActive"] != DBNull.Value ? (bool)reader["IsActive"] : (bool?)null,
            };
        }
    }
}
