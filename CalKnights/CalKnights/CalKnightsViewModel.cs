using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;
using Tesseract;
using XLabs.Ioc;
using XLabs.Platform.Device;
using System.Threading.Tasks;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System.Diagnostics;
using System.IO;
using SkiaSharp;

namespace CalKnights
{
    public class CalKnightsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ITesseractApi _tesseractApi;
        private readonly IDevice _device;

        private bool _imageWaiting = true;
        private ImageSource _ocrImageSource;
        private Dictionary<string, string> _dicts;
        private List<Boolean> _isFrameVisible = new List<Boolean> { true, true, true };

        private SKColorFilter _greyFilter = SKColorFilter.CreateColorMatrix(new float[]
                {
                    0.21f, 0.72f, 0.07f, 0, 0,
                    0.21f, 0.72f, 0.07f, 0, 0,
                    0.21f, 0.72f, 0.07f, 0, 0,
                    0,     0,     0,     1, 0
                });

        public CalKnightsViewModel()
        {
            _tesseractApi = Resolver.Resolve<ITesseractApi>();
            _device = Resolver.Resolve<IDevice>();
            _dicts = new Dictionary<string, string>
            {
                { "vanguard", "test" }
            };

            PickImageCommand = new Command(
                execute: async () =>
                {
                    ImageWaiting = false;

                    if (!_tesseractApi.Initialized)
                    {
                        await _tesseractApi.Init("kor", OcrEngineMode.TesseractOnly);
                        _tesseractApi.SetPageSegmentationMode(PageSegmentationMode.SparseText);
                    }

                    var imageFile = await TakePic();
                    if (imageFile != null)
                    {
                       

                        //var imgStream = imageFile.GetStream();
                        //int imgLen = (int)imgStream.Length;
                        //var bytes = new byte[imgLen];

                        //imgStream.Position = 0;
                        //imgStream.Read(bytes, 0, imgLen);
                        //for (int i = 0; i < imgLen; i++)
                        //    bytes[i] = (byte)(0xFF ^ bytes[i]);

                        //var memoryStream = new MemoryStream();
                        //await mediaFile.GetStream().CopyToAsync(memoryStream);
                        //byte[] imageAsByte = memoryStream.ToArray();
                        byte[] bytes;

                        using (Stream stream = imageFile.GetStream())
                        {
                            //await stream.CopyToAsync(memStream);
                            //memStream.Seek(0, SeekOrigin.Begin);
                            var bitmap = SKBitmap.Decode(stream);

                            var img = SKImage.FromBitmap(bitmap);
                            int height = img.Height;
                            int width = img.Width;

                            var subset = SKRectI.Create(
                                (int)(width * 0.2f), (int)(height * 0.4f),
                                (int)(width * 0.5f), (int)(height * 0.3f));
                            var placeholder = SKRectI.Empty;
                            var placeholder2 = SKPointI.Empty;

                            var img2 = img.ApplyImageFilter(
                                SKImageFilter.CreateColorFilter(_greyFilter),
                                subset, subset, out placeholder, out placeholder2);
                            if (img2 == null)
                            {
                                Debug.WriteLine("ISNULL");
                            }
                            img = img.Subset(subset);

                            bytes = img.Encode().ToArray();
                            OCRImageSource = ImageSource.FromStream(() =>
                            {
                                return new MemoryStream(bytes);
                            });
                        }
                        
                        // ImageSource.FromStream(() => { return imageFile.GetStream(); });
                        var tessResult = await _tesseractApi.SetImage(bytes);

                        if (tessResult)
                        {
                            Debug.WriteLine("read");
                            Debug.WriteLine(_tesseractApi.Text);
                        } else
                        {
                            Debug.WriteLine("unavaliable");
                        }
                    }
                    ImageWaiting = true;
                });

            FoldSelectorCommand = new Command<string>(
                execute: (string selector) =>
                {
                    int i = 0;
                    switch (selector)
                    {
                        case "filter": i = 0; break;
                        case "tag": i = 1; break;
                        case "sort": i = 2; break;
                    }
                    _isFrameVisible[i] = !_isFrameVisible[i];
                    OnPropertyChanged("IsFrameVisible");
                });
        }
        private async Task<MediaFile> TakePic()
        {
            await CrossMedia.Current.Initialize();

            var file = await CrossMedia.Current.PickPhotoAsync();

            return file;
        }

        void RefreshCanExecutes()
        {
            //((Command)BackspaceCommand).ChangeCanExecute();
            //((Command)DigitCommand).ChangeCanExecute();
        }

        void RefreshStackDisplay()
        {
            OnPropertyChanged("XStackValue");
            OnPropertyChanged("YStackValue");
        }

        public bool ImageWaiting {
            private set { SetProperty(ref _imageWaiting, value); }
            get { return _imageWaiting; }
        }

        public ImageSource OCRImageSource {
            private set { SetProperty(ref _ocrImageSource, value); }
            get { return _ocrImageSource; }
        }

        public Dictionary<string, string> Dicts {
            get { return _dicts; }
        }

        public List<Boolean> IsFrameVisible {
            get { return _isFrameVisible;  }
        }

        public ICommand PickImageCommand { private set; get; }
        public ICommand FoldSelectorCommand { private set; get; }

        bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Object.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
