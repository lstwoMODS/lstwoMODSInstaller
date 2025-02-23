using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace lstwoMODSInstaller.MVVM.View
{
    /// <summary>
    /// Interaction logic for ManageInstallView.xaml
    /// </summary>
    public partial class ManageInstallView : UserControl
    {
        public ManageInstallView()
        {
            InitializeComponent();
        }

        private void OpenGameDirectory_Click(object sender, RoutedEventArgs e)
        {
            var gamePath = MainWindow.SelectedGame?.GetGamePath();

            if (MainWindow.SelectedGame == null || string.IsNullOrEmpty(gamePath) || !Directory.Exists(gamePath))
            {
                return;
            }

            var psi = new ProcessStartInfo()
            {
                FileName = gamePath,
                UseShellExecute = true
            };

            Process.Start(psi);
        }

        private void DeleteKeybinds_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("This will permanently delete all saved keybinds for this game!", "WARNING", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
            {
                return;
            }

            var gamePath = MainWindow.SelectedGame?.GetGamePath();

            if (MainWindow.SelectedGame == null || string.IsNullOrEmpty(gamePath) || !Directory.Exists(gamePath) || !Directory.Exists($"{gamePath}/lstwoMODS") || 
                !File.Exists($"{gamePath}/lstwoMODS/keybinds.json"))
            {
                return;
            }

            File.Delete($"{gamePath}/lstwoMODS/keybinds.json");
        }

        private void DeleteBepInEx_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("This will permanently delete ALL installed mods for this game!", "WARNING", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
            {
                return;
            }

            var gamePath = MainWindow.SelectedGame?.GetGamePath();

            if (MainWindow.SelectedGame == null || string.IsNullOrEmpty(gamePath) || !Directory.Exists(gamePath) || !Directory.Exists($"{gamePath}/BepInEx"))
            {
                return;
            }

            Directory.Delete($"{gamePath}/BepInEx", true);
        }
    }
}
