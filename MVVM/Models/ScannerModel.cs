using Android.Widget;
using Com.Zebra.Rfid.Api3;
using Com.Zebra.Scannercontrol;
using Java.Lang;
using MauiRfidSample.MVVM.Services;
using System.Runtime.CompilerServices;
using System.Xml;
using Exception = Java.Lang.Exception;

namespace MauiRfidSample.MVVM.Models
{
    class ScannerModel : Java.Lang.Object, IDcsSdkApiDelegate
    {
        private static ScannerModel _scannerModel;
        private int scannerId;

        //string inXml;
        private const string FIRMWARE_FOLDER_PATH = "ZebraFirmware/";
        private const string OUTPUT_FOLDER_PATH = "ZebraOutput/";
        private string filePathOfFirmware = "";
        private string outputFolderPath = "";
        private string[] filesInFirmware;
        private static SDKHandler sdkHandler;
        public static List<DCSScannerInfo> scannerList = new List<DCSScannerInfo>();
        private bool isConnected = false;
        private string deviceName, sFWVersion;
        private static ReaderModel rfid = ReaderModel.readerModel;

        internal delegate void ConnectionHandler(string deviceName);
        internal delegate void CurrentProgressUpdate(int currentProgress);
        internal delegate void ScannerFWVersion(string scannerFWVersion);
        internal delegate void BarcodeReadEvent(string barcode, string barcodeType);

        internal event ConnectionHandler ScannerConnectionEvent;
        internal event CurrentProgressUpdate CurrentProgress;
        internal event ScannerFWVersion FWVersion;
        internal event BarcodeReadEvent BarcodeEvent;

        public static ScannerModel scannerModel
        {
            get
            {
                if (_scannerModel == null)
                    _scannerModel = new ScannerModel();
                return _scannerModel;
            }
        }

        private ScannerModel()
        {

        }

        public bool IsConnected
        {
            get => isConnected;
        }

        public string DeviceName
        {
            get => deviceName;
        }

        public string getFWVersion
        {
            get => sFWVersion;
        }

