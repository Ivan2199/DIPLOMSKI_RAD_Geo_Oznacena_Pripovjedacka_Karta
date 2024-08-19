using GeoTagMap.Models.Common;
using GeoTagMap.Repository.Common;
using GeoTagMap.Service.Common;
using System;
using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GeoTagMap.Service
{
    public class EventPerformerService : IEventPerformerService
    {
        private static readonly string AdminId = ConfigurationManager.AppSettings["AdminId"];
        private readonly IEventPerformerRepository _eventPerformerRepository;

        public EventPerformerService(IEventPerformerRepository eventPerformerRepository)
        {
            _eventPerformerRepository = eventPerformerRepository;
        }

        public async Task AddEventPerformerAsync(IEventPerformerModel eventPerformer)
        {
            Guid userId = GetUserId() ?? Guid.Parse(AdminId);
            eventPerformer.CreatedBy = userId;
            eventPerformer.UpdatedBy = userId;
            await _eventPerformerRepository.AddEventPerformerAsync(eventPerformer);
        }

        public Guid? GetUserId()
        {
            var identity = ClaimsPrincipal.Current.Identity as ClaimsIdentity;
            var userIdClaim = identity?.FindFirst("userId")?.Value;
            return userIdClaim != null ? Guid.Parse(userIdClaim) : (Guid?)null;
        }
    }
}
