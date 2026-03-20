using Project_5010.Models;
using Project_5010.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Project_5010.Views
{
    public partial class LibraryView : UserControl
    {
        private readonly ExerciseFileService exerciseService = new();
        private readonly PRFileService prService = new();
        private readonly SettingsFileService settingsService = new();

        private readonly ObservableCollection<Exercise> exercises;
        private readonly ObservableCollection<PersonalRecord> prs;

        private PersonalRecord? selectedPR;
        private UserSettings? currentSettings;
        private string? selectedExerciseName;

        // Default constructor
        public LibraryView()
        {
            InitializeComponent();

            exercises = new ObservableCollection<Exercise>(exerciseService.Load());
            prs = new ObservableCollection<PersonalRecord>(prService.Load());

            ExercisesListView.ItemsSource = exercises;
            PRListView.ItemsSource = prs;

            PRDatePicker.SelectedDate = DateTime.Now;

            currentSettings = SafeLoadSettings();
            ApplyExerciseFilter();
            RefreshSuggestions();
        }

        // Reflection constructor required for MainWindow routing
        public LibraryView(UserSettings settings, SettingsFileService settingsSvc, object? workoutsSource, object? workoutFileService)
        {
            InitializeComponent();

            exerciseService = new ExerciseFileService();
            prService = new PRFileService();
            settingsService = settingsSvc ?? new SettingsFileService();

            exercises = new ObservableCollection<Exercise>(exerciseService.Load());
            prs = new ObservableCollection<PersonalRecord>(prService.Load());

            ExercisesListView.ItemsSource = exercises;
            PRListView.ItemsSource = prs;

            PRDatePicker.SelectedDate = DateTime.Now;

            currentSettings = settings;
            ApplyExerciseFilter();
            RefreshSuggestions();
        }

        // Methods for MainWindow routing updates
        public void ApplyUserSettings(UserSettings settings)
        {
            currentSettings = settings;
            RefreshSuggestions();
        }
        public void ApplySettings(UserSettings settings) => ApplyUserSettings(settings);
        public void RefreshForSettings(UserSettings settings) => ApplyUserSettings(settings);
        public void ConfigureTabsForSplit(string splitId) => currentSettings ??= SafeLoadSettings();
        public void ApplySplitSelection(string splitId) => ConfigureTabsForSplit(splitId);

        private UserSettings SafeLoadSettings()
        {
            try { return settingsService.Load(); }
            catch { return new UserSettings(); }
        }

        // ========== EXERCISE FILTERING ==========

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyExerciseFilter();

        private void ApplyExerciseFilter()
        {
            if (ExercisesListView == null || ExercisesListView.ItemsSource == null) return;

            string q = (SearchTextBox?.Text ?? "").Trim().ToLowerInvariant();

            ICollectionView view = CollectionViewSource.GetDefaultView(ExercisesListView.ItemsSource);
            view.Filter = obj =>
            {
                if (obj is not Exercise ex) return false;

                return string.IsNullOrWhiteSpace(q) ||
                       ex.Name.ToLowerInvariant().Contains(q) ||
                       ex.Muscle.ToLowerInvariant().Contains(q) ||
                       ex.Equipment.ToLowerInvariant().Contains(q);
            };
            view.Refresh();
        }

        private void ExercisesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ExercisesListView.SelectedItem is Exercise ex)
            {
                selectedExerciseName = ex.Name;
                DetailNameText.Text = ex.Name;
                DetailEquipText.Text = "Equipment: " + ex.Equipment;
                DetailPrimaryText.Text = "Primary: " + ex.Muscle;
                DetailSecondaryText.Text = "Secondary: " + (string.IsNullOrWhiteSpace(ex.SubMuscle) ? "—" : ex.SubMuscle);
                ApplyPrFilter();
            }
            else
            {
                selectedExerciseName = null;
                DetailNameText.Text = "Select an exercise...";
                DetailEquipText.Text = "Equipment: —";
                DetailPrimaryText.Text = "Primary: —";
                DetailSecondaryText.Text = "Secondary: —";
                ApplyPrFilter();
            }
        }

        // ========== PR LIST & EDITOR ==========

        private void PRListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PRListView.SelectedItem is PersonalRecord pr)
            {
                selectedPR = pr;
                PRWeightTextBox.Text = pr.Weight.ToString();
                PRRepsTextBox.Text = pr.Reps.ToString();
                PRDatePicker.SelectedDate = pr.Date;

                SavePRButton.Content = "Update Record";
                DeletePRButton.Visibility = Visibility.Visible;
                PRStatusText.Text = "";
            }
        }

        private void SavePRButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(selectedExerciseName))
            {
                SetStatus("Please select an exercise first.", Colors.IndianRed);
                return;
            }

            if (!double.TryParse(PRWeightTextBox.Text, out double weight) || weight < 0)
            {
                SetStatus("Enter a valid weight.", Colors.IndianRed);
                return;
            }

            int.TryParse(PRRepsTextBox.Text, out int reps);
            DateTime date = PRDatePicker.SelectedDate ?? DateTime.Now;

            if (selectedPR == null)
            {
                prs.Add(new PersonalRecord
                {
                    ExerciseName = selectedExerciseName,
                    Weight = weight,
                    Reps = reps,
                    Date = date
                });
                SetStatus("New PR Added to Trophy Room!", Colors.LimeGreen);
            }
            else
            {
                selectedPR.Weight = weight;
                selectedPR.Reps = reps;
                selectedPR.Date = date;
                PRListView.Items.Refresh();
                SetStatus("PR Updated!", Colors.LimeGreen);
            }

            prService.Save(prs.ToList());
            ClearPRForm();
            ApplyPrFilter();
        }

        private void DeletePRButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPR == null) return;
            prs.Remove(selectedPR);
            prService.Save(prs.ToList());

            SetStatus("PR Deleted.", Colors.IndianRed);
            ClearPRForm();
            ApplyPrFilter();
        }

        private void ClearPRForm()
        {
            selectedPR = null;
            PRWeightTextBox.Text = "";
            PRRepsTextBox.Text = "";
            PRDatePicker.SelectedDate = DateTime.Now;
            SavePRButton.Content = "Log PR";
            DeletePRButton.Visibility = Visibility.Collapsed;
        }

        private void ApplyPrFilter()
        {
            if (PRListView == null || PRListView.ItemsSource == null) return;

            string exName = selectedExerciseName ?? "";

            ICollectionView view = CollectionViewSource.GetDefaultView(PRListView.ItemsSource);
            view.Filter = obj =>
            {
                if (obj is not PersonalRecord pr) return false;
                if (string.IsNullOrWhiteSpace(exName)) return true;
                return string.Equals(pr.ExerciseName, exName, StringComparison.OrdinalIgnoreCase);
            };
            view.Refresh();
        }

        private void SetStatus(string message, Color color)
        {
            PRStatusText.Text = message;
            PRStatusText.Foreground = new SolidColorBrush(color);
        }

        // ========== CUSTOM EXERCISE ==========

        private void CustomExercise_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TrackerPanel.Visibility = Visibility.Collapsed;
            CustomExercisePanel.Visibility = Visibility.Visible;
            CustomNameBox.Text = "";
            CustomTypeCombo.SelectedIndex = 0;
            CustomEquipCombo.SelectedIndex = 0;
            CustomPrimaryCombo.SelectedIndex = 0;
            CustomSecondaryList.SelectedItems.Clear();
            CustomFormErrorText.Text = "";
        }

        private void CancelCustom_Click(object sender, RoutedEventArgs e)
        {
            CustomExercisePanel.Visibility = Visibility.Collapsed;
            TrackerPanel.Visibility = Visibility.Visible;
        }

        private void SaveCustom_Click(object sender, RoutedEventArgs e)
        {
            string name = CustomNameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                CustomFormErrorText.Text = "Exercise name is required.";
                return;
            }

            string equipment = (CustomEquipCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "None";
            string primary = (CustomPrimaryCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Other";
            string secondary = string.Join(", ", CustomSecondaryList.SelectedItems
                .Cast<ListBoxItem>()
                .Select(i => i.Content?.ToString() ?? ""));

            string category = primary switch
            {
                "Chest" or "Shoulders" or "Triceps" => "Push",
                "Back" or "Biceps" => "Pull",
                "Legs" => "Legs",
                "Cardio" => "Cardio",
                _ => "Push"
            };

            string exerciseType = (CustomTypeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Weight & Reps";

            exercises.Add(new Exercise
            {
                Name = name,
                Category = category,
                Muscle = primary,
                Equipment = equipment,
                ExerciseType = exerciseType,
                SecondaryMuscles = secondary,
                SubMuscle = secondary
            });

            exerciseService.Save(exercises.ToList());

            CustomExercisePanel.Visibility = Visibility.Collapsed;
            TrackerPanel.Visibility = Visibility.Visible;
        }

        // ========== DAILY SUGGESTIONS ==========

        private void RefreshSuggestions()
        {
            if (SuggestionItemsControl == null || exercises == null) return;

            currentSettings ??= SafeLoadSettings();
            var (title, categories) = GetSuggestionPlan(currentSettings);
            SuggestionTitleText.Text = title;

            var pool = exercises
                .Where(e => categories.Contains(e.Category, StringComparer.OrdinalIgnoreCase))
                .Select(e => e.Name)
                .Distinct()
                .OrderBy(n => n)
                .Take(3)
                .ToList();

            if (!pool.Any())
            {
                pool = exercises.Select(e => e.Name).Distinct().OrderBy(n => n).Take(3).ToList();
            }

            SuggestionItemsControl.ItemsSource = pool;
        }

        private static (string Title, string[] Categories) GetSuggestionPlan(UserSettings settings)
        {
            string split = (settings.SplitPlanId ?? "PPL").Trim().Replace(" ", "").Replace("-", "").Replace("_", "").ToLowerInvariant();
            DayOfWeek day = DateTime.Today.DayOfWeek;

            if (split == "ppl")
            {
                return day switch
                {
                    DayOfWeek.Monday or DayOfWeek.Thursday => ("Push Day Suggestions", new[] { "Push" }),
                    DayOfWeek.Tuesday or DayOfWeek.Friday => ("Pull Day Suggestions", new[] { "Pull" }),
                    DayOfWeek.Wednesday or DayOfWeek.Saturday => ("Legs Day Suggestions", new[] { "Legs" }),
                    _ => ("Active Recovery / Cardio", new[] { "Cardio" })
                };
            }

            if (split == "upperlower")
            {
                return day switch
                {
                    DayOfWeek.Monday or DayOfWeek.Thursday => ("Upper Day Suggestions", new[] { "Push", "Pull" }),
                    DayOfWeek.Tuesday or DayOfWeek.Friday => ("Lower Day Suggestions", new[] { "Legs" }),
                    _ => ("Cardio / Core Focus", new[] { "Cardio" })
                };
            }

            return ("Full Body Mix", new[] { "Push", "Pull", "Legs", "Cardio" });
        }
    }
}
