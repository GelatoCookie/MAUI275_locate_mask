using Android.Net;
using Android.Widget;
using AndroidX.Lifecycle;
using Com.Zebra.Rfid.Api3;
using ICSharpCode.SharpZipLib.Core;
using MauiRfidSample.MVVM.Models;
using MauiRfidSample.MVVM.Services;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Storage;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Input;


namespace MauiRfidSample.MVVM.ViewModels
{


    public partial class AddCertificateViewModel : BaseViewModel
    {
        //private readonly INavigationService _navigationService;
        public FileResult fileResult { get; set; }

        public AddCertificateViewModel()
        {
            if (rfidModel.rfidReader != null && rfidModel.rfidReader.IsConnected)
            {
                if (rfidModel.rfidReader.Transport.ToString().Equals(ENUM_TRANSPORT.QcSerial.ToString())
                        || rfidModel.rfidReader.Transport.ToString().Equals(ENUM_TRANSPORT.ReSerial.ToString())
                        || rfidModel.rfidReader.Transport.ToString().Equals(ENUM_TRANSPORT.ReUsb.ToString()))
                {
                    ShowAlert("Feature not supported for this device.");
                    Shell.Current.GoToAsync("..");
                    return;
                }
            }
            SelectFileCommand = new Command(async () => await SelectFileFromDevice());
            UploadCertificateCommand = new Command(async () => await UploadCertificateAsync());
        }

        private string _selectedType;
        public string SelectedType
        {
            get => _selectedType;
            set
            {
                if (_selectedType != value)
                {
                    _selectedType = value;
                    OnPropertyChanged(nameof(SelectedType));
                    OnPropertyChanged(nameof(IsOthersSelected));
                }
            }
        }

        public bool IsOthersSelected => SelectedType == "others";

        private string _othersCertName;
        public string OthersCertName
        {
            get => _othersCertName;
            set
            {
                if (_othersCertName != value)
                {
                    _othersCertName = value;
                    OnPropertyChanged(nameof(OthersCertName));
                    System.Diagnostics.Debug.WriteLine($"OthersCertName updated: {_othersCertName}");
                }
            }
        }

        private string _selectedCertType;
        public string SelectedCertType
        {
            get => _selectedCertType;
            set
            {
                if (_selectedCertType != value)
                {
                    _selectedCertType = value;
                    OnPropertyChanged(nameof(SelectedCertType));
                }
            }
        }

        private string _filePath;
        public string FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged(nameof(FilePath));
                }
            }
        }

        private string _selectedFileName;
        public string SelectedFileName
        {
            get => _selectedFileName;
            set
            {
                if (_selectedFileName != value)
                {
                    _selectedFileName = value;
                    OnPropertyChanged(nameof(SelectedFileName));
                }
            }
        }

        public ICommand SelectFileCommand { get; }
        public ICommand UploadCertificateCommand { get; }

        public event EventHandler? RequestPopPage;

        public async Task SelectFileFromDevice()
        {
            var result = await FilePicker.PickAsync();
            if (result != null)

            {
                fileResult = result;
                FilePath = result.FullPath;
                SelectedFileName = Path.GetFileNameWithoutExtension(result.FileName);
                System.Diagnostics.Debug.WriteLine($"Selected file: {SelectedFileName}");
            }
        }

        public async Task UploadCertificateAsync()
        {
            if (string.IsNullOrEmpty(SelectedFileName) || string.IsNullOrEmpty(FilePath) || string.IsNullOrEmpty(SelectedType))
            {
                ShowAlert("Please provide certificate name, type, and file");
                return;
            }
            else
            {
                string certificateName;
                if (IsOthersSelected)
                {
                    certificateName = string.Format("{0}_{1}",
                        OthersCertName,
                        SelectedCertType);
                }
                else
                {
                    certificateName = string.Format("{0}_{1}",
                        SelectedType,
                        SelectedCertType
                       );
                }

                var newCert = new CertificateModel
                {
                    FilePath = FilePath,
                    CertificateType = SelectedType,
                    CertType = SelectedCertType,
                    Name = certificateName,
                    FileResult = fileResult,

                };

                System.Diagnostics.Debug.WriteLine("Certificates name: " + newCert.Name);
                await AddCertificateAsync(newCert);
            }

        }

        public async Task AddCertificateAsync(CertificateModel certificate)
        {
            try
            {
                string copiedFilePath = await CopyCertFromUri(certificate);
                System.Diagnostics.Debug.WriteLine("AddCertificates " + copiedFilePath);

                if (rfidModel.rfidReader != null && rfidModel.rfidReader.IsConnected)
                {
                    var certList = rfidModel.rfidReader.Config.AddCertificate(copiedFilePath); // API call
                    if (certList == RFIDResults.RfidApiSuccess)
                    {
                        RequestPopPage?.Invoke(this, EventArgs.Empty);
                    }

                }
            }

            catch (InvalidUsageException iuex)
            {
                System.Diagnostics.Debug.WriteLine($"InvalidUsageException in AddCertificateAsync: {iuex.Message}");
            }
            catch (OperationFailureException ofex)
            {
                System.Diagnostics.Debug.WriteLine($"OperationFailureException in AddCertificateAsync: {ofex.VendorMessage}, Result: {ofex.Results}");
                if (ofex.Results == RFIDResults.AdminConnectError)
                {
                    await AuthenticaionDialog.ShowConnectionLostDialog();
                    await Shell.Current.GoToAsync("..");
                }
            }

            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AddCertificates: {ex.Message} ");
                ShowAlert("Error :" + ex.Message);
            }
        }


        public async Task<string> CopyCertFromUri(CertificateModel certificateModel)
        {
            if (certificateModel.FilePath == null)
                return null;

            // Define the destination directory
            string extractPath = FileSystem.Current.CacheDirectory;
            string datFile = null;

            try
            {
                // Extract file extension and build the file name
                string ext = Path.GetExtension(certificateModel.Name);
                string fileName;

                datFile = Path.Combine(extractPath, certificateModel.Name);

                using (var inStream = await certificateModel.FileResult.OpenReadAsync())
                using (var outStream = File.Create(datFile))
                {
                    await inStream.CopyToAsync(outStream);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying file: {ex.Message}");
                return null;
            }

            return datFile;
        }

        public override void CertificateEvent(IEvents.CertificateEventData statusEvent)
        {
            ShowAlert("Certificate Event: " + statusEvent.Status);
        }

        public void ShowAlert(string v)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Toast.MakeText(Android.App.Application.Context, v, ToastLength.Short).Show();
            });
            Console.WriteLine(v);
        }
    }

}
