using Android.Bluetooth;
using Android.Nfc;
using Android.OS;
using Android.Util;
using Android.Widget;
using Com.Zebra.Rfid.Api3;
using MauiRfidSample.MVVM.Services;
using MauiRfidSample.MVVM.Views;
using Microsoft.Maui.Controls.Compatibility;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using static Android.Views.WindowInsetsAnimation;


namespace MauiRfidSample.MVVM.Models
{
    public class ReaderModel : Java.Lang.Object, Readers.IRFIDReaderEventHandler, IRfidEventsListener, IWifiScanDataEventsListener
    {
        private static ReaderModel _ReaderModel;

        public IList<ReaderDevice> ReadersList = new List<ReaderDevice>();
        public RFIDReader rfidReader;
        public ImpinjExtensions impinjExtensions;
        private Readers readers;
        ReaderDevice readerDevice;
        public bool isBatchMode = false;
        private bool bluetoothEnabledWithPermission = false;
        private ENUM_TRANSPORT? _preferredTransport = null;
        private bool _isPromptingTransport;

        private ReaderModel()
        {
            //Readers.Attach(this);
            MainActivity mainActivity = Platform.CurrentActivity as MainActivity;
            mainActivity.CheckPermissions(OnRequestPermissionsResult);

        }

        private void OnRequestPermissionsResult(bool permissionGranted)
        {
            if (permissionGranted)
            {
                BluetoothManager bluetoothManager = (BluetoothManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.BluetoothService);
                if (!bluetoothManager.Adapter.IsEnabled)
                {
                    MainActivity mainActivity = Platform.CurrentActivity as MainActivity;
                    mainActivity.CheckBTEnable(OnRequestBTEnable);
                }
                else
                {
                    RFIDReader.Logger.EnableDebugLogs(true);

                  //  IRFIDLogger.GetLogger("rfidapi3").EnableDebugLogs(true);
                    PrepareAndSetup();
                    bluetoothEnabledWithPermission = true;
                }
            }
            else
            {
                PrepareAndSetup();
                bluetoothEnabledWithPermission = false;
            }
        }

        private void OnRequestBTEnable(bool btEnabled)
        {
            if (btEnabled)
            {
                PrepareAndSetup();
                bluetoothEnabledWithPermission = true;
            }
            else
            {
                PrepareAndSetup();
                bluetoothEnabledWithPermission = false;
            }
        }


