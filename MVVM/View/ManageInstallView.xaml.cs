using System;
using lstwoMODSInstaller.ModManagement;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.Win32;

namespace lstwoMODSInstaller.MVVM.View
{
    /// <summary>
    /// Interaction logic for ManageInstallView.xaml
    /// </summary>
    public partial class ManageInstallView : UserControl
    {
        private static Game currentGame;

        public ManageInstallView()
        {
            InitializeComponent();

            MainWindow.OnSelectedGameChanged += RefreshModList;
            RefreshModList();
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

        private void ToggleModActive_Click(object sender, RoutedEventArgs e)
        {
            if (ModList.SelectedItem is ModFile selectedMod)
            {
                selectedMod.Enabled = !selectedMod.Enabled;
            }

            RefreshModList();
        }

        private void AddModBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.SelectedGame == null || string.IsNullOrEmpty(MainWindow.SelectedGame.GetGamePath()))
            {
                return;
            }
            
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select BepInEx Plugin DLLs",
                Filter = "DLL Files (*.dll)|*.dll",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }
            
            foreach (var filePath in openFileDialog.FileNames)
            {
                File.Copy(filePath, $"{MainWindow.SelectedGame.GetGamePath()}/BepInEx/plugins/{Path.GetFileName(filePath)}");
            }
            
            RefreshModList();
        }

        private void DeleteModBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedMod = ModList.SelectedItem as ModFile;

            if (selectedMod == null)
            {
                return;
            }

            if (MessageBox.Show($"Are you sure you want to delete \"{selectedMod}\"?\n",
                    "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning) !=
                MessageBoxResult.Yes)
            {
                return;
            }
            
            if (File.Exists(selectedMod.Path))
            {
                File.Delete(selectedMod.Path);
            }
            
            RefreshModList();
        }

        private void RefreshModList()
        {
            currentGame = MainWindow.SelectedGame;

            if (currentGame == null)
            {
                ModList.ItemsSource = new List<ModFile>();
                return;
            }
            
            var gameMods = GetModFiles(currentGame);
            ModList.ItemsSource = gameMods;
        }

        private static List<ModFile> GetModFiles(Game game)
        {
            return ScanModFilesInDirectory($"{game.GetGamePath()}/BepInEx/plugins");
        }

        private static List<ModFile> ScanModFilesInDirectory(string path)
        {
            var modFiles = new List<ModFile>();

            foreach (var file in Directory.GetFiles(path))
            {
                var extension = Path.GetExtension(file);

                if (extension != ".dll" && !file.EndsWith(".dll.disabled"))
                {
                    continue;
                }

                modFiles.Add(new() { Path = file });
            }

            foreach (var dir in Directory.GetDirectories(path))
            {
                modFiles.AddRange(ScanModFilesInDirectory(dir));
            }

            return modFiles;
        }

        private class ModFile
        {
            public string Path;

            public bool Enabled
            {
                get => !Path.EndsWith(".disabled");
                set 
                {
                    if (Enabled == value || !File.Exists(Path))
                    {
                        return;
                    }
                    
                    var newPath = value ? Path[..^9] : $"{Path}.disabled";
                    File.Move(Path, newPath);
                    Path = newPath;
                }
            }

            public override string ToString()
            {
                var fileName = System.IO.Path.GetFileNameWithoutExtension(Path);

                if (fileName != null && fileName.EndsWith(".dll"))
                {
                    fileName = fileName[..^4];
                }

                if (!Enabled)
                {
                    fileName = $"(DISABLED) {fileName}";
                }
                
                return fileName;
            }
        }
    }
}
