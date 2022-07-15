using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StationSearchCore.Service;

namespace StationSearchCore.Web.Controllers
{
    public class StationsController : ControllerBase
    {
        //more descriptive variable name, as well as extraction to interface
        private ILookupService LookupService { get; set; } = new LookupService();

        // GET api/values
        public async Task<StationSearchResult> GetStations(string filter)
        {
            //move input validation to separate method
            filter = ValidateFilterInput(filter);

            //todo: add error handling. What if LookupService is unavailable?
            var stations = await LookupService.GetAllStartingWithAsync(filter);

            //avoid possible multiple enumeration, since ApplyFilter also enumerates the stations
            var stationsEnumerable = stations as string[] ?? stations.ToArray();
            //extract filter application to separate method
            var nextPossibleChars = ApplyFilter(filter, stationsEnumerable);

            return new StationSearchResult(nextPossibleChars, stationsEnumerable.OrderBy(x => x));
        }

        private IEnumerable<char> ApplyFilter(string filter, IEnumerable<string> stations)
        {
            var nextPossibleChars = stations.Where(station => station.Length
                                                              > filter.Length)
                .Select(station => station[filter.Length]).OrderBy(x => x).Distinct();
            return nextPossibleChars;
        }

        public string ValidateFilterInput(string filter)
        {
            //TODO: add better input checking
            if (string.IsNullOrEmpty(filter))
            {
                filter = " ";
            }

            filter = filter.Replace("\"", "");
            return filter;
        }
    }

    //perhaps not currently necessary, but could be made an interface to support possibly needing different types
    //of search results in the future
    public class StationSearchResult
    {
        public StationSearchResult() { }

        public StationSearchResult(IEnumerable<char> nextPossibleCharacters, IEnumerable<string> stations)
        {
            //mostly stylistic choices
            NextPossibleCharacters = nextPossibleCharacters ?? Array.Empty<char>();
            Stations = stations ?? Array.Empty<string>();
        }

        public IEnumerable<char> NextPossibleCharacters { get; private set; }

        public IEnumerable<string> Stations { get; private set; }
    }

}
