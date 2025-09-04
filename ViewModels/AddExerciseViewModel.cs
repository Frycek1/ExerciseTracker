using ExerciseTracker.Core;
using ExerciseTracker.Models;
using ExerciseTracker.Services;
using System.Collections.ObjectModel;

namespace ExerciseTracker.ViewModels
{
    public class AddExerciseViewModel : ViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly DatabaseService _databaseService;

        public RelayCommand AddExerciseCommand { get; }
        public RelayCommand CancelCommand { get; }

        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        private string _description;
        public string Description { get => _description; set => SetProperty(ref _description, value); }
        public ObservableCollection<SelectableMuscleGroupViewModel> MuscleGroups { get; } = new();

        public AddExerciseViewModel(INavigationService navigation, DatabaseService databaseService)
        {
            _navigationService = navigation;
            _databaseService = databaseService;

            AddExerciseCommand = new RelayCommand(
                execute: o => AddExercise(),
                canExecute: o => !string.IsNullOrWhiteSpace(Name) && MuscleGroups.Any(m => m.IsSelected)
            );
            CancelCommand = new RelayCommand(o => _navigationService.NavigateTo<ExerciseListViewModel>(), o => true);
            LoadMuscleGroups();
        }
        private void LoadMuscleGroups()
        {
            var groupsFromDb = _databaseService.GetAllMuscleGroups();
            MuscleGroups.Clear();
            foreach (var group in groupsFromDb)
            {
                MuscleGroups.Add(new SelectableMuscleGroupViewModel(group));
            }
        }
        private void AddExercise()
        {
            var newExercise = new Exercise { Name = this.Name, Description = this.Description };
            long exerciseId = _databaseService.AddExercise(newExercise);

            var selectedGroups = MuscleGroups.Where(m => m.IsSelected).Select(m => m.MuscleGroup);

            foreach (var group in selectedGroups)
            {
                _databaseService.AddExerciseMuscleGroupLink(exerciseId, group.Id);
            }

            _navigationService.NavigateTo<ExerciseListViewModel>();
        }
        public override void OnNavigatedTo()
        {
            Name = string.Empty;
            Description = string.Empty;
            foreach (var group in MuscleGroups)
            {
                group.IsSelected = false;
            }
        }
    }
}
