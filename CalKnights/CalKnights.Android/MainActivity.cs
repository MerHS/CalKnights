using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using TinyIoC;
using Tesseract;
using Tesseract.Droid;
using XLabs.Ioc;
using XLabs.Ioc.TinyIOC;
using XLabs.Platform.Device;

namespace CalKnights.Droid
{
    [Activity(Label = "CalKnights", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            global::Xamarin.Forms.Forms.Init(this, bundle);

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            var container = TinyIoCContainer.Current;
            container.Register<IDevice>(AndroidDevice.CurrentDevice);
            container.Register<ITesseractApi>((cont, parameters) =>
            {
                return new TesseractApi(ApplicationContext, Tesseract.Droid.AssetsDeployment.OncePerInitialization);
            });
            Resolver.SetResolver(new TinyResolver(container));
            
            LoadApplication(new App());
        }
    }
}

