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

namespace CalKnights
{
    public class CalKnightsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ITesseractApi _tesseractApi;
        private readonly IDevice _device;

        private bool _imageLoading = false;
        private ImageSource _ocrImageSource;
        private Dictionary<string, string> _dicts;

        public CalKnightsViewModel()
        { 
            _tesseractApi = Resolver.Resolve<ITesseractApi>();
            _device = Resolver.Resolve<IDevice>();
            _dicts = new Dictionary<string, string>
            {
                { "vanguard", "test" }
            };
            //DigitCommand = new Command<string>(
            //    execute: async (string arg) =>
            //    {
            //        _takePictureButton.Text = "Working...";
            //        _takePictureButton.IsEnabled = false;

            //        //TesseractAPI 초기화 (언어 선정)
            //        if (!_tesseractApi.Initialized)
            //            await _tesseractApi.Init("eng");

            //        //카메라 촬영 후 byte[] 형식 변수로 받아오기
            //        var photo = await TakePic();
            //        if (photo != null)
            //        {
            //            var imageBytes = new byte[photo.Source.Length];
            //            photo.Source.Position = 0;
            //            photo.Source.Read(imageBytes, 0, (int)photo.Source.Length);
            //            photo.Source.Position = 0;

            //            //문자 인식 수행
            //            var tessResult = await _tesseractApi.SetImage(imageBytes);

            //            //촬영이미지, 인식 결과 출력
            //            if (tessResult)
            //            {
            //                _takenImage.Source = ImageSource.FromStream(() => photo.Source);
            //                _recognizedTextLabel.Text = _tesseractApi.Text;
            //            }
            //        }

            //        //버튼 상태 복구
            //        _takePictureButton.Text = "New scan";
            //        _takePictureButton.IsEnabled = true;
            //    },
            //    canExecute: (string arg) =>
            //    {
            //        return !(arg == "." && Entry.Contains("."));
            //    });
            PickImageCommand = new Command(
                execute: async () =>
                {
                    ImageLoading = true;

                    var imageFile = await TakePic();
                    if (imageFile != null)
                    {
                        OCRImageSource = ImageSource.FromStream(() => { return imageFile.GetStream(); });
                    }

                    ImageLoading = false;
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

        public bool ImageLoading {
            private set { SetProperty(ref _imageLoading, value); }
            get { return _imageLoading; }
        }

        public ImageSource OCRImageSource {
            private set { SetProperty(ref _ocrImageSource, value); }
            get { return _ocrImageSource; }
        }

        public Dictionary<string, string> Dicts {
            get { return _dicts; }
        }

        public ICommand PickImageCommand { private set; get; }

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
