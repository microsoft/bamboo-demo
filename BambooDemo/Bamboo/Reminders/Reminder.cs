using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Bamboo.Reminders
{
    public delegate void ReminderFiredHandler(Reminder sender);

    public class Reminder
    {
        private Timer timer;

        public Reminder(Reminder lhs)
        {
            this.Name = lhs.Name;
            this.IsEnabled = lhs.IsEnabled;
            this.Time = new DateTime(lhs.Time.Year, 
                                        lhs.Time.Month, 
                                        lhs.Time.Day, 
                                        lhs.Time.Hour, 
                                        lhs.Time.Minute, 
                                        lhs.Time.Second);
        }

        public Reminder(string name, DateTime time, bool isEnabled)
        {
            this.Name = name;
            this.Time = time;
            this.IsEnabled = isEnabled;
        }

        public event ReminderFiredHandler ReminderFired;

        public string Name
        {
            get;
            set;
        }

        public DateTime Time
        {
            get;
            set;
        }

        public bool IsEnabled
        {
            get;
            set;
        }

        private void timerCallback(object state)
        {
            this.ReminderFired?.Invoke(this);
        }

        public void StartTimer()
        {
            var nowMilliseconds = DateTime.Now.TimeOfDay.TotalMilliseconds;
            var reminderMilliseconds = this.Time.TimeOfDay.TotalMilliseconds;

            if (nowMilliseconds < reminderMilliseconds)
            {
                this.timer = new Timer(this.timerCallback, null, (int)(reminderMilliseconds - nowMilliseconds), Timeout.Infinite);
            }
        }

        public void StopTimer()
        {
            if (this.timer != null)
            {
                this.timer.Dispose();
                this.timer = null;
            }
        }

        public static Reminder FromXml(XElement xml)
        {
            var name = xml.Element("Name").Value;
            var time = DateTime.Parse(xml.Element("Time").Value);
            var isEnabled = bool.Parse(xml.Element("IsEnabled").Value);
            return new Reminder(name, time, isEnabled);
        }

        public XElement ToXml()
        {
            var xml = new XElement("Reminder");

            xml.Add(new XElement("Name", this.Name));
            xml.Add(new XElement("Time", this.Time.ToString()));
            xml.Add(new XElement("IsEnabled", this.IsEnabled.ToString()));

            return xml;
        }
    }
}
