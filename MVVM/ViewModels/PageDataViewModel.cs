
using MauiRfidSample.MVVM.Views;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MauiRfidSample.MVVM.ViewModels
{
	public class PageDataViewModel
	{
		public PageDataViewModel(Type type, string title, string description)
		{
			Type = type;
			Title = title;
			Description = description;
		}

		public Type Type { private set; get; }

		public string Title { private set; get; }

		public string Description { private set; get; }

        [MethodImpl(MethodImplOptions.Synchronized)]
        static PageDataViewModel()
		{
			All = new List<PageDataViewModel>
			{
                new PageDataViewModel(typeof(ReaderList), "Reader List",
                                      "To select the reader from available reader list"),
                new PageDataViewModel(typeof(InventoryList), "Item inventory",
									  "Used for Item count or stock count\nPerforms inventory operation"),
                new PageDataViewModel(typeof(BarcodeScanner), "Barcode Scanner",
                                      "Used for Scanning Barcode"),
				new PageDataViewModel(typeof(ReadWriteAccess), "Item commission",
									  "Used for item commissioning, Performs read, write or lock operation on tag memory bank"),
				new PageDataViewModel(typeof(LocateTag), "Item search",
									  "Used for locate the intersted item\nPerforms tag locatinging operation"),
				new PageDataViewModel(typeof(RelativeDistance), "Relative Distance",
									  "Shows the relative proximity of multiple tags"),
           //     new PageDataViewModel(typeof(ViewDemo), "Demo *",
									  //"Place holder"),
				new PageDataViewModel(typeof(FirmwareUpdate), "Firmware Update",
									  "Used for Updating Firmware"),
				new PageDataViewModel(typeof(TriggerMapping), "Trigger Key Mapping",
									  "Used for config trigger keys"),
				new PageDataViewModel(typeof(ReaderWiFi), "Reader WiFi Settings",
                                      "Used for config WLAN Settings in Reader\n Supported in PREMIUM & PREMIUM PLUS devices"),
                new PageDataViewModel(typeof(EndpointConfiguration), "Endpoint Configuration",
                                      "Endpoint Configuration"),
                 new PageDataViewModel(typeof(Certificates), "Certificates",
                                      "Add Certificates"),
                new PageDataViewModel(typeof(WifiStatus), " WifiStatus",
                                      " WifiStatus"),
				new PageDataViewModel(typeof(AdminLogin), "Admin Login",
									"Admin Login to access secure apis"),
                new PageDataViewModel(typeof(EndpointStatus), "Endpoint Status",
                                    "Endpoint Status"),
                new PageDataViewModel(typeof(Gen2XView), "Gen2X Features ",
                                    "TagFocus, Tag Quieting, FastID"),
				new PageDataViewModel(typeof(Gen2XTagProtection), "Gen2X Tag protection ",
									"Tag Protection" ),
				new PageDataViewModel(typeof(SingulationView), "Singulation ",
				"Singulation Settings" ),
				new PageDataViewModel(typeof(Prefilters), "Prefilters ",
				"Prefilters Settings" ),
            };
		}

		public static IList<PageDataViewModel> All { private set; get; }
	}
}
