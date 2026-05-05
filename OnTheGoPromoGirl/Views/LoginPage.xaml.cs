using Newtonsoft.Json;
using OnTheGoPromoGirl.Common;
using OnTheGoPromoGirl.Model;
using OnTheGoPromoGirl.ServiceAccess;
using System.Reflection;

namespace OnTheGoPromoGirl.Views;
public partial class LoginPage : ContentPage
{
    private IAndroidOSService _androidOSService;
	public LoginPage()
	{
		InitializeComponent();

        _androidOSService = DependencyService.Resolve<IAndroidOSService>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        InitializeUI();

    }

    private async void loginBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            _androidOSService.HideKeyboard();
            Vibration.Vibrate(TimeSpan.FromMilliseconds(50));

            loginBtn.IsEnabled = false;

            bool IsInternetOn = await GeneralClass.CheckInternetConnectivityAsync();
            if (!IsInternetOn)
                return;

            string promoGirlID = PromoGirlIDEntry.Text;
            string password = PasswordEntry.Text;

            if (!ValidateInputs(promoGirlID, password))
                return;

            await PerformLogin(promoGirlID, password);

        }
        catch (Exception ex)
        {
            await GeneralClass.ShowMessageAsync(string.Format(GeneralClass.Message.failpleasecontact + GeneralClass.Message.singlespace + ex,nameof(LoginAuth)));

        }
        finally
        {
            loginBtn.IsEnabled = true;

        }

    }

    private void InitializeUI()
    {
        string deviceID = Preference.GetValueOrDefault(nameof(Device_Model.DeviceID), GeneralClass.Message.dash);
        DeviceIDLbl.Text = deviceID;

        Assembly assembly = typeof(App).Assembly;
        Version version = assembly.GetName().Version;
        versionLbl.Text = "ver: " + version.ToString();

    }

    private bool ValidateInputs(string promoGirlID, string password)
    {
        return !string.IsNullOrEmpty(promoGirlID) && !string.IsNullOrEmpty(password);

    }

    private async Task PerformLogin(string promoGirlID, string password)
    {
        try
        {
            string deviceSecurityCode = Preference.GetValueOrDefault(nameof(DeviceActivation.DeviceSecurityCode), GeneralClass.Message.dash);
            string deviceID = Preference.GetValueOrDefault(nameof(Device_Model.DeviceID), GeneralClass.Message.dash);
            string remoteServerUrl = Preference.GetValueOrDefault(nameof(GeneralClass.RemoteServerURL), GeneralClass.Message.dash);
            
            var json = await WebService.FetchSyncData(
                () => WebService.SoapRequest.LoginAuth(deviceSecurityCode, promoGirlID, deviceID),
                WebService.Namespace.LoginAuth,
                WebService.XName.LoginAuth,
                nameof(LoginAuth),
                WebService.ASMX.LoginService,
                remoteServerUrl, 
                default);
            
            if (!string.IsNullOrEmpty(json))
            {
                LoginAuth loginAuth = JsonConvert.DeserializeObject<LoginAuth>(json);
                await HandleLoginResponse(loginAuth, password);

            }
            else
            {
                await GeneralClass.ShowMessageAsync(string.Format(GeneralClass.Message.failpleasecontact, nameof(LoginAuth)));
            
            }

        }
        catch(Exception ex)
        {
            await GeneralClass.ShowMessageAsync(string.Format(GeneralClass.Message.failpleasecontact + GeneralClass.Message.singlespace + ex, nameof(LoginAuth)));

        }

    }

    private async Task HandleLoginResponse(LoginAuth loginAuth, string password)
    {
        try
        {
            Preference.SaveSetting(nameof(LoginAuth.SalesPersonID), loginAuth.SalesPersonID);
            Preference.SaveSetting(nameof(LoginAuth.UserName), loginAuth.UserName);

            string servicePasswordHash = loginAuth.Password;
            
            bool isPasswordValid = OnTheGo.Library.Security.WP.PasswordHash.ValidatePassword(password, servicePasswordHash);

            if (isPasswordValid)
            {
                await GeneralClass.NavigateToPage(nameof(HomePage));

            }
            else
            {
                await GeneralClass.ShowMessageAsync(GeneralClass.Message.incorrectpassword);

            }

        }
        catch(Exception ex)
        {
            // Show an error message if an exception occurs during the processing of the login response
            await GeneralClass.ShowMessageAsync(string.Format(GeneralClass.Message.failpleasecontact + GeneralClass.Message.singlespace + ex, nameof(LoginAuth)));

        }

    }

}