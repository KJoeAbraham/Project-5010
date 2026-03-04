using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Project_5010.Models;

namespace Project_5010
{
    public partial class MainWindow : Window
    {
        private readonly List<Workout> workouts = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SaveWorkoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(DurationTextBox.Text, out int duration) || duration <= 0)
            {
                StatusTextBlock.Text = "Please enter a valid duration.";
                return;
            }

            ComboBoxItem selectedItem = (ComboBoxItem)WorkoutTypeCombo.SelectedItem;
            string workoutType = selectedItem.Content?.ToString() ?? "";

            Workout workout = new Workout
            {
                Type = workoutType,
                Date = System.DateTime.Now,
                DurationMinutes = duration,
                Notes = NotesTextBox.Text
            };

            workouts.Add(workout);

            StatusTextBlock.Text = $"Workout saved! Total workouts: {workouts.Count}";

            DurationTextBox.Clear();
            NotesTextBox.Clear();
            WorkoutTypeCombo.SelectedIndex = 0;
        }
    }
}