using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiRfidSample.MVVM.Services
{
    internal class AuthenticaionDialog
    {
        private static bool _isDialogDisplayed = false;

        public static async Task ShowErrorDialog(string title, string message)
        {
            // Check if a dialog is already open
            if (_isDialogDisplayed)
                return;

            try
            {
                _isDialogDisplayed = true;
                await Application.Current.MainPage.DisplayAlert(title, message, "OK");
            }
            finally
            {
                // Ensure the flag is reset even if an exception occurs
                _isDialogDisplayed = false;
            }
        }

        public static async Task ShowConnectionLostDialog()
        {

            if (_isDialogDisplayed)
                return;

            try
            {
                _isDialogDisplayed = true;
                await Application.Current.MainPage.DisplayAlert(
                    "Authentication Error",
                    "Device authentication required. Please go to Admin Login page and login as admin to continue.",
                    "OK");
            }
            finally
            {
                // Ensure the flag is reset even if an exception occurs
                _isDialogDisplayed = false;
            }
        }


    }
}
