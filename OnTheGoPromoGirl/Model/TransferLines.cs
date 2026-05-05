namespace OnTheGoPromoGirl.Model
{
    
    public class TransferLines
    {
        public static string CreateTableTransferLines => "CREATE TABLE IF NOT EXISTS TransferLines (TransferID varchar, ItemID varchar,TransferTo varchar,TransferQty float,UomID varchar,CreateDateTime varchar,PRIMARY KEY(TransferID,ItemID,TransferTo))";
        public string TransferID { get; set; } //PK
        public string ItemID { get; set; } //PK
        public string TransferTo { get; set; } //PK. SalesPersonID
        public decimal TransferQty { get; set; }
        public string UomID { get; set; }
        public DateTime CreateDateTime { get; set; }

    }


}
