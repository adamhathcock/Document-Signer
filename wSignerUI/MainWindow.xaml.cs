using System.Windows;

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
            Window.DataContext = ViewModel = new DocumentSignerViewModel();
        }

        public DocumentSignerViewModel ViewModel { get; set; }

        private void LayoutRoot_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();    
            }
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
            
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            var filePaths = (e.Data.GetData(DataFormats.FileDrop)) as string[];
            if (filePaths == null || filePaths.Length == 0)
            {
                return;
            }
            ViewModel.AddFiles(filePaths);
        }
    }
}