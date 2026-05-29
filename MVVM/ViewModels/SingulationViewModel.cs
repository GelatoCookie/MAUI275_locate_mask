using Android.Widget;
using Com.Zebra.Rfid.Api3;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Diagnostics;

namespace MauiRfidSample.MVVM.ViewModels
{
     public class SingulationViewModel : BaseViewModel
     {
         public ObservableCollection<string> SessionOptions { get; } = new() { "S0", "S1", "S2", "S3" };
         public ObservableCollection<string> InventoryStateOptions { get; } = new() { "STATE_A", "STATE_B", "AB_FLIP" };
         public ObservableCollection<string> SlFlagOptions { get; } = new() { "ASSERTED", "DEASSERTED", "ALL" };

         private string _selectedSession = "S0";
         private string _selectedInventoryState = "STATE_A";
         private string _selectedSlFlag = "ALL";
         private string _tagPopulation = "32";
         private string _statusMessage;

         public string SelectedSession { get => _selectedSession; set { _selectedSession = value; OnPropertyChanged(); } }
         public string SelectedInventoryState { get => _selectedInventoryState; set { _selectedInventoryState = value; OnPropertyChanged(); } }
         public string SelectedSlFlag { get => _selectedSlFlag; set { _selectedSlFlag = value; OnPropertyChanged(); } }
         public string TagPopulation { get => _tagPopulation; set { _tagPopulation = value; OnPropertyChanged(); } }
         public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }

         public ICommand SaveCommand { get; }

         public SingulationViewModel()
         {
            SaveCommand = new Command(SaveSingulation);
         }

         public void LoadCurrentSettings()
         {
             if (rfidModel.rfidReader == null || !rfidModel.rfidReader.IsConnected)
             {
                 StatusMessage = "Reader not connected";
                 return;
             }
             try
             {
                 var sc = rfidModel.rfidReader.Config.Antennas.GetSingulationControl(1);
                 if (sc?.Session != null)
                 SelectedSession = MapSession(sc.Session);
                 if (sc?.Action?.InventoryState != null)
                 SelectedInventoryState = MapInventoryState(sc.Action.InventoryState);
                 if (sc?.Action?.SLFlag != null)
                 SelectedSlFlag = MapSlFlag(sc.Action.SLFlag);
                 TagPopulation = sc?.TagPopulation.ToString() ?? TagPopulation;
                 StatusMessage = "Loaded";
             }
             catch (Exception ex)
             {
                StatusMessage = "Load failed: " + ex.Message;
             }
         }

         private void SaveSingulation()
         {
             if (rfidModel.rfidReader == null || !rfidModel.rfidReader.IsConnected)
             {
                 StatusMessage = "Reader not connected";
                 return;
             }
             if (!short.TryParse(TagPopulation, out short pop) || pop <0)
             {
                 StatusMessage = "Invalid tag population";
                 return;
             }
             try
             {
                 var sc = rfidModel.rfidReader.Config.Antennas.GetSingulationControl(1);
                 sc.Session = ParseSession(SelectedSession);
                 sc.Action.InventoryState = ParseInventoryState(SelectedInventoryState);
                 sc.Action.SLFlag = ParseSlFlag(SelectedSlFlag);
                 sc.TagPopulation = pop;
                 rfidModel.rfidReader.Config.Antennas.SetSingulationControl(1, sc);
                 StatusMessage = "Saved";
             }
             catch (InvalidUsageException ie)
             {
                StatusMessage = "Invalid: " + ie.Info;
             }
             catch (OperationFailureException ofe)
             {
                StatusMessage = "Failed: " + ofe.VendorMessage;
             }
             catch (Exception ex)
             {
                 StatusMessage = "Error: " + ex.Message;
             }
         }

         private string MapSession(SESSION s) => s switch
         {
             var x when x.Equals(SESSION.SessionS0) => "S0",
             var x when x.Equals(SESSION.SessionS1) => "S1",
             var x when x.Equals(SESSION.SessionS2) => "S2",
             var x when x.Equals(SESSION.SessionS3) => "S3",
             _ => "S0"
         };
         private SESSION ParseSession(string s) => s switch
         {
             "S0" => SESSION.SessionS0,
             "S1" => SESSION.SessionS1,
             "S2" => SESSION.SessionS2,
             "S3" => SESSION.SessionS3,
             _ => SESSION.SessionS0
         };
         private string MapInventoryState(INVENTORY_STATE st) => st switch
         {
             var x when x.Equals(INVENTORY_STATE.InventoryStateA) => "STATE_A",
             var x when x.Equals(INVENTORY_STATE.InventoryStateB) => "STATE_B",
             var x when x.Equals(INVENTORY_STATE.InventoryStateAbFlip) => "AB_FLIP",
             _ => "STATE_A"
         };
         private INVENTORY_STATE ParseInventoryState(string s) => s switch
         {
             "STATE_A" => INVENTORY_STATE.InventoryStateA,
             "STATE_B" => INVENTORY_STATE.InventoryStateB,
             "AB_FLIP" => INVENTORY_STATE.InventoryStateAbFlip,
             _ => INVENTORY_STATE.InventoryStateA
         };
         private string MapSlFlag(SL_FLAG f) => f switch
         {
             var x when x.Equals(SL_FLAG.SlFlagAsserted) => "ASSERTED",
             var x when x.Equals(SL_FLAG.SlFlagDeasserted) => "DEASSERTED",
             var x when x.Equals(SL_FLAG.SlAll) => "ALL",
             _ => "ALL"
         };
         private SL_FLAG ParseSlFlag(string s) => s switch
         {
             "ASSERTED" => SL_FLAG.SlFlagAsserted,
             "DEASSERTED" => SL_FLAG.SlFlagDeasserted,
             "ALL" => SL_FLAG.SlAll,
             _ => SL_FLAG.SlAll
         };

        
        private void ShowAlert(string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Toast.MakeText(Android.App.Application.Context, message, ToastLength.Short).Show();
            });
        }

    }
}
