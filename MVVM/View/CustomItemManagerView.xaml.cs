using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO.Compression;
using lstwoMODSInstaller.ModManagement;

namespace lstwoMODSInstaller.MVVM.View
{
    /// <summary>
    /// Interaction logic for CustomItemManagerView.xaml
    /// </summary>
    public partial class CustomItemManagerView : UserControl
    {
        private CustomItemPackData Data
        {
            get
            {
                return selectedData;
            }
            set
            {
                selectedData = value;
                newSelectedData = value;

                IsUnsaved = false;
            }
        }

        private CustomItemPackData NewData
        {
            get
            {
                return newSelectedData;
            }
            set
            {
                newSelectedData = value;
                IsUnsaved = true;
            }
        }

        private CustomItemPackData selectedData;
        private CustomItemPackData newSelectedData;
        private bool IsUnsaved = false;

        private readonly string customItemsFolder = DependencyManager.GetWobblyLifeFolder() + "/CustomItems/";

        private List<string> assetBundles = new();
        private List<string> assemblies = new();
        private List<string> itemPacks = new();

        public CustomItemManagerView()
        {
            InitializeComponent();

            var packs = GetAllItemPacks();

            foreach(var pack in packs)
            {
                itemPacks.Add(pack.folderName);
            }

            UpdateItemPackList();
        }

        private void PackListNewButton_Click(object sender, RoutedEventArgs e)
        {
            NewItemPack();
        }

        private void PackListDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedItemPack();
        }


        private void PackList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(PackList.SelectedItem == null)
            {
                return;
            }

            if(IsUnsaved)
            {
                var result = MessageBox.Show("You have unsaved changes! Do you want to save them?", "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning,
                    MessageBoxResult.Yes);

                if (result == MessageBoxResult.Yes)
                {
                    SaveData();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            var pack = PackList.SelectedItem != null ? GetItemPackFromFolder(customItemsFolder + PackList.SelectedItem.ToString()) : new();

            SetUIData(pack.Value);
            Data = pack.Value;
        }


        private void AssetBundlesNewButton_Click(object sender, RoutedEventArgs e)
        {
            AddAssetBundles();
        }

        private void AssetBundlesDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectedAssetBundle();
        }


        private void AssembliesNewButton_Click(object sender, RoutedEventArgs e)
        {
            AddAssemblies();
        }

        private void AssembliesDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedAssembly();
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveData();
            IsUnsaved = false;
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("This will get rid of all unsaved changes. Do you want to continue?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question,
                MessageBoxResult.No);

            if (result == MessageBoxResult.No)
            {
                return;
            }

            NewData = Data;
            IsUnsaved = false;

