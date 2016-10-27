using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Bamboo.Reminders
{
    public class ReminderManager
    {
        public  List<Reminder> Reminders;

        public ReminderManager()
        {
            this.Reminders = new List<Reminder>();
        }

        public async Task Initialize()
        {
            await this.loadFromFile();
        }

        public async Task UpdateReminder(Reminder reminder)
        {
            if (this.Reminders.Any(r => r.Name == reminder.Name))
            {
                this.Reminders.Remove(this.Reminders.First(r => r.Name == reminder.Name));
            }

            this.Reminders.Add(reminder);

            await this.saveToFile();
        }

        public async Task DeleteReminder(Reminder reminder)
        {
            reminder.StopTimer();
            if (this.Reminders.Any(r => r.Name == reminder.Name))
            {
                this.Reminders.Remove(this.Reminders.First(r => r.Name == reminder.Name));
            }

            await this.saveToFile();
        }

        private async Task loadFromFile()
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile file;

            try
            {
                file = await folder.GetFileAsync("reminders.xml");

                using (var stream = await file.OpenStreamForReadAsync())
                {
                    var remindersXml = XElement.Load(stream);

                    foreach (var reminderXml in remindersXml.Elements("Reminder"))
                    {
                        this.Reminders.Add(Reminder.FromXml(reminderXml));
                    }
                    this.Reminders = this.Reminders.OrderBy(r => r.Time).ToList();
                }
            }
            catch(FileNotFoundException)
            {
                this.createDefaultReminders();
                await this.saveToFile();
            }
        }

        private async Task saveToFile()
        {
            var xml = new XElement("Reminders");

            foreach(var reminder in this.Reminders)
            {
                xml.Add(reminder.ToXml());
            }

            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile file = await folder.CreateFileAsync("reminders.xml", CreationCollisionOption.ReplaceExisting);

            using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (IOutputStream outputStream = fileStream.GetOutputStreamAt(0))
                {
                    using (DataWriter dataWriter = new DataWriter(outputStream))
                    {
                        //TODO: Replace "Bytes" with the type you want to write.
                        dataWriter.WriteString(xml.ToString());
                        await dataWriter.StoreAsync();
                        dataWriter.DetachStream();
                    }

                    await outputStream.FlushAsync();
                }
            }
        }

        private void createDefaultReminders()
        {
            this.Reminders.Add(new Reminder("Reminder 1", new DateTime(1066, 10, 14, 7, 30, 0), true));
            this.Reminders.Add(new Reminder("Reminder 2", new DateTime(1066, 10, 14, 9, 45, 0), false));
            this.Reminders.Add(new Reminder("Reminder 3", new DateTime(1066, 10, 14, 11, 15, 0), false));
            this.Reminders.Add(new Reminder("Reminder 4", new DateTime(1066, 10, 14, 14, 00, 0), true));
            this.Reminders.Add(new Reminder("Reminder 5", new DateTime(1066, 10, 14, 17, 30, 0), true));

            this.Reminders.Add(new Reminder("Reminder 6", new DateTime(1066, 10, 14, 19, 45, 0), false));
            this.Reminders.Add(new Reminder("Reminder 7", new DateTime(1066, 10, 14, 21, 30, 0), true));
            this.Reminders.Add(new Reminder("Reminder 8", new DateTime(1066, 10, 14, 00, 00, 0), false));
        }
    }
}
