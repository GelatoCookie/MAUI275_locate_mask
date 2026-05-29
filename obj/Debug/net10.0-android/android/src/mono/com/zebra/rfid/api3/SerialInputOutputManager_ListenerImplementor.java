package mono.com.zebra.rfid.api3;


public class SerialInputOutputManager_ListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.zebra.rfid.api3.SerialInputOutputManager.Listener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_isConnected:()Ljava/lang/Boolean;:GetIsConnectedHandler:Com.Zebra.Rfid.Api3.SerialInputOutputManager+IListenerInvoker, MauiJavaSdkBinding\n" +
			"n_onNewData:([B)V:GetOnNewData_arrayBHandler:Com.Zebra.Rfid.Api3.SerialInputOutputManager+IListenerInvoker, MauiJavaSdkBinding\n" +
			"n_onRunError:(Ljava/lang/Exception;)V:GetOnRunError_Ljava_lang_Exception_Handler:Com.Zebra.Rfid.Api3.SerialInputOutputManager+IListenerInvoker, MauiJavaSdkBinding\n" +
			"";
		mono.android.Runtime.register ("Com.Zebra.Rfid.Api3.SerialInputOutputManager+IListenerImplementor, MauiJavaSdkBinding", SerialInputOutputManager_ListenerImplementor.class, __md_methods);
	}

	public SerialInputOutputManager_ListenerImplementor ()
	{
		super ();
		if (getClass () == SerialInputOutputManager_ListenerImplementor.class) {
			mono.android.TypeManager.Activate ("Com.Zebra.Rfid.Api3.SerialInputOutputManager+IListenerImplementor, MauiJavaSdkBinding", "", this, new java.lang.Object[] {  });
		}
	}

	public java.lang.Boolean isConnected ()
	{
		return n_isConnected ();
	}

	private native java.lang.Boolean n_isConnected ();

	public void onNewData (byte[] p0)
	{
		n_onNewData (p0);
	}

	private native void n_onNewData (byte[] p0);

	public void onRunError (java.lang.Exception p0)
	{
		n_onRunError (p0);
	}

	private native void n_onRunError (java.lang.Exception p0);

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
