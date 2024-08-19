using GeoTagMap.Models.Common;
using GeoTagMap.RestViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeoTagMap.WebApi.RestViewModels.View
{
    public class StoryView
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public DateTime? DateTime { get; set; }
        public int? NumberOfLikes { get; set; }
        public bool? IsReported { get; set; }


        public LocationView Location { get; set; }
        public List<GeoLocationView> GeoLocations { get; set; }
        public List<CommentView> Comment { get; set; }
        public UserModelView User { get; set; }
        public List<PhotoView> Photos { get; set; }
    }
}