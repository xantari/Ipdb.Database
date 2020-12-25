using System;

namespace Ipdb.Models
{
    public class ProxySetting
    {
        public ProxySetting() { }

        public int Port { get; set; }
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public int FailureRetryCount { get; set; }
    }
}
