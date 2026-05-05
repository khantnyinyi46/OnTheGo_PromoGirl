using static OnTheGoPromoGirl.Views.TransferPage;

namespace OnTheGoPromoGirl
{
    public class MyItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate SpecialTemplate { get; set; }
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var combinedData = item as CombinedData;
            if (combinedData != null && combinedData.TransferQty > 0)
            {
                return SpecialTemplate;
            }

            return DefaultTemplate;
        }
    }
}
