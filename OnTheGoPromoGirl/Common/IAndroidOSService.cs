using OnTheGoPromoGirl.Model;

namespace OnTheGoPromoGirl.Common
{
    public interface IAndroidOSService
    {
        Device_Model GetBasicDeviceInfo();
        void HideKeyboard();

    }

}
