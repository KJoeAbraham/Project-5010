using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project_5010.Models;
using Project_5010.Services;

namespace Project_5010.Views
{
    public partial class WorkoutsView : UserControl
    {
        private readonly ObservableCollection<Workout> workouts;
        private readonly WorkoutFileService workoutFileService;

        private Workout? selectedWorkout;

        public WorkoutsView(ObservableCollection<Workout> workouts, WorkoutFileService workoutFileService)
        {
            InitializeComponent();

            this.workouts = workouts;
            this.workoutFileService = workoutFileService;

            HistoryListView.ItemsSource = this.workouts;

            WorkoutDatePicker.SelectedDate = DateTime.Now.Date;
            WorkoutTimeTextBox.Text = DateTime.Now.ToString("HH:mm");

            StatusTextBlock.Text = $"{this.workouts.Count} workout(s) loaded.";
        }

        private void SaveOrUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(DurationTextBox.Text, out int duration) || duration <= 0)
            {
                StatusTextBlock.Text = "Please enter a valid duration (positive number).";
                return;
            }

            ComboBoxItem selectedItem = (ComboBoxItem)WorkoutTypeCombo.SelectedItem;
            string workoutType = selectedItem.Content?.ToString() ?? "";

            DateTime date = WorkoutDatePicker.SelectedDate ?? DateTime.Now.Date;

            if (!TimeSpan.TryParse(WorkoutTimeTextBox.Text, out TimeSpan time))
            {
                StatusTextBlock.Text = "Please enter time in HH:mm format (example: 18:30).";
                return;
            }

            DateTime dateTime = date.Date + time;
            string notes = NotesTextBox.Text ?? "";

            if (selectedWorkout == null)
            {
                Workout workout = new Workout
                {
                    Type = workoutType,
                    Date = dateTime,
                    DurationMinutes = duration,
                    Notes = notes
                };

                workouts.Add(workout);
                SaveAll();

                StatusTextBlock.Text = "Workout saved!";
                ClearForm();
            }
            else
            {
                int idx = workouts.IndexOf(selectedWorkout);
                if (idx >= 0)
                {
                    Workout updated = new Workout
                    {
                        Type = workoutType,
                        Date = dateTime,
                        DurationMinutes = duration,
                        Notes = notes
                    };

                    workouts[idx] = updated;
                    selectedWorkout = updated;
                    HistoryListView.SelectedItem = updated;

                    SaveAll();
                    StatusTextBlock.Text = "Workout updated!";
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedWorkout == null)
            {
                StatusTextBlock.Text = "Select a workout first.";
                return;
            }

            workouts.Remove(selectedWorkout);
            SaveAll();

            StatusTextBlock.Text = "Workout deleted.";
            ClearForm();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            StatusTextBlock.Text = "Cleared.";
        }

        private void HistoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HistoryListView.SelectedItem is not Workout w)
            {
                selectedWorkout = null;
                SaveOrUpdateButton.Content = "Save Workout";
                DeleteButton.Visibility = Visibility.Collapsed;
                return;
            }

            selectedWorkout = w;

            WorkoutTypeCombo.SelectedIndex = TypeToIndex(w.Type);
            WorkoutDatePicker.SelectedDate = w.Date.Date;
            WorkoutTimeTextBox.Text = w.Date.ToString("HH:mm");
            DurationTextBox.Text = w.DurationMinutes.ToString();
            NotesTextBox.Text = w.Notes;

            SaveOrUpdateButton.Content = "Update Workout";
            DeleteButton.Visibility = Visibility.Visible;

            StatusTextBlock.Text = "Editing selected workout...";
        }

        private void SaveAll()
        {
            workoutFileService.SaveWorkouts(workouts.ToList());
        }

        private void ClearForm()
        {
            selectedWorkout = null;
            HistoryListView.SelectedItem = null;

            WorkoutTypeCombo.SelectedIndex = 0;
            WorkoutDatePicker.SelectedDate = DateTime.Now.Date;
            WorkoutTimeTextBox.Text = DateTime.Now.ToString("HH:mm");

            DurationTextBox.Clear();
            NotesTextBox.Clear();

            SaveOrUpdateButton.Content = "Save Workout";
            DeleteButton.Visibility = Visibility.Collapsed;
        }

        private static int TypeToIndex(string type)
        {
            return type switch
            {
                "Cardio" => 0,
                "Strength" => 1,
                "Flexibility" => 2,
                _ => 0
            };
        }
    }
}