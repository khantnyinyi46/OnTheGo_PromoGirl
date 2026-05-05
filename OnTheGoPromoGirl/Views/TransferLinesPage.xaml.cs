using Newtonsoft.Json;
using OnTheGoPromoGirl.Common;
using OnTheGoPromoGirl.DataAccess;
using OnTheGoPromoGirl.Model;
using OnTheGoPromoGirl.ServiceAccess;

namespace OnTheGoPromoGirl.Views;

public partial class TransferLinesPage : ContentPage, IQueryAttributable
{
    private IAndroidOSService _androidOSService;
    private bool isConfirmSync = false;
    public TransferLinesPage()
	{
		InitializeComponent();

	}

    private async void removeBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (collectionView.SelectedItem == null)
                return;

            bool confirm = await DisplayAlert(GeneralClass.Message.removeitem,
                                              GeneralClass.Message.removeitemfromlist,
                                              GeneralClass.Message.remove,
                                              GeneralClass.Message.cancel);
            if (!confirm)
                return;

            Vibration.Vibrate(TimeSpan.FromMilliseconds(50));

            var selected = collectionView.SelectedItem as dynamic;
            if (selected == null)
                return;

            var response = DBAccess.DeleteByOneColumn<TransferLines>(nameof(TransferLines.ItemID), selected.ItemID);
            if (response > 0)
            {
                _ = LoadData();

            }

        }
        catch (Exception ex)
        {
            GeneralClass.ShowMessage($"{string.Format(GeneralClass.Message.failpleasecontact, string.Empty)} {ex}");

        }
    }

    private async void newBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            await GeneralClass.NavigateToPage(nameof(TransferPage),
                                            (nameof(GeneralClass.IsNewPage), false),
                                            (nameof(GeneralClass.Model), nameof(TransferLines)),
                                            (nameof(GeneralClass.AddNew), true)
                                            );
        }
        catch(Exception ex)
        {
            GeneralClass.ShowMessage($"{string.Format(GeneralClass.Message.failpleasecontact, string.Empty)} {ex}");

        }
    }

    private async void browseBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (collectionView.SelectedItem == null)
                return;

                var selected = collectionView.SelectedItem as dynamic;
                string itemid = selected.ItemID;
                string itemname = selected.ItemName;

                var delete = DBAccess.DeleteAll<ItemTemp>();
                ItemTemp itemTemp = new ItemTemp { ItemID = itemid, ItemName = itemname };
                var response = DBAccess.Insert<ItemTemp>(itemTemp);
                if (response > 0)
                {
                    await GeneralClass.NavigateToPage(nameof(TransferPage),
                                                    (nameof(GeneralClass.IsNewPage), false),
                                                    (nameof(GeneralClass.Model), nameof(TransferLines)),
                                                    (nameof(GeneralClass.AddNew), false),
                                                    (nameof(GeneralClass.IsConfirmSync), isConfirmSync));
                }
        }
        catch(Exception ex)
        {
            GeneralClass.ShowMessage($"{string.Format(GeneralClass.Message.failpleasecontact, string.Empty)} {ex}");

        }

    }

    private async void confirmSyncBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            _androidOSService.HideKeyboard();
            collectionView.SelectedItems.Clear();
            var lines = DBAccess.GetAll<TransferLines>();
            if (lines.Count == 0)
                return;

            bool action = await DisplayAlert(GeneralClass.Message.message, GeneralClass.Message.areyousurewanttoconfirm, GeneralClass.Message.yes, GeneralClass.Message.no);
            if (!action)
                return;

            Vibration.Vibrate(TimeSpan.FromMilliseconds(50));

            confirmSyncBtn.IsEnabled = false;
            Transfer header = CreateTransfer();
            string transfer = JsonConvert.SerializeObject(header);

            var orderedLines = lines.OrderBy(line => line.ItemID).ToList();
            string transferline = JsonConvert.SerializeObject(orderedLines);

            confirmSyncBtn.IsEnabled = false;
            newBtn.IsEnabled = false;
            removeBtn.IsEnabled = false;

            isConfirmSync = true;
            Preference.SaveSetting(nameof(GeneralClass.IsConfirmSync), GeneralClass.Message._true);
            var result = DBAccess.Insert<Transfer>(header);
            if (result > 0)
            {
                var json = await WebService.FetchSyncData(
                                    () => WebService.SoapRequest.CreateSync(transfer, transferline),
                                    WebService.Namespace.CreateSync,
                                    WebService.XName.CreateSync,
                                    nameof(Transfer),
                                    WebService.ASMX.TransferService,
                                    Preference.GetValueOrDefault(nameof(GeneralClass.RemoteServerURL), GeneralClass.Message.dash),
                                    default);
                if (json == GeneralClass.Message._true)
                {
                    await GeneralClass.ShowMessageAsync(string.Format(GeneralClass.Message.successfully, GeneralClass.Message.confirmandsync));

                    confirmSyncBtn.IsEnabled = false;

                }
                else
                {
                    DeleteTransferAndRefreshUI();
                    await GeneralClass.ShowMessageAsync(string.Format(GeneralClass.Message.failpleasecontact, GeneralClass.Message.confirmandsync));

                }

            }
            else
            {
                await GeneralClass.ShowMessageAsync(string.Format(GeneralClass.Message.failpleasecontact, GeneralClass.Message.confirmandsync));
                confirmSyncBtn.IsEnabled = true;

            }
        }
        catch(Exception ex)
        {
            DeleteTransferAndRefreshUI();
            GeneralClass.ShowMessage($"{string.Format(GeneralClass.Message.failpleasecontact, string.Empty)} {ex}");

        }

    }

    private void DeleteTransferAndRefreshUI()
    {
        try
        {
            DBAccess.DeleteByOneColumn<Transfer>(nameof(Transfer.TransferID), transferID.Text);
            confirmSyncBtn.IsEnabled = true;
            removeBtn.IsEnabled = true;
            newBtn.IsEnabled = true;
        }
        catch(Exception ex)
        {
            GeneralClass.ShowMessage($"{string.Format(GeneralClass.Message.failpleasecontact, string.Empty)} {ex}");

        }

    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        try
        {
            if (query.TryGetValue(nameof(GeneralClass.IsConfirmSync), out object bool_result_obj) &&
                bool_result_obj is bool)
            {
                bool bool_result = (bool)bool_result_obj;

                InitializeData(bool_result);

            }
        }
        catch(Exception ex)
        {
            GeneralClass.ShowMessage($"{string.Format(GeneralClass.Message.failpleasecontact, string.Empty)} {ex}");
        }
    }

    private void collectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            var selected = e.CurrentSelection.FirstOrDefault();
            if (selected == null)
                return;

            removeBtn.IsVisible = true;
            browseBtn.IsVisible = true;

        }
        catch(Exception ex)
        {
            GeneralClass.ShowMessage($"{string.Format(GeneralClass.Message.failpleasecontact, string.Empty)} {ex}");

        }

    }

    private async Task LoadData()
    {
        try
        {
            var result = await Task.Run(async () =>
            {
                await Task.Delay(500);
                var transferLines = DBAccess.GetAll<TransferLines>();
                var items = DBAccess.GetAll<Item>();

                var combinedData = from tl in transferLines
                                   join item in items on tl.ItemID equals item.ItemID
                                   select new
                                   {
                                       tl.ItemID,
                                       ItemName = item.ItemName,
                                   };

                return combinedData.GroupBy(x => x.ItemID)
                                           .Select(g => g.First())
                                           .ToList();

            });

            Dispatcher.Dispatch(() =>
            {
                transferID.Text = Preference.GetValueOrDefault(nameof(Transfer.TransferID), GeneralClass.Message.dash);

                collectionView.ItemsSource = result;
                if (result.Count > 0)
                {
                    confirmSyncBtn.IsEnabled = true;
                }
                else
                {
                    confirmSyncBtn.IsEnabled = false;

                }
                removeBtn.IsVisible = false;
                browseBtn.IsVisible = false;
            });
        }
        catch(Exception ex)
        {
            GeneralClass.ShowMessage($"{string.Format(GeneralClass.Message.failpleasecontact, string.Empty)} {ex}");

        }

    }

    private Transfer CreateTransfer()
    {
        return new Transfer
        {
            TransferID = transferID.Text,
            TransferDate = DateTime.Now,
            TransferFrom = Preference.GetValueOrDefault(nameof(LoginAuth.SalesPersonID), GeneralClass.Message.dash),
            DeviceID = Preference.GetValueOrDefault(nameof(Device_Model.DeviceID), GeneralClass.Message.dash),
            CreateBy = Preference.GetValueOrDefault(nameof(LoginAuth.UserName), GeneralClass.Message.dash),
            CreateDateTime = DateTime.Now,
            UDF1 = string.Empty,
            UDF2 = string.Empty,
            UDF3 = string.Empty,
            IsSync = false,
            SyncDateTime = DateTime.Now

        };

    }
    private void InitializeData(bool bool_result)
    {
        try
        {
            _androidOSService = DependencyService.Resolve<IAndroidOSService>();
            _ = LoadData();

            if (!bool_result)
                return;

                isConfirmSync = true;

                confirmSyncBtn.IsEnabled = false;
                newBtn.IsEnabled = false;
                removeBtn.IsEnabled = false;
        }
        catch(Exception ex)
        {
            GeneralClass.ShowMessage($"{string.Format(GeneralClass.Message.failpleasecontact, string.Empty)} {ex}");

        }

    }

}