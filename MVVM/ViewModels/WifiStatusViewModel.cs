using Android.Widget;
using Com.Zebra.Rfid.Api3;
using MauiRfidSample.MVVM.Services;
using MauiRfidSample.MVVM.Views;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MauiRfidSample.MVVM.ViewModels
{
    public class WifiStatusViewModel : BaseViewModel
    {
        ENUM_WIFI_BAND eNUM;
        private ObservableCollection<string> status;

        public ObservableCollection<string> WifiStatus
        {
            get => status;
            set
            {
                status = value;
                OnPropertyChanged();
            }
        }

        private bool is24GHzChecked;
        private bool is5GHzNonDFSChecked;
        private bool is5GHzDFSChecked;

        public bool Is24GHzChecked
        {
            get => is24GHzChecked;
            set
            {
                if (is24GHzChecked != value)
                {
                    is24GHzChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Is5GHzNonDFSChecked
        {
            get => is5GHzNonDFSChecked;
            set
            {
                if (is5GHzNonDFSChecked != value)
                {
                    is5GHzNonDFSChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Is5GHzDFSChecked
        {
            get => is5GHzDFSChecked;
            set
            {
                if (is5GHzDFSChecked != value)
                {
                    is5GHzDFSChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SaveChannelBandCommand { get; }

        public WifiStatusViewModel()
        {
            WifiStatus = new ObservableCollection<string>();
            SaveChannelBandCommand = new Command(async () => await SaveChannelBandAsync());
        }

        public async Task LoadWifiStatus()
        {
            System.Diagnostics.Debug.WriteLine("LoadWifi");
            try
            {
                if (rfidModel.rfidReader != null && rfidModel.rfidReader.IsConnected)
                {
                    IDictionary<string, string>? statusResults = new Dictionary<string, string>();
                    IDictionary<string, string>? _wifiGetStatus = new Dictionary<string, string>();
                    statusResults = rfidModel.rfidReader.Config.WifiStatusMap(_wifiGetStatus); // API call

                    // Reset checkboxes before updating
                    Is24GHzChecked = false;
                    Is5GHzNonDFSChecked = false;
                    Is5GHzDFSChecked = false;

                    //WifiStatus.Clear();
                    if (statusResults.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine("RfidAPisucces");
                        System.Diagnostics.Debug.WriteLine($"Wifistatus {statusResults} length {statusResults.Count}");
                        foreach (var key in statusResults.Keys)
                        {
                            System.Diagnostics.Debug.WriteLine($"Key {key}");
                            if (key == "PreferredSSID" || key == "2_4GHzAllowedChannelListMask" ||
                                key == "5GHzNonDFSAllowedChannelListMask" || key == "5GHzDFSAllowedChannelListMask" ||
                                key == "ChannelListMask")
                            {
                                continue;
                            }
                            else if (key == "ChannelListEnum")
                            {
                                string keys = statusResults[key];
                                switch (keys)
                                {
                                    case "1":
                                        Is24GHzChecked = true;
                                        break;
                                    case "2":
                                        Is5GHzNonDFSChecked = true;
                                        break;
                                    case "3":
                                        Is24GHzChecked = true;
                                        Is5GHzNonDFSChecked = true;
                                        break;
                                    case "4":
                                        Is5GHzDFSChecked = true;
                                        break;
                                    case "5":
                                        Is5GHzDFSChecked = true;
                                        Is24GHzChecked = true;
                                        break;
                                    case "6":
                                        Is5GHzDFSChecked = true;
                                        Is5GHzNonDFSChecked = true;
                                        break;
                                    case "7":
                                        Is24GHzChecked = true;
                                        Is5GHzNonDFSChecked = true;
                                        Is5GHzDFSChecked = true;
                                        break;
                                }
                            }
                            else if (key == "wifi")
                            {
                                string keys = statusResults[key];
                                string formattedKey = "Wi-Fi";
                                System.Diagnostics.Debug.WriteLine($"{formattedKey}  : {keys}");
                                WifiStatus.Add($"{formattedKey}  : {keys}");
                            }
                            else if (key == "WifiRegion")
                            {
                                string keys = statusResults[key];
                                string formattedKey = "Wi-Fi Region";
                                System.Diagnostics.Debug.WriteLine($"{formattedKey}  : {keys}");
                                WifiStatus.Add($"{formattedKey}  : {keys}");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"{key}  : {statusResults[key]}");
                                WifiStatus.Add($"{key}  : {statusResults[key]}");
                            }
                        }
                    }

                }


            }
            catch (InvalidUsageException iuex)
            {
                System.Diagnostics.Debug.WriteLine("InvalidUsageException in Getting WifiStatus: " + iuex.Message);
            }
            catch (OperationFailureException ofex)
            {
                System.Diagnostics.Debug.WriteLine("OperationFailureException in Getting WifiStatus: " + ofex.Message);
                if (ofex.Results == RFIDResults.AdminConnectError)
                {
                    await AuthenticaionDialog.ShowConnectionLostDialog();
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in Getting WifiStatus");
            }
        }

        private async Task SaveChannelBandAsync()
        {
            RFIDResults result = null;
            int enumValue = 0;

            if (Is24GHzChecked)
            {
                enumValue |= ENUM_WIFI_BAND.B24GHz.EnumValue;
            }
            if (Is5GHzNonDFSChecked)
            {
                enumValue |= ENUM_WIFI_BAND.B5GHzNONDFS.EnumValue;
            }
            if (Is5GHzDFSChecked)
            {
                enumValue |= ENUM_WIFI_BAND.B5GHzDFS.EnumValue;
            }

            try
            {
                if (rfidModel.rfidReader != null && rfidModel.rfidReader.IsConnected)
                {
                    result = rfidModel.rfidReader.Config.EnableWifiChannelBand(enumValue);
                }
                if(result == RFIDResults.RfidApiSuccess)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Toast.MakeText(Android.App.Application.Context, "Channel Band updated successfully", ToastLength.Short).Show();
                    });
                    await Shell.Current.GoToAsync("..");
                }
                
            }
            catch (InvalidUsageException e)
            {
                System.Diagnostics.Debug.WriteLine($"InvalidUsageException: {e.Message}");
            }
            catch (OperationFailureException e)
            {
                if (e.Results == RFIDResults.AdminConnectError)
                {
                    await AuthenticaionDialog.ShowConnectionLostDialog();
                    await Shell.Current.GoToAsync("..");
                }
                System.Diagnostics.Debug.WriteLine($"OperationFailureException: {e.Message}");
            }
        }
    }
}
