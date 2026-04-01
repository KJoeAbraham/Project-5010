// OnboardingWindow.cs
// This window shows up the first time a new user registers.
// It collects their basic profile info (name, height, weight, etc.)
// and saves it to their settings file so the app can calculate
// things like calorie goals right from the start.

using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Project_5010.Models;
using Project_5010.Services;

namespace Project_5010
{
    public partial class OnboardingWindow : Window
    {
        private readonly SettingsFileService _settingsService;
        private readonly string _username;

        // After the user clicks "Get Started", this holds their completed settings
        public UserSettings CompletedSettings { get; private set; } = new();

        public OnboardingWindow(SettingsFileService settingsService, string username)
        {
            InitializeComponent();
            _settingsService = settingsService;
            _username = username;

            // Pre-fill the name box with their username as a starting point
            NameBox.Text = username;
        }

        private void GetStarted_Click(object sender, RoutedEventArgs e)
        {
            // --- Validate the inputs ---
            string name = NameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                StatusText.Text = "Please enter your name.";
                return;
            }

            if (!double.TryParse(HeightBox.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double height) || height <= 0)
            {
                StatusText.Text = "Please enter a valid height in cm.";
                return;
            }

            if (!double.TryParse(WeightBox.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double weight) || weight <= 0)
            {
                StatusText.Text = "Please enter a valid weight in kg.";
                return;
            }

            if (!int.TryParse(AgeBox.Text.Trim(), out int age) || age <= 0 || age > 120)
            {
                StatusText.Text = "Please enter a valid age.";
                return;
            }

            // --- Read combo box selections ---
            string sex = ((ComboBoxItem)SexCombo.SelectedItem)?.Content?.ToString() ?? "Male";
            string activity = ((ComboBoxItem)ActivityCombo.SelectedItem)?.Content?.ToString() ?? "Moderate";
            string goal = ((ComboBoxItem)GoalCombo.SelectedItem)?.Content?.ToString() ?? "Maintain";
            int workoutsPerWeek = WorkoutsCombo.SelectedIndex + 1; // index 0 = "1", index 1 = "2", etc.
            bool hasExperience = ExperienceCombo.SelectedIndex == 0; // index 0 = "Yes"

            // --- Build and save settings ---
            UserSettings settings = _settingsService.Load(_username);
            settings.DisplayName = name;
            settings.HeightCm = height;
            settings.WeightKg = weight;
            settings.Age = age;
            settings.Sex = sex;
            settings.ActivityLevel = activity;
            settings.GoalType = goal;
            settings.WorkoutsPerWeek = workoutsPerWeek;
            settings.HasWorkedOutBefore = hasExperience;
            settings.IsOnboardingComplete = true;

            _settingsService.Save(settings, _username);

            CompletedSettings = settings;
            DialogResult = true;
            Close();
        }
    }
}
