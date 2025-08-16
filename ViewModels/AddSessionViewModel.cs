using ExerciseTracker.Core;
using ExerciseTracker.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ExerciseTracker.ViewModels
{
    public class AddSessionViewModel : Core.ViewModel
    {
        INavigationService _navigation;
        public INavigationService Navigation
        {
            get => _navigation;
            set
            {
                if (_navigation != value)
                {
                    _navigation = value;
                    OnPropertyChanged(nameof(NavigationService));
                }
            }
        }
        public ICommand SaveSessionCommand { get; }
        public AddSessionViewModel(INavigationService navigation)
        {
            Navigation = navigation;
            SaveSessionCommand = new RelayCommand(o => Navigation.NavigateTo<SessionListViewModel>(), o => true);
        }
    }
}
