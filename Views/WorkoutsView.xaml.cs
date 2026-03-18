using System;
using System.Collections.ObjectModel;
using System.Globalization;
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
        private string currentFilter = "All";

        public WorkoutsView(ObservableCollection<Workout> workouts, WorkoutFileService workoutFileService)
        {
            InitializeComponent();

            this.workouts = workouts;
            this.workoutFileService = workoutFileService;

            WorkoutDatePicker.SelectedDate = DateTime.Now.Date;
            WorkoutTimeTextBox.Text = DateTime.Now.ToString("HH:mm");
            WorkoutTypeCombo.SelectedIndex = 0;

            RefreshHistory();
            UpdateWeeklySummary();
            UpdateWorkoutCount();
            UpdateCaloriesPreview();

            StatusTextBlock.Text = $"{this.workouts.Count} workout(s) loaded.";
        }

        private void SaveOrUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            ClearValidationErrors();

            if (!TryReadForm(out string workoutType, out string title, out DateTime dateTime, out int duration, out int calories, out string notes))
            {
                return;
            }

            if (selectedWorkout == null)
            {
                Workout workout = new Workout
                {
                    Type = workoutType,
                    Title = title,
                    Date = dateTime,
                    DurationMinutes = duration,
                    Calories = calories,
                    Notes = notes
                };

                workouts.Add(workout);
                SaveAll();

                StatusTextBlock.Text = "Workout saved.";

                ClearForm();
                RefreshHistory();
            }
            else
            {
                selectedWorkout.Type = workoutType;
                selectedWorkout.Title = title;
                selectedWorkout.Date = dateTime;
                selectedWorkout.DurationMinutes = duration;
                selectedWorkout.Calories = calories;
                selectedWorkout.Notes = notes;

                SaveAll();
                RefreshHistory();

                HistoryListView.SelectedItem = selectedWorkout;
                SaveOrUpdateButton.Content = "Update Workout";
                DeleteButton.Visibility = Visibility.Visible;

                StatusTextBlock.Text = "Workout updated.";
            }

            UpdateWeeklySummary();
            UpdateWorkoutCount();
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
            RefreshHistory();
            UpdateWeeklySummary();
            UpdateWorkoutCount();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            StatusTextBlock.Text = "Form cleared.";
        }

        private void HistoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HistoryListView.SelectedItem is not Workout workout)
            {
                selectedWorkout = null;
                SaveOrUpdateButton.Content = "Save Workout";
                DeleteButton.Visibility = Visibility.Collapsed;
                return;
            }

            selectedWorkout = workout;

            WorkoutTypeCombo.SelectedIndex = TypeToIndex(workout.Type);
            TitleTextBox.Text = workout.Title;
            WorkoutDatePicker.SelectedDate = workout.Date.Date;
            WorkoutTimeTextBox.Text = workout.Date.ToString("HH:mm");
            DurationTextBox.Text = workout.DurationMinutes.ToString();
            CaloriesTextBox.Text = workout.Calories.ToString();
            NotesTextBox.Text = workout.Notes;

            SaveOrUpdateButton.Content = "Update Workout";
            DeleteButton.Visibility = Visibility.Visible;

            StatusTextBlock.Text = "Editing selected workout.";
        }

        private void DurationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCaloriesPreview();
        }

        private void WorkoutTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCaloriesPreview();
        }

        private void UpdateCaloriesPreview()
        {
            if (WorkoutTypeCombo == null || DurationTextBox == null || CaloriesTextBox == null)
            {
                return;
            }

            if (WorkoutTypeCombo.SelectedItem is not ComboBoxItem selectedItem)
            {
                CaloriesTextBox.Text = "";
                return;
            }

            string type = selectedItem.Content?.ToString() ?? "";

            if (!int.TryParse(DurationTextBox.Text, out int duration) || duration <= 0)
            {
                CaloriesTextBox.Text = "";
                return;
            }

            CaloriesTextBox.Text = EstimateCalories(type, duration).ToString();
        }

        private int EstimateCalories(string type, int duration)
        {
            return type switch
            {
                "Cardio" => duration * 8,
                "Strength" => duration * 5,
                "HIIT" => duration * 10,
                "Flexibility" => duration * 3,
                _ => 0
            };
        }

        private bool TryReadForm(out string workoutType, out string title, out DateTime dateTime, out int duration, out int calories, out string notes)
        {
            workoutType = "";
            title = "";
            dateTime = DateTime.Now;
            duration = 0;
            calories = 0;
            notes = "";

            if (WorkoutTypeCombo.SelectedItem is not ComboBoxItem selectedItem)
            {
                StatusTextBlock.Text = "Select a workout type.";
                return false;
            }

            workoutType = selectedItem.Content?.ToString() ?? "";

            title = TitleTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                title = $"{workoutType} Workout";
            }

            if (!int.TryParse(DurationTextBox.Text.Trim(), out duration) || duration <= 0)
            {
                DurationErrorTextBlock.Text = "Duration must be a positive number.";
                return false;
            }

            string timeText = WorkoutTimeTextBox.Text.Trim();

            if (!DateTime.TryParseExact(timeText, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTime))
            {
                TimeErrorTextBlock.Text = "Time must be in HH:mm format.";
                return false;
            }

            DateTime date = WorkoutDatePicker.SelectedDate ?? DateTime.Now.Date;
            dateTime = date.Date.AddHours(parsedTime.Hour).AddMinutes(parsedTime.Minute);

            calories = EstimateCalories(workoutType, duration);
            notes = NotesTextBox.Text.Trim();

            return true;
        }

        private void RefreshHistory()
        {
            var filtered = workouts.AsEnumerable();

            if (currentFilter != "All")
            {
                filtered = filtered.Where(w => w.Type == currentFilter);
            }

            HistoryListView.ItemsSource = filtered
                .OrderByDescending(w => w.Date)
                .ToList();
        }

        private void UpdateWeeklySummary()
        {
            DateTime today = DateTime.Today;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime startOfWeek = today.AddDays(-diff).Date;

            var weeklyWorkouts = workouts.Where(w => w.Date.Date >= startOfWeek).ToList();

            WeeklySummaryTextBlock.Text = $"{weeklyWorkouts.Count} workouts · {weeklyWorkouts.Sum(w => w.Calories)} kcal this week";
        }

        private void UpdateWorkoutCount()
        {
            int count = currentFilter == "All"
                ? workouts.Count
                : workouts.Count(w => w.Type == currentFilter);

            WorkoutCountTextBlock.Text = $"{count} workouts";
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
            TitleTextBox.Clear();
            WorkoutDatePicker.SelectedDate = DateTime.Now.Date;
            WorkoutTimeTextBox.Text = DateTime.Now.ToString("HH:mm");
            DurationTextBox.Clear();
            CaloriesTextBox.Clear();
            NotesTextBox.Clear();

            SaveOrUpdateButton.Content = "Save Workout";
            DeleteButton.Visibility = Visibility.Collapsed;

            ClearValidationErrors();
        }

        private void ClearValidationErrors()
        {
            DurationErrorTextBlock.Text = "";
            TimeErrorTextBlock.Text = "";
        }

        private static int TypeToIndex(string type)
        {
            return type switch
            {
                "Cardio" => 0,
                "Strength" => 1,
                "HIIT" => 2,
                "Flexibility" => 3,
                _ => 0
            };
        }

        private void FilterAll_Click(object sender, RoutedEventArgs e)
        {
            currentFilter = "All";
            RefreshHistory();
            UpdateWorkoutCount();
        }

        private void FilterCardio_Click(object sender, RoutedEventArgs e)
        {
            currentFilter = "Cardio";
            RefreshHistory();
            UpdateWorkoutCount();
        }

        private void FilterStrength_Click(object sender, RoutedEventArgs e)
        {
            currentFilter = "Strength";
            RefreshHistory();
            UpdateWorkoutCount();
        }

        private void FilterHIIT_Click(object sender, RoutedEventArgs e)
        {
            currentFilter = "HIIT";
            RefreshHistory();
            UpdateWorkoutCount();
        }

        private void FilterFlexibility_Click(object sender, RoutedEventArgs e)
        {
            currentFilter = "Flexibility";
            RefreshHistory();
            UpdateWorkoutCount();
        }
    }
}