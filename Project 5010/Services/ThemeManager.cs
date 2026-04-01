// ThemeManager.cs
// Switches between light and dark mode by swapping color resources.
// Every color in the app should use DynamicResource so it updates live.

using System.Windows;
using System.Windows.Media;

namespace Project_5010.Services
{
    public static class ThemeManager
    {
        public static void ApplyTheme(string theme)
        {
            bool d = theme == "Dark";

            // Main surfaces
            SetBrush("ShellBg",       d ? "#0B0F14" : "#F5F6FA");
            SetBrush("CardBg",        d ? "#111827" : "#FFFFFF");
            SetBrush("SidebarBg",     d ? "#0F1420" : "#FFFFFF");

            // Text colors
            SetBrush("TextPrimary",   d ? "#EAF2FF" : "#1D2435");
            SetBrush("TextSecondary", d ? "#8FA3BF" : "#727990");
            SetBrush("TextMuted",     d ? "#5A6A80" : "#959CB0");

            // Borders
            SetBrush("BorderSoft",    d ? "#1E2A3A" : "#E8EAF3");

            // Input fields
            SetBrush("InputBg",       d ? "#0D1219" : "#F3F4F6");
            SetBrush("InputText",     d ? "#D0DAEA" : "#333A50");

            // Progress bar tracks
            SetBrush("ProgressTrackBg", d ? "#1A2232" : "#EEF0F6");

            // Accent colors — purple stays the same in both themes
            SetBrush("AppAccent",      "#6D4AFF");
            SetBrush("AccentSoftBrush", d ? "#1A1530" : "#F0EBFF");
            SetBrush("AccentSoftBg",    d ? "#1A1530" : "#F5F3FF");

            // Status colors — slightly brighter in dark mode for readability
            SetBrush("SuccessColor",   d ? "#34D399" : "#10B981");
            SetBrush("DangerColor",    d ? "#F87171" : "#EF4444");
            SetBrush("DangerBg",       d ? "#2A1515" : "#FEF2F2");
            SetBrush("SuccessBg",      d ? "#0F2A1F" : "#F0FDF4");
            SetBrush("WarningBg",      d ? "#2A2010" : "#FFFBEB");
            SetBrush("WarningText",    d ? "#FBBF24" : "#92400E");

            // Nav button states
            SetBrush("BodyTextBrush",   d ? "#8FA3BF" : "#6E7386");
            SetBrush("NavHoverBg",      d ? "#1A2433" : "#F6F4FF");
            SetBrush("NavHoverBorder",  d ? "#2A3A4A" : "#E2DAFF");
            SetBrush("NavActiveBg",     d ? "#1A1530" : "#F0EBFF");
            SetBrush("NavActiveBorder", d ? "#3D2A7A" : "#DCCFFF");
        }

        private static void SetBrush(string key, string hex)
        {
            Color color = (Color)ColorConverter.ConvertFromString(hex);
            var brush = new SolidColorBrush(color);

            if (Application.Current.Resources.Contains(key))
                Application.Current.Resources[key] = brush;
            else
                Application.Current.Resources.Add(key, brush);
        }
    }
}
