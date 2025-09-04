using ExerciseTracker.Core;

namespace ExerciseTracker.Services
{
    public interface INavigationService
    {
        ViewModel CurrentView { get; }
        void NavigateTo<T>() where T : ViewModel;
        void NavigateTo<TViewModel, TParameter>(TParameter parameter) where TViewModel : ViewModel;
    }
}
