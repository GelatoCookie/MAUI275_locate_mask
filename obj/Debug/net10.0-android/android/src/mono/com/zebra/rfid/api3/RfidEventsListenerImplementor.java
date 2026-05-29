package mono.com.zebra.rfid.api3;


public class RfidEventsListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.zebra.rfid.api3.RfidEventsListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_eventReadNotify:(Lcom/zebra/rfid/api3/RfidReadEvents;)V:GetEventReadNotify_Lcom_zebra_rfid_api3_RfidReadEvents_Handler:Com.Zebra.Rfid.Api3.IRfidEventsListenerInvoker, MauiJavaSdkBinding\n" +
			"n_eventStatusNotify:(Lcom/zebra/rfid/api3/RfidStatusEvents;)V:GetEventStatusNotify_Lcom_zebra_rfid_api3_RfidStatusEvents_Handler:Com.Zebra.Rfid.Api3.IRfidEventsListenerInvoker, MauiJavaSdkBinding\n" +
			"";
		mono.android.Runtime.register ("Com.Zebra.Rfid.Api3.IRfidEventsListenerImplementor, MauiJavaSdkBinding", RfidEventsListenerImplementor.class, __md_methods);
	}

	public RfidEventsListenerImplementor ()
	{
		super ();
		if (getClass () == RfidEventsListenerImplementor.class) {
			mono.android.TypeManager.Activate ("Com.Zebra.Rfid.Api3.IRfidEventsListenerImplementor, MauiJavaSdkBinding", "", this, new java.lang.Object[] {  });
		}
	}

	public void eventReadNotify (com.zebra.rfid.api3.RfidReadEvents p0)
	{
		n_eventReadNotify (p0);
	}

	private native void n_eventReadNotify (com.zebra.rfid.api3.RfidReadEvents p0);

	public void eventStatusNotify (com.zebra.rfid.api3.RfidStatusEvents p0)
	{
		n_eventStatusNotify (p0);
	}

	private native void n_eventStatusNotify (com.zebra.rfid.api3.RfidStatusEvents p0);

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
