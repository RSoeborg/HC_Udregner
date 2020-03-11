using HC_Lib.Maple;
using HC_Lib.MathML;
using HC_Udregner.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace HC_Udregner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            HideOtherPanels();
            UpdateMaplePath();
        }

        private void SelectMaple()
        {
            var ofd = new OpenFileDialog();
            var result = ofd.ShowDialog();
            if (result.HasValue && result.Value)
            {
                if (ofd.FileName.Contains("maplew.exe") || ofd.FileName.Contains("cmaple.exe"))
                {
                    var file = new FileInfo(ofd.FileName);
                    var cmaple = file.Name.Equals("cmaple.exe") ? file : file.Directory.GetFiles().FirstOrDefault(f => f.Name.Equals("cmaple.exe"));

                    if (cmaple != default(FileInfo))
                    {
                        Settings.Default.Path = cmaple.FullName;
                        Settings.Default.Save();
                        Settings.Default.Reload();
                        UpdateMaplePath();
                        return;
                    }
                }
                MessageBox.Show("Maple Command Line kunne ikke findes.");
            }
        }
        private void UpdateMaplePath()
        {
            if (!string.IsNullOrWhiteSpace(Settings.Default.Path))
            {
                FileInfo cmaple = new FileInfo(Settings.Default.Path);
                rtbMaplePath.Document.Blocks.Clear();
                rtbMaplePath.AppendText($"Path: {cmaple.Directory.Parent.FullName}");
            }
        }

        private void btnSelectMaple_Click(object sender, RoutedEventArgs e)
        {
            SelectMaple();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void maximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }

        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void HideDashboard(Border border)
        {
            Dashboard.Visibility = Visibility.Hidden;
            border.Visibility = Visibility.Visible;
        }

        private void GaussJordanButton_Click(object sender, RoutedEventArgs e)
        {
            HideDashboard(GaussJordanPanel);
            //GaussJordanBackButton.Visibility = Visibility.Visible;
        }

        private void GaussianButton_Click(object sender, RoutedEventArgs e)
        {
            HideDashboard(GaussianPanel);
        }

        public void HideOtherPanels()
        {
            Dashboard.Visibility = Visibility.Visible;
            GaussJordanPanel.Visibility = Visibility.Hidden;
            GaussianPanel.Visibility = Visibility.Hidden;
        }

        private void btnCalcGauss_Click(object sender, RoutedEventArgs e)
        {
            MathMLBuilder MathBuilder = default;
            var engine = new MapleLinearAlgebra(Settings.Default.Path);

            string gaussMatrixRaw = rtbMatrix.Text;
            if (gaussMatrixRaw.EndsWith(";"))
                gaussMatrixRaw = gaussMatrixRaw.Remove(gaussMatrixRaw.Length - 1, 1).Replace("\r\n", string.Empty);

            Task.Run(async () => {
                engine.Open();
                var minified = await engine.LPrint(gaussMatrixRaw);
                minified = minified.Replace("\r\n", "");

                MapleMatrix matrix = default;
                try
                {
                    matrix = new MapleMatrix(minified);
                }
                catch (ArgumentException)
                {
                    MessageBox.Show("Matrix kunne ikke fortolkes. Vær sikker på du har kopieret fra maple");
                    //rtOutput.Dispatcher.Invoke(() => {
                    //    btnTest.Content = "Udregn Matrix";
                    //    btnTest.IsEnabled = true;
                    //});
                    engine.Close();
                    return;
                }

                var TutorResult = await HC_Lib.JavaWin.JavaMapletInteractor.GaussJordanEliminationTutor(engine, matrix);
                engine.Close();

                MathBuilder = new MathMLBuilder(TutorResult.MathML);
                //MathBuilder.AddText("\nOpskriver Ligninger:\n");

                var MapleML = new MapleMathML(Settings.Default.Path);
                MapleML.Open();

                var specialML = MathBuilder.ToString().Replace("<mo>&RightArrow;</mo>", string.Empty);
                var firstIndex = specialML.IndexOf("<mfenced", 0);
                specialML = specialML.Insert(firstIndex, "<mtext>&NewLine; Opstiller matrix &NewLine;</mtext>");

                var imported = await MapleML.Import(specialML);

                // For copy: 
                //MathBuilder.ToString();

                rtbOutput.Dispatcher.Invoke(() => {
                    rtbOutput.Document.Blocks.Clear();
                    rtbOutput.AppendText( imported );
                });

                MapleML.Close();


            });


        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            rtbOutput.Document.Blocks.Clear();
            rtbOutput.AppendText("Maple Output");
        }
    }
}
