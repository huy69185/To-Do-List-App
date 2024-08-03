using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TodoListApp
{
    public partial class NoteDetailsWindow : Window
    {
        private Note note;
        private List<Note> notes;

        public delegate void SettingsChangedEventHandler();
        public event SettingsChangedEventHandler? SettingsChanged;

        public NoteDetailsWindow(Note note, List<Note> notes)
        {
            InitializeComponent();
            this.note = note;
            this.notes = notes;
            NoteTitleTextBox.Text = note.Title;
            NoteDetailTextBox.Text = note.Detail;
            LoadReminders();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplySettings();
        }

        private void ApplySettings()
        {
            SolidColorBrush textColor;
            SolidColorBrush backgroundColor;
            SolidColorBrush textBoxBackgroundColor;
            SolidColorBrush buttonBackgroundColor;

            if (Settings.Default.DarkMode)
            {
                backgroundColor = new SolidColorBrush(Color.FromRgb(45, 45, 48)); // Màu xám đậm
                textColor = new SolidColorBrush(Colors.White); // Màu chữ trắng
                textBoxBackgroundColor = new SolidColorBrush(Color.FromRgb(30, 30, 30)); // Màu xám đậm hơn cho nền TextBox
                buttonBackgroundColor = new SolidColorBrush(Color.FromRgb(70, 70, 70)); // Màu xám cho nền Button
            }
            else
            {
                backgroundColor = new SolidColorBrush(Colors.White);
                textColor = new SolidColorBrush(Colors.Black);
                textBoxBackgroundColor = new SolidColorBrush(Colors.White);
                buttonBackgroundColor = new SolidColorBrush(Color.FromRgb(240, 240, 240)); // Màu xám nhạt cho nền Button
            }

            NoteDetailsGrid.Background = backgroundColor;

            foreach (var control in FindVisualChildren<Control>(this))
            {
                control.Foreground = textColor;

                if (control is TextBox textBox)
                {
                    textBox.Background = textBoxBackgroundColor;
                }
                else if (control is Button button)
                {
                    button.Background = buttonBackgroundColor;
                }
                else if (control is ListBox listBox)
                {
                    listBox.Background = textBoxBackgroundColor;
                    foreach (var item in FindVisualChildren<ListBoxItem>(listBox))
                    {
                        item.Background = textBoxBackgroundColor;
                        item.Foreground = textColor;
                    }
                }
            }

            foreach (var textBlock in FindVisualChildren<TextBlock>(this))
            {
                textBlock.Foreground = textColor;
            }
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void LoadReminders()
        {
            ReminderListBox.Items.Clear();
            foreach (var reminder in note.Reminders)
            {
                ReminderListBox.Items.Add(reminder.ToString("g"));
            }
        }

        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            note.Title = NoteTitleTextBox.Text;
            note.Detail = NoteDetailTextBox.Text;
            Close();
        }
    }
}
