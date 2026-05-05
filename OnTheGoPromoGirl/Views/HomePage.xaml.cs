using Newtonsoft.Json;
using OnTheGoPromoGirl.Common;
using OnTheGoPromoGirl.DataAccess;
using OnTheGoPromoGirl.Model;
using OnTheGoPromoGirl.ServiceAccess;

namespace OnTheGoPromoGirl.Views;

public partial class HomePage : ContentPage
{
    public string MasterSync { get; set; }
    private IAndroidOSService _androidOSService;

    public HomePage()
	{
		InitializeComponent();
        _androidOSService = DependencyService.Resolve<IAndroidOSService>();

        IsSyncButtonEnable(true);
        ShowProgressAndText(false);

    }

    private async void MasterSyncBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            bool IsInternetOn = await GeneralClass.CheckInternetConnectivityAsync();
            if (!IsInternetOn)
                return;

            progressBar.Value = 0;
            ShowProgressAndText(true);
            IsSyncButtonEnable(false);

            await ItemSync();
            progressBar.Value = 50;

            await SalesPersonSync();
            progressBar.Value = 100;

            await Task.Delay(1500);

            IsSyncButtonEnable(true); 
            ShowProgressAndText(false);
        }
        catch (Exception ex)
        {
            string errorMessage = string.Format(GeneralClass.Message.failpleasecontact, nameof(MasterSync));

            await GeneralClass.ShowMessageAsync(errorMessage + GeneralClass.Message.singlespace + ex);

            IsSyncButtonEnable(true);
            ShowProgressAndText(false);

        }

    }

    private void IsSyncButtonEnable(bool isEnabled)
    {
        masterSyncBtn.IsEnabled = isEnabled;

    }

    private void ShowProgressAndText(bool isVisible)
    {
        progressBar.IsVisible = isVisible;
        syncLbl.IsVisible = isVisible;

    }

    private async Task ItemSync()
    {        
        try
        {
            var json = await WebService.FetchSyncData(
                WebService.SoapRequest.ItemSync, 
                WebService.Namespace.ItemSync,
                WebService.XName.ItemSync,
                nameof(Item),
                WebService.ASMX.MasterService,
                Preference.GetValueOrDefault(nameof(GeneralClass.RemoteServerURL), GeneralClass.Message.dash),
                default);

            List<Item> list = JsonConvert.DeserializeObject<List<Item>>(json);
            DBAccess.DeleteAll<Item>();
            DBAccess.InsertAll<Item>(list);

        }
        catch(Exception ex)
        {
            await GeneralClass.ShowMessageAsync($"{string.Format(GeneralClass.Message.failpleasecontact, nameof(Item))} {ex}");
            
        }

    }

    private async Task SalesPersonSync()
    {
        try
        {
            var json = await WebService.FetchSyncData(
                WebService.SoapRequest.SalesPersonSync,
                WebService.Namespace.SalesPersonSync,
                WebService.XName.SalesPersonSync,
                nameof(SalesPerson),
                WebService.ASMX.MasterService,
                Preference.GetValueOrDefault(nameof(GeneralClass.RemoteServerURL),
                GeneralClass.Message.dash),
                default);

            List<SalesPerson> list = JsonConvert.DeserializeObject<List<SalesPerson>>(json);
            DBAccess.DeleteAll<SalesPerson>();
            DBAccess.InsertAll<SalesPerson>(list);

        }
        catch (Exception ex)
        {
            await GeneralClass.ShowMessageAsync($"{string.Format(GeneralClass.Message.failpleasecontact, nameof(SalesPerson))} {ex}");

        }

    }

    private async void TransferBtn_Clicked(object sender, EventArgs e)
    {
        try 
        {
            await GeneralClass.NavigateToPage(nameof(TransferPage),(nameof(GeneralClass.IsNewPage),true));

        }
        catch(Exception ex)
        {
            await GeneralClass.ShowMessageAsync(string.Format(GeneralClass.Message.failpleasecontact + ex, string.Empty));

        }

    }
}