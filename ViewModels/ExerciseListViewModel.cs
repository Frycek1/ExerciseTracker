using ExerciseTracker.Core;
using ExerciseTracker.Models; // Upewnij się, że masz ten using
using ExerciseTracker.Services;
using System.Collections.ObjectModel;
using System.Collections.Generic; // Dodaj ten using

namespace ExerciseTracker.ViewModels
{
    public class ExerciseListViewModel : ViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly DatabaseService _databaseService;

        public ObservableCollection<ExerciseWithMuscleGroups> Exercises { get; } = new();

        public RelayCommand NavigateToSessionListCommand { get; }
        public RelayCommand NavigateToAddExerciseCommand { get; }
        
        public ExerciseListViewModel(INavigationService navigation, DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _navigationService = navigation;
            NavigateToSessionListCommand = new RelayCommand(o => _navigationService.NavigateTo<SessionListViewModel>(), o => true);
            NavigateToAddExerciseCommand = new RelayCommand(o => _navigationService.NavigateTo<AddExerciseViewModel>(), o => true);
            LoadExercises();
        }
        public override void OnNavigatedTo()
        {
            LoadExercises();
        }
        public void LoadExercises()
        {
            Exercises.Clear();
            var exercisesFromDb = _databaseService.GetAllExercisesWithMuscleGroups();
            foreach (var exercise in exercisesFromDb)
            {
                Exercises.Add(exercise);
            }
        }
    }
}