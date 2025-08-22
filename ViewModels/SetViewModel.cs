using ExerciseTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExerciseTracker.ViewModels
{
    public class SetViewModel
    {
        public Exercise Exercise { get; set; }
        public int Reps { get; set; }
        public double Weight { get; set; }

        public string ExerciseName => Exercise.Name;
        public int ExerciseId => Exercise.Id;
    }
}
