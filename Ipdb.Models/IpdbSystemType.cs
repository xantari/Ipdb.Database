using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Ipdb.Models
{
    public enum IpdbSystemType
    {
        [Description("Solid State")]
        SolidState,
        [Description("Electro Mechanical")]
        ElectroMechanical,
        [Description("Purely Mechanical")]
        Mechanical,
        Unknown
    }

    public static class IpdbSystemTypeInfo
    {
        public static IpdbSystemType GetSystemTypeFromString(string type)
        {
            if (type.Contains("Solid State Electronic"))
                return IpdbSystemType.SolidState;
            if (type.Contains("Electro-mechanical"))
                return IpdbSystemType.ElectroMechanical;
            if (type.Contains("Purely Mechanical"))
                return IpdbSystemType.Mechanical;
            return IpdbSystemType.Unknown;
        }

        public static string GetTypeShortName(string type)
        {
            if (type.Contains("Solid State Electronic"))
                return "SS";
            if (type.Contains("Electro-mechanical"))
                return "EM";
            if (type.Contains("Purely Mechanical"))
                return "ME";
            return string.Empty;
        }
    }
}
