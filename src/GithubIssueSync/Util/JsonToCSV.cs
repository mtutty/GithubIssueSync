using System.Data;
using System.Collections;
using System.IO;

namespace GithubIssueSync.Util {
    public class JsonToCSV {
        public static void WriteAll(DataTable dt, TextWriter writer) {
            writer.WriteLine(HeadingList(dt));
            foreach (DataRow row in dt.Rows) {
                writer.WriteLine(ValueList(row));
            }
            writer.Flush();
        }

        public static string HeadingList(DataTable dt) {
            ArrayList al = new ArrayList();
            foreach (DataColumn col in dt.Columns) {
                al.Add(col.ColumnName);
            }
            return string.Join(@", ", al.ToArray());
        }

        public static string ValueList(DataRow row) {
            return string.Join(@", ", row.ItemArray);
        }
    }
}
