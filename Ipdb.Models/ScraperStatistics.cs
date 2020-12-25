using System;
using System.Collections.Generic;
using System.Text;

namespace Ipdb.Models
{
    public class ScraperStatistics
    {
        public int TotalHTTPGets { get; set; }
        public int TotalHTTPPosts { get; set; }
        public int TotalRetries { get; set; }
        public int TotalFailures { get; set; }
        public DateTime ProcessStartTime { get; set; }
        public DateTime ProcessEndTime { get; set; }

    }
}
