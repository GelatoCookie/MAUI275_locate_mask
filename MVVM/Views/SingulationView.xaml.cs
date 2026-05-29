using AndroidX.Lifecycle;
using MauiRfidSample.MVVM.ViewModels;
using Microsoft.Maui.Controls;
using System;

namespace MauiRfidSample.MVVM.Views
{
    public partial class SingulationView : ContentPage
    {
        private readonly SingulationViewModel _vm;
        public SingulationView()
        {
            InitializeComponent(); // generated implementation will load XAML
            Title = "Singulation";
            BindingContext = _vm = new SingulationViewModel();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _vm.UpdateIn();
            _vm.LoadCurrentSettings();
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _vm.UpdateOut();
        }

        private async void InfoItemClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Info","Configure singulation: Session (S0-S3), Tag Population, Inventory State (STATE_A/STATE_B/AB_FLIP), SL Flag (ASSERTED/DEASSERTED/ALL). Press Save to apply.","OK");
        }
    }
}