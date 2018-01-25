using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DicScrapper {
    public class OracleCon : IDisposable {
        OracleConnection con;
        private void Connect() {
            con = new OracleConnection();
            con.ConnectionString = "User Id=hr;Password=hr;Data Source=xe";
            con.Open();
            Console.WriteLine("Connected to Oracle" + con.ServerVersion);
        }

        public OracleCon() {
            Connect();
        }

        ~OracleCon() {
            Dispose(false);
        }

        public void Close() {
            con.Close();
            con.Dispose();
        }

        public void SelectQuery(string SQL) {
            DataSet ds = new DataSet();
            OracleDataAdapter ad = new OracleDataAdapter();
            ad.SelectCommand = new OracleCommand(SQL, con);
            ad.Fill(ds, "query");
            var something = from dr in ds.Tables["query"].AsEnumerable() select dr;
            var field = something.Where(p => p.Field<int>("num") == 123);
            
            Console.WriteLine(ds.Tables["query"].Rows.Count);
        }

        public bool isNumDBExists(string url) {
            DataSet ds = new DataSet();
            OracleDataAdapter ad = new OracleDataAdapter();
            //entry/jk/JK000000303212.nhn
            var regex = Regex.Match(url, @"/entry/jk/JK(?<num>\d*).nhn");
            if (regex.Success) {
                string num = regex.Groups["num"].Value;
                string SQL = $"SELECT num FROM jwords where num={Convert.ToInt32(num)}";
                ad.SelectCommand = new OracleCommand(SQL, con);
                ad.Fill(ds, "query");
                if(ds.Tables["query"].Rows.Count > 0) {
                    return true;
                }
            }
            return false;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing) {
            if(disposing) {

            }
            Close();
        }
    }
}
