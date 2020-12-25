using System;
using System.Collections.Generic;
using System.Text;

namespace Ipdb.Models
{
    public class IpdbDatabase
    {
        public IpdbDatabase() {
            LastRefreshDateUtc = DateTime.UtcNow;
            Data = new List<IpdbResult>();
        }

        public DateTime LastRefreshDateUtc { get; set; }
        public List<IpdbResult> Data { get; set; }
    }
}
