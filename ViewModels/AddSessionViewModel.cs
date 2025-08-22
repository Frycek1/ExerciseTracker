using ExerciseTracker.Core;
using ExerciseTracker.Models;
using ExerciseTracker.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

namespace ExerciseTracker.ViewModels
{
    public class AddSessionViewModel : Core.ViewModel, IParameterReceiver<Session>
    {
        private readonly INavigationService _navigationService;
        private readonly DatabaseService _databaseService;

        private bool _isEditing = false;
        private Session _currentSession;

        public string Title => _isEditing ? "Edit Training Session" : "Add Training Session";
        private bool _parameterJustReceived = false;
        public ObservableCollection<Exercise> AllExercises { get; } = new();
        public ObservableCollection<SetViewModel> CurrentSessionSets { get; } = new();

        private Exercise _selectedExercise;
        public Exercise SelectedExercise { get => _selectedExercise; set => SetProperty(ref _selectedExercise, value); }
        private int _reps;
        public int Reps { get => _reps; set => SetProperty(ref _reps, value); }
        private double _weight;
        public double Weight { get => _weight; set => SetProperty(ref _weight, value); }

        private string _description;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public RelayCommand AddSetCommand { get; }
        public RelayCommand SaveSessionCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand RemoveSetCommand { get; }
        public AddSessionViewModel(INavigationService navigation, DatabaseService databaseService)
        {
            _navigationService = navigation;
            _databaseService = databaseService;

            AddSetCommand = new RelayCommand(o => AddSet(), o => SelectedExercise != null && Reps > 0);
            SaveSessionCommand = new RelayCommand(
                execute: o => SaveSession(),
                canExecute: o => CurrentSessionSets.Any() && !string.IsNullOrWhiteSpace(Description)
);
            CancelCommand = new RelayCommand(o => _navigationService.NavigateTo<SessionListViewModel>(), o => true);
            RemoveSetCommand = new RelayCommand(
                execute: (param) =>
                {
                    if (param is SetViewModel setToRemove)
                    {
                        CurrentSessionSets.Remove(setToRemove);
                    }
                },
                canExecute: (param) => param is SetViewModel
            );

            LoadInitialData();
        }
        public override void OnNavigatedTo()
        {
            if (!_parameterJustReceived)
            {
                ResetState();
            }

            _parameterJustReceived = false;
        }
        public void ReceiveParameter(Session session)
        {
            _parameterJustReceived = true;

            _isEditing = true;
            _currentSession = session;
            Description = session.Description;
            OnPropertyChanged(nameof(Title));
            LoadSessionDetails(session.Id);
        }

        private void ResetState()
        {
            _isEditing = false;
            _currentSession = null;
            CurrentSessionSets.Clear();
            Description = string.Empty;
            Reps = 0;
            Weight = 0;
            SelectedExercise = null;
            OnPropertyChanged(nameof(Title));
        }
        
        private void LoadInitialData()
        {
            var exercisesFromDb = _databaseService.GetAllExercises();
            AllExercises.Clear();
            foreach (var ex in exercisesFromDb) AllExercises.Add(ex);
        }

        private void LoadSessionDetails(int sessionId)
        {
            CurrentSessionSets.Clear();
            var setsFromDb = _databaseService.GetSetsForSession(sessionId);
            foreach (var set in setsFromDb)
            {
                CurrentSessionSets.Add(set);
            }
        }

        private void AddSet()
        {
            CurrentSessionSets.Add(new SetViewModel
            {
                Exercise = SelectedExercise,
                Reps = this.Reps,
                Weight = this.Weight
            });
            Reps = 0; Weight = 0;
        }

        

        private void SaveSession()
        {
            if (_isEditing)
            {
                _currentSession.Description = Description;
                _databaseService.UpdateSessionAndSets(_currentSession, CurrentSessionSets.ToList());
            }
            else
            {
                var newSession = new Session { Date = System.DateTime.Now, Description = Description};
                long sessionId = _databaseService.AddSession(newSession);
                var exercisesInSession = CurrentSessionSets.GroupBy(s => s.Exercise);
                foreach (var group in exercisesInSession)
                {
                    var sessionExercise = new SessionExercise { SessionId = (int)sessionId, ExerciseId = group.Key.Id };
                    long sessionExerciseId = _databaseService.AddSessionExercise(sessionExercise);
                    int setCounter = 1;
                    foreach (var set in group)
                    {
                        var exerciseSet = new ExerciseSet { SessionExerciseId = (int)sessionExerciseId, SetNumber = setCounter++, Reps = set.Reps, Weight = set.Weight };
                        _databaseService.AddExerciseSet(exerciseSet);
                    }
                }
            }
            _navigationService.NavigateTo<SessionListViewModel>();
        }
    }
}

