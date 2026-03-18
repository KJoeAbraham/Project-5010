using Project_5010.Models;
using Project_5010.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace Project_5010.Views
{
    public partial class LibraryView : UserControl
    {
        private readonly SettingsFileService settingsService = new();
        private readonly ExerciseFileService exerciseService = new();
        private readonly PRFileService prService = new();

        private readonly ObservableCollection<Exercise> exercises;
        private readonly ObservableCollection<PersonalRecord> prs;

        private PersonalRecord? selectedPR;
        private string selectedCategory = "All";

        public LibraryView()
        {
            InitializeComponent();

            // Split affects which tabs to show (simple first version)
            var settings = settingsService.Load();
            ConfigureTabsForSplit(settings.SplitPlan);

            exercises = new ObservableCollection<Exercise>(exerciseService.Load());
            prs = new ObservableCollection<PersonalRecord>(prService.Load());

            ExercisesListView.ItemsSource = exercises;
            PRListView.ItemsSource = prs;

            ApplyExerciseFilter();
        }

        private void ConfigureTabsForSplit(string splitPlan)
        {
            // You can expand this mapping later
            // For now: show common tabs like the screenshot
            Tab1.Content = "Push";
            Tab2.Content = "Pull";
            Tab3.Content = "Legs";

            if (splitPlan == "UpperLower")
            {
                Tab1.Content = "Upper";
                Tab2.Content = "Lower";
                Tab3.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (splitPlan == "FullBody")
            {
                Tab1.Content = "FullBody";
                Tab2.Visibility = System.Windows.Visibility.Collapsed;
                Tab3.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                Tab2.Visibility = System.Windows.Visibility.Visible;
                Tab3.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyExerciseFilter();

        private void Tab_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                selectedCategory = b.Content?.ToString() ?? "All";
                ApplyExerciseFilter();
            }
        }

        private void ApplyExerciseFilter()
        {
            string q = (SearchTextBox.Text ?? "").Trim().ToLowerInvariant();
            ICollectionView view = CollectionViewSource.GetDefaultView(ExercisesListView.ItemsSource);

            view.Filter = obj =>
            {
                if (obj is not Exercise ex) return false;

                bool matchesSearch =
                    string.IsNullOrWhiteSpace(q) ||
                    ex.Name.ToLowerInvariant().Contains(q) ||
                    ex.Muscle.ToLowerInvariant().Contains(q) ||
                    ex.Equipment.ToLowerInvariant().Contains(q);

                bool matchesCategory =
                    selectedCategory == "All" ||
                    ex.Category == selectedCategory;

                return matchesSearch && matchesCategory;
            };

            view.Refresh();
        }

        private void ExercisesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ExercisesListView.SelectedItem is Exercise ex)
            {
                // Convenience: selecting an exercise fills PR Exercise name
                PRExerciseTextBox.Text = ex.Name;
                PRStatusText.Text = "Selected exercise for PR.";
            }
        }

        private void PRListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PRListView.SelectedItem is not PersonalRecord pr)
            {
                selectedPR = null;
                SavePRButton.Content = "Save PR";
                DeletePRButton.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }

            selectedPR = pr;
            PRExerciseTextBox.Text = pr.ExerciseName;
            PRWeightTextBox.Text = pr.Weight.ToString();
            PRNotesTextBox.Text = pr.Notes;

            SavePRButton.Content = "Update PR";
            DeletePRButton.Visibility = System.Windows.Visibility.Visible;
        }

        private void SavePRButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string exName = (PRExerciseTextBox.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(exName))
            {
                PRStatusText.Text = "Exercise name is required.";
                return;
            }

            if (!double.TryParse(PRWeightTextBox.Text, out double weight) || weight <= 0)
            {
                PRStatusText.Text = "Enter a valid weight.";
                return;
            }

            string notes = PRNotesTextBox.Text ?? "";

            if (selectedPR == null)
            {
                prs.Add(new PersonalRecord
                {
                    ExerciseName = exName,
                    Weight = weight,
                    Notes = notes
                });

                PRStatusText.Text = "PR saved!";
            }
            else
            {
                selectedPR.ExerciseName = exName;
                selectedPR.Weight = weight;
                selectedPR.Notes = notes;

                // Force refresh
                PRListView.ItemsSource = null;
                PRListView.ItemsSource = prs;

                PRStatusText.Text = "PR updated!";
            }

            prService.Save(prs.ToList());
        }

        private void DeletePRButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (selectedPR == null) return;

            prs.Remove(selectedPR);
            prService.Save(prs.ToList());

            selectedPR = null;
            DeletePRButton.Visibility = System.Windows.Visibility.Collapsed;
            SavePRButton.Content = "Save PR";
            PRStatusText.Text = "PR deleted.";
        }
    }
}