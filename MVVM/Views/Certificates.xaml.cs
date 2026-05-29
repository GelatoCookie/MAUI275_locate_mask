using AndroidX.Lifecycle;
using MauiRfidSample.MVVM.Models;
using MauiRfidSample.MVVM.ViewModels;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
namespace MauiRfidSample.MVVM.Views;

public partial class Certificates : ContentPage
{
    
    private readonly CertificatesViewModel _viewModel;

    public Certificates()
    {
        InitializeComponent();
        _viewModel = new CertificatesViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.UpdateCertificateEventsIn();
        await _viewModel.LoadCertificatesAsync(); // Fetch certificates when the page appears
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        var addCertificatePage = new AddCertificate();
        await Navigation.PushAsync(addCertificatePage);
    }

    private void OnRemoveAllClicked(object sender, EventArgs e)
    {
        _viewModel.RemoveAllCertificate();
    }

    private async void OnRemoveClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var certName = button?.CommandParameter as string;

        if (certName != null)
        {
             _viewModel.RemoveCertificate(certName); // Call API to remove individual certificate
        }
    }

    private void OnSaveClicked(object sender, EventArgs e)
    {
        _viewModel.SaveAllCertificate();
    }

  
}
