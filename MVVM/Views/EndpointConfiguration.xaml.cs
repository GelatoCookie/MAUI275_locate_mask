using AndroidX.Lifecycle;
using MauiRfidSample.MVVM.ViewModels;

namespace MauiRfidSample.MVVM.Views;

public partial class EndpointConfiguration : ContentPage

{
    private  EndpointConfigViewModel _viewModel;
    public EndpointConfiguration()
	{
		InitializeComponent();
        _viewModel = new EndpointConfigViewModel();
        BindingContext = _viewModel;
    }

    

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadEndpointConfigAsync(); // Fetch certificates when the page appears
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        var addEndpointPage = new EndpointSettings(string.Empty,Com.Zebra.Rfid.Api3.ENUM_EP_OPERATION.New);
        await Navigation.PushAsync(addEndpointPage);
    }

   

    private async void OnRemoveClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var endpointItem = button?.CommandParameter as EndpointItem;
        if (endpointItem != null)
        {
            var endPointName = endpointItem.EPName;          
            await _viewModel.RemoveConfigurationAsync(endPointName);
        }
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var endpointItem = button?.CommandParameter as EndpointItem;
        if (endpointItem != null)
        {
            var endPointName = endpointItem.EPName; // Access the Name property
            var addEndpointPage = new EndpointSettings(endPointName, Com.Zebra.Rfid.Api3.ENUM_EP_OPERATION.Update);
            await Navigation.PushAsync(addEndpointPage);
        }
       
    }
    private void OnCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_viewModel.IsLoadingEndpoints)
            return; 

        if (sender is CheckBox checkBox && checkBox.BindingContext is EndpointItem item)
        {
            _viewModel.OnItemCheckedChanged(item);
            _viewModel.SetIot(item);
        }
    }
}