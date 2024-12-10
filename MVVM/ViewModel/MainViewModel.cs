using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lstwoMODSInstaller.Core;

namespace lstwoMODSInstaller.MVVM.ViewModel
{
    internal class MainViewModel : ObservableObject
    {
        public RelayCommand InstallerViewCommand { get; set; }
        public RelayCommand CustomItemViewCommand { get; set; }

        public InstallerViewModel InstallerVM { get; set; }
        public CustomItemViewModel CustomItemVM { get; set; }

        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set 
            { 
                _currentView = value;
                OnPropertyChanged();
            }
        }


        public MainViewModel()
        {
            InstallerVM = new InstallerViewModel();
            CustomItemVM = new CustomItemViewModel();

            CurrentView = InstallerVM;

            InstallerViewCommand = new RelayCommand(o =>
            {
                CurrentView = InstallerVM;
            });

            CustomItemViewCommand = new RelayCommand(o =>
            {
                CurrentView = CustomItemVM;
            });
        }
    }
}
