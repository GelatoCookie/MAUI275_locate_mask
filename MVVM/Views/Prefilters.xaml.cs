namespace MauiRfidSample.MVVM.Views;

public partial class Prefilters : ContentPage
{
	public Prefilters()
	{
		InitializeComponent();
		BindingContext = new MVVM.ViewModels.PrefiltersViewModel();
	}
}