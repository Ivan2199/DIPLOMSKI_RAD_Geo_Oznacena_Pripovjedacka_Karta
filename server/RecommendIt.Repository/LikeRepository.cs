using GeoTagMap.Models;
using GeoTagMap.Models.Common;
using GeoTagMap.Repository.Common;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoTagMap.Repository
{
    public class LikeRepository : ILikeRepository
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["CONNECTION_STRING"].ToString();

        public async Task<List<ILikeModel>> GetAllLikesAsync()
        {
            Dictionary<Guid, ILikeModel> likes = new Dictionary<Guid, ILikeModel>();
            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;
                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT l.\"Id\", l.\"IsLike\", l.\"CommentId\", l.\"EventId\", l.\"TouristSiteId\", l.\"StoryId\", l.\"UserId\", l.\"DateCreated\" ");
                    query.Append("FROM \"Like\" l ");
                    query.Append("WHERE l.\"IsActive\" = true");

                    cmd.CommandText = query.ToString();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ILikeModel currentLike;
                            Guid likeId = reader.GetGuid(reader.GetOrdinal("Id"));

                            if (!likes.TryGetValue(likeId, out currentLike))
                            {
                                currentLike = MapLike(reader);
                                likes.Add(likeId, currentLike);
                            }
                        }
                    }
                }
            }
            return likes.Values.ToList();
        }
        public async Task<ILikeModel> GetLikeAsync(Guid id)
        {
            ILikeModel like = null;
            using (var con = new NpgsqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;
                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT l.\"Id\", l.\"IsLike\", l.\"CommentId\", l.\"EventId\", l.\"TouristSiteId\", l.\"StoryId\", l.\"UserId\", l.\"DateCreated\" ");
                    query.Append("FROM \"Like\" l ");
                    query.Append("WHERE l.\"IsActive\" = true ");
                    query.Append("AND l.\"Id\" = @likeId");


                    cmd.Parameters.AddWithValue("@likeId", id);
                    cmd.CommandText = query.ToString();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (like == null)
                            {
                                like = MapLike(reader);
                            }
                        }
                    }
                }
            }
            return like;
        }
        public async Task<ILikeModel> GetLikeByCommentAndUserIdAsync(Guid commentId, Guid userId)
        {
            ILikeModel like = null;
            using (var con = new NpgsqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("SELECT l.\"Id\", l.\"IsLike\", l.\"CommentId\", l.\"EventId\", l.\"TouristSiteId\", l.\"StoryId\", l.\"UserId\", l.\"DateCreated\" ");
                    query.AppendLine("FROM \"Like\" l ");
                    query.AppendLine("WHERE l.\"IsActive\" = true ");
                    query.AppendLine("AND l.\"UserId\" = @userId ");
                    query.AppendLine("AND l.\"CommentId\" = @commentId");

                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@commentId", commentId);
                    cmd.CommandText = query.ToString();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (like == null)
                            {
                                like = MapLike(reader);
                            }
                        }
                    }
                }
            }
            return like;
        }

        public async Task<ILikeModel> GetLikeByEventAndUserIdAsync(Guid eventId, Guid userId)
        {
            ILikeModel like = null;
            using (var con = new NpgsqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;
                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT \"Id\", \"IsLike\", \"CommentId\", \"EventId\", \"TouristSiteId\", \"StoryId\", \"UserId\", \"DateCreated\" ");
                    query.Append("FROM \"Like\" ");
                    query.Append("WHERE \"IsActive\" = true ");
                    query.Append("AND \"UserId\" = @userId ");
                    query.Append("AND \"EventId\" = @eventId");

                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@eventId", eventId);
                    cmd.CommandText = query.ToString();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (like == null)
                            {
                                like = MapLike(reader);
                            }
                        }
                    }
                }
            }
            return like;
        }
        public async Task<ILikeModel> GetLikeByTouristSiteAndUserIdAsync(Guid touristSiteId, Guid userId)
        {
            ILikeModel like = null;
            using (var con = new NpgsqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;
                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT \"Id\", \"IsLike\", \"CommentId\", \"EventId\", \"TouristSiteId\", \"StoryId\", \"UserId\", \"DateCreated\" ");
                    query.Append("FROM \"Like\" ");
                    query.Append("WHERE \"IsActive\" = true ");
                    query.Append("AND \"UserId\" = @userId ");
                    query.Append("AND \"TouristSiteId\" = @touristSiteId");


                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@touristSiteId", touristSiteId);
                    cmd.CommandText = query.ToString();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (like == null)
                            {
                                like = MapLike(reader);
                            }
                        }
                    }
                }
            }
            return like;
        }
        public async Task<ILikeModel> GetLikeByStoryAndUserIdAsync(Guid storyId, Guid userId)
        {
            ILikeModel like = null;
            using (var con = new NpgsqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using (var cmd = con.CreateCommand())
                {
                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT \"Id\", \"IsLike\", \"CommentId\", \"EventId\", \"TouristSiteId\", \"StoryId\", \"UserId\", \"DateCreated\" ");
                    query.Append("FROM \"Like\" ");
                    query.Append("WHERE \"IsActive\" = true ");
                    query.Append("AND \"UserId\" = @userId ");
                    query.Append("AND \"StoryId\" = @storyId");

                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@storyId", storyId);
                    cmd.CommandText = query.ToString();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (like == null)
                            {
                                like = MapLike(reader);
                            }
                        }
                    }
                }
            }
            return like;
        }

        public async Task<List<ILikeModel>> GetAllLikesByCommentIdAsync(Guid commentId)
        {
            Dictionary<Guid, ILikeModel> likes = new Dictionary<Guid, ILikeModel>();
            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;
                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT l.\"Id\", l.\"IsLike\", l.\"CommentId\", l.\"EventId\", l.\"TouristSiteId\", l.\"StoryId\", l.\"UserId\", l.\"DateCreated\" ");
                    query.Append("FROM \"Like\" l ");
                    query.Append("WHERE l.\"IsActive\" = true ");
                    query.Append("AND l.\"CommentId\" = @commentId");


                    cmd.Parameters.AddWithValue("@commentId", commentId);
                    cmd.CommandText = query.ToString();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ILikeModel currentLike;
                            Guid likeId = reader.GetGuid(reader.GetOrdinal("Id"));

                            if (!likes.TryGetValue(likeId, out currentLike))
                            {
                                currentLike = MapLike(reader);
                                likes.Add(likeId, currentLike);
                            }
                        }
                    }
                }
            }
            return likes.Values.ToList();
        }
        public async Task<List<ILikeModel>> GetAllLikesByUserIdAsync(Guid userId, string likesOf)
        {
            Dictionary<Guid, ILikeModel> likes = new Dictionary<Guid, ILikeModel>();
            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;
                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT l.\"Id\", l.\"IsLike\", l.\"CommentId\", l.\"EventId\", l.\"TouristSiteId\", l.\"StoryId\", l.\"UserId\", l.\"DateCreated\" ");
                    query.Append("FROM \"Like\" l ");
                    query.Append("WHERE l.\"IsActive\" = true ");
                    query.Append("AND l.\"UserId\" = @userId ");
                    if(likesOf == "")
                    {
                        return null;
                    }
                    else if(likesOf == "story")
                    {
                        query.Append("AND l.\"StoryId\" IS NOT NULL");
                    }
                    else if(likesOf == "event")
                    {
                        query.Append("AND l.\"EventId\" IS NOT NULL");
                    }
                    else if(likesOf == "site")
                    {
                        query.Append("AND l.\"TouristSiteId\" IS NOT NULL");
                    }

                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.CommandText = query.ToString();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ILikeModel currentLike;
                            Guid likeId = reader.GetGuid(reader.GetOrdinal("Id"));

                            if (!likes.TryGetValue(likeId, out currentLike))
                            {
                                currentLike = MapLike(reader);
                                likes.Add(likeId, currentLike);
                            }
                        }
                    }
                }
            }
            return likes.Values.ToList();
        }
        public async Task<List<ILikeModel>> GetAllLikesByEventIdAsync(Guid eventId)
        {
            Dictionary<Guid, ILikeModel> likes = new Dictionary<Guid, ILikeModel>();
            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;
                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT l.\"Id\", l.\"IsLike\", l.\"CommentId\", l.\"EventId\", l.\"TouristSiteId\", l.\"StoryId\", l.\"UserId\", l.\"DateCreated\" ");
                    query.Append("FROM \"Like\" l ");
                    query.Append("WHERE l.\"IsActive\" = true ");
                    query.Append("AND l.\"CommentId\" = @eventId");


                    cmd.Parameters.AddWithValue("@eventId", eventId);
                    cmd.CommandText = query.ToString();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ILikeModel currentLike;
                            Guid likeId = reader.GetGuid(reader.GetOrdinal("Id"));

                            if (!likes.TryGetValue(likeId, out currentLike))
                            {
                                currentLike = MapLike(reader);
                                likes.Add(likeId, currentLike);
                            }
                        }
                    }
                }
            }
            return likes.Values.ToList();
        }
        public async Task<List<ILikeModel>> GetAllLikesByStoryIdAsync(Guid storyId)
        {
            Dictionary<Guid, ILikeModel> likes = new Dictionary<Guid, ILikeModel>();
            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;
                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT l.\"Id\", l.\"IsLike\", l.\"CommentId\", l.\"EventId\", l.\"TouristSiteId\", l.\"StoryId\", l.\"UserId\", l.\"DateCreated\" ");
                    query.Append("FROM \"Like\" l ");
                    query.Append("WHERE l.\"IsActive\" = true ");
                    query.Append("AND l.\"CommentId\" = @storyId");


                    cmd.Parameters.AddWithValue("@storyId", storyId);
                    cmd.CommandText = query.ToString();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ILikeModel currentLike;
                            Guid likeId = reader.GetGuid(reader.GetOrdinal("Id"));

                            if (!likes.TryGetValue(likeId, out currentLike))
                            {
                                currentLike = MapLike(reader);
                                likes.Add(likeId, currentLike);
                            }
                        }
                    }
                }
            }
            return likes.Values.ToList();
        }
        public async Task<List<ILikeModel>> GetAllLikesByTouristSiteIdAsync(Guid touristSiteId)
        {
            Dictionary<Guid, ILikeModel> likes = new Dictionary<Guid, ILikeModel>();
            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;
                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT l.\"Id\", l.\"IsLike\", l.\"CommentId\", l.\"EventId\", l.\"TouristSiteId\", l.\"StoryId\", l.\"UserId\", l.\"DateCreated\" ");
                    query.Append("FROM \"Like\" l ");
                    query.Append("WHERE l.\"IsActive\" = true ");
                    query.Append("AND l.\"CommentId\" = @touristSite");


                    cmd.Parameters.AddWithValue("@touristSite", touristSiteId);
                    cmd.CommandText = query.ToString();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ILikeModel currentLike;
                            Guid likeId = reader.GetGuid(reader.GetOrdinal("Id"));

                            if (!likes.TryGetValue(likeId, out currentLike))
                            {
                                currentLike = MapLike(reader);
                                likes.Add(likeId, currentLike);
                            }
                        }
                    }
                }
            }
            return likes.Values.ToList();
        }
        public async Task AddLikeAsync(ILikeModel like)
        {
            using (var con = new NpgsqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;

                    StringBuilder query = new StringBuilder();
                    query.Append("INSERT INTO \"Like\" (\"Id\", \"IsLike\" , \"CommentId\", \"EventId\", \"StoryId\", \"UserId\", \"TouristSiteId\", \"DateCreated\", \"DateUpdated\", \"CreatedBy\", \"UpdatedBy\", \"IsActive\") ");
                    query.Append("VALUES (@id, @isLike, @commentId, @eventId, @storyId, @userId, @touristSiteId, @dateCreated, @dateUpdated, @createdBy, @updatedBy, @isActive)");

                    cmd.CommandText = query.ToString();

                    Guid randid = Guid.NewGuid();
                    cmd.Parameters.AddWithValue("@id", randid);
                    cmd.Parameters.AddWithValue("@isLike", like.IsLike);
                    cmd.Parameters.AddWithValue("@commentId", like.CommentId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@eventId", like.EventId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@storyId", like.StoryId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@touristSiteId", like.TouristSiteId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@userId", like.UserId);
                    cmd.Parameters.AddWithValue("@dateCreated", like.DateCreated);
                    cmd.Parameters.AddWithValue("@dateUpdated", like.DateUpdated);
                    cmd.Parameters.AddWithValue("@updatedBy", like.UpdatedBy);
                    cmd.Parameters.AddWithValue("@createdBy", like.CreatedBy);
                    cmd.Parameters.AddWithValue("@isActive", like.IsActive);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task DeleteLikeAsync(Guid id)
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
                        StringBuilder queryCategory = new StringBuilder();
                        queryCategory.Append("UPDATE \"Like\" SET \"IsActive\" = false WHERE \"Id\" = @id");
                        cmd.CommandText = queryCategory.ToString();
                        await cmd.ExecuteNonQueryAsync();

                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }
        private ILikeModel MapLike(NpgsqlDataReader reader)
        {
            return new LikeModel
            {
                Id = (Guid)reader["Id"],
                CommentId = Convert.IsDBNull(reader["CommentId"]) ? null : (Guid)reader["CommentId"],
                EventId = Convert.IsDBNull(reader["EventId"]) ? null : (Guid)reader["EventId"],
                TouristSiteId = Convert.IsDBNull(reader["TouristSiteId"]) ? null : (Guid)reader["TouristSiteId"],
                StoryId = Convert.IsDBNull(reader["StoryId"]) ? null : (Guid)reader["StoryId"],
                UserId = (Guid)reader["UserId"],
                DateCreated = Convert.ToDateTime(reader["DateCreated"]),
            };
        }
    }
}
