using Bamboo.Movement;
using Bamboo.Utilities;
using SDKTemplate;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Bamboo.Views
{
    public sealed partial class Move : UserControl
    {
        private MediaCapture _mediaCapture;

        FrameRenderer frameRenderer;
        IMovementManager movementManager;

        public bool FrameRendererStatus
        {
            set { frameRenderer.renderFramesInUI = value; }
            get { return frameRenderer.renderFramesInUI; }
        }

        public Move()
        {
            this.InitializeComponent();
            frameRenderer = new FrameRenderer(this.colorPreviewImage);
            this.FrameRendererStatus = true;
        }

        public void SetMovementManager(IMovementManager movementManager)
        {
            this.movementManager = movementManager;
        }

        /// <summary>
        /// Switches to the next camera source and starts reading frames.
        /// </summary>
        private async Task BeginColorFrameStreaming()
        {
            CleanupMediaCaptureAsync();

            // Identify the color, depth, and infrared sources of each group,
            // and keep only the groups that have a color source.
            var allGroups = await MediaFrameSourceGroup.FindAllAsync();
            var eligibleGroups = allGroups.Select(g => new
            {
                Group = g,

                // For the Move panel we only care about the Color source feed
                SourceInfos = new MediaFrameSourceInfo[]
                {
                    g.SourceInfos.FirstOrDefault(info => info.SourceKind == MediaFrameSourceKind.Color)
                }
            }).Where(g => g.SourceInfos.Any(info => info != null)).ToList();

            if (eligibleGroups.Count == 0)
            {
                Logger.GetInstance().LogLine("No source group with color info found.");
                return;
            }

            // The SR300 camera used in the demo will return 3 groups that contain a color
            // source. It does not matter which one we use so we'll just use the first.
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
                CleanupMediaCaptureAsync();
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
                        MediaFrameReader frameReader = await _mediaCapture.CreateFrameReaderAsync(frameSource);
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
        private void CleanupMediaCaptureAsync()
        {
            if (_mediaCapture != null)
            {
                _mediaCapture.Dispose();
                _mediaCapture = null;
            }
        }

        /// <summary>
        /// Handles a frame arrived event and renders the frame to the screen.
        /// </summary>
        private void FrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            // TryAcquireLatestFrame will return the latest frame that has not yet been acquired.
            // This can return null if there is no such frame, or if the reader is not in the
            // "Started" state. The latter can occur if a FrameArrived event was in flight
            // when the reader was stopped.
            using (var frame = sender.TryAcquireLatestFrame())
            {
                if (frame != null)
                {
                    if (frame.SourceKind == MediaFrameSourceKind.Color)
                    {
                        frameRenderer.ProcessFrame(frame);
                    }
                }
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await BeginColorFrameStreaming();
        }

        private async void Button_Click_Up(object sender, RoutedEventArgs e)
        {
            await movementManager.MoveForward();
        }

        private async void Button_Click_Down(object sender, RoutedEventArgs e)
        {
            await movementManager.MoveBackward();
        }

        private async void Button_Click_Left(object sender, RoutedEventArgs e)
        {
            await movementManager.TurnLeft();
        }

        private async void Button_Click_Right(object sender, RoutedEventArgs e)
        {
            await movementManager.TurnRight();
        }

        private async void Button_Click_Stop(object sender, RoutedEventArgs e)
        {
            await movementManager.Stop();
        }
    }
}
