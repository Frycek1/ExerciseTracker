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
using System.Windows.Navigation;

namespace ExerciseTracker.ViewModels
{
    class SessionListViewModel : ViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly INavigationService _navigationService;

        public ObservableCollection<Session> Sessions { get; } = new();

        private Session _selectedSession;
        public Session SelectedSession
        {
            get => _selectedSession;
            set
            {
                if (SetProperty(ref _selectedSession, value) && value != null)
                {
                    _navigationService.NavigateTo<AddSessionViewModel, Session>(value);
                    _selectedSession = null;
                }
            }
        }

        public RelayCommand AddNewSessionCommand { get; }
        public RelayCommand ShowExerciseListCommand { get; }
        public SessionListViewModel(INavigationService navigation, DatabaseService databaseService)
        {
            _navigationService = navigation;
            _databaseService = databaseService;

            AddNewSessionCommand = new RelayCommand(o => _navigationService.NavigateTo<AddSessionViewModel>(), o => true);
            ShowExerciseListCommand = new RelayCommand(o => _navigationService.NavigateTo<ExerciseListViewModel>(), o => true);

            LoadSessions();
        }

        public override void OnNavigatedTo()
        {
            
            LoadSessions();
        }
        private void LoadSessions()
        {
            Sessions.Clear();
            var sessionsFromDb = _databaseService.GetAllSessions();
            foreach (var session in sessionsFromDb)
            {
                Sessions.Add(session);
            }
        }

    }
}
