using AndroidX.Lifecycle;
using MauiRfidSample.MVVM.ViewModels;

namespace MauiRfidSample.MVVM.Views
{
    public partial class EndpointStatus : ContentPage
    {
        private EndpointStatusViewModel viewModel;
        
        public EndpointStatus()
        {
            InitializeComponent();
            BindingContext = viewModel = new EndpointStatusViewModel();
        }
        
        protected override async void OnAppearing()
        {
            base.OnAppearing();
           // viewModel.UpdateIn();
            viewModel.UpdateEndpointStatusEventsIn();
            await viewModel.LoadStatusAsync();
        }
        
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
           // viewModel.UpdateOut();
            viewModel.UpdateEndpointStatusEventsOut();
        }
    }
}