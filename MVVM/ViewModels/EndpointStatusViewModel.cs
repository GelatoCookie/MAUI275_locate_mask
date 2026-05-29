using Android.Widget;
using Com.Zebra.Rfid.Api3;
using MauiRfidSample.MVVM.Models;
using MauiRfidSample.MVVM.Services;
using Microsoft.Maui.Dispatching;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MauiRfidSample.MVVM.ViewModels
{
    public class EndpointStatusViewModel : BaseViewModel
    {
        private string _connectionStatus;
        private bool _isConnected;
        private string _status;
        
        public ObservableCollection<EndpointStatusEventData> EndpointEvents { get; } = new ObservableCollection<EndpointStatusEventData>();
        
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set
            {
                _connectionStatus = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged();
            }
        }
        
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
        

        
        public EndpointStatusViewModel()
        {
            // Initialize with default values
            ConnectionStatus = "Unknown";
            Status = "Checking status...";
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

        }

        public void ShowAlert(string v)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Toast.MakeText(Android.App.Application.Context, v, ToastLength.Short).Show();
            });
            Console.WriteLine(v);
        }

        public async Task LoadStatusAsync()
        {
            try
            {
                if (rfidModel.rfidReader != null && rfidModel.rfidReader.IsConnected)
                {
                    IsConnected = true;
                    ConnectionStatus = "Connected";
                    Status = "Reader connected";
                    
                    RFIDResults rFIDResults = rfidModel.rfidReader.Config.EndpointStatus;
                    System.Diagnostics.Debug.WriteLine($"Endpoint Status: {rFIDResults}");
                }
                else
                {
                    IsConnected = false;
                    ConnectionStatus = "Disconnected";
                    Status = "Reader not connected";
                }
            }
            catch (OperationFailureException ofex)
            {
                Status = $"Error: {ofex.VendorMessage}";
                
                if (ofex.Results == RFIDResults.AdminConnectError)
                {
                    await AuthenticaionDialog.ShowConnectionLostDialog();
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                Status = $"Error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error loading endpoint status: {ex}");
            }
        }

        private void AddEventToCollection(EndpointStatusEventData eventData)
        {
            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                
                EndpointEvents.Add(eventData);
                OnPropertyChanged(nameof(EndpointEvents));
            });
        }

        public override void IotMgmtStatusEvent(IEvents.IotEventMgmt statusEvent)
        {
            System.Diagnostics.Debug.WriteLine($" EventType: {statusEvent.EpType}, Status: {statusEvent.Status}, EpName {statusEvent.EpName}, Reason {statusEvent.Reason}, Cause {statusEvent.Cause}");
            
            AddEventToCollection(new EndpointStatusEventData
            {
                EventSource = "IoT Management",
                EventType = statusEvent.EpType,
                Status = statusEvent.Status,
                EpName = statusEvent.EpName,
                Reason = statusEvent.Reason,
                Cause = statusEvent.Cause
            });
        }

        public override void IotMgmtEvtStatusEvent(IEvents.IotEventMgmtEvt statusEvent)
        {
            System.Diagnostics.Debug.WriteLine($" EventType: {statusEvent.EpType}, Status: {statusEvent.Status}, EpName {statusEvent.EpName}, Reason {statusEvent.Reason}, Cause {statusEvent.Cause}");
            
            AddEventToCollection(new EndpointStatusEventData
            {
                EventSource = "IoT Management Event",
                EventType = statusEvent.EpType,
                Status = statusEvent.Status,
                EpName = statusEvent.EpName,
                Reason = statusEvent.Reason,
                Cause = statusEvent.Cause
            });
        }

        public override void IotControlStatusEvent(IEvents.IotEventControl statusEvent)
        {
            System.Diagnostics.Debug.WriteLine($" EventType: {statusEvent.EpType}, Status: {statusEvent.Status}, EpName {statusEvent.EpName}, Reason {statusEvent.Reason}, Cause {statusEvent.Cause}");
            
            AddEventToCollection(new EndpointStatusEventData
            {
                EventSource = "IoT Control",
                EventType = statusEvent.EpType,
                Status = statusEvent.Status,
                EpName = statusEvent.EpName,
                Reason = statusEvent.Reason,
                Cause = statusEvent.Cause
            });
        }

        public override void IotData1StatusEvent(IEvents.IotEventData1 statusEvent)
        {
            System.Diagnostics.Debug.WriteLine($" EventType: {statusEvent.EpType}, Status: {statusEvent.Status}, EpName {statusEvent.EpName}, Reason {statusEvent.Reason}, Cause {statusEvent.Cause}");
            
            AddEventToCollection(new EndpointStatusEventData
            {
                EventSource = "IoT Data 1",
                EventType = statusEvent.EpType,
                Status = statusEvent.Status,
                EpName = statusEvent.EpName,
                Reason = statusEvent.Reason,
                Cause = statusEvent.Cause
            });
        }

        public override void IotData2StatusEvent(IEvents.IotEventData2 statusEvent)
        {
            System.Diagnostics.Debug.WriteLine($" EventType: {statusEvent.EpType}, Status: {statusEvent.Status}, EpName {statusEvent.EpName}, Reason {statusEvent.Reason}, Cause {statusEvent.Cause}");
            
            AddEventToCollection(new EndpointStatusEventData
            {
                EventSource = "IoT Data 2",
                EventType = statusEvent.EpType,
                Status = statusEvent.Status,
                EpName = statusEvent.EpName,
                Reason = statusEvent.Reason,
                Cause = statusEvent.Cause
            });
        }
    }
}