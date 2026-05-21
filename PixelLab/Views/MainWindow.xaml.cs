using System.Windows;
using System.Windows.Controls;
using PixelLab_Desktop.ViewModels;

namespace PixelLab_Desktop.Views
{
    public partial class MainWindow : Window
    {
        private bool _sidebarOpen = false;

        public MainWindow() => InitializeComponent();

        // ── Sidebar toggle ───────────────────────────────────────────────
        private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
        {
            _sidebarOpen = !_sidebarOpen;

            if (_sidebarOpen)
            {
                SidebarColumn.Width = new GridLength(330);
                Sidebar.Visibility  = Visibility.Visible;
                BtnToggleSidebar.Content = "◀ Color Spaces";
            }
            else
            {
                SidebarColumn.Width = new GridLength(0);
                Sidebar.Visibility  = Visibility.Collapsed;
                BtnToggleSidebar.Content = "▶ Color Spaces";
            }
        }

        // ── Color Controls popup ─────────────────────────────────────────
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

        // ── Drag & Drop ──────────────────────────────────────────────────
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
