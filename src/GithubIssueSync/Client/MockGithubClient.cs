using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GithubIssueSync.Client {
    public class MockGithubClient : GithubClient {
        public override string GetRequest(string requestPath) {

            this.LastResponse = string.Empty;

            if (requestPath.Equals(@"/issues", StringComparison.CurrentCultureIgnoreCase))
                this.LastResponse = MockListUserIssues(this.UserName);
            if (requestPath.StartsWith(@"/issues", StringComparison.CurrentCultureIgnoreCase))
                this.LastResponse = MockListProjectIssues(this.UserName, requestPath.ToLower().Replace(@"/issues/", ""));
            else if (requestPath.StartsWith(@"/users/", StringComparison.CurrentCultureIgnoreCase))
                this.LastResponse = MockGetUser(requestPath.ToLower().Replace(@"/users/", ""));

            return this.LastResponse;
        }

        public static string MockListUserIssues(string userName) {
            string template = @"{
                    ""url"": ""https://api.github.com/repos/octocat/Hello-World/issues/1"",
                    ""html_url"": ""https://github.com/octocat/Hello-World/issues/1"",
                    ""number"": 1347,
                    ""state"": ""open"",
                    ""title"": ""Found a bug"",
                    ""body"": ""I'm having a problem with this."",
                    ""user"": {
                      ""login"": ""octocat"",
                      ""id"": 1,
                      ""avatar_url"": ""https://github.com/images/error/octocat_happy.gif"",
                      ""gravatar_id"": ""somehexcode"",
                      ""url"": ""https://api.github.com/users/octocat""
                    },
                    ""labels"": [
                      {
                        ""url"": ""https://api.github.com/repos/octocat/Hello-World/labels/bug"",
                        ""name"": ""bug"",
                        ""color"": ""f29513""
                      }
                    ],
                    ""assignee"": {
                      ""login"": ""{0}"",
                      ""id"": 1,
                      ""avatar_url"": ""https://github.com/images/error/{0}.gif"",
                      ""gravatar_id"": ""somehexcode"",
                      ""url"": ""https://api.github.com/users/{0}""
                    },
                    ""milestone"": {
                      ""url"": ""https://api.github.com/repos/octocat/Hello-World/milestones/1"",
                      ""number"": 1,
                      ""state"": ""open"",
                      ""title"": ""v1.0"",
                      ""description"": """",
                      ""creator"": {
                        ""login"": ""octocat"",
                        ""id"": 1,
                        ""avatar_url"": ""https://github.com/images/error/octocat_happy.gif"",
                        ""gravatar_id"": ""somehexcode"",
                        ""url"": ""https://api.github.com/users/octocat""
                      },
                      ""open_issues"": 4,
                      ""closed_issues"": 8,
                      ""created_at"": ""2011-04-10T20:09:31Z"",
                      ""due_on"": null
                    },
                    ""comments"": 0,
                    ""pull_request"": {
                      ""html_url"": ""https://github.com/octocat/Hello-World/issues/1"",
                      ""diff_url"": ""https://github.com/octocat/Hello-World/issues/1.diff"",
                      ""patch_url"": ""https://github.com/octocat/Hello-World/issues/1.patch""
                    },
                    ""closed_at"": null,
                    ""created_at"": ""2011-04-22T13:33:48Z"",
                    ""updated_at"": ""2011-04-22T13:33:48Z""
                  }";
            return MockList(template.Replace(@"{0}", userName), 5);
        }

        public static string MockListProjectIssues(string userName, string projectPath) {
            string template = @"{
                    ""url"": ""https://api.github.com/repos/{1}/issues/1"",
                    ""html_url"": ""https://github.com/{1}/issues/1"",
                    ""number"": 1347,
                    ""state"": ""open"",
                    ""title"": ""Found a bug"",
                    ""body"": ""I'm having a problem with this."",
                    ""user"": {
                      ""login"": ""octocat"",
                      ""id"": 1,
                      ""avatar_url"": ""https://github.com/images/error/octocat_happy.gif"",
                      ""gravatar_id"": ""somehexcode"",
                      ""url"": ""https://api.github.com/users/octocat""
                    },
                    ""labels"": [
                      {
                        ""url"": ""https://api.github.com/repos/{1}/labels/bug"",
                        ""name"": ""bug"",
                        ""color"": ""f29513""
                      }
                    ],
                    ""assignee"": {
                      ""login"": ""{0}"",
                      ""id"": 1,
                      ""avatar_url"": ""https://github.com/images/error/{0}.gif"",
                      ""gravatar_id"": ""somehexcode"",
                      ""url"": ""https://api.github.com/users/{0}""
                    },
                    ""milestone"": {
                      ""url"": ""https://api.github.com/repos/{1}/milestones/1"",
                      ""number"": 1,
                      ""state"": ""open"",
                      ""title"": ""v1.0"",
                      ""description"": """",
                      ""creator"": {
                        ""login"": ""octocat"",
                        ""id"": 1,
                        ""avatar_url"": ""https://github.com/images/error/octocat_happy.gif"",
                        ""gravatar_id"": ""somehexcode"",
                        ""url"": ""https://api.github.com/users/octocat""
                      },
                      ""open_issues"": 4,
                      ""closed_issues"": 8,
                      ""created_at"": ""2011-04-10T20:09:31Z"",
                      ""due_on"": null
                    },
                    ""comments"": 0,
                    ""pull_request"": {
                      ""html_url"": ""https://github.com/{1}/issues/1"",
                      ""diff_url"": ""https://github.com/{1}/issues/1.diff"",
                      ""patch_url"": ""https://github.com/{1}/issues/1.patch""
                    },
                    ""closed_at"": null,
                    ""created_at"": ""2011-04-22T13:33:48Z"",
                    ""updated_at"": ""2011-04-22T13:33:48Z""
                  }";
            string item = template.Replace(@"{0}", userName)
                                .Replace(@"{1}", projectPath);
            return MockList(item, 5);
        }

        public static string MockList(string template, int count) {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"[");
            for(int i = 0; i < count; i++) {
                if (i > 0) sb.AppendLine(@",");
                sb.Append(template);
            }
            sb.Append(@"]");
            return sb.ToString();
        }

        public static string MockGetUser(string userName) {
            const string template = @"{
              ""login"": ""{0}"",
              ""id"": 1,
              ""avatar_url"": ""https://github.com/images/error/{0}.gif"",
              ""gravatar_id"": ""somehexcode"",
              ""url"": ""https://api.github.com/users/{0}""
            }";
            return template.Replace(@"{0}", userName);
        }
    }
}
