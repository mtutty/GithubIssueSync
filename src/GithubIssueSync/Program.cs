using System;
using CommandLine;

namespace GithubIssueSync {
    public class Program : CommandLineProgram<Program, Program.Arguments> {
        public static void Main(string[] args) {
            Program me = new Program();
            me.RunProgram(args);
        }

        protected override void Validate(Arguments arguments) {
            if (arguments == null) throw new ArgumentNullException(@"Arguments", @"Program arguments cannot be null");

            RequireString(@"Repository Name", arguments.RepositoryName);
            if (string.IsNullOrEmpty(arguments.Token)) {
                if (string.IsNullOrEmpty(arguments.UserName) ||
                    string.IsNullOrEmpty(arguments.Password)) {
                        throw new ArgumentNullException(@"UserName/Password", @"User Name and Password is required if Token is not specified");
                }
            }
            if (string.IsNullOrEmpty(arguments.ImportFile) &&
                string.IsNullOrEmpty(arguments.ExportFile)) {
                    throw new ArgumentNullException(@"Import/Export File", @"Either Import File or Export File must be specified");
            }
        }

        protected override void Run(Arguments args) {
            Out(@"Github Issues Sync:  Using settings: {0}", args);
            Client.GithubClient client = new Client.GithubClient();
            client.Authenticate(args.UserName, args.Password);
            Out(client.User.ToString());
        }

        public class Arguments {

            [Option(@"r", @"repository", HelpText = @"The name of the Github repository")]
            public string RepositoryName = null;

            [Option(@"u", @"user", HelpText = @"The user name for authentication")]
            public string UserName = null;

            [Option(@"p", @"password", HelpText = @"The specified user's password")]
            public string Password = null;

            [Option(@"t", @"token", HelpText = @"The specified user's API token (instead of password)")]
            public string Token = null;

            [Option(@"i", @"import", HelpText = @"Path and file name to import from Github Issues.  The specified file will be overwritten if it exists.")]
            public string ImportFile = null;

            [Option(@"e", @"export", HelpText = @"Path and file name to export to Github.  Issues in the file will be created as new in Github.")]
            public string ExportFile = null;

            [Option(@"t", @"test", HelpText = @"If TRUE, then test connection to Github and exit.")]
            public bool TestConnection = false;

            [Option(@"w", @"wait", HelpText = @"If TRUE, then wait for <Enter> before ending the program.")]
            public bool WaitForExit = false;

            public override string ToString() {
                return string.Format(@"Repository = {0}, Test = {4}, Wait for Exit = {1}, {2} {3}",
                    this.RepositoryName, this.WaitForExit,
                    string.IsNullOrEmpty(ImportFile) ? @"Export from" : @"Import to",
                    string.IsNullOrEmpty(ImportFile) ? this.ExportFile : this.ImportFile,
                    this.TestConnection
                    );
            }
        }
    }
}
