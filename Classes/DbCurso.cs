using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLEFinder.Classes
{
    public class DbCurso
    {
        SQLiteAsyncConnection Sql;


        public DbCurso() { }

        async Task Init()
        {
            if(Sql is not null) { return; }

            Sql = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            await Sql.CreateTableAsync<Curso>();
        }

        public async Task<List<List<string>>> GetCursos()
        {
            await Init();
            var cursos = await Sql.Table<Curso>().ToListAsync();
            return new List<List<string>>
            {
                cursos.Select(c => c.Name).ToList(),
                cursos.Select(s => s.Sigla).ToList(),
            };
        }

        public async Task<int> SaveItem(Curso item)
        {
            await Init();
            return await Sql.InsertAsync(item);
        }

        public async Task<int> UpdateItem(Curso item)
        {
            await Init();
            return await Sql.UpdateAsync(item);
        }

        public async Task<int> DeleteItem(Curso item)
        {
            await Init();
            return await Sql.DeleteAsync(item);
        }
        public async Task<int> ClearItens()
        {
            await Init();
            await DbCount.Reset("Curso");
            return await Sql.ExecuteAsync("DELETE FROM Curso");
        }

    }

    // Classe com modelo de dados do curso
    public class Curso
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Sigla { get; set; }
    }

    // Constantes para criação do banco de dados
    public static class Constants
    {
        public const string DbName = "AthonDb.db3";

        public const SQLite.SQLiteOpenFlags Flags =
            SQLite.SQLiteOpenFlags.ReadWrite |
            SQLite.SQLiteOpenFlags.Create |
            SQLite.SQLiteOpenFlags.SharedCache;

        public static string DatabasePath => Path.Combine(FileSystem.AppDataDirectory, DbName);
    }
}
