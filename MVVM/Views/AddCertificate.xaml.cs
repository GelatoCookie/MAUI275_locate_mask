using MauiRfidSample.MVVM.Models;
using MauiRfidSample.MVVM.ViewModels;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Microsoft.Maui;
using static System.Net.Mime.MediaTypeNames;
using Android.Content.PM;
using Java.Nio.FileNio.Attributes;
using System.Runtime.ConstrainedExecution;
using Java.Nio.Channels;
using System.Windows.Input;
using Android.Widget;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;


namespace MauiRfidSample.MVVM.Views
{

    public partial class AddCertificate : ContentPage
    {
        AddCertificateViewModel viewModel;
        private EventHandler? _popHandler;

        public AddCertificate()
        {
            InitializeComponent();
            BindingContext = viewModel = new AddCertificateViewModel();

            // Initialize pickers if necessary
            //TypePicker.ItemsSource = new List<string> { "WiFi", "MQTT", "Others", "filestore" };
            //CertTypePicker.ItemsSource = new List<string> { "ca_cert", "client_cert", "client_key" };

            // Subscribe to the message
            viewModel.UpdateCertificateEventsIn();

            _popHandler = async (s, e) => await Navigation.PopAsync();
            viewModel.RequestPopPage += _popHandler;
        }


        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            viewModel.UpdateCertificateEventsOut();
            if (_popHandler != null)
                viewModel.RequestPopPage -= _popHandler;
        }
    }

    //{
    //    private readonly AddCertificateViewModel _viewModel;
    //    public string SelectedFileName { get;  set; }
    //    public FileResult fileResult { get; set; }


    //    public AddCertificate( )
    //    {
    //        InitializeComponent();
    //        //TypePicker.ItemsSource = new List<string> { "WiFi", "MQTT", "Others", "filestore" };
    //        CertTypePicker.ItemsSource = new List<string> { "ca_cert", "client_cert", "client_key" };
    //        _viewModel = new AddCertificateViewModel();
    //    }

    //    private async void OnSelectCertificateFileClicked(object sender, EventArgs e)
    //    {
    //        var result = await FilePicker.PickAsync();
    //        if (result != null)
    //        {
    //            fileResult = result;
    //            FilePathLabel.Text = result.FullPath;
    //            SelectedFileName = Path.GetExtension(result.FileName)?.TrimStart('.');
    //            CertNameEntry.Text = SelectedFileName;
    //        }
    //    }

    //    private void OnUploadCertificateClicked(object sender, EventArgs e)

    //    {
    //        System.Diagnostics.Debug.WriteLine("Before Upload, OthersCertName: " + _viewModel.OthersCertName);
    //        if (string.IsNullOrEmpty(CertNameEntry.Text) || string.IsNullOrEmpty(FilePathLabel.Text) || string.IsNullOrEmpty(CertTypePicker.SelectedItem.ToString()) || string.IsNullOrEmpty(TypePicker.SelectedItem.ToString()))

    //        {
    //            DisplayAlert("Error", "Please provide certificate name,type and file", "OK");
    //            return;
    //        }

    //        string certificateName;
    //        if (TypePicker.SelectedItem.ToString() == "Others")
    //        {
    //            System.Diagnostics.Debug.WriteLine("OthersCert name "+ _viewModel.OthersCertName);
    //            // Include OthersCertName if "Others" is selected
    //          certificateName = string.Format("{0}_{1}_{2}.{3}",
    //                _viewModel.OthersCertName,
    //                TypePicker.SelectedItem,
    //                CertTypePicker.SelectedItem,                  
    //                SelectedFileName);
    //        }
    //        else
    //        {
    //            // Default naming convention
    //            certificateName = string.Format("{0}_{1}.{2}",
    //                TypePicker.SelectedItem,
    //                CertTypePicker.SelectedItem,
    //                SelectedFileName);
    //        }
    //        var newCert = new CertificateModel
    //        {

    //           // Name = SelectedFileName,
    //            FilePath = FilePathLabel.Text,
    //            CertificateType = TypePicker.SelectedItem.ToString(),
    //            CertType = CertTypePicker.SelectedItem.ToString(),
    //            Name = certificateName,
    //            FileResult = fileResult,

    //        }; 
    //        System.Diagnostics.Debug.WriteLine("Certificates name : " + newCert.Name);
    //        _viewModel.AddCertificateAsync(newCert);
    //        Navigation.PopAsync();
    //    }
    //}

}
