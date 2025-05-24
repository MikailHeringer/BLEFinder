using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLEFinder.Classes
{
    public class DbSemestre
    {
        SQLiteAsyncConnection Sql;


        public DbSemestre() { }

        async Task Init()
        {
            if (Sql is not null) { return; }

            Sql = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            await Sql.CreateTableAsync<Semestre>();
        }

        public async Task<List<List<string>>> GetSemestre()
        {
            await Init();

            var semestres = await Sql.Table<Semestre>().ToListAsync();

            return semestres.GroupBy(s => s.CursoId)
                            .Select(g => g.Select(s => s.Name).ToList())
                            .ToList();
        }

        public async Task<int> SaveItem(Semestre item)
        {
            await Init();
            return await Sql.InsertAsync(item);
        }

        public async Task<int> UpdateItem(Semestre item)
        {
            await Init();
            return await Sql.UpdateAsync(item);
        }

        public async Task<int> DeleteItem(Semestre item)
        {
            await Init();
            return await Sql.DeleteAsync(item);
        }

        public async Task<int> ClearItens()
        {
            await Init();
            await DbCount.Reset("Semestre");
            return await Sql.ExecuteAsync("DELETE FROM Semestre");
        }
    }

    public class Semestre
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string? Name { get; set; }
        public int CursoId { get; set; }
    }
}
