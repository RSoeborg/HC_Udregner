using HC_Lib.Maple;
using HC_Lib.Sniffing;
using HC_Lib.Sniffing.Outputs.PcapNg;
using HC_Lib.Win;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace HC_Lib.JavaWin
{
    public sealed class JavaMapletInteractor
    {
        static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public async static Task<JavaMapletGaussOutput> GaussJordanEliminationTutor(MapleLinearAlgebra engine, MapleMatrix matrix)
        {
            return await MatrixTutorResult(engine, matrix, nameof(engine.GaussJordanEliminationTutor));
        }

        public async static Task<JavaMapletGaussOutput> GaussianEliminationTutor(MapleLinearAlgebra engine, MapleMatrix matrix)
        {
            return await MatrixTutorResult(engine, matrix, nameof(engine.GaussianEliminationTutor));
        }

        public async static Task<JavaMapletGaussOutput> InverseTutor(MapleLinearAlgebra engine, MapleMatrix matrix)
        {
            return await MatrixTutorResult(engine, matrix, nameof(engine.InverseTutor));
        }



        private async static Task<JavaMapletGaussOutput> MatrixTutorResult(MapleLinearAlgebra engine, MapleMatrix matrix, string methodName)
        {
            await semaphore.WaitAsync();//Since same protocol is used for every Tutor we have to wait for other tutor's to be closed before we can recall.
            try
            {
                var engineType = typeof(MapleLinearAlgebra);
                var method = engineType.GetMethod(methodName);//reflection used since each Tutor uses same protocol, for linearalgebra, (but different call methods to maple)
               
                IWindow window = await (Task<IWindow>)method.Invoke(engine, new object[] { matrix }); // await engine.GaussJordanEliminationTutor(matrix);   
                
                if (window is MSWindow) // Microsoft Windows
                {
                    // Find Loopback Pseudo interface for sniffing.
                    var nic = NetworkInterfaceInfo
                        .GetInterfaces()
                        .FirstOrDefault(c => c.Name.Contains("Loopback Pseudo"));

                    // Ensure loopback pseudo interface is found before we start trying to sniff.
                    if (nic != default(NetworkInterfaceInfo))
                    {
                        PcapNgFileOutput output;
                        SocketSniffer sniffer;

                        StartSniffing(nic, out output, out sniffer);
                        await InteractWithDefaultTutor((MSWindow)window, sniffer);
                        StopSniffing(output, sniffer);
                        return InterpretMatrixSniffedData();
                    }
                }
                
            } finally
            {
                semaphore.Release();
            }

            return null;
        }

        /// <summary>
        /// Creates an default interaction for data extraction with a default Java Maplet tutor using a Microsoft Window.
        /// </summary>
        /// <param name="window">The java maplet to interact with converted to a MSWindow (Microsoft Window)</param>
        /// <param name="sniffer">The sniffer used during capturing.</param>
        private static async Task InteractWithDefaultTutor(MSWindow window, SocketSniffer sniffer)
        {
            window.WindowPos(0, 0, 400, 800);

            for (int i = 0; i < 4; i++)
            {
                window.SendKeyStroke(System.Windows.Forms.Keys.Tab);
                await Task.Delay(60);
            }

            window.SendKeyStroke(System.Windows.Forms.Keys.Enter);
            window.Hide();
            long LastPackageCount = 0;
            int WaitTries = 0;
            while (true) // wait for program to stop sending packages to intercept.
            {
                await Task.Delay(400);
                LastPackageCount = sniffer.PacketsCaptured;
                if (LastPackageCount > 0 && LastPackageCount == sniffer.PacketsCaptured) WaitTries++;
                if (WaitTries > 4) break;
            }
            window.Close();
        }

        /// <summary>
        /// Interprets data from snifter.pcapng file captured with a default sniffer on a java maplet tutor window (assuming matrix related Tutor).
        /// </summary>
        /// <returns>An interpreted output in the form of JavaMplaetGaussOutput</returns>
        private static JavaMapletGaussOutput InterpretMatrixSniffedData()
        {
            List<string> operations = new List<string>();
            var gaussOutput = new JavaMapletGaussOutput();

            using (var reader = new StreamReader("snifter.pcapng"))
            {
                var content = reader.ReadToEnd();
                var regex = new Regex(@"\<application_communications.*?\<content\>Applied operation\:\ (.*?)\<\/content\>", RegexOptions.Singleline);
                var match = regex.Match(content);

                while (match.Success)
                {
                    var operation = match.Groups[1].Value.Trim();//initial space
                    operations.Add(operation);
                    match = match.NextMatch();
                }

                var mapleMatrixRegex = new Regex(@"\<content\>(\&lt\;.*?)\<", RegexOptions.Singleline);
                var mapleMatrixMatch = mapleMatrixRegex.Match(content);

                var lastMatchStr = "";
                while (mapleMatrixMatch.Success)
                {
                    lastMatchStr = mapleMatrixMatch.Groups[1].Value;
                    mapleMatrixMatch = mapleMatrixMatch.NextMatch();
                }

                StringBuilder builder = new StringBuilder(lastMatchStr);
                gaussOutput.Operations = operations.ToArray();

                int ra_index = 0;
                int index = 0;
                var search = "mtext&gt;&amp;NewLine;";
                while ((ra_index = builder.ToString().IndexOf(search, ra_index)) != -1)
                {
                    ra_index += search.Length;
                    if (index >= operations.Count) break;
                    builder.Insert(ra_index, $" {gaussOutput.OperationsDa[index++]} &amp;NewLine;&amp;NewLine;&amp;NewLine;");
                }


                gaussOutput.MathML = HttpUtility.HtmlDecode(builder.ToString());
            }
            return gaussOutput;
        }

        private static void StopSniffing(PcapNgFileOutput output, SocketSniffer sniffer)
        {
            sniffer.Stop();
            output.Dispose();
        }

        private static void StartSniffing(NetworkInterfaceInfo nic, out PcapNgFileOutput output, out SocketSniffer sniffer)
        {
            var appOptions = new AppOptions();
            appOptions.Parse(new string[] { "" });
            var filters = appOptions.BuildFilters();
            output = new PcapNgFileOutput(nic, appOptions.Filename);
            sniffer = new SocketSniffer(nic, filters, output);
            sniffer.Start();
        }
    }
}
