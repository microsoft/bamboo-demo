using Bamboo.Models;
using Bamboo.Movement;
using Bamboo.Reminders;
using Bamboo.Speech;
using Bamboo.Utilities;
using Bamboo.Views;
using Bamboo.Voice;
using BroxtonDemo.Movement;
using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Bamboo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SpeechManager speechManager;
        private IMovementManager movementManager;
        private ReminderManager reminderManager;
        private VoiceManager voiceManager;
        private LEDStrip ledStrip;

        public MainPage()
        {
            this.speechManager = new SpeechManager();
            //this.movementManager = new EZBMovementManager();
            this.movementManager = new DebugMovementManager();
            this.reminderManager = new ReminderManager();
            this.voiceManager = new VoiceManager();
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ledStrip = new LEDStrip(11, 10, 46);
                this.ledStrip.SetStripColor(Colors.Black);
                this.ledStrip.Refresh();
            }
            catch(Exception ledException)
            {
                Logger.GetInstance().LogLine(ledException.Message);
            }

            //Init speech
            this.speechManager.ListenStateChanged += SpeechManager_ListenStateChanged;
            this.speechManager.SourceLanguageChanged += SpeechManager_SourceLanguageChanged;
            await movementManager.Initialize();

            await this.voiceManager.Initialize();
            

            await this.speechManager.Initialize();
            this.SetupVoiceCommands();
            
            this.MovePivot.SetMovementManager(this.movementManager);

            await this.reminderManager.Initialize();
            this.SchedulePivot.SetReminderManager(this.reminderManager);
            this.SchedulePivot.SetVoiceManager(this.voiceManager);

            Odometer.Instance.PositionChanged += Instance_PositionChanged;

            await this.voiceManager.Speak("Hi, my name is Bamboo.");
        }

        private async void Instance_PositionChanged(object sender, OdometryThresholdReachedEventArgs e)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                this.PositionTextBlock.Text = $"x:{e.Data.X.ToString("F4")} y:{e.Data.Y.ToString("F4")}";
                this.AngleTextBlock.Text = $"{e.Data.Theta.ToString("F1")}";
            });
        }

        private async void SpeechManager_SourceLanguageChanged(object sender, Language e)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                this.LanguageTextBlock.Text = e.ToString();
            });
        }

        private async void SpeechManager_ListenStateChanged(object sender, ListenState e)
        {
            switch(e)
            {
                case ListenState.Initializing:
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        this.SpeechStatusTextBlock.Text = "Initializing";
                    });
                    break;
                case ListenState.Listening:
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        this.SpeechStatusTextBlock.Text = "Listening";
                        this.ledStrip?.SetLEDColor(10, Colors.Blue);
                        this.ledStrip?.Refresh();
                    });
                    break;
                case ListenState.NotListening:
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        this.SpeechStatusTextBlock.Text = "Not Listening";
                        this.ledStrip?.SetLEDColor(10, Colors.Black);
                        this.ledStrip?.Refresh();
                    });
                    break;
                case ListenState.Error:
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        this.SpeechStatusTextBlock.Text = "Error";
                    });
                    break;
            }
        }

        private void SetupVoiceCommands()
        {
            this.speechManager.AddCommand("Left arm up", async () =>
            {
                await this.movementManager.LeftArmUp();
            });

            this.speechManager.AddCommand("Left arm down", async () =>
            {
                await this.movementManager.LeftArmDown();
            });

            this.speechManager.AddCommand("Right arm up", async () =>
            {
                await this.movementManager.RightArmUp();
            });

            this.speechManager.AddCommand("Right arm down", async () =>
            {
                await this.movementManager.RightArmDown();
            });

            this.speechManager.AddCommand("Forward", async () =>
            {
                await Task.FromResult(this.movementManager.MoveForward());
            });

            this.speechManager.AddCommand("Backward", async () =>
            {
                await this.movementManager.MoveBackward();
            });

            this.speechManager.AddCommand("Right", async () =>
            {
                await this.movementManager.TurnRight();
            });

            this.speechManager.AddCommand("Left", async () =>
            {
                await this.movementManager.TurnLeft();
            });

            this.speechManager.AddCommand("Dance", async () =>
            {
                await this.movementManager.Dance();
            });

            this.speechManager.AddCommand("Schedule", async () =>
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    this.Pivot.SelectedIndex = 0;
                });
            });

            this.speechManager.AddCommand("Move", async () =>
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    this.Pivot.SelectedIndex = 1;
                });
            });

            this.speechManager.AddCommand("Map", async () =>
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    this.Pivot.SelectedIndex = 2;
                });
            });
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mapping = ((PivotItem)((Pivot)sender).SelectedItem).Content as Mapping;
            var schedule = ((PivotItem)((Pivot)sender).SelectedItem).Content as Schedule;
            var move = ((PivotItem)((Pivot)sender).SelectedItem).Content as Move;

            if(move == null)
            {
                try
                {
                    MovePivot.FrameRendererStatus = false;
                }
                catch { }
                
            }
            else
            {
                try
                {
                    MovePivot.FrameRendererStatus = true; ;
                }
                catch { }
            }
        }
    }
}
