using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoTagMap.Common.Filtering
{
    public class StoryFiltering
    {
        public DateTime? Date { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public int? NumberOfLikes { get; set; }
        public bool? IsReported { get; set; }

        public StoryFiltering(DateTime? date, int? numberOfLikes, string country, string city, bool? isReported)
        {
            Date = date;
            NumberOfLikes = numberOfLikes;
            Country = country;
            City = city;
            IsReported = isReported;
        }
    }
}