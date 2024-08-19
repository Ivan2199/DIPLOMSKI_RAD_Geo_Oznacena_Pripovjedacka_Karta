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
    public class TicketInformationService : ITicketInformationService
    {
        private static readonly string AdminId = ConfigurationManager.AppSettings["AdminId"];
        private readonly ITicketInformationRepository _ticketInformationRepository;

        public TicketInformationService(ITicketInformationRepository ticketInformationRepository)
        {
            _ticketInformationRepository = ticketInformationRepository;
        }

        public async Task<List<ITicketInformationModel>> GetTicketInformationsAsync()
        {
            return await _ticketInformationRepository.GetTicketInformationsAsync();
        }

        public async Task<ITicketInformationModel> GetTicketInformationAsync(Guid id)
        {
            return await _ticketInformationRepository.GetTicketInformationAsync(id);
        }

        public async Task AddTicketInformationAsync(ITicketInformationModel ticketInformation)
        {
            Guid userId = GetUserId() ?? Guid.Parse(AdminId);
            ticketInformation.CreatedBy = userId;
            ticketInformation.UpdatedBy = userId;
            await _ticketInformationRepository.AddTicketInformationAsync(ticketInformation);
        }

        public async Task UpdateTicketInformationAsync(Guid id, ITicketInformationModel ticketInformationData)
        {
            Guid userId = GetUserId() ?? Guid.Parse(AdminId);
            ticketInformationData.UpdatedBy = userId;
            await _ticketInformationRepository.UpdateTicketInformationAsync(id, ticketInformationData);
        }

        public Guid? GetUserId()
        {
            var identity = ClaimsPrincipal.Current.Identity as ClaimsIdentity;
            var userIdClaim = identity?.FindFirst("userId")?.Value;
            return userIdClaim != null ? Guid.Parse(userIdClaim) : (Guid?)null;
        }
    }
}
