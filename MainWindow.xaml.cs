using lstwoMODSInstaller.MVVM.View;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using lstwoMODSInstaller.ModManagement;
using System.Linq;
using System.Windows.Media;

namespace lstwoMODSInstaller
{
    public partial class MainWindow : Window
    {
        public static Action OnPATInit;
        public static List<Type> PATInitCallbackLocks = new List<Type>() { typeof(InstallerView) };

        public static Action OnInit;
        public static List<Type> InitCallbackLocks = new List<Type>() { typeof(InstallerView) };

        public static Action OnSelectedGameChanged;
        public static Game SelectedGame { get; private set; }

        public static MainWindow Instance { get; private set; }

        public static bool ShouldDarkenMainPart
        {
            set
            {
                Instance.MainPartCover.Visibility = value ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public MainWindow()
        {
            Instance = this;

            InitializeComponent();

            ShouldDarkenMainPart = true;

            _=Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                await InitializePAT();
                await DataManager.UpdateData();

                var games = DataManager.games.ToDictionary(entry => entry.Key, entry => entry.Value);
                games.Remove("all");

                foreach (var game in games)
                {
                    try
                    {
                        if (game.Value.locating_method.method_type == "steam" && string.IsNullOrEmpty(game.Value.GetGamePath()))
                        {
                            games.Remove(game.Key);
                        }
                    }
                    catch
                    {
                        games.Remove(game.Key);
                    }
                }

                GameSelectDropdown.ItemsSource = games.Values.ToList();

                while (InitCallbackLocks.Count > 0)
                {
                    await Task.Delay(50);
                }

                OnInit?.Invoke();
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{ex.Message}");
                Console.WriteLine($"{ex.StackTrace}");
                Console.ForegroundColor = ConsoleColor.White;
            }
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
                await GithubManager.RefreshPATAsync();
                await DataManager.UpdateData(true);
                MessageBox.Show("PAT refreshed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to refresh PAT: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GameSelectDropdown_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SelectedGame = GameSelectDropdown.SelectedItem as Game;
            OnSelectedGameChanged?.Invoke();
        }
    }
}
