package mono.com.zebra.barcode.sdk;


public class AuxiliaryScannerStatusChangeListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.zebra.barcode.sdk.AuxiliaryScannerStatusChangeListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onAuxillaryDeviceAdded:(Lcom/zebra/barcode/sdk/AuxiliaryDeviceStatusEventArgs;)V:GetOnAuxillaryDeviceAdded_Lcom_zebra_barcode_sdk_AuxiliaryDeviceStatusEventArgs_Handler:Com.Zebra.Barcode.Sdk.IAuxiliaryScannerStatusChangeListenerInvoker, MauiJavaSdkBinding\n" +
			"n_onAuxillaryDeviceRemoved:(Lcom/zebra/barcode/sdk/AuxiliaryDeviceStatusEventArgs;)V:GetOnAuxillaryDeviceRemoved_Lcom_zebra_barcode_sdk_AuxiliaryDeviceStatusEventArgs_Handler:Com.Zebra.Barcode.Sdk.IAuxiliaryScannerStatusChangeListenerInvoker, MauiJavaSdkBinding\n" +
			"";
		mono.android.Runtime.register ("Com.Zebra.Barcode.Sdk.IAuxiliaryScannerStatusChangeListenerImplementor, MauiJavaSdkBinding", AuxiliaryScannerStatusChangeListenerImplementor.class, __md_methods);
	}

	public AuxiliaryScannerStatusChangeListenerImplementor ()
	{
		super ();
		if (getClass () == AuxiliaryScannerStatusChangeListenerImplementor.class) {
			mono.android.TypeManager.Activate ("Com.Zebra.Barcode.Sdk.IAuxiliaryScannerStatusChangeListenerImplementor, MauiJavaSdkBinding", "", this, new java.lang.Object[] {  });
		}
	}

	public void onAuxillaryDeviceAdded (com.zebra.barcode.sdk.AuxiliaryDeviceStatusEventArgs p0)
	{
		n_onAuxillaryDeviceAdded (p0);
	}

	private native void n_onAuxillaryDeviceAdded (com.zebra.barcode.sdk.AuxiliaryDeviceStatusEventArgs p0);

	public void onAuxillaryDeviceRemoved (com.zebra.barcode.sdk.AuxiliaryDeviceStatusEventArgs p0)
	{
		n_onAuxillaryDeviceRemoved (p0);
	}

	private native void n_onAuxillaryDeviceRemoved (com.zebra.barcode.sdk.AuxiliaryDeviceStatusEventArgs p0);

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
