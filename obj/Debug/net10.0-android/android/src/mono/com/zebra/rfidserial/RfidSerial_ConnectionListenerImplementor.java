package mono.com.zebra.rfidserial;


public class RfidSerial_ConnectionListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.zebra.rfidserial.RfidSerial.ConnectionListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onServiceConnected:()V:GetOnServiceConnectedHandler:Com.Zebra.Rfidserial.RfidSerial+IConnectionListenerInvoker, MauiJavaSdkBinding\n" +
			"n_onServiceDisconnected:()V:GetOnServiceDisconnectedHandler:Com.Zebra.Rfidserial.RfidSerial+IConnectionListenerInvoker, MauiJavaSdkBinding\n" +
			"";
		mono.android.Runtime.register ("Com.Zebra.Rfidserial.RfidSerial+IConnectionListenerImplementor, MauiJavaSdkBinding", RfidSerial_ConnectionListenerImplementor.class, __md_methods);
	}

	public RfidSerial_ConnectionListenerImplementor ()
	{
		super ();
		if (getClass () == RfidSerial_ConnectionListenerImplementor.class) {
			mono.android.TypeManager.Activate ("Com.Zebra.Rfidserial.RfidSerial+IConnectionListenerImplementor, MauiJavaSdkBinding", "", this, new java.lang.Object[] {  });
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
