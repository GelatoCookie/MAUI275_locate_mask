using Com.Zebra.Rfid.Api3;
using MauiRfidSample.MVVM.Models;
using MauiRfidSample.MVVM.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;



namespace MauiRfidSample.MVVM.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Gen2XView : ContentPage
	{
        private Gen2XViewModel viewmodel;
        public Gen2XView()
        {
            InitializeComponent();
            BindingContext = viewmodel = new Gen2XViewModel();
            Title = "Gen2X Features";

            // Subscribe to ViewModel event
            viewmodel.RequestUiSelectionClear += OnRequestUiSelectionClear;
        }
        private void InfoItemClicked(object sender, EventArgs e)
        {
            viewmodel.displayDialog();
        }

        private void OnTagSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Directly call ViewModel handler method (create a public method)
            viewmodel.HandleSelectionChanged(e);    
        }

        private void OnRequestUiSelectionClear()
        {
            // Ensure we are on UI thread
            Dispatcher.Dispatch(() =>
            {
                if (TagCollectionView?.SelectedItems?.Count > 0)
                {
                    TagCollectionView.SelectedItems.Clear();
                    Debug.WriteLine("[Gen2X][UI] CollectionView.SelectedItems cleared");
                }
            });
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

            viewmodel.RequestUiSelectionClear -= OnRequestUiSelectionClear;
        }

    }
}