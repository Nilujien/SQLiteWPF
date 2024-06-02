using SQLiteWPF.Properties;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;

namespace SQLiteWPF
{
    class SetupSQLite
    {
        public static SQLiteConnection sqliteconn = new SQLiteConnection("Data Source = " + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\HER_Sqlite_DB\" + "DB_00.sqlite" + "; version=3; new=False; datetimeformat=CurrentCulture"));

        public static bool isFirstTime;

        public static string DB_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\HER_Sqlite_DB\" + "DB_00.sqlite");//DataBase Name 

        #region Constructor
        public SetupSQLite()
        {
            if (!checkExists("DB_00.sqlite").Result)
            {       //check for the existence of the database
                using (var db = new SQLiteConnection(DB_PATH))
                {
                    CreateSQLiteDatabase();
                    string dbpath = Path.Combine(DBDirectory() + @"\HER_Sqlite_DB\DB_00.sqlite");
                    string connectionString = "Data Source=" + dbpath + ";Version=3";
                    var buildings = LoadBuildingsFromJson("SQLiteWPF.Batiments.json");
                    InsertBuildingsIntoSQLite(buildings, connectionString);
                    Debug.WriteLine("Batiments importés.");
                    isFirstTime = true;     //using this to avoid calling the create command in the Main Window if the database already exists
                }
            }
            else
            {
                string dbpath = Path.Combine(DBDirectory() + @"\HER_Sqlite_DB\DB_00.sqlite");
                string connectionString = "Data Source=" + dbpath + ";Version=3";
                var buildings = LoadBuildingsFromJson("SQLiteWPF.Batiments.json");
                InsertBuildingsIntoSQLite(buildings, connectionString);
                Debug.WriteLine("Batiments importés.");
                isFirstTime = false;
            }
        }
        #endregion
        public async Task<bool> checkExists(string filename)
        {
            return System.IO.File.Exists(DBDirectory() + @"\HER_Sqlite_DB\" + filename + "");
        }
        public static string DBDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);       //fetch App Data Local directory
        }
        public void CreateSQLiteDatabase()
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HER_Sqlite_DB")); //creating directory name MySQLiteDB in the App Data Local directory where we will store our SQLite files
            SQLiteConnection.CreateFile(Path.Combine(DBDirectory() + @"\HER_Sqlite_DB\DB_00.sqlite"));
            sqliteconn.Open();

            var command = sqliteconn.CreateCommand();       //create command using the SQLiteConnection      
            command.CommandText = "CREATE TABLE IF NOT EXISTS project(id INTEGER PRIMARY KEY, date DATETIME NOT NULL, name VARCHAR(50), age int, amount REAL, comments TEXT)";
            command.ExecuteNonQuery();      //execute the create command


            //command.CommandText = "CREATE TABLE IF NOT EXISTS batiments(id INTEGER PRIMARY KEY, nom VARCHAR(50), ville VARCHAR(50), code_postal INT, adresse VARCHAR(50))";
            //command.ExecuteNonQuery();
            //this is how you would go on to create another table executing non queries one after the other


        }

        private static List<Batiment> LoadBuildingsFromJson(string ressourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(ressourceName))
            {
                if (stream == null)
                    throw new InvalidOperationException($"Resource {ressourceName} not found.");

                using (var reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<List<Batiment>>(json);
                }
            }

        }

        public void PopulateBuildingsTable()
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HER_Sqlite_DB")); //creating directory name MySQLiteDB in the App Data Local directory where we will store our SQLite files
            SQLiteConnection.CreateFile(Path.Combine(DBDirectory() + @"\HER_Sqlite_DB\DB_00.sqlite"));
            sqliteconn.Open();
        }

        public static List<string> GetBuildingsNamesFromSQLite(string connectionString)
        {
            var buildingsNames = new List<string>();
            using(var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string selectQuery = "SELECT Nom FROM batiments";
                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    using (var reader = command.ExecuteReader()) 
                    {
                        while (reader.Read())
                        {
                            buildingsNames.Add(reader["Nom"].ToString());
                        }
                    }
                    

                }
            }
            return buildingsNames;
        }

        public static void InsertBuildingsIntoSQLite(List<Batiment> buildings, string connectionString)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string createBuildingTableQuery = "CREATE TABLE IF NOT EXISTS batiments(id INTEGER PRIMARY KEY," +
                    " nom VARCHAR(50)," +
                    " ville VARCHAR(50)," +
                    " code_postal INT," +
                    " adresse VARCHAR(50))";

                using (var command = new SQLiteCommand(createBuildingTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Clear existing data
                string deleteQuery = "DELETE FROM batiments";
                using (var command = new SQLiteCommand(deleteQuery, connection))
                {
                    command.ExecuteNonQuery();
                }



                string insertQuery = "INSERT INTO batiments(nom, ville, code_postal, adresse) VALUES (@Nom, @Ville, @CodePostal, @Adresse)";

                using (var command = new SQLiteCommand(insertQuery, connection)) 
                {
                    foreach(var building in buildings)
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Nom", building.Nom);
                        command.Parameters.AddWithValue("@Ville", building.Ville);
                        command.Parameters.AddWithValue("@CodePostal", building.CodePostal);
                        command.Parameters.AddWithValue("@Adresse", building.Adresse);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public class Batiment
        {
            public int Id { get; set; }
            public string Nom { get; set; }
            public string Ville { get; set; }
            public int CodePostal { get; set; }
            public string Adresse { get; set; }
        }
    }
}
