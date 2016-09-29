using System;
using FreeTools.ViewModels;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Lumia.Imaging;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Media.Devices;
using ZXing;
using Windows.UI.Xaml;
using System.Linq;
using Windows.UI.Xaml.Input;
using Windows.UI.Core;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using ZXing.Common;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;

namespace FreeTools.Views
{
    public sealed partial class ScannerPage : Page
    {
        private CameraPreviewImageSource _cameraPreviewImageSource;
        private VideoDeviceController _videoDevice;
        private WriteableBitmap _writeableBitmap;
        private WriteableBitmapRenderer _writeableBitmapRenderer;
        private readonly BarcodeReader _reader;

        private DispatcherTimer _timer;

        private bool _initialized;
        private bool _isRendering;
        private bool _decoding;
        private bool _focusing;

        private double _width;
        private double _height;

        private ScannerViewModel ViewModel
        {
            get { return SimpleIoc.Default.GetInstance<ScannerViewModel>(); }
        }

        public ScannerPage()
        {
            this.InitializeComponent();

            this.Tapped += OnTapped;

            _reader = new BarcodeReader
            {
                AutoRotate = true,
                Options = new DecodingOptions
                {
                    TryHarder = true
                }
            };
        }

        private async void Initialize()
        {
            var cameraId = await ViewModel.GetCameraIdAsync(Windows.Devices.Enumeration.Panel.Back);
            
            _cameraPreviewImageSource = new CameraPreviewImageSource();
            await _cameraPreviewImageSource.InitializeAsync(cameraId.Id);
            var properties = await _cameraPreviewImageSource.StartPreviewAsync();
            
            _width = Window.Current.Bounds.Width;
            _height = Window.Current.Bounds.Height;
            var bitmap = new WriteableBitmap((int)_width, (int)_height);

            _writeableBitmap = bitmap;

            PreviewImage.Source = _writeableBitmap;

            _writeableBitmapRenderer = new WriteableBitmapRenderer(_cameraPreviewImageSource, _writeableBitmap);

            _cameraPreviewImageSource.PreviewFrameAvailable += OnPreviewFrameAvailable;

            _videoDevice = (VideoDeviceController)_cameraPreviewImageSource.VideoDeviceController;
            
            if (_videoDevice.FocusControl.Supported)
            {
                var focusSettings = new FocusSettings
                {
                    AutoFocusRange = AutoFocusRange.Macro,
                    Mode = FocusMode.Auto,
                    WaitForFocus = false,
                    DisableDriverFallback = false
                };

                _videoDevice.FocusControl.Configure(focusSettings);

                _timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(2)
                };
                _timer.Tick += OnTick;
                _timer.Start();
            }

            await _videoDevice.ExposureControl.SetAutoAsync(true);

            _initialized = true;

            ViewModel.ScannedText = "http://microsoft.com";
        }

        private async void OnPreviewFrameAvailable(IImageSize args)
        {
            if (!_initialized || _isRendering)
                return;

            _isRendering = true;

            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                _writeableBitmap.Invalidate();
            });

            await _writeableBitmapRenderer.RenderAsync();

            Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                Decode(_writeableBitmap.PixelBuffer.ToArray(), BitmapFormat.Unknown);
            });

            _isRendering = false;
        }

        private async void Decode(byte[] rawRgb, BitmapFormat bitmapFormat)
        {
            await Task.Run(() =>
            {
                if (_decoding || !string.IsNullOrWhiteSpace(ViewModel.ScannedText)) return;

                _decoding = true;

                var decoded = _reader.Decode(rawRgb, (int)_width, (int)_height, bitmapFormat);

                if (decoded != null)
                    ViewModel.ScannedText = decoded.Text;

                _decoding = false;
            });
        }

        private async void Focus()
        {
            if (_focusing || !_initialized || _videoDevice == null || !_videoDevice.FocusControl.Supported)
                return;

            _focusing = true;
            await _videoDevice.FocusControl.LockAsync();
            await _videoDevice.FocusControl.FocusAsync();
            await _videoDevice.FocusControl.UnlockAsync();
            _focusing = false;
        }

        private async void Dispose()
        {
            if (_cameraPreviewImageSource != null)
            {
                await _cameraPreviewImageSource.StopPreviewAsync();
            }

            if (_timer != null)
            {
                _timer.Stop();
            }
        }

        private void OnTick(object sender, object e) => Focus();

        private void OnTapped(object sender, TappedRoutedEventArgs e) => Focus();

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            await StatusBar.GetForCurrentView().ShowAsync();

            Messenger.Default.Unregister(this);

            Dispose();
        }

        private void ToggleResultGrid(bool showResult)
        {
            if (showResult)
                ShowResultStoryboard.Begin();
            else
                HideResultStoryboard.Begin();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            await StatusBar.GetForCurrentView().HideAsync();

            Messenger.Default.Register<bool>(this, ToggleResultGrid);

            Initialize();
        }
    }
}