        /// <summary>
        /// Setup the SDK handler
        /// </summary>

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void setupSDKHandler(string hostName, bool BTenabled)
        {
            if (sdkHandler == null)
            {
                sdkHandler = new SDKHandler(Android.App.Application.Context);
                //For cdc device
                DCSSDKDefs.DCSSDK_RESULT result = sdkHandler.DcssdkSetOperationalMode(DCSSDKDefs.DCSSDK_MODE.DcssdkOpmodeUsbCdc);

                if (result != DCSSDKDefs.DCSSDK_RESULT.DcssdkResultSuccess)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to set operational mode: " + result);
                }

                //For bluetooth device
                if (BTenabled)
                {
                    DCSSDKDefs.DCSSDK_RESULT btResult = sdkHandler.DcssdkSetOperationalMode(DCSSDKDefs.DCSSDK_MODE.DcssdkOpmodeBtNormal);
                }

                sdkHandler.DcssdkSetDelegate(this);

                int notifications_mask = 0;
                // We would like to subscribe to all scanner available/not-available events
                notifications_mask |= DCSSDKDefs.DCSSDK_EVENT.DcssdkEventScannerAppearance.Value | DCSSDKDefs.DCSSDK_EVENT.DcssdkEventScannerDisappearance.Value;


                // We would like to subscribe to all scanner connection events
                notifications_mask |= DCSSDKDefs.DCSSDK_EVENT.DcssdkEventBarcode.Value | DCSSDKDefs.DCSSDK_EVENT.DcssdkEventBarcode.Value | DCSSDKDefs.DCSSDK_EVENT.DcssdkEventSessionEstablishment.Value | DCSSDKDefs.DCSSDK_EVENT.DcssdkEventSessionTermination.Value;


                // We would like to subscribe to all barcode events
                // subscribe to events set in notification mask
                result =  sdkHandler.DcssdkSubsribeForEvents(notifications_mask);

                if (result != DCSSDKDefs.DCSSDK_RESULT.DcssdkResultSuccess)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to subscribe " + result);
                }


            }
            if (sdkHandler != null)
            {
                IList<DCSScannerInfo> availableScanners = sdkHandler.DcssdkGetAvailableScannersList();

                scannerList.Clear();
                if (availableScanners != null)
                {
                    foreach (DCSScannerInfo scanner in availableScanners)
                    {
                        System.Diagnostics.Debug.WriteLine("Scanner Name : " + scanner.ScannerName + " Scanner ID : " + scanner.ScannerID);
                        scannerList.Add(scanner);
                    }
                }
                /*  Device.BeginInvokeOnMainThread(() =>
                  {
                      ScannerList?.Invoke(scannerList);
                      if (GetFilePathOfFirmwareFile())
                      {
                          //if use dat file no needed to extract..
                          // ExtractTheFirmwareFile();

                      }
                  });*/
            }
            if (hostName != null)
            {
                System.Diagnostics.Debug.WriteLine("----------------------------------------Hostname " + hostName);

                bool isTC22R = hostName.StartsWith("TC22R");
                bool isTC27R = hostName.StartsWith("TC27R");

                foreach (DCSScannerInfo device in scannerList)
                {
                    string scannerName = device.ScannerName;
                    System.Diagnostics.Debug.WriteLine("Scanner name " + scannerName);

                    if ((isTC22R && scannerName.StartsWith("TC220")) ||
                        (isTC27R && scannerName.StartsWith("TC275")) ||
                        (!isTC22R && !isTC27R && scannerName.Contains(hostName)))
                    {
                        ConnectScanner(device.ScannerID);
                    }
                }
            }
        }

        public string getScannerFirmwareVersion(int scannerid)
        {
            var fwVersion = "";
            var radioVersion = "";
            var deviceVersionInfo = new Android.Runtime.JavaDictionary<string, string>();

            // Null checks to prevent crash
            if (rfid == null || rfid.rfidReader == null || !rfid.rfidReader.IsConnected)
            {
                System.Diagnostics.Debug.WriteLine("RFID reader is not connected or not initialized.");
                sFWVersion = "";
                FWVersion?.Invoke("");
                return "";
            }

            try
            {
                rfid.rfidReader.Config.GetDeviceVersionInfo(deviceVersionInfo);
            }
            catch (InvalidUsageException e)
            {
                e.PrintStackTrace();
            }
            catch (OperationFailureException e)
            {
                e.PrintStackTrace();
            }
            if (deviceVersionInfo.ContainsKey("NGE"))
            {
                radioVersion = deviceVersionInfo["NGE"]; //NGE
                fwVersion = deviceVersionInfo["NGE"];
            }
            if (deviceVersionInfo.ContainsKey("CRIMAN_DEVICE"))
            {
                fwVersion = deviceVersionInfo["CRIMAN_DEVICE"];
            }
            sFWVersion = fwVersion;
            FWVersion?.Invoke(fwVersion);
            return fwVersion;
        }

        private string getSingleStringValue(StringBuilder outXML)
        {
            try
            {
                XmlReader reader = XmlReader.Create(new StringReader((string)outXML));
                reader.ReadToFollowing("value");
                return reader.ReadElementContentAsString().Trim();
            }
            catch (XmlException e)
            {
                System.Console.WriteLine(e);
            }
            return "";
        }

        /// <summary>
        /// Connect to the scanner
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ConnectScanner(int scannerID)
        {
            try
            {
               DCSSDKDefs.DCSSDK_RESULT dCSSDK_RESULT= sdkHandler.DcssdkEstablishCommunicationSession(scannerID);
                System.Diagnostics.Debug.WriteLine("Scanner communication result  : " + dCSSDK_RESULT);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Scanner Error : " + e.ToString());
            }
            _ = getScannerFirmwareVersion(scannerID);
        }

        /// <summary>
        /// Disonnect to the scanner
        /// </summary>
        public void DisconnectScanner(string hostName)
        {
            if (hostName != null)
            {
                foreach (DCSScannerInfo device in scannerList)
                {
                    if (device.ScannerName.Contains(hostName))
                    {
                        try
                        {
                            sdkHandler.DcssdkTerminateCommunicationSession(device.ScannerID);
                        }
                        catch (Exception e)
                        {
                            System.Diagnostics.Debug.WriteLine("Scanner Error : " + e.ToString());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update the firmware
        /// </summary>

        public async Task UpdateScanner(string selectedFile)
        {
            try
            {
                if (selectedFile != null)
                {
                    string inXml = "<inArgs><scannerID>" + scannerId + "</scannerID><cmdArgs><arg-string>" + selectedFile + "</arg-string></cmdArgs></inArgs>";
                    Task<bool> result = Task.Run(() => executeCommand(scannerId, DCSSDKDefs.DCSSDK_COMMAND_OPCODE.DcssdkUpdateFirmware, null, inXml));
                    if (await result)
                    {

                    }
                }
            }
            catch (Java.Lang.Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Scanner Error : " + e.ToString());
            }
        }

        public async Task getScannerBatchedData()
        {
            string inXML = "<inArgs><scannerID>" + scannerId + "</scannerID></inArgs>";
            Task<bool> result = Task.Run(() => executeCommand(scannerId, DCSSDKDefs.DCSSDK_COMMAND_OPCODE.DcssdkDeviceBatchRequest, null, inXML));
            if (await result)
            {

            }
        }

        /// <summary>
        /// Notification to inform that new Aux scanner has been appeared
        /// </summary>
        /// <param name="newTopology">Device tree that change has occurred</param>
        /// <param name="scanerInformation"Scanner Information></param>
        public void DcssdkEventAuxScannerAppeared(DCSScannerInfo newTopology, DCSScannerInfo scanerInformation)
        {
            System.Diagnostics.Debug.WriteLine("Scanner Aux data : " + scanerInformation.ScannerName);
        }

        /// <summary>
        /// The event responsible for capturing the barcode data.
        /// </summary>
        /// <param name="barcodeData">Barcode data</param>
        /// <param name="barcodeType">Barcode type of the scanned barcode. Values of bar code data types</param>
        /// <param name="scannerId">Unique identifier of a particular active scanner assigned by SDK</param>
        public void DcssdkEventBarcode(byte[] barcodeData, int barcodeType, int scannerId)
        {
            BarcodeEvent?.Invoke(System.Text.Encoding.Default.GetString(barcodeData), BarcodeTypes.getBarcodeTypeName(barcodeType));
            System.Diagnostics.Debug.WriteLine("Scanner Barcode data : " + barcodeData);
        }

        /// <summary>
        /// The event responsible for capturing the binary data.
        /// </summary>
        /// <param name="barcodeData">Object representing raw data of the received Intelligent Document Capture(IDC) data.</param>
        /// <param name="scannerId">Unique identifier of a particular active scanner assigned by SDK</param>
        public void DcssdkEventBinaryData(byte[] barcodeData, int scannerId)
        {
            System.Diagnostics.Debug.WriteLine("Scanner Barcode data : " + barcodeData);
        }

        /// <summary>
        /// The event responsible for capturing the scanner connection
        /// </summary>
        /// <param name="scannerInfo">Object representing an appeared active scanner.</param>
        public void DcssdkEventCommunicationSessionEstablished(DCSScannerInfo scannerInfo)
        {
            isConnected = true;
            deviceName = scannerInfo.ScannerName;
            scannerId = scannerInfo.ScannerID;
            ScannerConnectionEvent?.Invoke(scannerInfo.ScannerName);
        }

        /// <summary>
        /// "Session Terminated" notification informs about disappearance of a particular active scanner
        /// </summary>
        /// <param name="connectedScanner">Unique identifier of a disappeared active scanner assigned by SDK</param>
        public void DcssdkEventCommunicationSessionTerminated(int connectedScanner)
        {
            isConnected = false;
            deviceName = "";
            sFWVersion = "";
            scannerId = connectedScanner;
            ScannerConnectionEvent?.Invoke("");
        }

        /// <summary>
        /// The event responsible for capturing the firmware update data.
        /// </summary>
        /// <param name="firmwareUpdateEvent">Firmware update information</param>
        public void DcssdkEventFirmwareUpdate(FirmwareUpdateEvent firmwareUpdateEvent)
        {

            if (firmwareUpdateEvent.EventType == DCSSDKDefs.DCSSDK_FU_EVENT_TYPE.ScannerUfDlStart)
            {
                rfid.rfidReader.SetFirmwareUpdateInProgress(true);
                System.Diagnostics.Debug.WriteLine("ScannerControl Update Firmware Session Started!  ");
            }

            if (firmwareUpdateEvent.EventType == DCSSDKDefs.DCSSDK_FU_EVENT_TYPE.ScannerUfDlProgress)
            {
                if (firmwareUpdateEvent.CurrentRecord % 100 == 0)
                {
                    CurrentProgress?.Invoke(firmwareUpdateEvent.CurrentRecord * 100 / firmwareUpdateEvent.MaxRecords);
                }
            }
            if (firmwareUpdateEvent.EventType == DCSSDKDefs.DCSSDK_FU_EVENT_TYPE.ScannerUfSessEnd)
            {
               
              
                CurrentProgress?.Invoke(100);
                try
                {
                    System.Diagnostics.Debug.WriteLine("Scanner update firmware Session end event received");
                   Device.BeginInvokeOnMainThread(() =>
                    {

                        Toast.MakeText(Android.App.Application.Context, "Firmware update ended. Reconnect after 30 seconds.", ToastLength.Long).Show();

                    });
                    
                }
                catch (InterruptedException e)
                {
                    e.PrintStackTrace();
                }
               
              
                Device.BeginInvokeOnMainThread(() =>
                {
                    Task.Run(async () =>
                    {
                        //for (int i = 1; i <= 5; i++)
                        //{
                        //    System.Diagnostics.Debug.WriteLine($"MainThread Waiting... {i} seconds");

                        //}
                        await Task.Delay(1000);
                        System.Diagnostics.Debug.WriteLine("ScannerControl Firmware update ended. Rebooting device...");
                        rfid.rfidReader.SetFirmwareUpdateInProgress(false);
                        await startNewFirmware();
                    });
                });
            }
            if (firmwareUpdateEvent.EventType == DCSSDKDefs.DCSSDK_FU_EVENT_TYPE.ScannerUfStatus)
            {
                if(firmwareUpdateEvent.Status == DCSSDKDefs.DCSSDK_RESULT.DcssdkResultFirmwareUpdateFailedLlInternalError)
                {
                    System.Diagnostics.Debug.WriteLine("ScannerControl Update Firmware Admin Authentication required!  " + firmwareUpdateEvent.Status);
                    Device.BeginInvokeOnMainThread(() =>
                    {            
                        AuthenticaionDialog.ShowConnectionLostDialog();
                        Shell.Current.GoToAsync("..");
                    });
                    
                }
                if(firmwareUpdateEvent.Status == DCSSDKDefs.DCSSDK_RESULT.DcssdkResultFailure)
                {
                    rfid.rfidReader.SetFirmwareUpdateInProgress(false);
                    Device.BeginInvokeOnMainThread(() =>
                    {

                        Toast.MakeText(Android.App.Application.Context, "Firmware Update Failed.", ToastLength.Long).Show();

                    });
                    System.Diagnostics.Debug.WriteLine("ScannerControl Update Firmware  Error!  " + firmwareUpdateEvent.Status);
                }

            }

        }

        /// <summary>
        /// Start the new firmware
        /// </summary>
        private async Task startNewFirmware()
        {

            string inXml = "<inArgs><scannerID>" + scannerId + "</scannerID></inArgs>";
            StringBuilder outXml = new StringBuilder();

            //for (int i = 1; i <= 30; i++)
            //{
            //    System.Diagnostics.Debug.WriteLine($"Waiting before calling start new firmware... {i} seconds");
               
            //}
            await Task.Delay(1000);
            System.Diagnostics.Debug.WriteLine("ScannerControl Staring new firmware  ");
            Task<bool> result = Task.Run(() => executeCommand(scannerId, DCSSDKDefs.DCSSDK_COMMAND_OPCODE.DcssdkStartNewFirmware, null, inXml));
            if (await result)
            {

            }
        }

        /// <summary>
        /// The event responsible for capturing the Image data.
        /// </summary>
        /// <param name="imageData">Object representing raw data of the received image.</param>
        /// <param name="scannerId">Unique identifier of a particular active scanner assigned by SDK.</param>
        public void DcssdkEventImage(byte[] imageData, int scannerId)
        {
            System.Diagnostics.Debug.WriteLine("Scanner event image data  : " + imageData);
        }

        /// <summary>
        /// Device Arrival" notification informs about appearance of a particular available scanner
        /// </summary>
        /// <param name="scannerInfo">Object representing an appeared available scanner.</param>
        public void DcssdkEventScannerAppeared(DCSScannerInfo scannerInfo)
        {
            System.Diagnostics.Debug.WriteLine("Scanner appeared : " + scannerInfo.ScannerName);
        }

        /// <summary>
        /// The event responsible for capturing the scanner disappearing.
        /// </summary>
        /// <param name="scannerId">Unique identifier of a disappeared available scanner assigned by SDK.</param>
        public void DcssdkEventScannerDisappeared(int scannerId)
        {
            isConnected = false;
            deviceName = "";
            sFWVersion = "";
            ScannerConnectionEvent?.Invoke("");
        }

        /// <summary>
        ///  The event responsible for handling the video data.
        /// </summary>
        /// <param name="videoFrame">Object representing raw data of the received video frame</param>
        /// <param name="scannerId">Unique identifier of a particular active scanner assigned by SDK.</param>
        public void DcssdkEventVideo(byte[] videoFrame, int scannerId)
        {
            System.Diagnostics.Debug.WriteLine("Scanner video event : " + videoFrame);
        }

        private async Task<bool> executeCommand(int scannerId, DCSSDKDefs.DCSSDK_COMMAND_OPCODE opCode, StringBuilder outXml, string inXml)
        {
            if (sdkHandler != null)
            {
                if (outXml == null)
                {
                    outXml = new StringBuilder();
                }
                Task<DCSSDKDefs.DCSSDK_RESULT> result = Task.Run(() => sdkHandler.DcssdkExecuteCommandOpCodeInXMLForScanner(opCode, inXml, outXml, scannerId));
                if (await result == DCSSDKDefs.DCSSDK_RESULT.DcssdkResultSuccess)
                    return true;
                else if (await result == DCSSDKDefs.DCSSDK_RESULT.DcssdkResultFailure)
                    return false;
            }
            return false;
        }
    }
}
