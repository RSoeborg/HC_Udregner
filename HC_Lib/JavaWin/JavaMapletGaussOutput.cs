using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HC_Lib.JavaWin
{
    class JavaMapletGaussOutput
    {
        private bool HasChanged = true;
        private string[] UnchangedTranslation;

        // Swap rækkes 1 and 2

        public string[] OperationsDa {
            get {
                if (HasChanged)
                {
                    UnchangedTranslation = Operations.Select(r => r
                        .Replace("Add", "Adder")
                        .Replace("Multiply", "Multiplicer")
                        .Replace("Swap", "Byt rundt på")
                        .Replace("times", "gange")
                        .Replace("rows", "række")
                        .Replace("row", "række")
                        .Replace("and", "og")
                        .Replace("by", "med")
                        .Replace("to", "til")
                    ).ToArray();
                    HasChanged = false;
                }
                return UnchangedTranslation;
            }
        }

        private string[] _operations;
        public string[] Operations
        {
            get { return _operations; }
            set { _operations = value; HasChanged = true; }
        }
        public string MathML { get; set; }

        
    }
}
