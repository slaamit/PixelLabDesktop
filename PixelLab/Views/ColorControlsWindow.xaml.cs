using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using PixelLab_Desktop.ViewModels;

namespace PixelLab_Desktop.Views
{
    /// <summary>
    /// this class would init the WPF view , but we have it connected via ICommands so we don't have to deal with the values here . 
    /// and the ICommands are called via WPF and in the XAML file .
    /// </summary>
    public partial class ColorControlsWindow : Window
    {
        public ColorControlsWindow(MainViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }
    }
}