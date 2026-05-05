using OnTheGoPromoGirl.Model;

namespace OnTheGoPromoGirl.Common
{
    public class GeneralClass
    {
        public static bool IsNewPage { get; set; }
        public static string PCS { get; set; }
        public static string _Type { get; set; }
        public static string LocalServerURL { get; set; }
        public static string RemoteServerURL { get; set; }
        public static string Model { get; set; }
        public static string AddNew { get; set; }
        public static string IsConfirmSync { get; set; }
        public static string OnThGopromogirlDB
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/OnTheGoPGDB.db3";//OnTheGoPGDB

            }

        }
        public static class Message
        {
            public static string failpleasecontact = "{0} Failed! Please contact support@onthego.com.sg";
            public static string devicesecuritycodemissing = "*Device Security Code is missing.*";
            public static string devicenotactivate = "*Device is not activated.*";
            public static string couldnotsavetodb = "Unable to save the data to database.";
            public static string nointernet = "Sorry, Please check internet connection!";
            public static string incorrectpassword = "The password is incorrect!";
            public static string itemalreadyexist = "{0} already exist !";
            public static string entertransferquantity = "Enter the transfer quantity for Promoter: {0}:";
            public static string transferquantitydialogtitle = "Transfer Quantity";
            public static string edittransferquantity = "Edit Transfer Quantity";
            public static string transferquantityentered = "Transfer quantity entered for Promoter: {0}.";
            public static string remove = "REMOVE";
            public static string removepromoter = "Remove Promoter";
            public static string removepromoterfromlist = "Remove this Promoter from your list ?";
            public static string removeitem = "Remove Item";
            public static string removeitemfromlist = "Remove this item from your list ?";
            public static string itemremovedfrompromotionalitems = "Item: {0} has been removed from your Promotional Items.";
            public static string pleasefiltransferqty= "Please choose Item first to fill Transfer Quantity.";
            public static string success = "Success!";
            public static string successfully = "{0} Successfully !";
            public static string itemandtransferquantitysaved = "Item and Transfer Quantity saved to your Promotional Items.";
            public static string areyousurewanttoconfirm = "Are you sure you want to confirm ?";
            public static string areyousurewanttoleave = "⚠️ Are you sure you want to leave this page?";
            public static string leavingwillcancelcurrenttransfer = "Leaving this page will cancel the current transfer. All items and the assigned promo will be removed.";
            public static string leavepage = "Leave page";
            public static string stayhere = "Stay Here";
            public static string gotohome = "Go to Home";
            public static string unsavedchangesdoyouwanttoleave = "You have unsaved changes. Do you want to Leave ?";
            public static string willberedirectedtohomepage = "You will be redirected to the Home Page. Do you want to proceed?";
            public static string message = "Message";
            public static string ok = "OK";
            public static string forwardslash = "/";
            public static string dash = "-";
            public static string singlespace = " ";
            public static char dot = '.';
            public static string _true = "true";
            public static string _false = "false";
            public static string cancel = "CANCEL";
            public static string save = "SAVE";
            public static string showallrows = "Show All Rows";
            public static string showfilledrows = "Show Filled Rows";
            public static string yes = "Yes";
            public static string no = "No";
            public static string confirmandsync = "Confirm And Sync";
            public static string navigationtargetpagecannotbenullorempty = "Navigation target page cannot be null or empty.";
            public static string invalidargumentfornavigation = "Invalid arguments for navigation.";
            public static string errornavigation = "An error occurred while navigating to the page.";
            public static string errorinternet = "An error occurred while checking internet connectivity.";
            public static string errorsoaprequest = "An error occurred while performing SOAP request.";

        }
        public static async Task ShowMessageAsync(string messageBody)
        {
            await Shell.Current.DisplayAlert(Message.message, $"{messageBody}", Message.ok);

        }
        
        public static void ShowMessage(string messageBody)
        {
            Shell.Current.DisplayAlert(Message.message, $"{messageBody}", Message.ok);

        } 
        
        public static async Task<bool> CheckInternetConnectivityAsync()
        {
            try
            {
                NetworkAccess accessType = Connectivity.Current.NetworkAccess;
                if (accessType == NetworkAccess.Internet)
                {
                    return true;

                }
                else
                {
                    await ShowMessageAsync(Message.nointernet);
                    return false;

                }

            }
            catch(Exception ex)
            {
                await ShowMessageAsync(Message.errorinternet + ex);
                return false;

            }

        }
        
        public static async Task NavigateToPage(ShellNavigationState page, params (string key, object value)[] keyValues)
        {
            try
            {
                if (page == null || string.IsNullOrWhiteSpace(page.Location?.ToString()))
                {
                    throw new ArgumentNullException(nameof(page), Message.navigationtargetpagecannotbenullorempty);
                }

                var keyValuesDict = keyValues.ToDictionary(kv => kv.key, kv => kv.value);
                await Shell.Current.GoToAsync(page, keyValuesDict);

            }
            catch (ArgumentException ex)
            {
                await ShowMessageAsync(Message.invalidargumentfornavigation +ex);

            }
            catch (Exception ex) 
            {
                await ShowMessageAsync(Message.errornavigation +ex);

            }

        }
        public static Type GetTypeFromName(string name)
        {
            switch (name)
            {
                case nameof(Item):
                    return typeof(Item);
                case nameof(SalesPerson):
                    return typeof(SalesPerson);
                case nameof(ItemTemp):
                    return typeof(ItemTemp);
                default:
                    throw new ArgumentException();

            }

        }

    }

}
