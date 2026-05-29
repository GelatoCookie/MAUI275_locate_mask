package mono.com.zebra.rfid.api3.eventHandling;


public class EventListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.zebra.rfid.api3.eventHandling.EventListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_eventReadNotify:(Lcom/zebra/rfid/api3/eventHandling/ReadEvents;)V:GetEventReadNotify_Lcom_zebra_rfid_api3_eventHandling_ReadEvents_Handler:Com.Zebra.Rfid.Api3.EventHandling.IEventListenerInvoker, MauiJavaSdkBinding\n" +
			"n_eventStatusNotify:(Lcom/zebra/rfid/api3/eventHandling/StatusEvents;)V:GetEventStatusNotify_Lcom_zebra_rfid_api3_eventHandling_StatusEvents_Handler:Com.Zebra.Rfid.Api3.EventHandling.IEventListenerInvoker, MauiJavaSdkBinding\n" +
			"";
		mono.android.Runtime.register ("Com.Zebra.Rfid.Api3.EventHandling.IEventListenerImplementor, MauiJavaSdkBinding", EventListenerImplementor.class, __md_methods);
	}

	public EventListenerImplementor ()
	{
		super ();
		if (getClass () == EventListenerImplementor.class) {
			mono.android.TypeManager.Activate ("Com.Zebra.Rfid.Api3.EventHandling.IEventListenerImplementor, MauiJavaSdkBinding", "", this, new java.lang.Object[] {  });
		}
	}

	public void eventReadNotify (com.zebra.rfid.api3.eventHandling.ReadEvents p0)
	{
		n_eventReadNotify (p0);
	}

	private native void n_eventReadNotify (com.zebra.rfid.api3.eventHandling.ReadEvents p0);

	public void eventStatusNotify (com.zebra.rfid.api3.eventHandling.StatusEvents p0)
	{
		n_eventStatusNotify (p0);
	}

	private native void n_eventStatusNotify (com.zebra.rfid.api3.eventHandling.StatusEvents p0);

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
