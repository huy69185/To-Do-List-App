using System;
using System.Collections.Generic;
using System.Windows;

namespace TodoListApp
{
    public partial class ManageNotesWindow : Window
    {
        private List<Note> notes;

        public delegate void NoteDeletedEventHandler();
        public event NoteDeletedEventHandler? NoteDeleted;

        public ManageNotesWindow(List<Note> notes)
        {
            InitializeComponent();
            this.notes = notes;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ThemeManager.ApplySettings(this); // Áp dụng cài đặt khi cửa sổ được tải
            LoadNotes();
        }

        private void LoadNotes()
        {
            ManageNotesListBox.Items.Clear();
            foreach (var note in notes)
            {
                ManageNotesListBox.Items.Add(note.Title);
            }
        }

        private void DeleteNoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ManageNotesListBox.SelectedItem != null)
            {
                string selectedTitle = ManageNotesListBox.SelectedItem.ToString();
                Note selectedNote = notes.Find(note => note.Title == selectedTitle);
                if (selectedNote != null)
                {
                    MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete the note \"{selectedTitle}\"?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        notes.Remove(selectedNote);
                        ManageNotesListBox.Items.Remove(selectedTitle);
                        NoteDeleted?.Invoke(); // Kích hoạt sự kiện NoteDeleted
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a note to delete.");
            }
        }
    }
}