            SetUIData(Data);
        }

        private void SaveAndExportButton_Click(object sender, RoutedEventArgs e)
        {
            SaveData();
            IsUnsaved = false;

            ExportSelectedItemPack();
        }


        private void FolderNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var data = NewData;
            data.folderName = FolderNameTextBox.Text;
            NewData = data;
        }

        private void PackNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var data = NewData;
            data.name = PackNameTextBox.Text;
            NewData = data;
        }

        private void PackAuthorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var data = NewData;
            data.author = PackAuthorTextBox.Text;
            NewData = data;
        }


        private void PackListContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            var hasSelected = PackList.SelectedItem != null;

            PackListContext_Delete.IsEnabled = hasSelected;
            PackListContext_Export.IsEnabled = hasSelected;
        }

        private void PackList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var r = VisualTreeHelper.HitTest(this, e.GetPosition(this));

            if (r.VisualHit.GetType() != typeof(ListBoxItem))
            {
                PackList.UnselectAll();
            }
        }

        private void PackListContext_New_Click(object sender, RoutedEventArgs e)
        {
            NewItemPack();
        }

        private void PackListContext_Delete_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedItemPack();
        }

        private void PackListContext_Export_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = PackList.SelectedItem.ToString();

            if (selectedItem == Data.folderName)
            {
                SaveData();
                IsUnsaved = false;
            }

            if (selectedItem == null)
            {
                return;
            }

            var pack = GetItemPackFromFolder(customItemsFolder + selectedItem);

            ExportItemPack(pack.Value.folderName);
        }

        private void PackListContext_FileExplorer_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", Path.GetFullPath($"{customItemsFolder}/{PackList.SelectedItem}/"));
        }


        private void AssembliesContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            var hasSelected = AssembliesList.SelectedItem != null;

            AssembliesContextMenu_Delete.IsEnabled = hasSelected;
            AssembliesContextMenu_Explorer.IsEnabled = hasSelected;
        }

        private void AssembliesContextMenu_Add_Click(object sender, RoutedEventArgs e)
        {
            AddAssemblies();
        }

        private void AssembliesContextMenu_Delete_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedAssembly();
        }

        private void AssembliesContextMenu_Explorer_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "/select, \"" + Path.GetFullPath($"{customItemsFolder}/{Data.folderName}/{AssembliesList.SelectedItem}") + "\"");
        }


        private void AssetBundlesContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            var hasSelected = AssetBundlesList.SelectedItem != null;

            AssetBundlesContextMenu_Delete.IsEnabled = hasSelected;
            AssetBundlesContextMenu_Explorer.IsEnabled = hasSelected;
        }

        private void AssetBundlesContextMenu_Add_Click(object sender, RoutedEventArgs e)
        {
            AddAssetBundles();
        }

        private void AssetBundlesContextMenu_Delete_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectedAssetBundle();
        }

        private void AssetBundlesContextMenu_Explorer_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "/select, \"" + Path.GetFullPath($"{customItemsFolder}/{Data.folderName}/{AssetBundlesList.SelectedItem}") + "\"");
        }


        private void AssetBundlesList_Drop(object sender, DragEventArgs e)
        {
            AssetBundlesList.Background = Brushes.White;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string filePath in files)
                {
                    AddAssetBundle(filePath);
                }
            }
        }

        private void AssetBundlesList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files != null && files.Length > 0)
                {
                    e.Effects = DragDropEffects.Copy;

                    var brush = new SolidColorBrush(Color.FromArgb(255, 0, 210, 50));
                    AssetBundlesList.Background = brush;
                }
                else
                {
                    e.Effects = DragDropEffects.None;

                    AssetBundlesList.Background = Brushes.White;
                }
            }
        }

        private void AssetBundlesList_DragLeave(object sender, DragEventArgs e)
        {
            AssetBundlesList.Background = Brushes.White;
        }


        private void AssembliesList_Drop(object sender, DragEventArgs e)
        {
            AssembliesList.Background = Brushes.White;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string filePath in files)
                {
                    if(Path.GetExtension(filePath).ToLower() == ".dll")
                    {
                        AddAssembly(filePath);
                        continue;
                    }

                    MessageBox.Show($"Invalid file: {filePath}\nOnly .dll files are supported.", "Invalid File", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void AssembliesList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files != null && files.Length > 0)
                {
                    e.Effects = DragDropEffects.Copy;

                    var brush = new SolidColorBrush(Color.FromArgb(255, 0, 210, 50));
                    AssembliesList.Background = brush;
                }
                else
                {
                    e.Effects = DragDropEffects.None;

                    AssembliesList.Background = Brushes.White;
                }
            }
        }

        private void AssembliesList_DragLeave(object sender, DragEventArgs e)
        {
            AssembliesList.Background = Brushes.White;
        }


        private void SetUIData(CustomItemPackData data)
        {
            FolderNameTextBox.Text = data.folderName;

            PackNameTextBox.Text = data.name == null ? "" : data.name;
            PackAuthorTextBox.Text = data.author == null ? "" : data.author;

            AssetBundlesList.Items.Clear();
            assetBundles = data.assetPaths == null ? new() : data.assetPaths.ToList();

            if(data.assetPath != null)
            {
                assetBundles.Add(data.assetPath);
            }

            foreach(var path in assetBundles)
            {
                AssetBundlesList.Items.Add(path);
            }

            AssembliesList.Items.Clear();
            assemblies = data.assemblyPaths == null ? new() : data.assemblyPaths.ToList();

            if (data.assemblyPath != null)
            {
                assemblies.Add(data.assemblyPath);
            }

            foreach (var path in assemblies)
            {
                AssembliesList.Items.Add(path);
            }
        }

        private void SaveData()
        {
            var data = NewData;

            var serializeable = new SerializeableCustomItemPackData
            {
                name = data.name ?? "",
                author = data.author ?? "",
                assetPaths = data.assetPaths,
                assetPath = data.assetPath,
                assemblyPaths = data.assemblyPaths,
                assemblyPath = data.assemblyPath
            };

            var newDataFolderPath = $"{customItemsFolder}/{NewData.folderName}/";
            var oldDataFolderPath = $"{customItemsFolder}/{Data.folderName}/";

            if (!Directory.Exists(newDataFolderPath))
            {
                if (Directory.Exists(oldDataFolderPath))
                {
                    Directory.Move(oldDataFolderPath, newDataFolderPath);
                }
                else
                {
                    Directory.CreateDirectory(newDataFolderPath);
                }
            }

            var path = newDataFolderPath + "/data.json";
            var json = JsonConvert.SerializeObject(serializeable, Formatting.Indented);

            File.WriteAllText(path, json);

            IsUnsaved = false;

            itemPacks.Clear();
            var packs = GetAllItemPacks();

            foreach (var pack in packs)
            {
                itemPacks.Add(pack.folderName);
            }

            var newData = NewData;
            newData.savedFolderPath = newDataFolderPath;
            NewData = newData;
            Data = NewData;

            UpdateItemPackList();
        }

        private void UpdateItemPackList()
        {
            PackList.Items.Clear();

            foreach(var pack in itemPacks)
            {
                PackList.Items.Add(pack);
            }
        }

        private CustomItemPackData[] GetAllItemPacks()
        {
            if(!Directory.Exists(customItemsFolder))
            {
                Directory.CreateDirectory(customItemsFolder);
            }

            var packs = new List<CustomItemPackData>();
            var packDirs = Directory.GetDirectories(customItemsFolder);

            foreach(var dir in packDirs)
            {
                if(!Directory.GetFiles(dir).Where(path => path.EndsWith("data.json")).Any())
                {
                    continue;
                }

                var pack = GetItemPackFromFolder(dir);

                if(pack.HasValue)
                {
                    packs.Add(pack.Value);
                }
            }

            return packs.ToArray();
        }

        private CustomItemPackData? GetItemPackFromFolder(string folder)
        {
            var dataJson = Directory.GetFiles(folder).Where(path => path.EndsWith("data.json")).First();

            if (dataJson == null)
            {
                return null;
            }

            var json = File.ReadAllText(dataJson);
            var pack = JsonConvert.DeserializeObject<CustomItemPackData>(json);

            pack.savedFolderPath = folder;
            pack.folderName = folder.Split('/').Last();

            return pack;
        }

        private void NewItemPack()
        {
            var data = new CustomItemPackData();
            data.folderName = "New Custom Item Pack";
            Data = data;

            SetUIData(Data);

            FolderNameTextBox.SelectAll();
            FolderNameTextBox.Focus();
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

        private void DeleteSelectedItemPack()
        {
            var pack = GetItemPackFromFolder(customItemsFolder + PackList.SelectedItem.ToString());

            if (pack == null)
            {
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete " +
                $"\"{(pack.Value.name != null && pack.Value.name != "" ? pack.Value.name : pack.Value.folderName)}\"" +
                $"{((pack.Value.author != null && pack.Value.author != "") ? $"by \"{pack.Value.author}\"" : "")}",
                "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

            if (result == MessageBoxResult.No || !Directory.Exists(pack.Value.savedFolderPath) || !pack.Value.savedFolderPath.StartsWith(customItemsFolder))
            {
                return;
            }

            Directory.Delete(pack.Value.savedFolderPath, true);

            itemPacks.Clear();
            var packs = GetAllItemPacks();

            foreach (var _pack in packs)
            {
                itemPacks.Add(_pack.folderName);
            }

            UpdateItemPackList();
        }

        private void AddAssetBundles()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select an Asset Bundle File",
                Multiselect = true,
            };

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            foreach (string file in openFileDialog.FileNames)
            {
                AddAssetBundle(file);
            }

            SaveData();
            IsUnsaved = false;
        }

        private void AddAssetBundle(string file)
        {
            if (Path.GetFileName(file) == "data.json" || Data.folderName == null || Data.folderName == "")
            {
                return;
            }

            var fileName = Path.GetFileName(file);
            var packPath = $"{customItemsFolder}/{Data.folderName}/";
            var filePath = $"{packPath}/{fileName}";

            if (!Directory.Exists(packPath))
            {
                Directory.CreateDirectory(packPath);
            }

            File.Copy(file, filePath, true);

            var data = NewData;

            if (!assetBundles.Contains(fileName))
            {
                assetBundles.Add(fileName);

                var assetPathsList = data.assetPaths == null ? new List<string>() : data.assetPaths.ToList();
                assetPathsList.Add(fileName);
                data.assetPaths = assetPathsList.ToArray();
            }

            NewData = data;

            SetUIData(NewData);
        }

        private void RemoveSelectedAssetBundle()
        {
            if (AssetBundlesList.SelectedItem == null)
            {
                return;
            }

            var fileName = (string)AssetBundlesList.SelectedItem;

            var result = MessageBox.Show($"Are you sure you want to delete {fileName}? You cannot undo this action!",
                "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

            if (result == MessageBoxResult.No)
            {
                return;
            }

            var packPath = $"{customItemsFolder}/{Data.folderName}/";
            var filePath = $"{packPath}/{fileName}";

            if (!Directory.Exists(packPath) || !packPath.StartsWith(customItemsFolder))
            {
                return;
            }

            File.Delete(filePath);

            assetBundles.Remove(fileName);

            var data = NewData;

            if (data.assetPaths == null)
            {
                return;
            }

            var assetPathsList = data.assetPaths.ToList();
            assetPathsList.Remove(fileName);
            data.assetPaths = assetPathsList.ToArray();
            NewData = data;

            SetUIData(NewData);
            SaveData();
            IsUnsaved = false;
        }

        private void AddAssemblies()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Assembly Files (*.dll)|*.dll",
                Title = "Select an Assembly File",
                Multiselect = true,
            };

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            foreach (string file in openFileDialog.FileNames)
            {
                AddAssembly(file);
            }

            SaveData();
            IsUnsaved = false;
        }

        private void AddAssembly(string file)
        {
            if (Data.folderName == null || Data.folderName == "")
            {
                return;
            }

            var fileName = Path.GetFileName(file);
            var packPath = $"{customItemsFolder}/{Data.folderName}/";
            var filePath = $"{packPath}/{fileName}";
            var extension = Path.GetExtension(file);

            if (extension.ToLower() != ".dll")
            {
                MessageBox.Show($"Can't add file \"{fileName}\" because it is not a dll!", "Can't add file!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Directory.Exists(packPath))
            {
                Directory.CreateDirectory(packPath);
            }

            File.Copy(file, filePath, true);

            var data = NewData;

            if (!assemblies.Contains(fileName))
            {
                assemblies.Add(fileName);

                var assemblyPathsList = data.assemblyPaths == null ? new List<string>() : data.assemblyPaths.ToList();
                assemblyPathsList.Add(fileName);
                data.assemblyPaths = assemblyPathsList.ToArray();
            }

            NewData = data;

            SetUIData(NewData);
        }

        private void DeleteSelectedAssembly()
        {
            if (AssembliesList.SelectedItem == null)
            {
                return;
            }

            var fileName = (string)AssembliesList.SelectedItem;

            var result = MessageBox.Show($"Are you sure you want to delete {fileName}? You cannot undo this action!",
                "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

            if (result == MessageBoxResult.No)
            {
                return;
            }

            var packPath = $"{customItemsFolder}/{Data.folderName}/";
            var filePath = $"{packPath}/{fileName}";

            if (!Directory.Exists(packPath) || !packPath.StartsWith(customItemsFolder))
            {
                return;
            }

            File.Delete(filePath);

            assemblies.Remove(fileName);

            var data = NewData;

            if (data.assetPaths == null)
            {
                return;
            }

            var assetPathsList = data.assemblyPaths.ToList();
            assetPathsList.Remove(fileName);
            data.assemblyPaths = assetPathsList.ToArray();
            NewData = data;

            SetUIData(NewData);
            SaveData();
            IsUnsaved = false;
        }

        private void ExportSelectedItemPack()
        {
            ExportItemPack(Data.folderName);
        }

        private void ExportItemPack(string folderName)
        {
            var dialog = new SaveFileDialog()
            {
                Filter = "ZIP Files (*.zip)|*.zip",
                Title = "Save Item Pack ZIP File"
            };

            if (dialog.ShowDialog() == false)
            {
                return;
            }

            var exportPath = dialog.FileName;

            var tempFolderPath = Path.GetTempPath() + "/lstwoMODS/";
            var tempExportPath = tempFolderPath + Path.GetFileName(exportPath);
            var tempPackFolderPath = tempFolderPath + folderName + "/";

            var folderPath = $"{customItemsFolder}/{folderName}/";

            if (File.Exists(exportPath))
            {
                File.Delete(exportPath);
            }

            if (!Directory.Exists(tempFolderPath))
            {
                Directory.CreateDirectory(tempFolderPath);
            }

            if (File.Exists(tempExportPath))
            {
                File.Delete(tempExportPath);
            }

            if (Directory.Exists(tempPackFolderPath))
            {
                foreach (var file in Directory.GetFiles(tempPackFolderPath))
                {
                    File.Delete(file);
                }
            }
            else
            {
                Directory.CreateDirectory(tempPackFolderPath);
            }

            CopyDirectory(folderPath, tempPackFolderPath + folderName);

            ZipFile.CreateFromDirectory(tempPackFolderPath, tempExportPath, CompressionLevel.SmallestSize, false);
            File.Copy(tempExportPath, exportPath);
            File.Delete(tempExportPath);

            MessageBox.Show("Finished Export", "Export Status", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public struct CustomItemPackData
    {
        public string savedFolderPath;
        public string folderName;

        public string name;
        public string author;

        public string[] assetPaths;
        public string assetPath;

        public string[] assemblyPaths;
        public string assemblyPath;
    }

    [Serializable]
    public struct SerializeableCustomItemPackData
    {
        public string name;
        public string author;

        public string[] assetPaths;
        public string assetPath;

        public string[] assemblyPaths;
        public string assemblyPath;
    }
}
