using Bamboo.Reminders;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Bamboo.Views
{
    public delegate void ReminderChangedEventHandler(object sender, Reminder e);

    public sealed partial class ReminderControl : UserControl
    {
        private bool isInitializing;
        private Reminder reminder;

        public event ReminderChangedEventHandler ReminderChanged;
        public Reminder Reminder
        {
            get
            {
                return this.reminder;
            }
            set
            {
                this.reminder = value;
                this.ReminderChanged(this, this.reminder);
            }
        }

        public ReminderControl(Reminder reminderForControl)
        {
            this.isInitializing = true;
            this.InitializeComponent();
            this.reminder = reminderForControl;
            this.ReminderName.Text = this.reminder.Name;
            this.ReminderEnabled.IsOn = this.reminder.IsEnabled;

            if (this.reminder.Time.Hour == 0)
            {
                this.HourCombo.SelectedIndex = 11;
            }
            else if (this.reminder.Time.Hour <= 12)
            {
                this.HourCombo.SelectedIndex = this.reminder.Time.Hour - 1;
                this.AMPMCombo.SelectedIndex = 0;
            }
            else
            {
                this.HourCombo.SelectedIndex = this.reminder.Time.Hour - 13;
                this.AMPMCombo.SelectedIndex = 1;
            }

            if (this.reminder.Time.Minute < 15)
            {
                this.MinuteCombo.SelectedIndex = 0;
            }
            else if (this.reminder.Time.Minute < 30)
            {
                this.MinuteCombo.SelectedIndex = 1;
            }
            else if (this.reminder.Time.Minute < 45)
            {
                this.MinuteCombo.SelectedIndex = 2;
            }
            else
            {
                this.MinuteCombo.SelectedIndex = 3;
            }

            this.isInitializing = false;
        }

        private void Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.updateReminder();
        }

        private void updateReminder()
        {
            if (this.isInitializing)
                return;

            var hour = this.HourCombo.SelectedIndex + 1;
            if (this.AMPMCombo.SelectedIndex == 1)
            {
                hour += 12;
                if (24 == hour)
                    hour = 0;
            }

            var minute = this.MinuteCombo.SelectedIndex * 15;
            var time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, minute, 0);

            this.Reminder = new Reminder(this.reminder.Name, time, this.ReminderEnabled.IsOn);
        }

        private void ReminderEnabled_Toggled(object sender, RoutedEventArgs e)
        {
            this.updateReminder();
        }
    }
}
