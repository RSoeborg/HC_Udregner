using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HC_Lib.Maple
{
    public class MapleEngine
    {
        private readonly string MaplePath;
        private Process MapleProcess;

        private StringBuilder InputData = new StringBuilder();
        private readonly object InputMutex = new object();

        public MapleEngine(string MaplePath)
        {
            this.MaplePath = MaplePath;
        }

        public void Open()
        {
            MapleProcess = new Process();
            // Redirect the output stream of the child process.
            MapleProcess.StartInfo.UseShellExecute = false;
            MapleProcess.StartInfo.RedirectStandardOutput = true;
            MapleProcess.StartInfo.RedirectStandardInput = true;
            MapleProcess.StartInfo.CreateNoWindow = true;
            MapleProcess.StartInfo.FileName = MaplePath;
            MapleProcess.StartInfo.Arguments = "-q -u";
            MapleProcess.OutputDataReceived += (s, e) => { WriteToData(e.Data); };
            MapleProcess.Start();

            MapleProcess.BeginOutputReadLine();
        }

        public async Task<string> Simplify(string Expression)
        {
            return await LPrint($"simplify({Expression})");
        }
        public async Task<string> LPrint(string Expression)
        {
            return await Evaluate($"lprint({Expression});");
        }

        public async void IncludePackage(string PackageName)
        {
            await Evaluate($"with({PackageName}):", false);
        }

     
        public void Close()
        {
            if (MapleProcess != null)
            {
                MapleProcess.Kill();
            }
        }

        protected async Task<string> Evaluate(string Expression, bool HasData = true)
        {
            ClearBufferedData();
            MapleProcess.StandardInput.WriteLine($"{Expression}");
            if (!HasData) return "";
            await WaitForData();
            return GetOutput();
        }

        private void ClearBufferedData()
        {
            lock (InputMutex)
            {
                InputData.Clear();
            }
        }
        private string GetOutput()
        {
            lock (InputMutex)
            {
                string output = InputData.ToString();
                InputData.Clear();
                return output;
            }
        }
        private async Task WaitForData(int lines = 1)
        {
            while (InputData.Length == 0) { }

            
            int originalLength = InputData.Length;
            int timesEqual = 0;
            while (true)
            {
                await Task.Delay(200);
                if (originalLength != InputData.Length) { originalLength = InputData.Length; continue; }

                timesEqual++;

                if (timesEqual > 5)
                {
                    break;
                }
            }
        }
        private async void SkipData(int lines = 1)
        {
            for (int i = 0; i < lines; i++)
            {
                await WaitForData();
            }
            ClearBufferedData();
        }
        private void WriteToData(string Data)
        {
            lock (InputMutex)
            {
                InputData.AppendLine(Data);
            }
        }
    }
}
