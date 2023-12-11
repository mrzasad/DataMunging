using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Test
{
    class Program
    {
        static void WeatherTest()
        {
            var dreader = new ReadDelimatedFile();
            dreader.FilePath = @"C:\Users\Liaquat\Downloads\weather.txt";
            dreader.Headers.Add(new DelimitedField(2, "Blank"));
            dreader.Headers.Add(new DelimitedField(3, "Day"));
            dreader.Headers.Add(new DelimitedField(6, "Max Temp"));
            dreader.Headers.Add(new DelimitedField(6, "Min Temp"));
            dreader.Headers.Add(new DelimitedField(6, "Avg Temp"));

            var count = dreader.LoadData();
            var _data = dreader.Data;

            foreach (var _d in _data)
            {
                System.Console.WriteLine("{0} {1} {2}", _d[1], _d[2], _d[3]);
            }
            System.Console.ReadKey();

        }
        static void SoccerLeagueTable()
        {
            var dreader = new ReadDelimatedFile();
            dreader.FilePath = @"C:\Users\Liaquat\Downloads\football.dat";
            dreader.TrimValue = true;

            var _data = dreader.LoadData<SoccerLeagueTableData>();

            var _mind = _data.Min(m => m.Diff);

            var _teams = _data.Where(w => w.Diff == _mind).ToList();

            Console.WriteLine("Team(s) with minimal score difference of {0}: {1}", _mind, _teams.Count);

            System.Console.ReadKey();

        }
        private static string PostSpaces(string txt, int length)
        {
            if (txt.Length < length) txt = txt + string.Empty.PadLeft(length - txt.Length, ' ');
            return txt;
        }
        static void Main(string[] args)
        {
            //WeatherTest();
            SoccerLeagueTable();
        }
    }
    public class DelimitedFieldAttribute : System.Attribute
    {
       public int Length { get; set; }
        public string FieldName { get; set; }
        public DelimitedFieldAttribute(string fieldname,int length)
        {
            this.FieldName = fieldname;
            this.Length = length;
        }
        public DelimitedFieldAttribute(int length) { this.Length = length; }
    }

    public class SoccerLeagueTableData
    {
        [DelimitedField(3)]
        public string SrNo { get; set; }

        [DelimitedField(16)]
        public string TeamName { get; set; }

        [DelimitedField(6)]
        public string P { get; set; }

        [DelimitedField(4)]
        public string W { get; set; }

        [DelimitedField(4)]
        public string L { get; set; }

        [DelimitedField(6)]
        public string D { get; set; }

        [DelimitedField(2)]
        public string F { get; set; }

        [DelimitedField(5)]
        public string Sign { get; set; }

        [DelimitedField(6)]
        public string A { get; set; }

        [DelimitedField(3)]
        public string PTS { get; set; }

        private int? _Diff;
        public int Diff
        {
            get
            {
                if (_Diff == null) _Diff = System.Convert.ToInt32(System.Math.Abs(System.Convert.ToInt32(this.F) - System.Convert.ToInt32(this.A)));
                return _Diff.Value;
            }
        }
    }

    public class DelimitedField
    {
        public int Length { get; set; }
        public string Header { get; set; }

        public DelimitedField(int length, string header)
        {
            this.Length = length;
            this.Header = header;
        }
    }
    public class ReadDelimatedFile
    {
        public bool FirstRowHeader { get; set; }
        public bool TrimValue { get; set; }
        public string FilePath { get; set; }
        public System.Collections.Generic.List<DelimitedField> Headers { get; set; }

        public System.Collections.Generic.List<string[]> Data { get; set; }

        public int ContentLength { get; set; }
        public ReadDelimatedFile()
        {
            this.FirstRowHeader = true;
            this.Headers = new System.Collections.Generic.List<DelimitedField>();
            this.Data = new System.Collections.Generic.List<string[]>();
        }

        public int LoadData()
        {
            int _line_number = 0;
            using (var sr = new System.IO.StreamReader(this.FilePath))
            {
                if (this.FirstRowHeader) sr.ReadLine();

                this.ContentLength = 0;
                foreach (var h in this.Headers) this.ContentLength = this.ContentLength + h.Length;

                this.Data.Clear();

                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();



                    if (line.Length >= this.ContentLength)
                    {
                        var _row = new string[this.Headers.Count];
                        _line_number++;
                        var _start_index = 0;
                        for (var i = 0; i < this.Headers.Count; i++)
                        {
                            var h = this.Headers[i];

                            var _max_length = _start_index + h.Length;

                            var _content = string.Empty;
                            if (_max_length >= line.Length) _content = line.Substring(_start_index);
                            else _content = line.Substring(_start_index, h.Length);


                            if (this.TrimValue) _content = _content.Trim();

                            _row[i] = _content;
                            _start_index += h.Length;
                        }
                        this.Data.Add(_row);
                    }
                }
                sr.Close();
            }
            return _line_number;
        }

        public System.Collections.Generic.List<T> LoadData<T>()
        {

            var _list = new System.Collections.Generic.List<T>();

            var _type = typeof(T);
            var _properties = _type.GetProperties();
            using (var sr = new System.IO.StreamReader(this.FilePath))
            {
                if (this.FirstRowHeader) sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();

                    if (this.TrimValue) line = line.Trim();

                    if(string.IsNullOrEmpty( line.Replace("-",string.Empty))) continue;

                    var o = System.Activator.CreateInstance<T>();

                    var _start_index = 0;
                    foreach (var _p in _properties)
                    {
                        var _attrs = _p.GetCustomAttributes(true);
                        if (_attrs.Length > 0)
                        {
                            var _att = (DelimitedFieldAttribute)_attrs[0];
                            var _max_length = _start_index + _att.Length;

                            var _content = string.Empty;
                            if (_max_length >= line.Length) _content = line.Substring(_start_index);
                            else _content = line.Substring(_start_index, _att.Length);

                            if (this.TrimValue) _content = _content.Trim();

                            _p.SetValue(o, _content);

                            _start_index += _att.Length;
                        }
                    }

                    _list.Add(o);
                }
                sr.Close();
            }

            return _list;
        }
    }

}
