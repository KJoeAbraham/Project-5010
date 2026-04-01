================================================================================
                        MOMENTUM - Fitness Tracker
                              README
================================================================================

PROJECT OVERVIEW
----------------
Momentum is a desktop fitness and nutrition tracking application built with
WPF (Windows Presentation Foundation) and C# on .NET 10. It allows users to
log workouts, track calories, browse an exercise library, set fitness goals,
and view dashboard analytics -- all stored locally on the user's machine.


INCORPORATING THE SOURCE CODE INTO A FUNCTIONAL PROJECT
-------------------------------------------------------
Prerequisites:
  - Windows 10 or 11
  - .NET 10 SDK (download from https://dotnet.microsoft.com/download)
  - (Optional) Visual Studio 2022+, JetBrains Rider, or VS Code

Steps:

  1. Extract the zip file to a folder of your choice.

  2. Open a terminal in the extracted folder (the one containing
     Project_5010.sln).

  3. Restore dependencies and build:
         dotnet restore
         dotnet build

  4. Run the application:
         dotnet run --project "Project 5010"

     Alternatively, open Project_5010.sln in Visual Studio or Rider and
     press F5 to build and run.

  5. On first launch you will see a login screen. Click "Register" to create
     a new account, then log in.

No external databases, APIs, or assets are required. All data is persisted as
JSON files under %LOCALAPPDATA%\Momentum\.


SOURCE CODE OVERVIEW
--------------------
All submitted files are source code authored by our group.

Project 5010/                   -- Main WPF project folder
|
|-- Project_5010.csproj         -- Project file (.NET 10, WPF)
|-- App.xaml / App.xaml.cs      -- Application entry point, global styles and
|                                  resource dictionaries (colors, brushes,
|                                  control templates for the dark theme)
|-- LoginWindow.xaml / .cs      -- Login and registration screen
|-- MainWindow.xaml / .cs       -- Application shell with sidebar navigation
|-- AssemblyInfo.cs             -- Assembly metadata
|
|-- Models/                     -- Data model classes
|   |-- UserAccount.cs          -- Credentials with salted PBKDF2 hashing
|   |-- UserProfile.cs          -- Extended profile (BMI, age calculations)
|   |-- UserSettings.cs         -- Preferences (height, weight, split, goals)
|   |-- Workout.cs              -- Workout session (type, date, duration, notes)
|   |-- Exercise.cs             -- Exercise entry (name, category, muscle, equipment)
|   |-- FoodEntry.cs            -- Food/calorie log entry
|   |-- Goal.cs                 -- Fitness or nutrition goal
|   |-- PersonalRecord.cs       -- Personal record (exercise, weight, reps, date)
|
|-- Services/                   -- Business logic and local file persistence
|   |-- AuthService.cs          -- Registration and login with PBKDF2-SHA256
|   |-- CalorieCalculator.cs    -- BMR via Mifflin-St Jeor equation; daily
|   |                              calorie goal with activity and goal adjustments
|   |-- SettingsFileService.cs  -- Read/write user settings JSON
|   |-- WorkoutFileService.cs   -- Read/write workout history JSON
|   |-- ExerciseFileService.cs  -- Read/write exercise library JSON; seeds
|   |                              150+ exercises on first run
|   |-- FoodFileService.cs      -- Read/write food log JSON
|   |-- PRFileService.cs        -- Read/write personal records JSON
|
|-- Views/                      -- WPF UserControls (UI pages)
    |-- DashboardView.xaml/.cs  -- Weekly stats, calorie bar, activity chart,
    |                              workout type breakdown, highlights
    |-- WorkoutsView.xaml/.cs   -- Log, edit, delete workouts; filter by type;
    |                              weekly summary
    |-- LibraryView.xaml/.cs    -- Exercise library organized by training split
    |                              (PPL, Upper/Lower, Full Body, Bro Split);
    |                              personal record logging and history
    |-- GoalsView.xaml/.cs      -- Daily calorie goal, food logging by meal
    |                              type, progress bar
    |-- SettingsView.xaml/.cs   -- Profile editing, activity level, goal type,
                                   training split selection, logout

Project_5010.sln                -- Visual Studio solution file (in parent folder)


KEY FEATURES
------------
  - Multi-user authentication with secure password hashing (PBKDF2-SHA256,
    100,000 iterations, 16-byte salt)
  - Dashboard with 7-day activity overview, calorie progress, workout type
    breakdown, and session statistics
  - Workout logging with type filtering (Strength, Cardio, Flexibility),
    editing, and deletion
  - Exercise library with 150+ pre-seeded exercises across Push, Pull, Legs,
    and Cardio categories
  - Personal record tracking per exercise
  - Calorie tracking using the Mifflin-St Jeor BMR equation with adjustable
    activity levels and weight goals
  - Food logging by meal type (Breakfast, Lunch, Dinner, Snack)
  - Configurable training splits: PPL, Upper/Lower, Full Body, Bro Split
  - Dark-themed modern UI with sidebar navigation
  - All data stored locally as JSON -- no external services required


AUXILIARY FILES / ADDITIONAL EFFORT
------------------------------------
The following materials were created during development but are not included
in this submission per the instructions:

  - UI Design Assets: Custom color palette, layout mockups, and icon
    selections were designed to achieve the polished dark-theme interface
    visible in the XAML resource dictionaries (defined inline in App.xaml).

  - Exercise Database: A library of 150+ exercises with category, muscle
    group, and equipment metadata was researched and compiled. This data is
    seeded programmatically in ExerciseFileService.cs rather than stored as
    a separate data file.

  - Design Report: A design report documenting architecture decisions,
    user stories, and UI/UX rationale was produced separately.


GROUP MEMBERS
-------------
(Add your group member names here)
