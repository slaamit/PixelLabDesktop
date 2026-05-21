using System.Windows;

using PixelLab_Desktop.ViewModels;

namespace PixelLab_Desktop.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow() => InitializeComponent();

        // فتح نافذة التحكم المتقدم
        private void OpenColorControlsWindow_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as MainViewModel;
            if (vm != null)
            {
                var win = new ColorControlsWindow(vm);
                win.Owner = this;
                win.Show();
            }
        }

        // معالج سحب وإفلات الصورة (Drag & Drop)
        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    var vm = this.DataContext as MainViewModel;
                    vm?.LoadImageFromPath(files[0]);
                }
            }
        }
    }
}