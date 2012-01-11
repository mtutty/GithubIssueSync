using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GithubIssueSync.Util {
    public class CSVToDataTable {
        public static System.Data.DataTable GetDataTable(string strFileName) {
            string fullPath = System.IO.Path.GetFullPath(strFileName);
            string fileLocation = System.IO.Path.GetDirectoryName(fullPath);
            string tableName = System.IO.Path.GetFileName(fullPath);

            string connTemplate = "Driver={0}; Dbq={1}; Extensions=asc,csv,tab,txt;Persist Security Info=False";
            string connString = string.Format(connTemplate, @"{Microsoft Text Driver (*.txt; *.csv)}", fileLocation);
            System.Data.Odbc.OdbcConnection conn = new System.Data.Odbc.OdbcConnection(connString);
            conn.Open();

            string strQuery = string.Format("SELECT * FROM [{0}]", tableName);
            System.Data.Odbc.OdbcDataAdapter adapter = new System.Data.Odbc.OdbcDataAdapter(strQuery, conn);
            System.Data.DataSet ds = new System.Data.DataSet();
            adapter.Fill(ds);
            return ds.Tables[0];
        }
    }
}
