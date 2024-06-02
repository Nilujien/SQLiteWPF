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

namespace SQLiteWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            setup();

            


        }

        public static string DBDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);       //fetch App Data Local directory
        }

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
            CBB_Buildings.ItemsSource = buildingNames;


        }

        bool Insert(DateTime date, string name, int age, double amount, string comments, SQLiteConnection sqliteconn)
        {
            var command = sqliteconn.CreateCommand();
            command.CommandText = "INSERT INTO project(date, name, age, amount, comments) VALUES ('" + date + "', '" + name + "','" + age + "','" + amount + "', '" + comments + "')";
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


        public void Select(SQLiteConnection sqliteconn)      //selecting all data in SQLite db and displaying it in a ListView named listView as declared in XAML
        {
            DataSet ds = new DataSet();        //dataset used to hold content returned from SQLite db

            string str_query = "SELECT id, date, name, age, amount, comments FROM project ORDER BY id DESC"; //consider using LINQ to SQL
            using (var cmd = new SQLiteCommand(str_query))
            {
                SQLiteDataAdapter sda = new SQLiteDataAdapter();
                handleConn(sqliteconn);
                sda.SelectCommand = cmd;
                cmd.Connection = sqliteconn;
                ds.Clear();     //clear dataset before loading new content to make sure no data mixup
                sda.Fill(ds);
                listView.DataContext = ds.Tables[0].DefaultView;
            }
            handleConn(sqliteconn);
        }


        private void submit_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Insert(DateTime.Now, name_txtbox.Text, int.Parse(age_txtbox.Text), double.Parse(amount_txtbox.Text), comments_txtbox.Text, SetupSQLite.sqliteconn))
                {
                    //MessageBox.Show("Information successfully submitted");
                    Select(SetupSQLite.sqliteconn);

                }
                else
                {
                    MessageBox.Show("Oops! Something went wrong");
                }
            }


            catch (System.FormatException ex)
            { MessageBox.Show(ex.Message + " >><< "); }

            
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
