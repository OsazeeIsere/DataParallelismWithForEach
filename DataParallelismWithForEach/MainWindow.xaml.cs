using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace DataParallelismWithForEach
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //new window level variable
        private CancellationTokenSource cancelToken = new CancellationTokenSource();

        public MainWindow()
        {
            InitializeComponent();
        }
        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            cancelToken.Cancel();
        }

        private void cmdProcess_Click(object sender, RoutedEventArgs e)
        {
            // Start a new "task" to process the files. 
            Task.Factory.StartNew(() => ProcessFiles());
           // ProcessFiles();
        }

        private void ProcessFiles()
        {
           // Use ParallelOptions instance to store the CancellationToken
           ParallelOptions parOpts = new ParallelOptions
           {
               CancellationToken = cancelToken.Token,
               MaxDegreeOfParallelism = System.Environment.ProcessorCount
           };

            // Load up all *.jpg files, and make a new folder for the modified data.
            string[] files = Directory.GetFiles(@".\TestPictures", "*.jpg",
                SearchOption.AllDirectories);
            string newDir = @".\ModifiedPictures";
            Directory.CreateDirectory(newDir);

            try
            {
                //  Process the image data in a parallel manner! 
                Parallel.ForEach(files, parOpts, currentFile =>
                {
                    parOpts.CancellationToken.ThrowIfCancellationRequested();

                    string filename = Path.GetFileName(currentFile);
                    using (Bitmap bitmap = new Bitmap(currentFile))
                    {
                        bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        bitmap.Save(Path.Combine(newDir, filename));

                        //this.Title = $"Processing {filename} on thread {Thread.CurrentThread.ManagedThreadId}";

                        // We need to ensure that the secondary threads access controls
                        // created on primary thread in a safe manner.
                        this.Dispatcher.Invoke((Action)delegate
                        {
                            this.Title =
                                $"Processing {filename} on thread {Thread.CurrentThread.ManagedThreadId}";
                        });
                    }
                }
                );
                this.Dispatcher.Invoke((Action)delegate { this.Title = "Done!"; });
            }
            catch (OperationCanceledException ex)
            {
                this.Dispatcher.Invoke((Action)delegate { this.Title = ex.Message; });
            }


            // prpcess the data in a parallel manner
            //    Parallel.ForEach(files, currentFile =>
            //     {
            //         string filename = Path.GetFileName(currentFile);
            //         using (Bitmap bitmap = new Bitmap(currentFile))
            //         {
            //             bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
            //             bitmap.Save(Path.Combine(newDir, filename));
            //             this.Dispatcher.Invoke((Action)delegate
            //             {
            //             //print out the id of the thread processing the current image
            //             this.Title = $"Processing {filename} on thread {Thread.CurrentThread.ManagedThreadId}";

            //             }
            //          );
            //         }
            //     }
            //);
            //foreach (string currentFile in files)
            //{
            //    string filename = Path.GetDirectoryName(currentFile);
            //    using (Bitmap bitmap = new Bitmap(currentFile))
            //    {
            //        bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
            //        bitmap.Save(Path.Combine(newDir, filename));

            //        //print out the id of the thread processing the current image
            //        this.Title = $"Processing {filename} on thread {Thread.CurrentThread.ManagedThreadId}";

            //    }
            //}



        }
    }
}
