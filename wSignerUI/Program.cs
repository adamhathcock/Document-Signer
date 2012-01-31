using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;

namespace wSignerUI
{
    class Program
    {
        private static readonly IDictionary<string, Assembly> Libs = new Dictionary<string, Assembly>();

        [STAThread]
        public static void Main(params string[] arguments)
        {
            AppDomain.CurrentDomain.AssemblyResolve += FindAssembly;
            if (arguments.Length > 0 && File.Exists(arguments[0]))
            {
                var file = arguments[0];
                DocumentSignerViewModel.SignFile(file);
            }
            else
            {
                var app = new Application();
                app.Run(new MainWindow());    
            }
        }

        static Assembly FindAssembly(object sender, ResolveEventArgs args)
        {
            Assembly result;
            var shortName = new AssemblyName(args.Name).Name;
            if (Libs.ContainsKey(shortName))
            {
                result = Libs[shortName];
            }
            else
            {
                using (var s = Assembly
                                .GetExecutingAssembly()
                                .GetManifestResourceStream("wSignerUI.Resources." + shortName + ".dll"))
                {
                    var data = new BinaryReader(s).ReadBytes((int)s.Length);
                    result = Assembly.Load(data);
                    Libs[shortName] = result;
                }
            }
            return result;
        }
    }
}