using HC_Lib.Maple;
using HC_Lib.MathML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HC_Lib.JavaWin
{
    public class MapletOutput
    {
        private string DefaultPath = "";

        public string MathML { get; private set; }

        public MapletOutput(string Path) { DefaultPath = Path; }

        public async Task<string> GaussJordanEliminationTutor(string RawMatrix)
        {
            return await TutorResult(nameof(GaussJordanEliminationTutor), RawMatrix);
        }

        public async Task<string> GaussianEliminationTutor(string RawMatrix)
        {
            return await TutorResult(nameof(GaussianEliminationTutor), RawMatrix);
        }

        private async Task<string> TutorResult(string Method, string RawMatrix)
        {

            MathMLBuilder MathBuilder = default;
            var engine = new MapleLinearAlgebra(DefaultPath);

            if (RawMatrix.EndsWith(";"))
                RawMatrix = RawMatrix.Remove(RawMatrix.Length - 1, 1).Replace("\r\n", string.Empty);

            engine.Open();
            var minified = await engine.LPrint(RawMatrix);
            minified = minified.Replace("\r\n", "");

            MapleMatrix matrix = default;
            try
            {
                matrix = new MapleMatrix(minified);
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Matrix kunne ikke fortolkes. Vær sikker på du har kopieret fra maple");
                engine.Close();
                return default;
            }

            var methodType = typeof(JavaMapletInteractor).GetMethod(Method);
            var resultTask = (Task<JavaMapletGaussOutput>)methodType.Invoke(null, new object[] { engine, matrix });
            var TutorResult = await resultTask;
            engine.Close();

            MathBuilder = new MathMLBuilder(TutorResult.MathML);
            //MathBuilder.AddText("\nOpskriver Ligninger:\n");

            var MapleML = new MapleMathML(DefaultPath);
            MapleML.Open();

            var specialML = MathBuilder.ToString().Replace("<mo>&RightArrow;</mo>", string.Empty);
            var firstIndex = specialML.IndexOf("<mfenced", 0);
            specialML = specialML.Insert(firstIndex, "<mtext>&NewLine; Opstiller matrix &NewLine;</mtext>");
            var formatted = await MapleML.Import(specialML);

            // Remove empty brackets from the formatted output
            formatted = Regex.Replace(formatted, @"[\s+]+\[\s+\]", "");

            // Remove multiple newlines in formatted output
            formatted = Regex.Replace(formatted, @"\n+", "");

            // Close maple process and save the MathML output from MathBuilder (this is used when user wants to copy the text).
            MathML = MathBuilder.ToString();
            MapleML.Close();

            // Return the formatted result (Used for displaying in RTB)
            return formatted;
        }
    }
}
