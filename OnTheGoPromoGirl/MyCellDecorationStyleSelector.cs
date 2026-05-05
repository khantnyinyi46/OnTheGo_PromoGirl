using OnTheGoPromoGirl.DataAccess;
using OnTheGoPromoGirl.Model;
using Telerik.Maui.Controls.DataGrid;

namespace OnTheGoPromoGirl
{
    public class MyCellDecorationSelector:DataGridStyleSelector
    {
        public DataGridStyle CellTemplate1 { get; set; }
        public override DataGridStyle SelectStyle(object item, BindableObject container)
        {
            DataGridCellInfo cellInfo = item as DataGridCellInfo;
            if(cellInfo != null)
            {
                //string salesPersonID = (cellInfo.Item as SalesPerson).SalesPersonID;
                string salesPersonID = (cellInfo.Item as dynamic).SalesPersonID;
                string itemID;
                var transferLines = DBAccess.GetAll<TransferLines>();
                var itemTemp = DBAccess.GetAll<ItemTemp>().FirstOrDefault();
                if (itemTemp != null)
                { 
                    itemID = itemTemp.ItemID;
                }
                else
                {
                    itemID = string.Empty;
                }
                    foreach (var line in transferLines)
                    {
                        if (line.TransferTo == salesPersonID &&
                            line.TransferQty > 0 &&
                            line.ItemID == itemID
                            )
                        {
                            return CellTemplate1;

                        }

                    }

                
                
            }
            return base.SelectStyle(item, container);
        }
    }
}
