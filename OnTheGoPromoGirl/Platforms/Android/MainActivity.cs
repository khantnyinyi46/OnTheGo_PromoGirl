using Android.App;
using Android.Content.PM;
using Android.OS;
using OnTheGoPromoGirl.Common;
using OnTheGoPromoGirl.Platforms.Android;

namespace OnTheGoPromoGirl
{
	[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait/*| ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density*/)]
	public class MainActivity : MauiAppCompatActivity
	{
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            DependencyService.Register<IAndroidOSService, AndroidOsServiceImpl>();

        }
        public override void OnBackPressed()
        {
            var navigationService = new NavigationService();
            navigationService.HandlePageNavigation(Shell.Current.CurrentPage);

        }


    }

}
