using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data;

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

        protected static DataTable JsonToDataTable(string rawJson) {
            string json = string.Format("{0}result: {1}{2}", @"{", rawJson, @"}");

            DataTable ret = new DataTable();
            JObject root = JObject.Parse(json);
            JArray items = (JArray)root["result"];
            JObject item;
            JToken jtoken;
            JProperty prop;
            string propName;

            if (items.Count > 0) {
                item = (JObject)items[0];
                jtoken = item.First;

                while (jtoken != null) {
                    prop = jtoken as JProperty;
                    propName = prop.Name.ToString();
                    if (prop != null && prop.Type != JTokenType.Array && prop.Type != JTokenType.Constructor)
                        ret.Columns.Add(propName);
                    jtoken = jtoken.Next;
                }
            }

            for (int i = 0; i < items.Count; i++) {

                DataRow dr = ret.NewRow();
                item = (JObject)items[i];
                jtoken = item.First;

                while (jtoken != null) {
                    prop = jtoken as JProperty;
                    propName = prop.Name.ToString();
                    if (prop != null && ret.Columns.Contains(propName))
                        dr[propName] = prop.Value.ToString();
                    jtoken = jtoken.Next;
                }
                ret.Rows.Add(dr);
            }
            ret.AcceptChanges();
            return ret;
        }


        public DataTable RequestToDataTable(string requestPath) {
            return JsonToDataTable(GetRequest(requestPath));
        }

        public DataTable ListIssues(string userOrOrg, string repo) {
            // GET /repos/:user/:repo/issues
            string requestPath = string.Format(@"/repos/{0}/{1}/issues", userOrOrg, repo);
            return RequestToDataTable(requestPath);
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
