using Project_5010.Services;
using Project_5010.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Project_5010.Views
{
    public partial class SettingsView : UserControl
    {
        private readonly SettingsFileService _settingsService;
        private readonly string _username;
        private UserSettings _settings;

        public event Action<UserSettings>? SettingsSaved;
        public event Action? LogoutRequested;

        public SettingsView(SettingsFileService settingsService, string username = "default")
        {
            InitializeComponent();
            _settingsService = settingsService;
            _username = username;
            _settings = new UserSettings();
            ReloadFromService();
        }

        public void ReloadFromService()
        {
            _settings = _settingsService.Load(_username);
            LoadIntoUi();
            UpdatePreview();
            StatusTextBlock.Text = string.Empty;
        }

        private void LoadIntoUi()
        {
            DisplayNameTextBox.Text = _settings.DisplayName;
            HeightTextBox.Text = _settings.HeightCm.ToString("0.##", CultureInfo.InvariantCulture);
            WeightTextBox.Text = _settings.WeightKg.ToString("0.##", CultureInfo.InvariantCulture);
            AgeTextBox.Text = _settings.Age > 0 ? _settings.Age.ToString() : "";
            SexCombo.SelectedIndex = _settings.Sex == "Female" ? 1 : 0;
            ActivityLevelCombo.SelectedIndex = _settings.ActivityLevel switch
            {
                "Sedentary"   => 0,
                "Light"       => 1,
                "Active"      => 3,
                "Very Active" => 4,
                _             => 2
            };
            GoalTypeCombo.SelectedIndex = _settings.GoalType switch
            {
                "Lose Weight" => 0,
                "Gain Weight" => 2,
                _             => 1
            };

            switch (NormalizeSplitId(_settings.SplitPlanId))
            {
                case "UpperLower":
                    UpperLowerRadioButton.IsChecked = true;
                    break;
                case "FullBody":
                    FullBodyRadioButton.IsChecked = true;
                    break;
                case "BroSplit":
                    BroSplitRadioButton.IsChecked = true;
                    break;
                default:
                    PplRadioButton.IsChecked = true;
                    break;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string displayName = string.IsNullOrWhiteSpace(DisplayNameTextBox.Text)
                ? "Athlete"
                : DisplayNameTextBox.Text.Trim();

            bool heightValid = double.TryParse(HeightTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double heightCm);
            bool weightValid = double.TryParse(WeightTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double weightKg);

            if (!heightValid)
            {
                heightCm = 170;
            }

            if (!weightValid)
            {
                weightKg = 70;
            }

            _settings.DisplayName = displayName;
            _settings.HeightCm = heightCm;
            _settings.WeightKg = weightKg;
            if (int.TryParse(AgeTextBox.Text, out int age) && age > 0)
                _settings.Age = age;
            _settings.Sex = ((ComboBoxItem)SexCombo.SelectedItem)?.Content?.ToString() ?? "Male";
            _settings.ActivityLevel = ((ComboBoxItem)ActivityLevelCombo.SelectedItem)?.Content?.ToString() ?? "Moderate";
            _settings.GoalType = ((ComboBoxItem)GoalTypeCombo.SelectedItem)?.Content?.ToString() ?? "Maintain";
            _settings.SplitPlanId = GetSelectedSplitId();

            _settingsService.Save(_settings, _username);

            StatusTextBlock.Text = (heightValid && weightValid)
                ? "Settings saved successfully."
                : "Settings saved. Invalid numeric values were replaced with defaults.";

            UpdatePreview();
            SettingsSaved?.Invoke(_settings);
        }

        private void SplitRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            string displayName = string.IsNullOrWhiteSpace(DisplayNameTextBox.Text)
                ? "Athlete"
                : DisplayNameTextBox.Text.Trim();

            string splitId = GetSelectedSplitId();

            PreviewNameText.Text = displayName;
            PreviewSplitText.Text = ToPrettySplit(splitId) + " split";

            switch (splitId)
            {
                case "UpperLower":
                    LibraryEffectText.Text = "Likely Library tabs: Upper, Lower, Cardio.";
                    break;
                case "FullBody":
                    LibraryEffectText.Text = "Likely Library tabs: Full Body, Accessories, Cardio.";
                    break;
                case "BroSplit":
                    LibraryEffectText.Text = "Likely Library tabs: Chest, Back, Legs, Shoulders, Arms.";
                    break;
                default:
                    LibraryEffectText.Text = "Likely Library tabs: Push, Pull, Legs, Cardio.";
                    break;
            }
        }

        private string GetSelectedSplitId()
        {
            if (UpperLowerRadioButton.IsChecked == true)
            {
                return "UpperLower";
            }

            if (FullBodyRadioButton.IsChecked == true)
            {
                return "FullBody";
            }

            if (BroSplitRadioButton.IsChecked == true)
            {
                return "BroSplit";
            }

            return "PPL";
        }

        private static string NormalizeSplitId(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "PPL";
            }

            string normalized = value.Trim().Replace(" ", string.Empty).Replace("/", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty);

            switch (normalized.ToLowerInvariant())
            {
                case "upperlower":
                    return "UpperLower";
                case "fullbody":
                    return "FullBody";
                case "brosplit":
                    return "BroSplit";
                default:
                    return "PPL";
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            LogoutRequested?.Invoke();
        }

        private static string ToPrettySplit(string splitId)
        {
            switch (NormalizeSplitId(splitId))
            {
                case "UpperLower":
                    return "Upper / Lower";
                case "FullBody":
                    return "Full Body";
                case "BroSplit":
                    return "Bro Split";
                default:
                    return "PPL";
            }
        }
    }
}