using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HC_Lib.Maple
{
    public class MapleMatrix
    {
        public MapleMatrix(int Rows, int Columns)
        {
            InitializeMatrix(Rows, Columns);
        }
        public MapleMatrix(string[][] Values)
        {
            this.Values = Values;
            Rows = Values.Length;
            Columns = Values.First().Length;
        }
        public MapleMatrix(string LPrintMapleOutput) {
            LPrintMapleOutput = LPrintMapleOutput.Replace(" ", "");

            // Matrix(3,3,{(1, 1) = 1, (2, 2) = 1, (3, 3) = 1},datatype = anything,storage = rectangular,order = Fortran_order,shape = [])
            var DimensionsRegex = new Regex(@"Matrix\(([0-9]+)\,([0-9]+)\,\{", RegexOptions.Singleline);
            var ValuesRegex = new Regex(@"\(([0-9]+)\,([0-9]+)\)\=([0-9a-zA-Z\\\(\) \-'_\*\/\+\-]+)(\,|)", RegexOptions.Singleline);

            var DimensionsMatch = DimensionsRegex.Match(LPrintMapleOutput);
            if (DimensionsMatch.Success)
            {
                int Rows = int.Parse(DimensionsMatch.Groups[1].Value);
                int Columns = int.Parse(DimensionsMatch.Groups[2].Value);//safe operation because regex group is parsed by [0-9]+
                InitializeMatrix(Rows, Columns);

                var ValuesMatch = ValuesRegex.Match(LPrintMapleOutput);
                while (ValuesMatch.Success)
                {
                    int ri = int.Parse(ValuesMatch.Groups[1].Value);
                    int ci = int.Parse(ValuesMatch.Groups[2].Value);
                    var value = ValuesMatch.Groups[3].Value;
                    Values[ri-1][ci-1] = value;
                    ValuesMatch = ValuesMatch.NextMatch();
                }
                return;
            }
            throw new ArgumentException("Invalid Matrix. Make sure you use exact output from lprint(matrix) given by Maple.");
        }

        public string[][] Values { get; private set; }
        public int Rows { get; private set; }
        public int Columns { get; private set; }

        public override string ToString()
        {
            StringBuilder @out = new StringBuilder();
            @out.Append($"Matrix({Rows},{Columns},{{");
            for (int ri = 0; ri < Rows; ri++)
            {
                for (int ci = 0; ci < Columns; ci++)
                {
                    @out.Append($"({ri+1}, {ci+1}) = {Values[ri][ci]}");

                    if (ri != Rows - 1 || ci != Columns - 1) // exclude comma on last entry.                    
                        @out.Append(", ");
                    
                }
            }
            @out.Append("})"); // ,datatype = anything,storage = rectangular,order = Fortran_order,shape = []

            return @out.ToString();
        }
        
        private void InitializeMatrix(int Rows, int Columns)
        {
            this.Rows = Rows;
            this.Columns = Columns;
            Values = new string[Rows][];
            for (int ri = 0; ri < Rows; ri++)
            {
                Values[ri] = new string[Columns];
                for (int ci = 0; ci < Columns; ci++)
                {
                    Values[ri][ci] = "0";
                }
            }
        }
    }
}
