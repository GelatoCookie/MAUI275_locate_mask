
using MauiRfidSample.MVVM.Models;
using MauiRfidSample.MVVM.ViewModels;
using System;
using System.Xml.Serialization;


namespace MauiRfidSample.MVVM.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Gen2XTagProtection : ContentPage
    {
        Gen2XTagProtectionViewModel viewmodel;
        public Gen2XTagProtection()
        {
            InitializeComponent();
            Title = "Tag Protection";
            BindingContext = viewmodel = new Gen2XTagProtectionViewModel();
        }

        private void PerformOperation(object sender, EventArgs e)
        {
            viewmodel.PerformOpClicked();
        }

        private void DeleteAll(object sender, EventArgs e)
        {
            viewmodel.DeleteAllClicked();
        }

        private void InfoItemClicked(object sender, EventArgs e)
        {
            viewmodel.displayDialog();
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            viewmodel.UpdateIn();
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            viewmodel.UpdateOut();
        }

    }
}