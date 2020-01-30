using System;
using Xamarin.Forms;

namespace CalKnights
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            // SizeChanged += (sender, args) => portrait.IsVisible = !(landscape.IsVisible = Width > Height);
        }
    }
}
