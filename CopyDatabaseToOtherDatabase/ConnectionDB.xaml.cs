using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
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
using System.Windows.Shapes;

namespace CopyDatabaseToOtherDatabase
{
   
    public partial class ConnectionDB : Window
    {
        private bool bLadowanie = false;
        bool bZmianPodmiotu = false;

        public ConnectionDB()
        {
            InitializeComponent();
            var brush = new ImageBrush();
            //brush.ImageSource = DlaInsERT_WPF.Properties.Resources.refresh_icon;
            //todo
            //brush.ImageSource  = new BitmapImage(new Uri(@"Resources/refresh_icon.png", UriKind.Relative));
            radBTNrefreshDB.Background = brush;
            radBTNrefreshPodmioty.Background = brush;
        }

        public ConnectionDB(string sConnString)
        {
            InitializeComponent();
            UstawKontrolkiSQL(sConnString);
            var brush = new ImageBrush();
            radBTNrefreshDB.Background = brush;
            radBTNrefreshPodmioty.Background = brush;
        }

        public string Get_Connection
        {
            get
            {
                return ZrobConnStringDB();
            }
        }

        private void UstawKontrolkiSQL(string sConnString)
        {
            try
            {
                bLadowanie = true;
                string[] result;
                result = sConnString.Split(';');
                foreach (string entry in result)
                {
                    if (entry.IndexOf("Password", 0) > -1)
                    {
                        radUserSQLHaslo.Password = entry.Substring(entry.IndexOf("=", 0) + 1);
                    }
                    else if (entry.IndexOf("Security", 0) > -1)
                    {
                        if (entry.ToLower().Contains("true"))
                        {
                            radAutentykacja.IsChecked = false;
                            radUserSQL.Text = "";
                            radUserSQLHaslo.Password = "";
                            
                        }
                        else
                        {
                            radAutentykacja.IsChecked = true;
                        }
                        radUserSQL.IsEnabled = (bool)!radAutentykacja.IsChecked;
                    }
                    else if (entry.IndexOf("User ID", 0) > -1)
                    {
                        radUserSQL.Text = entry.Substring(entry.IndexOf("=", 0) + 1);
                    }
                    else if (entry.IndexOf("Initial Catalog", 0) > -1)
                    {
                        radBazyDanych.Items.Add(entry.Substring(entry.IndexOf("=", 0) + 1));
                    }
                    else if (entry.IndexOf("Data Source", 0) > -1)
                    {
                        radSerwerSQL.Items.Add(entry.Substring(entry.IndexOf("=", 0) + 1));
                    }

                }
            }
            catch
            {
                
            }
            finally
            {
                radSerwerSQL.SelectedIndex = 0;
                radBazyDanych.SelectedIndex = 0;
                bLadowanie = false;
            }
        }

        private void LoadServers()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var brush = new ImageBrush();
                radBTNrefreshDB.Background = brush;
                radSerwerSQL.Items.Clear();
                radBazyDanych.Items.Clear();
                DataTable servers = SqlDataSourceEnumerator.Instance.GetDataSources();

                for (int i = 0; i < servers.Rows.Count; i++)
                {
                    radSerwerSQL.Items.Add(servers.Rows[i]["ServerName"] + "\\" + servers.Rows[i]["InstanceName"]);
                }
                radSerwerSQL.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
            }
            finally
            {
                var brush = new ImageBrush();
                radBTNrefreshDB.Background = brush;
                Mouse.OverrideCursor = null;
            }
        }

        private void LoadDatabase()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                radBazyDanych.Items.Clear();
                using (SqlCommand comm = new SqlCommand())
                {
                    using (SqlConnection myConn = new SqlConnection(ZrobTestConnString("")))
                    {
                        myConn.Open();
                        string strBaza;
                        using (SqlCommand conn = new SqlCommand("SELECT name FROM master.dbo.sysdatabases WHERE databasepropertyex( name, 'status' ) = 'ONLINE'", myConn))
                        {
                            using (SqlDataReader dr = conn.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    strBaza = dr["name"].ToString();
                                    radBazyDanych.Items.Add(strBaza);

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (radBazyDanych.Items.Count > 0)
                    radBazyDanych.SelectedIndex = 0;
            }
            Mouse.OverrideCursor = null;
        }

        private string ZrobConnStringDB()
        {
            string cs = "";
            if ((bool)radAutentykacja.IsChecked)
            {
                cs = String.Format("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog={0};Data Source={1}"
                    , radBazyDanych.Text
                    , radSerwerSQL.Text);
            }
            else
            {
                cs = String.Format("Persist Security Info=True;User ID={0};Password={1};Initial Catalog={2};Data Source={3}"
                    , radUserSQL.Text
                    , radUserSQLHaslo.Password
                    , radBazyDanych.Text
                    , radSerwerSQL.Text);
            }
            return cs;
        }

        private string ZrobDBConnString(string strDatabase)
        {
            string cs = "";
            if ((bool)radAutentykacja.IsChecked)
            {
                cs = String.Format("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog={0};Data Source={1}"
                    , strDatabase
                    , radSerwerSQL.Text);
            }
            else
            {
                cs = String.Format("Persist Security Info=True;User ID={0};Password={1};Initial Catalog={2};Data Source={3}"
                    , radUserSQL.Text
                    , radUserSQLHaslo.Password
                    , strDatabase
                    , radSerwerSQL.Text);
            }
            return cs;
        }

        private string ZrobTestConnString(string strDatabase)
        {
            string cs = "";
            if ((bool)radAutentykacja.IsChecked)
            {
                cs = String.Format("Integrated Security=True;Initial Catalog={0};Data Source={1}", strDatabase, radSerwerSQL.Text);
            }
            else
            {
                cs = String.Format("Persist Security Info=True;User ID={0};Password={1};Initial Catalog={2};Data Source={3}", radUserSQL.Text, radUserSQLHaslo.Password, strDatabase, radSerwerSQL.Text);
            }
            return cs;
        }

        private void radLBLtestSQL_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                using (SqlConnection testConn = new SqlConnection())
                {
                    testConn.ConnectionString = ZrobDBConnString("");
                    testConn.Open();
                    MessageBox.Show("Połączenie do serwera SQL zakończone sukcesem!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);

                    testConn.Close();

                    LoadDatabase();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd połączenia ..." + ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void radAutentykacja_Checked(object sender, RoutedEventArgs e)
        {
            radUserSQL.IsEnabled = (bool)!radAutentykacja.IsChecked;
            radUserSQLHaslo.IsEnabled = (bool)!radAutentykacja.IsChecked;
        }

        private void radBTNzapisz_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void radBTNanuluj_Click(object sender, RoutedEventArgs e)
        {
            WindowConnectionDB.DialogResult = false;
            WindowConnectionDB.Close();
        }

        private void radSerwerSQL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!bLadowanie)
                LoadDatabase();
        }

        private void radBTNrefreshDB_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            LoadServers();
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void radBTNrefreshPodmioty_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            LoadDatabase();
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void radBazyDanych_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bZmianPodmiotu = true;
        }

        private void radLBLtestDB_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                using (SqlConnection testConn = new SqlConnection())
                {
                    testConn.ConnectionString = ZrobDBConnString(string.Format("{0}", radBazyDanych.Text));
                    testConn.Open();

                    MessageBox.Show("Połączenie do bazy danych serwera SQL zakończone sukcesem!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);

                    testConn.Close();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd połączenia ..." + ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Mouse.OverrideCursor = Cursors.Arrow;
        }

    }
}
