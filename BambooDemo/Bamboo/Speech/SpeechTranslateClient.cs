// ----------------------------------------------------------------------
// <copyright file="SpeechTranslateClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// All rights reserved.
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
// </copyright>
// ----------------------------------------------------------------------
// <summary>SpeechTranslateClient.cs</summary>
// ----------------------------------------------------------------------

using Bamboo.Speech;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace RealTimeSpeechTranslateUWPSample
{
    class SpeechTranslateClient
    {
        public delegate void OnTextToSpeechData(AudioFrame frame);
        public delegate void OnSpeechTranslateResult(Result result);

        const string AzureMarketPlaceUrl = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
        const string AzureScope = "http://api.microsofttranslator.com";
        const string SpeechTranslateUrl = @"wss://dev.microsofttranslator.com/speech/translate?from={0}&to={1}{2}&api-version=1.0";
        private static readonly Encoding UTF8 = new UTF8Encoding();

        private MessageWebSocket webSocket;
        private DataWriter dataWriter;
        private string clientId;
        private string clientSecret;
        private Timer timer;
        private OnTextToSpeechData onTextToSpeechData;
        private OnSpeechTranslateResult onSpeechTranslateResult;

        /// <summary>
        /// Create a speech tarnslate client that will talk to the MT Service
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        public SpeechTranslateClient(string clientId, string clientSecret)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
        }

        /// <summary>
        /// Connect to the server before sending audio
        /// It will get the ADM credentials and add it to the header
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task Connect(string from, string to, string voice, OnSpeechTranslateResult onSpeechTranslateResult, OnTextToSpeechData onTextToSpeechData)
        {
            this.webSocket = new MessageWebSocket();
            this.onTextToSpeechData = onTextToSpeechData;
            this.onSpeechTranslateResult = onSpeechTranslateResult;

            // get Azure Data Market token
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(AzureMarketPlaceUrl, new FormUrlEncodedContent(
                    new KeyValuePair<string, string>[] {
                        new KeyValuePair<string,string>("grant_type", "client_credentials"),
                        new KeyValuePair<string,string>("client_id", clientId),
                        new KeyValuePair<string,string>("client_secret", clientSecret),
                        new KeyValuePair<string,string>("scope", AzureScope),
                    }));

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                dynamic admAccessToken = JObject.Parse(json);

                var admToken = "Bearer " + admAccessToken.access_token;

                this.webSocket.SetRequestHeader("Authorization", admToken);
            }

            var url = String.Format(SpeechTranslateUrl, from, to, voice == null ? String.Empty : "&features=texttospeech&voice=" + voice);

            this.webSocket.MessageReceived += WebSocket_MessageReceived;

            // setup the data writer
            this.dataWriter = new DataWriter(this.webSocket.OutputStream);
            this.dataWriter.ByteOrder = ByteOrder.LittleEndian;
            this.dataWriter.WriteBytes(GetWaveHeader());

            // connect to the service
            await this.webSocket.ConnectAsync(new Uri(url));

            // flush the dataWriter periodically
            this.timer = new Timer(async (s) => 
            {
                if (this.dataWriter.UnstoredBufferLength > 0)
                {
                    try
                    {
                        await this.dataWriter.StoreAsync();
                    }
                    catch (Exception e)
                    {
                        this.onSpeechTranslateResult(new Result() { Status = "DataWriter Failed: " + e.Message });
                    }
                }

                // reset the timer
                this.timer.Change(TimeSpan.FromMilliseconds(250), Timeout.InfiniteTimeSpan);
            }, 
            null, TimeSpan.FromMilliseconds(250), Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// Process the response from the websocket
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void WebSocket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            try
            {
                if (args.MessageType == SocketMessageType.Utf8)
                {
                    // parse the text result that contains the recognition and translation
                    // {"type":"final","id":"0","recognition":"Hello, can you hear me now?","translation":"Hallo, kannst du mich jetzt hören?"}
                    string jsonOutput;
                    using (var dataReader = args.GetDataReader())
                    {
                        dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                        jsonOutput = dataReader.ReadString(dataReader.UnconsumedBufferLength);
                    }
                    
                    var result = JsonConvert.DeserializeObject<Result>(jsonOutput);
                    this.onSpeechTranslateResult(result);
                }
                else if (args.MessageType == SocketMessageType.Binary)
                {
                    // the binary output is the text to speech audio
                    using (var dataReader = args.GetDataReader())
                    {
                        dataReader.ByteOrder = ByteOrder.LittleEndian;
                        this.onTextToSpeechData(AudioFrameHelper.GetAudioFrame(dataReader));
                    }
                }
            }
            catch (Exception e)
            {
                this.onSpeechTranslateResult(new Result() { Status = e.Message });
            }
        }

        /// <summary>
        /// Send audio frame to the Machine Translation Service
        /// </summary>
        /// <param name="frame"></param>
        public void SendAudioFrame(AudioFrame frame)
        {
            AudioFrameHelper.SendAudioFrame(frame, this.dataWriter);           
        }

        /// <summary>
        /// Disconnect the service
        /// </summary>
        public void Close()
        {
            this.timer.Change(Timeout.Infinite, Timeout.Infinite);
            this.webSocket.Close((ushort) 1000, "end of Stream");
        }

        /// <summary>
        /// Create a RIFF Wave Header for PCM 16bit 16kHz Mono
        /// </summary>
        /// <returns></returns>
        private byte[] GetWaveHeader()
        {
            var channels = (short)1;
            var sampleRate = 16000;
            var bitsPerSample = (short)16;
            var extraSize = 0;
            var blockAlign = (short)(channels * (bitsPerSample / 8));
            var averageBytesPerSecond = sampleRate * blockAlign;

            using (MemoryStream stream = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8);
                writer.Write(Encoding.UTF8.GetBytes("RIFF"));
                writer.Write(0);
                writer.Write(Encoding.UTF8.GetBytes("WAVE"));
                writer.Write(Encoding.UTF8.GetBytes("fmt "));
                writer.Write((int)(18 + extraSize)); // wave format length 
                writer.Write((short)1);// PCM
                writer.Write((short)channels);
                writer.Write((int)sampleRate);
                writer.Write((int)averageBytesPerSecond);
                writer.Write((short)blockAlign);
                writer.Write((short)bitsPerSample);
                writer.Write((short)extraSize);

                writer.Write(Encoding.UTF8.GetBytes("data"));
                writer.Write(0);

                stream.Position = 0;
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        /// <summary>
        /// Dispose the websocket client object
        /// </summary>
        public void Dispose()
        {
            if (this.webSocket != null)
            {
                this.webSocket.Dispose();
                this.webSocket = null;
            }
        }
    }

    /// <summary>
    /// IMemoryBuferByteAccess is used to access the underlying audioframe for read and write
    /// </summary>
    [ComImport]
    [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }

    internal static class AudioFrameHelper
    {
        /// <summary>
        /// This is a way to write to the audioframe 
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="writer"></param>
        internal static void SendAudioFrame(AudioFrame frame, DataWriter writer)
        {
            var audioBuffer = frame.LockBuffer(AudioBufferAccessMode.Read);
            var buffer = Windows.Storage.Streams.Buffer.CreateCopyFromMemoryBuffer(audioBuffer);
            buffer.Length = audioBuffer.Length;
            using (var dataReader = DataReader.FromBuffer(buffer))
            {
                dataReader.ByteOrder = ByteOrder.LittleEndian;
                while (dataReader.UnconsumedBufferLength > 0)
                {
                    writer.WriteInt16(FloatToInt16(dataReader.ReadSingle()));
                }
            }
        }

        /// <summary>
        /// AudioFrame is in IEEE 32bit format.  We need to convert it to 16 bit PCM and send it to the datawriter
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="writer"></param>
        unsafe internal static void SendAudioFrameNative(AudioFrame frame, DataWriter writer)
        {
            using (var buffer = frame.LockBuffer(AudioBufferAccessMode.Read))
            using (IMemoryBufferReference reference = buffer.CreateReference())
            {
                byte* dataInBytes;
                uint capacityInBytes;

                // Get the buffer from the AudioFrame
                ((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacityInBytes);

                // convert the bytes into float
                float* dataInFloat = (float*)dataInBytes;

                for (int i = 0; i < capacityInBytes / sizeof(float); i++)
                {
                    // convert the float into a double byte for 16 bit PCM
                    writer.WriteInt16(FloatToInt16(dataInFloat[i]));
                }
            }
        }

        /// <summary>
        /// The bytes that we get from audiograph is in IEEE float, we need to covert that to 16 bit
        /// before sending it to the speech translate service
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static Int16 FloatToInt16(float value)
        {
            float f = value * Int16.MaxValue;
            if (f > Int16.MaxValue) f = Int16.MaxValue;
            if (f < Int16.MinValue) f = Int16.MinValue;
            return (Int16)f;
        }

        /// <summary>
        /// Get the Text To Speech output and create an audio frame for the audio graph
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        unsafe internal static AudioFrame GetAudioFrame(DataReader reader)
        {
            var numBytes = reader.UnconsumedBufferLength;

            // The Text to Speech output contains the RIFF header for PCM 16bit 16kHz mono output
            // We do not need this for the audio graph
            var headerSize = 44;
            var bytes = new byte[headerSize];
            reader.ReadBytes(bytes);

            // skip the header
            var numSamples = (uint)(numBytes - headerSize);
            AudioFrame frame = new AudioFrame(numSamples);

            using (var buffer = frame.LockBuffer(AudioBufferAccessMode.Write))
            using (IMemoryBufferReference reference = buffer.CreateReference())
            {
                byte* dataInBytes;
                uint capacityInBytes;

                // Get the buffer from the AudioFrame to write to
                ((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacityInBytes);

                Int16* dataInInt16 = (Int16*)dataInBytes;

                for (int i = 0; i < capacityInBytes / sizeof(Int16); i++)
                {
                    // write to the underlying stream 
                    dataInInt16[i] = reader.ReadInt16();
                }
            }

            // return the frame for the audiograph to process
            return frame;
        }
    }
}
