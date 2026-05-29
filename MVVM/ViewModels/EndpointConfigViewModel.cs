using Android.Widget;
using Com.Zebra.Rfid.Api3;
using MauiRfidSample.MVVM.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiRfidSample.MVVM.ViewModels
{
    public class EndpointConfigViewModel : BaseViewModel
    {
        public ObservableCollection<EndpointItem> _endPointNames;

        public ObservableCollection<EndpointItem> EndpointList
        {
            get => _endPointNames;
            set
            {
                _endPointNames = value;
                OnPropertyChanged(nameof(EndpointList));
            }
        }

        public bool IsLoadingEndpoints { get; set; }

        public EndpointConfigViewModel()
        {
            if (rfidModel.rfidReader != null && rfidModel.rfidReader.IsConnected)
            {
                if (rfidModel.rfidReader.Transport.ToString().Equals(ENUM_TRANSPORT.QcSerial.ToString())
                        || rfidModel.rfidReader.Transport.ToString().Equals(ENUM_TRANSPORT.ReSerial.ToString())
                        || rfidModel.rfidReader.Transport.ToString().Equals(ENUM_TRANSPORT.ReUsb.ToString()))
                {
                    ShowAlert("Feature not supported for this device.");
                    return;
                }
            }

            _endPointNames = new ObservableCollection<EndpointItem>();
            EndpointList = new ObservableCollection<EndpointItem>();
        }

        public void ShowAlert(string v)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Toast.MakeText(Android.App.Application.Context, v, ToastLength.Short).Show();
            });
            Console.WriteLine(v);
        }

        public async Task LoadEndpointConfigAsync()
        {
            try
            {
                IsLoadingEndpoints = true;
                if (rfidModel.rfidReader != null && rfidModel.rfidReader.IsConnected)
                {
                    var endPointNames = rfidModel.rfidReader.Config.EndpointNames;
                    IotConfigInfo iotConfigInfo = rfidModel.rfidReader.Config.ActiveEndpoints;

                    EndpointList.Clear();
                    if (endPointNames != null)
                    {

                        foreach (var name in endPointNames)
                        {
                            bool isActiveEp = GetIot(name);
                            EndpointList.Add(new EndpointItem { EPName = name, IsChecked = isActiveEp });
                            System.Diagnostics.Debug.WriteLine("Endpoint : " + name);
                        }

                    }


                }
            }
            catch(InvalidUsageException iuex)
            {
                System.Diagnostics.Debug.WriteLine($"Error Loading endpoint names: {iuex.Message}");
                Console.WriteLine($"Error saving configuration: {iuex.Message}");
            }
            catch (OperationFailureException ofex)
            {
                System.Diagnostics.Debug.WriteLine($"Error Loading endpoint names: {ofex.VendorMessage}");
                Console.WriteLine($"Error saving configuration: {ofex.Message}");
                if (ofex.Results == RFIDResults.AdminConnectError)
                {
                    await AuthenticaionDialog.ShowConnectionLostDialog();
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error Loading endpoint names: {ex.Message}");
                Console.WriteLine($"Error saving configuration: {ex.Message}");
            }
            finally
            {
                IsLoadingEndpoints = false;
            }

        }

        public async Task RemoveConfigurationAsync(string epName)
        {
            try
            {
                EndpointConfigurationInfo _endpointConfigurationInfo = new EndpointConfigurationInfo();
                _endpointConfigurationInfo.Epname = epName;
                _endpointConfigurationInfo.Operation = ENUM_EP_OPERATION.Delete;
                RFIDResults rFIDResults = await Task.Run(() => rfidModel.rfidReader.Config.SetEndpointConfiguration(_endpointConfigurationInfo)); // EndpointConfiguration.SetConfig(_endpointConfigurationInfo));

                if (rFIDResults != null)
                {
                    System.Diagnostics.Debug.WriteLine("Configuration Deleted successfully.");
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Toast.MakeText(Android.App.Application.Context, "Endpoint Configuration Deleted successfully.", ToastLength.Short).Show();
                    });
                    OnPropertyChanged(nameof(EndpointList));
                    OnPropertyChanged();
                    await LoadEndpointConfigAsync();
                }

            }
            catch(InvalidUsageException iuex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Deleting configuration: {iuex.Message}");
                Console.WriteLine($"Error in Deleting configuration: {iuex.Message}");
            }
            catch(OperationFailureException ofex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Deleting configuration: {ofex.VendorMessage}");
                Console.WriteLine($"Error in Deleting configuration: {ofex.Message}");
                if (ofex.Results == RFIDResults.AdminConnectError)
                {
                    await AuthenticaionDialog.ShowConnectionLostDialog();
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving configuration: {ex.Message}");
                Console.WriteLine($"Error saving configuration: {ex.Message}");
            }
        }

        public void OnItemCheckedChanged(EndpointItem changedItem)
        {
            foreach (var item in _endPointNames)
            {
                if (!item.Equals(changedItem))
                {
                    item.IsChecked = false;
                }
            }
        }

        public void SetIot(EndpointItem endpointItem)
        {
            try
            {
                IotConfigInfo iotConfigInfo = new IotConfigInfo();
                
                bool isChecked = endpointItem.IsChecked;
                if (!isChecked)
                {
                    iotConfigInfo.Activemgmtep = "";
                }
                else
                {
                    iotConfigInfo.Activemgmtep = endpointItem.EPName;
                }
                if (rfidModel.rfidReader != null && rfidModel.rfidReader.IsConnected)
                {
                    rfidModel.rfidReader.Config.ActivateEndpoint(iotConfigInfo);
                }
            }
            catch(InvalidUsageException iuex)
            {
                System.Diagnostics.Debug.WriteLine($"Error Loading endpoint names: {iuex.Message}");
                Console.WriteLine($"Error saving configuration: {iuex.Message}");
            }
            catch (OperationFailureException ofex)
            {
                System.Diagnostics.Debug.WriteLine($"Error Loading endpoint names: {ofex.VendorMessage}");
                Console.WriteLine($"Error saving configuration: {ofex.Message}");
                if (ofex.Results == RFIDResults.AdminConnectError)
                {
                     AuthenticaionDialog.ShowConnectionLostDialog();
                     Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error Loading endpoint names: {ex.Message}");
                Console.WriteLine($"Error saving configuration: {ex.Message}");
            }

        }

        public bool GetIot( string endPointName)
        {
            if (rfidModel.rfidReader != null && rfidModel.rfidReader.IsConnected)
            {
                try { 
                    IotConfigInfo iotConfigInfo = rfidModel.rfidReader.Config.ActiveEndpoints;
                    if (iotConfigInfo != null)
                    {
                        if (iotConfigInfo.Activemgmtep != null && iotConfigInfo.Activemgmtep.Equals(endPointName))
                        {
                            return true;
                        }
                        if (iotConfigInfo.Activemgmtevtep != null && iotConfigInfo.Activemgmtevtep.Equals(endPointName))
                        {
                            return true;
                        }
                        if (iotConfigInfo.Activectrlep != null && iotConfigInfo.Activectrlep.Equals(endPointName))
                        {
                            return true;
                        }
                        if (iotConfigInfo.Activedat1ep != null && iotConfigInfo.Activedat1ep.Equals(endPointName))
                        {
                            return true;
                        }
                        if (iotConfigInfo.Activedat2ep != null && iotConfigInfo.Activedat2ep.Equals(endPointName))
                        {
                            return true;
                        }
                        if (iotConfigInfo.Backupmgmtep != null && iotConfigInfo.Backupmgmtep.Equals(endPointName))
                        {
                            return true;
                        }
                        if (iotConfigInfo.Backupmgmtevtep != null && iotConfigInfo.Backupmgmtevtep.Equals(endPointName))
                        {
                            return true;
                        }
                        if (iotConfigInfo.Backupctrlep != null && iotConfigInfo.Backupctrlep.Equals(endPointName))
                        {
                            return true;
                        }
                        if (iotConfigInfo.Backupdat1ep != null && iotConfigInfo.Backupdat1ep.Equals(endPointName))
                        {
                            return true;
                        }
                        if (iotConfigInfo.Backupdat2ep != null && iotConfigInfo.Backupdat2ep.Equals(endPointName))
                        {
                            return true;
                        }
                    }
                }
                catch (InvalidUsageException iuex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error Loading endpoint names: {iuex.Message}");
                    Console.WriteLine($"Error saving configuration: {iuex.Message}");
                }
                catch (OperationFailureException ofex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error Loading endpoint names: {ofex.VendorMessage}");
                    Console.WriteLine($"Error saving configuration: {ofex.Message}");
                    if (ofex.Results == RFIDResults.AdminConnectError)
                    {
                         AuthenticaionDialog.ShowConnectionLostDialog();
                         Shell.Current.GoToAsync("..");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error Loading endpoint names: {ex.Message}");
                    Console.WriteLine($"Error saving configuration: {ex.Message}");
                }

            }
            return false;
        }
    }
}
