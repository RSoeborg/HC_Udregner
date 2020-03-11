using HC_Lib.JavaWin;
using HC_Udregner.Properties;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HC_Udregner
{
    /// <summary>
    /// Interaction logic for GaussJordanPanel.xaml
    /// </summary>
    public partial class GaussJordanPanel : UserControl
    {

        private string LastMathML;

        public GaussJordanPanel()
        {
            InitializeComponent();
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

                var matrix = rtbMatrix.Text;
                var maplet = new MapletOutput(Settings.Default.Path);
                Task.Run(async () =>
                {
                    var imported = await (Task<string>)typeof(MapletOutput).GetMethod(method).Invoke(maplet, new object[] { matrix });

                    rtbOutput.Dispatcher.Invoke(() =>
                    {
                        LastMathML = maplet.MathML;
                        rtbOutput.Document.Blocks.Clear();
                        rtbOutput.AppendText(imported);

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
