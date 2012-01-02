using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace GithubIssueSync.Client {
    public class GithubClient {
        public string UserName { get; set; }
        public string Token { get; set; }

        public string BaseUrl {get; set; }

        public GithubClient() : this(@"https://api.github.com") {}

        public GithubClient(string baseUrl) {
            this.BaseUrl = baseUrl;
        }

        public string GetRequestUrl(string relativePath) {
            return this.BaseUrl + relativePath;
        }

        public void Authenticate(string userName, string pwd) {
            HttpWebRequest req = HttpWebRequest.Create(GetRequestUrl(@"/users/")) as HttpWebRequest;
            req.Credentials = new NetworkCredential(userName, pwd);
            StreamReader sr = new StreamReader(req.GetResponse().GetResponseStream());
            string result = sr.ReadToEnd();

        }

        public void Authenticate(string token) {
        }
}
