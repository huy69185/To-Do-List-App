using System;
using System.Windows;

namespace TodoListApp
{
    public partial class SettingsWindow : Window
    {
        public delegate void SettingsChangedEventHandler();
        public event SettingsChangedEventHandler? SettingsChanged;

        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            DarkModeCheckBox.IsChecked = Settings.Default.DarkMode;
            EnableNotificationsCheckBox.IsChecked = Settings.Default.EnableNotifications;
        }

        private void DarkModeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.DarkMode = true;
            Settings.Default.Save();
            SettingsChanged?.Invoke();
        }

        private void DarkModeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.DarkMode = false;
            Settings.Default.Save();
            SettingsChanged?.Invoke();
        }

        private void EnableNotificationsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.EnableNotifications = true;
            Settings.Default.Save();
            SettingsChanged?.Invoke();
        }

        private void EnableNotificationsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.EnableNotifications = false;
            Settings.Default.Save();
            SettingsChanged?.Invoke();
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
            SettingsChanged?.Invoke();
            MessageBox.Show("Settings saved successfully!");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ThemeManager.ApplySettings(this); // Apply settings when the window is loaded
        }
    }
}
