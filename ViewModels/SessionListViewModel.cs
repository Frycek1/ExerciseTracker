using ExerciseTracker.Core;
using ExerciseTracker.Models;
using ExerciseTracker.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ExerciseTracker.ViewModels
{
    class SessionListViewModel : ViewModel
    {
        private readonly ObservableCollection<Session> _sessions;
        public IEnumerable<Session> Sessions => _sessions;

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
        public RelayCommand AddNewSessionCommand { get; }
        public RelayCommand ShowExerciseListCommand { get; }
        public SessionListViewModel(INavigationService navigation)
        {
            Navigation = navigation;
            AddNewSessionCommand = new RelayCommand(o => Navigation.NavigateTo<AddSessionViewModel>(), o => true);
            ShowExerciseListCommand = new RelayCommand(o => Navigation.NavigateTo<ExerciseListViewModel>(), o => true);

            _sessions = new ObservableCollection<Session>
            {
                new Session
                {
                    Id = 1,
                    Date = DateTime.Now,
                    
                },
                new Session
                {
                    Id = 2,
                    Date = DateTime.Now.AddDays(-1),
                    
                }
            };
        }
    }
}
