using Android.Net;
using Android.Widget;
using AndroidX.Lifecycle;
using Com.Zebra.Rfid.Api3;
using ICSharpCode.SharpZipLib.Core;
using MauiRfidSample.MVVM.Models;
using MauiRfidSample.MVVM.Services;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MauiRfidSample.MVVM.ViewModels
{


    public class CertificatesViewModel : BaseViewModel
    {
        private ObservableCollection<string> _certificates;

        public ObservableCollection<string> Certificates
        {
            get => _certificates;
            set
            {
                _certificates = value;
                OnPropertyChanged();
            }
        }

        public ICommand RemoveCommand { get; }

        //public ObservableCollection<CertificateModel> Certificates
        //{
        //    get => _certificates;
        //    set
        //    {
        //        _certificates = value;
        //        OnPropertyChanged();
        //    }
        //}

        public CertificatesViewModel()
        {
            Certificates = new ObservableCollection<string>();

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

            //RemoveCommand = new Command<CertificateModel>(RemoveCertificate);
        }



        public void RemoveAllCertificate()
        {
            try
            {
                if (rfidModel.rfidReader != null && rfidModel.rfidReader.IsConnected)
                {
                    var certList = rfidModel.rfidReader.Config.RemoveAllCertificates(); // API call
                }
                LoadCertificatesAsync();
            }
            catch(InvalidUsageException iuex)
            {
                System.Diagnostics.Debug.WriteLine("InvalidUsageException in Remove all Certificates: " + iuex.Message);
            }
            catch(OperationFailureException ofex)
            {
                System.Diagnostics.Debug.WriteLine("OperationFailureException in Remove all Certificates: " + ofex.VendorMessage);
                if (ofex.Results == RFIDResults.AdminConnectError)
                {
                     AuthenticaionDialog.ShowConnectionLostDialog();
                     Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in Remove all Certificates");
            }
        }

        public void SaveAllCertificate()
        {
            try
            {
                if (rfidModel.rfidReader != null && rfidModel.rfidReader.IsConnected)
                {
                    var results = rfidModel.rfidReader.Config.SaveCertificates(); // API call

                    if(results == RFIDResults.RfidApiSuccess)
                    {
                        ShowAlert("Saved successfully");
                    }
                }
               // LoadCertificatesAsync();
            }
            catch (InvalidUsageException iuex)
            {
                System.Diagnostics.Debug.WriteLine("InvalidUsageException in Remove all Certificates: " + iuex.Message);
            }
            catch (OperationFailureException ofex)
            {
                System.Diagnostics.Debug.WriteLine("OperationFailureException in Remove all Certificates: " + ofex.VendorMessage);
                if (ofex.Results == RFIDResults.AdminConnectError)
                {
                    AuthenticaionDialog.ShowConnectionLostDialog();
                    Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in Remove all Certificates");
            }
        }


        public void RemoveCertificate(string certificate)
        {
            try
            {
                if (rfidModel.rfidReader != null && rfidModel.rfidReader.IsConnected)
                {
                    var certList = rfidModel.rfidReader.Config.RemoveCertificate(certificate); // API call
                }
                LoadCertificatesAsync();
            }
            catch (InvalidUsageException iuex)
            {
                System.Diagnostics.Debug.WriteLine("InvalidUsageException in Remove Certificates: " + iuex.Message);
            }
            catch (OperationFailureException ofex)
            {
                System.Diagnostics.Debug.WriteLine("OperationFailureException in Remove Certificates: " + ofex.Message);
                if (ofex.Results == RFIDResults.AdminConnectError)
                {
                     AuthenticaionDialog.ShowConnectionLostDialog();
                     Shell.Current.GoToAsync("..");
                }
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in Remove Certificates");
            }
        }

        public void ClearCertificates()
        {
            Certificates.Clear();
        }


        public async Task LoadCertificatesAsync()
        {
            try
            {
                            
                if (rfidModel.rfidReader != null && rfidModel.rfidReader.IsConnected)
                {
                    var certList =  rfidModel.rfidReader.Config.Certificates ; // API call
                    

                    Certificates.Clear();
                    if (certList != null)
                    {
                        foreach (var cert in certList)
                        {
                            Certificates.Add(cert);
                            System.Diagnostics.Debug.WriteLine("Certificates : " + cert);
                        }
                      
                    }
                   
                }
                
              
            }
            catch (InvalidUsageException iuex)
            {
                System.Diagnostics.Debug.WriteLine("InvalidUsageException in GetCertificates: " + iuex.Message);
            }
            catch (OperationFailureException ofex)
            {
                System.Diagnostics.Debug.WriteLine("OperationFailureException in GetCertificates: " + ofex.VendorMessage);
                if (ofex.Results == RFIDResults.AdminConnectError)
                {
                    await AuthenticaionDialog.ShowConnectionLostDialog();
                    await Shell.Current.GoToAsync("..");
                }
            }   
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in GetCertificates");
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


    }
}       


