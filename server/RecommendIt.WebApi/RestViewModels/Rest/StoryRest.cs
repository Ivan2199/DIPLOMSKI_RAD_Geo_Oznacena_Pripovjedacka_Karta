using GeoTagMap.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeoTagMap.WebApi.RestViewModels.Rest
{
    public class StoryRest
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public string ImageData { get; set; }
        public Guid LocationId { get; set; }
        public int? NumberOfLikes { get; set; }
        public bool? IsReported { get; set; }
    }
}