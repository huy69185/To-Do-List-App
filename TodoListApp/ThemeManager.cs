using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TodoListApp
{
    public static class ThemeManager
    {
        public static void ApplySettings(Window window)
        {
            SolidColorBrush textColor;
            SolidColorBrush backgroundColor;
            SolidColorBrush textBoxBackgroundColor;
            SolidColorBrush buttonBackgroundColor;

            if (Settings.Default.DarkMode)
            {
                backgroundColor = new SolidColorBrush(Color.FromRgb(45, 45, 48)); // Màu xám đậm
                textColor = new SolidColorBrush(Colors.White); // Màu chữ trắng
                textBoxBackgroundColor = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                buttonBackgroundColor = new SolidColorBrush(Color.FromRgb(52, 90, 242));
            }
            else
            {
                backgroundColor = new SolidColorBrush(Colors.White);
                textColor = new SolidColorBrush(Colors.Black);
                textBoxBackgroundColor = new SolidColorBrush(Colors.White);
                buttonBackgroundColor = new SolidColorBrush(Color.FromRgb(50, 205, 50));
            }

            window.Background = backgroundColor;

            foreach (var control in FindVisualChildren<Control>(window))
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
                else if (control is DatePicker datePicker)
                {
                    datePicker.Background = textBoxBackgroundColor;
                }
                else if (control is ListBox listBox)
                {
                    listBox.Background = textBoxBackgroundColor;
                }
            }

            foreach (var textBlock in FindVisualChildren<TextBlock>(window))
            {
                textBlock.Foreground = textColor;
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
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
    }
}
