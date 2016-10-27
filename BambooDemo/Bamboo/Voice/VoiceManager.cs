using Bamboo.Utilities;
using Microsoft.Maker.Media.UniversalMediaEngine;
using System;
using System.Threading.Tasks;
using Windows.Media.SpeechSynthesis;

namespace Bamboo.Voice
{
    public class VoiceManager
    {
        private SpeechSynthesizer speechSynthesizer;
        private MediaEngine mediaEngine;

        public VoiceManager()
        {
            this.speechSynthesizer = new SpeechSynthesizer();
            this.mediaEngine = new MediaEngine();
        }

        public async Task Initialize()
        {
            var result = await mediaEngine.InitializeAsync();
            if (MediaEngineInitializationResult.Success != result)
            {
                Logger.GetInstance().LogLine("Failed to start MediaEngine");
            }
        }

        public async Task Speak(string text)
        {
            var announceSsml = @"<speak version='1.0' " +
            "xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'><voice gender='female'> " +
            text +
            "</voice></speak>";
            SpeechSynthesisStream stream = await this.speechSynthesizer.SynthesizeSsmlToStreamAsync(announceSsml);

            mediaEngine.PlayStream(stream);
        }
    }
}
