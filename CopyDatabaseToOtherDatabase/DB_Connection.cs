using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyDatabaseToOtherDatabase
{
    public class DB_Connection
    {
        public string ErrorMessage { get; set; }

        public List<string> ListTables(string ConnectionString, string DataBaseName)
        {
            try
            {
                List<string> ListTable = new List<string>();
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand command = conn.CreateCommand())
                    {
                        command.CommandText = string.Format("SELECT '[' + TABLE_SCHEMA + '].[' + TABLE_NAME + ']' FROM {0}.INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' ORDER BY TABLE_SCHEMA", DataBaseName);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ListTable.Add(reader.GetString(0));
                            }
                        }
                    }
                    conn.Close();
                }
                return ListTable;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return null;
            }
        }

        public string GenerateCreateOfTable(string ConnectionString, string NameTable)
        {
            try
            {
                string ret = string.Format("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{0}') AND type in (N'U')) BEGIN DROP TABLE {1} END \n", NameTable.Replace("[","").Replace("]",""), NameTable);
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand command = conn.CreateCommand())
                    {
                        command.CommandText = string.Format(Properties.Resources.CreateOfTable, NameTable.Replace("[","").Replace("]", ""));
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ret += reader.GetString(0);
                            }
                        }
                    }
                    conn.Close();
                }
                return ret;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return null;
            }
        }

        public bool ExecuteCreateTable(string ConnectionString, string Query, string NameTable)
        {
            try
            {
                string ret = "";
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand command = conn.CreateCommand())
                    {
                        command.CommandText = Query;// NameTable.Replace("[", "").Replace("]", ""));
                        command.ExecuteReader();
                    }
                    conn.Close();
                    conn.Open();
                    using (SqlCommand command = conn.CreateCommand())
                    {
                        command.CommandText = string.Format("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{0}') AND type in (N'U'))BEGIN SELECT 'true' END ELSE BEGIN SELECT 'false' END", NameTable);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ret = reader.GetString(0);
                            }
                        }
                    }
                    conn.Close();
                }
                return Convert.ToBoolean(ret);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }

        public DataTable SelectAllFromTable(string ConnectionString, string NameTable)
        {
            try
            {
                DataTable dt = null;
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand command = conn.CreateCommand())
                    {
                        command.CommandText = string.Format("SELECT * FROM {0}", NameTable);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            dt = new DataTable();
                            dt.Load(reader);
                            
                        }
                    }
                    conn.Close();
                }
                return dt;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return null;
            }
        }

        public bool ExecuteQuery(string ConnectionString, string Query)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand command = conn.CreateCommand())
                    {
                        command.CommandText = Query.Replace("GO"," ");
                        command.ExecuteNonQuery();
                    }
                    conn.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }

        public bool CheckColumnIsIdentity(string ConnectionString, string Query)
        {
            try
            {
                int ret = 0;
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand command = conn.CreateCommand())
                    {
                        command.CommandText = Query;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ret = reader.GetInt32(0);
                            }
                        }
                    }
                    conn.Close();
                }
                return Convert.ToBoolean(ret);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }

        public bool CheckTableIsExist(string ConnectionString, string TableName)
        {
            try
            {
                int ret = 0;
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand command = conn.CreateCommand())
                    {
                        command.CommandText = string.Format("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{0}') AND type in (N'U')) BEGIN SELECT 1 END ELSE BEGIN SELECT 0 END", TableName);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ret = reader.GetInt32(0);
                            }
                        }
                    }
                    conn.Close();
                }
                return Convert.ToBoolean(ret);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }
    }
}
