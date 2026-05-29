package mono.com.zebra.barcode.sdk;


public class FirmwareUpdateEventListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.zebra.barcode.sdk.FirmwareUpdateEventListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onFirmwareUpdateEventReceived:(Lcom/zebra/barcode/sdk/FirmwareUpdateEventArgs;)V:GetOnFirmwareUpdateEventReceived_Lcom_zebra_barcode_sdk_FirmwareUpdateEventArgs_Handler:Com.Zebra.Barcode.Sdk.IFirmwareUpdateEventListenerInvoker, MauiJavaSdkBinding\n" +
			"";
		mono.android.Runtime.register ("Com.Zebra.Barcode.Sdk.IFirmwareUpdateEventListenerImplementor, MauiJavaSdkBinding", FirmwareUpdateEventListenerImplementor.class, __md_methods);
	}

	public FirmwareUpdateEventListenerImplementor ()
	{
		super ();
		if (getClass () == FirmwareUpdateEventListenerImplementor.class) {
			mono.android.TypeManager.Activate ("Com.Zebra.Barcode.Sdk.IFirmwareUpdateEventListenerImplementor, MauiJavaSdkBinding", "", this, new java.lang.Object[] {  });
		}
	}

	public void onFirmwareUpdateEventReceived (com.zebra.barcode.sdk.FirmwareUpdateEventArgs p0)
	{
		n_onFirmwareUpdateEventReceived (p0);
	}

	private native void n_onFirmwareUpdateEventReceived (com.zebra.barcode.sdk.FirmwareUpdateEventArgs p0);

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
