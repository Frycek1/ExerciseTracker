
namespace ExerciseTracker.Models
{ 

    public class ExerciseWithMuscleGroups
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> MuscleGroups { get; set; }
    }
}
