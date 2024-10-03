using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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

namespace CopyDatabaseToOtherDatabase
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (Properties.Settings.Default.SourceConnectionString.Length > 1 && Properties.Settings.Default.DestinationConnectionString.Length > 1)
            {
                SetSourceTreeView();
                SetDestinationTreeView();
            }
            EnableButtons(false);
        }

        #region Private Methods
        private void SetSourceTreeView()
        {
            DB_Connection dbc = new DB_Connection();
            var list = dbc.ListTables(Properties.Settings.Default.SourceConnectionString, Properties.Settings.Default.NameSourceDataBase);
            tviSourceTable.ItemsSource = list;
            tviNameSourceDB.Header = Properties.Settings.Default.NameSourceServer + @"\" + Properties.Settings.Default.NameSourceDataBase;
        }

        private void SetDestinationTreeView()
        {
            DB_Connection dbc = new DB_Connection();
            var list = dbc.ListTables(Properties.Settings.Default.DestinationConnectionString, Properties.Settings.Default.NameDestinationDataBase);
            tviDestinationTable.ItemsSource = list;
            tviNameDestinationDB.Header = Properties.Settings.Default.NameDestinationServer + @"\" + Properties.Settings.Default.NameDestinationDataBase;
        }

        private void CopySchemaOnTable()
        {
            string table_name = tvSourceTree.SelectedItem.ToString();
            DB_Connection dbc = new DB_Connection();
            string create_table = dbc.GenerateCreateOfTable(Properties.Settings.Default.SourceConnectionString, table_name);
            if (dbc.ExecuteCreateTable(Properties.Settings.Default.DestinationConnectionString, create_table, table_name))
                MessageBox.Show("Struktura tabeli " + table_name + "\n\nZostała poprawnie przeniesiona do bazy:\n" + Properties.Settings.Default.NameDestinationServer + @"\" + Properties.Settings.Default.NameDestinationDataBase, "Zakładanie tabeli ", MessageBoxButton.OK, MessageBoxImage.Information);
            else MessageBox.Show("Struktura tabeli " + table_name + "\n\nNie została prawidłowo przeniesiona \n\nLog błędu:\n" + dbc.ErrorMessage, "Błąd zakładania tabeli", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void CopyDataOnTable()
        {
            DB_Connection dbc = new DB_Connection();
            string table_name = tvSourceTree.SelectedItem.ToString();
            if (dbc.CheckTableIsExist(Properties.Settings.Default.DestinationConnectionString, table_name))
            {       
                DataTable dt = dbc.SelectAllFromTable(Properties.Settings.Default.SourceConnectionString, table_name);
                string columns = "";
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    columns += i > 0 ? ";" + dt.Columns[i].ColumnName : dt.Columns[i].ColumnName;
                }
                var col = columns.Split(';');
                string sql_query = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string insert = ""; string values = "";
                    for (int z = 0; z < col.Length; z++)
                    {
                        if (!dbc.CheckColumnIsIdentity(Properties.Settings.Default.SourceConnectionString, string.Format("SELECT columnproperty(object_id('{0}'),'{1}','IsIdentity')", table_name, col[z].Replace("[", "").Replace("]", ""))))
                        {
                            insert += !string.IsNullOrEmpty(insert) ? ",[" + col[z] + "]" : "[" + col[z] + "]";
                            values += !string.IsNullOrEmpty(values) ? ",'" + dt.Rows[i][col[z]].ToString() + "'" : "'" + dt.Rows[i][col[z]].ToString() + "'";
                        }
                    }
                    sql_query += "INSERT INTO " + table_name + " ( " + insert + " ) VALUES ( " + values + " )\n";
                }
                if (dbc.ExecuteQuery(Properties.Settings.Default.DestinationConnectionString, sql_query))
                    MessageBox.Show("Dane do tabeli " + table_name + "\nzostały przeniesione prawidłowo", "Przenoszenie danych", MessageBoxButton.OK, MessageBoxImage.Information);
                else MessageBox.Show("Nie można przenieść danych\nUżyj opcji \"Schema and Data\"\nJeśli błąd nadal występuje sprawdź ustawienia uprawnień w bazie docelowej.", "Błąd przenoszenia danych", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show("Nie można przenieść danych, prawdopodobnie tabela \ndo której próbujesz zapisać dane, nie istnieje !\n\nUżyj opcji \"Schema and Data\"\nJeśli błąd nadal występuje sprawdź ustawienia uprawnień w bazie docelowej.", "Błąd przenoszenia danych", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CreateExampleTable()
        {
            DB_Connection dbc = new DB_Connection();
            if (dbc.ExecuteQuery(Properties.Settings.Default.SourceConnectionString, Properties.Resources.CreateExampleTable))
                MessageBox.Show("Tabela CennikDostawcow została poprawnie dodana\ndo listy tabel bazy: " + Properties.Settings.Default.NameSourceDataBase, "Poprawna implementacja przykładowej tabeli", MessageBoxButton.OK, MessageBoxImage.Information);
            else MessageBox.Show("Nie można utworzyć nowej tabeli", "Odmowa dostępu", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void EnableButtons(bool Set)
        {
            btnSchemaOnly.IsEnabled = Set;
            btnDataOnly.IsEnabled = Set;
            btnSchemaAndData.IsEnabled = Set;
        }
        #endregion

        #region Menu Event
        private void miDestSQL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ConnectionDB sourceDB = Properties.Settings.Default.DestinationConnectionString.Length > 1 ? new ConnectionDB(Properties.Settings.Default.DestinationConnectionString) : new ConnectionDB();
                sourceDB.ShowDialog();
                if ((bool)sourceDB.DialogResult)
                {
                    string conn = sourceDB.Get_Connection;
                    Properties.Settings.Default.DestinationConnectionString = conn;
                    if (conn.Split(';')[0].ToLower().Contains("true"))
                    {
                        Properties.Settings.Default.NameDestinationDataBase = conn.Split(';')[3].Split('=')[1];
                        Properties.Settings.Default.NameDestinationServer = conn.Split(';')[4].Split('=')[1];
                    }
                    else
                    {
                        Properties.Settings.Default.NameDestinationDataBase = conn.Split(';')[2].Split('=')[1];
                        Properties.Settings.Default.NameDestinationServer = conn.Split(';')[3].Split('=')[1];
                    }
                    
                    Properties.Settings.Default.Save();
                    SetDestinationTreeView();
                }
            }
            catch (Exception)
            {

            }
        }

        private void miSourSQL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ConnectionDB sourceDB = Properties.Settings.Default.SourceConnectionString.Length > 1? new ConnectionDB(Properties.Settings.Default.SourceConnectionString) : new ConnectionDB();
                sourceDB.ShowDialog();
                if ((bool)sourceDB.DialogResult)
                {
                    string conn = sourceDB.Get_Connection;
                    Properties.Settings.Default.SourceConnectionString = conn;
                    if (conn.Split(';')[0].ToLower().Contains("true"))
                    {
                        Properties.Settings.Default.NameSourceDataBase = conn.Split(';')[3].Split('=')[1];
                        Properties.Settings.Default.NameSourceServer = conn.Split(';')[4].Split('=')[1];
                    }
                    else
                    {
                        Properties.Settings.Default.NameSourceDataBase = conn.Split(';')[2].Split('=')[1];
                        Properties.Settings.Default.NameSourceServer = conn.Split(';')[3].Split('=')[1];
                    }
                    Properties.Settings.Default.Save();
                    SetSourceTreeView();
                }
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region Button Event
        private void btnSchemaOnly_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            CopySchemaOnTable();
            SetDestinationTreeView();
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void btnDataOnly_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            CopyDataOnTable();
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void btnSchemaAndData_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            CopySchemaOnTable();
            CopyDataOnTable();
            SetDestinationTreeView();
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void btnCreateExampleTable_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            CreateExampleTable();
            SetSourceTreeView();
            Mouse.OverrideCursor = Cursors.Arrow;
        }
        #endregion

        #region TreeView Event
        private void tviSourceTable_Selected(object sender, RoutedEventArgs e)
        {
            EnableButtons(!tvSourceTree.SelectedItem.ToString().ToLower().Contains("tabele"));
        }
        #endregion
    }
}
