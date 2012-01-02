using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace GithubIssueSync.Client {
    public class GithubClient {
        public string UserName { get; set; }
        private string Password = null;
        public string Token { get; set; }
        public string LastResponse { get; private set; }
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
            this.RequestToJSONResponse(@"/issues");
            this.User = GetUserInfo(this.UserName);
        }

        private NetworkCredential UserCredential {
            get {
                return new NetworkCredential(this.UserName, this.Password);
            }
        }

        private HttpWebRequest GetAuthenticatedRequest(string requestPath) {
            string url = GetRequestUrl(requestPath);
            HttpWebRequest req = HttpWebRequest.Create(url) as HttpWebRequest;

            string basicauth = Convert.ToBase64String(Encoding.UTF8.GetBytes(this.UserName + @":" + this.Password));
            req.Headers.Add(@"Authorization", @"Basic " + basicauth);
            return req;
        }

        public JObject RequestToJSONResponse(string requestPath) {
            using (HttpWebResponse resp = GetAuthenticatedRequest(requestPath).GetResponse() as HttpWebResponse) {
                if (resp == null) throw new NullReferenceException(@"The web request to " + requestPath + @" did not receive a response");
                if (resp.StatusCode == HttpStatusCode.OK) throw new Exception(@"The web request to " + requestPath + " return HTTP Status " + resp.StatusCode.ToString());
                using (StreamReader sr = new StreamReader(resp.GetResponseStream())) {
                    this.LastResponse = sr.ReadToEnd();
                    return JObject.Parse(this.LastResponse);
                }
            }
        }

        public JObject GetUserInfo(string userName) {
            return RequestToJSONResponse(@"/users");
        }

        public void Authenticate(string token) {
        }
    }
}
