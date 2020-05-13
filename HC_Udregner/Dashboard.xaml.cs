using System.Windows;
using System.Windows.Controls;

namespace HC_Udregner
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : UserControl
    {
        private MainWindow parentWindow;

        public Dashboard()
        {
            InitializeComponent();
        }

        private void GaussJordanButton_Click(object sender, RoutedEventArgs e)
        {
            parentWindow = (MainWindow)Window.GetWindow(this);
            parentWindow.ShownPanel(parentWindow.GaussJordanPanel);
        }

        private void GaussianButton_Click(object sender, RoutedEventArgs e)
        {
            parentWindow = (MainWindow)Window.GetWindow(this);
            parentWindow.ShownPanel(parentWindow.GaussianPanel);
        }

        private void InversMatrixButton_Click(object sender, RoutedEventArgs e)
        {
            parentWindow = (MainWindow)Window.GetWindow(this);
            parentWindow.ShownPanel(parentWindow.InversMatrixPanel);
        }

        private void MatrixMultiplicationButton_Click(object sender, RoutedEventArgs e)
        {
            parentWindow = (MainWindow)Window.GetWindow(this);
            parentWindow.ShownPanel(parentWindow.MatrixMultiplicationPanel);
        }
    }
}
