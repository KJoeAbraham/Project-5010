\# Momentum (MNTM) — ECE 5010 Group 10



\## What this is

WPF desktop fitness tracker. C# / .NET 10 / Windows only.

Inspired by Hevy app. Local JSON storage only, no database, no cloud.



\## Project folder

C:\\Users\\MSI-\\RiderProjects\\MNTM\\Project 5010



\## Who owns what

\- WorkoutsView.xaml / .cs     → Ahmed (workout logging + calorie tracking)

\- LibraryView.xaml / .cs      → Abdul (125 exercises + PR system)

\- DashboardView.xaml / .cs    → Kevin (stats + bar chart + observer)

\- MainWindow.xaml / .cs       → Kevin (sidebar navigation shell)



\## Architecture

\- 4 layers: Views → Services → Models → JSON files in Data/

\- Observer pattern: ObservableCollection<Workout> + CollectionChanged

\- Strategy pattern: exercise filtering via ICollectionView predicates

\- MVVM partial: code-behind Phase 1, full ViewModels Phase 2



\## Important rules

\- Dark theme only — all brushes in App.xaml as StaticResources

\- NEVER use CharacterSpacing — WPF does not support it (UWP only)

\- NEVER add NuGet packages without team agreement

\- Target: net10.0-windows



\## Key models

\- Workout: Id, Title, Type, Date, DurationMinutes, CaloriesBurned, Notes

\- Exercise: Id, Name, Category, Muscle, SecondaryMuscles, Equipment, ExerciseType, IsCustom

\- PersonalRecord: Id, ExerciseName, Weight, Reps, Date, Notes



\## Build

Open MNTM\\Momentum.sln in JetBrains Rider → Shift+F10

```



\---



\## Now you can ask Claude Code anything



Once `claude` is running, just type naturally:

```

> fix all the build errors

```

```

> add a filter by workout type to the history list in WorkoutsView

```

```

> why is the exercise list showing "Push Accessory 50" instead of real exercises

```

```

> show me all the files Ahmed needs to edit

```

```

> commit my changes and push to GitHub

