using System.Data;
using System.Collections;
using System.IO;
using System.Text;

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
            StringBuilder sb = new StringBuilder();
            string delim = @"";
            foreach (object field in row.ItemArray) {
                sb.Append(delim);
                if (field != null)
                    sb.Append(QuotedString(field.ToString()));
                delim = @",";
            }
            return sb.ToString();
        }

        public static string QuotedString(string val) {
            string temp = val.Replace("\"", "\\\"");
            if (temp.Contains(",")) temp = "\"" + temp + "\"";
            return temp;
        }
    }
}
