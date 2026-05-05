package crc64edbbe597c7bb739a;


public class MauiComboBoxEntry
	extends crc64b72313ee0f5a60c4.RadMauiEntry
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_dispatchTouchEvent:(Landroid/view/MotionEvent;)Z:GetDispatchTouchEvent_Landroid_view_MotionEvent_Handler\n" +
			"";
		mono.android.Runtime.register ("Telerik.Maui.Controls.ComboBox.MauiComboBoxEntry, Telerik.Maui.Controls", MauiComboBoxEntry.class, __md_methods);
	}


	public MauiComboBoxEntry (android.content.Context p0)
	{
		super (p0);
		if (getClass () == MauiComboBoxEntry.class) {
			mono.android.TypeManager.Activate ("Telerik.Maui.Controls.ComboBox.MauiComboBoxEntry, Telerik.Maui.Controls", "Android.Content.Context, Mono.Android", this, new java.lang.Object[] { p0 });
		}
	}


	public MauiComboBoxEntry (android.content.Context p0, android.util.AttributeSet p1)
	{
		super (p0, p1);
		if (getClass () == MauiComboBoxEntry.class) {
			mono.android.TypeManager.Activate ("Telerik.Maui.Controls.ComboBox.MauiComboBoxEntry, Telerik.Maui.Controls", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android", this, new java.lang.Object[] { p0, p1 });
		}
	}


	public MauiComboBoxEntry (android.content.Context p0, android.util.AttributeSet p1, int p2)
	{
		super (p0, p1, p2);
		if (getClass () == MauiComboBoxEntry.class) {
			mono.android.TypeManager.Activate ("Telerik.Maui.Controls.ComboBox.MauiComboBoxEntry, Telerik.Maui.Controls", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android:System.Int32, System.Private.CoreLib", this, new java.lang.Object[] { p0, p1, p2 });
		}
	}


	public boolean dispatchTouchEvent (android.view.MotionEvent p0)
	{
		return n_dispatchTouchEvent (p0);
	}

	private native boolean n_dispatchTouchEvent (android.view.MotionEvent p0);

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
