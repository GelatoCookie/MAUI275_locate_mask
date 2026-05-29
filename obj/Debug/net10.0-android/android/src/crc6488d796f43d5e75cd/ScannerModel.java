package crc6488d796f43d5e75cd;


public class ScannerModel
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.zebra.scannercontrol.IDcsSdkApiDelegate
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_dcssdkEventAuxScannerAppeared:(Lcom/zebra/scannercontrol/DCSScannerInfo;Lcom/zebra/scannercontrol/DCSScannerInfo;)V:GetDcssdkEventAuxScannerAppeared_Lcom_zebra_scannercontrol_DCSScannerInfo_Lcom_zebra_scannercontrol_DCSScannerInfo_Handler:Com.Zebra.Scannercontrol.IDcsSdkApiDelegateInvoker, MauiJavaSdkBinding\n" +
			"n_dcssdkEventBarcode:([BII)V:GetDcssdkEventBarcode_arrayBIIHandler:Com.Zebra.Scannercontrol.IDcsSdkApiDelegateInvoker, MauiJavaSdkBinding\n" +
			"n_dcssdkEventBinaryData:([BI)V:GetDcssdkEventBinaryData_arrayBIHandler:Com.Zebra.Scannercontrol.IDcsSdkApiDelegateInvoker, MauiJavaSdkBinding\n" +
			"n_dcssdkEventCommunicationSessionEstablished:(Lcom/zebra/scannercontrol/DCSScannerInfo;)V:GetDcssdkEventCommunicationSessionEstablished_Lcom_zebra_scannercontrol_DCSScannerInfo_Handler:Com.Zebra.Scannercontrol.IDcsSdkApiDelegateInvoker, MauiJavaSdkBinding\n" +
			"n_dcssdkEventCommunicationSessionTerminated:(I)V:GetDcssdkEventCommunicationSessionTerminated_IHandler:Com.Zebra.Scannercontrol.IDcsSdkApiDelegateInvoker, MauiJavaSdkBinding\n" +
			"n_dcssdkEventFirmwareUpdate:(Lcom/zebra/scannercontrol/FirmwareUpdateEvent;)V:GetDcssdkEventFirmwareUpdate_Lcom_zebra_scannercontrol_FirmwareUpdateEvent_Handler:Com.Zebra.Scannercontrol.IDcsSdkApiDelegateInvoker, MauiJavaSdkBinding\n" +
			"n_dcssdkEventImage:([BI)V:GetDcssdkEventImage_arrayBIHandler:Com.Zebra.Scannercontrol.IDcsSdkApiDelegateInvoker, MauiJavaSdkBinding\n" +
			"n_dcssdkEventScannerAppeared:(Lcom/zebra/scannercontrol/DCSScannerInfo;)V:GetDcssdkEventScannerAppeared_Lcom_zebra_scannercontrol_DCSScannerInfo_Handler:Com.Zebra.Scannercontrol.IDcsSdkApiDelegateInvoker, MauiJavaSdkBinding\n" +
			"n_dcssdkEventScannerDisappeared:(I)V:GetDcssdkEventScannerDisappeared_IHandler:Com.Zebra.Scannercontrol.IDcsSdkApiDelegateInvoker, MauiJavaSdkBinding\n" +
			"n_dcssdkEventVideo:([BI)V:GetDcssdkEventVideo_arrayBIHandler:Com.Zebra.Scannercontrol.IDcsSdkApiDelegateInvoker, MauiJavaSdkBinding\n" +
			"";
		mono.android.Runtime.register ("MauiRfidSample.MVVM.Models.ScannerModel, MauiRfidSample", ScannerModel.class, __md_methods);
	}

	public ScannerModel ()
	{
		super ();
		if (getClass () == ScannerModel.class) {
			mono.android.TypeManager.Activate ("MauiRfidSample.MVVM.Models.ScannerModel, MauiRfidSample", "", this, new java.lang.Object[] {  });
		}
	}

	public void dcssdkEventAuxScannerAppeared (com.zebra.scannercontrol.DCSScannerInfo p0, com.zebra.scannercontrol.DCSScannerInfo p1)
	{
		n_dcssdkEventAuxScannerAppeared (p0, p1);
	}

	private native void n_dcssdkEventAuxScannerAppeared (com.zebra.scannercontrol.DCSScannerInfo p0, com.zebra.scannercontrol.DCSScannerInfo p1);

	public void dcssdkEventBarcode (byte[] p0, int p1, int p2)
	{
		n_dcssdkEventBarcode (p0, p1, p2);
	}

	private native void n_dcssdkEventBarcode (byte[] p0, int p1, int p2);

	public void dcssdkEventBinaryData (byte[] p0, int p1)
	{
		n_dcssdkEventBinaryData (p0, p1);
	}

	private native void n_dcssdkEventBinaryData (byte[] p0, int p1);

	public void dcssdkEventCommunicationSessionEstablished (com.zebra.scannercontrol.DCSScannerInfo p0)
	{
		n_dcssdkEventCommunicationSessionEstablished (p0);
	}

	private native void n_dcssdkEventCommunicationSessionEstablished (com.zebra.scannercontrol.DCSScannerInfo p0);

	public void dcssdkEventCommunicationSessionTerminated (int p0)
	{
		n_dcssdkEventCommunicationSessionTerminated (p0);
	}

	private native void n_dcssdkEventCommunicationSessionTerminated (int p0);

	public void dcssdkEventFirmwareUpdate (com.zebra.scannercontrol.FirmwareUpdateEvent p0)
	{
		n_dcssdkEventFirmwareUpdate (p0);
	}

	private native void n_dcssdkEventFirmwareUpdate (com.zebra.scannercontrol.FirmwareUpdateEvent p0);

	public void dcssdkEventImage (byte[] p0, int p1)
	{
		n_dcssdkEventImage (p0, p1);
	}

	private native void n_dcssdkEventImage (byte[] p0, int p1);

	public void dcssdkEventScannerAppeared (com.zebra.scannercontrol.DCSScannerInfo p0)
	{
		n_dcssdkEventScannerAppeared (p0);
	}

	private native void n_dcssdkEventScannerAppeared (com.zebra.scannercontrol.DCSScannerInfo p0);

	public void dcssdkEventScannerDisappeared (int p0)
	{
		n_dcssdkEventScannerDisappeared (p0);
	}

	private native void n_dcssdkEventScannerDisappeared (int p0);

	public void dcssdkEventVideo (byte[] p0, int p1)
	{
		n_dcssdkEventVideo (p0, p1);
	}

	private native void n_dcssdkEventVideo (byte[] p0, int p1);

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
