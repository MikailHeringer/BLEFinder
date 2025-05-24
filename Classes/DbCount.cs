using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLEFinder.Classes
{

    public static class DbCount
    {
        static SQLiteAsyncConnection Sql;

        static async Task Init()
        {
            if (Sql is not null) { return; }
            Sql = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        }

        public static async Task Reset(string databaseName)
        {
            await Init();
            await Sql.ExecuteAsync($"DELETE FROM sqlite_sequence WHERE name = ?", databaseName);
        }
    }
}
