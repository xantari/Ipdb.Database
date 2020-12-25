using HtmlAgilityPack;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Ipdb.Utilities
{
    public static class Extensions
    {
        private static Regex digitsOnly = new Regex(@"[^\d]");

        public static string GetOnlyNumeric(this string data)
        {
            return digitsOnly.Replace(data, "");
        }

        public static string GetDescriptionAttr<T>(this T source)
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0) return attributes[0].Description;
            else return source.ToString();
        }

        public static bool IsNumeric(this string value)
        {
            double retNum;

            bool isNum = Double.TryParse(Convert.ToString(value), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }

        public static string RemoveNonAsciiCharacters(this string s)
        {
            return Regex.Replace(s, @"[^\u0000-\u007F]+", string.Empty);
        }

        public static string GetEnumDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                    typeof(DescriptionAttribute),
                    false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        public static class ThreadSafeRandom
        {
            [ThreadStatic]
            private static Random Local;

            public static Random ThisThreadsRandom
            {
                get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
            }
        }

        public static string ConvertHtmlToPlainText(this string html, bool lineAppendMode = false) //https://stackoverflow.com/questions/4182594/grab-all-text-from-html-with-html-agility-pack
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml($@"<html><body>{html}</body></html>");

            StringBuilder sb = new StringBuilder();
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//text()"))
            {
                if (lineAppendMode)
                    sb.AppendLine(node.InnerText);
                else
                    sb.Append(node.InnerText);
            }
            return sb.ToString().Trim();
        }

        public static string CondenseHtml(this string s, bool retainCarriageReturns = false, bool eliminateDoubleSpaces = true, bool stripTabs = true)
        {
            string temp = s.Replace("&nbsp;", " ");
            if (!retainCarriageReturns)
            {
                temp = temp.Replace("\n", "").Replace("\r\n", "");
            }
            if (eliminateDoubleSpaces)
            {
                temp = temp.Replace("  ", " ");
            }

            if (stripTabs)
            {
                temp = temp.Replace("\t", "");
            }

            return temp.Trim();
        }

        public static string NormalizeCarriageReturns(this string s)
        {
            return s
                .Replace("\r\n", "\n")
                .Replace("\n\r", "\n")
                .Replace("\r", "\n")
                .Replace("\n", "\r\n");
        }

        public static string ConvertBreaksToCarriageReturns(this string s)
        {
            return s.Replace("<br>", "\r\n").Replace("<br/>", "\r\n").Replace("<br />", "\r\n").Trim();
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static int GetRandomTime(int lowestSleepTimeInMilliseconds, int largestSleepTimeInMilliseconds)
        {
            Random rnd = new Random();
            return rnd.Next(lowestSleepTimeInMilliseconds, largestSleepTimeInMilliseconds);
        }

        public static void SleepForRandomTime(bool throttleWebRequests, int lowestSleepTimeInMilliseconds, int largestSleepTimeInMillseconds)
        {
            if (throttleWebRequests)
            {
                int time = GetRandomTime(lowestSleepTimeInMilliseconds, largestSleepTimeInMillseconds);
                Log.Debug("Sleepinging for {time}ms", time);
                Thread.Sleep(time);
            }
        }
    }
}