        /// <summary>
        /// Connnect with reader after instance creation
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Setup()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                GetAvailableReaders();
                ConnectReader(0);
            });
        }

        #region Events

        /// <summary>
        /// Delegate for tag read event
        /// </summary>
        /// <param name="tags">array of tag data</param>
        internal delegate void TagReadHandler(TagData[] tags);
        internal delegate void TriggerHandler(bool pressed);
        internal delegate void StatusHandler(IEvents.StatusEventData statusEvent);
        internal delegate void ConnectionHandler(bool connected);
        internal delegate void ReaderAppearanceHandler(bool appeared);
        internal delegate void WiFiNotification(string scanStatus);
        internal delegate void WiFiScanResults(WifiScanData scanData);
        internal delegate void UpdateFirmwareProgressHandler(IEvents.FirmwareUpdateEvent firmwareUpdateEvent);
        internal delegate void IotEventMgmtHandler(IEvents.IotEventMgmt Status);
        internal delegate void IotEventMgmtEvtHandler(IEvents.IotEventMgmtEvt Status);
        internal delegate void IotEventControlHandler(IEvents.IotEventControl Status);
        internal delegate void IotEventData1Handler(IEvents.IotEventData1 Status);
        internal delegate void IotEventData2Handler(IEvents.IotEventData2 Status);
        internal delegate void CertificateEventHandler(IEvents.CertificateEventData Status);

        internal event TagReadHandler TagRead;
        internal event TriggerHandler TriggerEvent;
        internal event StatusHandler StatusEvent;
        internal event ConnectionHandler ReaderConnectionEvent;
        internal event ReaderAppearanceHandler ReaderAppearanceEvent;
        internal event WiFiNotification WiFiNotificationEvent;
        internal event WiFiScanResults WiFiScanResultsEvent;
        internal event UpdateFirmwareProgressHandler UpdateFirmwareProgressResult;
        internal event IotEventMgmtHandler IotEventMgmt;
        internal event IotEventMgmtEvtHandler IotEventMgmtEvt;
        internal event IotEventControlHandler IotEventControl;
        internal event IotEventData1Handler IotEventData1;
        internal event IotEventData2Handler IotEventData2;
        internal event CertificateEventHandler CertificateEvent;


        #endregion

        public Boolean isConnected
        {
            get => getConnectionStatus();
        }

        private bool getConnectionStatus()
        {
            if (rfidReader != null)
            {
                try
                {
                    return rfidReader.IsConnected;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        public static ReaderModel readerModel
        {
            get
            {
                if (_ReaderModel == null)
                    _ReaderModel = new ReaderModel();
                return _ReaderModel;
            }
        }

        //private int LastConnectedReaderIndex
        //{
        //    get
        //    {
        //        int index = 0;
        //        try
        //        {
        //            index = (int)Application.Current.Properties["ReaderIndex"];
        //        }
        //        catch (KeyNotFoundException) { }
        //        return index;
        //    }
        //    set
        //    {
        //        Application.Current.Properties["ReaderIndex"] = value;
        //        Application.Current.SavePropertiesAsync();
        //    }
        //}


        /// <summary>
        /// Set trigger mode to rfid on resume / screen appearance or start
        /// </summary>
        internal void SetTriggerMode()
        {
            //Try to connect
            try
            {
                if (rfidReader != null && !rfidReader.IsConnected)
                {
                    ConnectReader(0);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("RFID Reader Connection error");
            }
            if (isConnected)
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        rfidReader.Config.SetTriggerMode(ENUM_TRIGGER_MODE.RfidMode, true);
                    }
                    catch (OperationFailureException e)
                    {
                        e.PrintStackTrace();
                    }
                });
            }
        }

        public void RFIDReaderAppeared(ReaderDevice readerDevice)
        {
            ReadersList.Add(readerDevice);
            ReaderAppearanceEvent?.Invoke(true);
        }

        public void RFIDReaderDisappeared(ReaderDevice readerDevice)
        {
            if (ReadersList.Contains(readerDevice))
            {
                ReadersList.Remove(readerDevice);
            }
            ReaderAppearanceEvent?.Invoke(false);
            if (rfidReader != null && rfidReader.IsConnected)
            {
                rfidReader.Disconnect();
            }
        }

        public IList<ReaderDevice> GetAvailableReaders()
        {
            bool serialDeviceNotFound = false;

            if (_preferredTransport != null)
            {
                try
                {
                    if (readers == null)
                    {
                        ReadersList.Clear();
                        System.Diagnostics.Debug.WriteLine("Trying to get readers with transport " + _preferredTransport);
                        readers = new Readers(Android.App.Application.Context, _preferredTransport);

                        //readers = new Readers(Android.App.Application.Context, ENUM_TRANSPORT.QcSerial);
                        ReadersList = readers.AvailableRFIDReaderList;
                    }   
                }
                catch (Exception)
                {
                    serialDeviceNotFound = true;
                    readers?.Dispose();
                    readers = null;
                }
            }

            //if (ReadersList == null || ReadersList.Count == 0)
            //{
            //    try
            //    {
            //        if (readers == null)
            //            readers = new Readers(Android.App.Application.Context, ENUM_TRANSPORT.ReSerial);
            //        ReadersList = readers.AvailableRFIDReaderList;
            //    }
            //    catch (Exception)
            //    {
            //        serialDeviceNotFound = true;
            //        readers?.Dispose();
            //        readers = null;
            //    }
            //}

            //if (ReadersList != null && ReadersList.Count == 0)
            //{
            //    if (readers != null)
            //    {
            //        readers = new Readers(Android.App.Application.Context, ENUM_TRANSPORT.ServiceUsb);
            //        ReadersList = readers.AvailableRFIDReaderList;
            //    }
            //}

            //// Bluetooth (only if permission+adapter enabled)
            //if (ReadersList != null && ReadersList.Count == 0)
            //{
            //    if (readers != null && bluetoothEnabledWithPermission)
            //    {
            //        readers.Dispose();
            //        readers = new Readers(Android.App.Application.Context, ENUM_TRANSPORT.Bluetooth);
            //        ReadersList = readers.AvailableRFIDReaderList;
            //    }
            //}

            //if (ReadersList != null && ReadersList.Count == 0)
            //{
            //    if (readers != null)
            //    {
            //        readers.Dispose();
            //        readers = new Readers(Android.App.Application.Context, ENUM_TRANSPORT.ServiceSerial);
            //        ReadersList = readers.AvailableRFIDReaderList;
            //    }
            //}

            //if (ReadersList != null && ReadersList.Count == 0)
            //{
            //    if (readers != null)
            //    {
            //        readers = new Readers(Android.App.Application.Context, ENUM_TRANSPORT.ReUsb); // FXP20
            //        ReadersList = readers.AvailableRFIDReaderList;
            //    }
            //}

            Readers.Attach(this);
            return ReadersList;
        }

        public void ConnectReader(int index)
        {
            Console.WriteLine("ConnectReader" + index);
            ThreadPool.QueueUserWorkItem(o =>
            {
                ConnectReaderSync(index);
            });
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ConnectReaderSync(int index)
        {
            try
            {
                isBatchMode = false;
                Console.WriteLine("Available readers " + ReadersList.Count);
                if (ReadersList.Count > 0 && index < ReadersList.Count)
                {
                    readerDevice = ReadersList[index];
                    rfidReader = readerDevice.RFIDReader;
                    //rfidReader.ReinitTransport();
                    rfidReader.Connect();
                }
            }
            catch (InvalidUsageException e)
            {
                e.PrintStackTrace();
            }
            catch (OperationFailureException e)
            {
                e.PrintStackTrace();
                if (e.Results == RFIDResults.RfidBatchmodeInProgress)
                {
                    isBatchMode = true;
                    ShowAlert("" + e.Results);
                }
                else if (e.Results == RFIDResults.RfidChargingCommandNotAllowed)
                {
                    ShowAlert("Connection failed\n" + e.Results);
                }
              
                else
                {
                    ShowAlert("Connection failed : " + e.VendorMessage + " " + e.Results);
                    Console.WriteLine(e.StatusDescription);

                    //In case Region not configured, no UI is added to configure
                    ////- disconnecting to configure it
                    if (rfidReader != null && rfidReader.IsConnected)
                    {
                        rfidReader.Disconnect();
                    }
                    ReaderConnectionEvent?.Invoke(false);
                    return;
                }
            }

            if (rfidReader != null && rfidReader.IsConnected)
            {
                if (isBatchMode)
                {
                    ConfigureReader(false);
                }
                else
                {
                    ConfigureReader(true);
                }

                ReaderConnectionEvent?.Invoke(true);
                Console.WriteLine("Connected " + rfidReader.HostName);
                //LastConnectedReaderIndex = index;

                //setup Scanner SDK
                string NGEDeviceName = "RFID" + Build.Model;
                bool isDeviceRunsOnNGEProtocol = NGEDeviceName.Equals(rfidReader.HostName) || rfidReader.HostName.Contains("FXP20");
                if (!isDeviceRunsOnNGEProtocol)
                {
                    SetupScannerSDK(); // Only for handheld
                }
                 
            }
        }


        /// <summary>
        /// Configure Reader
        /// Setup event listnere and enable required event types
        /// Set trigger mode to rfid
        /// Configure antenna, singulation and trigger setting using single API call
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ConfigureReader(bool isFullConfiguration)
        {
            
            if (rfidReader != null && rfidReader.IsConnected)
            {
                try
                {
                    impinjExtensions = new ImpinjExtensions(rfidReader);
                    // receive events from reader
                    rfidReader.Events.AddEventsListener(this);
                    //ReaderConnection.rfidReader.Events.EventReadNotify += EventReadNotifier;
                    //ReaderConnection.rfidReader.Events.EventStatusNotify += EventStatusNotifier;

                    // HH event
                    rfidReader.Events.SetHandheldEvent(true);
                    
                    // tag event with tag data
                    rfidReader.Events.SetTagReadEvent(true);
                    rfidReader.Events.SetAttachTagDataWithReadEvent(false);
                    //
                    rfidReader.Events.SetInventoryStartEvent(true);
                    rfidReader.Events.SetInventoryStopEvent(true);
                    rfidReader.Events.SetOperationEndSummaryEvent(true);
                    rfidReader.Events.SetReaderDisconnectEvent(true);
                    rfidReader.Events.SetBatteryEvent(true);
                    rfidReader.Events.SetPowerEvent(true);
                    rfidReader.Events.SetTemperatureAlarmEvent(true);
                    rfidReader.Events.SetBufferFullEvent(true);
                    rfidReader.Events.SetBufferFullWarningEvent(true);
                    rfidReader.Events.SetFirmwareUpdateEvent(true);
                    rfidReader.Events.SetCertificateEvent(true);
                    rfidReader.Events.SetIotEvent(true);

                    //WiFi Event
                    rfidReader.Events.AddWifiScanDataEventsListener(this);
                    rfidReader.Events.SetWPAEvent(true);
                    rfidReader.Events.SetScanDataEvent(true);

                    if (isFullConfiguration)
                    {
                        // set trigger mode as rfid, pass second parameter as true so scanner will be disabled
                        rfidReader.Config.SetTriggerMode(ENUM_TRIGGER_MODE.RfidMode, true);
            
                        // configure for antenna and singulation etc.
                        var antenna = rfidReader.Config.Antennas.GetAntennaRfConfig(1);
                        antenna.SetrfModeTableIndex(0);
                        antenna.TransmitPowerIndex = rfidReader.ReaderCapabilities.GetTransmitPowerLevelValues().Length - 1;
                        rfidReader.Config.Antennas.SetAntennaRfConfig(1, antenna);

                        var singulation = rfidReader.Config.Antennas.GetSingulationControl(1);
                        singulation.Session = SESSION.SessionS0;
                        singulation.Action.InventoryState = INVENTORY_STATE.InventoryStateA;
                        singulation.Action.SetPerformStateAwareSingulationAction(false);
                        rfidReader.Config.Antennas.SetSingulationControl(1, singulation);

                        /*
                        // set start and stop triggers
                        TriggerInfo triggerInfo = new TriggerInfo();
                        triggerInfo.StartTrigger.TriggerType = START_TRIGGER_TYPE.StartTriggerTypeImmediate;
                        triggerInfo.StopTrigger.TriggerType = STOP_TRIGGER_TYPE.StopTriggerTypeImmediate;

                        rfidReader.Config.StartTrigger = triggerInfo.StartTrigger;
                        rfidReader.Config.StopTrigger = triggerInfo.StopTrigger;
                        */

                        TAG_FIELD[] tag_fields = { TAG_FIELD.PeakRssi, TAG_FIELD.TagSeenCount };
                        rfidReader.Config.TagStorageSettings.SetTagFields(tag_fields);

                        // If RFD8500 then disable batch mode and DPO
                        if (rfidReader.ReaderCapabilities.ModelName.Contains("RFD8500"))
                        {
                            rfidReader.Config.SetBatchMode(BATCH_MODE.Disable);
                            // Important: DPO should be disabled based on need here disabled for all operations
                            rfidReader.Config.DPOState = DYNAMIC_POWER_OPTIMIZATION.Disable;
                            //
                            rfidReader.Config.BeeperVolume = BEEPER_VOLUME.HighBeep;
                        }

                        var HostName = rfidReader.HostName;
                        var region = rfidReader.Config.RegulatoryConfig.Region;
                        var modelName = rfidReader.ReaderCapabilities.ModelName;
                        var serialNumber = rfidReader.ReaderCapabilities.SerialNumber;
                        var radioVersion = "";
                        var moduleVersion = "";
                        var deviceVersionInfo = new Android.Runtime.JavaDictionary<string, string>();
                        rfidReader.Config.GetDeviceVersionInfo(deviceVersionInfo);
                        if (deviceVersionInfo.ContainsKey(Constants.Nge))
                        {
                            radioVersion = deviceVersionInfo[Constants.Nge]; //NGE
                        }

                        if (deviceVersionInfo.ContainsKey(Constants.GenxDevice))
                        {
                            moduleVersion = deviceVersionInfo[Constants.GenxDevice]; //RFID_DEVICE
                        }
                        VersionInfo versioninfo = rfidReader.VersionInfo();

                        var message = string.Format("HostName:{0} \n Region:{1} \n ModelName:{2} \n SerialNumber:{3} \n RadioVersion:{4} \n ModuleVersion:{5} \n SDKVersion:{6}",
                                                           HostName, region, modelName, serialNumber, radioVersion, moduleVersion, versioninfo.Version);

                        Console.WriteLine(message);

                        rfidReader.Config.GetDeviceStatus(true, true, true);

                       
                    }

                    Console.WriteLine("Firmware Version: "+ rfidReader.ReaderCapabilities.FirwareVersion);
                }
                catch (InvalidUsageException e)
                {
                    e.PrintStackTrace();
                }
                catch (OperationFailureException e)
                {
                    e.PrintStackTrace();
                    ShowAlert(e);
                }
            }
        }


        internal bool SetKeyLayoutType(string selectedUTrigger, string selectedLTrigger)
        {
            if (rfidReader != null && rfidReader.IsConnected)
            {
                ENUM_NEW_KEYLAYOUT_TYPE upperTrigger = null, lowerTrigger = null;
                switch (selectedUTrigger)
                {
                    case AppConstants.RFID:
                        upperTrigger = ENUM_NEW_KEYLAYOUT_TYPE.Rfid;
                        break;
                    case AppConstants.SledScanner:
                        upperTrigger = ENUM_NEW_KEYLAYOUT_TYPE.SledScan;
                        break;
                    case AppConstants.TerminalScanner:
                        upperTrigger = ENUM_NEW_KEYLAYOUT_TYPE.TerminalScan;
                        break;
                    case AppConstants.ScanNotification:
                        upperTrigger = ENUM_NEW_KEYLAYOUT_TYPE.ScanNotify;
                        break;
                    case AppConstants.NoAction:
                        upperTrigger = ENUM_NEW_KEYLAYOUT_TYPE.NoAction;
                        break;
                }

                switch (selectedLTrigger)
                {
                    case AppConstants.RFID:
                        lowerTrigger = ENUM_NEW_KEYLAYOUT_TYPE.Rfid;
                        break;
                    case AppConstants.SledScanner:
                        lowerTrigger = ENUM_NEW_KEYLAYOUT_TYPE.SledScan;
                        break;
                    case AppConstants.TerminalScanner:
                        lowerTrigger = ENUM_NEW_KEYLAYOUT_TYPE.TerminalScan;
                        break;
                    case AppConstants.ScanNotification:
                        lowerTrigger = ENUM_NEW_KEYLAYOUT_TYPE.ScanNotify;
                        break;
                    case AppConstants.NoAction:
                        lowerTrigger = ENUM_NEW_KEYLAYOUT_TYPE.NoAction;
                        break;
                }
                try
                {
                    if (!isBatchMode)
                    {
                        RFIDResults rFIDResults = rfidReader.Config.SetKeylayoutType(upperTrigger, lowerTrigger);
                        if (rFIDResults == RFIDResults.RfidApiSuccess)
                        {
                            return true;
                        }
                    }
                }
                catch (InvalidUsageException e)
                {
                    e.PrintStackTrace();
                }
                catch (OperationFailureException e)
                {
                    e.PrintStackTrace();
                }
            }
            return false;
        }

        public string GetUpperTrigger()
        {
            if (rfidReader != null && rfidReader.IsConnected)
            {
                try
                {
                    var keylayoutType = rfidReader.Config.UpperTriggerValue;
                    if (keylayoutType.Equals(ENUM_NEW_KEYLAYOUT_TYPE.Rfid))
                    {
                        return AppConstants.RFID;
                    }
                    else if (keylayoutType.Equals(ENUM_NEW_KEYLAYOUT_TYPE.SledScan))
                    {
                        return AppConstants.SledScanner;
                    }
                    else if (keylayoutType.Equals(ENUM_NEW_KEYLAYOUT_TYPE.TerminalScan))
                    {
                        return AppConstants.TerminalScanner;
                    }
                    else if (keylayoutType.Equals(ENUM_NEW_KEYLAYOUT_TYPE.ScanNotify))
                    {
                        return AppConstants.ScanNotification;
                    }
                    else if (keylayoutType.Equals(ENUM_NEW_KEYLAYOUT_TYPE.NoAction))
                    {
                        return AppConstants.NoAction;
                    }
                    else
                    {
                        return AppConstants.NoAction;
                    }
                }
                catch (InvalidUsageException e)
                {
                    e.PrintStackTrace();
                }
                catch (OperationFailureException e)
                {
                    e.PrintStackTrace();
                }

            }
            return "";
        }

        public string GetLowerTrigger()
        {
            if (rfidReader != null && rfidReader.IsConnected)
            {
                try
                {
                    var keylayoutType = rfidReader.Config.LowerTriggerValue;
                    if (keylayoutType.Equals(ENUM_NEW_KEYLAYOUT_TYPE.Rfid))
                    {
                        return AppConstants.RFID;
                    }
                    else if (keylayoutType.Equals(ENUM_NEW_KEYLAYOUT_TYPE.SledScan))
                    {
                        return AppConstants.SledScanner;
                    }
                    else if (keylayoutType.Equals(ENUM_NEW_KEYLAYOUT_TYPE.TerminalScan))
                    {
                        return AppConstants.TerminalScanner;
                    }
                    else if (keylayoutType.Equals(ENUM_NEW_KEYLAYOUT_TYPE.ScanNotify))
                    {
                        return AppConstants.ScanNotification;
                    }
                    else if (keylayoutType.Equals(ENUM_NEW_KEYLAYOUT_TYPE.NoAction))
                    {
                        return AppConstants.NoAction;
                    }
                    else
                    {
                        return AppConstants.NoAction;
                    }
                }
                catch (InvalidUsageException e)
                {
                    e.PrintStackTrace();
                }
                catch (OperationFailureException e)
                {
                    e.PrintStackTrace();
                }

            }
            return "";
        }

        /*
		private void EventStatusNotifier(object sender, EventStatusNotifyEventArgs e)
		{
			
		}

		private void EventReadNotifier(object sender, EventReadNotifyEventArgs e)
		{
			
		}
		*/

        public void EventReadNotify(RfidReadEvents readEvent)
        {
            TagData[] myTags = rfidReader.Actions.GetReadTags(100);
            if (myTags != null)
            {
                ThreadPool.QueueUserWorkItem(o => TagRead?.Invoke(myTags));
            }
            else
            {
                TagData[] myMultiLocateTags = rfidReader.Actions.GetMultiTagLocateTagInfo(100);
                if(myMultiLocateTags != null)
                {
                    for (int index = 0; index < myMultiLocateTags.Length; index++)
                    {
                        if (myMultiLocateTags[index].MultiTagLocateInfo != null)
                        {
                            Console.WriteLine("MultiTagLocateInfo-Tag: " + myMultiLocateTags[index].TagID + 
                                " RelativeDistance: " + myMultiLocateTags[index].MultiTagLocateInfo.RelativeDistance);
                        }
                    }
                }
            }
        }

        public void EventStatusNotify(RfidStatusEvents rfidStatusEvents)
        {
            Console.WriteLine("Status Notification: " + rfidStatusEvents.StatusEventData.StatusEventType);
            if (rfidStatusEvents.StatusEventData.StatusEventType == STATUS_EVENT_TYPE.HandheldTriggerEvent)
            {
                if (rfidStatusEvents.StatusEventData.HandheldTriggerEventData.HandheldTriggerType.Equals(HANDHELD_TRIGGER_TYPE.HandheldTriggerRfid))
                {
                    if (rfidStatusEvents.StatusEventData.HandheldTriggerEventData.HandheldEvent == HANDHELD_TRIGGER_EVENT_TYPE.HandheldTriggerPressed)
                    {
                        ThreadPool.QueueUserWorkItem(o => TriggerEvent?.Invoke(true));
                    }
                    if (rfidStatusEvents.StatusEventData.HandheldTriggerEventData.HandheldEvent == HANDHELD_TRIGGER_EVENT_TYPE.HandheldTriggerReleased)
                    {
                        ThreadPool.QueueUserWorkItem(o => TriggerEvent?.Invoke(false));
                    }
                }
            }
            else if (rfidStatusEvents.StatusEventData.StatusEventType == STATUS_EVENT_TYPE.InventoryStartEvent)
            {
                ThreadPool.QueueUserWorkItem(o => StatusEvent?.Invoke(rfidStatusEvents.StatusEventData));
            }
            else if (rfidStatusEvents.StatusEventData.StatusEventType == STATUS_EVENT_TYPE.InventoryStopEvent)
            {
                ThreadPool.QueueUserWorkItem(o => StatusEvent?.Invoke(rfidStatusEvents.StatusEventData));
            }
            else if (rfidStatusEvents.StatusEventData.StatusEventType == STATUS_EVENT_TYPE.OperationEndSummaryEvent)
            {
                int rounds = rfidStatusEvents.StatusEventData.OperationEndSummaryData.TotalRounds;
                int totaltags = rfidStatusEvents.StatusEventData.OperationEndSummaryData.TotalTags;
                long timems = rfidStatusEvents.StatusEventData.OperationEndSummaryData.TotalTimeuS / 1000;
                Console.WriteLine("Summary: Rounds: " + rounds + " Tags: " + totaltags + " Time: " + timems);
                ThreadPool.QueueUserWorkItem(o => StatusEvent?.Invoke(rfidStatusEvents.StatusEventData));
            }
            else if (rfidStatusEvents.StatusEventData.StatusEventType == STATUS_EVENT_TYPE.DisconnectionEvent)
            {
                ThreadPool.QueueUserWorkItem(o => ReaderConnectionEvent?.Invoke(false));
                ShowAlert("Reader Disconnected");
            }
            else if (rfidStatusEvents.StatusEventData.StatusEventType == STATUS_EVENT_TYPE.BatteryEvent)
            {
                var battery = rfidStatusEvents.StatusEventData.BatteryData;
                Console.WriteLine("Battery: Cause: " + battery.Cause + " Charging: " + battery.Charging + " Level: " + battery.Level);
            }
            else if (rfidStatusEvents.StatusEventData.StatusEventType == STATUS_EVENT_TYPE.PowerEvent)
            {
                var power = rfidStatusEvents.StatusEventData.PowerData;
                Console.WriteLine("PowerData: Cause: " + power.Cause + " Current: " + power.Current + " Voltage: " + power.Voltage + " Power " + power.Power);
            }
            else if (rfidStatusEvents.StatusEventData.StatusEventType == STATUS_EVENT_TYPE.TemperatureAlarmEvent)
            {
                var temperature = rfidStatusEvents.StatusEventData.TemperatureAlarmData;
                Console.WriteLine("TemperatureAlarmEvent: AlarmLevel: " + temperature.AlarmLevel + " AmbientTemp: " + temperature.AmbientTemp + " Current: " + temperature.CurrentTemperature + " PATemp:" + temperature.PATemp);
            }
            else if (rfidStatusEvents.StatusEventData.StatusEventType == STATUS_EVENT_TYPE.BufferFullWarningEvent)
            {
                Console.WriteLine("BufferFullWarningEvent");
            }
            else if (rfidStatusEvents.StatusEventData.StatusEventType == STATUS_EVENT_TYPE.BufferFullEvent)
            {
                Console.WriteLine("BufferFullEvent: ");
            }
            else if (rfidStatusEvents.StatusEventData.StatusEventType == STATUS_EVENT_TYPE.WpaEvent)
            {
                string scanStatus = rfidStatusEvents.StatusEventData.WPAEventData.Type;
                ThreadPool.QueueUserWorkItem(o => WiFiNotificationEvent?.Invoke(scanStatus));
            }
            else if (rfidStatusEvents.StatusEventData.StatusEventType == STATUS_EVENT_TYPE.FirmwareUpdateEvent)
            {
                System.Diagnostics.Debug.WriteLine("STATUS_EVENT_TYPE.FirmwareUpdateEvent " + rfidStatusEvents.StatusEventData.FWEventData.OverallUpdateProgress);
                IEvents.FirmwareUpdateEvent firmwareUpdateEvent = rfidStatusEvents.StatusEventData.FWEventData;
                ThreadPool.QueueUserWorkItem(o => UpdateFirmwareProgressResult?.Invoke(firmwareUpdateEvent));
            }
            else if (rfidStatusEvents.StatusEventData.StatusEventType == STATUS_EVENT_TYPE.IotEventMgmt)
            {
                IEvents.IotEventMgmt iotEventMgmtStat = rfidStatusEvents.StatusEventData.IotEventMgmt;
                System.Diagnostics.Debug.WriteLine( " STATUS_EVENT_TYPE.IotEventMgmt " + iotEventMgmtStat.EpType+ " "+ iotEventMgmtStat.EpName + " " + iotEventMgmtStat.Status + " " + iotEventMgmtStat.Reason);
                if (iotEventMgmtStat != null)
                {
                    ThreadPool.QueueUserWorkItem(o => IotEventMgmt?.Invoke(iotEventMgmtStat));
                }
            }
            else if (rfidStatusEvents.StatusEventData.StatusEventType == STATUS_EVENT_TYPE.IotEventMgmtEvt)
            {
                IEvents.IotEventMgmtEvt iotEvent = rfidStatusEvents.StatusEventData.IotEventMgmtEvt;
                System.Diagnostics.Debug.WriteLine(" STATUS_EVENT_TYPE.IotEventMgmtEvt " + iotEvent.EpType + " " + iotEvent.EpName + " " + iotEvent.Status + " " + iotEvent.Reason);
                if (iotEvent != null)
                {
                    ThreadPool.QueueUserWorkItem(o => IotEventMgmtEvt?.Invoke(iotEvent));
                }
            }

            else if (rfidStatusEvents.StatusEventData.StatusEventType == STATUS_EVENT_TYPE.IotEventControl)
            {
                IEvents.IotEventControl iotEvent = rfidStatusEvents.StatusEventData.IotEventControl;
                System.Diagnostics.Debug.WriteLine(" STATUS_EVENT_TYPE.IotEventMgmtEvt " + iotEvent.EpType + " " + iotEvent.EpName + " " + iotEvent.Status + " " + iotEvent.Reason);
                if (iotEvent != null)
                {
                    ThreadPool.QueueUserWorkItem(o => IotEventControl?.Invoke(iotEvent));
                }
            }
            else if (rfidStatusEvents.StatusEventData.StatusEventType == STATUS_EVENT_TYPE.IotEventData1)
            {
                IEvents.IotEventData1 iotEvent = rfidStatusEvents.StatusEventData.IotEventData1;
                System.Diagnostics.Debug.WriteLine(" STATUS_EVENT_TYPE.IotEventMgmtEvt " + iotEvent.EpType + " " + iotEvent.EpName + " " + iotEvent.Status + " " + iotEvent.Reason);
                if (iotEvent != null)
                {
                    ThreadPool.QueueUserWorkItem(o => IotEventData1?.Invoke(iotEvent));
                }
            }
            else if (rfidStatusEvents.StatusEventData.StatusEventType == STATUS_EVENT_TYPE.IotEventData2)
            {
                IEvents.IotEventData2 iotEvent = rfidStatusEvents.StatusEventData.IotEventData2;
                System.Diagnostics.Debug.WriteLine(" STATUS_EVENT_TYPE.IotEventMgmtEvt " + iotEvent.EpType + " " + iotEvent.EpName + " " + iotEvent.Status + " " + iotEvent.Reason);
                if (iotEvent != null)
                {
                    ThreadPool.QueueUserWorkItem(o => IotEventData2?.Invoke(iotEvent));
                }
            }
            else if(rfidStatusEvents.StatusEventData.StatusEventType == STATUS_EVENT_TYPE.CertificateEvent)
            {
                IEvents.CertificateEventData certificateEvent = rfidStatusEvents.StatusEventData.CertificateEventData;
                System.Diagnostics.Debug.WriteLine(" STATUS_EVENT_TYPE.CertificateEvent " + certificateEvent.GetType() + " " + certificateEvent.Status );
                if (certificateEvent != null)
                {
                    ThreadPool.QueueUserWorkItem(o => CertificateEvent?.Invoke(certificateEvent));
                }
            }

        }
        

        internal bool PerformInventory()
        {
            try
            {
                if (rfidReader.IsConnected)
                {
                    rfidReader.ReinitTransport();
                    rfidReader.Actions.Inventory.Perform();
                    return true;
                }
            }
            catch (InvalidUsageException e)
            {
                e.PrintStackTrace();
            }
            catch (OperationFailureException e)
            {
                e.PrintStackTrace();
                ShowAlert(e);
            }
            return false;
        }

        internal void StopInventory()
        {
            try
            {
                rfidReader.Actions.Inventory.Stop();
            }
            catch (InvalidUsageException e)
            {
                e.PrintStackTrace();
            }
            catch (OperationFailureException e)
            {
                e.PrintStackTrace();
                ShowAlert(e);
            }
        }

        internal void Locate(bool start, string tagPattern, string tagMask)
        {
            try
            {
                if (start)
                {
                    rfidReader.Actions.TagLocationing.Perform(tagPattern, tagMask, null);
                }
                else
                {
                    rfidReader.Actions.TagLocationing.Stop();
                }
            }
            catch (InvalidUsageException e)
            {
                e.PrintStackTrace();
            }
            catch (OperationFailureException e)
            {
                e.PrintStackTrace();
                ShowAlert(e);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Disconnect()
        {
            Console.WriteLine("Disconnect" + rfidReader?.HostName);
            if (rfidReader != null)
            {
                try
                {
                    rfidReader.Disconnect();
                    ReaderConnectionEvent?.Invoke(false);
                }
                catch (InvalidUsageException e)
                {
                    e.PrintStackTrace();
                    Console.WriteLine(e.Info);
                }

                //Terminate Scanner SDK
                TerminateScannerSDK();
            }
        }

        public void DeInit()
        {
            Readers.Deattach(this);
            readers?.Dispose();
            readers = null;
        }

        private void ShowAlert(OperationFailureException e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                //Toast.MakeText(Android.App.Application.Context, e.VendorMessage, ToastLength.Short).Show();
            });
            Console.WriteLine(e.VendorMessage);
        }

        public void ShowAlert(string v)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Toast.MakeText(Android.App.Application.Context, v, ToastLength.Short).Show();
            });
            Console.WriteLine(v);
        }

        public bool AddWiFiProfile(WifiProfile wifiProfile)
        {
            if (rfidReader != null && rfidReader.IsConnected)
            {
                try
                {

                    RFIDResults rFIDResults = rfidReader.Config.WifiAddProfile(wifiProfile, false);
                    if (rFIDResults == RFIDResults.RfidApiSuccess)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (InvalidUsageException e)
                {
                    e.PrintStackTrace();
                }
                catch (OperationFailureException e)
                {
                    if (e.Results == RFIDResults.AdminConnectError)
                    {
                        AuthenticaionDialog.ShowConnectionLostDialog();
                         
                    }
                    e.PrintStackTrace();
                }

            }
            return false;
        }

        public bool DeleteWiFiProfile(string ssid)
        {
            if (rfidReader != null && rfidReader.IsConnected)
            {
                try
                {
                    RFIDResults rFIDResults = rfidReader.Config.WifiDeleteProfile(ssid);
                    if (rFIDResults == RFIDResults.RfidApiSuccess)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (InvalidUsageException e)
                {
                    e.PrintStackTrace();
                }
                catch (OperationFailureException e)
                {
                    if (e.Results == RFIDResults.AdminConnectError)
                    {
                         AuthenticaionDialog.ShowConnectionLostDialog();
                          
                    }
                    e.PrintStackTrace();
                }
            }
            return false;
        }

        public IList<WifiProfile> GetSavedWiFiProfiles()
        {
            List<string> ssidList = new List<string>();
            if (rfidReader != null && rfidReader.IsConnected)
            {
                try
                {
                    return rfidReader.Config.WifiListProfile();
                }
                catch (InvalidUsageException e)
                {
                    e.PrintStackTrace();
                }
                catch (OperationFailureException e)
                {
                    if (e.Results == RFIDResults.AdminConnectError)
                    {
                         AuthenticaionDialog.ShowConnectionLostDialog();
                          
                    }
                    e.PrintStackTrace();
                }
            }
            return null;
        }

        public RFIDResults ScanWiFi()
        {
            RFIDResults rFIDResults = null;
            if (rfidReader != null && rfidReader.IsConnected)
            {
                try
                {
                     rFIDResults = rfidReader.Config.WifiScan();
                    if (rFIDResults == RFIDResults.RfidApiSuccess)
                    {
                      //  return true;
                    }
                    else
                    {
                       // return false;
                    }

                }
                catch (InvalidUsageException e)
                {
                    e.PrintStackTrace();
                }
                catch (OperationFailureException e)
                {
                    if (e.Results == RFIDResults.AdminConnectError)
                    {
                        rFIDResults = e.Results;
                        AuthenticaionDialog.ShowConnectionLostDialog();

                    }
                    e.PrintStackTrace();
                }
            }
            return rFIDResults;
        }

        public string GetWiFiStatus()
        {
            if (rfidReader != null && rfidReader.IsConnected)
            {
               IDictionary<string, string>? rfidWifi = new Dictionary<string, string>();
                try
                {
                    RFIDResults rFIDResults = rfidReader.Config.WifiGetStatus(rfidWifi);
                    if (rFIDResults == RFIDResults.RfidApiSuccess)
                    {
                        if (rfidWifi.ContainsKey("wifi"))
                        {
                            if (rfidWifi["wifi"].Equals("DISABLE")) 
                            {
                                return "DISABLED";
                            }
                            else
                            {
                                return "ENABLED";
                            }
                        }
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (InvalidUsageException e)
                {
                    e.PrintStackTrace();
                }
                catch (OperationFailureException e)
                {
                    if (e.Results == RFIDResults.AdminConnectError)
                    {
                        AuthenticaionDialog.ShowConnectionLostDialog();
                         
                    }
                    e.PrintStackTrace();
                }

            }
            return "";
        }

        public bool WiFiConnect(string ssid)
        {
            if (rfidReader != null && rfidReader.IsConnected)
            {
                try
                {
                    RFIDResults rFIDResults = rfidReader.Config.WifiConnectNonRoaming(ssid);
                    if (rFIDResults == RFIDResults.RfidApiSuccess)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (InvalidUsageException e)
                {
                    e.PrintStackTrace();
                }
                catch (OperationFailureException e)
                {
                    if (e.Results == RFIDResults.AdminConnectError)
                    {
                        AuthenticaionDialog.ShowConnectionLostDialog();
                         
                    }
                    e.PrintStackTrace();
                }
            }
            return false;
        }

        public bool WiFiDisconnect()
        {
            if (rfidReader != null && rfidReader.IsConnected)
            {
                try
                {
                    RFIDResults rFIDResults = rfidReader.Config.WifiDisconnect();
                    if (rFIDResults == RFIDResults.RfidApiSuccess)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (InvalidUsageException e)
                {
                    e.PrintStackTrace();
                }
                catch (OperationFailureException e)
                {
                    if(e.Results == RFIDResults.AdminConnectError)
                    {
                        AuthenticaionDialog.ShowConnectionLostDialog();
                        
                    }
                    e.PrintStackTrace();
                }
            }
            return false;
        }

        public IList<string> WifiGetCertificates()
        {
            if (rfidReader != null && rfidReader.IsConnected)
            {
                try
                {
                    return rfidReader.Config.Certificates;


                }
                catch (InvalidUsageException e)
                {
                    e.PrintStackTrace();
                }
                catch (OperationFailureException e)
                {
                    if (e.Results == RFIDResults.AdminConnectError)
                    {
                        AuthenticaionDialog.ShowConnectionLostDialog();

                    }
                    e.PrintStackTrace();
                }
            }
            return null;
        }

        public bool enableWiFi()
        {
            if (rfidReader != null && rfidReader.IsConnected)
            {
                try
                {
                    RFIDResults rFIDResults = rfidReader.Config.WifiEnable();
                    if (rFIDResults == RFIDResults.RfidApiSuccess)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (InvalidUsageException e)
                {
                    e.PrintStackTrace();
                }
                catch (OperationFailureException e)
                {
                    if (e.Results == RFIDResults.AdminConnectError)
                    {
                        AuthenticaionDialog.ShowConnectionLostDialog();

                    }
                    e.PrintStackTrace();
                }
            }
            return false;
        }

        public void EventWifiScanNotify(RfidWifiScanEvents wifiScanEvent)
        {
            IEvents.WifiScanEventData data = wifiScanEvent.WifiScanEventData;
            ThreadPool.QueueUserWorkItem(o => WiFiScanResultsEvent?.Invoke(data.Wifiscandata));
        }

        private void SetupScannerSDK()
        {
            ScannerModel scannerModel = ScannerModel.scannerModel;
            scannerModel.setupSDKHandler(rfidReader.HostName, bluetoothEnabledWithPermission);
        }

        private void TerminateScannerSDK()
        {
            ScannerModel scannerModel = ScannerModel.scannerModel;
            scannerModel.DisconnectScanner(rfidReader?.HostName);
        }

        private void PrepareAndSetup()
        {
            if (_preferredTransport == null && !_isPromptingTransport)
            {
                _isPromptingTransport = true;
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    var transport = await PromptTransportAsync();
                    _preferredTransport = transport;
                    _isPromptingTransport = false;

                    if (transport == ENUM_TRANSPORT.Bluetooth && !bluetoothEnabledWithPermission)
                    {
                        ShowAlert("Bluetooth not available. Please enable Bluetooth or choose another transport.");
                    }

                    Setup();
                });
            }
            else
            {
                Setup();
            }
        }

        private async Task<ENUM_TRANSPORT> PromptTransportAsync()
        {
            var page = Application.Current?.MainPage;
            if (page == null)
            {
                System.Diagnostics.Debug.WriteLine("MainPage is null, defaulting to ReSerial transport.");
                return ENUM_TRANSPORT.ReSerial;
            }

            string cancel = "Cancel";
            string[] options =
            {
                "ReSerial - TC53e-RFID, EM45, ET6x",
                "ServiceUsb - RFD40/90, TC22R",
                "ServiceSerial - MC33xxR",
                "Bluetooth - RFD8500, RFD40/90",
                "ReUSB - FXP20",
                "QcSerial - TC201, TC501, TC701, ET401"
            };

            string choice = null;
            
            while (string.IsNullOrEmpty(choice))
            {
                System.Diagnostics.Debug.WriteLine("Prompting user for reader transport...");
                choice = await page.DisplayActionSheet("Select reader transport", null, null, options);
                System.Diagnostics.Debug.WriteLine($"User selected transport: {choice}");
            }

            return choice switch
            {
                "Bluetooth - RFD8500, RFD40/90" => ENUM_TRANSPORT.Bluetooth,
                "ServiceSerial - MC33xxR" => ENUM_TRANSPORT.ServiceSerial,
                "ServiceUsb - RFD40/90, TC22R" => ENUM_TRANSPORT.ServiceUsb,
                "ReUSB - FXP20" => ENUM_TRANSPORT.ReUsb,
                "ReSerial - TC53e-RFID, EM45, ET6x" => ENUM_TRANSPORT.ReSerial,
                "QcSerial - TC201, TC501, TC701, ET401" => ENUM_TRANSPORT.QcSerial,
                _ => _preferredTransport ?? ENUM_TRANSPORT.ReSerial 
            };
        }

    }
}
