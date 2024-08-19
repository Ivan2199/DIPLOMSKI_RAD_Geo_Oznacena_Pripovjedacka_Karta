using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoTagMap.Common.Filtering
{
    public class CommentFiltering
    {
        public string Username { get; set; }
        public DateTime? DateCreated { get; set; }
        public bool? IsReported { get; set; }

        public CommentFiltering(string username, DateTime? dateCreated, bool? isReported)
        {
            Username = username;
            DateCreated = dateCreated;
            IsReported = isReported;
        }
    }
}