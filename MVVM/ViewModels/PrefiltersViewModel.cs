using Com.Zebra.Rfid.Api3;
using MauiRfidSample.MVVM.Models;
using static Com.Zebra.Rfid.Api3.PreFilters;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MauiRfidSample.MVVM.ViewModels
{
    public class PrefiltersViewModel : BaseViewModel
    {
        public ObservableCollection<string> Targets { get; } = new();
        public ObservableCollection<string> Actions { get; } = new();
        public ObservableCollection<string> MemoryBanks { get; } = new();

        private string _tagPattern = string.Empty;
        public string TagPattern { get => _tagPattern; set { _tagPattern = value; OnPropertyChanged(); } }

        private string _selectedTarget;
        public string SelectedTarget { get => _selectedTarget; set { _selectedTarget = value; OnPropertyChanged(); } }

        private string _selectedAction;
        public string SelectedAction { get => _selectedAction; set { _selectedAction = value; OnPropertyChanged(); } }

        private string _selectedMemoryBank;
        public string SelectedMemoryBank { get => _selectedMemoryBank; set { _selectedMemoryBank = value; OnPropertyChanged(); } }

        private int _pointer;
        public int Pointer { get => _pointer; set { _pointer = value; OnPropertyChanged(); } }

        private int _length = 96;
        public int Length { get => _length; set { _length = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }
        public ICommand DeleteAllCommand { get; }
        private string _status = string.Empty;
        public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }

        public PrefiltersViewModel()
        {
            // Populate with user-specified labels
            Actions.Add("INV A NOT INV B OR ASRT SL NOT DSRT SL");
            Actions.Add("INV A OR ASRT SL");
            Actions.Add("NOT INV B OR NOT DSRT SL");
            Actions.Add("INV A2BB2A NOT INV A OR NEG SL NOT ASRT SL");
            Actions.Add("INV B NOT INV A OR DSRT SL NOT ASRT SL");
            Actions.Add("INV B OR DSRT SL");
            Actions.Add("NOT INV A OR NOT ASRT SL");
            Actions.Add("NOT INV A2BB2A OR NOT NEG SL");

            Targets.Add("SL FLAG");
            Targets.Add("SESSION S0");
            Targets.Add("SESSION S1");
            Targets.Add("SESSION S2");
            Targets.Add("SESSION S3");

            MemoryBanks.Add("RESERVED");
            MemoryBanks.Add("EPC");
            MemoryBanks.Add("TID");
            MemoryBanks.Add("USER");

            
            TagPattern = InventoryListModel.SelectedItem?.ToString() ?? string.Empty;

            SaveCommand = new Command(_ => Save());
            DeleteAllCommand = new Command(_ => DeleteAll());
        }

        private void Save()
        {
            try
            {
                if (rfidModel.rfidReader == null)
                {
                    Status = "Reader not connected.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(TagPattern)
                    || string.IsNullOrWhiteSpace(SelectedTarget)
                    || string.IsNullOrWhiteSpace(SelectedAction)
                    || string.IsNullOrWhiteSpace(SelectedMemoryBank)
                    || Pointer < 0
                    || Length <= 0)
                {
                    Status = "Fill Tag Pattern, Target, Action, Memory Bank, Pointer, and Length.";
                    return;
                }

                var container = new PreFilters();
                var pf = new PreFilters.PreFilter(container)
                {
                    AntennaID = 1,
                    BitOffset = Pointer,
                    TagPatternBitCount = Length,
                    MemoryBank = MapMemoryBank(SelectedMemoryBank),
                    FilterAction = FILTER_ACTION.FilterActionStateAware
                };
                if (!string.IsNullOrWhiteSpace(TagPattern))
                    pf.SetTagPattern(TagPattern);

                pf.StateAwareAction.Target = MapTarget(SelectedTarget);
                pf.StateAwareAction.StateAwareAction = MapAction(SelectedAction);

                rfidModel.rfidReader.Actions.PreFilters.Add(new[] { pf }, null);
                Status = "Prefilter saved.";
            }
            catch (InvalidUsageException)
            {
                Status = "Invalid usage while saving prefilter.";
            }
            catch (OperationFailureException ofex)
            {
                Status = ofex.VendorMessage;
            }
        }

        private void DeleteAll()
        {
            try
            {
                rfidModel.rfidReader?.Actions?.PreFilters?.DeleteAll();
                Status = "All prefilters deleted.";
            }
            catch (InvalidUsageException)
            {
                Status = "Invalid usage while deleting prefilters.";
            }
            catch (OperationFailureException ofex)
            {
                Status = ofex.VendorMessage;
            }
        }
        private TARGET MapTarget(string label)
        {
            return label switch
            {
                "SL FLAG" => TARGET.TargetSl,
                "SESSION S0" => TARGET.TargetInventoriedStateS0,
                "SESSION S1" => TARGET.TargetInventoriedStateS1,
                "SESSION S2" => TARGET.TargetInventoriedStateS2,
                "SESSION S3" => TARGET.TargetInventoriedStateS3,
                _ => TARGET.TargetInventoriedStateS3
            };
        }

        private STATE_AWARE_ACTION MapAction(string label)
        {
            return label switch
            {
                "INV A NOT INV B OR ASRT SL NOT DSRT SL" => STATE_AWARE_ACTION.StateAwareActionInvANotInvB,
                "INV A OR ASRT SL" => STATE_AWARE_ACTION.StateAwareActionInvA,
                "NOT INV B OR NOT DSRT SL" => STATE_AWARE_ACTION.StateAwareActionNotInvB,
                "INV A2BB2A NOT INV A OR NEG SL NOT ASRT SL" => STATE_AWARE_ACTION.StateAwareActionInvA2bb2aNotInvA,
                "INV B NOT INV A OR DSRT SL NOT ASRT SL" => STATE_AWARE_ACTION.StateAwareActionInvBNotInvA,
                "INV B OR DSRT SL" => STATE_AWARE_ACTION.StateAwareActionInvB,
                "NOT INV A OR NOT ASRT SL" => STATE_AWARE_ACTION.StateAwareActionNotInvA,
                "NOT INV A2BB2A OR NOT NEG SL" => STATE_AWARE_ACTION.StateAwareActionNotInvA2bb2a,
                _ => STATE_AWARE_ACTION.StateAwareActionNotInvA2bb2a
            };
        }

        private MEMORY_BANK MapMemoryBank(string label)
        {
            return label switch
            {
                "RESERVED" => MEMORY_BANK.MemoryBankReserved,
                "EPC" => MEMORY_BANK.MemoryBankEpc,
                "TID" => MEMORY_BANK.MemoryBankTid,
                "USER" => MEMORY_BANK.MemoryBankUser,
                _ => MEMORY_BANK.MemoryBankEpc
            };
        }
    }
}
