# Momentum

A desktop fitness and nutrition tracking application built with WPF and .NET 10.

## Features

- **Authentication** — Multi-user login/registration with PBKDF2-SHA256 password hashing
- **Dashboard** — Weekly activity overview, calorie progress, workout type breakdown, session stats, and daily highlights
- **Workout Logging** — Log workouts by type (Strength, Cardio, Flexibility) with duration, date, and notes; filter and edit history
- **Exercise Library** — 150+ pre-seeded exercises organized by training split (PPL, Upper/Lower, Full Body, Bro Split) with personal record tracking
- **Nutrition Tracking** — Daily calorie goal calculated via Mifflin-St Jeor equation, food logging by meal type, and visual progress bar
- **Settings & Profile** — Height, weight, age, sex, activity level, goal type, training split, and unit preferences

## Tech Stack

| Layer | Technology |
|-------|------------|
| Framework | .NET 10.0 (Windows) |
| UI | WPF (Windows Presentation Foundation) |
| Language | C# |
| Storage | Local JSON files (`%LOCALAPPDATA%/Momentum/`) |
| Architecture | MVVM-style (Views, Models, Services) |

## Project Structure

```
Project 5010/
├── App.xaml                  # Global styles and resources
├── LoginWindow.xaml          # Authentication screen
├── MainWindow.xaml           # Application shell with sidebar navigation
├── Models/
│   ├── UserAccount.cs        # Login credentials
│   ├── UserProfile.cs        # Extended profile (BMI, age calculations)
│   ├── UserSettings.cs       # Preferences (height, weight, split, goals)
│   ├── Workout.cs            # Workout session data
│   ├── Exercise.cs           # Exercise library entries
│   ├── FoodEntry.cs          # Nutrition tracking entries
│   ├── Goal.cs               # Fitness/nutrition goals
│   └── PersonalRecord.cs     # PR tracking
├── Services/
│   ├── AuthService.cs        # Registration & login
│   ├── CalorieCalculator.cs  # BMR & daily calorie goal
│   ├── SettingsFileService.cs
│   ├── WorkoutFileService.cs
│   ├── ExerciseFileService.cs
│   ├── FoodFileService.cs
│   └── PRFileService.cs
└── Views/
    ├── DashboardView.xaml     # Stats, charts, highlights
    ├── WorkoutsView.xaml      # Log & manage workouts
    ├── LibraryView.xaml       # Exercise library & PRs
    ├── GoalsView.xaml         # Calorie tracking & food log
    └── SettingsView.xaml      # Profile & preferences
```

## Data Storage

All data is stored locally as JSON files:

```
%LOCALAPPDATA%/Momentum/
├── users.json
└── Profiles/<username>/
    ├── settings.json
    ├── workouts.json
    ├── food.json
    ├── prs.json
    └── exercises_v3.json
```

## Getting Started

### Prerequisites

- Windows 10/11
- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Build & Run

```bash
dotnet build
dotnet run
```

Or open `Project_5010.sln` in Visual Studio / Rider and press F5.

## Calorie Calculation

Daily calorie goals use the **Mifflin-St Jeor** equation:

- **Male**: BMR = 10 * weight(kg) + 6.25 * height(cm) - 5 * age - 161 + 166
- **Female**: BMR = 10 * weight(kg) + 6.25 * height(cm) - 5 * age - 161

Daily goal = BMR * activity factor + goal adjustment

| Activity Level | Factor |
|---------------|--------|
| Sedentary | 1.2 |
| Light | 1.375 |
| Moderate | 1.465 |
| Active | 1.55 |
| Very Active | 1.725 |

| Goal | Adjustment |
|------|-----------|
| Lose Weight | -400 kcal |
| Maintain | 0 |
| Gain Weight | +300 kcal |
