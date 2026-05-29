package mono.com.zebra.rfid.api3;


public class WifiScanDataEventsListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.zebra.rfid.api3.WifiScanDataEventsListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_eventWifiScanNotify:(Lcom/zebra/rfid/api3/RfidWifiScanEvents;)V:GetEventWifiScanNotify_Lcom_zebra_rfid_api3_RfidWifiScanEvents_Handler:Com.Zebra.Rfid.Api3.IWifiScanDataEventsListenerInvoker, MauiJavaSdkBinding\n" +
			"";
		mono.android.Runtime.register ("Com.Zebra.Rfid.Api3.IWifiScanDataEventsListenerImplementor, MauiJavaSdkBinding", WifiScanDataEventsListenerImplementor.class, __md_methods);
	}

	public WifiScanDataEventsListenerImplementor ()
	{
		super ();
		if (getClass () == WifiScanDataEventsListenerImplementor.class) {
			mono.android.TypeManager.Activate ("Com.Zebra.Rfid.Api3.IWifiScanDataEventsListenerImplementor, MauiJavaSdkBinding", "", this, new java.lang.Object[] {  });
		}
	}

	public void eventWifiScanNotify (com.zebra.rfid.api3.RfidWifiScanEvents p0)
	{
		n_eventWifiScanNotify (p0);
	}

	private native void n_eventWifiScanNotify (com.zebra.rfid.api3.RfidWifiScanEvents p0);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
