using Android.Widget;
using Com.Zebra.Rfid.Api3;
using Java.Lang;
using MauiRfidSample.MVVM.Services;
using Microsoft.Maui.Storage;
using System.Windows.Input;

namespace MauiRfidSample.MVVM.ViewModels
{
    public class AdminLoginViewModel : BaseViewModel
    {
        private string _password;
        private bool _isAuthenticated;
        private bool _isBusy;

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            private set
            {
                _isAuthenticated = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand NavigateToChangePasswordCommand { get; }

        public AdminLoginViewModel()
        {
            LoginCommand = new Command(async () => await ExecuteLoginAsync());
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
            
        }

        private async Task ExecuteLoginAsync()
        {
            RFIDResults rFIDResults = null;
            if (string.IsNullOrEmpty(Password))
            {
                ShowAlert("Please enter a password");
                return;
            }
            if(Password.Contains(" "))
            {
                ShowAlert("Password shouldn't contain empty space");
                return;
            }

            IsBusy = true;

            try
            {
               
                //await Task.Delay(500);

                if (rfidModel.rfidReader != null && rfidModel.rfidReader.IsConnected)
                {
                  rFIDResults = rfidModel.rfidReader.Config.AdminLogin(Password);
                }
                if(rFIDResults != null && rFIDResults == RFIDResults.RfidApiSuccess)
                {
                    ShowAlert("Login successful");
                    await Shell.Current.GoToAsync("..");
                  
                }
               
            }
            catch(InvalidUsageException iuex)
            {
                ShowAlert("Invalid usage error : " + iuex.Message);
                System.Diagnostics.Debug.WriteLine("Invalid usage error : " + iuex.Message);
            }
            catch (OperationFailureException ofex)
            {
                if (ofex.Results == RFIDResults.DefaultPasswordError)
                {
                    ShowAlert($"{ofex.Results + " " + ofex.VendorMessage}");
                    System.Diagnostics.Debug.WriteLine("Default password error : " + ofex.VendorMessage);
                }
                else if (ofex.Results == RFIDResults.PasswordMismatchError)
                {
                   ShowAlert($"{ofex.Results + " " + ofex.VendorMessage}");
                    System.Diagnostics.Debug.WriteLine($"Login failed: {ofex.VendorMessage}");
                }
                else
                {
                    ShowAlert($"Login failed: {ofex.VendorMessage}");
                    System.Diagnostics.Debug.WriteLine($"Login failed: {ofex.VendorMessage}");
                }
            }
            
            finally
            {
                IsBusy = false;
            }
        }

        public void ShowAlert(string message)
        {            
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.DisplayAlert("Alert", message, "OK");
            });
        }

        public void ShowPasswordCriteria()
        {
            string criteria = "• Minimum length: 9 characters, maximum length: 31 characters\n" +
                              "• At least one uppercase letter\n" +
                              "• At least one lowercase letter\n" +
                              "• At least one number\n" +
                              "• At least one special character";

            MainThread.BeginInvokeOnMainThread(async () => {
                await Application.Current.MainPage.DisplayAlert(
                    "Password Information",
                    criteria,
                    "OK");
            });
        }
    }
}