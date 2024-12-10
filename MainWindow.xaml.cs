using lstwoMODSInstaller.MVVM.View;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace lstwoMODSInstaller
{
    public partial class MainWindow : Window
    {
        public static Action OnPATInit;
        public static List<Type> PATInitCallbackLocks = new List<Type>() { typeof(InstallerView) };

        public MainWindow()
        {
            InitializeComponent();
            _ = InitializePAT();
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async Task InitializePAT()
        {
            try
            {
                await PATManager.EnsurePATExistsAsync();

                while(PATInitCallbackLocks.Count > 0)
                {
                    await Task.Delay(50);
                }

                if(OnPATInit != null)
                {
                    OnPATInit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing GitHub connection: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RefreshButton_Click(object sender, EventArgs e)
        {
            try
            {
                await DependencyManager.RefreshPATAsync();
                MessageBox.Show("PAT refreshed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to refresh PAT: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
