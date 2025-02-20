using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using lstwoMODSInstaller.ModManagement;

namespace lstwoMODSInstaller.MVVM.View
{
    [SupportedOSPlatform("windows")]
    public partial class InstallerView : UserControl
    {
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

            if (MainWindow.SelectedGame == null)
            {
                MainWindow.ShouldDarkenMainPart = true;
            }
            else
            {
                MainWindow.ShouldDarkenMainPart = false;
            }

            if(DataManager.coreData != null)
            {
                UpdateCoreModUI();
            }

            if(MainWindow.SelectedGame != null)
            {
                SelectedGameChanged(MainWindow.SelectedGame);
            }

            if(selectedMod != null)
            {
                SelectedModChanged(selectedMod);
            }
        }

        private async Task InitUI()
        {
            CoreModInstallButton.IsEnabled = false;
            GameModInstallButton.IsEnabled = false;
            AdditionalModsDropdown.IsEnabled = false;
            AdditionalModInstallButton.IsEnabled = false;

            UpdateCoreModUI();
        }

        private async void UpdateCoreModUI()
        {
            var lstwoModsCoreData = DataManager.coreData.lstwomods_core;
            await lstwoModsCoreData.UpdateReleases();

            CoreModHeader.Text = $"Install {lstwoModsCoreData.mod_name}";
            CoreModNameText.Text = lstwoModsCoreData.mod_name;

            if (lstwoModsCoreData.latestRelease != null)
            {
                CoreModVersionText.Text = $"Latest Version: {lstwoModsCoreData.latestRelease.TagName}";
                CoreModInstallButton.IsEnabled = true;
            }
            else
            {
                CoreModVersionText.Text = $"Latest Version: Unreleased";
                CoreModInstallButton.IsEnabled = false;
            }

            CoreModDownloadsText.Text = $"Downloads: {lstwoModsCoreData.GetFullDownloadCount()}";
        }

        private async void SelectedGameChanged(Game game)
        {
            if (game == null)
            {
                return;
            }

            await game.mod_pack.UpdateReleases();

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

            GameModDownloadsText.Text = $"Downloads: {game.mod_pack.GetFullDownloadCount()}";

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

            await mod.UpdateReleases();
            
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

            AdditionalModDownloadsText.Text = $"Downloads: {mod.GetFullDownloadCount()}";
        }

        private async void DownloadCore()
        {
            if(MainWindow.SelectedGame == null)
            {
                return;
            }

            await MainWindow.SelectedGame.bepinex.DownloadMod(OnDownloadProgressChanged, MainWindow.SelectedGame, false);
            await DataManager.coreData.lstwomods_core.DownloadMod(OnDownloadProgressChanged, MainWindow.SelectedGame, true);
            await DataManager.DownloadOverridesFolderAsync(MainWindow.SelectedGame.GetGamePath());
        }

        private async void DownloadGameMods()
        {
            if(MainWindow.SelectedGame == null)
            {
                return;
            }

            await MainWindow.SelectedGame.mod_pack.DownloadMod(OnDownloadProgressChanged, MainWindow.SelectedGame, true);
        }

        private async void DownloadAdditionalMod()
        {
            if(MainWindow.SelectedGame == null || selectedMod == null)
            {
                return;
            }

            await selectedMod.DownloadMod(OnDownloadProgressChanged, MainWindow.SelectedGame, true);
        }


        private void OnDownloadProgressChanged(int progress)
        {
            progressBar.Value = progress;
        }

        private void CoreModInstallButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadCore();
        }

        private void GameModInstallButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadGameMods();
        }

        private void AdditionalModInstallButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadAdditionalMod();
        }

        private void AdditionalModsDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedModChanged(AdditionalModsDropdown.SelectedItem as Mod);
        }
    }
}
