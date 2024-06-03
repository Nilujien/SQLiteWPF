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
        /// <summary>
        /// Module de connection à la base de données.
        /// </summary>
        public static SQLiteConnection sqliteconn = new SQLiteConnection("Data Source = " + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\HER_Sqlite_DB\" + "DB_00.sqlite" + "; version=3; new=False; datetimeformat=CurrentCulture"));

        public static bool isFirstTime;

        /// <summary>
        /// Chemin complet du fichier de base de données.
        /// </summary>
        public static string DB_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\HER_Sqlite_DB\" + "DB_00.sqlite");//DataBase Name 

        #region Constructor
        /// <summary>
        /// Méthode principale du constructeur
        /// </summary>
        public SetupSQLite()
        {
            //Si la base données n'est pas trouvée:
            if (!checkExists("DB_00.sqlite").Result)
            {       
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
            // Si la base de données est trouvée :
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
        /// <summary>
        /// Méthode de vérification d'existance
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public async Task<bool> checkExists(string filename)
        {
            return System.IO.File.Exists(DBDirectory() + @"\HER_Sqlite_DB\" + filename + "");
        }
        /// <summary>
        /// Chemin par défaut de la base de données
        /// </summary>
        /// <returns></returns>
        public static string DBDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);       //fetch App Data Local directory
        }
        /// <summary>
        /// Création de la base de données, n'est appellé que si la base de données est absente au lancement du logiciel
        /// </summary>
        public void CreateSQLiteDatabase()
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HER_Sqlite_DB")); //creating directory name MySQLiteDB in the App Data Local directory where we will store our SQLite files
            SQLiteConnection.CreateFile(Path.Combine(DBDirectory() + @"\HER_Sqlite_DB\DB_00.sqlite"));
            sqliteconn.Open();

            // Création de la base de données ---
            // Création de la base de données ---
            // Création de la base de données ---
            // Création de la base de données ---

            var command = sqliteconn.CreateCommand();       //create command using the SQLiteConnection      
            command.CommandText = "CREATE TABLE IF NOT EXISTS project(iD INTEGER PRIMARY KEY, project_creation_date DATETIME NOT NULL, project_name VARCHAR(50) NOT NULL, project_completed INT, project_batiment TEXT, project_city TEXT, project_due_date DATETIME, project_floors TEXT, project_specialist TEXT, project_floors_PDF TEXT, project_floors_DWG TEXT, project_adress TEXT, project_zip_code INT )";
            command.ExecuteNonQuery();      //execute the create command


            //command.CommandText = "CREATE TABLE IF NOT EXISTS batiments(id INTEGER PRIMARY KEY, nom VARCHAR(50), ville VARCHAR(50), code_postal INT, adresse VARCHAR(50))";
            //command.ExecuteNonQuery();
            //this is how you would go on to create another table executing non queries one after the other


        }
        /// <summary>
        /// Liste des batiments et de leurs caractéristiques, chargées depuis le fichier JSON correspondant.
        /// </summary>
        /// <param name="ressourceName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
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


        /// <summary>
        /// Récupère la liste des noms des batiments depuis la base de données
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Insertion des batiments dans la table batiments de la base de données.
        /// </summary>
        /// <param name="buildings"></param>
        /// <param name="connectionString"></param>
        public static void InsertBuildingsIntoSQLite(List<Batiment> buildings, string connectionString)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string createBuildingTableQuery = "CREATE TABLE IF NOT EXISTS batiments(id INTEGER PRIMARY KEY," +
                    " nom VARCHAR(50)," +
                    " ville VARCHAR(50)," +
                    " code_postal INT," +
                    " adresse VARCHAR(50)," + 
                    " etages TEXT)";

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



                string insertQuery = "INSERT INTO batiments(nom, ville, code_postal, adresse, etages) VALUES (@Nom, @Ville, @CodePostal, @Adresse, @Etages)";

                using (var command = new SQLiteCommand(insertQuery, connection)) 
                {
                    foreach(var building in buildings)
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Nom", building.Nom);
                        command.Parameters.AddWithValue("@Ville", building.Ville);
                        command.Parameters.AddWithValue("@CodePostal", building.CodePostal);
                        command.Parameters.AddWithValue("@Adresse", building.Adresse);
                        command.Parameters.AddWithValue("@Etages", building.Etages);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
        /// <summary>
        /// Définition de la classe batiment, qui sert à récupérer les informations du JSON.
        /// </summary>
        public class Batiment
        {
            public int Id { get; set; }
            public string Nom { get; set; }
            public string Ville { get; set; }
            public int CodePostal { get; set; }
            public string Adresse { get; set; }
            public string Etages {  get; set; }
        }
    }
}
