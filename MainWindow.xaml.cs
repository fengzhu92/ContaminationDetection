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
using System.Windows.Threading;
using TensorFlow;

namespace CheckContamination
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string model = "model.pb";
        public MainWindow ()
        {
            InitializeComponent ();
        }

        private void BrowseButton_Click ( object sender, RoutedEventArgs e )
        {
            Dispatcher.BeginInvoke ( new Action ( () =>
            {
                BitmapImage bitmap = new BitmapImage ();
                bitmap.BeginInit ();
                bitmap.UriSource = new Uri ( "pack://application:,,,/empty.jpg" );
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.EndInit ();
                bitmap.Freeze ();
                viewer.Source = bitmap;
                viewer.UpdateLayout ();
                imageTB.Clear ();
                resultTB.Text = "";
            } ), DispatcherPriority.ContextIdle, null );

            OpenFileDialog openFileDialog = new OpenFileDialog ();
            if ( openFileDialog.ShowDialog () == true )
            {
                imageTB.Text = openFileDialog.FileName;

                BitmapImage bitmap = new BitmapImage ();
                bitmap.BeginInit ();
                bitmap.UriSource = new Uri ( imageTB.Text );
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.EndInit ();
                bitmap.Freeze ();
                viewer.Source = bitmap;
                viewer.UpdateLayout ();
            }   
        }

        private void Run_Click ( object sender, RoutedEventArgs e )
        {
            byte[] buffer = System.IO.File.ReadAllBytes ( model );
            using ( var graph = new TensorFlow.TFGraph () )
            {
                graph.Import ( buffer );
                using ( var session = new TensorFlow.TFSession ( graph ) )
                {
                    var file = imageTB.Text; 
                    var runner = session.GetRunner ();
                    graph.GetEnumerator ();
                    var tensor = Utils.ImageToTensor ( file );
                    runner.AddInput ( graph["img"][0], tensor );   
                    runner.Fetch ( graph["dense_3/Softmax"][0] );

                    var output = runner.Run ();
                    var vecResults = output[0].GetValue ();
                    float[,] results = (float[,]) vecResults;

                    /// Evaluate the results
                    int[] quantized = Utils.Quantized ( results );
                    resultTB.Text = ( quantized[0] == 1 ) ? "Not Contaminated" : "Contaminated";
                }
            }
           
        }
    }
}
