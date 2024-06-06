using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;
using System.Globalization;
using SQLiteWPF.Properties;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Maps.MapControl.WPF;

namespace SQLiteWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Constructeur de la classe, fenetre principale
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            // Setup de la fenetre principale, se connecte à la base de données, actualise la listview
            setup();
            project_batiment_combobox.SelectionChanged += Project_batiment_combobox_SelectionChanged;
            string BasePDFFile = Path.Combine(Path.GetTempPath(), "sample.pdf");
            File.WriteAllBytes(BasePDFFile, SQLiteWPF.Properties.Resources.sample);
            webView.Source = new Uri(BasePDFFile);
            
            webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
            //MessageBox.Show(Path.Combine(Path.GetTempPath()));
            myMap.SetView(new Microsoft.Maps.MapControl.WPF.Location(48.888058302205025, 2.4023131957530106), 19);
            
            Pushpin pin = new Pushpin();
            pin.Location = new Location(48.888058302205025, 2.4023131957530106);
            pin.Content = "A";
            

            myMap.Children.Add(pin);


            

            


        }

        private void MapExpander_Expanded(object sender, RoutedEventArgs e)
        {
            ((MainWindow)System.Windows.Application.Current.MainWindow).UpdateLayout();
        }

        private void WebView_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            WebView2 webv = (WebView2)sender;
            webv.CoreWebView2.Profile.PreferredColorScheme = Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Dark;
        }


        /// <summary>
        /// Update des étages selon le batiment choisi.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Project_batiment_combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Récupérer le nom du batiment pour ensuite en récupérer les étages.
            ComboBox comboBox = sender as ComboBox;
            var selected_batiment = comboBox.SelectedItem;
            if(selected_batiment != null)
            {
                //MessageBox.Show(selected_batiment.ToString());
                SQLiteConnection sqlc = SetupSQLite.sqliteconn;
                UpdateEtagesList(sqlc, selected_batiment.ToString());
                UpdateCityValue(sqlc, selected_batiment.ToString());
                // Le nom est récupérer, il faut maintenant une commande de lecture de la table batiment, pour la colonne etages, par le nom du batiment -> C'est fait
            }
            else
            {

            }


        }

        /// <summary>
        /// Chemin par défaut du dossier contenant la base de données
        /// </summary>
        /// <returns></returns>
        public static string DBDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);       //fetch App Data Local directory
        }

        /// <summary>
        /// Méthode de paramétrage et d'initialisation de la base de données.
        /// </summary>
        void setup()
        {
            if (!SetupSQLite.isFirstTime)   //if database is not found, create a new one
            {
                new SetupSQLite();
            }
            Select(SetupSQLite.sqliteconn);
            //load database content into listView

            string dbpath = Path.Combine(DBDirectory() + @"\HER_Sqlite_DB\DB_00.sqlite");
            string connectionString = "Data Source=" + dbpath + ";Version=3";

            List<string> buildingNames = SetupSQLite.GetBuildingsNamesFromSQLite(connectionString);
            project_batiment_combobox.ItemsSource = buildingNames;


        }

        /// <summary>
        /// Méthode d'insertion des nouveaux projets dans la base de données.
        /// </summary>
        /// <param name="project_creation_date">Date de création du projet, calculée automatiquement par DateTime.Now</param>
        /// <param name="project_name">Nom du projet.</param>
        /// <param name="project_batiment">Ville du projet. Devra être amenée directement par le choix du batiment</param>
        /// <param name="projectSurface">Surface de l'emprise du projet. Peut être nulle.</param>
        /// <param name="projectComments">Premiers commentaires sur le projet.</param>
        /// <param name="sqliteconn">Objet de connection à la base de données.</param>
        /// <returns></returns>
        bool Insert(string project_creation_date, string project_name, string project_batiment, int project_completed, string project_due_date, string project_specialist, string project_floors, string project_floors_PDF, string project_floors_DWG, int project_zip_code, string project_adress, string project_city, SQLiteConnection sqliteconn)
        {
            var command = sqliteconn.CreateCommand();
            command.CommandText = "INSERT INTO project(project_creation_date, project_name, project_batiment, project_completed, project_due_date, project_specialist, project_floors, project_floors_PDF, project_floors_DWG, project_zip_code, project_adress, project_city) VALUES ('"
                + project_creation_date
                + "', '"
                + project_name
                + "','"
                + project_batiment
                + "','"
                + project_completed
                + "','"
                + project_due_date
                + "','"
                + project_specialist
                + "','"
                + project_floors
                + "','"
                + project_floors_PDF
                + "','"
                + project_floors_DWG
                + "','"
                + project_zip_code
                + "','"
                + project_adress
                + "','"
                + project_city
                + "')";
            handleConn(sqliteconn);
            bool s = command.ExecuteNonQuery() == 1 ? true : false;       //ExecuteNonQuery method returns 1 for success and 0 for failure, if it returns 1 assign boolean value true to indicate a successful commit
            handleConn(sqliteconn);
            return s;
        }

        bool Truncate(SQLiteConnection sqliteconn)
        {
            var command = sqliteconn.CreateCommand();
            command.CommandText = "DELETE FROM project";     //Currently SQLite does not use TRUNCATE, its either you use DELETE or DROP TABLE IF EXISTS then recreate it
            handleConn(sqliteconn);
            bool s = command.ExecuteNonQuery() == 1 ? false : true;   
            handleConn(sqliteconn);
            return s;
        }

        public void UpdateCityValue(SQLiteConnection sqliteconn, string nom_batiment)
        {
            string ville_obtenue;

            string query = "SELECT ville FROM batiments WHERE nom = @nom_batiment";

            using(var cmd = new SQLiteCommand(query, sqliteconn))
            {
                sqliteconn.Open();
                cmd.Parameters.AddWithValue("@nom_batiment", nom_batiment);
                
                object result = cmd.ExecuteScalar();
                if(result != null && result != DBNull.Value)
                {
                    ville_obtenue = result.ToString();
                    //MessageBox.Show(string.Join(" ",etages_obtenus));
                    project_city_txtbox.Text = ville_obtenue;
                }
                else { }
                sqliteconn.Close();
            }
        }
        public void UpdateEtagesList(SQLiteConnection sqliteconn, string nom_batiment)
        {
            List<string> etages_obtenus = new List<string>();

            string query = "SELECT etages FROM batiments WHERE nom = @nom_batiment";

            using(var cmd = new SQLiteCommand(query, sqliteconn))
            {
                try
                {
                    sqliteconn.Open();
                    cmd.Parameters.AddWithValue("@nom_batiment", nom_batiment);

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        etages_obtenus = result.ToString().Split(',').ToList();
                        //MessageBox.Show(string.Join(" ",etages_obtenus));
                        project_etages_listbox.SelectedItems.Clear();
                        project_etages_listbox.ItemsSource = etages_obtenus;
                    }
                    sqliteconn.Close();
                }
                catch (System.InvalidOperationException ex) 
                {
                    MessageBox.Show(ex.ToString());
                }
                
            }
        }

        public void Select(SQLiteConnection sqliteconn)      //selecting all data in SQLite db and displaying it in a ListView named listView as declared in XAML
        {
            DataSet ds = new DataSet();        //dataset used to hold content returned from SQLite db

            string str_query = "SELECT iD, project_creation_date, project_name, project_completed, project_batiment, project_city, project_due_date, project_floors, project_specialist FROM project ORDER BY iD DESC"; //consider using LINQ to SQL
            using (var cmd = new SQLiteCommand(str_query))
            {
                SQLiteDataAdapter sda = new SQLiteDataAdapter();
                handleConn(sqliteconn);
                sda.SelectCommand = cmd;
                cmd.Connection = sqliteconn;
                ds.Clear();     //clear dataset before loading new content to make sure no data mixup
                sda.Fill(ds);
                listView.DataContext = ds.Tables[0].DefaultView;
                projectsDataGrid.DataContext = ds.Tables[0].DefaultView;
            }
            handleConn(sqliteconn);
        }

        /// <summary>
        /// Methode appellée lors du clic sur le bouton de création d'un nouveau projet.
        /// Renseigne la base de données des projets avec une injonction INSERT.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void submit_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ce if est particulierement grossier
                if (Insert(DateTime.Now.ToString("dd/MM/yyyy"),
                           project_name_txtbox.Text,
                           project_batiment_combobox.SelectedItem.ToString(),
                           0,
                           DateTime.Now.ToString("dd/MM/yyyy"),
                           responsable_txtbox.Text,
                           concatenatePickedFloors(project_etages_listbox.SelectedItems),
                           "PDF", // Fichiers PDF à référencer
                           "DWG", // Fichiers DWG à référencer
                           75008, // Code postal à recueillir d'après le batiment
                           "ProjectAdress", // Adresse à recueillir d'après le batiment
                           project_city_txtbox.Text, // Ville recueillie d'après le batiment
                           SetupSQLite.sqliteconn))
                {
                    //MessageBox.Show("Information successfully submitted");
                    Select(SetupSQLite.sqliteconn);
                    AutoResizeGridViewColumns((GridView)listView.View);

                }
                else
                {
                    MessageBox.Show("Oops! Something went wrong");
                }
            }


            catch (System.FormatException ex)
            { MessageBox.Show(ex.Message + " >><< "); }

            
        }

        static void AutoResizeGridViewColumns(GridView view)
        {
            if(view == null || view.Columns.Count < 1) return;
            foreach(var column in view.Columns)
            {
                if (double.IsNaN(column.Width))
                    column.Width = 1;
                column.Width = double.NaN;
            }
        }

        private string concatenatePickedFloors(System.Collections.IList etages)
        {
            string concatenatedPickedFloorsString = null;
            List<string> listString = etages.Cast<string>().ToList();
            if(etages.Count >0 && etages != null)
            {
                concatenatedPickedFloorsString = string.Join(",", listString);
            }
            else
            {
                concatenatedPickedFloorsString = "Tous";
            }
            
            return concatenatedPickedFloorsString;

        }

        private void truncate_btn_Click(object sender, RoutedEventArgs e)
        {
            if (Truncate(SetupSQLite.sqliteconn))
            {
                MessageBox.Show("Information successfully deleted");
                Select(SetupSQLite.sqliteconn);
            }
        }
        private void handleConn(SQLiteConnection sqliteconn)
        {
            if (sqliteconn.State == ConnectionState.Closed)
            {
                sqliteconn.Open();
            }
            else
            {
                sqliteconn.Close();
            }
        }

        private void listView_Selected(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Hey");
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            DataRowView dtrv = listView.SelectedItem as DataRowView;
            if(dtrv != null)
            {
                DataRow dtr = dtrv.Row;
                object[] obj = dtr.ItemArray;
                foreach (object obj2 in obj)
                {

                    //MessageBox.Show(obj2.ToString());
                }
            }
            
        }

        private void listView_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("hey");
        }
    }
}
