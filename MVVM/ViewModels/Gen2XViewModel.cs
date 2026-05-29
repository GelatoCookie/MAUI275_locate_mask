using Android.Locations;
using Android.Widget;
using Com.Zebra.Rfid.Api3;
using MauiRfidSample.MVVM.Models;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using static Com.Zebra.Rfid.Api3.PreFilters;

namespace MauiRfidSample.MVVM.ViewModels
{
    public class Gen2XViewModel : BaseViewModel
    {

        private static ObservableCollection<TagItem> _allItems = new();
        private static readonly Dictionary<string, int> _tagCounts = new();
        public event Action? RequestUiSelectionClear;

        private DateTime _startTime;
        private int _totalSeen;
        private string _uniqueTags = "0";
        private string _totalTags = "0";
        private string _totalTime = "00:00:00";
        private string _connectionStatus = "";
        private string _readerStatus = "";
        private bool _listAvailable;
        private bool _isInventoryRunning;

        private bool _isQuiet;
        private bool _isUnquiet;
        private bool _isTagFocus;
        private bool _isFastID;

        private enum Mode { None, Quiet, Unquiet, TagFocus, FastID }
        private Mode _currentMode = Mode.None;

        private const int MaxSelection = 30;

        
        private readonly ObservableCollection<TagItem> _selectedTagItems = new();
        public ObservableCollection<TagItem> SelectedTagItems => _selectedTagItems;
        public int SelectedCount => _selectedTagItems.Count;

        public string SelectionWarning { get; private set; } = string.Empty;

        public ICommand StartStopCommand { get; }

        public Gen2XViewModel()
        {
            if (rfidModel.rfidReader != null && rfidModel.rfidReader.IsConnected)
            {
                if (rfidModel.rfidReader.Transport.ToString().Equals(ENUM_TRANSPORT.QcSerial.ToString()))
                {
                    ShowAlert("Feature not supported for this device.");
                    return;
                }         
            }

            UpdateHints();
            StartStopCommand = new Command(_ => OnStartStopClicked());
            if (rfidModel.rfidReader != null)
            {
                //    rfidModel.impinjExtensions = new ImpinjExtensions(rfidModel.rfidReader);
            }

            Debug.WriteLine("[Gen2X] VM constructed");
        }


        public ObservableCollection<TagItem> AllItems { get => _allItems; set { if (_allItems != value) { _allItems = value; OnPropertyChanged(); } } }
        public string UniqueTags { get => _uniqueTags; set { _uniqueTags = value; OnPropertyChanged(); } }
        public string TotalTags { get => _totalTags; set { _totalTags = value; OnPropertyChanged(); } }
        public string TotalTime { get => _totalTime; set { _totalTime = value; OnPropertyChanged(); } }
        public string readerConnection { get => _connectionStatus; set { _connectionStatus = value; OnPropertyChanged(); } }
        public string readerStatus { get => _readerStatus; set { _readerStatus = value; OnPropertyChanged(); } }
        public bool listAvailable { get => _listAvailable; set { _listAvailable = value; OnPropertyChanged(); OnPropertyChanged(nameof(hintAvailable)); } }
        public bool hintAvailable => !_listAvailable;

