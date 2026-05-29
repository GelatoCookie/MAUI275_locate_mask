using Android.OS;
using Android.Widget;
using Com.Zebra.Rfid.Api3;
using ICSharpCode.SharpZipLib.Core;
using MauiRfidSample.MVVM.Models;
using System.Net;

namespace MauiRfidSample.MVVM.ViewModels
{
    class FirmwareUpdateViewModel : BaseViewModel
    { 
     
        private ScannerModel scannerModel;
        private string _connectedDevice, _scannerFWVersion, _mySelectedFile, _firmwareButton;

        public FirmwareUpdateViewModel()
        {
            scannerModel = ScannerModel.scannerModel;

            if (rfidModel.rfidReader != null && rfidModel.rfidReader.IsConnected)
            {
                if (rfidModel.rfidReader.Transport.ToString().Equals(ENUM_TRANSPORT.QcSerial.ToString()))
                {

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Toast.MakeText(Android.App.Application.Context, "Feature not supported for this device.", ToastLength.Short).Show();
                    });

                    return;
                }    
            }
            UpdateUI();
        }

        internal async void SelectUpdateFirmware()
        {
            if (MySelectedFile == null)
            {
                FileResult fileResult = await PickFirmwareFile();

                if (fileResult != null)
                {
                    byte[] buffer = new byte[4096];
                    var fileName = fileResult.FileName;
                    var stream = await fileResult.OpenReadAsync();
                    var cacheFilePath = Path.Combine(FileSystem.AppDataDirectory, "ZebraFirmware/", fileName);

                    if (File.Exists(cacheFilePath))
                    {
                        File.Delete(cacheFilePath);
                    }
                    string directoryName = Path.GetDirectoryName(cacheFilePath);

                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                    using (FileStream streamWriter = File.Create(cacheFilePath))
                    {
                        StreamUtils.Copy(stream, streamWriter, buffer);
                        if (File.Exists(cacheFilePath))
                        {
                            directoryName = Path.GetDirectoryName(cacheFilePath);
                            MySelectedFile = cacheFilePath;
                            FirmwareButton = "Update Firmware";
                        }
                    }
                }
            }
            else
            {
                string NGEDeviceName = "RFID" + Build.Model; 
                bool isDeviceRunsOnNGEProtocol = NGEDeviceName.Equals(rfidModel.rfidReader.HostName) || rfidModel.rfidReader.HostName.Contains("FXP20");
                System.Diagnostics.Debug.WriteLine("isDeviceRunsOnNGEProtocol : " + isDeviceRunsOnNGEProtocol + "  NGEDeviceNAme " + NGEDeviceName);
                System.Diagnostics.Debug.WriteLine("Ip : " + Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString());
                if (isDeviceRunsOnNGEProtocol)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        _ = UpdateNGEFirmware(MySelectedFile);
                    });


                    // (firmwarePath, Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString());
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        _ = scannerModel.UpdateScanner(MySelectedFile);
                    });
                }

            }
        }

        public async Task UpdateNGEFirmware(string selectedFile)
        {
            try
            {
                if (selectedFile != null)
                {
                    rfidModel.rfidReader.Config.UpdateFirmware(selectedFile, Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString());
                }
            }
            catch (Java.Lang.Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Scanner Error : " + e.ToString());
            }
        }


        async Task<FileResult> PickFirmwareFile()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select firmware file"
                });
                return result;
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public string MySelectedFile { get => _mySelectedFile; set { _mySelectedFile = value; OnPropertyChanged(); } }
        public string ConnectedDevice { get => _connectedDevice; set { _connectedDevice = value; OnPropertyChanged(); } }
        //public string CurrentProgress { get => _currentProgress; set { _currentProgress = value; OnPropertyChanged(); } }
        //public string CurrentPercentage { get => _currentPercentage; set { _currentPercentage = value; OnPropertyChanged(); } }

        private double _currentProgress;
        private string _currentPercentage;
        public double CurrentProgress
        {
            get => _currentProgress;
            set
            {
                if (Math.Abs(_currentProgress - value) > 0.0001) // Compare doubles with precision
                {
                    _currentProgress = value;
                    OnPropertyChanged(nameof(CurrentProgress));
                }
            }
        }

        // Property for Label
        public string CurrentPercentage
        {
            get => _currentPercentage;
            set
            {
                if (_currentPercentage != value)
                {
                    _currentPercentage = value;
                    OnPropertyChanged(nameof(CurrentPercentage));
                }
            }
        }
        public string ScannerFWVersion { get => _scannerFWVersion; set { _scannerFWVersion = value; OnPropertyChanged(); } }
        public string FirmwareButton { get => _firmwareButton; set { _firmwareButton = value; OnPropertyChanged(); } }

        public override void ScannerConnectionEvent(string deviceName)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                UpdateUI();
                if (deviceName.Equals(""))
                {
                    Toast.MakeText(Android.App.Application.Context, "Disconnected..!", ToastLength.Long).Show();
                    ScannerFWVersion = "";
                }
                else
                {
                    Toast.MakeText(Android.App.Application.Context, "Connected..!", ToastLength.Long).Show();
                }
            });
        }

        private void UpdateUI()
        {
            ConnectedDevice = rfidModel.rfidReader.IsConnected ? rfidModel.rfidReader.HostName + " Connected" : "Not Connected";
            ScannerFWVersion = scannerModel.getScannerFirmwareVersion(0);
            FirmwareButton = "Select Firmware";
            MySelectedFile = null;
        }

        private string fwUpdateStatus = string.Empty;
        private int fwUpdateProgress = 0;

        /// <summary>
        /// // This method is called to update the current progress of the firmware update. For EM45 and TC53E-RFID devices, it will update the progress of the firmware update from the RFID reader.
        /// </summary>
        /// <param name="fwEventData"></param>
        public override void CurrentProgressUpdate(IEvents.FirmwareUpdateEvent fwEventData)
        {

            if (!fwEventData.Status.Equals(fwUpdateStatus) || fwEventData.ImageDownloadProgress != fwUpdateProgress)
            {
                fwUpdateStatus = fwEventData.Status;
                fwUpdateProgress = fwEventData.ImageDownloadProgress;

                CurrentProgress = fwEventData.ImageDownloadProgress / 100.0;
                CurrentPercentage = $"{fwEventData.ImageDownloadProgress}%";

                if ("FWUpdate_START".Equals(fwEventData.Status) && fwEventData.ImageDownloadProgress == 0)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {

                        Toast.MakeText(Android.App.Application.Context, "Firmware update started", ToastLength.Short).Show();

                    });


                }
                else if ("FWUpdate_START".Equals(fwEventData.Status) && fwEventData.ImageDownloadProgress > 0 && fwEventData.ImageDownloadProgress < 100)
                {
                    System.Diagnostics.Debug.WriteLine($"Firmware update in progress: {fwEventData.ImageDownloadProgress}%");
                    //Device.BeginInvokeOnMainThread(() =>
                    //{

                    //    Toast.MakeText(Android.App.Application.Context, "Firmware updating....", ToastLength.Short).Show();

                    //});
                }
                else if ("FWUpdate_END".Equals(fwEventData.Status) && fwEventData.ImageDownloadProgress == 100)
                {
                    System.Diagnostics.Debug.WriteLine("Firmware update completed successfully.");
                    Device.BeginInvokeOnMainThread(() =>
                    {

                        Toast.MakeText(Android.App.Application.Context, "Firmware update ended. Please disconnect and connect.", ToastLength.Long).Show();

                    });
                }

            }
            else if (fwEventData.Status.StartsWith("Error:"))
            {
                Console.WriteLine($"Firmware update failed: {fwEventData.Status}");
                Device.BeginInvokeOnMainThread(() =>
                {

                    Toast.MakeText(Android.App.Application.Context, "Firmware update error.", ToastLength.Short).Show();

                });

            }
            else
            {
                Console.WriteLine($"Unexpected firmware update status: {fwEventData.Status}");
            }
        }

        /// <summary>
        ///  This method is called to update the current progress of the firmware update. For RFD40, RFD90, TC22R, TC27R, it will update the progress of the firmware update from the RFID reader.
        /// </summary>
        /// <param name="currentProgress"></param>
        public override void CurrentProgressUpdate(int currentProgress)
        {
            if (currentProgress == 100)
            {
                CurrentPercentage = currentProgress + "%";
            }
            else
            {
                CurrentProgress = currentProgress / 100.0;
                CurrentPercentage = $"{currentProgress}%";
                //CurrentProgress = currentProgress < 10 ? "0.0" + currentProgress : "0." + currentProgress;
                //CurrentPercentage = currentProgress + "%";
            }
        }

        public override void FWVersion(string scannerFWVersion)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                ScannerFWVersion = scannerFWVersion;
            });
        }

    }
}
