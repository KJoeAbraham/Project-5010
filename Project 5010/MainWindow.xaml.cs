using System.Collections.ObjectModel;
using System.Windows;
using Project_5010.Models;
using Project_5010.Services;
using Project_5010.Views;

namespace Project_5010
{
    public partial class MainWindow : Window
    {
        private readonly WorkoutFileService workoutFileService;
        private readonly ObservableCollection<Workout> workouts;

        public MainWindow()
        {
            InitializeComponent();

            workoutFileService = new WorkoutFileService();
            workouts = new ObservableCollection<Workout>(workoutFileService.LoadWorkouts());

            // Default page
            MainContent.Content = new DashboardView(workouts);
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new DashboardView(workouts);
        }

        private void Workouts_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new WorkoutsView(workouts, workoutFileService);
        }

        private void Library_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new LibraryView();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new SettingsView();
        }
    }
}