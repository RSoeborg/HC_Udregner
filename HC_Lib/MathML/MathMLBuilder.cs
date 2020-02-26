using HC_Lib.Maple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HC_Lib.MathML
{
    class MathMLBuilder
    {
        private StringBuilder builder = new StringBuilder();
        private readonly bool IncludeHeader;

        public MathMLBuilder(bool IncludeHeader) {
            this.IncludeHeader = IncludeHeader;
            if (IncludeHeader)
            {
                builder.Append("<math>");
            }
        }

        public MathMLBuilder(string MathML)
        {
            IncludeHeader = true;
            MathML = MathML.Trim().Replace("\r\n", "").Replace("\n", "");

            var endTag = "</math>";
            if (!MathML.ToLower().EndsWith(endTag))
            {
                throw new ArgumentException("Invalid MathML source.");
            }
            
            builder.Append(MathML.Remove(MathML.Length - endTag.Length, endTag.Length));
        }

        public void AddText(string Text)
        {
            builder.Append($"<mtext>{FormatText(Text)}</mtext>");
        }

        public void AddMatrix(MapleMatrix Matrix)
        {
            builder.Append("<mfenced close=']' open='['>");
            builder.Append("<mtable>");
            

            for (int ri = 0; ri < Matrix.Rows; ri++)
            {
                builder.Append("<mtr>");
                for (int ci = 0; ci < Matrix.Columns; ci++)
                {
                    builder.Append("<mtd>");
                    builder.Append($"<mn>{Matrix.Values[ri][ci]}</mn>");
                    builder.Append("</mtd>");
                }
                builder.Append("</mtr>");
            }

            builder.Append("</mtable>");
            builder.Append("</mfenced>");
        }

        public void MergeML(string ML)
        {
            var header = "<math xmlns='http://www.w3.org/1998/Math/MathML'><semantics>";
            var footer = "</semantics></math>";

            if (ML.StartsWith(header) && ML.EndsWith(footer))
            {
                builder.Append(ML.Substring(header.Length, ML.Length - (header.Length + footer.Length)));
            }
            else throw new ArgumentException("Invalid MathML. Ensure output is from Maple.");
        }
        
        public override string ToString()
        {
            return builder.ToString() + (IncludeHeader ? "</math>" : "");
        }

        private string FormatText(string Text)
        {
            return Text.Replace("\r\n", "&NewLine;").Replace("\n", "&NewLine;");
        }
    }
}
