using Android.Widget;
using AndroidX.Emoji2.Text.FlatBuffer;
using Com.Zebra.Rfid.Api3;
using Java.Lang;
using MauiRfidSample.MVVM.Models;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace MauiRfidSample.MVVM.ViewModels
{
    public class Gen2XTagProtectionViewModel : BaseViewModel
    {
        private static ReaderModel rfid = ReaderModel.readerModel;
        private string _accessData;
        private string _TagPattern;
        private string _Password;
        private string _opMode;


        public Gen2XTagProtectionViewModel()
        {
            TagPattern = InventoryListModel.SelectedItem?.ToString() ?? "";

            if(rfidModel.rfidReader != null && rfidModel.rfidReader.IsConnected)
            {
                if(rfidModel.rfidReader.Transport.ToString().Equals( ENUM_TRANSPORT.QcSerial.ToString()))
                {
                    ShowAlert("Feature not supported for this device.");
                    return;
                }
            }     
        }
        
        public string TagPattern
        {
            get { return _TagPattern; }
            set { _TagPattern = value; OnPropertyChanged(); }
        }

        public string AccessPW
        {
            get { return _Password; }
            set { _Password = value; OnPropertyChanged(); }
        }


        public string OpMode
        {
            get { return _opMode; }
            set { _opMode = value; OnPropertyChanged(); }
        }

        public void PerformOpClicked()
        {
            switch (OpMode)
            {
                //x: String > Protect </ x:String >
                //                    < x:String > Unprotect </ x:String >
                //                    < x:String > GetPassword </ x:String >
                //                    < x:String > SetPassword </ x:String >
                //                    < x:String > Enable Inventory Of Protected Tags</ x:String >
                //                    < x:String > Clear Protected Mode Configuration </ x:String >
                case "Protect":
                    TagProtection();
                    break;

                case "Unprotect":
                    UnProtect();
                    break ;

                case "GetPassword":
                    GetPassword();
                    break;

                case "SetPassword":
                    SetPassword();
                    break;

                case "Enable Inventory Of Protected Tags":
                    EnableVisibility();
                    break;

                case "Clear Protected Mode Configuration":
                    DisableVisibility();
                    break;
            }
        }

        public void DeleteAllClicked()
        {
            if (rfidModel.rfidReader == null)
            {
                ShowAlert("Reader not connected");
                return;
            }
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    rfidModel.rfidReader.Actions.PreFilters.DeleteAll();
                    ShowAlert("Delete all prefilters success");
                }
                catch (InvalidUsageException e)
                {
                    Debug.WriteLine("InvalidUsage exception " + e.Message);
                }
                catch (OperationFailureException e)
                {
                    Debug.WriteLine("OperationFailure Exception " + e.VendorMessage);
                }

            });
        }

        private void TagProtection()
        {
            string TagId = TagPattern;
            string pin = AccessPW;
            if (rfidModel.rfidReader == null)
            {
                ShowAlert("Reader not connected");
                return;
            }
            if (string.IsNullOrEmpty(pin) || string.IsNullOrEmpty(TagId)) { 
                ShowAlert("TagID or Password cannot be empty");
                return;
            }
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    if (rfidModel.rfidReader != null)
                    {
                        //rfidModel.rfidReader.Actions.PreFilters.DeleteAll();
                        rfidModel.impinjExtensions.EnableTagProtection(TagId, pin, null);
                        ShowAlert("Tag Protection success");
                    }
                }
                catch (InvalidUsageException e)
                {
                    Debug.WriteLine("InvalidUsage exception " + e.Message);
                    ShowAlert(e);
                }
                catch (OperationFailureException e)
                {
                    ShowAlert(e);
                    Debug.WriteLine("OperationFailure Exception " + e.VendorMessage);
                }

            });
            
        }

        private void UnProtect()
        {
            string TagId = TagPattern; string pin = AccessPW;
            if (string.IsNullOrEmpty(pin) || string.IsNullOrEmpty(TagId)) {
                ShowAlert("TagID or Password cannot be empty");
                return;
            }
            ThreadPool.QueueUserWorkItem(o =>
            {

                try
                {
                    if (rfidModel.rfidReader != null)
                    {
                        rfidModel.rfidReader.Actions.PreFilters.DeleteAll();

                        rfidModel.impinjExtensions.DisableTagProtection(TagId, pin, null, 1);
                        ShowAlert("UnProtect success");
                    }

                }
                catch (InvalidUsageException e)
                {
                    ShowAlert(e);
                    Debug.WriteLine("InvalidUsage exception " + e.Message);
                }
                catch (OperationFailureException e)
                {
                    ShowAlert(e);
                    Debug.WriteLine("OperationFailure Exception " + e.VendorMessage);
                }
            });
           
        }

        private void EnableVisibility()
        {
            string TagId = TagPattern; string pin = AccessPW;
            if (rfidModel.rfidReader == null)
            { 
                ShowAlert("Reader not connected");
                return;
            }
                if (string.IsNullOrEmpty(pin))
            {
                ShowAlert("Pin/Password cannot be empty");
                return;
            }
            try
            {
                rfidModel.rfidReader.Actions.PreFilters.DeleteAll();
                rfidModel.impinjExtensions.EnableTagVisibility( pin, 1);
                ShowAlert("Enable Visibility success");
            }
            catch (InvalidUsageException e)
            {
                ShowAlert(e);
                Debug.WriteLine("InvalidUsage exception " + e.Message);
            }
            catch (OperationFailureException e)
            {
                ShowAlert(e);
                Debug.WriteLine("OperationFailure Exception " + e.VendorMessage);
            }
        }

        private void DisableVisibility()
        {
            string TagId = TagPattern; string pin = AccessPW;
            if (rfidModel.rfidReader == null)
            {
                ShowAlert("Reader not connected");
                return;
            }
            if (string.IsNullOrEmpty(pin))
            {
                ShowAlert("Pin/Password cannot be empty");
                return;
            }
            try
            {
                rfidModel.impinjExtensions.DisableTagVisibility( pin, 1);
                ShowAlert("Disable Visibility success");
            }
            catch (InvalidUsageException e)
            {
                ShowAlert(e);
                Debug.WriteLine("InvalidUsage exception " + e.Message);
            }
            catch (OperationFailureException e)
            {
                ShowAlert(e);
                Debug.WriteLine("OperationFailure Exception " + e.VendorMessage);
            }
        }

        private void GetPassword()
        {
            string TagId = TagPattern; string pin = AccessPW;
            if (rfidModel.rfidReader == null)
            {
                ShowAlert("Reader not connected");
                return;
            }
            if (string.IsNullOrEmpty(TagId))
            {
                ShowAlert("TagID cannot be empty");
                return ;
            }

            TagAccess tagAccess = new TagAccess();
            TagAccess.ReadAccessParams readAccessParams = new TagAccess.ReadAccessParams(tagAccess);

            readAccessParams.AccessPassword = 0x00000000;
            readAccessParams.Count = 2;
            readAccessParams.MemoryBank = MEMORY_BANK.MemoryBankReserved;
            readAccessParams.Offset = 2;

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    TagData tagData = rfid.rfidReader.Actions.TagAccess.ReadWait(TagId, readAccessParams, null, true);
                    AccessPW = tagData.MemoryBankData?.ToString();
                    ShowAlert(tagData.OpStatus.ToString());
                }
                catch (InvalidUsageException e)
                {
                    e.PrintStackTrace();
                    ShowAlert(e);
                }
                catch (OperationFailureException e)
                {
                    e.PrintStackTrace();
                    ShowAlert(e);
                }
            });

        }

        private void SetPassword()
        {
            string TagId = TagPattern; string pin = AccessPW;
            if (rfidModel.rfidReader == null)
            {
                ShowAlert("Reader not connected");
                return;
            }
            if (string.IsNullOrEmpty(pin) || string.IsNullOrEmpty(TagId))
            {
                ShowAlert("TagID or Password cannot be empty");
                return;
            }
            try
            {

                TagAccess tagAccess = new TagAccess();
                TagAccess.WriteAccessParams writeAccessParams = new TagAccess.WriteAccessParams(tagAccess);
                writeAccessParams.AccessPassword =0x00000000;
                writeAccessParams.MemoryBank = MEMORY_BANK.MemoryBankReserved;
                writeAccessParams.Offset = 2;
                writeAccessParams.SetWriteData(pin);
                writeAccessParams.WriteDataLength = 2;
                TagData writeTagData = new TagData();
                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        rfidModel.rfidReader.Actions.PreFilters.DeleteAll();
                        rfidModel.rfidReader.Actions.TagAccess.WriteWait(TagId, writeAccessParams, null, writeTagData, true, false);
                        if(writeTagData != null)
                        {
                            ShowAlert("Result : " + writeTagData.OpStatus);
                        }
                            
                    }
                    catch (InvalidUsageException e)
                    {
                        e.PrintStackTrace();
                        ShowAlert(e);
                    }
                    catch (OperationFailureException e)
                    {
                        e.PrintStackTrace();
                        ShowAlert(e);
                    }
                });

            }
            catch (InvalidUsageException e)
            {
                ShowAlert(e);
                Debug.WriteLine("InvalidUsage exception " + e.Message);
            }
            catch (OperationFailureException e)
            {
                ShowAlert(e);
                Debug.WriteLine("OperationFailure Exception " + e.VendorMessage);
            }
        }

       

        private void ShowAlert(string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Toast.MakeText(Android.App.Application.Context, message, ToastLength.Short).Show();
            });
        }


        private void ShowAlert(OperationFailureException e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Toast.MakeText(Android.App.Application.Context, e.VendorMessage, ToastLength.Short).Show();
            });
        }

        private void ShowAlert(InvalidUsageException e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Toast.MakeText(Android.App.Application.Context, e.Info, ToastLength.Short).Show();
            });
        }

        public void displayDialog()
        {
            string UserGuideText = "To auto-fill the tag ID, perform an inventory on the inventory page and select a tag.\n" +
                "\r•Protect: Requires TagID and Password/PIN\n" +
                "\r•UnProtect: Requires TagID and Password/PIN\n" +
                "\r•GetPassword: Requires TagID (reads current access password)\n" +
                "\r•SetPassword: Requires TagID and new Password/PIN\n" +
                "\r•Enable Inventory Of Protected Tags: Requires Password/PIN (makes protected tags visible)\n" +
                "\r•To check whether protected tag is visible, do the inventory in inventory page not in GenxFeatures page\n"+

            "\r•Clear Protected Mode Configuration: Requires Password/PIN (protected tags will NOT be inventoried afterward)\n" +
                "\r•Delete all will delete all pre-filters before performing the operation.\n" +
                "\nNote: Clearing protected mode stops the reader from reporting protected tags until protection is re-enabled.";


            MainThread.BeginInvokeOnMainThread(async () => {
                await Application.Current.MainPage.DisplayAlert(
                    "Info :",
                    UserGuideText,
                    "OK");
            });
        }


    }
}
