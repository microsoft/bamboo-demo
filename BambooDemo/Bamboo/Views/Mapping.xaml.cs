using Bamboo.Models;
using Bamboo.Utilities;
using SDKTemplate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Bamboo.Views
{
    public sealed partial class Mapping : UserControl
    {
        private MediaCapture _mediaCapture;
        FrameRenderer frameRenderer = null;
        MediaFrameReader frameReader;

        public Mapping()
        {
            this.InitializeComponent();

            frameRenderer = new FrameRenderer(this.depthPreviewImage);
            frameRenderer.renderFramesInUI = true;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await BeginDepthFrameStreaming();
        }

        private async void OnUnloaded(object sender, RoutedEventArgs e)
        {
            await CleanupMediaCaptureAsync();
        }


        /// <summary>
        /// Switches to the next camera source and starts reading frames.
        /// </summary>
        private async Task BeginDepthFrameStreaming()
        {
            await CleanupMediaCaptureAsync();

            // Identify the color, depth, and infrared sources of each group,
            // and keep only the groups that have a depth source.
            var allGroups = await MediaFrameSourceGroup.FindAllAsync();
            var eligibleGroups = allGroups.Select(g => new
            {
                Group = g,

                // For the Move panel we only care about the Depth source feed
                SourceInfos = new MediaFrameSourceInfo[]
                {
                    g.SourceInfos.FirstOrDefault(info => info.SourceKind == MediaFrameSourceKind.Depth)
                }
            }).Where(g => g.SourceInfos.Any(info => info != null)).ToList();

            if (eligibleGroups.Count == 0)
            {
                Logger.GetInstance().LogLine("No source group with color info found.");
                return;
            }

            // It does not matter which one we use so we'll just use the first.
            var selected = eligibleGroups.First();

            Logger.GetInstance().LogLine($"Found {eligibleGroups.Count} groups and selecting the first: {selected.Group.DisplayName}");

            try
            {
                // Initialize MediaCapture with selected group.
                // This can raise an exception if the source no longer exists,
                // or if the source could not be initialized.
                await InitializeMediaCaptureAsync(selected.Group);
            }
            catch (Exception exception)
            {
                Logger.GetInstance().LogLine($"MediaCapture initialization error: {exception.Message}");
                await CleanupMediaCaptureAsync();
                return;
            }

            // Set up frame readers, register event handlers and start streaming.
            for (int i = 0; i < selected.SourceInfos.Length; i++)
            {
                MediaFrameSourceInfo info = selected.SourceInfos[i];
                if (info != null)
                {
                    // Access the initialized frame source by looking up the the ID of the source found above.
                    // Verify that the Id is present, because it may have left the group while were were
                    // busy deciding which group to use.
                    MediaFrameSource frameSource = null;
                    if (_mediaCapture.FrameSources.TryGetValue(info.Id, out frameSource))
                    {
                        frameReader = await _mediaCapture.CreateFrameReaderAsync(frameSource);
                        frameReader.FrameArrived += FrameReader_FrameArrived;

                        MediaFrameReaderStartStatus status = await frameReader.StartAsync();
                        if (status != MediaFrameReaderStartStatus.Success)
                        {
                            Logger.GetInstance().LogLine("Unable to start the MediaFrameReader frameReader.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the MediaCapture object with the given source group.
        /// </summary>
        /// <param name="sourceGroup">SourceGroup with which to initialize.</param>
        private async Task InitializeMediaCaptureAsync(MediaFrameSourceGroup sourceGroup)
        {
            if (_mediaCapture != null)
            {
                return;
            }

            // Initialize mediacapture with the source group.
            _mediaCapture = new MediaCapture();
            var settings = new MediaCaptureInitializationSettings
            {
                SourceGroup = sourceGroup,

                // This media capture can share streaming with other apps.
                SharingMode = MediaCaptureSharingMode.SharedReadOnly,

                // Only stream video and don't initialize audio capture devices.
                StreamingCaptureMode = StreamingCaptureMode.Video,

                // Set to CPU to ensure frames always contain CPU SoftwareBitmap images
                // instead of preferring GPU D3DSurface images.
                MemoryPreference = MediaCaptureMemoryPreference.Cpu
            };

            await _mediaCapture.InitializeAsync(settings);
        }

        /// <summary>
        /// Unregisters FrameArrived event handlers, stops and disposes frame readers
        /// and disposes the MediaCapture object.
        /// </summary>
        private async Task CleanupMediaCaptureAsync()
        {
            if (_mediaCapture != null)
            {
                if (_mediaCapture != null)
                {
                    _mediaCapture.Dispose();
                    _mediaCapture = null;
                }

                if (frameReader != null)
                {
                    frameReader.FrameArrived -= FrameReader_FrameArrived;
                    await frameReader.StopAsync();
                    frameReader.Dispose();
                }
            }
        }

        /// <summary>
        /// Handles a frame arrived event and renders the frame to the screen.
        /// </summary>
        private void FrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            using (var frame = sender.TryAcquireLatestFrame())
            {
                if (frame != null)
                {
                    if (frame.SourceKind == MediaFrameSourceKind.Depth)
                    {
                        frameRenderer.ProcessFrame(frame);
                    }
                }
            }
        }
    }
}
