using OnTheGoPromoGirl.Common;
using OnTheGoPromoGirl.Views;
using OnTheGoPromoGirl.Model;
using OnTheGoPromoGirl.DataAccess;

namespace OnTheGoPromoGirl.Platforms.Android
{
    public class NavigationService
    {

        public async void HandlePageNavigation(Page page)
        {
            string[] RawPageName = page.ToString().Split(GeneralClass.Message.dot);
            string pageName = RawPageName.Last();

            if (pageName == nameof(HomePage))
            {
                await GeneralClass.NavigateToPage(nameof(LoginPage));

            }
            else if (pageName == nameof(SearchPage))
            {
                await GeneralClass.NavigateToPage(nameof(TransferPage),
                                                (nameof(GeneralClass.IsNewPage), false),
                                                (nameof(GeneralClass.Model), nameof(Item)));

            }
            else if (pageName == nameof(TransferPage))
            {
                string preferenceResult = Preference.GetValueOrDefault(nameof(GeneralClass.IsConfirmSync), GeneralClass.Message.dash);
                if (preferenceResult == GeneralClass.Message._false)
                {
                    await HandleFalse();
                }
                else
                {
                    bool homepage = await Shell.Current.DisplayAlert(GeneralClass.Message.areyousurewanttoleave,
                                                                    GeneralClass.Message.willberedirectedtohomepage,
                                                                    GeneralClass.Message.gotohome,
                                                                    GeneralClass.Message.stayhere);
                    if (homepage){await GeneralClass.NavigateToPage(nameof(HomePage));}
                }
            }
            else if (pageName == nameof(TransferLinesPage))
            {
                string result = Preference.GetValueOrDefault(nameof(GeneralClass.IsConfirmSync), GeneralClass.Message.dash);
                if (result == GeneralClass.Message._true)
                {
                    bool homepage = await Shell.Current.DisplayAlert(GeneralClass.Message.areyousurewanttoleave,
                                                                    GeneralClass.Message.willberedirectedtohomepage,
                                                                    GeneralClass.Message.gotohome,
                                                                    GeneralClass.Message.stayhere);
                    if (homepage) { await GeneralClass.NavigateToPage(nameof(HomePage)); }
                }
                else
                {
                    await GeneralClass.NavigateToPage(nameof(TransferPage),
                                                     (nameof(GeneralClass.IsNewPage), false),
                                                     (nameof(GeneralClass.Model), nameof(TransferLines)),
                                                     (nameof(GeneralClass.AddNew), true));
                }
            }
            else
            {
                App.Current.Quit();

            }

        }

        private async Task HandleFalse()
        {
            var result = DBAccess.GetAll<TransferLines>();
            if (result.Count > 0)
            {
                bool leavepage = await HandleDisplayAlert(GeneralClass.Message.leavingwillcancelcurrenttransfer);
                if (leavepage) { await GeneralClass.NavigateToPage(nameof(HomePage)); }
            }
            else
            {
                
                bool leavepage = await HandleDisplayAlert(GeneralClass.Message.unsavedchangesdoyouwanttoleave);
                if (leavepage){await GeneralClass.NavigateToPage(nameof(HomePage));}
            }
        }

        private async Task<bool> HandleDisplayAlert(string messageBody)
        {
            return await Shell.Current.DisplayAlert(
                                    GeneralClass.Message.areyousurewanttoleave,
                                    messageBody,
                                    GeneralClass.Message.leavepage,
                                    GeneralClass.Message.stayhere);
        }

    }

}
