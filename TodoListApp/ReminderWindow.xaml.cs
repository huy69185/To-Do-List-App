using System.Media;
using System.Windows;
using System.Windows.Resources;

namespace TodoListApp
{
    public partial class ReminderWindow : Window
    {
        private SoundPlayer player;

        public ReminderWindow()
        {
            InitializeComponent();
        }

        public void SetReminderText(string text)
        {
            PopupTextBlock.Text = text;
        }

        public void PlayReminderSound()
        {
            if (Settings.Default.EnableNotifications)
            {
                try
                {
                    Uri uri = new Uri("pack://application:,,,/Resources/alarm_sound.wav");
                    StreamResourceInfo info = Application.GetResourceStream(uri);
                    player = new SoundPlayer(info.Stream);
                    player.PlayLooping();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error playing sound: {ex.Message}\n{ex.InnerException?.Message}");
                }
            }
        }


        private void StopReminderButton_Click(object sender, RoutedEventArgs e)
        {
            if (player != null)
            {
                player.Stop();
            }
            this.Close();
        }
    }
}
