using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Resources;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace TodoListApp
{
    public partial class MainWindow : Window
    {
        private List<Note> notes;
        private DispatcherTimer reminderTimer;
        private Dictionary<DateTime, Note> reminders;
        private const string NotesFilePath = "notes.json";
        private ManageNotesWindow manageNotesWindow;
        private SettingsWindow settingsWindow;
        private ReminderWindow reminderWindow;

        public MainWindow()
        {
            InitializeComponent();
            AddToStartup();
            notes = new List<Note>();
            reminders = new Dictionary<DateTime, Note>();
            InitializeReminderTimer();
            LoadNotes();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplySettings();
        }

        private void ApplySettings()
        {
            ThemeManager.ApplySettings(this);

            if (manageNotesWindow != null)
            {
                ThemeManager.ApplySettings(manageNotesWindow);
            }

            if (settingsWindow != null)
            {
                ThemeManager.ApplySettings(settingsWindow);
            }

            if (Settings.Default.EnableNotifications)
            {
                TaskbarIcon.ShowBalloonTip("Notifications", "Notifications are enabled", BalloonIcon.Info);
            }
            else
            {
                TaskbarIcon.ShowBalloonTip("Notifications", "Notifications are disabled", BalloonIcon.Info);
            }
        }

        private void AddNoteButton_Click(object sender, RoutedEventArgs e)
        {
            string title = NoteTitleTextBox.Text;
            string detail = NoteDetailTextBox.Text;

            if (string.IsNullOrWhiteSpace(title) || title == "Enter note title...")
            {
                MessageBox.Show("Please enter a note title.");
                return;
            }

            if (string.IsNullOrWhiteSpace(detail) || detail == "Enter note detail...")
            {
                MessageBox.Show("Please enter note detail.");
                return;
            }

            Note note = new Note { Title = title, Detail = detail };
            notes.Add(note);
            NotesListBox.Items.Add(note.Title);
            NoteTitleTextBox.Clear();
            NoteDetailTextBox.Clear();

            SaveNotes();

            if (Settings.Default.EnableNotifications)
            {
                MessageBox.Show("Note added successfully!");
            }
        }

        private void SetReminderButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime? selectedDate = ReminderDatePicker.SelectedDate;
            if (selectedDate != null && ReminderTimePicker.Value.HasValue)
            {
                DateTime reminderDateTime = selectedDate.Value.Date + ReminderTimePicker.Value.Value.TimeOfDay;
                reminderDateTime = new DateTime(reminderDateTime.Year, reminderDateTime.Month, reminderDateTime.Day, reminderDateTime.Hour, reminderDateTime.Minute, 0);

                Note note = GetSelectedNote();
                if (note != null)
                {
                    note.Reminders.Add(reminderDateTime);
                    reminders[reminderDateTime] = note;
                    MessageBox.Show($"Reminder set for {reminderDateTime}");
                    SaveNotes();
                }
                else
                {
                    MessageBox.Show("Please select a note for the reminder.");
                }
            }
            else
            {
                MessageBox.Show("Please select a valid date and time for the reminder.");
            }
        }

        private Note GetSelectedNote()
        {
            if (NotesListBox.SelectedItem != null)
            {
                string selectedTitle = NotesListBox.SelectedItem.ToString();
                return notes.Find(note => note.Title == selectedTitle);
            }
            return null;
        }

        private void InitializeReminderTimer()
        {
            reminderTimer = new DispatcherTimer();
            reminderTimer.Interval = TimeSpan.FromSeconds(1);
            reminderTimer.Tick += ReminderTimer_Tick;
            reminderTimer.Start();
        }

        private void ReminderTimer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            List<DateTime> toRemove = new List<DateTime>();

            foreach (var reminder in reminders)
            {
                if (now >= reminder.Key && now.Second == 0)
                {
                    if (!reminder.Value.ProcessedReminders.Contains(reminder.Key))
                    {
                        if (Settings.Default.EnableNotifications)
                        {
                            ShowReminder(reminder.Value, reminder.Key);
                        }
                        toRemove.Add(reminder.Key); // Remove reminder after displaying notification
                    }
                }
            }

            foreach (var time in toRemove)
            {
                reminders.Remove(time);
            }
        }


        private void ShowReminder(Note note, DateTime reminderTime)
        {
            if (reminderWindow == null)
            {
                reminderWindow = new ReminderWindow();
                reminderWindow.Closed += (s, e) =>
                {
                    reminderWindow = null;
                    IsHitTestVisible = true;
                    note.ProcessedReminders.Add(reminderTime);
                    SaveNotes(); // Save the processed state of the reminder
                };
                reminderWindow.SetReminderText($"Reminder for: {note.Title}\n{note.Detail}");
                reminderWindow.PlayReminderSound();
                reminderWindow.Show();
                IsHitTestVisible = false; // Lock all interactions except for the reminder window
            }
        }

        private void NotesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (NotesListBox.SelectedItem != null)
            {
                string selectedTitle = NotesListBox.SelectedItem.ToString();
                Note selectedNote = notes.Find(note => note.Title == selectedTitle);
                if (selectedNote != null)
                {
                    NoteDetailsWindow noteDetailsWindow = new NoteDetailsWindow(selectedNote, notes);
                    noteDetailsWindow.SettingsChanged += ApplySettings;
                    noteDetailsWindow.ShowDialog();
                    NotesListBox.Items[NotesListBox.SelectedIndex] = selectedNote.Title;

                    SaveNotes();
                }
            }
        }

        private void ManageNotesButton_Click(object sender, RoutedEventArgs e)
        {
            if (manageNotesWindow == null)
            {
                manageNotesWindow = new ManageNotesWindow(notes);
                manageNotesWindow.NoteDeleted += UpdateNotesList;
                manageNotesWindow.Closed += (s, args) => manageNotesWindow = null;
                manageNotesWindow.Show();
                ThemeManager.ApplySettings(manageNotesWindow);
            }
            else
            {
                manageNotesWindow.Activate();
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (settingsWindow == null)
            {
                settingsWindow = new SettingsWindow();
                settingsWindow.SettingsChanged += ApplySettings;
                settingsWindow.SettingsChanged += () => ApplySettingsToWindow(manageNotesWindow);
                settingsWindow.SettingsChanged += () => ApplySettingsToWindow(settingsWindow);
                settingsWindow.Closed += (s, args) => settingsWindow = null;
                settingsWindow.Show();
                ThemeManager.ApplySettings(settingsWindow);
            }
            else
            {
                settingsWindow.Activate();
            }
        }

        private void ApplySettingsToWindow(Window window)
        {
            if (window != null)
            {
                ThemeManager.ApplySettings(window);
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text == "Enter note title..." || textBox.Text == "Enter note detail..." || textBox.Text == "HH:MM")
            {
                textBox.Text = string.Empty;
                textBox.Foreground = new SolidColorBrush(Settings.Default.DarkMode ? Colors.White : Colors.Black);
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                if (textBox.Name == "NoteTitleTextBox")
                {
                    textBox.Text = "Enter note title...";
                }
                else if (textBox.Name == "NoteDetailTextBox")
                {
                    textBox.Text = "Enter note detail...";
                }
                else if (textBox.Name == "ReminderTimeTextBox")
                {
                    textBox.Text = "HH:MM";
                }
                textBox.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        private void SaveNotes()
        {
            string json = JsonConvert.SerializeObject(notes, Formatting.Indented);
            File.WriteAllText(NotesFilePath, json);
        }

        private void LoadNotes()
        {
            if (File.Exists(NotesFilePath))
            {
                string json = File.ReadAllText(NotesFilePath);
                notes = JsonConvert.DeserializeObject<List<Note>>(json);
                foreach (var note in notes)
                {
                    NotesListBox.Items.Add(note.Title);
                    foreach (var reminder in note.Reminders)
                    {
                        reminders[reminder] = note;
                    }
                }
            }
        }

        private void UpdateNotesList()
        {
            NotesListBox.Items.Clear();
            foreach (var note in notes)
            {
                NotesListBox.Items.Add(note.Title);
            }

            SaveNotes();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            TaskbarIcon.Visibility = Visibility.Visible;
        }

        private void ShowMainWindow(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            TaskbarIcon.Visibility = Visibility.Collapsed;
        }

        private void ExitApplication(object sender, RoutedEventArgs e)
        {
            TaskbarIcon.Dispose();
            Application.Current.Shutdown();
        }

        private void AddToStartup()
        {
            string appName = "TodoListApp";
            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            key.SetValue(appName, $"\"{appPath}\"");
        }
    }

    public class Note
    {
        public string Title { get; set; }
        public string Detail { get; set; }
        public List<DateTime> Reminders { get; set; } = new List<DateTime>();
        public List<DateTime> ProcessedReminders { get; set; } = new List<DateTime>(); // Flag for processed reminders
    }
}
