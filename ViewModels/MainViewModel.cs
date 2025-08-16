using ExerciseTracker.Core;
using ExerciseTracker.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExerciseTracker.ViewModels
{
    class MainViewModel : ViewModel
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
        public MainViewModel(INavigationService navigaton)
        {
            Navigation = navigaton;
            Navigation.NavigateTo<SessionListViewModel>();
        }

    }
}