        public bool IsInventoryRunning
        {
            get => _isInventoryRunning;
            set
            {
                if (_isInventoryRunning == value) return;
                _isInventoryRunning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StartStopButtonText));
            }
        }
        public string StartStopButtonText => IsInventoryRunning ? "Stop" : "Start";

        public bool IsQuiet
        {
            get => _isQuiet;
            set
            {
                if (_isQuiet == value) return;
                _isQuiet = value;
                OnPropertyChanged();
                if (value)
                {
                    SetMode(Mode.Quiet);
                }
                else
                {
                    deleteAllPrefilters();
                    readerStatus = "Quiet mode off.";
                    OnPropertyChanged(nameof(readerStatus));
                }
            }
        }
        public bool IsUnquiet
        {
            get => _isUnquiet;
            set
            {
                if (_isUnquiet == value) return;
                _isUnquiet = value;
                OnPropertyChanged();
                if (value) SetMode(Mode.Unquiet); else CheckNoneFallback();
            }
        }
        public bool IsTagFocus
        {
            get => _isTagFocus;
            set
            {
                if (_isTagFocus == value) return;
                _isTagFocus = value;
                OnPropertyChanged();
                if (value) SetMode(Mode.TagFocus); else CheckNoneFallback();
            }
        }
        public bool IsFastID
        {
            get => _isFastID;
            set
            {
                if (_isFastID == value) return;
                _isFastID = value;
                OnPropertyChanged();
                if (value) SetMode(Mode.FastID); else CheckNoneFallback();
            }
        }
        public bool AnyModeSelected => _currentMode != Mode.None;
        public string CurrentModeDisplay => _currentMode.ToString();

        public void HandleSelectionChanged(SelectionChangedEventArgs e)
        {
            if (e?.CurrentSelection == null || e.CurrentSelection.Count == 0)
                return;
            Debug.WriteLine(" Current selection " + e.CurrentSelection.Count);
            var newSelection = e.CurrentSelection
                .OfType<TagItem>()
                .Where(t => !string.IsNullOrWhiteSpace(t.InvID))
                .Distinct()
                .Take(MaxSelection)
                .ToList();

            _selectedTagItems.Clear();
            foreach (var tag in newSelection)
                _selectedTagItems.Add(tag);

            SelectionWarning = newSelection.Count >= MaxSelection
                ? $"Selection limited to {MaxSelection} tags."
                : string.Empty;

            Debug.WriteLine("[Gen2X] Selected Items " + _selectedTagItems.Count);

            OnPropertyChanged(nameof(SelectedTagItems));
            OnPropertyChanged(nameof(SelectedCount));
            OnPropertyChanged(nameof(SelectionWarning));


            if (_isQuiet)
                applyQuietMode();
        }


        private void OnStartStopClicked()
        {
            if (IsInventoryRunning)
            {
                IsInventoryRunning = false;
                StopInventory();
                listAvailable = true;
            }
            else
            {
                PerformInventory();
                IsInventoryRunning = true;
            }
        }

        public override void HHTriggerEvent(bool pressed)
        {
            if (pressed && !IsInventoryRunning)
            {
                PerformInventory();
                IsInventoryRunning = true;
                listAvailable = true;
            }
            else if (!pressed && IsInventoryRunning)
            {
                IsInventoryRunning = false;
                StopInventory();
            }
        }

        private void PerformInventory()
        {
            ApplyMode(_currentMode);
            _tagCounts.Clear();
            AllItems.Clear();
            _selectedTagItems.Clear();
            OnPropertyChanged(nameof(SelectedTagItems));
            OnPropertyChanged(nameof(SelectedCount));
            SelectionWarning = string.Empty;
            OnPropertyChanged(nameof(SelectionWarning));

            _totalSeen = 0;
            _startTime = DateTime.Now;

            rfidModel.PerformInventory();
        }

        private void StopInventory()
        {
            _selectedTagItems.Clear();
            SelectedTagItems.Clear();
            RequestUiSelectionClear?.Invoke();
            clearAll();
            Debug.WriteLine("Inventory stopped, Cleared the selected items");
            OnPropertyChanged(nameof(SelectedTagItems));
            OnPropertyChanged(nameof(SelectedCount));
            rfidModel.StopInventory();


        }

        private void clearAll()
        {
            SetMode(Mode.None);
            if (_isFastID)
            {
                _isFastID = false;
                OnPropertyChanged(nameof(IsFastID));
            }
            if (_isTagFocus)
            {
                _isTagFocus = false;
                OnPropertyChanged(nameof(IsTagFocus));
            }
            if (_isUnquiet)
            {
                _isUnquiet = false;
                OnPropertyChanged(nameof(IsUnquiet));
            }
            if (_isQuiet)
            {
                _isQuiet = false;
                OnPropertyChanged(nameof(IsQuiet));
            }
        }

        // Tag reading
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void TagReadEvent(TagData[]? tags)
        {
            if (tags == null || tags.Length == 0) return;
            lock (_tagCounts)
            {
                foreach (var t in tags)
                {
                    var id = t.TagID;
                    if (string.IsNullOrEmpty(id)) continue;

                    if (_tagCounts.TryGetValue(id, out var existing))
                    {
                        _tagCounts[id] = existing + t.TagSeenCount;
                        UpdateExisting(id, _tagCounts[id], t.PeakRSSI);
                    }
                    else
                    {
                        _tagCounts[id] = t.TagSeenCount;
                        AddNew(id, t.TagSeenCount, t.PeakRSSI);
                    }
                    _totalSeen += t.TagSeenCount;
                }
            }
            UpdateStats();
        }

        private void AddNew(string id, int count, short rssi)
        {
            MainThreadDispatch(() =>
            {
                AllItems.Add(new TagItem { InvID = id, TagCount = count, RSSI = rssi });
            });
        }

        private void UpdateExisting(string id, int count, short rssi)
        {
            MainThreadDispatch(() =>
            {
                var item = AllItems.FirstOrDefault(x => x.InvID == id);
                if (item != null)
                {
                    item.TagCount = count;
                    item.RSSI = rssi;
                }
            });
        }

        private void UpdateStats()
        {
            MainThreadDispatch(() =>
            {
                UniqueTags = _tagCounts.Count.ToString();
                TotalTags = _totalSeen.ToString();
                TotalTime = (DateTime.Now - _startTime).ToString("hh\\:mm\\:ss");
            });
        }

        // Mode
        private void SetMode(Mode mode)
        {
            if (_currentMode == mode) return;
            _currentMode = mode;

            _isQuiet = mode == Mode.Quiet;
            _isUnquiet = mode == Mode.Unquiet;
            _isTagFocus = mode == Mode.TagFocus;
            _isFastID = mode == Mode.FastID;

            OnPropertyChanged(nameof(IsQuiet));
            OnPropertyChanged(nameof(IsUnquiet));
            OnPropertyChanged(nameof(IsTagFocus));
            OnPropertyChanged(nameof(IsFastID));
            OnPropertyChanged(nameof(AnyModeSelected));
            OnPropertyChanged(nameof(CurrentModeDisplay));


        }

        private void CheckNoneFallback()
        {
            if (!_isQuiet && !_isUnquiet && !_isTagFocus && !_isFastID)
                SetMode(Mode.None);
        }

        private void ApplyMode(Mode mode)
        {
            switch (mode)
            {
                case Mode.None:
                    deleteAllPrefilters();
                    break;

                case Mode.Quiet:
                    applyQuietMode();
                    break;

                case Mode.Unquiet:
                    applyUnquietMode();
                    break;

                case Mode.TagFocus:
                    applyTagFocusMode();
                    break;

                case Mode.FastID:
                    applyFastIDMode();
                    break;

            }
            OnPropertyChanged(nameof(readerStatus));
        }

        // Quiet uses the current SelectedTagItems (live list)
        private void applyQuietMode()
        {
            try
            {
                if (rfidModel.rfidReader == null)
                {
                    Debug.WriteLine("[Gen2X] RFID reader is null.");
                    return;
                }
                deleteAllPrefilters();

                if (_selectedTagItems.Count == 0)
                {
                    Debug.WriteLine("Quiet mode: no selected tags.");
                    OnPropertyChanged(nameof(readerStatus));
                    return;
                }

                var filters = new PreFilters.PreFilter[_selectedTagItems.Count];
                for (int i = 0; i < _selectedTagItems.Count; i++)
                {
                    var id = _selectedTagItems[i].InvID;
                    var pfContainer = new PreFilters();
                    var pf = new PreFilters.PreFilter(pfContainer)
                    {
                        AntennaID = 1,
                        BitOffset = 32,
                        TagPatternBitCount = 96,
                        MemoryBank = MEMORY_BANK.MemoryBankEpc,
                        FilterAction = FILTER_ACTION.FilterActionStateAware
                    };
                    pf.SetTagPattern(id);
                    pf.StateAwareAction.Target = TARGET.TargetInventoriedStateS3;
                    pf.StateAwareAction.StateAwareAction = STATE_AWARE_ACTION.StateAwareActionInvB;
                    filters[i] = pf;
                }
                Debug.WriteLine($"Quiet mode applied to {_selectedTagItems.Count} tag(s).");

                rfidModel.rfidReader?.Actions?.PreFilters?.Add(filters, null);

                Thread.Sleep(100);

                ENUM_TAGQUIET_MASK[] quietMasks = { ENUM_TAGQUIET_MASK.S3b };
                rfidModel.impinjExtensions.SetTagQuiet(quietMasks, TARGET.TargetSl, STATE_AWARE_ACTION.StateAwareActionAsrtSl, 1);

                SetSingulation(SESSION.SessionS2, INVENTORY_STATE.InventoryStateAbFlip, SL_FLAG.SlFlagDeasserted);


                OnPropertyChanged(nameof(readerStatus));
            }
            catch (InvalidUsageException iuex)
            {
                Debug.WriteLine($"[Gen2X][FastID] InvalidUsageException: {iuex.Message}");
            }
            catch (OperationFailureException ofex)
            {
                Debug.WriteLine($"[Gen2X][FastID] OperationFailureException: {ofex.VendorMessage}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Gen2X][Quiet] Error: {ex.Message}");
            }
        }

        private void applyUnquietMode()
        {
            try
            {
                if (rfidModel.rfidReader == null)
                {
                    Debug.WriteLine("[Gen2X] RFID reader is null.");
                    return;
                }
                deleteAllPrefilters();
                ENUM_TAGQUIET_MASK[] tagMask = { ENUM_TAGQUIET_MASK.S3b };
                rfidModel.impinjExtensions.SetTagQuiet(tagMask, TARGET.TargetSl, STATE_AWARE_ACTION.StateAwareActionDsrtSl, 1);
                rfidModel.impinjExtensions.SetTagQuiet(tagMask, TARGET.TargetInventoriedStateS3, STATE_AWARE_ACTION.StateAwareActionInvA, 1);
            }
            catch (InvalidUsageException iuex)
            {
                Debug.WriteLine($"[Gen2X][FastID] InvalidUsageException: {iuex.Message}");
            }
            catch (OperationFailureException ofex)
            {
                Debug.WriteLine($"[Gen2X][FastID] OperationFailureException: {ofex.VendorMessage}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Gen2X][Unquiet] Error: {ex.Message}");
            }
        }

        private void applyTagFocusMode()
        {
            try
            {
                if (rfidModel.rfidReader == null)
                {
                    Debug.WriteLine("[Gen2X] RFID reader is null.");
                    return;
                }
                deleteAllPrefilters();
                rfidModel.impinjExtensions.SetTagFocus(true, 1);
                SetSingulation(SESSION.SessionS1, INVENTORY_STATE.InventoryStateA, SL_FLAG.SlAll);
            }
            catch (InvalidUsageException iuex)
            {
                Debug.WriteLine($"[Gen2X][FastID] InvalidUsageException: {iuex.Message}");
            }
            catch (OperationFailureException ofex)
            {
                Debug.WriteLine($"[Gen2X][FastID] OperationFailureException: {ofex.VendorMessage}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Gen2X][TagFocus] Error: {ex.Message}");
            }
        }

        private void applyFastIDMode()
        {
            try
            {
                if (rfidModel.rfidReader == null)
                {
                    Debug.WriteLine("[Gen2X] RFID reader is null.");
                    return;
                }
                deleteAllPrefilters();
                rfidModel.impinjExtensions.SetFastID(true, 1);
            }
            catch (InvalidUsageException iuex)
            {
                Debug.WriteLine($"[Gen2X][FastID] InvalidUsageException: {iuex.Message}");
            }
            catch (OperationFailureException ofex)
            {
                Debug.WriteLine($"[Gen2X][FastID] OperationFailureException: {ofex.VendorMessage}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Gen2X][FastID] Error: {ex.Message}");
            }
        }

        private void deleteAllPrefilters()
        {
            try
            {
                rfidModel.rfidReader?.Actions?.PreFilters?.DeleteAll();
                Thread.Sleep(100);
            }
            catch (InvalidUsageException iuex)
            {
                Debug.WriteLine($"[Gen2X][FastID] InvalidUsageException: {iuex.Message}");
            }
            catch (OperationFailureException ofex)
            {
                Debug.WriteLine($"[Gen2X][FastID] OperationFailureException: {ofex.VendorMessage}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Gen2X][Prefilters] DeleteAll error: {ex.Message}");
            }
        }

        private void SetSingulation(SESSION session, INVENTORY_STATE state, SL_FLAG sl)
        {
            try
            {
                if (rfidModel.rfidReader == null)
                {
                    Debug.WriteLine("[Gen2X] RFID reader is null.");
                    return;
                }
                var antennas = rfidModel.rfidReader?.Config?.Antennas;
                if (antennas == null) return;
                var sc = antennas.GetSingulationControl(1);
                if (sc == null) return;
                sc.Session = session;
                sc.Action.InventoryState = state;
                sc.Action.SLFlag = sl;
                antennas.SetSingulationControl(1, sc);
            }
            catch (InvalidUsageException iuex)
            {
                Debug.WriteLine($"[Gen2X][FastID] InvalidUsageException: {iuex.Message}");
            }
            catch (OperationFailureException ofex)
            {
                Debug.WriteLine($"[Gen2X][FastID] OperationFailureException: {ofex.VendorMessage}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Gen2X][Singulation] Error: {ex.Message}");
            }
        }

        public override void ReaderConnectionEvent(bool connection)
        {
            base.ReaderConnectionEvent(connection);
            UpdateHints();
        }

        private void UpdateHints() => updateHints();
        private void updateHints()
        {
            if (AllItems.Count == 0)
            {
                listAvailable = false;
                readerConnection = isConnected ? "Connected" : "Not connected";
                if (isConnected)
                {
                    readerStatus = rfidModel.isBatchMode
                        ? "Inventory is running in batch mode"
                        : "Press and hold the trigger for tag reading";
                }
            }
            else
            {
                listAvailable = true;
            }
            OnPropertyChanged(nameof(readerStatus));
        }

        private void MainThreadDispatch(Action a)
        {
            if (Application.Current?.Dispatcher != null)
            {
                if (Application.Current.Dispatcher.IsDispatchRequired)
                    Application.Current.Dispatcher.Dispatch(a);
                else
                    a();
            }
            else
            {
                a();
            }
        }

        public void displayDialog()
        {
            string UserGuideText = "•Quiet:" +
                "\r\n   - Applies tag quieting prefilters to currently selected tags." +
                    "\r\n   - You can select upto 31 tags for the devices which support 32-prefilters before starting inventory." +
                "\r\n•TagFocus:" +
                    "\r\n   - Enables TagFocus  prefilter.\r\n   - Adjusts singulation (Session S1, InventoryState A, SL All)." +
                "\r\n•FastID:" +
                    "\r\n   - Enables FastID prefilter." +
                "\r\n•None Selected:" +
                    "\r\n   - If none of the checkboxes are selected all filters are cleared before doing inventory." +
                "\r\n•Note:" +
                    "\r\nAll prefilters are applied immediately before doing inventory";

            MainThread.BeginInvokeOnMainThread(async () => {
                await Application.Current.MainPage.DisplayAlert(
                    "Info :",
                    UserGuideText,
                    "OK");
            });
        }

        private void ShowAlert(string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Toast.MakeText(Android.App.Application.Context, message, ToastLength.Short).Show();
            });
        }
    }
}

     

