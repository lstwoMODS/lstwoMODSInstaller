using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace lstwoMODSInstaller.MVVM.View
{
    /// <summary>
    /// Interaction logic for CustomItemView.xaml
    /// </summary>
    public partial class CustomItemView : UserControl
    {
        public CustomItemView()
        {
            InitializeComponent();
        }

        private void DropZone_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0 && Path.GetExtension(files[0]).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    e.Effects = DragDropEffects.Copy;

                    var brush = new SolidColorBrush(Color.FromArgb(80, 0, 255, 50));
                    DropZone.Background = brush;
                }
                else
                {
                    e.Effects = DragDropEffects.None;

                    DropZone.Background = Brushes.Transparent;
                }
            }
        }

        private void DropZone_Drop(object sender, DragEventArgs e)
        {
            DropZone.Background = Brushes.Transparent;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string filePath in files)
                {
                    if (Path.GetExtension(filePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        HandleZipFile(filePath);
                        continue;
                    }

                    MessageBox.Show($"Invalid file: {filePath}\nOnly .zip files are supported.",
                                    "Invalid File", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void DropZone_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "ZIP Files (*.zip)|*.zip",
                Title = "Select a ZIP File",
                Multiselect = true,
            };

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            foreach (string file in openFileDialog.FileNames)
            {
                var extension = Path.GetExtension(file);

                if (extension.ToLower() != ".zip")
                {
                    MessageBox.Show($"Invalid file: {file}\nOnly .zip files are supported.",
                                     "Invalid File", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                HandleZipFile(file);
            }
        }

        private void HandleZipFile(string zipFile)
        {
            try
            {
                string tempDir = Path.Combine(Path.GetTempPath(), "lstwoMODSInstaller", Path.GetFileNameWithoutExtension(zipFile));

                try
                {
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                }
                catch { }
                

                Directory.CreateDirectory(tempDir);

                System.IO.Compression.ZipFile.ExtractToDirectory(zipFile, tempDir);

                string[] jsonFiles = Directory.GetFiles(tempDir, "data.json", SearchOption.AllDirectories);

                if (jsonFiles.Length == 0)
                {
                    MessageBox.Show($"No data.json found in {zipFile}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string dataJsonPath = jsonFiles[0];
                string dataJsonParentDir = Directory.GetParent(dataJsonPath).FullName;

                string customItemsDir = Path.Combine(DependencyManager.GetWobblyLifeFolder(), "CustomItems");
                Directory.CreateDirectory(customItemsDir);

                if (dataJsonParentDir == tempDir)
                {
                    string targetDir = Path.Combine(customItemsDir, Path.GetFileNameWithoutExtension(zipFile));
                    Directory.CreateDirectory(targetDir);
                    CopyDirectory(tempDir, targetDir);
                }
                else
                {
                    string targetDir = Path.Combine(customItemsDir, Path.GetFileName(dataJsonParentDir));
                    CopyDirectory(dataJsonParentDir, targetDir);
                }

                Directory.Delete(tempDir, true);

                MessageBox.Show($"Processed {zipFile} successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing {zipFile}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string targetFile = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, targetFile, true);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                string targetSubDir = Path.Combine(targetDir, Path.GetFileName(directory));
                CopyDirectory(directory, targetSubDir);
            }
        }

        private void DropZone_DragLeave(object sender, DragEventArgs e)
        {
            DropZone.Background = Brushes.Transparent;
        }
    }
}
