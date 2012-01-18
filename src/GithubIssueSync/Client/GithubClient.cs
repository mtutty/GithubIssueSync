using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data;
using System.Collections.Specialized;

namespace GithubIssueSync.Client {
    public class GithubClient {
        public string UserName { get; set; }
        private string Password = null;
        public string Token { get; set; }
        public string LastResponse { get; protected set; }
        public JObject User { get; private set; }

        public string BaseUrl { get; set; }

        public GithubClient() : this(@"https://api.github.com") { }

        public GithubClient(string baseUrl) {
            this.BaseUrl = baseUrl;
        }

        public string GetRequestUrl(string relativePath) {
            return this.BaseUrl + relativePath;
        }

        public void Authenticate(string userName, string pwd) {
            this.UserName = userName;
            this.Password = pwd;
            //this.RequestToJSONArrayResponse(@"/issues");
            this.User = GetUserInfo(this.UserName);
        }

        private HttpWebRequest GetAuthenticatedRequest(string requestPath) {
            string url = GetRequestUrl(requestPath);
            HttpWebRequest req = HttpWebRequest.Create(url) as HttpWebRequest;

            string basicauth = Convert.ToBase64String(Encoding.UTF8.GetBytes(this.UserName + @":" + this.Password));
            req.Headers.Add(@"Authorization", @"Basic " + basicauth);
            return req;
        }

        public virtual string PostRequest(string requestPath, string requestBody) {
            HttpWebRequest req = GetAuthenticatedRequest(requestPath);
            req.Method = @"POST";
            using (StreamWriter sw = new StreamWriter(req.GetRequestStream())) {
                sw.WriteLine(requestBody);
                sw.WriteLine();
            }
            using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse) {
                if (resp == null) throw new NullReferenceException(@"The web request to " + requestPath + @" did not receive a response");
                if (resp.StatusCode >= HttpStatusCode.BadRequest) throw new Exception(@"The web request to " + requestPath + " returned HTTP Status " + resp.StatusCode.ToString());
                using (StreamReader sr = new StreamReader(resp.GetResponseStream())) {
                    this.LastResponse = sr.ReadToEnd();
                    return this.LastResponse;
                }
            }
        }

        public virtual string GetRequest(string requestPath) {
            using (HttpWebResponse resp = GetAuthenticatedRequest(requestPath).GetResponse() as HttpWebResponse) {
                if (resp == null) throw new NullReferenceException(@"The web request to " + requestPath + @" did not receive a response");
                if (resp.StatusCode >= HttpStatusCode.BadRequest) throw new Exception(@"The web request to " + requestPath + " returned HTTP Status " + resp.StatusCode.ToString());
                using (StreamReader sr = new StreamReader(resp.GetResponseStream())) {
                    this.LastResponse = sr.ReadToEnd();
                    return this.LastResponse;
                }
            }
        }

        public JObject GetUserInfo(string userName) {
            return RequestToJSONResponse(@"/users/" + userName);
        }

        public JObject RequestToJSONResponse(string requestPath) {
            string response = GetRequest(requestPath);
            return JObject.Parse(response);
        }

        protected static void AppendJsonData(string rawJson, DataTable dt) {
            string json = string.Format("{0}result: {1}{2}", @"{", rawJson, @"}");

            JObject root = JObject.Parse(json);
            JArray items = (JArray)root["result"];

            foreach (JObject item in items) {

                DataRow dr = dt.NewRow();

                string temp;
                foreach (DataColumn col in dt.Columns) {
                    JToken val = item.SelectToken(col.Caption);
                    if (val == null) continue;
                    dr[col] = val.ToString();
                }
                dt.Rows.Add(dr);
            }
            dt.AcceptChanges();
        }

        private static DataTable CreateIssuesTable() {
            DataTable ret = new DataTable(@"GithubIssues");
            StringDictionary sd = new StringDictionary();

            sd.Add(@"id", @"id");
            sd.Add("state", "state");
            sd.Add("milestone", "milestone.title");
            sd.Add("user", "user.login");
            sd.Add("created_at", "created_at");
            sd.Add("assignee", "assignee.login");
            sd.Add("updated_at", "updated_at");
            sd.Add("title", "title");
            sd.Add("labels", "labels[0].name");
            sd.Add("comments", "comments");
            sd.Add("number", "number");
            sd.Add("html_url", "html_url");
            sd.Add("pull_request", "pull_request.html_url");
            sd.Add("url", "url");
            sd.Add("closed_at", "closed_at");

            foreach (string colName in sd.Keys) {
                ret.Columns.Add(colName).Caption = sd[colName];
            }
            return ret;
        }

        private static string GetName(JToken token) {
            JProperty prop = token as JProperty;
            if (prop == null) return string.Empty;
            return prop.Name;
        }

        public DataTable RequestToDataTable(string requestPath) {
            DataTable ret = CreateIssuesTable();
            AppendJsonData(GetRequest(requestPath), ret);
            return ret;
        }

        public DataTable ListIssues(string userOrOrg, string repo, DateTime? since) {
            // GET /repos/:user/:repo/issues
            int pageSize = 100;
            int page = 0;

            string template = @"/repos/{0}/{1}/issues?state=open,closed&page={2}&per_page={3}";
            if (since.HasValue)
                template = @"/repos/{0}/{1}/issues?state=closed&page={2}&per_page={3}&since={4:s}&sort=updated&direction=desc";
            DataTable ret = CreateIssuesTable();

            while (ret.Rows.Count == (page * pageSize)) {
                if (since.HasValue)
                    AppendJsonData(GetRequest(string.Format(template, userOrOrg, repo, page++, pageSize, since)), ret);
                else
                    AppendJsonData(GetRequest(string.Format(template, userOrOrg, repo, page++, pageSize)), ret);
            };

            return ret;
        }

        public DataTable ListIssues(string userOrOrg, string repo) {
            return ListIssues(userOrOrg, repo, null);
        }

        public DataTable ListIssues(string filter, int pageSize, int pageNum, string sortField, bool sortAscending) {
            // GET /issues
            string requestPath = string.Format(@"/issues?filter={0}&sort={1}&direction={2}", filter, sortField, sortAscending ? @"asc" : @"desc");
            return RequestToDataTable(requestPath);
        }


        internal void CreateIssue(string userOrOrg, string repo, string title, string body, string assignee, int milestone) {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            var item = new { title = title, body = body, assignee = assignee, milestone = milestone };
            new JsonSerializer().Serialize(sw, item);
            string json = sb.ToString();

            // POST /repos/:user/:repo/issues
            string requestPath = string.Format(@"/repos/{0}/{1}/issues", userOrOrg, repo);
            PostRequest(requestPath, json);
        }
    }
}
