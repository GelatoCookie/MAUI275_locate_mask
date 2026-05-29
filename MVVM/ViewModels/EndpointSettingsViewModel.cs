using Android.Net;
using Android.Widget;
using AndroidX.Lifecycle;
using Com.Zebra.Rfid.Api3;
using ICSharpCode.SharpZipLib.Core;
using Java.Net;
using MauiRfidSample.MVVM.Models;
using MauiRfidSample.MVVM.Services;
using MauiRfidSample.MVVM.Views;
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
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Input;
using static AndroidX.Navigation.NavType;


namespace MauiRfidSample.MVVM.ViewModels
{
    public class EndpointSettingsViewModel : BaseViewModel
    {
        private EndpointConfigurationInfo _endpointConfigurationInfo = new EndpointConfigurationInfo();
        private bool _isConfigChanged = false;

        public bool IsConfigChanged => _isConfigChanged;

        public ObservableCollection<ENUM_EP_PROTOCOL_TYPE> ProtocolTypes { get; } = new()
        {
            ENUM_EP_PROTOCOL_TYPE.EpProtoTypMqtt,
           
            ENUM_EP_PROTOCOL_TYPE.EpProtoTypMqttTls,
            
        };

        public ObservableCollection<ENUM_EP_TYPE> Types { get; } = new()
        {
            ENUM_EP_TYPE.Soti,
            ENUM_EP_TYPE.Mdm
        };

        public ObservableCollection<ENUM_HOST_VERIFY> HostVerifyTypes { get; } = new()
        {
            ENUM_HOST_VERIFY.VerifyNone,
            ENUM_HOST_VERIFY.VerifyPeer,
            ENUM_HOST_VERIFY.VerifyHost,
            ENUM_HOST_VERIFY.VerifyHostAndPeer
        };

