using Bamboo.Utilities;
using RealTimeSpeechTranslateUWPSample;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Media.Render;

namespace Bamboo.Speech
{
    public delegate void ListenStateChangedEventHandler(object sender, ListenState e);
    public delegate void SourceLanguageChangedEventHandler(object sender, Language e);

    public class SpeechManager
    {

        public delegate Task commandExecutionRoutine();

        public event SourceLanguageChangedEventHandler SourceLanguageChanged;
        private Language sourceLanguage;
        public Language SourceLanguage
        {
            get
            {
                return this.sourceLanguage;
            }
            private set
            {
                this.sourceLanguage = value;
                this.SourceLanguageChanged(this, this.sourceLanguage);
            }
        }

        public event ListenStateChangedEventHandler ListenStateChanged;
        private ListenState listenState;
        public ListenState ListenState
        {
            get
            {
                return this.listenState;
            }
            private set
            {
                this.listenState = value;
                this.ListenStateChanged(this, this.listenState);
            }
        }

        // The keyword that triggers the robot to listen for commands.
        private string WakeWord = Configuration.ROBOT_NAME;
        private Timer listenTimer;

        private Dictionary<string, commandExecutionRoutine> commands;
        private SpeechTranslateClient speechTranslateClient;

        
        private AudioGraph graph;
        private AudioFrameOutputNode speechTranslateOutputMode;

        public SpeechManager()
        {
            this.commands = new Dictionary<string, commandExecutionRoutine>();

            if(String.IsNullOrEmpty(Secrets.AzureDataMarketClientId) || String.IsNullOrEmpty(Secrets.AzureDataMarketClientSecret))
            {
                throw new ArgumentException("Please provide values for AzureDataMarketClientId and AzureDataMarketClientSecret in Settings.cs");
            }
            this.speechTranslateClient = new SpeechTranslateClient(Secrets.AzureDataMarketClientId, Secrets.AzureDataMarketClientSecret);

            // Language Commands
            this.commands.Add("English.", async () => 
            {
                await this.changeSourceLanguage("en-US");
                this.SourceLanguage = Language.English;
            });

            this.commands.Add("Spanish.", async () =>
            {
                await this.changeSourceLanguage("es-ES");
                this.SourceLanguage = Language.Spanish;
            });

            this.commands.Add("German.", async () =>
            {
                await this.changeSourceLanguage("de-DE");
                this.SourceLanguage = Language.German;
            });

            this.commands.Add("French.", async () =>
            {
                await this.changeSourceLanguage("fr-FR");
                this.SourceLanguage = Language.French;
            });

            this.commands.Add("Italian.", async () =>
            {
                await this.changeSourceLanguage("it-IT");
                this.SourceLanguage = Language.Italian;
            });

            this.commands.Add("Chinese.", async () =>
            {
                await this.changeSourceLanguage("zh-CN");
                this.SourceLanguage = Language.Chinese;
            });

            this.commands.Add("Japanese.", async () =>
            {
                await this.changeSourceLanguage("ja-JP");
                this.SourceLanguage = Language.Japanese;
            });

            // Add additional supported languages here and in Language.cs
        }

        public async Task Initialize()
        {
            // Default the language to English
            this.ListenState = ListenState.Initializing;
            this.SourceLanguage = Language.English;
            try {
                await this.speechTranslateClient.Connect("en-US", "en", null, this.DisplayResult, this.SendAudioOut);

                var pcmEncoding = AudioEncodingProperties.CreatePcm(16000, 1, 16);

                var result = await AudioGraph.CreateAsync(
                  new AudioGraphSettings(AudioRenderCategory.Speech)
                  {
                      DesiredRenderDeviceAudioProcessing = AudioProcessing.Raw,
                      AudioRenderCategory = AudioRenderCategory.Speech,
                      EncodingProperties = pcmEncoding
                  });

                if (result.Status == AudioGraphCreationStatus.Success)
                {
                    this.graph = result.Graph;

                    var microphone = await DeviceInformation.CreateFromIdAsync(((DeviceInformation)(await DeviceInformation.FindAllAsync(MediaDevice.GetAudioCaptureSelector())).First()).Id);

                    this.speechTranslateOutputMode = this.graph.CreateFrameOutputNode(pcmEncoding);
                    this.graph.QuantumProcessed += (s, a) => this.SendToSpeechTranslate(this.speechTranslateOutputMode.GetFrame());

                    this.speechTranslateOutputMode.Start();

                    var micInputResult = await this.graph.CreateDeviceInputNodeAsync(MediaCategory.Speech, pcmEncoding, microphone);

                    if (micInputResult.Status == AudioDeviceNodeCreationStatus.Success)
                    {
                        micInputResult.DeviceInputNode.AddOutgoingConnection(this.speechTranslateOutputMode);
                        micInputResult.DeviceInputNode.Start();
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }

                    // start the graph
                    this.graph.Start();
                    this.ListenState = ListenState.NotListening;
                }
                else
                {
                    this.ListenState = ListenState.Error;
                }
            }
            catch(Exception e)
            {
                this.ListenState = ListenState.Error;
                Logger.GetInstance().LogLine(e.Message);
            }
            
        }

        public void AddCommand(string command, commandExecutionRoutine action)
        {
            if (!this.commands.ContainsKey(command))
            {
                this.commands.Add(command, action);
            }
        }

        private async void DisplayResult(Result result)
        {
            Logger.GetInstance().LogLine("Translation result: " + result.Translation);
            // If command word is present or
            // we are already listening extend the listening
            // preiod by 5 seconds
            if (result.Translation.Contains(WakeWord) ||
                this.ListenState == ListenState.Listening)
            {
                this.startListenTimeout(TimeSpan.FromSeconds(5));
            }
            else
            {
                return;
            }

            foreach (var commandString in this.commands)
            {
                if (CultureInfo.CurrentCulture.CompareInfo.IndexOf(result.Translation, commandString.Key, CompareOptions.IgnoreCase) >= 0)
                {
                    Logger.GetInstance().LogLine("Bamboo heard you ask for: " + commandString.Key);
                    await commandString.Value.Invoke();
                    break;
                }
            }
        }

        private void SendAudioOut(AudioFrame frame)
        {
            // The demo doesn't support text to speech from the tranlation service.
            // This can be enabled by passing a non-null voice string to the call
            // to SpeechTranslateClient.Connect. This method will then be called
            // when the translation service receives text to speech audio data.
        }

        private void SendToSpeechTranslate(AudioFrame frame)
        {
            this.speechTranslateClient.SendAudioFrame(frame);
        }

        private async Task changeSourceLanguage(string languageCode)
        {
            this.speechTranslateClient.Close();
            await this.speechTranslateClient.Connect(languageCode, "en", null, this.DisplayResult, this.SendAudioOut);
        }

        private void startListenTimeout(TimeSpan timeout)
        {
            this.ListenState = ListenState.Listening;
            if (null != this.listenTimer)
            {
                this.listenTimer.Dispose();
                this.listenTimer = null;
            }

            this.listenTimer = new Timer(this.stopListen, this, (int)timeout.TotalMilliseconds, Timeout.Infinite);
            Logger.GetInstance().LogLine("Bamboo is listening.");
        }

        private void stopListen(object state)
        {
            this.ListenState = ListenState.NotListening;
            Logger.GetInstance().LogLine("Bamboo is no longer listening.");
        }
    }
}
