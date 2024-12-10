using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using lstwoMODSInstaller;

namespace lstwoMODSInstaller.MVVM.View
{
    public partial class InstallerView : UserControl
    {
        public InstallerView()
        {
            InitializeComponent();

            MainWindow.OnPATInit += () =>
            {
                _ = LoadDependencies();
            };

            MainWindow.PATInitCallbackLocks.Remove(GetType());
        }

        private async Task LoadDependencies()
        {
            var versions = await DependencyManager.GetLatestVersionsAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (versions.TryGetValue("BepInEx 5", out var bepinex))
                {
                    BepInExVersionText.Text = $"Newest Version: {bepinex.Version}";
                }
                if (versions.TryGetValue("ShadowLib", out var shadowLib))
                {
                    ShadowLibVersionText.Text = $"Newest Version: {shadowLib.Version}";
                }
                if (versions.TryGetValue("CUE", out var cue))
                {
                    CUEVersionText.Text = $"Newest Version: {cue.Version}";
                }
                if (versions.TryGetValue("lstwoMODS", out var lstwoMods))
                {
                    LstwoMODSVersionText.Text = $"Newest Version: {lstwoMods.Version}";
                }
            });
        }

        private void BepInEx_Install_Click(object sender, RoutedEventArgs e)
        {
            BepInEx_Install.IsEnabled = false;

            _ = DependencyManager.InstallDependencyAsync(DependencyManager.BepInExDependency, () =>
            {
                BepInEx_Install.IsEnabled = true;
            }, OnDownloadProgressChanged);
        }

        private void ShadowLib_Install_Click(object sender, RoutedEventArgs e)
        {
            ShadowLib_Install.IsEnabled = false;

            _ = DependencyManager.InstallDependencyAsync(DependencyManager.ShadowLibDependency, () =>
            {
                ShadowLib_Install.IsEnabled = true;
            }, OnDownloadProgressChanged);
        }

        private void CUE_Install_Click(object sender, RoutedEventArgs e)
        {
            CUE_Install.IsEnabled = false;

            _ = DependencyManager.InstallDependencyAsync(DependencyManager.CUEDependency, () =>
            {
                CUE_Install.IsEnabled = true;
            }, OnDownloadProgressChanged);
        }

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            Install.IsEnabled = false;

            _ = DependencyManager.InstallDependencyAsync(DependencyManager.lstwoMODSDependency, () =>
            {
                Install.IsEnabled = true;
            }, OnDownloadProgressChanged);
        }

        private void OnDownloadProgressChanged(int progress)
        {
            progressBar.Value = progress;
        }
    }
}
