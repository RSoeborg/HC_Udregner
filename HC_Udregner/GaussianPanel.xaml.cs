using HC_Lib.JavaWin;
using HC_Udregner.Properties;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HC_Udregner
{
    /// <summary>
    /// Interaction logic for GaussianPanel.xaml
    /// </summary>
    public partial class GaussianPanel : UserControl
    {

        private string LastMathML;

        public GaussianPanel()
        {
            InitializeComponent();
        }

        private void btnCalcGauss_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                var senderButton = (Button)sender;
                var originalText = senderButton.Content;
                senderButton.Content = "Udregner ...";
                senderButton.IsEnabled = false;

                var parentWindow = (MainWindow)Window.GetWindow(this);
                parentWindow.Topmost = true;

                var matrix = rtbMatrix.Text;
                var maplet = new MapletOutput(Settings.Default.Path);
                Task.Run(async () =>
                {
                    var imported = await maplet.GaussJordanEliminationTutor(matrix);

                    rtbOutput.Dispatcher.Invoke(() =>
                    {
                        LastMathML = maplet.MathML;
                        rtbOutput.Document.Blocks.Clear();
                        rtbOutput.AppendText(imported);

                        parentWindow.Topmost = false;

                        senderButton.Content = originalText;
                        senderButton.IsEnabled = true;
                    });
                });
            }
        }

        private void CopyMapleButton_Click(object sender, RoutedEventArgs e)
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

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            rtbOutput.Document.Blocks.Clear();
            rtbOutput.AppendText("Maple Output");
        }
    }
}
