using Bamboo.Reminders;
using Bamboo.Voice;
using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Bamboo.Views
{
    public sealed partial class Schedule : UserControl
    {
        private ReminderManager reminderManager;
        private VoiceManager voiceManager;

        public Schedule()
        {
            this.InitializeComponent();
        }

        public void SetReminderManager(ReminderManager reminderManager)
        {
            this.reminderManager = reminderManager;
            this.updateReminderStack();
        }

        public void SetVoiceManager(VoiceManager voiceManager)
        {
            this.voiceManager = voiceManager;
        }

        private async void AddReminderText_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            await this.reminderManager.UpdateReminder(new Reminder($"Reminder {this.reminderManager.Reminders.Count + 1}", DateTime.Now.AddHours(1), true));
            this.updateReminderStack();
        }

        private async void ReminderControl_ReminderChanged(object sender, Reminder e)
        {
            await this.reminderManager.UpdateReminder(e);
        }

        private void updateReminderStack()
        {
            this.ReminderStack1.Children.Clear();
            this.ReminderStack2.Children.Clear();
            this.ReminderStack3.Children.Clear();

            int reminderCount = 0;
            foreach (var reminder in this.reminderManager.Reminders)
            {
                reminder.StopTimer();
                reminder.StartTimer();
                reminder.ReminderFired += Reminder_ReminderFired;

                var reminderControl = new ReminderControl(reminder);
                reminderControl.ReminderChanged += ReminderControl_ReminderChanged;

                if (reminderCount < 5)
                {

                    this.ReminderStack1.Children.Add(reminderControl);
                }
                else if (reminderCount < 10)
                {
                    this.ReminderStack2.Children.Add(reminderControl);
                }
                else if (reminderCount < 15)
                {
                    this.ReminderStack3.Children.Add(reminderControl);
                }

                reminderCount++;
            }

            var addReminderText = new TextBlock();
            addReminderText.Text = "+ Add Reminder";
            addReminderText.Foreground = new SolidColorBrush(Colors.Green);
            addReminderText.Margin = new Thickness(0, 15, 0, 0);
            addReminderText.PointerReleased += AddReminderText_PointerReleased;

            this.ReminderStack3.Children.Add(addReminderText);
        }

        private void Reminder_ReminderFired(Reminder sender)
        {
            this.voiceManager?.Speak("Have you checked your blood sugar?");
        }
    }
}
