using OnTheGoPromoGirl.Common;
using OnTheGoPromoGirl.DataAccess;
using OnTheGoPromoGirl.Model;

namespace OnTheGoPromoGirl.Views;
public partial class SearchPage : ContentPage
{
    private IAndroidOSService _androidOSService;
    public SearchPage()
	{
		InitializeComponent();
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        InitializeData();

    }
    
    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            _androidOSService.HideKeyboard();
            Vibration.Vibrate(TimeSpan.FromMilliseconds(50));

            if (collectionView.SelectedItem is Item selectedItem)
            {
                ItemTemp itemTemp = CreateItemTemp(selectedItem);

                await Task.Run(() => {
                    DBAccess.DeleteAll<ItemTemp>();
                    DBAccess.Insert<ItemTemp>(itemTemp);

                });

            }
            await GeneralClass.NavigateToPage(nameof(TransferPage),
                                                    (nameof(GeneralClass.IsNewPage), false),
                                                    (nameof(GeneralClass.Model), nameof(Item))
                                                    );

        }
        catch(Exception ex)
        {
            await GeneralClass.ShowMessageAsync(string.Format(GeneralClass.Message.failpleasecontact + ex,nameof(Item)));

        }

    }
    private async void searchBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            _androidOSService.HideKeyboard();
            Vibration.Vibrate(TimeSpan.FromMilliseconds(50));

            string value = "%" + searchEntry.Text + "%";
            
            var result = DBAccess.GetByColumn<Item>(nameof(Item.ItemName), value);
            collectionView.ItemsSource = result;
            rowNo.Text = result.Count.ToString();
            searchEntry.Text = string.Empty;

        }
        catch(Exception ex)
        {
            await GeneralClass.ShowMessageAsync(string.Format(GeneralClass.Message.failpleasecontact + ex, nameof(Item)));

        }

    }

    private async void InitializeData()
    {
        try
        {
            _androidOSService = DependencyService.Resolve<IAndroidOSService>();
            _ = LoadData();

        }
        catch(Exception ex)
        {
            await GeneralClass.ShowMessageAsync(string.Format(GeneralClass.Message.failpleasecontact + ex, nameof(Item)));

        }

    }

    private async Task LoadData()
    {
        try
        {
            var result = await Task.Run(async () =>
            {
                await Task.Delay(500);
                return DBAccess.GetAll<Item>();
            });

            Dispatcher.Dispatch(() =>
            {
                collectionView.ItemsSource = result;
                title.Text = nameof(Item);
                rowNo.Text = result.Count().ToString();

            });
        }
        catch(Exception ex)
        {
            await GeneralClass.ShowMessageAsync(
                string.Format(GeneralClass.Message.failpleasecontact + GeneralClass.Message.singlespace + ex, nameof(Item))
                );
        }

    }

    private ItemTemp CreateItemTemp(Item item)
    {
        return new ItemTemp
        {
            ItemID = item.ItemID,
            ItemName = item.ItemName,
            SegmentID = item.SegmentID,
            SegmentName = item.SegmentName,
            UDF1 = item.UDF1,
            UDF2 = item.UDF2,
            UDF3 = item.UDF3
        };
    }

}