using Microsoft.Maui.Handlers;
using Newtonsoft.Json;
using OnTheGoPromoGirl.Common;
using OnTheGoPromoGirl.DataAccess;
using OnTheGoPromoGirl.Model;
using OnTheGoPromoGirl.ServiceAccess;
using MicrosoftDeviceInfo = Microsoft.Maui.Devices.DeviceInfo;

namespace OnTheGoPromoGirl.Views;

public partial class ActivatePage : ContentPage
{ 
	private IAndroidOSService _androidOSService;
	public Device_Model device;
	public ActivatePage()
	{
		InitializeComponent();
		
		_androidOSService = DependencyService.Resolve<IAndroidOSService>();

	} 
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Set up the database if not already done
        await Database.SetUpDb();

        // Check device activation status and navigate accordingly
        await CheckDeviceActivationStatusAndNavigate();

	}
    private async Task CheckDeviceActivationStatusAndNavigate()
	{
		try
		{
			var result = DBAccess.GetAll<DeviceActivation>();
			if(result.Count > 0)
            {
				bool isActivated = result.FirstOrDefault().IsActivated;
				
				string deviceSecurityCode = result.FirstOrDefault().DeviceSecurityCode;
				if (isActivated && !string.IsNullOrEmpty(deviceSecurityCode))
				{
					await GeneralClass.NavigateToPage(nameof(HomePage));
				}

            }
			else
			{
				BusyIndicator.IsBusy = false;
				// If no device activation found, get basic device information
				device = _androidOSService.GetBasicDeviceInfo();

            }

        }
		catch(Exception ex)
		{
			await GeneralClass.ShowMessageAsync(string.Format(GeneralClass.Message.failpleasecontact + GeneralClass.Message.singlespace + ex,nameof(DeviceActivation)));

		}

    }

    private Device_Model BuildDeviceModel(Device_Model device, string deviceID)
	{
		//Build and return a DeviceModel object
		return new Device_Model
        {
			DeviceID = deviceID,
			DeviceName = MicrosoftDeviceInfo.Model,
			DeviceModel = MicrosoftDeviceInfo.Manufacturer,
			DeviceSoftware = System.Environment.OSVersion.ToString(),
			DeviceOSVersion = device.DeviceOSVersion,
			DeviceFirmwareVersion = device.DeviceFirmwareVersion,
			DeviceHardwareVersion = device.DeviceHardwareVersion,

		};

	}

    private string CheckSlash(string serverURL)
    {
		//Ensure the URL ends with a forward slash
		if (!serverURL.EndsWith(GeneralClass.Message.forwardslash))
        {
            serverURL += GeneralClass.Message.forwardslash;
			
        }
		return serverURL;

    }

    private void SaveDeviceActivationSettings(string deviceSecurityCode, string deviceID, string processedURL)
	{
		//Save device activation settings to preferences
        Preference.SaveSetting(nameof(DeviceActivation.DeviceSecurityCode), deviceSecurityCode);
		Preference.SaveSetting(nameof(Device_Model.DeviceID), deviceID);
		Preference.SaveSetting(nameof(GeneralClass.RemoteServerURL), processedURL);
		Preference.SaveSetting(nameof(GeneralClass.LocalServerURL), processedURL);

    }

    private bool AreDeviceSettingsConfigured()
	{
		// Check if device settings are configured in preferences
		string deviceSecurityCode = Preference.GetValueOrDefault(nameof(DeviceActivation.DeviceSecurityCode), GeneralClass.Message.dash);
		string deviceID = Preference.GetValueOrDefault(nameof(Device_Model.DeviceID), GeneralClass.Message.dash);
		string remoteServerURL = Preference.GetValueOrDefault(nameof(GeneralClass.RemoteServerURL), GeneralClass.Message.dash);
		string localServerURL = Preference.GetValueOrDefault(nameof(GeneralClass.LocalServerURL), GeneralClass.Message.dash);

		return !string.IsNullOrEmpty(deviceSecurityCode) && 
			!string.IsNullOrEmpty(deviceID) && 
			!string.IsNullOrEmpty(remoteServerURL) && 
			!string.IsNullOrEmpty(localServerURL);
		
    }

	private bool ValidateInputs(string deviceID, string serverURL)
	{
		// Validate device ID and server URL inputs
        return !string.IsNullOrEmpty(deviceID) && !string.IsNullOrEmpty(serverURL);

    }

	private DeviceActivation CreateDeviceActivation(DeviceActivation deviceActivation, string deviceID, string processedURL)
	{
		return new DeviceActivation
		{
            IsActivated = deviceActivation.IsActivated,
            DeviceSecurityCode = deviceActivation.DeviceSecurityCode,
			ActivatedDate = deviceActivation.ActivatedDate,
			RemoteServerURL = processedURL,
			LocalServerURL = processedURL,
            DeviceID = deviceID
        };
	}

    private async Task HandleDeviceActivationResponse(DeviceActivation deviceActivation, string deviceID, string processedURL)
	{
		try
		{
			string deviceSecurityCode = deviceActivation.DeviceSecurityCode;
			
			bool isActivated = deviceActivation.IsActivated;
			if (!await IsDeviceActivated(deviceSecurityCode, isActivated))
				return;

			DeviceActivation buildDeviceActivation = CreateDeviceActivation(deviceActivation, deviceID, processedURL);
            
			//Insert device activation details into the database
            var response = DBAccess.Insert<DeviceActivation>(buildDeviceActivation);
			if (response > 0)
			{
				//Save device activation settings and navigat to login page if device settigns are configured
				SaveDeviceActivationSettings(deviceSecurityCode, deviceID, processedURL);
				if (AreDeviceSettingsConfigured())
				{
					await GeneralClass.NavigateToPage(nameof(LoginPage));

				}

            }
			else
			{
                await GeneralClass.ShowMessageAsync(string.Format(GeneralClass.Message.failpleasecontact, nameof(DeviceActivation)) + GeneralClass.Message.couldnotsavetodb);

            }

		}
		catch(Exception ex)
		{
            await GeneralClass.ShowMessageAsync(string.Format(GeneralClass.Message.failpleasecontact + GeneralClass.Message.singlespace + ex, nameof(DeviceActivation)));


        }

    }

	private async Task<bool> IsDeviceActivated(string deviceSecurityCode, bool isActivated)
	{
		if(!string.IsNullOrEmpty(deviceSecurityCode) && isActivated)
		{
			return true;

		}
		else
		{
			//Show message if device security code or activation status is missing
			string reasonMessage = string.Empty;
			if (string.IsNullOrEmpty(deviceSecurityCode))
			{

                reasonMessage += GeneralClass.Message.devicesecuritycodemissing;

            }
			else if (!isActivated)
			{
                reasonMessage += GeneralClass.Message.devicenotactivate;

            }

			await GeneralClass.ShowMessageAsync(string.Format(GeneralClass.Message.failpleasecontact, nameof(DeviceActivation)) + reasonMessage) ;
			return false;

        }

    }

    private async void activateBtn_Clicked(object sender, EventArgs e)
    {
		try
		{
			_androidOSService.HideKeyboard();
            Vibration.Vibrate(TimeSpan.FromMilliseconds(50));

            //Disable the activate button to prevent multiple clicks
            activateBtn.IsEnabled = false;

			//Check internet connectivity
			bool IsInternetOn = await GeneralClass.CheckInternetConnectivityAsync();
			if (!IsInternetOn)
				return;
			// Get device ID and server URL from input fields
			string deviceID = deviceIDEntry.Text;
			string serverURL = serverURLEditor.Text;

			// Validate input fields and proceed if valid
			if (device == null && !ValidateInputs(deviceID, serverURL))
				return;
                // Ensure server URL has a trailing slash
                string processedURL = CheckSlash(serverURL);
					
				// Build device model from device information
				Device_Model builtDevice = BuildDeviceModel(device, deviceID);
                string deviceSerialized = JsonConvert.SerializeObject(builtDevice);

                // Perform device activation SOAP request
                var json = await WebService.FetchSyncData(
					() => WebService.SoapRequest.Activate(deviceSerialized),
					WebService.Namespace.Activate,
					WebService.XName.Activate,
                    nameof(DeviceActivation),
					WebService.ASMX.DeviceService,
					processedURL, 
					default);
				if (!string.IsNullOrEmpty(json))
				{
					// Deserialize device activation response and handle accordingly
                    DeviceActivation deviceActivation = JsonConvert.DeserializeObject<DeviceActivation>(json);
                    await HandleDeviceActivationResponse(deviceActivation, deviceID, processedURL);

				}
				else
				{
                    // Show message if activation response is empty
                    await GeneralClass.ShowMessageAsync(string.Format(GeneralClass.Message.failpleasecontact, nameof(DeviceActivation)));

				}

        }
        catch (Exception ex)
		{

            await GeneralClass.ShowMessageAsync(string.Format(GeneralClass.Message.failpleasecontact + GeneralClass.Message.singlespace + ex, nameof(DeviceActivation)));

        }
		finally 
		{ 
			activateBtn.IsEnabled = true; 
		}

    }

    private void serverURLEditor_Focused(object sender, FocusEventArgs e)
    {
        if (sender is Editor editor && e.IsFocused)
        {
            var handler = editor.Handler as EditorHandler;

            if (handler == null)
            {
                return;
            }

#if ANDROID
            var nativeEditor = handler.PlatformView as Android.Widget.EditText;
            nativeEditor?.Post(() => nativeEditor.SetSelection(0, nativeEditor.Text.Length));

#endif
        }
    }
}