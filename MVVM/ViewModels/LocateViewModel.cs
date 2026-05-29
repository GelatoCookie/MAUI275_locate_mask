
using Android.Widget;
using Com.Zebra.Rfid.Api3;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace MauiRfidSample.MVVM.ViewModels
{
	public class LocateViewModel : BaseViewModel
	{
		private string _TagPattern;
        private string _TagMask;
		private string _RelativeDistance;
		private Rect _distanceBox;
        private bool _isInventoryRunning;
        public bool IsInventoryRunning
        {
            get => _isInventoryRunning;
            set
            {
                if (_isInventoryRunning != value)
                {
                    _isInventoryRunning = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StartStopButtonText));
                }
            }
        }

        public string StartStopButtonText => IsInventoryRunning ? "Stop" : "Start";

        public ICommand StartStopCommand { get; }

        public LocateViewModel()
		{
			RelativeDistance = "0";
			TagPattern = InventoryListModel.SelectedItem?.ToString() ?? "";
            StartStopCommand = new Command(OnStartStopClicked);

            if (rfidModel.rfidReader != null && rfidModel.rfidReader.IsConnected)
            {
                if (rfidModel.rfidReader.Transport.ToString().Equals(ENUM_TRANSPORT.ReSerial.ToString()))
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Toast.MakeText(Android.App.Application.Context, "Feature not supported for this device.", ToastLength.Short).Show();
                    });

                    return;
                }  
            }
        }

        private void OnStartStopClicked()
        {
            if (!TryGetValidatedTagMask(out var tagMask))
            {
                return;
            }

            if (IsInventoryRunning)
            {
                IsInventoryRunning = false;
                if (TagPattern != null && TagPattern != "")
                    rfidModel.Locate(false, TagPattern, tagMask);

            }
            else
            {
                if (TagPattern != null && TagPattern != "")
                    rfidModel.Locate(true, TagPattern, tagMask);
                IsInventoryRunning = true;
            }
        }
        public override void HHTriggerEvent(bool pressed)
		{
            if (!TryGetValidatedTagMask(out var tagMask))
            {
                if (pressed)
                {
                    IsInventoryRunning = false;
                }
                return;
            }

            if (TagPattern != null && TagPattern != "")
				rfidModel.Locate(pressed, TagPattern, tagMask);

            IsInventoryRunning = pressed;
        }

		private string GetTagMaskOrNull()
		{
			if (string.IsNullOrWhiteSpace(TagMask))
			{
				return null;
			}

			return TagMask.Trim();
		}

        private bool TryGetValidatedTagMask(out string tagMask)
        {
            tagMask = GetTagMaskOrNull();
            if (tagMask == null)
            {
                return true;
            }

            if (!Regex.IsMatch(tagMask, "^[0-9a-fA-F]+$") || (tagMask.Length % 2 != 0))
            {
                rfidModel.ShowAlert("Tag Mask must be hex (0-9, A-F) and contain an even number of characters.");
                return false;
            }

            tagMask = tagMask.ToUpperInvariant();
            TagMask = tagMask;
            return true;
        }

		public override void TagReadEvent(TagData[] tags)
		{
			if (tags != null)
			{
				for (int index = 0; index < tags.Length; index++)
				{
					if (tags[index].LocationInfo != null)
					{
						RelativeDistance = tags[index].LocationInfo.RelativeDistance.ToString();
						UpdateFillView(tags[index].LocationInfo.RelativeDistance);
					}
				}
			}
		}

		private void UpdateFillView(short relativeDistance)
		{
			DistanceBox = new Rect(0, 0.05, 50, relativeDistance * 3);
		}

		public string TagPattern
		{
			get { return _TagPattern; }
			set { _TagPattern = value; OnPropertyChanged(); }
		}

        public string TagMask
        {
            get { return _TagMask; }
            set { _TagMask = value; OnPropertyChanged(); }
        }

		public string RelativeDistance
		{
			get { return _RelativeDistance; }
			set { _RelativeDistance = value; OnPropertyChanged(); }
		}

		public Rect DistanceBox
		{
			get { return _distanceBox; }
			set { _distanceBox = value; OnPropertyChanged(); }
		}


	}
}
