package mono.com.zebra.rfidhost;


public class RfidHost_ConnectionListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.zebra.rfidhost.RfidHost.ConnectionListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onServiceConnected:()V:GetOnServiceConnectedHandler:Com.Zebra.Rfidhost.RfidHost+IConnectionListenerInvoker, MauiJavaSdkBinding\n" +
			"n_onServiceDisconnected:()V:GetOnServiceDisconnectedHandler:Com.Zebra.Rfidhost.RfidHost+IConnectionListenerInvoker, MauiJavaSdkBinding\n" +
			"";
		mono.android.Runtime.register ("Com.Zebra.Rfidhost.RfidHost+IConnectionListenerImplementor, MauiJavaSdkBinding", RfidHost_ConnectionListenerImplementor.class, __md_methods);
	}

	public RfidHost_ConnectionListenerImplementor ()
	{
		super ();
		if (getClass () == RfidHost_ConnectionListenerImplementor.class) {
			mono.android.TypeManager.Activate ("Com.Zebra.Rfidhost.RfidHost+IConnectionListenerImplementor, MauiJavaSdkBinding", "", this, new java.lang.Object[] {  });
		}
	}

	public void onServiceConnected ()
	{
		n_onServiceConnected ();
	}

	private native void n_onServiceConnected ();

	public void onServiceDisconnected ()
	{
		n_onServiceDisconnected ();
	}

	private native void n_onServiceDisconnected ();

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
