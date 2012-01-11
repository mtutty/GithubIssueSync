using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using System.Reflection;

namespace ConsoleApplication {
    public abstract class CommandLineProgram<P, T>
        where P : new()
        where T : new() {
        public void RunProgram(string[] args) {
            var arguments = new T();
            ICommandLineParser parser = new CommandLineParser();
            if (parser.ParseArguments(args, arguments)) {
                try {
                    Validate(arguments);
                } catch (Exception ex) {
                    Out(ex.Message);
                    Out(HelpText(arguments));
                    return;
                }

                try {
                    Run(arguments);
                } catch (ArgumentException argx) {
                    Out(argx.Message);
                } catch (Exception ex) {
                    Out(ex.Message);
                    Out(ex.StackTrace);
                }
                Exit(arguments);
            } else {
                Out(HelpText(arguments));
            }
        }

        protected virtual void Validate(T arguments) { }
        protected abstract void Run(T arguments);
        protected virtual void Exit(T arguments) { return; }

        #region Console I/O methods
        protected bool YesOrNo(string template, params object[] vars) {
            return YesOrNo(string.Format(template, vars));
        }

        protected bool YesOrNo(string prompt) {
            string ret = Prompt(prompt);
            while (true) {
                if (ret.Equals(@"n", StringComparison.CurrentCultureIgnoreCase)) return false;
                if (ret.Equals(@"y", StringComparison.CurrentCultureIgnoreCase)) return true;
                ret = Prompt(@"Please enter y or n: ");
            }
        }

        protected string Prompt(string template, params object[] vars) {
            return Prompt(string.Format(template, vars));
        }

        protected string Prompt(string query) {
            Console.Write(query);
            return Console.ReadLine();
        }

        protected void Out(string line) {
            Console.WriteLine(line);
        }

        protected void Out(string template, params object[] vals) {
            Console.WriteLine(template, vals);
        }

        protected void Separator() {
            Out(@"****************************************");
        }

        protected void WaitForExit() {
            Console.Write(@"Press <Enter> to end:");
            Console.ReadLine();
        }
        #endregion

        #region Validation methods
        protected void RequireString(string fieldName, string value) {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(fieldName, string.Format(@"{0} cannot be empty", fieldName));
        }
        #endregion

        #region Usage string builders
        protected string FlagListItem(OptionAttribute opt) {
            return FlagListItem(opt, string.Empty);
        }

        protected string FlagListItem(OptionAttribute opt, string extra) {
            return string.Format(@"[-{0} -{1}]{2} ",
                                    opt.ShortName, opt.LongName, extra
                                );
        }

        protected string UsageListItem(OptionAttribute opt, object defaultValue) {
            string param = string.Format(@"  {0} ({1}):", opt.LongName, opt.ShortName);
            return String.Format("  {0, -20} {1}  {2}",
                                param,
                                opt.HelpText,
                                defaultValue == null ? "" : string.Format(@"Default is {0}.", defaultValue)
                                );
        }

        protected string HelpText(T arguments) {
            string programName = Assembly.GetEntryAssembly().GetName().Name;

            StringBuilder flagList = new StringBuilder();
            StringBuilder usageList = new StringBuilder();

            foreach (FieldInfo fi in typeof(T).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)) {
                foreach (Attribute attr in fi.GetCustomAttributes(false)) {
                    BaseOptionAttribute opt = attr as BaseOptionAttribute;
                    if (attr is OptionAttribute) {
                        flagList.Append(FlagListItem(attr as OptionAttribute));
                        usageList.AppendLine(UsageListItem(attr as OptionAttribute, fi.GetValue(arguments)));
                    }
                }
            }

            return string.Format("Usage: {0} {1}\n\n{2}", programName, flagList.ToString(), usageList.ToString());
        }
        #endregion
    }
}
