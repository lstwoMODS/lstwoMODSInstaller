using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using lstwoMODSInstaller.ModManagement;

namespace lstwoMODSInstaller.MVVM.View
{
    public partial class InstallerView : UserControl
    {
        private Game selectedGame;
        private Mod selectedMod;

        public InstallerView()
        {
            InitializeComponent();

            MainWindow.OnSelectedGameChanged += () =>
            {
                SelectedGameChanged(MainWindow.SelectedGame);
            };

            MainWindow.OnInit += () =>
            {
                _=InitUI();
            };

            MainWindow.InitCallbackLocks.Remove(GetType());
            MainWindow.PATInitCallbackLocks.Remove(GetType());

            CoreModInstallButton.IsEnabled = false;
            GameModInstallButton.IsEnabled = false;
            AdditionalModsDropdown.IsEnabled = false;
            AdditionalModInstallButton.IsEnabled = false;
        }

        private async Task InitUI()
        {
            var lstwoModsCoreData = DataManager.coreData.lstwomods_core;
            await lstwoModsCoreData.UpdateLatestRelease();

            CoreModHeader.Text = $"Install {lstwoModsCoreData.mod_name}";
            CoreModNameText.Text = lstwoModsCoreData.mod_name;

            if(lstwoModsCoreData.latestRelease != null)
            {
                CoreModVersionText.Text = $"Latest Version: {lstwoModsCoreData.latestRelease.TagName}";
                CoreModInstallButton.IsEnabled = true;
            }
            else
            {
                CoreModVersionText.Text = $"Latest Version: Unreleased";
                CoreModInstallButton.IsEnabled = false;
            }
        }

        private async void SelectedGameChanged(Game game)
        {
            if (game == null)
            {
                return;
            }

            var modPack = game.mod_pack;

            await game.mod_pack.UpdateLatestRelease();

            GameModHeader.Text = $"Install {game.game_name} Mods";
            GameModNameText.Text = $"{game.mod_pack.mod_name}";

            if(game.mod_pack.latestRelease != null)
            {
                GameModVersionText.Text = $"Latest Version: {game.mod_pack.latestRelease.TagName}";
                GameModInstallButton.IsEnabled = true;
            }
            else
            {
                GameModVersionText.Text = $"Latest Version: Unreleased";
                GameModInstallButton.IsEnabled = false;
            }

            var additionalMods = game.mods.Values.ToList();
            var nonHiddenMods = new List<Mod>();

            foreach (var mod in additionalMods)
            {
                if(!mod.isHidden)
                {
                    nonHiddenMods.Add(mod);
                }
            }

            AdditionalModsDropdown.ItemsSource = nonHiddenMods;

            MainWindow.ShouldDarkenMainPart = false;
            AdditionalModsDropdown.IsEnabled = true;
        }

        private async void SelectedModChanged(Mod mod)
        {
            selectedMod = mod;

            if(mod == null)
            {
                AdditionalModNameText.Text = "No Mod Selected";
                AdditionalModVersionText.Text = $"Latest Version: ---";
                AdditionalModInstallButton.IsEnabled = false;
                return;
            }

            await mod.UpdateLatestRelease();
            
            if(mod.latestRelease != null)
            {
                AdditionalModNameText.Text = mod.mod_name;
                AdditionalModVersionText.Text = $"Latest Version: {mod.latestRelease.TagName}";
                AdditionalModInstallButton.IsEnabled = true;
            }
            else
            {
                AdditionalModNameText.Text = mod.mod_name;
                AdditionalModVersionText.Text = $"Latest Version: Unreleased";
                AdditionalModInstallButton.IsEnabled = false;
            }
        }


        private void OnDownloadProgressChanged(int progress)
        {
            progressBar.Value = progress;
        }

        private void CoreModInstallButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GameModInstallButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AdditionalModInstallButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AdditionalModsDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedModChanged(AdditionalModsDropdown.SelectedItem as Mod);
        }
    }
}
