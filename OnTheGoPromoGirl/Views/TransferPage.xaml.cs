using OnTheGoPromoGirl.Common;
using OnTheGoPromoGirl.DataAccess;
using OnTheGoPromoGirl.Model;

namespace OnTheGoPromoGirl.Views;

public partial class TransferPage : ContentPage,IQueryAttributable
{
    private bool isFilterActive = false;
    private bool isConfirmSync = false;
    private string functionName;
    private string param1;
    private IAndroidOSService _androidOSService;
    private readonly Dictionary<string, Func<Task>> _functionMap;
    private int indexTemp;
    private List<CombinedData> temp;
    public class CombinedData
    {
        public string SalesPersonID { get; set; }
        public string SalesPersonName { get;set; }
        public decimal TransferQty { get; set; }
    }
    public TransferPage()
	{
		InitializeComponent();

        _functionMap = new Dictionary<string, Func<Task>>
        {
            {nameof(GetSearchItemsAsync), () => GetSearchItemsAsync(param1) },
            {nameof(ShowAllRows), ShowAllRows },
            {nameof(ShowFillRows), ShowFillRows }
        };

	}
    
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        try
        {
            bool newPage = TryGetBoolean(query, nameof(GeneralClass.IsNewPage));
            string model = TryGetString(query, nameof(GeneralClass.Model));
            bool addNew = TryGetBoolean(query, nameof(GeneralClass.AddNew));
            bool confirmSync = TryGetBoolean(query, nameof(GeneralClass.IsConfirmSync));
            _ =InitializeData(newPage, model, addNew, confirmSync);

        }
        catch (Exception ex)
        {
            GeneralClass.ShowMessage($"{string.Format(GeneralClass.Message.failpleasecontact, string.Empty)} {ex}");

        }

    }
    
    private async void itemSearch_Clicked(object sender, EventArgs e)
    {
        try
        {
            _androidOSService.HideKeyboard();
            Vibration.Vibrate(TimeSpan.FromMilliseconds(50));
            await GeneralClass.NavigateToPage(nameof(SearchPage));
        }
        catch (Exception ex)
        {
            await GeneralClass.ShowMessageAsync($"{string.Format(GeneralClass.Message.failpleasecontact, nameof(TransferLines))} {ex}");

        }

    }

    private async void promoSearch_Clicked(object sender, EventArgs e)
    {
        try
        {
            _androidOSService.HideKeyboard();
            Vibration.Vibrate(TimeSpan.FromMilliseconds(50));
            string value = "%" + searchEntry.Text + "%";

            functionName = nameof(GetSearchItemsAsync);
            param1 = value;

            await GetSearchItemsAsync(value);
        }
        catch (Exception ex)
        {
            await GeneralClass.ShowMessageAsync($"{string.Format(GeneralClass.Message.failpleasecontact, nameof(SalesPerson))} {ex}");

        }
    }

    private async Task HandleTransferQtyInput(CombinedData selected, string initalValue)
    {
        try
        {
            var tempSelected = temp.FirstOrDefault(e => e.SalesPersonID == selected.SalesPersonID);
            indexTemp = temp.IndexOf(tempSelected);

            var result = await Shell.Current.DisplayPromptAsync(
                    GeneralClass.Message.transferquantitydialogtitle,
                    string.Format(GeneralClass.Message.entertransferquantity, selected.SalesPersonName),
                    GeneralClass.Message.save,
                    GeneralClass.Message.cancel,
                    "0",
                    -1,
                    Keyboard.Numeric,
                    initalValue);

            if (!decimal.TryParse(result, out decimal transferQuantity) && transferQuantity == 0)
                return;

            Vibration.Vibrate(TimeSpan.FromMilliseconds(50));

            var item = await Task.Run(() => DBAccess.GetAll<ItemTemp>().FirstOrDefault());
            if (item == null)
                return;

            await DeleteExistingTransferLinesAsync(item.ItemID, selected.SalesPersonID);

            TransferLines transferLine = CreateTransferLines(item.ItemID, selected.SalesPersonID,transferQuantity);

            var insertResponse = await InsertTransferLine(transferLine);
            if (insertResponse > 0)
            {
                await HandlePostInsertActionsAsync(item.ItemID);
                
            }
            else
            {
                await GeneralClass.ShowMessageAsync($"{string.Format(GeneralClass.Message.failpleasecontact, nameof(TransferLines))}");

            }
        }
        catch (Exception ex)
        {
            await GeneralClass.ShowMessageAsync($"{string.Format(GeneralClass.Message.failpleasecontact, nameof(TransferLines))} {ex}");

        }

    }

    //private async Task ScrollTest(CombinedData selected)
    //{
    //    var lists = collectionView.ItemsSource as List<CombinedData>;
    //    //int count = 0;
    //    //foreach(CombinedData list in lists)
    //    //{
    //    //    if(list.SalesPersonID == selected.SalesPersonID)
    //    //    {
    //    //        await DisplayAlert("",count.ToString(),"OK");
    //    //        collectionView.ScrollTo(count, position:ScrollToPosition.MakeVisible);
    //    //        return;
    //    //    }
    //    //    count++;
    //    //}
    //    //return;

    //}

    private async Task DeleteExistingTransferLinesAsync(string itemId, string salesPersonId)
    {
        await Task.Run(() => DBAccess.DeleteByTwoColumn<TransferLines>(
            nameof(TransferLines.ItemID), itemId,
            nameof(TransferLines.TransferTo), salesPersonId));
    }

    private async Task<int> InsertTransferLine(TransferLines transferLine)
    {
        return await Task.Run(() => DBAccess.Insert<TransferLines>(transferLine));

    }

    private async Task HandlePostInsertActionsAsync(string itemId)
    {
        try
        {
            var transferLines = await Task.Run(() => DBAccess.GetAll<TransferLines>());
            var relatedTransfers = transferLines.FindAll(e => e.ItemID == itemId);
            if (relatedTransfers.Count == 1)
            {
                await DisplayAlert(GeneralClass.Message.success, GeneralClass.Message.itemandtransferquantitysaved, GeneralClass.Message.ok);

            }
            await InvokeFunction();
        }
        catch(Exception ex)
        {
            await GeneralClass.ShowMessageAsync($"{string.Format(GeneralClass.Message.failpleasecontact, nameof(TransferLines))} {ex}");

        }

    }

    private async Task InvokeFunction()
    {
        if (_functionMap.TryGetValue(functionName, out var function))
        {
            await function();
        }
        else
        {
            throw new InvalidOperationException($"Function '{functionName}' not found.");
        }
    }

    private string GetErrorMessage<T>()
    {
        return string.Format(GeneralClass.Message.failpleasecontact, nameof(T));

    }
    
    private void SetFunctionDefaults()
    {
        functionName = nameof(ShowAllRows);
        param1 = string.Empty;
    }

    private async Task HandleTransferLinesAsync(bool addNew, bool confirmSync)
    {
        try
        {
            if (addNew)
            {
                DBAccess.DeleteAll<ItemTemp>();
                toggleFilterBtn.IsVisible = false;

                _ = LoadDataAsync();
                SetFunctionDefaults();

            }
            else
            {
                await HandleBrowseNConfirmSyncAsync(confirmSync);

            }
        }
        catch(Exception ex)
        {
            await GeneralClass.ShowMessageAsync($"{string.Format(GeneralClass.Message.failpleasecontact, nameof(TransferLines))} {ex}");

        }

    }
    
    private async Task HandleBrowseNConfirmSyncAsync(bool confirmSync)
    {
        try
        {
            if (confirmSync)
            {
                collectionView.SelectionMode = SelectionMode.None;
                isConfirmSync = true;
            }

            var item = await Task.Run(() => DBAccess.GetAll<ItemTemp>().FirstOrDefault());

            isFilterActive = false;
            OnToggleFilterButtonClicked(null, EventArgs.Empty);

            Dispatcher.Dispatch(() =>
            {
                itemName.Text = item.ItemName;
                itemSearchBtn.IsVisible = false;
                emptyLbl_1.IsVisible = false;
                emptyLbl_2.IsVisible = false;
                toggleFilterBtn.IsVisible = true;
            });
        }
        catch(Exception ex)
        {
            await GeneralClass.ShowMessageAsync($"{string.Format(GeneralClass.Message.failpleasecontact, nameof(TransferLines))} {ex}");

        }
    }

    private async Task InitializeExistingPageAsync(string model, bool addNew, bool confirmSync)
    {
        try
        {
            if (model == nameof(TransferLines))
            {
                await HandleTransferLinesAsync(addNew, confirmSync);

            }
            else //model is Item -- from SearchPage
            {
                toggleFilterBtn.IsVisible = false;

                _ = LoadDataAsync();
                SetFunctionDefaults();

            }
        }
        catch(Exception ex)
        {
            await GeneralClass.ShowMessageAsync($"{string.Format(GeneralClass.Message.failpleasecontact, nameof(TransferLines))} {ex}");

        }
    }

    private async Task InitializeData(bool newPage, string model, bool addNew, bool confirmSync)
    {
        try
        {
            _androidOSService = DependencyService.Resolve<IAndroidOSService>();
            if (newPage)
            {
                _ = NewPageAsync();
                _ = LoadDataAsync();
                SetFunctionDefaults();

            }
            else
            {
                transferID.Text = Preference.GetValueOrDefault(nameof(Transfer.TransferID), GeneralClass.Message.dash);
                await InitializeExistingPageAsync(model, addNew, confirmSync);
            }

        }

        catch(Exception ex)
        {
            await GeneralClass.ShowMessageAsync($"{string.Format(GeneralClass.Message.failpleasecontact, nameof(TransferLines))} {ex}");

        }

    }

    private async Task NewPageAsync()
    {
        try
        {
            await Task.Run(async () =>
            {
                await Task.Delay(3000);
                DBAccess.DeleteAll<TransferLines>();
                DBAccess.DeleteAll<ItemTemp>();
            });

            Dispatcher.Dispatch(() =>
            {
                toggleFilterBtn.IsVisible = false;
                Preference.SaveSetting(nameof(GeneralClass.IsConfirmSync), GeneralClass.Message._false);
                transferID.Text = DateTime.Now.ToString("yyyyMMddhhmmss");
                Preference.SaveSetting(nameof(Transfer.TransferID), transferID.Text);
            });
        }
        catch(Exception ex)
        {
            await GeneralClass.ShowMessageAsync($"{string.Format(GeneralClass.Message.failpleasecontact, nameof(TransferLines))} {ex}");
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            List<TransferLines> transferLines = new List<TransferLines>();
            List<ItemTemp> item = new List<ItemTemp>();

            var combinedData = await Task.Run(async () =>
            {
                await Task.Delay(3000);
                item = DBAccess.GetAll<ItemTemp>();
                transferLines = DBAccess.GetAll<TransferLines>();
                var salesPerson = DBAccess.GetAll<SalesPerson>();
                var itemTemp = item.FirstOrDefault();
                string itemTempID = itemTemp?.ItemID ?? string.Empty;

                return from sp in salesPerson
                       join tl in transferLines
                   on sp.SalesPersonID equals tl.TransferTo into tlGroup
                       from tl in tlGroup.Where(t => t.ItemID == itemTempID).DefaultIfEmpty()
                       select new CombinedData
                       {
                           SalesPersonID = sp.SalesPersonID,
                           SalesPersonName = sp.SalesPersonName,
                           TransferQty = tl?.TransferQty ?? 0
                       };
            });

            Dispatcher.Dispatch(() =>
            {
                collectionView.ItemsSource = combinedData.ToList();
                rowNo.Text = combinedData.Count().ToString();

                temp = combinedData.ToList();

                if (item.Count > 0)
                {
                    itemName.Text = item.FirstOrDefault().ItemName;
                    var lines = transferLines.Find(e => e.ItemID == item.FirstOrDefault().ItemID);
                    if (lines != null)
                    {
                        toggleFilterBtn.IsVisible = true;

                    }
                }
            });
        }
        catch(Exception ex)
        {
            await GeneralClass.ShowMessageAsync($"{string.Format(GeneralClass.Message.failpleasecontact, nameof(TransferLines))} {ex}");

        }

    }
    
    private async Task GetSearchItemsAsync(string searchItem)
    {
        try
        {
            var salesPersonTask = Task.Run(() => DBAccess.GetByColumn<SalesPerson>(nameof(SalesPerson.SalesPersonName), searchItem));
            var transferLinesTask = Task.Run(() => DBAccess.GetAll<TransferLines>());
            var itemTempTask = Task.Run(() => DBAccess.GetAll<ItemTemp>().FirstOrDefault());

            await Task.WhenAll(salesPersonTask, transferLinesTask, itemTempTask);

            var salesPerson = await salesPersonTask;
            var transferLines = await transferLinesTask;
            var itemTemp = await itemTempTask;

            string itemTempID = itemTemp?.ItemID ?? string.Empty;

            var combinedData = from sp in salesPerson
                               join tl in transferLines
                               on sp.SalesPersonID equals tl.TransferTo into tlGroup
                               from tl in tlGroup.Where(t => t.ItemID == itemTempID).DefaultIfEmpty()
                               select new CombinedData
                               {
                                   SalesPersonID = sp.SalesPersonID,
                                   SalesPersonName = sp.SalesPersonName,
                                   TransferQty = tl?.TransferQty ?? 0
                               };

            Dispatcher.Dispatch(() =>
            {
                temp = new List<CombinedData>();
                temp = combinedData.ToList();

                collectionView.ItemsSource = combinedData;

                rowNo.Text = combinedData.Count().ToString();
                searchEntry.Text = string.Empty;

                collectionView.ScrollTo(indexTemp, position: ScrollToPosition.Start, animate: false);

            });
        }
        catch(Exception ex)
        {
            await GeneralClass.ShowMessageAsync($"{string.Format(GeneralClass.Message.failpleasecontact, nameof(TransferLines))} {ex}");

        }

    }
    
    private async void searchEntry_Completed(object sender, EventArgs e)
    {
        try
        {
            _androidOSService.HideKeyboard();
            string value = "%" + searchEntry.Text + "%";

            functionName = nameof(GetSearchItemsAsync);
            param1 = value;

            await GetSearchItemsAsync(value);
        
        }
        catch (Exception ex)
        {
            await GeneralClass.ShowMessageAsync($"{string.Format(GeneralClass.Message.failpleasecontact, nameof(SalesPerson))} {ex}");

        }

    }

    private void OnToggleFilterButtonClicked(object sender, EventArgs e)
    {
        try
        {
            _androidOSService.HideKeyboard();

            isFilterActive = !isFilterActive;
            toggleFilterBtn.Text = isFilterActive ? GeneralClass.Message.showallrows : GeneralClass.Message.showfilledrows;

            if (isFilterActive)
            {
                _ = ShowFillRows();
                functionName = nameof(ShowFillRows);
                param1 = string.Empty;
            }
            else
            {
                _ = ShowAllRows();
                functionName = nameof(ShowAllRows);
                param1 = string.Empty;
            }
        }
        catch(Exception ex)
        {
            GeneralClass.ShowMessage($"{string.Format(GeneralClass.Message.failpleasecontact, nameof(TransferLines))} {ex}");
        }
    }
    
    private async Task ShowAllRows()
    {
        try
        {
            var transferLinesTask = Task.Run(() => DBAccess.GetAll<TransferLines>());
            var salesPersonTask = Task.Run(() => DBAccess.GetAll<SalesPerson>());
            var itemTempTask = Task.Run(() => DBAccess.GetAll<ItemTemp>().FirstOrDefault());

            await Task.WhenAll(transferLinesTask, salesPersonTask, itemTempTask);

            var transferLines = await transferLinesTask;
            var salesPerson = await salesPersonTask;
            var itemTemp = await itemTempTask;

            string itemTempID = itemTemp?.ItemID ?? string.Empty;

            var combinedData = from sp in salesPerson
                               join tl in transferLines
                               on sp.SalesPersonID equals tl.TransferTo into tlGroup
                               from tl in tlGroup.Where(t => t.ItemID == itemTempID).DefaultIfEmpty()
                               select new CombinedData
                               {
                                   SalesPersonID = sp.SalesPersonID,
                                   SalesPersonName = sp.SalesPersonName,
                                   TransferQty = tl?.TransferQty ?? 0
                               };
            Dispatcher.Dispatch(() =>
            {
                temp = new List<CombinedData>();
                temp = combinedData.ToList();

                collectionView.ItemsSource = combinedData;
                rowNo.Text = combinedData.Count().ToString();
                searchEntry.IsVisible = true;
                promoSearchBtn.IsVisible = true;

                collectionView.ScrollTo(indexTemp, position: ScrollToPosition.Start, animate: false);


            });

        }
        catch(Exception ex)
        {
            await GeneralClass.ShowMessageAsync($"{string.Format(GeneralClass.Message.failpleasecontact, nameof(TransferLines))} {ex}");

        }

    }
    
    private async Task ShowFillRows()
    {
        try
        {
            var transferLinesTask = Task.Run(() => DBAccess.GetAll<TransferLines>());
            var salesPersonTask = Task.Run(() => DBAccess.GetAll<SalesPerson>());
            var itemTempTask = Task.Run(() => DBAccess.GetAll<ItemTemp>().FirstOrDefault());

            await Task.WhenAll(transferLinesTask, salesPersonTask, itemTempTask);

            var itemTemp = await itemTempTask;  
            var salesPerson = await salesPersonTask;
            var transferLines = await transferLinesTask;

            if (itemTemp != null)
            {
                string itemTempID = itemTemp?.ItemID ?? string.Empty;

                var combinedData = from sp in salesPerson
                                   join tl in transferLines
                                   on sp.SalesPersonID equals tl.TransferTo into tlGroup
                                   from tl in tlGroup.Where(t => t.ItemID == itemTempID)
                                   select new CombinedData
                                   {
                                       SalesPersonID = sp.SalesPersonID,
                                       SalesPersonName = sp.SalesPersonName,
                                       TransferQty = tl?.TransferQty ?? 0
                                   };
                Dispatcher.Dispatch(() =>
                {
                    temp = new List<CombinedData>();
                    temp = combinedData.ToList();

                    collectionView.ItemsSource = combinedData;
                    rowNo.Text = combinedData.Count().ToString();

                    searchEntry.IsVisible = false;
                    promoSearchBtn.IsVisible = false;

                    collectionView.ScrollTo(indexTemp, position: ScrollToPosition.Start, animate: false);

                });
            }
        }
        catch(Exception ex)
        {
            await GeneralClass.ShowMessageAsync($"{string.Format(GeneralClass.Message.failpleasecontact, nameof(TransferLines))} {ex}");

        }
    }

    private async void forwardBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            await GeneralClass.NavigateToPage(nameof(TransferLinesPage),
                                    (nameof(GeneralClass.IsConfirmSync), isConfirmSync)
                                    );
        }
        catch(Exception ex)
        {
            await GeneralClass.ShowMessageAsync($"{string.Format(GeneralClass.Message.failpleasecontact, nameof(TransferLines))} {ex}");

        }

    }

    private async Task HandleRemovePromoterAsync(CombinedData selected, ItemTemp itemTemp)
    {
        try
        {
            var tempSelected = temp.FirstOrDefault(e => e.SalesPersonID == selected.SalesPersonID);
            indexTemp = temp.IndexOf(tempSelected);

            DBAccess.DeleteByTwoColumn<TransferLines>(
                                        nameof(TransferLines.ItemID),
                                        itemTemp.ItemID,
                                        nameof(TransferLines.TransferTo),
                                        selected.SalesPersonID
                                        );

            var transferLines = DBAccess.GetAll<TransferLines>();
            var result = transferLines.Find(e => e.ItemID == itemTemp.ItemID);
            if (result == null)
            {
                await DisplayAlert(GeneralClass.Message.message,
                    string.Format(GeneralClass.Message.itemremovedfrompromotionalitems, itemTemp.ItemName),
                    GeneralClass.Message.ok);

                isFilterActive = true;
                OnToggleFilterButtonClicked(null, EventArgs.Empty);

            }
            else
            {
                await InvokeFunction();
            }
        }
        catch(Exception ex)
        {
            await GeneralClass.ShowMessageAsync($"{string.Format(GeneralClass.Message.failpleasecontact, nameof(TransferLines))} {ex}");

        }

    }

    private async void collectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(itemName.Text))
            {
                await DisplayAlert(GeneralClass.Message.message, GeneralClass.Message.pleasefiltransferqty, GeneralClass.Message.ok);
                return;

            }

            var selected = e.CurrentSelection.FirstOrDefault() as CombinedData;
            if(selected == null)
            return;

            var tr = DBAccess.GetAll<TransferLines>();
            var itemTemp = DBAccess.GetAll<ItemTemp>().FirstOrDefault();

            if (tr.Count > 0 && itemTemp != null)
            {
                var tr_result = tr.Find(e => e.TransferTo == selected.SalesPersonID && e.ItemID == itemTemp.ItemID);
                if (tr_result != null)
                {
                    string initalValue = tr_result.TransferQty.ToString();
                    string action = await DisplayActionSheet(
                                    string.Format(GeneralClass.Message.transferquantityentered, selected.SalesPersonName),
                                    GeneralClass.Message.cancel, null, GeneralClass.Message.edittransferquantity, GeneralClass.Message.removepromoter);
                    if (action == GeneralClass.Message.edittransferquantity)
                    {
                        await HandleTransferQtyInput(selected, initalValue);
                    }
                    else if (action == GeneralClass.Message.removepromoter)
                    {
                        bool confirm = await DisplayAlert(
                                        GeneralClass.Message.removepromoter,
                                        GeneralClass.Message.removepromoterfromlist,
                                        GeneralClass.Message.remove,
                                        GeneralClass.Message.cancel);
                        if (confirm)
                        {
                            Vibration.Vibrate(TimeSpan.FromMilliseconds(50));

                            await HandleRemovePromoterAsync(selected, itemTemp);
                        }
                    }
                }
                else
                {
                    await HandleTransferQtyInput(selected, string.Empty);
                }
            }
            else
            {
                await HandleTransferQtyInput(selected, string.Empty);
            }

            UpdateToggleFilterButtonVisibility();

        }
        catch (Exception ex)
        {
            await GeneralClass.ShowMessageAsync($"{string.Format(GeneralClass.Message.failpleasecontact, nameof(TransferLines))} {ex}");

        }

    }

    private void UpdateToggleFilterButtonVisibility()
    {
        try
        {
            var _tr = DBAccess.GetAll<TransferLines>();
            var it = DBAccess.GetAll<ItemTemp>().FirstOrDefault();
            if (it == null)
                return;

                var list = _tr.Find(e => e.ItemID == it.ItemID);
                if (list != null)
                {
                    toggleFilterBtn.IsVisible = true;

                }
                else
                {
                    toggleFilterBtn.IsVisible = false;

                }
        }
        catch(Exception ex)
        {
            GeneralClass.ShowMessage($"{string.Format(GeneralClass.Message.failpleasecontact, nameof(TransferLines))} {ex}");

        }
    }

    private TransferLines CreateTransferLines(string ItemID, string SalesPersonID, decimal transferQty)
    {
        return new TransferLines
        {
            TransferID = transferID.Text,
            ItemID = ItemID,
            TransferTo = SalesPersonID,
            TransferQty = transferQty,
            UomID = nameof(GeneralClass.PCS),
            CreateDateTime = DateTime.Now

        };

    }
    
    private bool TryGetBoolean(IDictionary<string, object> query, string key)
    {
        return query.TryGetValue(key, out var value) && value is bool boolValue && boolValue;

    }

    private string TryGetString(IDictionary<string, object> query, string key)
    {
        return query.TryGetValue(key, out var value) ? value as string : string.Empty;
    }



    //if (!newPage)//NewPage is false
    //{
    //    transferID.Text = Preference.GetValueOrDefault(nameof(Transfer.TransferID), GeneralClass.Message.dash);
    //    if (model == nameof(TransferLines))
    //    {
    //        if (addNew)//add btn is clicked from TransferLines Page
    //        {
    //            DBAccess.DeleteAll<ItemTemp>();
    //            toggleFilterBtn.IsVisible = false;

    //            _ = LoadDataAsync();
    //            functionName = nameof(ShowAllRows);
    //            param1 = string.Empty;

    //        }
    //        else//browse btn is clicked from TransferLines Page
    //        {
    //            //addBtn.IsVisible = false;
    //            if (confirmSync)
    //            {
    //                collectionView.SelectionMode = SelectionMode.None;
    //                isConfirmSync = true;
    //            }

    //            var item = DBAccess.GetAll<ItemTemp>().FirstOrDefault();

    //            isFilterActive = false;
    //            OnToggleFilterButtonClicked(null, EventArgs.Empty);

    //            Dispatcher.Dispatch(() =>
    //            {
    //                itemName.Text = item.ItemName;
    //                itemSearchBtn.IsVisible = false;
    //                emptyLbl_1.IsVisible = false;
    //                emptyLbl_2.IsVisible = false;
    //                toggleFilterBtn.IsVisible = true;
    //            });

    //        }
    //    }
    //    else //model is Item -- from SearchPage
    //    {
    //        toggleFilterBtn.IsVisible = false;

    //        _ = LoadDataAsync();
    //        functionName = nameof(ShowAllRows);
    //        param1 = string.Empty;


    //    }
    //}
    //else//NewPage is true
    //{
    //    _ = NewPageAsync();

    //    _ = LoadDataAsync();
    //    functionName = nameof(ShowAllRows);
    //    param1 = string.Empty;
    //}

    //if (!string.IsNullOrEmpty(itemName.Text))
    //{
    //    var tr = DBAccess.GetAll<TransferLines>();
    //    var selected = e.CurrentSelection.FirstOrDefault() as CombinedData;
    //    if (selected != null)
    //    {
    //        if (tr.Count > 0)
    //        {
    //            var itemTemp = DBAccess.GetAll<ItemTemp>().FirstOrDefault();
    //            if (itemTemp != null)
    //            {

    //                var tr_result = tr.Find(e => e.TransferTo == selected.SalesPersonID &&
    //                                        e.ItemID == itemTemp.ItemID);
    //                if (tr_result != null)
    //                {
    //                    string initalValue = tr_result.TransferQty.ToString();

    //                    string action = await DisplayActionSheet(
    //                        string.Format(GeneralClass.Message.transferquantityentered, selected.SalesPersonName),
    //                        GeneralClass.Message.cancel, null, GeneralClass.Message.edittransferquantity, GeneralClass.Message.removepromoter);
    //                    if (action == GeneralClass.Message.edittransferquantity)
    //                    {
    //                        await HandleTransferQtyInput(selected, initalValue);
    //                    }
    //                    else if (action == GeneralClass.Message.removepromoter)
    //                    {
    //                        bool confirm = await DisplayAlert(
    //                                        GeneralClass.Message.removepromoter,
    //                                        GeneralClass.Message.removepromoterfromlist,
    //                                        GeneralClass.Message.remove,
    //                                        GeneralClass.Message.cancel
    //                                    );
    //                        if (confirm)
    //                        {
    //                            var item = DBAccess.GetAll<ItemTemp>();
    //                            if (item.Count > 0)
    //                            {
    //                                DBAccess.DeleteByTwoColumn<TransferLines>(
    //                                                            nameof(TransferLines.ItemID),
    //                                                            item.FirstOrDefault().ItemID,
    //                                                            nameof(TransferLines.TransferTo),
    //                                                            selected.SalesPersonID
    //                                                            );

    //                                var transferLines = DBAccess.GetAll<TransferLines>();
    //                                var result = transferLines.Find(e=>e.ItemID == item.FirstOrDefault().ItemID);
    //                                if (result == null)
    //                                {
    //                                    await DisplayAlert(GeneralClass.Message.message,
    //                                        string.Format(GeneralClass.Message.itemremovedfrompromotionalitems, item.FirstOrDefault().ItemName),
    //                                        GeneralClass.Message.ok);

    //                                    isFilterActive = true;
    //                                    OnToggleFilterButtonClicked(null, EventArgs.Empty);

    //                                }
    //                                else 
    //                                {
    //                                    await InvokeFunction();
    //                                }

    //                                //isFilterActive = true;
    //                                //OnToggleFilterButtonClicked(null, EventArgs.Empty);

    //                            }

    //                        }

    //                    }

    //                }
    //                else
    //                {
    //                    await HandleTransferQtyInput(selected, string.Empty);

    //                }
    //            }

    //        }
    //        else
    //        {
    //            await HandleTransferQtyInput(selected, string.Empty);

    //        }

    //    }
    //    var _tr = DBAccess.GetAll<TransferLines>();
    //    var it = DBAccess.GetAll<ItemTemp>().FirstOrDefault();
    //    if (it != null)
    //    {
    //        var list = _tr.Find(e => e.ItemID == it.ItemID);
    //        if (list != null)
    //        {
    //            toggleFilterBtn.IsVisible = true;

    //        }
    //        else
    //        {
    //            toggleFilterBtn.IsVisible = false;

    //        }
    //    }

    //    
    //}
    //else
    //{
    //    await DisplayAlert(GeneralClass.Message.message, GeneralClass.Message.pleasefiltransferqty, GeneralClass.Message.ok);

    //}
}