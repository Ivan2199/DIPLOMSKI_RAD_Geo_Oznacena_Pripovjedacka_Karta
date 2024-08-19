using GeoTagMap.Models.Common;
using GeoTagMap.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoTagMap.Repository.Common;
using NpgsqlTypes;
using GeoTagMap.Common.Filtering;
using GeoTagMap.Common.Paging;
using GeoTagMap.Common.Sorting;
using GeoTagMap.Common;
using System.Web.Http.Filters;

namespace GeoTagMap.Repository
{
    public class LocationRepository : ILocationRepository
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["CONNECTION_STRING"].ToString();

        public async Task<PagingInfo<ILocationModel>> GetAllLocationsAsync(Paging paging, Sorting sort, LocationFiltering filtering)
        {
            int totalLocations = 0;
            Dictionary<Guid, ILocationModel> locations = new Dictionary<Guid, ILocationModel>();

            using (var con = new NpgsqlConnection(_connectionString))
            {
                await con.OpenAsync();

                List<Guid> locationIds = new List<Guid>();
                using (var locationIdCmd = con.CreateCommand())
                {
                    StringBuilder locationIdQuery = new StringBuilder();
                    locationIdQuery.Append("SELECT l.\"Id\" FROM \"Location\" l WHERE l.\"IsActive\" = true");

                    if (!string.IsNullOrEmpty(filtering.Country))
                    {
                        locationIdQuery.Append(" AND l.\"Country\" = @Country");
                        locationIdCmd.Parameters.AddWithValue("@Country", filtering.Country);
                    }
                    if (!string.IsNullOrEmpty(filtering.City))
                    {
                        locationIdQuery.Append(" AND l.\"City\" = @City");
                        locationIdCmd.Parameters.AddWithValue("@City", filtering.City);
                    }
                    if (!string.IsNullOrEmpty(filtering.Village))
                    {
                        locationIdQuery.Append(" AND l.\"Village\" = @Village");
                        locationIdCmd.Parameters.AddWithValue("@Village", filtering.Village);
                    }
                    if (!string.IsNullOrEmpty(filtering.Address))
                    {
                        locationIdQuery.Append(" AND l.\"Address\" = @Address");
                        locationIdCmd.Parameters.AddWithValue("@Address", filtering.Address);
                    }
                    if (!string.IsNullOrEmpty(filtering.NameOfPlace))
                    {
                        locationIdQuery.Append(" AND l.\"NameOfPlace\" = @NameOfPlace");
                        locationIdCmd.Parameters.AddWithValue("@NameOfPlace", filtering.NameOfPlace);
                    }
                    if (!string.IsNullOrEmpty(filtering.SearchKeyword))
                    {
                        locationIdQuery.Append(" AND (l.\"Country\" LIKE @SearchKeyword OR l.\"City\" LIKE @SearchKeyword OR l.\"Village\" LIKE @SearchKeyword OR l.\"Address\" LIKE @SearchKeyword OR l.\"NameOfPlace\" LIKE @SearchKeyword)");
                        locationIdCmd.Parameters.AddWithValue("@SearchKeyword", "%" + filtering.SearchKeyword + "%");
                    }

                    if (!string.IsNullOrEmpty(sort.OrderBy))
                    {
                        locationIdQuery.Append($" ORDER BY l.\"{sort.OrderBy}\" {(sort.SortOrder.ToUpper() == "DESC" ? "DESC" : "ASC")}");
                    }

                    locationIdQuery.Append(" LIMIT @Limit OFFSET @Offset");
                    locationIdCmd.Parameters.AddWithValue("@Limit", paging.RRP);
                    locationIdCmd.Parameters.AddWithValue("@Offset", (paging.PageNumber - 1) * paging.RRP);

                    locationIdCmd.CommandText = locationIdQuery.ToString();

                    using (var reader = await locationIdCmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            locationIds.Add(reader.GetGuid(reader.GetOrdinal("Id")));
                        }
                    }
                }

                if (locationIds.Count == 0)
                {
                    return new PagingInfo<ILocationModel>
                    {
                        List = new List<ILocationModel>(),
                        RRP = paging.RRP,
                        PageNumber = paging.PageNumber,
                        TotalSize = 0
                    };
                }

                using (var countCmd = con.CreateCommand())
                {
                    StringBuilder countQuery = new StringBuilder();
                    countQuery.Append("SELECT COUNT(*) FROM \"Location\" l WHERE l.\"IsActive\" = true");

                    if (!string.IsNullOrEmpty(filtering.Country))
                    {
                        countQuery.Append(" AND l.\"Country\" = @Country");
                        countCmd.Parameters.AddWithValue("@Country", filtering.Country);
                    }
                    if (!string.IsNullOrEmpty(filtering.City))
                    {
                        countQuery.Append(" AND l.\"City\" = @City");
                        countCmd.Parameters.AddWithValue("@City", filtering.City);
                    }
                    if (!string.IsNullOrEmpty(filtering.Village))
                    {
                        countQuery.Append(" AND l.\"Village\" = @Village");
                        countCmd.Parameters.AddWithValue("@Village", filtering.Village);
                    }
                    if (!string.IsNullOrEmpty(filtering.Address))
                    {
                        countQuery.Append(" AND l.\"Address\" = @Address");
                        countCmd.Parameters.AddWithValue("@Address", filtering.Address);
                    }
                    if (!string.IsNullOrEmpty(filtering.NameOfPlace))
                    {
                        countQuery.Append(" AND l.\"NameOfPlace\" = @NameOfPlace");
                        countCmd.Parameters.AddWithValue("@NameOfPlace", filtering.NameOfPlace);
                    }
                    if (!string.IsNullOrEmpty(filtering.SearchKeyword))
                    {
                        countQuery.Append(" AND (l.\"Country\" LIKE @SearchKeyword OR l.\"City\" LIKE @SearchKeyword OR l.\"Village\" LIKE @SearchKeyword OR l.\"Address\" LIKE @SearchKeyword OR l.\"NameOfPlace\" LIKE @SearchKeyword)");
                        countCmd.Parameters.AddWithValue("@SearchKeyword", "%" + filtering.SearchKeyword + "%");
                    }

                    countCmd.CommandText = countQuery.ToString();
                    totalLocations = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
                }

                using (var cmd = con.CreateCommand())
                {
                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT l.\"Id\", l.\"Country\", l.\"City\", l.\"Village\", l.\"Address\", l.\"NameOfPlace\", l.\"IsActive\", l.\"JambaseIdentifier\", ");
                    query.Append("g.\"Id\" AS GeoLocationId, g.\"Latitude\", g.\"Longitude\", g.\"LocationId\", g.\"IsActive\" AS GeoLocationIsActive, ");
                    query.Append("e.\"Id\" AS EventId, e.\"Name\", e.\"Url\", e.\"IsActive\" AS EventIsActive, e.\"EventStatus\", e.\"Image\", e.\"StartDate\", e.\"EndDate\", e.\"IsAccessibleForFree\", e.\"Type\", e.\"TicketInformationId\" AS EventTicketInformationId, e.\"LocationId\" AS EventLocationId, ");
                    query.Append("s.\"Id\" AS StoryId, s.\"StoryText\", s.\"Date\", s.\"LocationId\" AS StoryLocationId, s.\"UserId\" AS StoryUserId, s.\"IsActive\" AS StoryIsActive, ");
                    query.Append("t.\"Id\" AS TouristSitesId, t.\"SiteName\", t.\"Link\", t.\"LocationId\" AS TouristSiteLocationId, t.\"IsActive\" AS TouristSiteIsActive, t.\"Fsq_Id\", t.\"Popularity\", t.\"Rating\", t.\"Description\", t.\"WebsiteUrl\", t.\"Email\", t.\"HoursOpen\", t.\"FacebookId\", t.\"Instagram\", t.\"Twitter\" ");
                    query.Append("FROM \"Location\" l ");
                    query.Append("LEFT JOIN \"GeoLocation\" g ON l.\"Id\" = g.\"LocationId\" ");
                    query.Append("LEFT JOIN \"Event\" e ON l.\"Id\" = e.\"LocationId\" ");
                    query.Append("LEFT JOIN \"Story\" s ON l.\"Id\" = s.\"LocationId\" ");
                    query.Append("LEFT JOIN \"TouristSites\" t ON l.\"Id\" = t.\"LocationId\" ");
                    query.Append("WHERE l.\"Id\" = ANY(@LocationIds)");

                    cmd.Parameters.AddWithValue("@LocationIds", locationIds.ToArray());

                    cmd.CommandText = query.ToString();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ILocationModel currentLocation;
                            Guid locationId = reader.GetGuid(reader.GetOrdinal("Id"));

                            if (!locations.TryGetValue(locationId, out currentLocation))
                            {
                                currentLocation = MapLocation(reader);
                                locations.Add(locationId, currentLocation);
                            }
                            if (reader["GeoLocationId"] != DBNull.Value)
                            {
                                if (reader["GeoLocationIsActive"] != DBNull.Value && (bool)reader["GeoLocationIsActive"])
                                {
                                    var geoLocation = MapGeoLocation(reader);
                                    currentLocation.GeoLocations.Add(geoLocation);
                                }
                            }
                            if (reader["EventId"] != DBNull.Value)
                            {
                                if (reader["EventIsActive"] != DBNull.Value && (bool)reader["EventIsActive"])
                                {
                                    IEventModel eventModel = MapEvent(reader);
                                    currentLocation.Events.Add(eventModel);
                                }
                            }
                            if (reader["StoryId"] != DBNull.Value)
                            {
                                if (reader["StoryIsActive"] != DBNull.Value && (bool)reader["StoryIsActive"])
                                {
                                    IStoryModel story = MapStory(reader);
                                    currentLocation.Stories.Add(story);
                                }
                            }
                            if (reader["TouristSitesId"] != DBNull.Value)
                            {
                                if (reader["TouristSiteIsActive"] != DBNull.Value && (bool)reader["TouristSiteIsActive"])
                                {
                                    ITouristSitesModel touristSite = MapTouristSite(reader);
                                    currentLocation.Sites.Add(touristSite);
                                }
                            }
                        }
                    }
                }
            }

            PagingInfo<ILocationModel> pagingInfo = new PagingInfo<ILocationModel>
            {
                List = locations.Values.ToList(),
                RRP = paging.RRP,
                PageNumber = paging.PageNumber,
                TotalSize = totalLocations
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


        public async Task<ILocationModel> GetLocationAsync(Guid id)
        {
            ILocationModel location = null;
            using (var con = new NpgsqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;

                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT l.\"Id\", l.\"Country\", l.\"City\", l.\"Village\", l.\"Address\", l.\"NameOfPlace\", l.\"IsActive\", l.\"JambaseIdentifier\", ");
                    query.Append("g.\"Id\" AS GeoLocationId, g.\"Latitude\", g.\"Longitude\", g.\"LocationId\", g.\"IsActive\" AS GeoLocationIsActive, ");
                    query.Append("e.\"Id\" AS EventId, e.\"Name\", e.\"Url\", e.\"IsActive\" AS EventIsActive, e.\"EventStatus\", e.\"Image\", e.\"StartDate\", e.\"EndDate\", e.\"IsAccessibleForFree\", e.\"Type\", e.\"TicketInformationId\" AS EventTicketInformationId, e.\"LocationId\" AS EventLocationId, ");
                    query.Append("s.\"Id\" AS StoryId, s.\"StoryText\", s.\"Date\", s.\"LocationId\" AS StoryLocationId, s.\"UserId\" AS StoryUserId, s.\"IsActive\" AS StoryIsActive, ");
                    query.Append("t.\"Id\" AS TouristSitesId, t.\"SiteName\", t.\"Link\", t.\"LocationId\" AS TouristSiteLocationId, t.\"IsActive\" AS TouristSiteIsActive, t.\"Fsq_Id\", t.\"Popularity\", t.\"Rating\", t.\"Description\", t.\"WebsiteUrl\", t.\"Email\", t.\"HoursOpen\", t.\"FacebookId\", t.\"Instagram\", t.\"Twitter\" ");
                    query.Append("FROM \"Location\" l ");
                    query.Append("LEFT JOIN \"GeoLocation\" g ON l.\"Id\" = g.\"LocationId\" ");
                    query.Append("LEFT JOIN \"Event\" e ON l.\"Id\" = e.\"LocationId\" ");
                    query.Append("LEFT JOIN \"Story\" s ON l.\"Id\" = s.\"LocationId\" ");
                    query.Append("LEFT JOIN \"TouristSites\" t ON l.\"Id\" = t.\"LocationId\" ");
                    query.Append("WHERE l.\"Id\" = @locationId ");
                    query.Append("AND l.\"IsActive\" = true");


                    cmd.Parameters.AddWithValue("@locationId", id);
                    cmd.CommandText = query.ToString();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (location == null)
                            {
                                location = MapLocation(reader);
                            }
                            if (reader["GeoLocationId"] != DBNull.Value)
                            {
                                if (reader["GeoLocationIsActive"] != DBNull.Value && (bool)reader["GeoLocationIsActive"])
                                {
                                    var geoLocation = MapGeoLocation(reader);
                                    location.GeoLocations.Add(geoLocation);
                                }
                            }
                            if (reader["EventId"] != DBNull.Value)
                            {
                                if (reader["EventIsActive"] != DBNull.Value && (bool)reader["EventIsActive"])
                                {
                                    IEventModel eventModel = MapEvent(reader);
                                    location.Events.Add(eventModel);
                                }
                            }
                            if (reader["StoryId"] != DBNull.Value)
                            {
                                if (reader["StoryIsActive"] != DBNull.Value && (bool)reader["StoryIsActive"])
                                {
                                    IStoryModel story = MapStory(reader);
                                    location.Stories.Add(story);
                                }
                            }
                            if (reader["TouristSitesId"] != DBNull.Value)
                            {
                                if (reader["TouristSiteIsActive"] != DBNull.Value && (bool)reader["TouristSiteIsActive"])
                                {
                                    ITouristSitesModel touristSite = MapTouristSite(reader);
                                    location.Sites.Add(touristSite);
                                }
                            }
                        }
                    }
                }
            }
            return location;
        }

        public async Task AddLocationAsync(ILocationModel location)
        {
            using (var con = new NpgsqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;

                    StringBuilder query = new StringBuilder();
                    query.Append("INSERT INTO \"Location\" (\"Id\", \"Country\", \"City\", \"Village\", \"Address\", \"NameOfPlace\", \"DateCreated\", \"DateUpdated\", \"CreatedBy\", \"UpdatedBy\", \"IsActive\", \"JambaseIdentifier\") ");
                    query.Append("VALUES (@id, @country, @city, @village, @address, @nameOfPlace, @dateCreated, @dateUpdated, @createdBy, @updatedBy, @isActive, @jambaseIdentifier)");

                    cmd.CommandText = query.ToString();

                    cmd.Parameters.AddWithValue("@id", location.Id);
                    cmd.Parameters.AddWithValue("@country", location.Country ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@city", location.City ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@village", location.Village ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@address", location.Address ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@nameOfPlace", location.NameOfPlace ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@dateCreated", location.DateCreated);
                    cmd.Parameters.AddWithValue("@dateUpdated", location.DateUpdated);
                    cmd.Parameters.AddWithValue("@updatedBy", location.UpdatedBy);
                    cmd.Parameters.AddWithValue("@createdBy", location.CreatedBy);
                    cmd.Parameters.AddWithValue("@isActive", location.IsActive);
                    cmd.Parameters.AddWithValue("@jambaseIdentifier", NpgsqlDbType.Text, location.JambaseIdentifier ?? (object)DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdateLocationAsync(Guid id, ILocationModel locationData)
        {
            using (var con = new NpgsqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;

                    StringBuilder query = new StringBuilder();
                    query.Append("UPDATE \"Location\" SET ");

                    if (!string.IsNullOrEmpty(locationData.Country))
                    {
                        query.Append("\"Country\" = @country, ");
                        cmd.Parameters.AddWithValue("@country", locationData.Country);
                    }

                    if (!string.IsNullOrEmpty(locationData.City))
                    {
                        query.Append("\"City\" = @city, ");
                        cmd.Parameters.AddWithValue("@city", locationData.City);
                    }

                    if (!string.IsNullOrEmpty(locationData.Village))
                    {
                        query.Append("\"Village\" = @village, ");
                        cmd.Parameters.AddWithValue("@village", locationData.Village);
                    }

                    if (!string.IsNullOrEmpty(locationData.Address))
                    {
                        query.Append("\"Address\" = @address, ");
                        cmd.Parameters.AddWithValue("@address", locationData.Address);
                    }

                    if (!string.IsNullOrEmpty(locationData.NameOfPlace))
                    {
                        query.Append("\"NameOfPlace\" = @nameOfPlace, ");
                        cmd.Parameters.AddWithValue("@nameOfPlace", locationData.NameOfPlace);
                    }

                    if (locationData.UpdatedBy != Guid.Empty)
                    {
                        query.Append("\"UpdatedBy\" = @updatedBy, ");
                        cmd.Parameters.AddWithValue("@updatedBy", locationData.UpdatedBy);
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


        public async Task<ILocationModel> GetLocationByJambaseIdentifierAsync(string jambaseIdentifier)
        {
            ILocationModel location = null;
            using (var con = new NpgsqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;

                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT l.\"Id\", l.\"Country\", l.\"City\", l.\"Village\", l.\"Address\", l.\"NameOfPlace\", l.\"IsActive\", l.\"JambaseIdentifier\", ");
                    query.Append("g.\"Id\" AS GeoLocationId, g.\"Latitude\", g.\"Longitude\", g.\"LocationId\", ");
                    query.Append("e.\"Id\" AS EventId, e.\"Name\", e.\"Url\", e.\"IsActive\" AS EventIsActive, e.\"EventStatus\", e.\"Image\", e.\"StartDate\", e.\"EndDate\", e.\"IsAccessibleForFree\", e.\"Type\", e.\"TicketInformationId\" AS EventTicketInformationId, e.\"LocationId\" AS EventLocationId, ");
                    query.Append("s.\"Id\" AS StoryId, s.\"StoryText\", s.\"Date\", s.\"LocationId\" AS StoryLocationId, s.\"UserId\" AS StoryUserId, ");
                    query.Append("t.\"Id\" AS TouristSitesId, t.\"SiteName\", t.\"Link\", t.\"LocationId\" AS TouristSiteLocationId, t.\"Fsq_Id\", t.\"Popularity\", t.\"Rating\", t.\"Description\", t.\"WebsiteUrl\", t.\"Email\", t.\"HoursOpen\", t.\"FacebookId\", t.\"Instagram\", t.\"Twitter\" ");
                    query.Append("FROM \"Location\" l ");
                    query.Append("LEFT JOIN \"GeoLocation\" g ON l.\"Id\" = g.\"LocationId\" ");
                    query.Append("LEFT JOIN \"Event\" e ON l.\"Id\" = e.\"LocationId\" ");
                    query.Append("LEFT JOIN \"Story\" s ON l.\"Id\" = s.\"LocationId\" ");
                    query.Append("LEFT JOIN \"TouristSites\" t ON l.\"Id\" = t.\"LocationId\" ");
                    query.Append("WHERE l.\"JambaseIdentifier\" = @jambaseIdentifier ");
                    query.Append("AND l.\"IsActive\" = true");

                    cmd.Parameters.AddWithValue("@jambaseIdentifier", jambaseIdentifier);
                    cmd.CommandText = query.ToString();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (location == null)
                            {
                                location = MapLocation(reader);
                            }
                            if (reader["GeoLocationId"] != DBNull.Value)
                            {
                                var geoLocation = MapGeoLocation(reader);
                                location.GeoLocations.Add(geoLocation);
                            }
                            if (reader["EventId"] != DBNull.Value)
                            {
                                IEventModel eventModel = MapEvent(reader);
                                location.Events.Add(eventModel);
                            }
                            if (reader["StoryId"] != DBNull.Value)
                            {
                                IStoryModel story = MapStory(reader);
                                location.Stories.Add(story);
                            }
                            if (reader["TouristSitesId"] != DBNull.Value)
                            {
                                ITouristSitesModel touristSite = MapTouristSite(reader);
                                location.Sites.Add(touristSite);
                            }
                        }
                    }
                }
            }
            return location;
        }

        public async Task<ILocationModel> GetLocationByAddressAsync(string address)
        {
            ILocationModel location = null;
            using (var con = new NpgsqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;

                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT l.\"Id\", l.\"Country\", l.\"City\", l.\"Village\", l.\"Address\", l.\"NameOfPlace\", l.\"IsActive\", l.\"JambaseIdentifier\", ");
                    query.Append("g.\"Id\" AS GeoLocationId, g.\"Latitude\", g.\"Longitude\", g.\"LocationId\", ");
                    query.Append("e.\"Id\" AS EventId, e.\"Name\", e.\"Url\", e.\"IsActive\" AS EventIsActive, e.\"EventStatus\", e.\"Image\", e.\"StartDate\", e.\"EndDate\", e.\"IsAccessibleForFree\", e.\"Type\", e.\"TicketInformationId\" AS EventTicketInformationId, e.\"LocationId\" AS EventLocationId, ");
                    query.Append("s.\"Id\" AS StoryId, s.\"StoryText\", s.\"Date\", s.\"LocationId\" AS StoryLocationId, s.\"UserId\" AS StoryUserId, ");
                    query.Append("t.\"Id\" AS TouristSitesId, t.\"SiteName\", t.\"Link\", t.\"LocationId\" AS TouristSiteLocationId, t.\"Fsq_Id\", t.\"Popularity\", t.\"Rating\", t.\"Description\", t.\"WebsiteUrl\", t.\"Email\", t.\"HoursOpen\", t.\"FacebookId\", t.\"Instagram\", t.\"Twitter\" ");
                    query.Append("FROM \"Location\" l ");
                    query.Append("LEFT JOIN \"GeoLocation\" g ON l.\"Id\" = g.\"LocationId\" ");
                    query.Append("LEFT JOIN \"Event\" e ON l.\"Id\" = e.\"LocationId\" ");
                    query.Append("LEFT JOIN \"Story\" s ON l.\"Id\" = s.\"LocationId\" ");
                    query.Append("LEFT JOIN \"TouristSites\" t ON l.\"Id\" = t.\"LocationId\" ");
                    query.Append("WHERE l.\"Address\" = @address");

                    cmd.Parameters.AddWithValue("@address", address);
                    cmd.CommandText = query.ToString();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (location == null)
                            {
                                location = MapLocation(reader);
                            }
                            if (reader["GeoLocationId"] != DBNull.Value)
                            {
                                var geoLocation = MapGeoLocation(reader);
                                location.GeoLocations.Add(geoLocation);
                            }
                            if (reader["EventId"] != DBNull.Value)
                            {
                                IEventModel eventModel = MapEvent(reader);
                                location.Events.Add(eventModel);
                            }
                            if (reader["StoryId"] != DBNull.Value)
                            {
                                IStoryModel story = MapStory(reader);
                                location.Stories.Add(story);
                            }
                            if (reader["TouristSitesId"] != DBNull.Value)
                            {
                                ITouristSitesModel touristSite = MapTouristSite(reader);
                                location.Sites.Add(touristSite);
                            }
                        }
                    }
                }
            }
            return location;
        }


        private ILocationModel MapLocation(NpgsqlDataReader reader)
        {
            return new LocationModel
            {
                Id = (Guid)reader["Id"],
                Country = !string.IsNullOrWhiteSpace(Convert.ToString(reader["Country"])) ? Convert.ToString(reader["Country"]) : null,
                City = !string.IsNullOrWhiteSpace(Convert.ToString(reader["City"])) ? Convert.ToString(reader["City"]) : null,
                Village = !string.IsNullOrWhiteSpace(Convert.ToString(reader["Village"])) ? Convert.ToString(reader["Village"]) : null,
                Address = !string.IsNullOrWhiteSpace(Convert.ToString(reader["Address"])) ? Convert.ToString(reader["Address"]) : null,
                NameOfPlace = !string.IsNullOrWhiteSpace(Convert.ToString(reader["NameOfPlace"])) ? Convert.ToString(reader["NameOfPlace"]) : null,
                JambaseIdentifier = !string.IsNullOrWhiteSpace(Convert.ToString(reader["JambaseIdentifier"])) ? Convert.ToString(reader["JambaseIdentifier"]) : null,
                IsActive = Convert.ToBoolean(reader["IsActive"]),

                GeoLocations = Convert.IsDBNull(reader["GeoLocationId"]) ? null : new List<IGeoLocationModel>(),
                Events = Convert.IsDBNull(reader["EventId"]) ? null : new List<IEventModel>(),
                Sites = Convert.IsDBNull(reader["TouristSitesId"]) ? null : new List<ITouristSitesModel>(),
                Stories = Convert.IsDBNull(reader["StoryId"]) ? null : new List<IStoryModel>()
            };
        }

        private IGeoLocationModel MapGeoLocation(NpgsqlDataReader reader)
        {
            return new GeoLocation
            {
                Id = (Guid)reader["GeoLocationId"],
                Latitude = Convert.ToSingle(reader["Latitude"]),
                Longitude = Convert.ToSingle(reader["Longitude"]),
                LocationId = (Guid)reader["LocationId"],
                IsActive = reader["IsActive"] as bool?,
            };
        }

        private IEventModel MapEvent(NpgsqlDataReader reader)
        {
            return new EventModel
            {
                Id = (Guid)reader["EventId"],
                Name = !string.IsNullOrWhiteSpace(Convert.ToString(reader["Name"])) ? Convert.ToString(reader["Name"]) : null,
                Url = !string.IsNullOrWhiteSpace(Convert.ToString(reader["Url"])) ? Convert.ToString(reader["Url"]) : null,
                IsActive = Convert.ToBoolean(reader["EventIsActive"]),
                EventStatus = !string.IsNullOrWhiteSpace(Convert.ToString(reader["EventStatus"])) ? Convert.ToString(reader["EventStatus"]) : null,
                Image = !string.IsNullOrWhiteSpace(Convert.ToString(reader["Image"])) ? Convert.ToString(reader["Image"]) : null,
                StartDate = reader["StartDate"] != DBNull.Value ? Convert.ToDateTime(reader["StartDate"]) : null,
                EndDate = reader["EndDate"] != DBNull.Value ? Convert.ToDateTime(reader["EndDate"]) : null,
                IsAccessibleForFree = reader["IsAccessibleForFree"] != DBNull.Value && Convert.ToBoolean(reader["IsAccessibleForFree"]),
                Type = !string.IsNullOrWhiteSpace(Convert.ToString(reader["Type"])) ? Convert.ToString(reader["Type"]) : null,
                TicketInformationId = Convert.IsDBNull(reader["EventTicketInformationId"]) ? null : (Guid)reader["EventTicketInformationId"],
                LocationId = Convert.IsDBNull(reader["EventLocationId"]) ? null : (Guid)reader["EventLocationId"]
            };
        }
        private IStoryModel MapStory(NpgsqlDataReader reader)
        {
            return new StoryModel
            {
                Id = (Guid)reader["StoryId"],
                Text = !string.IsNullOrWhiteSpace(Convert.ToString(reader["StoryText"])) ? Convert.ToString(reader["StoryText"]) : null,
                LocationId = (Guid)reader["StoryLocationId"],
                UserId = (Guid)reader["StoryUserId"],
                DateTime = reader["Date"] != DBNull.Value ? Convert.ToDateTime(reader["Date"]) : null,
            };
        }

        private ITouristSitesModel MapTouristSite(NpgsqlDataReader reader)
        {
            return new TouristSitesModel
            {
                Id = (Guid)reader["TouristSitesId"],
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
            };
        }

    }
}
