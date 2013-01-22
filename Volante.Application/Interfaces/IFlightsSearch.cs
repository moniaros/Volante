using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Volante.Application.Requests;
using Volante.Flights.Domain.Entities;

namespace Volante.Application.Interfaces
{
    public interface IFlightsSearch
    {

        int SearchFlightsAsync(
            List<SearchFlightsRequestLeg> legs,
            bool directOnly = false,
            bool includeNearbyAirports = false,
            List<Airline> airlines = null,
            List<ClassType> classes = null
            );

        object SearchFlightsResult(int searchIdentifier, object criteria = null);  
    }
}
