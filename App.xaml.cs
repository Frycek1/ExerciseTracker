using ExerciseTracker.Services;
using ExerciseTracker.ViewModels;
using ExerciseTracker.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using ExerciseTracker.Core;

namespace ExerciseTracker
{
    public partial class App : Application
    {
        private readonly IServiceCollection _services = new ServiceCollection();
        private readonly ServiceProvider _serviceProvider;
        public App()
        {

            _services.AddSingleton<MainWindow>(provider => new MainWindow
            {
                DataContext = provider.GetRequiredService<MainViewModel>()
            });
            _services.AddSingleton<MainViewModel>();
            _services.AddSingleton<AddSessionViewModel>();
            _services.AddSingleton<SessionListViewModel>();
            _services.AddSingleton<ExerciseListViewModel>();
            _services.AddSingleton<AddExerciseViewModel>();
            _services.AddSingleton<INavigationService, NavigationService>();

            _services.AddSingleton<DatabaseService>(provider => new DatabaseService("ExerciseTracker")); 

            _services.AddSingleton<Func<Type, ViewModel>>(serviceProvider => viewModelType => (ViewModel)serviceProvider.GetRequiredService(viewModelType));

            _serviceProvider = _services.BuildServiceProvider();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            MainWindow.Show();
            base.OnStartup(e);
        }
        
    }
}
