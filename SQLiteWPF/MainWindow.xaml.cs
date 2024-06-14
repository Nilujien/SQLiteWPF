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
using System.Diagnostics;
using System.Drawing;
using System.Windows.Interop;
using Color = System.Drawing.Color;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;
using ScottPlot;
using Location = Microsoft.Maps.MapControl.WPF.Location;
using Label = System.Windows.Controls.Label;
using ScottPlot.Plottables;

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

            Border_Typologie_Travaux.MouseLeftButtonUp += Border_Selector_MouseLeftButtonUp;
            Border_Typologie_Transfert.MouseLeftButtonUp += Border_Selector_MouseLeftButtonUp;
            Border_Typologie_Mobilier.MouseLeftButtonUp += Border_Selector_MouseLeftButtonUp;

            Pushpin pin = new Pushpin();
            pin.Location = new Location(48.888058302205025, 2.4023131957530106);
            pin.Content = "A";

            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
            SQLiteWPF.Properties.Resources.Icon_Soft.Handle,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());

            this.Icon = imageSource;

            myMap.Children.Add(pin);

            List<PieSlice> projets_par_villes_PieSlices = new List<PieSlice>
            {
                new PieSlice() {Value = 50, FillColor = ScottPlot.Colors.LightSteelBlue, Label = "Paris : " },
                new PieSlice() {Value = 20, FillColor = ScottPlot.Colors.CadetBlue, Label = "Pantin : " },
                new PieSlice() {Value = 40, FillColor = ScottPlot.Colors.DodgerBlue, Label = "Pré-Saint-Gervais : " },
                new PieSlice() {Value = 10, FillColor = ScottPlot.Colors.PowderBlue, Label = "Bobigny : " }
            };

            List<Bar> projets_par_batiments_Bars = new List<Bar>
            {
                new Bar() {Value = 50, FillColor = ScottPlot.Colors.LightSteelBlue, Label = "Paris : " },
                new Bar() {Value = 20, FillColor = ScottPlot.Colors.CadetBlue, Label = "Pantin : " },
                new Bar() {Value = 40, FillColor = ScottPlot.Colors.DodgerBlue, Label = "Pré-Saint-Gervais : " },
                new Bar() {Value = 10, FillColor = ScottPlot.Colors.PowderBlue, Label = "Bobigny : " }
            };

            double[] values = {5, 10, 25, 13, 10, 12, 32, 16, 28, 30};

            foreach (PieSlice pieSlice in projets_par_villes_PieSlices)
            {
                pieSlice.Label = pieSlice.Label + pieSlice.Value.ToString();
            }

            //WPF_Plot_Projets_Batiments.Plot.Add.Palette = new ScottPlot.Palettes.Amber();

            

            var bars = WPF_Plot_Projets_Batiments.Plot.Add.Bars(values);
            
            // define the content of labels
            foreach (var bar in bars.Bars)
            {
                bar.Label = bar.Value.ToString();
                
                bar.FillColor = ScottPlot.Colors.DodgerBlue;
                
            }
            
            LegendItem lgi = new LegendItem();
            lgi.LabelText = "Essai";
            bars.LegendItems.Append(lgi);

            var pie = WPF_Plot_Projets_Villes.Plot.Add.Pie(projets_par_villes_PieSlices);
            pie.DonutFraction = .5;
            pie.ExplodeFraction = 0;
            pie.ShowSliceLabels = false;
            pie.SliceLabelDistance = .5;

            
            WPF_Plot_Projets_Villes.Plot.HideGrid();
            WPF_Plot_Projets_Villes.Plot.ShowLegend();
            //WPF_Plot_Projets_Villes.Plot.Layout.Frameless();
            WPF_Plot_Projets_Villes.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#222222");
            WPF_Plot_Projets_Villes.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#222222");
            WPF_Plot_Projets_Villes.Plot.Legend.BackgroundColor = ScottPlot.Color.FromHex("#484848");
            WPF_Plot_Projets_Villes.Plot.Legend.FontColor = ScottPlot.Colors.White;
            WPF_Plot_Projets_Villes.Plot.Legend.OutlineColor = ScottPlot.Colors.White;

            WPF_Plot_Projets_Villes.Plot.Axes.Title.Label.Text = "Nombre de projets par villes";
            WPF_Plot_Projets_Villes.Plot.Axes.Bottom.MajorTickStyle.Length = 0;
            WPF_Plot_Projets_Villes.Plot.Axes.Color(ScottPlot.Colors.White);

            // Il faut créer un générateur de ticks d'après le nombre de bâtiments concernés, extraire les valeurs uniques de la colonne batiment



            Tick[] ticks =
            {
                new Tick(0, "Apple"),
                new Tick(1, "Apple"),
                new Tick(2, "aaa"),
                new Tick(3, "Pear"),
                new Tick(4, "Banana"),
                new Tick(5, "Apple"),
                new Tick(6, "Orange"),
                new Tick(7, "Pear"),
                new Tick(8, "Banana"),
                new Tick(9, "Banana"),
                new Tick(10, "Banana"),
            };

            bars.ValueLabelStyle.ForeColor = ScottPlot.Colors.White;
            
            WPF_Plot_Projets_Batiments.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks);
            

            WPF_Plot_Projets_Batiments.Plot.ShowLegend();
            //WPF_Plot_Projets_Villes.Plot.Layout.Frameless();
            WPF_Plot_Projets_Batiments.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#222222");
            WPF_Plot_Projets_Batiments.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#222222");
            WPF_Plot_Projets_Batiments.Plot.Legend.BackgroundColor = ScottPlot.Color.FromHex("#484848");
            WPF_Plot_Projets_Batiments.Plot.Legend.FontColor = ScottPlot.Colors.White;
            WPF_Plot_Projets_Batiments.Plot.Legend.OutlineColor = ScottPlot.Colors.White;

            WPF_Plot_Projets_Batiments.Plot.Axes.Title.Label.Text = "Nombre de projets par batiments";
            WPF_Plot_Projets_Batiments.Plot.Axes.Bottom.MajorTickStyle.Length = 0;

            // Interressant pour planning ?
            // WPF_Plot_Projets_Batiments.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.DateTimeAutomatic();
            WPF_Plot_Projets_Batiments.Plot.Axes.Color(ScottPlot.Colors.White);
            WPF_Plot_Projets_Batiments.Plot.Axes.Margins(bottom: 0, top: .2);



        }

        private void WPF_Plot_Projets_Batiments_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WPF_Plot_Projets_Batiments.Plot.Axes.AutoScale();
            Debug.WriteLine("Hey");
        }

        private void Border_Selector_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Border bord = (Border)sender;
            Label lab = (Label)bord.Child;
            if(bord.Tag != null)
            {
                if (bord.Tag.ToString() == lab.Content.ToString())
                {
                    lab.Foreground = Brushes.White;
                    bord.Background = Brushes.DarkSlateGray;
                    bord.Tag = null;
                }
            }
            
            else
            {
                lab.Foreground = Brushes.Black;
                bord.Background = Brushes.CadetBlue;
                bord.Tag = lab.Content.ToString();
            }
            

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
        bool Insert(string project_creation_date, string project_name, string project_batiment, int project_completed, string project_due_date, string project_specialist, string project_floors, string project_floors_PDF, string project_floors_DWG, int project_zip_code, string project_adress, string project_city, string project_description, string project_typologies, int project_seen_for_session, SQLiteConnection sqliteconn)
        {
            var command = sqliteconn.CreateCommand();

            command.CommandText = "INSERT INTO project(project_creation_date, project_name, project_batiment, project_completed, project_due_date, project_specialist, project_floors, project_floors_PDF, project_floors_DWG, project_zip_code, project_adress, project_city, project_description, project_typologies, project_seen_for_session) VALUES ('"
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
                + "','"
                + project_description.Replace("'", "`")
                + "','"
                + project_typologies
                + "','"
                + project_seen_for_session
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
                handleConn(sqliteconn);
                cmd.Parameters.AddWithValue("@nom_batiment", nom_batiment);
                
                object result = cmd.ExecuteScalar();
                if(result != null && result != DBNull.Value)
                {
                    ville_obtenue = result.ToString();
                    //MessageBox.Show(string.Join(" ",etages_obtenus));
                    project_city_txtbox.Text = ville_obtenue;
                }
                else { }
                handleConn(sqliteconn);
            }
        }

        /// <summary>
        /// Méthode de mise à jour de la liste des étages, d'après le nom du bâtiment
        /// </summary>
        /// <param name="sqliteconn"></param>
        /// <param name="nom_batiment"></param>
        public void UpdateEtagesList(SQLiteConnection sqliteconn, string nom_batiment)
        {
            // Prédéclaration d'une liste de batiment
            List<string> etages_obtenus = new List<string>();
            // string de la requete SQL de selection
            string query = "SELECT etages FROM batiments WHERE nom = @nom_batiment";
            // utilisation d'une nouvelle commande
            using(var cmd = new SQLiteCommand(query, sqliteconn))
            {
                // essai
                try
                {
                    // Ouverture de la connection, pas sur la même méthode que pour les autres connexions
                    sqliteconn.Open();
                    // Ajout de la valeur à la requete
                    cmd.Parameters.AddWithValue("@nom_batiment", nom_batiment);
                    // Renvoi la premiere colonne de la première rangée de l'ensemble de la selection obtenue.
                    object result = cmd.ExecuteScalar();
                    // Si le resultat de la requete n'est pas null
                    if (result != null && result != DBNull.Value)
                    {
                        // decoupage des etages obtenus et stockage dans la liste précedemment déclarée
                        etages_obtenus = result.ToString().Split(',').ToList();
                        // nettoyage des etages précédents de la listbox
                        project_etages_listbox.SelectedItems.Clear();
                        // attribution d'une nouvelle source à la listbox, d'après la liste construite 
                        project_etages_listbox.ItemsSource = etages_obtenus;
                    }
                    // fermeture de la connexion
                    sqliteconn.Close();
                }
                // tentative de catch d'une opération invalide
                catch (System.InvalidOperationException ex) 
                {
                    MessageBox.Show(ex.ToString());
                }
                
            }
        }
        
        /// <summary>
        /// Sélection du contenu de la base de données.
        /// Modification du DataContext de la DataGrid des projets.
        /// </summary>
        /// <param name="sqliteconn"></param>
        public void Select(SQLiteConnection sqliteconn)      //selecting all data in SQLite db and displaying it in a DataGrid
        {
            // Prédéclaration d'un objet de stockage de données.
            DataSet ds = new DataSet();      

            // string de sélection des différentes colonnes de la table des projets, en classant par iD descendant (projet le plus récent en haut de la liste)
            string str_query = "SELECT iD, project_creation_date, project_name, project_completed, project_batiment, project_city, project_due_date, project_floors, project_specialist, project_description, project_typologies, project_seen_for_session FROM project ORDER BY iD DESC"; //consider using LINQ to SQL
            // Utilisation d'une nouvelle commande SQLiteCommand, prenant la string de sélection en parametre
            using (var cmd = new SQLiteCommand(str_query))
            {
                // Adaptateur de données SQLite
                SQLiteDataAdapter sda = new SQLiteDataAdapter();
                // Ouverture de la connexion
                handleConn(sqliteconn);
                // Etablissement de la commande de selection de l'adaptateur de données SQLite
                sda.SelectCommand = cmd;
                // Etablissement de la connexion de la commande
                cmd.Connection = sqliteconn;
                // Nettoyage de la table de données, bien qu'elle ai été déclarée new précédemment ?
                ds.Clear();     //clear dataset before loading new content to make sure no data mixup
                // Remplissage du DataSet par l'adaptateur de données SQLite
                sda.Fill(ds);
                // Redefinition des données affichées par la DataGrid
                DataTable dttb = ds.Tables[0];
                DataRow[] filteredRows = dttb.Select("project_completed <> 1");
                DataTable filtered_dttb = dttb.Clone();
                foreach(DataRow row in filteredRows)
                {
                    filtered_dttb.ImportRow(row);
                }
                projectsDataGrid.DataContext = filtered_dttb.DefaultView;
            }
            // Fermeture de la connexion
            handleConn(sqliteconn);
        }

        /// <summary>
        /// Methode appellée lors du clic sur le bouton de création d'un nouveau projet.
        /// Renseigne la base de données des projets avec une requête INSERT.
        /// Récupère toutes les valeurs clés de la création d'un projet.
        /// Le bouton devrait être désactivé par défaut, et activé lorsque toute les demandes de valeur sont satisfaites.
        /// Cette méthode ne s'occupe pas pour l'instant de savoir si les inputs sont corrects, non vides et réglementaires.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void submit_btn_Click(object sender, RoutedEventArgs e)
        {

            if (project_batiment_combobox.SelectedItem != null)
            {
                try
                {
                    // Ce if est particulierement grossier
                    // Si le resultat booléen de la méthode d'insertion est vrai, créé une nouvelle entrée de projet dans la table projets de la base de données SQL
                    // d'après les valeurs récupérées sur les différent contrôles du formulaire. 

                    if (Insert(DateTime.Now.ToString("dd/MM/yyyy"), // Date de création
                               project_name_txtbox.Text, // Nom du projet
                               project_batiment_combobox.SelectedItem.ToString(), // Batiment (nom)
                               0, // booleen entier de completion du projet
                               DateTime.Now.ToString("dd/MM/yyyy"), // Date de fin du projet, pour l'instant forcée sur la date du jour
                               responsable_txtbox.Text, // Nom du responsable du projet
                               concatenatePickedFloors(project_etages_listbox.SelectedItems), // string concaténée de la liste des étages sélectionnés
                               "PDF", // Fichiers PDF à référencer
                               "DWG", // Fichiers DWG à référencer
                               75008, // Code postal à recueillir d'après le batiment
                               "ProjectAdress", // Adresse à recueillir d'après le batiment
                               project_city_txtbox.Text, // Ville recueillie d'après le batiment
                               description_txtbox.Text, // récupère le texte de la description
                               concatenatePickedTypologies(), // concatene les typologies de projet choisies
                               0, // projet est il vu pour la session : non lors de sa création
                               SetupSQLite.sqliteconn))
                    {
                        Debug.WriteLine("-- Insertion du nouveau projet dans la base de données réussie");
                        // Reselection de la base de données pour affichage dans la DataGrid
                        Select(SetupSQLite.sqliteconn);
                    }

                    // Si le resultat de l'insertion est une booleene fausse, signe que l'insertion à échoué
                    else
                    {
                        MessageBox.Show("Oops! Something went wrong");
                    }
                }

                // Tentative de catch d'une exception de format
                catch (System.FormatException ex)
                { MessageBox.Show(ex.Message + " >><< "); }

            }

            else
            {
                Debug.WriteLine("Merci de bien vouloir sélectionner un bâtiment.");
            }



        }

        private string concatenatePickedTypologies()
        {
            string selectedTypos = null;
            if(Border_Typologie_Transfert.Tag != null)
            {
                selectedTypos += Border_Typologie_Transfert.Tag.ToString();
            }
            if(Border_Typologie_Travaux.Tag != null)
            {
                if(selectedTypos != null)
                {
                    selectedTypos += "," + Border_Typologie_Travaux.Tag.ToString();
                }
                else
                {
                    selectedTypos += Border_Typologie_Travaux.Tag.ToString();
                }
            }
            if(Border_Typologie_Mobilier.Tag != null)
            {
                if(selectedTypos != null)
                {
                    selectedTypos += "," + Border_Typologie_Mobilier.Tag.ToString();
                }
                else
                {
                    selectedTypos += Border_Typologie_Mobilier.Tag.ToString();
                }
            }
            return selectedTypos;
        }

        /// <summary>
        /// Méthode de concaténation des éléments de sélectionnés dans la listbox des étages du projet
        /// </summary>
        /// <param name="etages">Prend une liste en paramètre, la IList des valeurs de la listbox</param>
        /// <returns></returns>
        private string concatenatePickedFloors(System.Collections.IList etages)
        {
            string concatenatedPickedFloorsString = null;
            List<string> listString = etages.Cast<string>().ToList();
            if(etages.Count >0 && etages != null)
            {
                if(etages.Count == project_etages_listbox.Items.Count)
                {
                    concatenatedPickedFloorsString = "Tous";
                }
                else
                {
                    concatenatedPickedFloorsString = string.Join(",", listString);
                }
                
            }
            else
            {
                concatenatedPickedFloorsString = "Aucun";
            }
            
            return concatenatedPickedFloorsString;

        }
        /// <summary>
        /// Méthode déclenchée au click du bouton de suppression.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void truncate_btn_Click(object sender, RoutedEventArgs e)
        {
            // Si le retour booléen de l'appel à la méthode Truncate est vrai, afficher le message de confirmation
            if (Truncate(SetupSQLite.sqliteconn))
            {
                // Message de confirmation de la suppression de la table
                MessageBox.Show("Information successfully deleted");
                // Reselection des données de la base SQL pour affichage sur la datagrid
                Select(SetupSQLite.sqliteconn);
            }
        }

        /// <summary>
        /// Méthode controversée par moi, de gérance de la connexion SQLite.
        /// Cette méthode prend en paramètre une connection.
        /// Cette méthode est en réalité un switch de l'état de connection.
        /// </summary>
        /// <param name="sqliteconn">La connection SQLiteConnection à traiter.</param>
        private void handleConn(SQLiteConnection sqliteconn)
        {
            // Si la connexion est close
            if (sqliteconn.State == ConnectionState.Closed)
            {
                // Ouverture de la connexion
                sqliteconn.Open();
            }
            // Si la connexion est ouverte
            else
            {
                // Fermeture de la connexion
                sqliteconn.Close();
            }
        }

        /// <summary>
        /// Cette méthode est déclenchée lorsque la selection du row de la datagrid change, la sélection peut comprendre un seul ou plusieurs rows.
        /// La méthode actualise la position de la carte. Il faudrait coder une mise à l'échelle en fonction de la taille du batiment.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProjectsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("____");
            // Si la selection ne comporte qu'une seule rangée
            if (projectsDataGrid.SelectedItems.Count == 1) 
            {
                Debug.WriteLine("!");
                // Si la rangée selectionnée n'est pas nulle, je n'ai jamais rencontré ce cas pour l'instant
                if (projectsDataGrid.SelectedItem != null)
                {
                    DataRowView dtrv = projectsDataGrid.SelectedItem as DataRowView;
                    var bat_name = dtrv.Row["project_batiment"];
                    // Obtient le DataGridRow sélectionné
                    var selectedRow = (DataGridRow)projectsDataGrid.ItemContainerGenerator.ContainerFromItem(projectsDataGrid.SelectedItem);
                    // Si le row n'est pas nulle
                    if (selectedRow != null)
                    {
                        Location loc = GetBuildingLocation(bat_name.ToString());
                        // Si la localisation obtenue n'est pas nulle
                        if (loc != null)
                        {
                            // Paramétrage de la nouvelle vue de la carte
                            myMap.SetView(loc, 19);
                            // Obtention du PushPin créé dans le constructeur
                            Pushpin pp = myMap.Children[0] as Pushpin;
                            // Déplacement du PushPin aux coordonnées du bâtiment
                            pp.Location = loc;
                        }
                        nom_projet_info.Text = dtrv.Row["project_name"].ToString();
                        nom_batiment_info.Text = dtrv.Row["project_batiment"].ToString();
                        etages_projet_info.Text = dtrv.Row["project_floors"].ToString();
                    }
                }
                // Si la rangée sélectionnée est nulle
                else
                {
                    MessageBox.Show("La rangée sélectionnée n'est pas valide, la valeur retournée est nulle.");
                }
            }

            if(projectsDataGrid.SelectedItems == null)
            {

            }
        }
        /// <summary>
        /// Méthode d'obtention du bâtiment.
        /// Cette méthode utilise une requête SQLite pour obtenir les coordonnées du bâtiment, d'après son nom.
        /// Cette méthode renvoi la Location, créée d'après la string des valeurs "latitude, longitude" stockées dans la BDD.
        /// </summary>
        /// <param name="bat"></param>
        /// <returns></returns>
        public Location GetBuildingLocation(string bat)
        {
            // Création du chemin d'accès à la base de données
            string dbpath = Path.Combine(DBDirectory() + @"\HER_Sqlite_DB\DB_00.sqlite");
            // Formatage de la string de connexion pour SQLite
            string connectionString = "Data Source=" + dbpath + ";Version=3";
            // Prédéclaration de la localisation de la carte
            Location loc = null;
            // Etablissement de la connexion SQLite
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                // Ouverture de la connection
                connection.Open();
                // Etablissement de la requete SQL où @Batiment 
                string query = "SELECT location FROM batiments WHERE nom = @Batiment";
                // Ouverture d'une nouvelle commande SQLite
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    // Ajout du parametre à la commande
                    command.Parameters.AddWithValue("@Batiment", bat);
                    // Création d'un lecteur SQLite
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        // Tant que le lecteur lit
                        while (reader.Read())
                        {
                            // Obtention de la valeur string de la colonne localisation 
                            string valueA = reader["location"].ToString();
                            // Debug de la valeur de localisation
                            Debug.WriteLine(valueA);
                            // Séparation de la chaine par la virgule, en deux valeurs de string
                            string[] coord = valueA.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            // Si le nombre de string obtenues est égal à 2
                            if (coord.Length == 2) 
                            {
                                // Débugage des valeurs obtenues
                                Debug.WriteLine(coord[0].Trim());
                                Debug.WriteLine(coord[1].Trim());
                                // Conversion des valeurs string en valeurs double pour la latitude et la longitude
                                double lat = double.Parse(coord[0].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                double longi = double.Parse(coord[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                // Attribution de la valeur de localisation renvoyée par la fonction
                                loc = new Location(lat, longi);
                                // renvoi de la location à la méthdoe
                                return loc;
                            }
                        }
                    }
                }
            }
            // renvoi de la location à la méthode
            return loc;
        }

        /// <summary>
        /// Tentative de filtrage des row de la datagrid, d'après les valeurs inputées dans les textbox des entêtes de colonnes.
        /// Cette méthode n'est malheureusement pas cumulative. Cette question pourrait être intéressante à poser sur SOF.
        /// Cette méthode est appliquée de la sorte à toutes les textbox des entêtes de colonnes de la datagrid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Filter_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Obtention de la textbox utilisée,
            TextBox t = (TextBox)sender;
            
            Debug.WriteLine(t.Name + " : " + t.Text);
            Debug.WriteLine(projectsDataGrid.DataContext.ToString());
            // Obtention de la DataView de la DataGrid
            DataView dv = projectsDataGrid.DataContext as DataView;
            // Obtention du texte de la TextBox et stockage dans une string
            string filterstring = t.Text;
            // Etablissement du filtre sur la DataView
            dv.RowFilter = t.Tag + " LIKE '%" + t.Text + "%'";
            Debug.WriteLine("-- " + t.Tag);
            


        }

        private void Vu_Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                Debug.WriteLine(button.ToString() + " oooooo");

                // Trouver la DataGridRow parente
                DataGridRow row = FindVisualParent<DataGridRow>(button);
                if (row != null)
                {
                    // Obtenir la valeur de la cellule de la première colonne (ID dans cet exemple)
                    DataRowView rowView = row.Item as DataRowView;
                    if (rowView != null)
                    {
                        int projectId = Convert.ToInt32(rowView["iD"]);
                        Debug.WriteLine($"ID du projet : {projectId}");

                        int current_vue_value = Convert.ToInt32(rowView["project_seen_for_session"]);
                        Debug.WriteLine($"Valeur vue : {current_vue_value}");

                        

                        int newValue = (current_vue_value == 0) ? 1 : 0;
                        Debug.WriteLine($"Nouvelle valeur vue : {newValue}");


                        UpdateProjectSeenForSessionInDatabase(projectId, newValue);

                    }
                }

                
            }
            DataView dataContext = projectsDataGrid.DataContext as DataView;
            string current_filter = dataContext.RowFilter;
            string current_sort = dataContext.Sort;
            Debug.WriteLine(current_filter + " = Filtre courant");
            Debug.WriteLine(current_sort + " = Sort courant");
            Select(SetupSQLite.sqliteconn);
            DataView dataContext_past = projectsDataGrid.DataContext as DataView;
            dataContext_past.RowFilter = current_filter;
            DataView dataContext_past_2 = projectsDataGrid.DataContext as DataView;
            dataContext_past_2.Sort = current_sort;

        }

        // Méthode utilitaire pour trouver le parent visuel d'un type spécifique
        public static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            return parent ?? FindVisualParent<T>(parentObject);
        }

        private void UpdateProjectSeenForSessionInDatabase(int projectId, int newValue)
        {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + SetupSQLite.DB_PATH + ";Version=3;"))
            {
                connection.Open();
                Debug.WriteLine("Connexion BDD etablie");
                string sql = "UPDATE project SET project_seen_for_session = @newValue WHERE iD = @projectId";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@newValue", newValue);
                    command.Parameters.AddWithValue("@projectId", projectId);
                    command.ExecuteNonQuery();
                    Debug.WriteLine("Transaction BDD effectuee");
                }
            }
        }

        private void UpdateProjectCompletionInDatabase(int projectId, int newValue)
        {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + SetupSQLite.DB_PATH + ";Version=3;"))
            {
                connection.Open();
                Debug.WriteLine("Connexion BDD etablie");
                string sql = "UPDATE project SET project_completed = @newValue WHERE iD = @projectId";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@newValue", newValue);
                    command.Parameters.AddWithValue("@projectId", projectId);
                    command.ExecuteNonQuery();
                    Debug.WriteLine("Transaction BDD effectuee");
                }
            }
        }

        private void ContextMenuItemVue_Click(object sender, RoutedEventArgs e) 
        {
            MenuItem mit = sender as MenuItem;
            if(mit.Header.ToString() == "Marquer le(s) Projet(s) comme Vus")
            {
                //MessageBox.Show("Hey");
                var selection = projectsDataGrid.SelectedItems;
                foreach (DataRowView dgr in selection)
                {
                    // identifiants des Rows
                    var id_values = dgr.Row["iD"];
                    Debug.WriteLine("iD du row : " + id_values);
                    UpdateProjectSeenForSessionInDatabase(Convert.ToInt32(id_values), 1);

                }
                DataView dataContext = projectsDataGrid.DataContext as DataView;
                string current_filter = dataContext.RowFilter;
                string current_sort = dataContext.Sort;
                Debug.WriteLine(current_filter + " = Filtre courant");
                Debug.WriteLine(current_sort + " = Sort courant");
                Select(SetupSQLite.sqliteconn);
                DataView dataContext_past = projectsDataGrid.DataContext as DataView;
                dataContext_past.RowFilter = current_filter;
                DataView dataContext_past_2 = projectsDataGrid.DataContext as DataView;
                
                dataContext_past_2.Sort = current_sort;
            }
            else
            {
                var selection = projectsDataGrid.SelectedItems;
                foreach (DataRowView dgr in selection)
                {
                    // identifiants des Rows
                    var id_values = dgr.Row["iD"];
                    Debug.WriteLine("iD du row : " + id_values);
                    UpdateProjectSeenForSessionInDatabase(Convert.ToInt32(id_values), 0);

                }
                DataView dataContext = projectsDataGrid.DataContext as DataView;
                string current_filter = dataContext.RowFilter;
                string current_sort = dataContext.Sort;
                Debug.WriteLine(current_filter + " = Filtre courant");
                Debug.WriteLine(current_sort + " = Sort courant");
                Select(SetupSQLite.sqliteconn);
                DataView dataContext_past = projectsDataGrid.DataContext as DataView;
                dataContext_past.RowFilter = current_filter;
                DataView dataContext_past_2 = projectsDataGrid.DataContext as DataView;
                dataContext_past_2.Sort = current_sort;
            }
            
        }

        private void ContextMenuItem_CloseProjects(object sender, RoutedEventArgs e)
        {
            var selection = projectsDataGrid.SelectedItems;
            foreach (DataRowView dgr in selection)
            {
                // identifiants des Rows
                var id_values = dgr.Row["iD"];
                Debug.WriteLine("iD du row : " + id_values);
                UpdateProjectCompletionInDatabase(Convert.ToInt32(id_values), 1);
            }
            DataView dataContext = projectsDataGrid.DataContext as DataView;
            string current_filter = dataContext.RowFilter;
            string current_sort = dataContext.Sort;
            Debug.WriteLine(current_filter + " = Filtre courant");
            Debug.WriteLine(current_sort + " = Sort courant");
            Select(SetupSQLite.sqliteconn);
            DataView dataContext_past = projectsDataGrid.DataContext as DataView;
            dataContext_past.RowFilter = current_filter;
            DataView dataContext_past_2 = projectsDataGrid.DataContext as DataView;
            dataContext_past_2.Sort = current_sort;

        }

        private void ContextMenuItem_DeleteProjects(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Ha !");
        }
    }
}
