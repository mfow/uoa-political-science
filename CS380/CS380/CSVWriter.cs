using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CS380
{
    public class CSVWriter
    {
        private Stream strm;
        private StreamWriter writer;

        public CSVWriter(Stream strm)
        {
            this.strm = strm;
            this.writer = new StreamWriter(strm);
        }

        public void WriteLine(IEnumerable<string> values)
        {
            foreach (var s in values)
            {
                writer.Write(s);
                writer.Write(",");
            }
            writer.WriteLine();
            writer.Flush();
        }

        public void Close()
        {
            writer.Flush();
            writer.Close();
        }
    }

    public class CSVReportWriter<T>
    {
        private CSVWriter writer;
        private List<Tuple<string, Func<T, string>>> columns;
        private bool writtenHeader;

        public CSVReportWriter(CSVWriter csvWriter)
        {
            this.writer = csvWriter;
            this.columns = new List<Tuple<string, Func<T, string>>>();
            this.writtenHeader = false;
        }

        public void AddColumn(string headerName, Func<T, string> cellFunction)
        {
            if (writtenHeader)
            {
                throw new Exception();
            }

            columns.Add(new Tuple<string, Func<T, string>>(headerName, cellFunction));
        }

        public void WriteLine(T value)
        {
            if (!writtenHeader)
            {
                writer.WriteLine(from x in columns select x.Item1);

                writtenHeader = true;
            }

            writer.WriteLine(from x in columns select x.Item2(value));
        }

        public void Close()
        {
            writer.Close();
        }
    }
}
