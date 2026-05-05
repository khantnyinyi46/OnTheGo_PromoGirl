using Android.Content;
using Android.OS;
using Android.Views.InputMethods;
using OnTheGoPromoGirl.Common;
using OnTheGoPromoGirl.Model;
using Environment = Android.OS.Environment;

namespace OnTheGoPromoGirl.Platforms.Android
{
    public class AndroidOsServiceImpl:IAndroidOSService
    {
        public Device_Model GetBasicDeviceInfo()
        {

            return new Device_Model
            {
                DeviceFirmwareVersion = Build.Product.ToString(),
                DeviceHardwareVersion = Build.Hardware.ToString(),
                DeviceOSVersion = (int)Build.VERSION.SdkInt + GeneralClass.Message.singlespace + Build.VERSION.Release,
                
            };


        }

        public void HideKeyboard()
        {
            var context = Platform.AppContext;

            var inputMethodManager = context.GetSystemService(Context.InputMethodService) as InputMethodManager;
            if (inputMethodManager != null)
            {
                var activity = Platform.CurrentActivity;
                var token = activity.CurrentFocus?.WindowToken;
                inputMethodManager.HideSoftInputFromWindow(token, HideSoftInputFlags.None);

                activity.Window.DecorView.ClearFocus();

            }

        }

    }

}
