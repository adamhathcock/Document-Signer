using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using wSigner;

namespace wSignerUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Window.DataContext = ViewModel = new DocumentViewModel();
        }

        public DocumentViewModel ViewModel { get; set; }

        private void LayoutRoot_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Escape){
                Close();	
            }
        }

        private void Window_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }
            var filePaths = (e.Data.GetData(DataFormats.FileDrop)) as string[];
            if (filePaths == null || filePaths.Length == 0)
            {
                return;
            }
            e.Effects = DragDropEffects.Copy;
            ViewModel.DocsToSign = filePaths;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            var filePaths = (e.Data.GetData(DataFormats.FileDrop)) as string[];
            if (filePaths == null || filePaths.Length == 0)
            {
                return;
            }
            var cert = CertificateUtil.GetByDialog();
            if (cert == null)
            {
                return;
            }

            foreach (var filePath in filePaths)
            {
                var dir = Path.GetDirectoryName(filePath);
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var ext = Path.GetExtension(filePath);

                var outputPath = Path.Combine(dir, fileName + "-signed" + ext);
                var path = filePath;

                var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
                var signTask = Task.Factory.StartNew(() => DocumentSigner.For(ext).Sign(path, outputPath, cert));
                signTask.ContinueWith(task => MessageBox.Show(this, @"Error: " + task.Exception.InnerException.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error), CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, scheduler);
                signTask.ContinueWith(task => Process.Start(new ProcessStartInfo { FileName = outputPath, UseShellExecute = true }), TaskContinuationOptions.OnlyOnRanToCompletion);
                signTask.ContinueWith(task => ViewModel.DocsToSign = null, CancellationToken.None, TaskContinuationOptions.None, scheduler);
                if (filePaths.Length == 1)
                {
                    signTask.ContinueWith(task => Clipboard.SetText(outputPath, TextDataFormat.UnicodeText), CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, scheduler);
                }
            }
        }

        private void Window_DragLeave(object sender, DragEventArgs e)
        {
            ViewModel.DocsToSign = null;
        }
    }
}