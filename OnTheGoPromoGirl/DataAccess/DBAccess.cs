using OnTheGoPromoGirl.Common;
using SQLite;

namespace OnTheGoPromoGirl.DataAccess
{
    public class DBAccess
    {
        

        public static List<T> GetAll<T>() where T : new()
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(GeneralClass.OnThGopromogirlDB, false))
                {
                    return conn.Table<T>().ToList();

                }

            }
            catch (Exception ex)
            {
                GeneralClass.ShowMessage(
                    string.Format(GeneralClass.Message.failpleasecontact + GeneralClass.Message.singlespace + ex, nameof(T))
                    );
                return new List<T>();

            }

        }

        public static List<T> GetByColumn<T>(string columnName, string value) where T : new()
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(GeneralClass.OnThGopromogirlDB, false))
                {
                    string tableName = typeof(T).Name;
                    string query = $"Select * from {tableName} where {columnName} like ?";
                    
                    var rows = conn.Query<T>(query,value);
                    return rows;
                }

            }
            catch(Exception ex)
            {
                GeneralClass.ShowMessage(
                    string.Format(GeneralClass.Message.failpleasecontact + GeneralClass.Message.singlespace + ex, nameof(T))
                    );
                return new List<T>();

            }

        }
        public static int DeleteAll<T>() where T : new()
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(GeneralClass.OnThGopromogirlDB, false))
                {
                    string tableName = typeof(T).Name;
                    string deleteQuery = $"DELETE FROM {tableName}";

                    return conn.Execute(deleteQuery);

                }

            }
            catch (Exception ex)
            {
                GeneralClass.ShowMessage(
                    string.Format(GeneralClass.Message.failpleasecontact + GeneralClass.Message.singlespace + ex, nameof(T))
                    );
                return 0;

            }

        }
        public static int DeleteByTwoColumn<T>(string columnName, string value, string columnName2, string value2) where T : new()
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(GeneralClass.OnThGopromogirlDB, false))
                {
                    string tableName = typeof(T).Name;
                    string deleteQuery = $"DELETE FROM {tableName} WHERE {columnName} = ? AND {columnName2} = ?";

                    return conn.Execute(deleteQuery,value,value2);

                }

            }
            catch(Exception ex)
            {
                GeneralClass.ShowMessage(
                    string.Format(GeneralClass.Message.failpleasecontact + GeneralClass.Message.singlespace + ex, nameof(T))
                    );
                return 0;

            }

        }
        public static int DeleteByOneColumn<T>(string columnName, string value) where T : new()
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(GeneralClass.OnThGopromogirlDB, false))
                {
                    string tableName = typeof(T).Name;
                    string deleteQuery = $"DELETE FROM {tableName} WHERE {columnName} = ?";

                    return conn.Execute(deleteQuery, value);

                }

            }
            catch (Exception ex)
            {
                GeneralClass.ShowMessage(
                    string.Format(GeneralClass.Message.failpleasecontact + GeneralClass.Message.singlespace + ex, nameof(T))
                    );
                return 0;

            }

        }
        
        public static int Insert<T>(T item) where T : new()
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(GeneralClass.OnThGopromogirlDB, false))
                {
                    return conn.Insert(item);

                }

            }
            catch (Exception ex)
            {
                GeneralClass.ShowMessage(
                    string.Format(GeneralClass.Message.failpleasecontact + GeneralClass.Message.singlespace + ex, nameof(T))
                    );
                return 0;

            }

        }
        public static int InsertAll<T>(List<T> items) where T : new()
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(GeneralClass.OnThGopromogirlDB, false))
                {
                    return conn.InsertAll(items);

                }

            }
            catch (Exception ex)
            {
                GeneralClass.ShowMessage(
                    string.Format(GeneralClass.Message.failpleasecontact + GeneralClass.Message.singlespace + ex, nameof(T))
                    );
                return 0;

            }

        }

    }

}
