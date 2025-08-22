using ExerciseTracker.Core;
using ExerciseTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
