using SQLite;

namespace OnTheGoPromoGirl.Model
{
    public class Transfer
    {
        [PrimaryKey]
        public string TransferID { get; set; } //PK
        public DateTime TransferDate { get; set; }
        public string TransferFrom { get; set; }
        public string DeviceID { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDateTime { get; set; }
        public string UDF1 { get; set; }
        public string UDF2 { get; set; }
        public string UDF3 { get; set; }
        public bool IsSync { get; set; }
        public DateTime SyncDateTime { get; set; }

    }

}
