package mono.com.zebra.commoniolib;


public class usbiomgr_UsbPermissionStatusListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.zebra.commoniolib.usbiomgr.UsbPermissionStatusListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onPermissionDenied:()V:GetOnPermissionDeniedHandler:Com.Zebra.Commoniolib.Usbiomgr+IUsbPermissionStatusListenerInvoker, MauiJavaSdkBinding\n" +
			"n_onPermissionGranted:()V:GetOnPermissionGrantedHandler:Com.Zebra.Commoniolib.Usbiomgr+IUsbPermissionStatusListenerInvoker, MauiJavaSdkBinding\n" +
			"";
		mono.android.Runtime.register ("Com.Zebra.Commoniolib.Usbiomgr+IUsbPermissionStatusListenerImplementor, MauiJavaSdkBinding", usbiomgr_UsbPermissionStatusListenerImplementor.class, __md_methods);
	}

	public usbiomgr_UsbPermissionStatusListenerImplementor ()
	{
		super ();
		if (getClass () == usbiomgr_UsbPermissionStatusListenerImplementor.class) {
			mono.android.TypeManager.Activate ("Com.Zebra.Commoniolib.Usbiomgr+IUsbPermissionStatusListenerImplementor, MauiJavaSdkBinding", "", this, new java.lang.Object[] {  });
		}
	}

	public void onPermissionDenied ()
	{
		n_onPermissionDenied ();
	}

	private native void n_onPermissionDenied ();

	public void onPermissionGranted ()
	{
		n_onPermissionGranted ();
	}

	private native void n_onPermissionGranted ();

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
