using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExerciseTracker.Models
{
    public class InitialData
    {
        public List<string> MuscleGroups { get; set; }
        public List<ExerciseWithMuscleGroups> Exercises { get; set; }
    }
}
