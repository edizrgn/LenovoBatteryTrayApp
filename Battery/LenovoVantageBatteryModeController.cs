using System;
using System.IO;
using System.Linq;
using System.Reflection;
using LenovoBatteryTray.Utilities;

namespace LenovoBatteryTray.Battery
{
    public sealed class LenovoVantageBatteryModeController : IBatteryModeController
    {
        private const string ContractDllName = "BatteryManagementContract.dll";
        private const string AgentDllName = "IdeaBatteryAgent.dll";
        private const string AgentTypeName = "IdeaNotebookAddin.BatteryAgent";
        private const string RequestTypeName = "Lenovo.Modern.Contracts.BatteryManagement.BatteryMgmtRequest";
        private const string ModeTypeName = "Lenovo.Modern.Contracts.BatteryManagement.BatteryChargeModeType";

        private readonly LenovoVantagePathResolver pathResolver;
        private readonly object syncRoot = new object();

        private bool initialized;
        private bool assemblyResolveAttached;
        private string addinDirectory;
        private Type requestType;
        private Type modeType;
        private object agent;
        private MethodInfo getModeMethod;
        private MethodInfo setModeMethod;

        public LenovoVantageBatteryModeController(LenovoVantagePathResolver pathResolver)
        {
            if (pathResolver == null)
            {
                throw new ArgumentNullException("pathResolver");
            }

            this.pathResolver = pathResolver;
        }

        public LenovoBatteryMode GetMode()
        {
            lock (syncRoot)
            {
                try
                {
                    EnsureInitialized();
                    var response = getModeMethod.Invoke(agent, null);
                    var mode = ReadModeFromResponse(response);
                    AppLogger.Info("GetMode result: " + mode);
                    return mode;
                }
                catch (TargetInvocationException ex)
                {
                    throw CreateLenovoException("Lenovo pil modu okunurken hata oluştu.", ex);
                }
                catch (Exception ex)
                {
                    throw CreateLenovoException("Lenovo pil modu okunurken hata oluştu.", ex);
                }
            }
        }

        public void SetMode(LenovoBatteryMode mode)
        {
            if (mode == LenovoBatteryMode.Unknown)
            {
                throw new ArgumentException("Unknown modu ayarlanamaz.", "mode");
            }

            lock (syncRoot)
            {
                try
                {
                    EnsureInitialized();
                    AppLogger.Info("SetMode invoking Lenovo API: " + mode);

                    var request = Activator.CreateInstance(requestType);
                    var batteryChargeModeProperty = GetBatteryChargeModeProperty(requestType);

                    if (batteryChargeModeProperty == null)
                    {
                        throw new MissingMemberException(requestType.FullName, "BatteryChargeMode");
                    }

                    var selectedMode = Enum.Parse(modeType, mode.ToString());
                    batteryChargeModeProperty.SetValue(request, selectedMode, null);
                    setModeMethod.Invoke(agent, new[] { request });
                }
                catch (TargetInvocationException ex)
                {
                    throw CreateLenovoException("Lenovo pil modu değiştirilirken hata oluştu.", ex);
                }
                catch (Exception ex)
                {
                    throw CreateLenovoException("Lenovo pil modu değiştirilirken hata oluştu.", ex);
                }
            }
        }

        private void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            addinDirectory = pathResolver.Resolve();

            if (!assemblyResolveAttached)
            {
                AppDomain.CurrentDomain.AssemblyResolve += ResolveLenovoAssembly;
                assemblyResolveAttached = true;
            }

            Directory.SetCurrentDirectory(addinDirectory);

            var contractAssembly = Assembly.LoadFrom(Path.Combine(addinDirectory, ContractDllName));
            var agentAssembly = Assembly.LoadFrom(Path.Combine(addinDirectory, AgentDllName));

            var agentType = agentAssembly.GetType(AgentTypeName, true);
            requestType = contractAssembly.GetType(RequestTypeName, true);
            modeType = contractAssembly.GetType(ModeTypeName, true);

            var getInstanceMethod = agentType.GetMethod(
                "GetInstance",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            getModeMethod = agentType.GetMethod(
                "GetBatteryChargeMode",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            setModeMethod = agentType.GetMethod(
                "SetBatteryChargeMode",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (getInstanceMethod == null || getModeMethod == null || setModeMethod == null)
            {
                throw new InvalidOperationException("BatteryAgent üzerinde gerekli methodlardan biri bulunamadı.");
            }

            agent = getInstanceMethod.Invoke(null, null);

            if (agent == null)
            {
                throw new InvalidOperationException("BatteryAgent.GetInstance null döndürdü.");
            }

            initialized = true;
            AppLogger.Info("Lenovo battery controller initialized.");
        }

        private Assembly ResolveLenovoAssembly(object sender, ResolveEventArgs args)
        {
            if (string.IsNullOrEmpty(addinDirectory))
            {
                return null;
            }

            var assemblyName = new AssemblyName(args.Name).Name;
            var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(assembly => string.Equals(assembly.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase));

            if (loadedAssembly != null)
            {
                return loadedAssembly;
            }

            var candidate = Path.Combine(addinDirectory, assemblyName + ".dll");

            if (!File.Exists(candidate))
            {
                return null;
            }

            try
            {
                AppLogger.Info("Loading Lenovo dependency: " + candidate);
                return Assembly.LoadFrom(candidate);
            }
            catch (Exception ex)
            {
                AppLogger.Error("Lenovo dependency load failed: " + candidate, ex);
                return null;
            }
        }

        private LenovoBatteryMode ReadModeFromResponse(object response)
        {
            if (response == null)
            {
                return LenovoBatteryMode.Unknown;
            }

            object value;

            if (response.GetType().IsEnum)
            {
                value = response;
            }
            else
            {
                var batteryChargeModeProperty = GetBatteryChargeModeProperty(response.GetType());

                if (batteryChargeModeProperty == null)
                {
                    return LenovoBatteryMode.Unknown;
                }

                value = batteryChargeModeProperty.GetValue(response, null);
            }

            if (value == null)
            {
                return LenovoBatteryMode.Unknown;
            }

            LenovoBatteryMode mode;

            if (Enum.TryParse(value.ToString(), true, out mode))
            {
                return mode;
            }

            return LenovoBatteryMode.Unknown;
        }

        private static Exception CreateLenovoException(string message, Exception ex)
        {
            var invocationException = ex as TargetInvocationException;

            if (invocationException != null && invocationException.InnerException != null)
            {
                return new InvalidOperationException(message, invocationException.InnerException);
            }

            return new InvalidOperationException(message, ex);
        }

        private static PropertyInfo GetBatteryChargeModeProperty(Type type)
        {
            return type.GetProperty(
                "BatteryChargeMode",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}
