using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;

namespace wSignerUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static readonly IDictionary<string, Assembly> Libs = new Dictionary<string, Assembly>();

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.AssemblyResolve += FindAssembly;
            base.OnStartup(e);
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