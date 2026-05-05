namespace OnTheGoPromoGirl.Common
{
    public class Preference
    {
        public static void SaveSetting(string key, string value)
        {
            Preferences.Default.Set(key, value);

        }

        public static string GetValueOrDefault(string key, string defaultValue)
        {
            string contextEdit = Preferences.Default.Get(key, defaultValue);
            return contextEdit;

        }

        public static void RemoveSetting(string key)
        {
            Preferences.Default.Remove(key);

        }

    }

}
