using GeoTagMap.Models.Common;
using GeoTagMap.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using GeoTagMap.Repository.Common;
using GeoTagMap.Common.Paging;
using GeoTagMap.Common.Sorting;
using GeoTagMap.Common.Filtering;
using GeoTagMap.Common;

namespace GeoTagMap.Repository
{
    public class PerformerRepository : IPerformerRepository
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["CONNECTION_STRING"].ToString();

        public async Task<PagingInfo<IPerformerModel>> GetAllPerformersAsync(Paging paging, Sorting sort, PerformerFiltering filtering)
        {
            int totalPerformers = 0;
            Dictionary<Guid, IPerformerModel> performers = new Dictionary<Guid, IPerformerModel>();

            using (var con = new NpgsqlConnection(_connectionString))
            {
                await con.OpenAsync();

                List<Guid> performerIds = new List<Guid>();
                using (var performerIdCmd = con.CreateCommand())
                {
                    StringBuilder performerIdQuery = new StringBuilder();
                    performerIdQuery.Append("SELECT p.\"Id\" FROM \"Performer\" p WHERE p.\"IsActive\" = true");

                    if (filtering.PerformanceDate != null)
                    {
                        performerIdQuery.Append(" AND p.\"PerformanceDate\" >= @PerformanceDate");
                        performerIdCmd.Parameters.AddWithValue("@PerformanceDate", filtering.PerformanceDate.Value);
                    }
                    if (!string.IsNullOrEmpty(filtering.Name))
                    {
                        performerIdQuery.Append(" AND p.\"ArtistName\" = @Name");
                        performerIdCmd.Parameters.AddWithValue("@Name", filtering.Name);
                    }
                    if (!string.IsNullOrEmpty(filtering.BandOrMusician))
                    {
                        performerIdQuery.Append(" AND p.\"BandOrMusician\" = @BandOrMusician");
                        performerIdCmd.Parameters.AddWithValue("@BandOrMusician", filtering.BandOrMusician);
                    }
                    if (!string.IsNullOrEmpty(filtering.SearchKeyword))
                    {
                        performerIdQuery.Append(" AND p.\"ArtistName\" LIKE @SearchKeyword");
                        performerIdCmd.Parameters.AddWithValue("@SearchKeyword", "%" + filtering.SearchKeyword + "%");
                    }

                    if (!string.IsNullOrEmpty(sort.OrderBy))
                    {
                        performerIdQuery.Append($" ORDER BY p.\"{sort.OrderBy}\" {(sort.SortOrder.ToUpper() == "DESC" ? "DESC" : "ASC")}");
                    }

                    performerIdQuery.Append(" LIMIT @Limit OFFSET @Offset");
                    performerIdCmd.Parameters.AddWithValue("@Limit", paging.RRP);
                    performerIdCmd.Parameters.AddWithValue("@Offset", (paging.PageNumber - 1) * paging.RRP);

                    performerIdCmd.CommandText = performerIdQuery.ToString();

                    using (var reader = await performerIdCmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            performerIds.Add(reader.GetGuid(reader.GetOrdinal("Id")));
                        }
                    }
                }

                if (performerIds.Count == 0)
                {
                    return new PagingInfo<IPerformerModel>
                    {
                        List = new List<IPerformerModel>(),
                        RRP = paging.RRP,
                        PageNumber = paging.PageNumber,
                        TotalSize = 0
                    };
                }

                using (var countCmd = con.CreateCommand())
                {
                    StringBuilder countQuery = new StringBuilder();
                    countQuery.Append("SELECT COUNT(*) FROM \"Performer\" p WHERE p.\"IsActive\" = true");

                    if (filtering.PerformanceDate != null)
                    {
                        countQuery.Append(" AND p.\"PerformanceDate\" >= @PerformanceDate");
                        countCmd.Parameters.AddWithValue("@PerformanceDate", filtering.PerformanceDate.Value);
                    }
                    if (!string.IsNullOrEmpty(filtering.Name))
                    {
                        countQuery.Append(" AND p.\"ArtistName\" = @Name");
                        countCmd.Parameters.AddWithValue("@Name", filtering.Name);
                    }
                    if (!string.IsNullOrEmpty(filtering.BandOrMusician))
                    {
                        countQuery.Append(" AND p.\"BandOrMusician\" = @BandOrMusician");
                        countCmd.Parameters.AddWithValue("@BandOrMusician", filtering.BandOrMusician);
                    }
                    if (!string.IsNullOrEmpty(filtering.SearchKeyword))
                    {
                        countQuery.Append(" AND p.\"ArtistName\" LIKE @SearchKeyword");
                        countCmd.Parameters.AddWithValue("@SearchKeyword", "%" + filtering.SearchKeyword + "%");
                    }

                    countCmd.CommandText = countQuery.ToString();
                    totalPerformers = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
                }

                using (var cmd = con.CreateCommand())
                {
                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT p.\"Id\", p.\"ArtistName\", p.\"Image\", p.\"BandOrMusician\", p.\"NumOfUpcomingEvents\", p.\"PerformanceDate\", p.\"DateIsConfirmed\", p.\"IsActive\", p.\"JambaseIdentifier\", ");
                    query.Append("e.\"Id\" AS EventPerformerId, e.\"EventId\", e.\"PerformerId\", e.\"IsActive\" AS EventPerformerIsActive ");
                    query.Append("FROM \"Performer\" p ");
                    query.Append("LEFT JOIN \"PerformerEvent\" e ON p.\"Id\" = e.\"PerformerId\" ");
                    query.Append("WHERE p.\"Id\" = ANY(@PerformerIds)");

                    cmd.Parameters.AddWithValue("@PerformerIds", performerIds.ToArray());

                    cmd.CommandText = query.ToString();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            IPerformerModel currentPerformer;
                            Guid performerId = reader.GetGuid(reader.GetOrdinal("Id"));

                            if (!performers.TryGetValue(performerId, out currentPerformer))
                            {
                                currentPerformer = MapPerformer(reader);
                                performers.Add(performerId, currentPerformer);
                            }
                            if (reader["EventPerformerId"] != DBNull.Value)
                            {
                                if (reader["EventPerformerIsActive"] != DBNull.Value && (bool)reader["EventPerformerIsActive"])
                                {
                                    var eventPerformer = MapEventPerformer(reader);
                                    currentPerformer.EventPerformers.Add(eventPerformer);
                                }
                            }
                        }
                    }
                }
            }

            PagingInfo<IPerformerModel> pagingInfo = new PagingInfo<IPerformerModel>
            {
                List = performers.Values.ToList(),
                RRP = paging.RRP,
                PageNumber = paging.PageNumber,
                TotalSize = totalPerformers
            };
            return pagingInfo;
        }

        private string FormatString(string inputString)
        {
            string[] words = inputString.ToLower().Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (!string.IsNullOrEmpty(words[i]))
                {
                    char[] letters = words[i].ToCharArray();
                    if (char.IsLetter(letters[0]))
                        letters[0] = char.ToUpper(letters[0]);
                    for (int j = 1; j < letters.Length; j++)
                    {
                        letters[j] = char.ToLower(letters[j]);
                    }
                    words[i] = new string(letters);
                }
            }
            return string.Join(" ", words);
        }

        public async Task<IPerformerModel> GetPerformerAsync(Guid id)
        {
            IPerformerModel performer = null;

            using (var con = new NpgsqlConnection(_connectionString))
            {
                await con.OpenAsync();

                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;

                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT p.\"Id\", p.\"ArtistName\", p.\"Image\", p.\"BandOrMusician\", p.\"NumOfUpcomingEvents\", p.\"PerformanceDate\", p.\"DateIsConfirmed\", p.\"IsActive\", p.\"JambaseIdentifier\", ");
                    query.Append("e.\"Id\" AS EventPerformerId, e.\"EventId\", e.\"PerformerId\", e.\"IsActive\" AS EventPerformerIsActive ");
                    query.Append("FROM \"Performer\" p ");
                    query.Append("LEFT JOIN \"PerformerEvent\" e ON p.\"Id\" = e.\"PerformerId\" ");
                    query.Append(" WHERE p.\"Id\" = @performerId ");
                    query.Append("AND p.\"IsActive\" = true");

                    cmd.Parameters.AddWithValue("@performerId", id);
                    cmd.CommandText= query.ToString();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            if (performer == null)
                            {
                                performer = MapPerformer(reader);
                            }
                            if (reader["EventPerformerId"] != DBNull.Value)
                            {
                                if (reader["EventPerformerIsActive"] != DBNull.Value && (bool)reader["EventPerformerIsActive"])
                                {
                                    var eventPerformer = MapEventPerformer(reader);
                                    performer.EventPerformers.Add(eventPerformer);
                                }
                            }
                        }
                    }
                }
            }

            return performer;
        }

        public async Task AddPerformerAsync(IPerformerModel performerModel)
        {
            using (var con = new NpgsqlConnection(_connectionString))
            {
                await con.OpenAsync();

                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;
                    StringBuilder query = new StringBuilder();
                    query.Append("INSERT INTO \"Performer\" (\"Id\", \"ArtistName\", \"Image\", \"BandOrMusician\", \"NumOfUpcomingEvents\", \"PerformanceDate\", \"DateIsConfirmed\", \"DateCreated\", \"DateUpdated\", \"CreatedBy\", \"UpdatedBy\", \"IsActive\", \"JambaseIdentifier\") ");
                    query.Append("VALUES (@id, @name, @image, @bandOrMusician, @numOfUpcomingEvents, @performanceDate, @dateIsConfirmed, @dateCreated, @dateUpdated, @createdBy, @updatedBy, @isActive, @jambaseIdentifier)");
     
                    cmd.CommandText = query.ToString();

                    cmd.Parameters.AddWithValue("@id", performerModel.Id);
                    cmd.Parameters.AddWithValue("@name", performerModel.Name ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@image", performerModel.Image ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@bandOrMusician", performerModel.BandOrMusician ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@numOfUpcomingEvents", performerModel.NumOfUpcomingEvents ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@performanceDate", performerModel.PerformanceDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@dateIsConfirmed", performerModel.DateIsConfirmed ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@dateCreated", performerModel.DateCreated);
                    cmd.Parameters.AddWithValue("@dateUpdated", performerModel.DateUpdated);
                    cmd.Parameters.AddWithValue("@createdBy", performerModel.CreatedBy);
                    cmd.Parameters.AddWithValue("@updatedBy", performerModel.UpdatedBy);
                    cmd.Parameters.AddWithValue("@isActive", performerModel.IsActive);
                    cmd.Parameters.AddWithValue("@jambaseIdentifier", performerModel.JambaseIdentifier ?? (object)DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdatePerformerAsync(Guid id, IPerformerModel performerData)
        {
            using (var con = new NpgsqlConnection(_connectionString))
            {
                await con.OpenAsync();

                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;

                    StringBuilder query = new StringBuilder();
                    query.Append("UPDATE \"Performer\" SET ");

                    if (!string.IsNullOrEmpty(performerData.Name))
                    {
                        query.Append("\"ArtistName\" = @name, ");
                        cmd.Parameters.AddWithValue("@name", performerData.Name);
                    }

                    if (!string.IsNullOrEmpty(performerData.Image))
                    {
                        query.Append("\"Image\" = @image, ");
                        cmd.Parameters.AddWithValue("@image", performerData.Image);
                    }

                    if (!string.IsNullOrEmpty(performerData.BandOrMusician))
                    {
                        query.Append("\"BandOrMusician\" = @bandOrMusician, ");
                        cmd.Parameters.AddWithValue("@bandOrMusician", performerData.BandOrMusician);
                    }

                    if (performerData.NumOfUpcomingEvents != null)
                    {
                        query.Append("\"NumOfUpcomingEvents\" = @numOfUpcomingEvents, ");
                        cmd.Parameters.AddWithValue("@numOfUpcomingEvents", performerData.NumOfUpcomingEvents);
                    }

                    if (performerData.PerformanceDate != null)
                    {
                        query.Append("\"PerformanceDate\" = @performanceDate, ");
                        cmd.Parameters.AddWithValue("@performanceDate", performerData.PerformanceDate);
                    }

                    if (performerData.DateIsConfirmed.HasValue)
                    {
                        query.Append("\"DateIsConfirmed\" = @dateIsConfirmed, ");
                        cmd.Parameters.AddWithValue("@dateIsConfirmed", performerData.DateIsConfirmed);
                    }

                    if (performerData.UpdatedBy != Guid.Empty)
                    {
                        query.Append("\"UpdatedBy\" = @updatedBy, ");
                        cmd.Parameters.AddWithValue("@updatedBy", performerData.UpdatedBy);
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

        public async Task<IPerformerModel> GetPerformerByJambaseIdentifierAsync(string jambaseIdentifier)
        {
            IPerformerModel performer = null;
            using(var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;

                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT p.\"Id\", p.\"ArtistName\", p.\"Image\", p.\"BandOrMusician\", p.\"NumOfUpcomingEvents\", p.\"PerformanceDate\", p.\"DateIsConfirmed\", p.\"IsActive\", p.\"JambaseIdentifier\", ");
                    query.Append("e.\"Id\" AS EventPerformerId, e.\"EventId\", e.\"PerformerId\" ");
                    query.Append("FROM \"Performer\" p ");
                    query.Append("LEFT JOIN \"PerformerEvent\" e ON p.\"Id\" = e.\"PerformerId\" ");
                    query.Append("WHERE p.\"JambaseIdentifier\" = @jambaseIdentifier ");
                    query.Append("AND p.\"IsActive\" = true");

                    cmd.Parameters.AddWithValue("@jambaseIdentifier", jambaseIdentifier);
                    cmd.CommandText = query.ToString();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            if (performer == null)
                            {
                                performer = MapPerformer(reader);
                            }
                            if (reader["EventPerformerId"] != DBNull.Value)
                            {
                                var eventPerformer = MapEventPerformer(reader);
                                performer.EventPerformers.Add(eventPerformer);
                            }
                        }
                    }
                }
            }
            return performer;
        }

        public async Task DeletePerformerAsync(Guid id)
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
                        var queryPerformer = new StringBuilder();
                        queryPerformer.Append("UPDATE \"Performer\" SET \"IsActive\" = false WHERE \"Id\" = @id");
                        cmd.CommandText = queryPerformer.ToString();
                        await cmd.ExecuteNonQueryAsync();

                        var queryEventPerformer = new StringBuilder();
                        queryEventPerformer.Append("UPDATE \"PerformerEvent\" SET \"IsActive\" = false WHERE \"PerformerId\" = @id");
                        cmd.CommandText = queryEventPerformer.ToString();
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

        private IPerformerModel MapPerformer(NpgsqlDataReader reader)
        {
            return new PerformerModel
            {
                Id = (Guid)reader["Id"],    
                Name = !string.IsNullOrWhiteSpace(Convert.ToString(reader["ArtistName"])) ? Convert.ToString(reader["ArtistName"]) : null,
                Image = !string.IsNullOrWhiteSpace(Convert.ToString(reader["Image"])) ? Convert.ToString(reader["Image"]) : null,
                BandOrMusician = !string.IsNullOrWhiteSpace(Convert.ToString(reader["BandOrMusician"])) ? Convert.ToString(reader["BandOrMusician"]) : null,
                NumOfUpcomingEvents = reader["NumOfUpcomingEvents"] != DBNull.Value ? Convert.ToInt32(reader["NumOfUpcomingEvents"]) : null,
                PerformanceDate = reader["PerformanceDate"] != DBNull.Value ? Convert.ToDateTime(reader["PerformanceDate"]) : null,
                DateIsConfirmed = reader["DateIsConfirmed"] != DBNull.Value ? (bool?)Convert.ToBoolean(reader["DateIsConfirmed"]) : null,
                IsActive = reader["IsActive"] != DBNull.Value ? (bool?)Convert.ToBoolean(reader["IsActive"]) : null,
                JambaseIdentifier = !string.IsNullOrWhiteSpace(Convert.ToString(reader["JambaseIdentifier"])) ? Convert.ToString(reader["JambaseIdentifier"]) : null,

                EventPerformers = Convert.IsDBNull(reader["EventPerformerId"]) ? null : new List<IEventPerformerModel>()
            };
        }
        private IEventPerformerModel MapEventPerformer(NpgsqlDataReader reader)
        {
            return new EventPerformerModel
            {
                Id = (Guid)reader["EventPerformerId"],
                EventId = (Guid)reader["EventId"],
                PerformerId = (Guid)reader["PerformerId"],
            };
        }
    }
}
