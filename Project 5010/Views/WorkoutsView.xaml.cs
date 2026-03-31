using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Media;
using Project_5010.Models;
using Project_5010.Services;

namespace Project_5010.Views
{
    public partial class WorkoutsView : UserControl
    {
        private readonly ObservableCollection<Workout> workouts;
        private readonly WorkoutFileService workoutFileService;

        private Workout? selectedWorkout;
        private string _activeFilter = "All";

        public WorkoutsView(ObservableCollection<Workout> workouts, WorkoutFileService workoutFileService)
        {
            InitializeComponent();

            this.workouts = workouts;
            this.workoutFileService = workoutFileService;

            HistoryListView.ItemsSource = this.workouts;

            WorkoutDatePicker.SelectedDate = DateTime.Now.Date;
            WorkoutTimeTextBox.Text = DateTime.Now.ToString("HH:mm");

            UpdateWeeklySummary();
        }

        // ========== FILTER ==========

        private void FilterType_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            _activeFilter = btn.Tag?.ToString() ?? "All";

            var filterBtns = new[] { FilterAllBtn, FilterCardioBtn, FilterStrengthBtn, FilterFlexBtn };
            foreach (var b in filterBtns)
            {
                b.Background = new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6));
                b.Foreground = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80));
            }
            btn.Background = new SolidColorBrush(Color.FromRgb(0x6D, 0x4A, 0xFF));
            btn.Foreground = Brushes.White;

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (HistoryListView?.ItemsSource == null) return;
            ICollectionView view = CollectionViewSource.GetDefaultView(HistoryListView.ItemsSource);
            view.Filter = obj =>
            {
                if (obj is not Workout w) return false;
                return _activeFilter == "All" || w.Type.Equals(_activeFilter, StringComparison.OrdinalIgnoreCase);
            };
            view.Refresh();
        }

        // ========== WEEKLY SUMMARY ==========

        private void UpdateWeeklySummary()
        {
            DateTime startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var thisWeek = workouts.Where(w => w.Date.Date >= startOfWeek).ToList();

            WeekSessionsText.Text = $"{thisWeek.Count} session{(thisWeek.Count == 1 ? "" : "s")}";
            WeekMinutesText.Text = $"{thisWeek.Sum(w => w.DurationMinutes)} min total";

            var breakdown = thisWeek
                .GroupBy(w => w.Type)
                .Select(g => $"{g.Key}: {g.Count()}")
                .ToList();

            WeekBreakdownText.Text = breakdown.Count > 0 ? string.Join("\n", breakdown) : "No sessions yet";
        }

        // ========== CRUD ==========

        private void SaveOrUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(DurationTextBox.Text, out int duration) || duration <= 0)
            {
                SetStatus("Enter a valid duration (positive number).", Colors.IndianRed);
                return;
            }

            ComboBoxItem selectedItem = (ComboBoxItem)WorkoutTypeCombo.SelectedItem;
            string workoutType = selectedItem.Content?.ToString() ?? "";

            DateTime date = WorkoutDatePicker.SelectedDate ?? DateTime.Now.Date;

            if (!TimeSpan.TryParse(WorkoutTimeTextBox.Text, out TimeSpan time))
            {
                SetStatus("Enter time as HH:mm (e.g. 18:30).", Colors.IndianRed);
                return;
            }

            DateTime dateTime = date.Date + time;
            string notes = NotesTextBox.Text ?? "";

            if (selectedWorkout == null)
            {
                workouts.Add(new Workout
                {
                    Type = workoutType,
                    Date = dateTime,
                    DurationMinutes = duration,
                    Notes = notes
                });
                SaveAll();
                SetStatus("Workout saved!", Colors.LimeGreen);
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
                    SetStatus("Workout updated!", Colors.LimeGreen);
                }
            }

            ApplyFilter();
            UpdateWeeklySummary();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedWorkout == null)
            {
                SetStatus("Select a workout first.", Colors.IndianRed);
                return;
            }

            workouts.Remove(selectedWorkout);
            SaveAll();
            SetStatus("Workout deleted.", Colors.IndianRed);
            ClearForm();
            ApplyFilter();
            UpdateWeeklySummary();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            SetStatus("Cleared.", Colors.Gray);
        }

        private void HistoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HistoryListView.SelectedItem is not Workout w)
            {
                selectedWorkout = null;
                SaveOrUpdateButton.Content = "Save Workout";
                FormTitleText.Text = "Log Workout";
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
            FormTitleText.Text = "Edit Workout";
            DeleteButton.Visibility = Visibility.Visible;
            SetStatus("Editing selected workout...", Colors.CornflowerBlue);
        }

        // ========== HELPERS ==========

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
            FormTitleText.Text = "Log Workout";
            DeleteButton.Visibility = Visibility.Collapsed;
        }

        private void SetStatus(string message, Color color)
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Foreground = new SolidColorBrush(color);
        }

        private static int TypeToIndex(string type) => type switch
        {
            "Cardio"      => 0,
            "Strength"    => 1,
            "Flexibility" => 2,
            _             => 0
        };
    }
}
