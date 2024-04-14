using SQLite;

namespace CommuteMate.Services
{
    public class LocalDBServices
    {
        static SQLiteAsyncConnection db;
        public static async Task Init()
        {
            if (db != null)
                return;
            // Get an absolute path to the database file
            //var databasePath = PathAction.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyData.db");
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "Commute_Mate_Data.db");

            try
            {
                //SQLite Database connection
                db = new SQLiteAsyncConnection(databasePath);

                //Enable foreign key support
                await db.ExecuteAsync("PRAGMA foreign_keys = ON;");

                await db.CreateTableAsync<Route>();
                await db.CreateTableAsync<Street>();
                //junction table
                await db.CreateTableAsync<RouteStreet>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing database: {ex.Message}");
                throw;
            }
        }
        public static SQLiteAsyncConnection GetDatabase()
        {
            if (db == null)
                Task.Run(async () => await Init()).Wait();
            return db;
        }
    }
}
