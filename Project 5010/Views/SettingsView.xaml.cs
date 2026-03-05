using System.Globalization;
using System.Windows.Controls;
using Project_5010.Models;
using Project_5010.Services;

namespace Project_5010.Views
{
    public partial class SettingsView : UserControl
    {
        private readonly SettingsFileService settingsService;
        private UserSettings settings;

        public SettingsView()
        {
            InitializeComponent();

            settingsService = new SettingsFileService();
            settings = settingsService.Load();

            LoadIntoUI();
        }

        private void LoadIntoUI()
        {
            NameTextBox.Text = settings.DisplayName;
            AgeTextBox.Text = settings.Age.ToString(CultureInfo.InvariantCulture);
            HeightTextBox.Text = settings.HeightCm.ToString(CultureInfo.InvariantCulture);
            WeightTextBox.Text = settings.WeightKg.ToString(CultureInfo.InvariantCulture);

            // Match ComboBox items
            SplitCombo.SelectedIndex = settings.SplitPlan switch
            {
                "PPL" => 0,
                "UpperLower" => 1,
                "FullBody" => 2,
                "Bro" => 3,
                _ => 0
            };
        }

        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!int.TryParse(AgeTextBox.Text, out int age)) age = 18;
            if (!double.TryParse(HeightTextBox.Text, out double height)) height = 170;
            if (!double.TryParse(WeightTextBox.Text, out double weight)) weight = 70;

            string split = ((ComboBoxItem)SplitCombo.SelectedItem).Content?.ToString() ?? "PPL";

            settings.DisplayName = NameTextBox.Text?.Trim() ?? "User";
            settings.Age = age;
            settings.HeightCm = height;
            settings.WeightKg = weight;
            settings.SplitPlan = split;

            settingsService.Save(settings);

            StatusTextBlock.Text = "Saved!";
        }
    }
}