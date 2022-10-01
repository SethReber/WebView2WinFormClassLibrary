using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Forms
{
    public class Startup
    {
        [STAThread]
        public static void Main(string[] commandLineParameters)
        {
            Application.EnableVisualStyles();
            Application.Run(new Login());
        }

        /// <summary>
        /// Load all assemblies so they can be shadow cloned.
        /// It is important to do this because some assemblies are not loaded until referenced, so a clear cache
        /// could delete DLLs assemblies that have not yet been referenced.
        /// </summary>
        private static void loadAssembliesForShadowCloning()
        {
            // load the assemblies so they can be shadow cloned (so clear cache doesn't delete DLLs in use)
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                loadReferencedAssembly(assembly);
            }
        }

        private static void loadReferencedAssembly(Assembly assembly)
        {
            foreach (AssemblyName assemblyName in assembly.GetReferencedAssemblies())
            {
                bool assemblyAlreadyLoaded = AppDomain.CurrentDomain.GetAssemblies().Any(loadedAssemblies => loadedAssemblies.FullName == assemblyName.FullName);
                if (!assemblyAlreadyLoaded)
                {
                    loadReferencedAssembly(Assembly.Load(assemblyName));
                }
            }
        }
    }
}