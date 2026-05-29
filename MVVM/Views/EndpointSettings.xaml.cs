using MauiRfidSample.MVVM.ViewModels;
using Com.Zebra.Rfid.Api3;
using AndroidX.Lifecycle;
namespace MauiRfidSample.MVVM.Views;

public partial class EndpointSettings : ContentPage
{

    private EndpointSettingsViewModel viewmodel;
    private string EndpointName;
    private ENUM_EP_OPERATION epOp;


    public EndpointSettings(string name, ENUM_EP_OPERATION op)
	{
		InitializeComponent();
        BindingContext = viewmodel = new EndpointSettingsViewModel();
        Title = "Endpoint Settings";
        EndpointName = name;
        epOp = op;
        Loaded += OnPageLoaded;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
     //   await viewmodel.LoadConfigurationAsync(EndpointName); // Ensure data is loaded
    }


    private async void OnPageLoaded(object sender, EventArgs e)
    {
        // Perform any necessary setup here
        System.Diagnostics.Debug.WriteLine("Call LoadConfigurationAsync endpointname "+ EndpointName);
        await viewmodel.LoadConfigurationAsync(EndpointName);
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var endPointName = button?.CommandParameter as string;
        //System.Diagnostics.Debug.WriteLine("Call SaveConfig");
       
        if (viewmodel.IsConfigChanged)
        {
            System.Diagnostics.Debug.WriteLine("Call SaveConfig");
            await viewmodel.SaveConfigurationAsync(epOp);
        }

        if (Navigation.NavigationStack.Count > 1)
        {
            await Navigation.PopAsync();
        }

        
    }
}