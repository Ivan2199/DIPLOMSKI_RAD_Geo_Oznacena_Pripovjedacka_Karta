﻿using GeoTagMap.Common.Paging;
using GeoTagMap.Common.Sorting;
using GeoTagMap.Common;
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
using System.Xml.Linq;
using GeoTagMap.Common.Filtering;

namespace GeoTagMap.Repository
{
    public class StoryRepository : IStoryRepository
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["CONNECTION_STRING"].ToString();

        public async Task<PagingInfo<IStoryModel>> GetAllStoriesAsync(Paging paging, Sorting sort, StoryFiltering filtering)
        {
            Dictionary<Guid, IStoryModel> stories = new Dictionary<Guid, IStoryModel>();
            int totalStories = 0;

            using (var con = new NpgsqlConnection(_connectionString))
            {
                await con.OpenAsync();

                List<Guid> storyIds = new List<Guid>();
                using (var storyIdCmd = con.CreateCommand())
                {
                    StringBuilder storyIdQuery = new StringBuilder();
                    storyIdQuery.Append("SELECT s.\"Id\" FROM \"Story\" s ");
                    storyIdQuery.Append("LEFT JOIN \"Location\" l ON s.\"LocationId\" = l.\"Id\" ");
                    storyIdQuery.Append("WHERE s.\"IsActive\" = true");

                    if (filtering.Date != null)
                    {
                        storyIdQuery.Append(" AND s.\"Date\" >= @Date");
                        storyIdCmd.Parameters.AddWithValue("@Date", filtering.Date);
                    }
                    if (filtering.NumberOfLikes != null)
                    {
                        storyIdQuery.Append(" AND s.\"NumberOfLikes\" >= @StoryNumberOfLikes");
                        storyIdCmd.Parameters.AddWithValue("@StoryNumberOfLikes", filtering.NumberOfLikes);
                    }
                    if (filtering.IsReported != null)
                    {
                        storyIdQuery.Append(" AND s.\"IsReported\" >= @StoryIsReported");
                        storyIdCmd.Parameters.AddWithValue("@StoryIsReported", filtering.IsReported);
                    }
                    if (!string.IsNullOrEmpty(filtering.Country))
                    {
                        storyIdQuery.Append(" AND LOWER(l.\"Country\") = LOWER(@Country)");
                        storyIdCmd.Parameters.AddWithValue("@Country", filtering.Country.ToLower());
                    }
                    if (!string.IsNullOrEmpty(filtering.City))
                    {
                        storyIdQuery.Append(" AND LOWER(l.\"City\") = LOWER(@City)");
                        storyIdCmd.Parameters.AddWithValue("@City", filtering.City.ToLower());
                    }

                    if (!string.IsNullOrEmpty(sort.OrderBy))
                    {
                        string orderByColumn = sort.OrderBy;
                        string sortOrder = sort.SortOrder.ToUpper() == "DESC" ? "DESC" : "ASC";

                        storyIdQuery.Append($" ORDER BY s.\"{orderByColumn}\" {sortOrder}, s.\"Id\"");
                    }
                    else
                    {
                        storyIdQuery.Append(" ORDER BY s.\"Date\" ASC, s.\"Id\"");
                    }

                    storyIdQuery.Append(" LIMIT @Limit OFFSET @Offset");
                    storyIdCmd.Parameters.AddWithValue("@Limit", paging.RRP);
                    storyIdCmd.Parameters.AddWithValue("@Offset", (paging.PageNumber - 1) * paging.RRP);

                    storyIdCmd.CommandText = storyIdQuery.ToString();

                    System.Diagnostics.Debug.WriteLine(storyIdCmd.CommandText);

                    using (var reader = await storyIdCmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            storyIds.Add(reader.GetGuid(reader.GetOrdinal("Id")));
                        }
                    }
                }

                if (storyIds.Count == 0)
                {
                    return new PagingInfo<IStoryModel>
                    {
                        List = new List<IStoryModel>(),
                        RRP = paging.RRP,
                        PageNumber = paging.PageNumber,
                        TotalSize = 0
                    };
                }

                using (var countCmd = con.CreateCommand())
                {
                    StringBuilder countQuery = new StringBuilder();
                    countQuery.Append("SELECT COUNT(DISTINCT s.\"Id\") FROM \"Story\" s ");
                    countQuery.Append("LEFT JOIN \"Location\" l ON s.\"LocationId\" = l.\"Id\" ");
                    countQuery.Append("WHERE s.\"IsActive\" = true");

                    if (filtering.Date != null)
                    {
                        countQuery.Append(" AND s.\"Date\" >= @Date");
                        countCmd.Parameters.AddWithValue("@Date", filtering.Date);
                    }
                    if (filtering.NumberOfLikes != null)
                    {
                        countQuery.Append(" AND s.\"NumberOfLikes\" >= @StoryNumberOfLikes");
                        countCmd.Parameters.AddWithValue("@StoryNumberOfLikes", filtering.NumberOfLikes);
                    }
                    if (filtering.IsReported != null)
                    {
                        countQuery.Append(" AND s.\"IsReported\" >= @StoryIsReported");
                        countCmd.Parameters.AddWithValue("@StoryIsReported", filtering.IsReported);
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
                    totalStories = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
                }

                using (var cmd = con.CreateCommand())
                {
                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT s.\"Id\", s.\"StoryText\", s.\"Date\", s.\"LocationId\" AS StoryLocationId, s.\"UserId\" AS StoryUserId, s.\"NumberOfLikes\" AS StoryNumberOfLikes, s.\"IsReported\", ");
                    query.Append("l.\"Id\" AS LocationId, l.\"Country\", l.\"City\", l.\"Village\", l.\"Address\", l.\"NameOfPlace\", l.\"IsActive\" AS LocationIsActive, l.\"JambaseIdentifier\", ");
                    query.Append("g.\"Id\" AS GeoLocationId, g.\"Latitude\", g.\"Longitude\", g.\"LocationId\" AS GeoLocationLocationId, g.\"IsActive\" AS GeoLocationIsActive, ");
                    query.Append("c.\"Id\" AS CommentId, c.\"CommentText\", c.\"IsActive\" AS CommentIsActive, c.\"UserId\" AS CommentUserId, c.\"EventId\" AS CommentEventId, c.\"TouristSitesId\" AS CommentTouristSitesId, c.\"StoryId\" AS CommentStoryId, c.\"DateCreated\" AS CommentDateCreated, c.\"DateUpdated\" AS CommentDateUpdated, c.\"CreatedBy\" AS CommentCreatedBy, ");
                    query.Append("p.\"Id\" AS PhotoId, p.\"ImageData\", p.\"ImagePrefix\", p.\"ImageSuffix\", p.\"TouristSiteId\" AS PhotoTouristSiteId, p.\"StoryId\" AS PhotoStoryId, p.\"UserId\" AS PhotoUserId, p.\"DateCreated\" AS PhotoDateCreated, p.\"DateUpdated\" AS PhotoDateUpdated, p.\"CreatedBy\" AS PhotoCreatedBy, ");
                    query.Append("u.\"Id\" AS UserId, u.\"Username\", u.\"FirstName\", u.\"LastName\", u.\"Email\", u.\"Password\", u.\"Image\", u.\"IsActive\", u.\"RoleId\" AS UserRoleId ");
                    query.Append("FROM \"Story\" s ");
                    query.Append("LEFT JOIN \"Location\" l ON s.\"LocationId\" = l.\"Id\" ");
                    query.Append("LEFT JOIN \"GeoLocation\" g ON l.\"Id\" = g.\"LocationId\" ");
                    query.Append("LEFT JOIN \"Comment\" c ON s.\"Id\" = c.\"StoryId\" ");
                    query.Append("LEFT JOIN \"User\" u ON s.\"UserId\" = u.\"Id\" ");
                    query.Append("LEFT JOIN \"Photo\" p ON s.\"Id\" = p.\"StoryId\" ");
                    query.Append("WHERE s.\"Id\" = ANY(@StoryIds::uuid[])");

                    cmd.Parameters.Add("@StoryIds", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Uuid).Value = storyIds.ToArray();

                    cmd.CommandText = query.ToString();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            IStoryModel currentStory;
                            Guid currentStoryId = reader.GetGuid(reader.GetOrdinal("Id"));

                            if (!stories.TryGetValue(currentStoryId, out currentStory))
                            {
                                currentStory = MapStory(reader);
                                stories.Add(currentStoryId, currentStory);
                            }
                            if (reader["LocationId"] != DBNull.Value)
                            {
                                if (reader["LocationIsActive"] != DBNull.Value && (bool)reader["LocationIsActive"])
                                {
                                    var location = MapLocation(reader);
                                    currentStory.Location = location;
                                }
                            }
                            if (reader["CommentId"] != DBNull.Value)
                            {
                                if (reader["CommentIsActive"] != DBNull.Value && (bool)reader["CommentIsActive"])
                                {
                                    var comment = MapComment(reader);
                                    currentStory.Comment.Add(comment);
                                }
                            }
                            if (reader["PhotoId"] != DBNull.Value)
                            {
                                var photo = MapPhoto(reader);
                                currentStory.Photos.Add(photo);
                            }
                            if (reader["UserId"] != DBNull.Value)
                            {
                                var user = MapUser(reader);
                                currentStory.User = user;
                            }
                        }
                    }
                }
            }

            PagingInfo<IStoryModel> pagingInfo = new PagingInfo<IStoryModel>
            {
                List = stories.Values.ToList(),
                RRP = paging.RRP,
                PageNumber = paging.PageNumber,
                TotalSize = totalStories
            };
            return pagingInfo;
        }



        public async Task<List<IStoryModel>> GetAllStoriesByUserIdAsync(Guid userId)
        {
            Dictionary<Guid, IStoryModel> stories = new Dictionary<Guid, IStoryModel>();
            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;

                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT s.\"Id\", s.\"StoryText\", s.\"Date\", s.\"LocationId\" AS StoryLocationId, s.\"UserId\" AS StoryUserId, s.\"NumberOfLikes\" AS StoryNumberOfLikes, s.\"IsReported\", ");
                    query.Append("l.\"Id\" AS LocationId, l.\"Country\", l.\"City\", l.\"Village\", l.\"Address\", l.\"NameOfPlace\", l.\"IsActive\" AS LocationIsActive, l.\"JambaseIdentifier\",  ");
                    query.Append("g.\"Id\" AS GeoLocationId, g.\"Latitude\", g.\"Longitude\", g.\"LocationId\" AS GeoLocationLocationId, g.\"IsActive\" AS GeoLocationIsActive, ");
                    query.Append("c.\"Id\" AS CommentId, c.\"CommentText\", c.\"IsActive\", c.\"UserId\" AS CommentUserId, c.\"EventId\" AS CommentEventId, c.\"TouristSitesId\" AS CommentTouristSitesId, c.\"StoryId\" AS CommentStoryId, c.\"DateCreated\" AS CommentDateCreated, c.\"DateUpdated\" AS CommentDateUpdated, c.\"CreatedBy\" AS CommentCreatedBy, ");
                    query.Append("p.\"Id\" AS PhotoId, p.\"ImageData\", p.\"ImagePrefix\", p.\"ImageSuffix\", p.\"TouristSiteId\" AS PhotoTouristSiteId, p.\"StoryId\" AS PhotoStoryId, p.\"UserId\" AS PhotoUserId, p.\"DateCreated\" AS PhotoDateCreated, p.\"DateUpdated\" AS PhotoDateUpdated, p.\"CreatedBy\" AS PhotoCreatedBy, ");
                    query.Append("u.\"Id\" AS UserId, u.\"Username\", u.\"FirstName\", u.\"LastName\", u.\"Email\", u.\"Password\", u.\"Image\", u.\"IsActive\", u.\"RoleId\" AS UserRoleId ");
                    query.Append("FROM \"Story\" s ");
                    query.Append("LEFT JOIN \"Location\" l ON s.\"LocationId\" = l.\"Id\" ");
                    query.Append("LEFT JOIN \"GeoLocation\" g ON l.\"Id\" = g.\"LocationId\" ");
                    query.Append("LEFT JOIN \"Comment\" c ON s.\"Id\" = c.\"StoryId\" ");
                    query.Append("LEFT JOIN \"User\" u ON s.\"UserId\" = u.\"Id\" ");
                    query.Append("LEFT JOIN \"Photo\" p ON s.\"Id\" = p.\"StoryId\" ");
                    query.Append("WHERE s.\"UserId\" = @userId ");
                    query.Append("AND s.\"IsActive\" = true");

                    cmd.Parameters.AddWithValue("@userId", userId);

                    cmd.CommandText = query.ToString();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            IStoryModel currentStory;
                            Guid storyId = reader.GetGuid(reader.GetOrdinal("Id"));

                            if (!stories.TryGetValue(storyId, out currentStory))
                            {
                                currentStory = MapStory(reader);
                                stories.Add(storyId, currentStory);
                            }
                            if (reader["LocationId"] != DBNull.Value)
                            {
                                var location = MapLocation(reader);
                                currentStory.Location = location;
                            }
                            if (reader["CommentId"] != DBNull.Value)
                            {
                                var comment = MapComment(reader);
                                currentStory.Comment.Add(comment);
                            }
                            if (reader["PhotoId"] != DBNull.Value)
                            {
                                var photo = MapPhoto(reader);
                                currentStory.Photos.Add(photo);
                            }
                            if (reader["UserId"] != DBNull.Value)
                            {
                                var user = MapUser(reader);
                                currentStory.User = user;
                            }
                        }
                    }
                }
            }
            return stories.Values.ToList();
        }

        public async Task<IStoryModel> GetStoryAsync(Guid id)
        {
            IStoryModel story = null;
            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;

                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT s.\"Id\", s.\"StoryText\", s.\"Date\", s.\"LocationId\" AS StoryLocationId, s.\"UserId\" AS StoryUserId, s.\"NumberOfLikes\" AS StoryNumberOfLikes, s.\"IsReported\", ");
                    query.Append("l.\"Id\" AS LocationId, l.\"Country\", l.\"City\", l.\"Village\", l.\"Address\", l.\"NameOfPlace\", l.\"IsActive\" AS LocationIsActive, l.\"JambaseIdentifier\",  ");
                    query.Append("g.\"Id\" AS GeoLocationId, g.\"Latitude\", g.\"Longitude\", g.\"LocationId\" AS GeoLocationLocationId, g.\"IsActive\" AS GeoLocationIsActive, ");
                    query.Append("c.\"Id\" AS CommentId, c.\"CommentText\", c.\"IsActive\", c.\"UserId\" AS CommentUserId, c.\"EventId\" AS CommentEventId, c.\"TouristSitesId\" AS CommentTouristSitesId, c.\"StoryId\" AS CommentStoryId, c.\"DateCreated\" AS CommentDateCreated, c.\"DateUpdated\" AS CommentDateUpdated, c.\"CreatedBy\" AS CommentCreatedBy, ");
                    query.Append("p.\"Id\" AS PhotoId, p.\"ImageData\", p.\"ImagePrefix\", p.\"ImageSuffix\", p.\"TouristSiteId\" AS PhotoTouristSiteId, p.\"StoryId\" AS PhotoStoryId, p.\"UserId\" AS PhotoUserId, p.\"DateCreated\" AS PhotoDateCreated, p.\"DateUpdated\" AS PhotoDateUpdated, p.\"CreatedBy\" AS PhotoCreatedBy, ");
                    query.Append("u.\"Id\" AS UserId, u.\"Username\", u.\"FirstName\", u.\"LastName\", u.\"Email\", u.\"Password\", u.\"Image\", u.\"IsActive\", u.\"RoleId\" AS UserRoleId ");
                    query.Append("FROM \"Story\" s ");
                    query.Append("LEFT JOIN \"Location\" l ON s.\"LocationId\" = l.\"Id\" ");
                    query.Append("LEFT JOIN \"GeoLocation\" g ON l.\"Id\" = g.\"LocationId\" ");
                    query.Append("LEFT JOIN \"Comment\" c ON s.\"Id\" = c.\"StoryId\" ");
                    query.Append("LEFT JOIN \"User\" u ON s.\"UserId\" = u.\"Id\" ");
                    query.Append("LEFT JOIN \"Photo\" p ON s.\"Id\" = p.\"StoryId\" ");
                    query.Append("WHERE s.\"Id\" = @storyId ");
                    query.Append("AND s.\"IsActive\" = true");

                    cmd.Parameters.AddWithValue("@storyId", id);
                    cmd.CommandText = query.ToString();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {

                            if (story == null)
                            {
                                story = MapStory(reader);
                            }
                            if (reader["LocationId"] != DBNull.Value)
                            {
                                var location = MapLocation(reader);
                                story.Location = location;
                            }
                            if (reader["CommentId"] != DBNull.Value)
                            {
                                var comment = MapComment(reader);
                                story.Comment.Add(comment);
                            }
                            if(reader["PhotoId"] != DBNull.Value)
                            {
                                var photo = MapPhoto(reader);
                                story.Photos.Add(photo);
                            }
                            if (reader["UserId"] != DBNull.Value)
                            {
                                var user = MapUser(reader);
                                story.User = user;
                            }
                        }
                    }
                }
            }
            return story;
        }


        public async Task AddStoryAsync(IStoryModel story)
        {
            using(var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                using(var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;  

                    StringBuilder query = new StringBuilder();
                    query.Append("INSERT INTO \"Story\" (\"Id\", \"StoryText\", \"Date\", \"LocationId\", \"UserId\", \"DateCreated\", \"DateUpdated\", \"CreatedBy\", \"UpdatedBy\", \"IsActive\", \"NumberOfLikes\", \"IsReported\") ");
                    query.Append("VALUES (@id, @storyText, @date, @locationId, @userId, @dateCreated, @dateUpdated, @createdBy, @updatedBy, @isActive, @numberOfLikes, @isReported)");

                    cmd.CommandText= query.ToString();

                    cmd.Parameters.AddWithValue("@id", story.Id);
                    cmd.Parameters.AddWithValue("@storyText", story.Text ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@date", story.DateTime ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@locationId", story.LocationId);
                    cmd.Parameters.AddWithValue("@userId", story.UserId);
                    cmd.Parameters.AddWithValue("@dateCreated", story.DateCreated);
                    cmd.Parameters.AddWithValue("@dateUpdated", story.DateUpdated);
                    cmd.Parameters.AddWithValue("@createdBy", story.CreatedBy);
                    cmd.Parameters.AddWithValue("@updatedBy", story.UpdatedBy);
                    cmd.Parameters.AddWithValue("@isActive", story.IsActive);
                    cmd.Parameters.AddWithValue("@numberOfLikes", story.NumberOfLikes ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@isReported", story.IsReported ?? (object)DBNull.Value);


                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdateStoryAsync(Guid id, IStoryModel story)
        {
            using(var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                using(var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;
                    StringBuilder query = new StringBuilder();
                    query.Append("UPDATE \"Story\" SET ");

                    if (!string.IsNullOrEmpty(story.Text))
                    {
                        query.Append("\"StoryText\" = @text, ");
                        cmd.Parameters.AddWithValue("@text", story.Text);
                    }

                    if (story.DateTime != null)
                    {
                        query.Append("\"Date\" = @dateTime, ");
                        cmd.Parameters.AddWithValue("@dateTime", story.DateTime);
                    }

                    if (story.NumberOfLikes != null)
                    {
                        if (story.NumberOfLikes == 0)
                        {
                            cmd.Parameters.AddWithValue("@numberOfLikes", DBNull.Value);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@numberOfLikes", story.NumberOfLikes);
                        }
                        query.Append("\"NumberOfLikes\" = @numberOfLikes, ");
                    }
                    if (story.IsReported != null)
                    {
                        query.Append("\"IsReported\" = @isReported, ");
                        cmd.Parameters.AddWithValue("@isReported", story.IsReported);
                    }

                    if (story.UpdatedBy != Guid.Empty)
                    {
                        query.Append("\"UpdatedBy\" = @updatedBy, ");
                        cmd.Parameters.AddWithValue("@updatedBy", story.UpdatedBy);
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


        public async Task DeleteStoryAsync(Guid id)
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
                        var queryStory = new StringBuilder();
                        queryStory.Append("UPDATE \"Story\" SET \"IsActive\" = false WHERE \"Id\" = @id");
                        cmd.CommandText = queryStory.ToString();
                        await cmd.ExecuteNonQueryAsync();

                        var queryLocation = new StringBuilder();
                        queryLocation.Append("UPDATE \"Location\" SET \"IsActive\" = false WHERE \"Id\" IN (SELECT \"LocationId\" FROM \"Story\" WHERE \"Id\" = @id)");
                        cmd.CommandText = queryLocation.ToString();
                        await cmd.ExecuteNonQueryAsync();

                        var queryGeoLocation = new StringBuilder();
                        queryGeoLocation.Append("UPDATE \"GeoLocation\" SET \"IsActive\" = false WHERE \"LocationId\" IN (SELECT \"LocationId\" FROM \"Story\" WHERE \"Id\" = @id)");
                        cmd.CommandText = queryGeoLocation.ToString();
                        await cmd.ExecuteNonQueryAsync();

                        var queryComment = new StringBuilder();
                        queryComment.Append("UPDATE \"Comment\" SET \"IsActive\" = false WHERE \"StoryId\" = @id");
                        cmd.CommandText = queryComment.ToString();
                        await cmd.ExecuteNonQueryAsync();

                        var queryPhoto = new StringBuilder();
                        queryPhoto.Append("UPDATE \"Photo\" SET \"IsActive\" = false WHERE \"StoryId\" = @id");
                        cmd.CommandText = queryPhoto.ToString();
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

        private IStoryModel MapStory(NpgsqlDataReader reader)
        {
            return new StoryModel
            {
                Id = (Guid)reader["Id"],
                Text = !string.IsNullOrWhiteSpace(Convert.ToString(reader["StoryText"])) ? Convert.ToString(reader["StoryText"]) : null,
                LocationId = (Guid)reader["StoryLocationId"],
                UserId = (Guid)reader["StoryUserId"],
                DateTime = reader["Date"] != DBNull.Value ? Convert.ToDateTime(reader["Date"]) : null,
                NumberOfLikes = reader["StoryNumberOfLikes"] == DBNull.Value ? null : Convert.ToInt32(reader["StoryNumberOfLikes"]),
                IsReported = reader["IsReported"] != DBNull.Value && Convert.ToBoolean(reader["IsReported"]),

                Location = new LocationModel
                {
                    Id = reader["LocationId"] != DBNull.Value ? (Guid)reader["LocationId"] : Guid.Empty,
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

                Comment = Convert.IsDBNull(reader["CommentId"]) ? null : new List<ICommentModel>(),
                User = Convert.IsDBNull(reader["UserId"]) ? null : new UserModel(),
                Photos = Convert.IsDBNull(reader["PhotoId"]) ? null : new List<IPhotoModel>(),
            };
        }

        private ILocationModel MapLocation(NpgsqlDataReader reader)
        {
            return new LocationModel
            {
                Id = (Guid)reader["LocationId"],
                Country = !string.IsNullOrWhiteSpace(Convert.ToString(reader["Country"])) ? Convert.ToString(reader["Country"]) : null,
                City = !string.IsNullOrWhiteSpace(Convert.ToString(reader["City"])) ? Convert.ToString(reader["City"]) : null,
                Village = !string.IsNullOrWhiteSpace(Convert.ToString(reader["Village"])) ? Convert.ToString(reader["Village"]) : null,
                Address = !string.IsNullOrWhiteSpace(Convert.ToString(reader["Address"])) ? Convert.ToString(reader["Address"]) : null,
                NameOfPlace = !string.IsNullOrWhiteSpace(Convert.ToString(reader["NameOfPlace"])) ? Convert.ToString(reader["NameOfPlace"]) : null,
                IsActive = Convert.ToBoolean(reader["LocationIsActive"]),
                JambaseIdentifier = !string.IsNullOrWhiteSpace(Convert.ToString(reader["JambaseIdentifier"])) ? Convert.ToString(reader["JambaseIdentifier"]) : null
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
                UserId = (Guid)reader["CommentUserId"],
                EventId = Convert.IsDBNull(reader["CommentEventId"]) ? null : (Guid)reader["CommentEventId"],
                StoryId = Convert.IsDBNull(reader["CommentStoryId"]) ? null : (Guid)reader["CommentStoryId"],
                TouristSiteId = Convert.IsDBNull(reader["CommentTouristSitesId"]) ? null : (Guid)reader["CommentTouristSitesId"]
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
        private IUserModel MapUser(NpgsqlDataReader reader)
        {
            return new UserModel
            {
                Id = (Guid)reader["UserId"],
                Username = string.IsNullOrEmpty(reader["Username"].ToString()) ? null : Convert.ToString(reader["Username"]),
                FirstName = string.IsNullOrEmpty(reader["FirstName"].ToString()) ? null : Convert.ToString(reader["FirstName"]),
                LastName = string.IsNullOrEmpty(reader["LastName"].ToString()) ? null : Convert.ToString(reader["LastName"]),
                Email = string.IsNullOrEmpty(reader["Email"].ToString()) ? null : Convert.ToString(reader["Email"]),
                RoleId = Convert.IsDBNull(reader["UserRoleId"]) ? Guid.Empty : (Guid)reader["UserRoleId"],
                Image = string.IsNullOrEmpty(reader["Image"].ToString()) ? null : Convert.ToString(reader["Image"]),
            };
        }
    }
}
