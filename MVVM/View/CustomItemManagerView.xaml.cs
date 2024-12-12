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

        private void NewItemPack()
        {
            var data = new CustomItemPackData();
            data.folderName = "New Custom Item Pack";
            Data = data;

            SetUIData(Data);

            FolderNameTextBox.SelectAll();
            FolderNameTextBox.Focus();
        }

        private void PackListDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var pack = GetItemPackFromFolder(customItemsFolder + PackList.SelectedItem.ToString());

            if(pack == null)
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
                var fileName = Path.GetFileName(file);
                var packPath = $"{customItemsFolder}/{NewData.folderName}/";
                var filePath = $"{packPath}/{fileName}";

                if (!Directory.Exists(packPath))
                {
                    Directory.CreateDirectory(packPath);
                }

                File.Copy(file, filePath, true);

                assetBundles.Add(fileName);

                var data = NewData;
                var assetPathsList = data.assetPaths == null ? new List<string>() : data.assetPaths.ToList();
                assetPathsList.Add(fileName);
                data.assetPaths = assetPathsList.ToArray();
                NewData = data;

                SetUIData(NewData);
            }

            IsUnsaved = true;
            
        }

        private void AssetBundlesDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if(AssetBundlesList.SelectedItem == null)
            {
                return;
            }

            var fileName = (string)AssetBundlesList.SelectedItem;

            var result = MessageBox.Show($"Are you sure you want to delete {fileName}? You cannot undo this action!",
                "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

            if(result == MessageBoxResult.No)
            {
                return;
            }

            var packPath = $"{customItemsFolder}/{NewData.folderName}/";
            var filePath = $"{packPath}/{fileName}";

            if (!Directory.Exists(packPath) || !packPath.StartsWith(customItemsFolder))
            {
                return;
            }

            File.Delete(filePath);

            assetBundles.Remove(fileName);

            var data = NewData;

            if(data.assetPaths == null)
            {
                return;
            }

            var assetPathsList = data.assetPaths.ToList();
            assetPathsList.Remove(fileName);
            data.assetPaths = assetPathsList.ToArray();
            NewData = data;

            SetUIData(NewData);

            IsUnsaved = true;
        }

        private void AssembliesNewButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AssembliesDeleteButton_Click(object sender, RoutedEventArgs e)
        {

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
