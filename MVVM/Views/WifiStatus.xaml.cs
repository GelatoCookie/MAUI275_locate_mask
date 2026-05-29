using AndroidX.Lifecycle;
using MauiRfidSample.MVVM.ViewModels;

namespace MauiRfidSample.MVVM.Views;

public partial class WifiStatus : ContentPage
{
	private WifiStatusViewModel _wifiStatusViewModel;
	public WifiStatus()
	{
		InitializeComponent();
		_wifiStatusViewModel = new WifiStatusViewModel();
		BindingContext = _wifiStatusViewModel;
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();
		await _wifiStatusViewModel.LoadWifiStatus(); // Fetch certificates when the page appears
    }
}