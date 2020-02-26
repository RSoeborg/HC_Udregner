using HC_Lib.Win;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HC_Lib.Maple
{
    public sealed class MapleLinearAlgebra : MapleEngine
    {
        public MapleLinearAlgebra(string MaplePath) : base(MaplePath) { }
        
        public new void Open()
        {

            base.Open();
            IncludePackage("LinearAlgebra");
            IncludePackage("Student[LinearAlgebra]");
        }
        
        public async Task<MapleMatrix> ReducedRowEchelonForm(MapleMatrix Matrix) {
            var @output = await LPrint($"ReducedRowEchelonForm({Matrix})"); // reduce matrix to echelon form and transform it with lprint
            return new MapleMatrix(output); // create maple matrix from lprint and return
        }

        private async Task<IWindow> Tutor(MapleMatrix matrix, string WinTitle, string Method)
        {
            var WinList = new MSWinList();
            var PrevWins = WinList.Windows.Where(win => win.Title.EndsWith(WinTitle));
            await Evaluate($"{Method}({matrix});", false);

            IWindow window = default;
            while (window == default(IWindow))
            {
                await Task.Delay(5);
                window = WinList.Windows.FirstOrDefault(c => c.Title.EndsWith(WinTitle) && !PrevWins.Contains(c));
            }

            if (window is MSWindow)//minimize instantly when window is idle
            {
                await Task.Delay(350);
                ((MSWindow)window).SetWindowState(MSWindow.SW.SHOWMINIMIZED);
            }
            else await Task.Delay(200);//wait for it to be loaded properly.
            return window;
        }
        
        public async Task<IWindow> GaussJordanEliminationTutor(MapleMatrix matrix)
        {
            return await Tutor(matrix, "Gauss-Jordan Elimination", "GaussJordanEliminationTutor");
        }
        public async Task<IWindow> GaussianEliminationTutor(MapleMatrix matrix)
        {
            return await Tutor(matrix, "Elimination", "GaussianEliminationTutor");
        }


        public async Task<MapleMatrix> AddRow(MapleMatrix Matrix, int i, int j, string factor)
        {
            var matrix = await Evaluate($"lprint(AddRow({Matrix}, {i}, {j}, {factor}));");
            return new MapleMatrix(matrix);
        }

        public async Task<MapleMatrix> RowOperation(MapleMatrix Matrix, int[] Rows, string Factor)
        {
            var matrix = await Evaluate($"lprint(RowOperation({Matrix}, [{string.Join(",", Rows)}], {Factor}));");
            return new MapleMatrix(matrix);
        }

        public async Task<MapleMatrix> RowOperation(MapleMatrix Matrix, int Row, string Factor)
        {
            var matrix = await Evaluate($"lprint(RowOperation({Matrix}, {Row}, {Factor}));");
            return new MapleMatrix(matrix);
        }
    }
}
