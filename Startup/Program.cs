using System;
using System.Reflection;

namespace Startup
{
    internal static class Program
    {
        public const string applicationStartup = "Forms.Startup";
        public static bool assemblyExists;

        public const string dllFile = "Forms.dll";

        public static string[] assemblyFiles;

        public static string defaultAssemblyDir = AppDomain.CurrentDomain.BaseDirectory;
        public const string defaultAssemblyFolder = "\\DefaultLogin\\";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            string defaultPath = defaultAssemblyDir + defaultAssemblyFolder + dllFile;

            try
            {
                loadLoginAssembly(defaultPath, args);
            }
            catch (Exception)
            {
            }
        }

        private static void loadLoginAssembly(string loginAssemblyPath, string[] loginArgs)
        {
            AppDomain loginDomain = AppDomain.CreateDomain("login", null, new AppDomainSetup()
            {
                ShadowCopyFiles = "true"
            });

            Type type = typeof(LoginProxy);
            LoginProxy value = (LoginProxy)loginDomain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);

            value.StartApplication(loginAssemblyPath, loginArgs);
        }

        public class LoginProxy : MarshalByRefObject
        {
            public void StartApplication(string loginAssemblyPath, string[] loginArgs)
            {
                Assembly assembly = Assembly.LoadFrom(loginAssemblyPath);
                startApplication(assembly, loginArgs);
            }

            private void startApplication(Assembly startUpAssembly, string[] args)
            {
                Type startUp = startUpAssembly.GetType(applicationStartup);

                if (startUp != null)
                {
                    MethodInfo methodInfo = startUp.GetMethod("Main");
                    if (methodInfo != null)
                    {
                        invokeMainMethod(args, methodInfo, startUp);
                    }
                    else
                    {
                        if (assemblyExists)
                        {
                            methodInfo = getMain(startUp);
                            invokeMainMethod(args, methodInfo, startUp);
                        }
                    }
                }
                else
                {
                    if (assemblyExists)
                    {
                        startUp = Assembly.LoadFrom(defaultAssemblyDir + defaultAssemblyFolder + dllFile).GetType(applicationStartup);
                        MethodInfo methodInfo = getMain(startUp);
                        invokeMainMethod(args, methodInfo, startUp);
                    }
                }
            }

            private MethodInfo getMain(Type startUp)
            {
                return startUp.GetMethod("Main");
            }

            private void invokeMainMethod(string[] args, MethodInfo methodInfo, Type startUp)
            {
                ParameterInfo[] parameters = methodInfo.GetParameters();
                object classInstance = Activator.CreateInstance(startUp, null);

                Array.Resize(ref args, args.Length + 1);
                args[args.Length - 1] = "/path:" + Assembly.GetExecutingAssembly().Location; //add the path of the current app to fix shortcuts if we have to

                if (parameters.Length == 0)
                {
                    methodInfo.Invoke(classInstance, null);
                }
                else
                {
                    methodInfo.Invoke(classInstance, new object[] { args });
                }
            }
        }
    }
}
