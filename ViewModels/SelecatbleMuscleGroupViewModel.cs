using ExerciseTracker.Core;
using ExerciseTracker.Models;


namespace ExerciseTracker.ViewModels
{
    public class SelectableMuscleGroupViewModel : ObservableObject
    {
        public MuscleGroup MuscleGroup { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public SelectableMuscleGroupViewModel(MuscleGroup muscleGroup)
        {
            MuscleGroup = muscleGroup;
        }
    }
}
