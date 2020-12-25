using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ipdb.Utilities
{
    public static class UserAgentStrings
    {
        public static List<string> UserAgentStringList = new List<string>();

        public static void InitializeList(string userAgentStringFile)
        {
            using (StreamReader sr = File.OpenText(userAgentStringFile))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    UserAgentStringList.Add(s);
                }
            }
        }
        public static string GetRandomUserAgentString()
        {
            var random = new Random();
            if (UserAgentStringList.Count == 0)
                throw new Exception("You must initialize the list first.");
            int index = random.Next(UserAgentStringList.Count);
            return UserAgentStringList[index];
        }
    }
}
