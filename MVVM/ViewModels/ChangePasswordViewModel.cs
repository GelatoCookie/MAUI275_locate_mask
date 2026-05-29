using Android.Net;
using Android.Widget;
using Android.Widget;
using AndroidX.Lifecycle;
using Com.Zebra.Rfid.Api3;
using ICSharpCode.SharpZipLib.Core;
using Java.Net;
using MauiRfidSample.MVVM.Models;
using Microsoft.Maui.Storage;
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
using System.Windows.Input;
using static AndroidX.Navigation.NavType;

namespace MauiRfidSample.MVVM.ViewModels
{
    public class ChangePasswordViewModel : BaseViewModel
    {
        private string _oldPassword;
        private string _newPassword;
        private string _confirmPassword;

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public string OldPassword
        {
            get => _oldPassword;
            set
            {
                _oldPassword = value;
                OnPropertyChanged();
            }
        }

        public string NewPassword
        {
            get => _newPassword;
            set
            {
                _newPassword = value;
                OnPropertyChanged();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
            }
        }

        public ICommand ChangePasswordCommand { get; }

        public ChangePasswordViewModel()
        {

            ChangePasswordCommand = new Command(async () => await ExecuteChangePasswordAsync());
        }

        private async Task ExecuteChangePasswordAsync()
        {
            RFIDResults rFIDResults = null;

            if (string.IsNullOrEmpty(OldPassword) || string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmPassword))
            {
                ShowAlert("All fields are required");
                return;
            }

  
            IsBusy = true;
            
            try
            {
                if (rfidModel.rfidReader != null && rfidModel.rfidReader.IsConnected)
                {
                    rFIDResults = rfidModel.rfidReader.Config.AdminSetPassword(OldPassword, NewPassword, ConfirmPassword);
                }
                if (rFIDResults != null && rFIDResults == RFIDResults.RfidApiSuccess)
                {
                    ShowAlert("Password set successfully. Please Login with new password.");
                    await Shell.Current.GoToAsync("..");
                }

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
                    System.Diagnostics.Debug.WriteLine($"SetPassword failed: {ofex.VendorMessage}");
                }
                else if (ofex.Results == RFIDResults.PasswordCriteriaMismatchError)
                {
                    ShowAlert($"{ofex.Results + " " + ofex.VendorMessage}");
                    System.Diagnostics.Debug.WriteLine($"SetPassword failed: {ofex.VendorMessage}");
                }
                else
                {
                    ShowAlert($"Login failed: {ofex.VendorMessage}");
                    System.Diagnostics.Debug.WriteLine($"Login failed: {ofex.VendorMessage}");
                }
            }
            catch (Exception ex)
            {
                ShowAlert($"Error changing password: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Password change error: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ClearFields()
        {
            OldPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
        }

        public void ShowAlert(string message)
        {
            MainThread.BeginInvokeOnMainThread(async() =>
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