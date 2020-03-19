using HC_Udregner.Properties;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


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
            UpdateMaplePath();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        public void ShownPanel(UserControl userControl)
        {
            Dashboard.Visibility = Visibility.Collapsed;
            GaussJordanPanel.Visibility = Visibility.Collapsed;
            GaussianPanel.Visibility = Visibility.Collapsed;
            InversMatrixPanel.Visibility = Visibility.Collapsed;

            userControl.Visibility = Visibility.Visible;
        }

        #region Toolbar
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
        #endregion

        
    }
}
