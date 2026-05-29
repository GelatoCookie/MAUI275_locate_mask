package mono.com.zebra.scannercontrol;


public class SDKHandler_SfCoexistenceResultListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.zebra.scannercontrol.SDKHandler.SfCoexistenceResultListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onResult:(Lcom/zebra/scannercontrol/SDKHandler$SfCoexistenceResultListener$SfCoexistenceResult;)V:GetOnResult_Lcom_zebra_scannercontrol_SDKHandler_SfCoexistenceResultListener_SfCoexistenceResult_Handler:Com.Zebra.Scannercontrol.SDKHandler+ISfCoexistenceResultListenerInvoker, MauiJavaSdkBinding\n" +
			"";
		mono.android.Runtime.register ("Com.Zebra.Scannercontrol.SDKHandler+ISfCoexistenceResultListenerImplementor, MauiJavaSdkBinding", SDKHandler_SfCoexistenceResultListenerImplementor.class, __md_methods);
	}

	public SDKHandler_SfCoexistenceResultListenerImplementor ()
	{
		super ();
		if (getClass () == SDKHandler_SfCoexistenceResultListenerImplementor.class) {
			mono.android.TypeManager.Activate ("Com.Zebra.Scannercontrol.SDKHandler+ISfCoexistenceResultListenerImplementor, MauiJavaSdkBinding", "", this, new java.lang.Object[] {  });
		}
	}

	public void onResult (com.zebra.scannercontrol.SDKHandler.SfCoexistenceResultListener.SfCoexistenceResult p0)
	{
		n_onResult (p0);
	}

	private native void n_onResult (com.zebra.scannercontrol.SDKHandler.SfCoexistenceResultListener.SfCoexistenceResult p0);

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
