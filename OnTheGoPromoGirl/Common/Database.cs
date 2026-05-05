using OnTheGoPromoGirl.Model;
using SQLite;
using static SQLite.SQLiteConnection;

namespace OnTheGoPromoGirl.Common
{
    public class Database
    {
        private static SQLiteAsyncConnection _dbConnection;       
        public static async Task<SQLiteAsyncConnection> SetUpDb()
        {
            try
            {
                if (_dbConnection == null)
                {
                    var pathToNewFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string dbPath = Path.Combine(pathToNewFolder, "OnTheGoPGDB.db3");//OnTheGoPGDB
                    if (!Directory.Exists(pathToNewFolder))
                    {
                        Directory.CreateDirectory(pathToNewFolder);
                    }
                    if (Directory.Exists(pathToNewFolder))
                    {
                        _dbConnection = new SQLiteAsyncConnection(dbPath);
                        await _dbConnection.CreateTableAsync<DeviceActivation>();
                        await _dbConnection.CreateTableAsync<Item>();
                        await _dbConnection.CreateTableAsync<SalesPerson>();
                        await _dbConnection.CreateTableAsync<ItemTemp>();
                        await _dbConnection.CreateTableAsync<Transfer>();
                        await _dbConnection.ExecuteAsync(TransferLines.CreateTableTransferLines);

                    }
                }
                return _dbConnection;
            }
            catch(Exception ex)
            {
                GeneralClass.ShowMessage(ex.Message);
                return null;

            }
        }

        //bool udf1Exists = await ColumnExists(nameof(TransferLines), "UDF1");
        //if (!udf1Exists)
        //{
        //    string addColumnQuery = "ALTER TABLE TransferLines ADD COLUMN UDF1 varchar;";
        //    await _dbConnection.ExecuteAsync(addColumnQuery);

        //    TransferLines transferLines = new TransferLines
        //    {
        //        TransferID = "TransferID_1",
        //        ItemID = "ItemID_1",
        //        TransferTo = "TransferTo_1",
        //        TransferQty = 1,
        //        UomID = "UomID_1",
        //        CreateDateTime = DateTime.Now
        //    };
        //    await _dbConnection.InsertAsync(transferLines);

        //    string selectColumnQuery = "SELECT * FROM TransferLines;";
        //    var result = await _dbConnection.QueryAsync<TransferLines>(selectColumnQuery);

        //    string updateColumnQuery = "UPDATE TransferLines SET UDF1 = 'DefaultValue';";
        //    await _dbConnection.ExecuteAsync(updateColumnQuery);

        //    string selectColumnQuery2 = "SELECT * FROM TransferLines;";
        //    var result2 = await _dbConnection.QueryAsync<TransferLines>(selectColumnQuery2);

        //}

        //private static async Task<bool> ColumnExists(string tableName, string columnName)
        //{
        //    var query = $"PRAGMA table_info({tableName});";

        //    var columns = await _dbConnection.QueryAsync<ColumnInfo>(query);

        //    return columns.Any(col => col.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
        //}

        //private static async Task PrintOutColumnTransferLines()
        //{
        //    var query = "PRAGMA table_info(TransferLines);";

        //    var columns = await _dbConnection.QueryAsync<ColumnInfo>(query);

        //    var columnNames = columns.Select(col => col.Name).ToList();

        //    foreach (var columnName in columnNames)
        //    {
        //        Console.WriteLine("*** " + columnName);
        //    }
        //}
    }
}
