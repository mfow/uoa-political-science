using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ElectionSystem
{
    public static class CSV
    {
        public static IEnumerable<List<string>> ReadFromStream(Stream strm)
        {
            var r = new StreamReader(strm, Encoding.UTF8);

            while (!r.EndOfStream)
            {
                //yield return r.ReadLine().Split(',').ToList();

                var s = r.ReadLine();

                var result = new List<string>();
                string s2;
                int index;

                while (s.Length > 0)
                {
                    if (s.StartsWith("\""))
                    {
                        index = s.IndexOf("\",");

                    }
                    else
                    {
                        index = s.IndexOf(",");
                    }

                    if (index == -1)
                    {
                        result.Add(s);
                        break;
                    }

                    s2 = s.Substring(0, index);
                    s = s.Substring(s.IndexOf(',', index) + 1);

                    result.Add(s2);
                }

                yield return (from x in result select NoQuotesArround(x)).ToList();
            }
        }

        private static string NoQuotesArround(string s)
        {
            var result = s;

            if (result.StartsWith("\""))
            {
                result = result.Substring(1);
            }

            if (result.EndsWith("\""))
            {
                result = result.Substring(0, result.Length - 1);
            }

            return result;
        }

        public static void WriteToStream(Stream strm, IEnumerable<IEnumerable<string>> data)
        {
            var w = new StreamWriter(strm);

            foreach (var row in data)
            {
                foreach (var item in row)
                {
                    if (item.Contains(","))
                    {
                        w.Write("\"");
                        w.Write(item.Replace("\"", "\"\""));
                        w.Write("\"");
                    }
                    else
                    {
                        w.Write(item);
                    }

                    w.Write(",");
                }

                w.WriteLine();
            }

            w.Flush();
        }
    }
}
