using HC_Lib.JavaWin;
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
        private string LastMathML;

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
            GaussJordanBackButton.Visibility = Visibility.Visible;
        }
        private void GaussianButton_Click(object sender, RoutedEventArgs e)
        {
            HideDashboard(GaussianPanel);
        }
        private void HideOtherPanels()
        {
            Dashboard.Visibility = Visibility.Visible;
            GaussJordanPanel.Visibility = Visibility.Hidden;
            GaussianPanel.Visibility = Visibility.Hidden;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            HideOtherPanels();
        }
        private void btnCalcGauss_Click(object sender, RoutedEventArgs e)
        {
            var method = nameof(MapletOutput.GaussJordanEliminationTutor);
            if (sender is Button)
            {
                var senderButton = (Button)sender;
                var originalText = senderButton.Content;
                senderButton.Content = "Udregner ...";
                senderButton.IsEnabled = false;

                bool gaussian = senderButton.Uid.Equals("gaussian", StringComparison.CurrentCultureIgnoreCase);
                
                if (gaussian)
                    method = nameof(MapletOutput.GaussianEliminationTutor);
                
                var matrix = rtbMatrix.Text;
                var maplet = new MapletOutput(Settings.Default.Path);
                Task.Run(async () =>
                {
                    var imported = await (Task<string>)typeof(MapletOutput).GetMethod(method).Invoke(maplet, new object[] { matrix });

                    if (gaussian)
                    {
                        rtbGaussian.Dispatcher.Invoke(() =>
                        {
                            LastMathML = maplet.MathML;
                            rtbGaussian.Document.Blocks.Clear();
                            rtbGaussian.AppendText(imported);

                            senderButton.Content = originalText;
                            senderButton.IsEnabled = true;
                        });
                    }
                    else
                    {
                        rtbOutput.Dispatcher.Invoke(() =>
                        {
                            LastMathML = maplet.MathML;
                            rtbOutput.Document.Blocks.Clear();
                            rtbOutput.AppendText(imported);

                            senderButton.Content = originalText;
                            senderButton.IsEnabled = true;
                        });
                    }
                });
            }
        }
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            rtbOutput.Document.Blocks.Clear();
            rtbOutput.AppendText("Maple Output");
        }

        private void CopyMaple_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                var button = (Button)sender;
                var original = button.Content;
                button.IsEnabled = false;
                button.Content = "Kopieret";
                Clipboard.SetText(LastMathML);

                Task.Run(async () => {
                    await Task.Delay(5000);
                    button.Dispatcher.Invoke(() => {
                        button.IsEnabled = true;
                        button.Content = original;
                    });
                });
            
            }
        }
    }
}