        public string Name
        {
            get => _endpointConfigurationInfo?.Epname ?? string.Empty;
            set
            {
                if (_endpointConfigurationInfo != null && _endpointConfigurationInfo.Epname != value)
                {
                    _endpointConfigurationInfo.Epname = value;
                    _isConfigChanged = true;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string Url
        {
            get => _endpointConfigurationInfo?.Url ?? string.Empty;
            set
            {
                if (_endpointConfigurationInfo != null && _endpointConfigurationInfo.Url != value)
                {
                    _endpointConfigurationInfo.Url = value;
                    _isConfigChanged = true;
                    OnPropertyChanged(nameof(Url));
                }
            }
        }

        public ENUM_EP_TYPE SelectedType
        {
            get
            {
               
                return _endpointConfigurationInfo?.Type ?? ENUM_EP_TYPE.Soti;
            }
            set
            {
                if (_endpointConfigurationInfo != null && _endpointConfigurationInfo.Type != value)
                {
                    _endpointConfigurationInfo.Type = value;
                    _isConfigChanged = true;
                    OnPropertyChanged(nameof(SelectedType));
                    if (_endpointConfigurationInfo?.Type == ENUM_EP_TYPE.Mdm)
                    {
                        LoadCertificatesAsync();
                    }
                }
            }
        }

        public ENUM_EP_PROTOCOL_TYPE ProtocolSelectedType
        {
            get => _endpointConfigurationInfo?.Protocol ?? ENUM_EP_PROTOCOL_TYPE.EpProtoTypMqtt;
            set
            {
                if (_endpointConfigurationInfo != null && _endpointConfigurationInfo.Protocol != value)
                {
                    _endpointConfigurationInfo.Protocol = value;
                    _isConfigChanged = true;
                    OnPropertyChanged(nameof(ProtocolSelectedType));
                    
                }
            }
        }

        public int? Port
        {
            get => _endpointConfigurationInfo?.Port?.IntValue();
            set
            {
                if (_endpointConfigurationInfo != null && _endpointConfigurationInfo.Port?.IntValue() != value)
                {
                    _endpointConfigurationInfo.Port = value.HasValue ? new Java.Lang.Integer(value.Value) : null;
                    _isConfigChanged = true;
                    OnPropertyChanged(nameof(Port));
                }
            }
        }

        public int? KeepAlive
        {
            get => _endpointConfigurationInfo?.Keepalive?.IntValue();
            set
            {
                if (_endpointConfigurationInfo != null && _endpointConfigurationInfo.Keepalive?.IntValue() != value)
                {
                    _endpointConfigurationInfo.Keepalive = value.HasValue ? new Java.Lang.Integer(value.Value) : null;
                    _isConfigChanged = true;
                    OnPropertyChanged(nameof(KeepAlive));
                }
            }
        }

        public bool CleanSession
        {
            get => _endpointConfigurationInfo?.Encleanss ?? false;
            //set
            //{
            //    if (_endpointConfigurationInfo != null && _endpointConfigurationInfo.Encleanss != value)
            //    {
            //        _endpointConfigurationInfo.Encleanss = value;
            //        _isConfigChanged = true;
            //        OnPropertyChanged(nameof(CleanSession));
            //    }

            //}
            set
            {
                if (_endpointConfigurationInfo != null)
                {
                    if (value)
                    {
                        if (!_endpointConfigurationInfo.Encleanss)
                        {
                            _endpointConfigurationInfo.Encleanss = true;
                            _endpointConfigurationInfo.Dscleanss = false;
                            OnPropertyChanged(nameof(CleanSession));
                        }
                    }
                    else { 

                        if (!_endpointConfigurationInfo.Dscleanss)
                        {
                            _endpointConfigurationInfo.Encleanss = false;
                            _endpointConfigurationInfo.Dscleanss = true;
                            OnPropertyChanged(nameof(CleanSession));
                        }
                    }
                }
            }
        }

        public string TenantId
        {
            get => _endpointConfigurationInfo?.Tenantid ?? string.Empty;
            set
            {
                if (_endpointConfigurationInfo != null && _endpointConfigurationInfo.Tenantid != value)
                {
                    _endpointConfigurationInfo.Tenantid = value;
                    _isConfigChanged = true;
                    OnPropertyChanged(nameof(TenantId));
                }
            }
        }

        public int? MinReconnectDelay
        {
            get => _endpointConfigurationInfo?.Rcdelaymin?.IntValue();
            set
            {
                if (_endpointConfigurationInfo != null && _endpointConfigurationInfo.Rcdelaymin?.IntValue() != value)
                {
                    _endpointConfigurationInfo.Rcdelaymin = value.HasValue ? new Java.Lang.Integer(value.Value) : null;
                    _isConfigChanged = true;
                    OnPropertyChanged(nameof(MinReconnectDelay));
                }
            }
        }

        public int? MaxReconnectDelay
        {
            get => _endpointConfigurationInfo?.Rcdelaymax?.IntValue();
            set
            {
                if (_endpointConfigurationInfo != null && _endpointConfigurationInfo.Rcdelaymax?.IntValue() != value)
                {
                    _endpointConfigurationInfo.Rcdelaymax = value.HasValue ? new Java.Lang.Integer(value.Value) : null;
                    _isConfigChanged = true;
                    OnPropertyChanged(nameof(MaxReconnectDelay));
                }
            }
        }

        public ENUM_HOST_VERIFY HostVerifySelectedType
        {
            get => _endpointConfigurationInfo?.Hostvfy ?? ENUM_HOST_VERIFY.VerifyNone;
            set
            {
                if (_endpointConfigurationInfo != null && _endpointConfigurationInfo.Hostvfy != value)
                {
                    _endpointConfigurationInfo.Hostvfy = value;
                    _isConfigChanged = true;
                    OnPropertyChanged(nameof(HostVerifySelectedType));
                }
            }
        }

        public string Username
        {
            get => _endpointConfigurationInfo?.Username ?? string.Empty;
            set
            {
                if (_endpointConfigurationInfo != null && _endpointConfigurationInfo.Username != value)
                {
                    _endpointConfigurationInfo.Username = value;
                    _isConfigChanged = true;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        public string Password
        {
            get => _endpointConfigurationInfo?.Password ?? string.Empty;
            set
            {
                if (_endpointConfigurationInfo != null && _endpointConfigurationInfo.Password != value)
                {
                    _endpointConfigurationInfo.Password = value;
                    _isConfigChanged = true;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        public string CommandTopic
        {
            get => _endpointConfigurationInfo?.Subname ?? string.Empty;
            set
            {
                if (_endpointConfigurationInfo != null && _endpointConfigurationInfo.Subname != value)
                {
                    _endpointConfigurationInfo.Subname = value;
                    _isConfigChanged = true;
                    OnPropertyChanged(nameof(CommandTopic));
                }
            }
        }

        public string ResponseTopic
        {
            get => _endpointConfigurationInfo?.Pub1name ?? string.Empty;
            set
            {
                if (_endpointConfigurationInfo != null && _endpointConfigurationInfo.Pub1name != value)
                {
                    _endpointConfigurationInfo.Pub1name = value;
                    _isConfigChanged = true;
                    OnPropertyChanged(nameof(ResponseTopic));
                }
            }
        }

        public string EventTopic
        {
            get => _endpointConfigurationInfo?.Pub2name ?? string.Empty;
            set
            {
                if (_endpointConfigurationInfo != null && _endpointConfigurationInfo.Pub2name != value)
                {
                    _endpointConfigurationInfo.Pub2name = value;
                    _isConfigChanged = true;
                    OnPropertyChanged(nameof(EventTopic));
                }
            }
        }
        public EndpointSettingsViewModel()
        {
            LoadConfigurationCommand = new Command<string>(async (epName) => await LoadConfigurationAsync(epName), CanExecuteLoad);
            SaveConfigurationCommand = new Command<ENUM_EP_OPERATION>(async (operation) => await SaveConfigurationAsync(operation), CanExecuteSave);
            if (_endpointConfigurationInfo?.Type == ENUM_EP_TYPE.Mdm)
            {
                LoadCertificatesAsync().ConfigureAwait(false);
            }
        }

        private bool CanExecuteLoad(string epName)
        {
            System.Diagnostics.Debug.WriteLine("CanExecuteLoad" );

            return !string.IsNullOrEmpty(epName);
        }

        private bool CanExecuteSave(ENUM_EP_OPERATION operation)
        {
            return _isConfigChanged; // Only allow save if the configuration has changed
        }
        public ICommand LoadConfigurationCommand { get; }
        public ICommand SaveConfigurationCommand { get; }

        public async Task LoadConfigurationAsync(string epName)
        {
            try
            {

                if (epName != null)
                {
                    System.Diagnostics.Debug.WriteLine("endpoint"+ epName);

                    
                    _endpointConfigurationInfo = await Task.Run(() => rfidModel.rfidReader.Config.GetEndpointConfigByName(epName));


                    if (_endpointConfigurationInfo != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Loading configuration " + _endpointConfigurationInfo.Protocol + " " + _endpointConfigurationInfo.Epname);
                        System.Diagnostics.Debug.WriteLine($"Before Loading Certificates : {_endpointConfigurationInfo.Cacertname} {_endpointConfigurationInfo.Certname}");
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            OnPropertyChanged(nameof(Name));
                            OnPropertyChanged(nameof(Url));
                            OnPropertyChanged(nameof(SelectedType));
                            OnPropertyChanged(nameof(Port));
                            OnPropertyChanged(nameof(KeepAlive));
                            OnPropertyChanged(nameof(TenantId));
                            OnPropertyChanged(nameof(ProtocolSelectedType));
                            OnPropertyChanged(nameof(CleanSession));
                            OnPropertyChanged(nameof(MinReconnectDelay));
                            OnPropertyChanged(nameof(MaxReconnectDelay));
                            OnPropertyChanged(nameof(HostVerifySelectedType));
                            OnPropertyChanged(nameof(Username));
                            OnPropertyChanged(nameof(Password));
                            OnPropertyChanged(nameof(ResponseTopic));
                            OnPropertyChanged(nameof(EventTopic));
                            OnPropertyChanged(nameof(CommandTopic));
                            OnPropertyChanged(nameof(SelectedCACertificate));
                            OnPropertyChanged(nameof(SelectedClientCertificate));
                            OnPropertyChanged(nameof(SelectedPrivateKey));
                            OnPropertyChanged(nameof(DisplaySelectedCACertificate));
                            OnPropertyChanged(nameof(DisplaySelectedClientCertificate));
                            OnPropertyChanged(nameof(DisplaySelectedPrivateKey));

                        });
                        if (_endpointConfigurationInfo.Type == ENUM_EP_TYPE.Mdm)
                        {
                            await LoadCertificatesAsync();
                        }
                        System.Diagnostics.Debug.WriteLine($"Certificates : {_endpointConfigurationInfo.Cacertname} {_endpointConfigurationInfo.Certname}");


                        // Add OnPropertyChanged calls for all other properties
                    }
                    else
                        {
                            System.Diagnostics.Debug.WriteLine("Loading configuration EndpointInfo null"+ _endpointConfigurationInfo.Epname);
                        }
                }
                //OnPropertyChanged(); // Refresh all bindings
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading configuration: {ex.Message}");
              
            }
        }

        public async Task SaveConfigurationAsync(ENUM_EP_OPERATION eP_OPERATION)
        {
            try
            {
                RFIDResults rFIDResults;
                if (_endpointConfigurationInfo != null)
                {
                    _endpointConfigurationInfo.Operation = eP_OPERATION;
                    EnsureDefaultsBeforeSave();
                    System.Diagnostics.Debug.WriteLine($"saving endpoint {_endpointConfigurationInfo.Epname} url {_endpointConfigurationInfo.Url} port {_endpointConfigurationInfo.Port} keepAlive {_endpointConfigurationInfo.Keepalive} cleansession en {_endpointConfigurationInfo.Encleanss} ds{_endpointConfigurationInfo.Dscleanss} Hostverify {_endpointConfigurationInfo.Hostvfy}");

                    await Task.Run(() => rfidModel.rfidReader.Config.SetEndpointConfiguration(_endpointConfigurationInfo)); 

                    _endpointConfigurationInfo = new EndpointConfigurationInfo();
                    _endpointConfigurationInfo.Operation = ENUM_EP_OPERATION.Save;
                    rFIDResults = await Task.Run(() => rfidModel.rfidReader.Config.SetEndpointConfiguration(_endpointConfigurationInfo)); 
                    if (rFIDResults == RFIDResults.RfidApiSuccess)
                    {
                        System.Diagnostics.Debug.WriteLine("Configuration saved successfully.");
                        _isConfigChanged = false;
                        ShowAlert("Configuration saved successfully.");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Error in saving configuration");
                        ShowAlert("Error in saving configuration.");
                    }                 
                }
               
            }
            catch(InvalidUsageException iuex)
            {
                ShowAlert("Invalid input in configuration.");
                System.Diagnostics.Debug.WriteLine($"InvalidUsageException in saving configuration: {iuex.Message}");
               
            }
            catch (OperationFailureException ofex)
            {
                ShowAlert("Operation failed in saving configuration.");
                System.Diagnostics.Debug.WriteLine($"OperationFailureException in saving configuration: {ofex.VendorMessage}, Result: {ofex.Results}");
                if (ofex.Results == RFIDResults.AdminConnectError)
                {
                    await AuthenticaionDialog.ShowConnectionLostDialog();
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error in saving configuration.");
                System.Diagnostics.Debug.WriteLine($"Error saving configuration: {ex.Message}");
               
            }
        }


        private void EnsureDefaultsBeforeSave()
        {
            if (_endpointConfigurationInfo.Protocol == null)
            {
                _endpointConfigurationInfo.Protocol = ENUM_EP_PROTOCOL_TYPE.EpProtoTypMqtt;
            }
            if (_endpointConfigurationInfo.Type == null)
            {
                _endpointConfigurationInfo.Type = ENUM_EP_TYPE.Soti;
            }
            if (_endpointConfigurationInfo.Hostvfy == null)
            {
                _endpointConfigurationInfo.Hostvfy = ENUM_HOST_VERIFY.VerifyNone;
            }

            _endpointConfigurationInfo.Cacertname = string.IsNullOrEmpty(_endpointConfigurationInfo.Cacertname) ? null : _endpointConfigurationInfo.Cacertname;
            _endpointConfigurationInfo.Certname = string.IsNullOrEmpty(_endpointConfigurationInfo.Certname) ? null : _endpointConfigurationInfo.Certname;
            _endpointConfigurationInfo.Keyname = string.IsNullOrEmpty(_endpointConfigurationInfo.Keyname) ? null : _endpointConfigurationInfo.Keyname;
            _endpointConfigurationInfo.Pub1name = string.IsNullOrEmpty(_endpointConfigurationInfo.Pub1name) ? null : _endpointConfigurationInfo.Pub1name;
            _endpointConfigurationInfo.Pub2name = string.IsNullOrEmpty(_endpointConfigurationInfo.Pub2name) ? null : _endpointConfigurationInfo.Pub2name;
            _endpointConfigurationInfo.Subname = string.IsNullOrEmpty(_endpointConfigurationInfo.Subname) ? null : _endpointConfigurationInfo.Subname;
            _endpointConfigurationInfo.Username = string.IsNullOrEmpty(_endpointConfigurationInfo.Username) ? null : _endpointConfigurationInfo.Username;
            _endpointConfigurationInfo.Password = string.IsNullOrEmpty(_endpointConfigurationInfo.Password) ? null : _endpointConfigurationInfo.Password;

        }


        public void ShowAlert(string v)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Toast.MakeText(Android.App.Application.Context, v, ToastLength.Short).Show();
            });
            Console.WriteLine(v);
        }

        public ObservableCollection<string> CACertificates { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> ClientCertificates { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> PrivateKeys { get; } = new ObservableCollection<string>();

      //  public string DisplaySelectedCACertificate => string.IsNullOrEmpty(SelectedCACertificate) ? "None" : SelectedCACertificate;
      //  public string DisplaySelectedClientCertificate => string.IsNullOrEmpty(SelectedClientCertificate) ? "None" : SelectedClientCertificate;
        public string DisplaySelectedPrivateKey => string.IsNullOrEmpty(SelectedPrivateKey) ? "None" : SelectedPrivateKey;

        public string DisplaySelectedCACertificate
        {
            get {
                return _endpointConfigurationInfo.Cacertname ?? "None";
            }
           
        }

        public string DisplaySelectedClientCertificate
        {
            get
            {
                return _endpointConfigurationInfo.Certname ?? "None";
            }

        }

        public string SelectedCACertificate
        {
            get {
                return  _endpointConfigurationInfo.Cacertname ?? string.Empty;
            }
            set
            {
                if (_endpointConfigurationInfo != null && _endpointConfigurationInfo.Cacertname != value)
                {

                    _endpointConfigurationInfo.Cacertname = value;
                    _isConfigChanged = true;
                    OnPropertyChanged(nameof(SelectedCACertificate));
                    OnPropertyChanged(nameof(DisplaySelectedCACertificate));
                   
                }
            }
        }

        public string SelectedClientCertificate
        {
            get => _endpointConfigurationInfo.Certname ?? string.Empty;
            set
            {
                if (_endpointConfigurationInfo != null && _endpointConfigurationInfo.Certname != value)
                {
                    System.Diagnostics.Debug.WriteLine($"Setting Client Cert: {_endpointConfigurationInfo.Certname}");
                    _endpointConfigurationInfo.Certname = value;
                    _isConfigChanged = true;
                    OnPropertyChanged(nameof(SelectedClientCertificate));
                    OnPropertyChanged(nameof(DisplaySelectedClientCertificate));
                }
            }
        }

        public string SelectedPrivateKey
        {
            get => _endpointConfigurationInfo.Keyname ?? string.Empty;
            set
            {
                if (_endpointConfigurationInfo != null && _endpointConfigurationInfo.Keyname != value)
                {
                    _endpointConfigurationInfo.Keyname = value;
                    _isConfigChanged = true;
                    OnPropertyChanged(nameof(SelectedPrivateKey));
                    OnPropertyChanged(nameof(DisplaySelectedPrivateKey));
                }
            }
        }


        public async Task LoadCertificatesAsync()
        {
            try
            {
                var wpaCertificates = rfidModel.rfidReader.Config.Certificates;
                System.Diagnostics.Debug.WriteLine($"Total certificates {wpaCertificates.Count}");
                // Simulate fetching certificates from the reader
                if (wpaCertificates != null && wpaCertificates.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"call  GetCertificates : {_endpointConfigurationInfo.Cacertname} {_endpointConfigurationInfo.Certname}");
                    CACertificates.Add("");
                    ClientCertificates.Add("");
                    PrivateKeys.Add("");
                    
                    foreach (var certificate in wpaCertificates)
                    {
                        if (certificate.EndsWith("ca_cert", StringComparison.OrdinalIgnoreCase))
                        {
                            CACertificates.Add(certificate);
                        }
                        else if (certificate.EndsWith("client_cert", StringComparison.OrdinalIgnoreCase))
                        {
                            ClientCertificates.Add(certificate);
                        }
                        else if (certificate.EndsWith("client_key", StringComparison.OrdinalIgnoreCase) || certificate.EndsWith("client_key", StringComparison.OrdinalIgnoreCase))
                        {
                            PrivateKeys.Add(certificate);
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"After Get Certificates : {_endpointConfigurationInfo.Cacertname} {_endpointConfigurationInfo.Certname}");
                }
            }
            catch (InvalidUsageException iuex)
            {
                System.Diagnostics.Debug.WriteLine($"InvalidUsageException in LoadCertificatesAsync: {iuex.Message}");
            }
            catch (OperationFailureException ofex)
            {
                System.Diagnostics.Debug.WriteLine($"OperationFailureException in LoadCertificatesAsync: {ofex.VendorMessage}");
                if (ofex.Results == RFIDResults.AdminConnectError)
                {
                    await AuthenticaionDialog.ShowConnectionLostDialog();
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error LoadCertificatesAsync: {ex.Message}");

            }
           
        }

    }
}
