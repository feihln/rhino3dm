using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Rhino.Runtime.InteropWrappers;
using System.Net.NetworkInformation;

#if RHINO_SDK
using Rhino.PlugIns;
using System.Management;
#endif

namespace Rhino.Runtime
{
  /// <summary>
  /// Marks a method as const. This attribute is purely informative to help the
  /// developer know that a method on a class does not alter the class itself.
  /// </summary>
  [AttributeUsage(AttributeTargets.Method)]
  class ConstOperationAttribute : Attribute
  {
    /// <summary>Basic constructor to mark a method as const</summary>
    public ConstOperationAttribute()
    {
    }
  }

#if RHINO_SDK
  /// <summary>
  /// Dictionary style class used for named callbacks from C++ -> .NET
  /// </summary>
  public class NamedParametersEventArgs : EventArgs
  {
    internal IntPtr m_pNamedParams;
    internal NamedParametersEventArgs(IntPtr ptr)
    {
      m_pNamedParams = ptr;
    }

    /// <summary>
    /// Try to get a string value for a given key name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetString(string name, out string value)
    {
      using (var str = new StringWrapper())
      {
        IntPtr pString = str.NonConstPointer;
        bool rc = UnsafeNativeMethods.CRhParameterDictionary_GetString(m_pNamedParams, name, pString);
        value = str.ToString();
        return rc;
      }
    }

    /// <summary>
    /// Set a string value for a given key name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void SetString(string name, string value)
    {
      UnsafeNativeMethods.CRhParameterDictionary_SetString(m_pNamedParams, name, value);
    }

    /// <summary>
    /// Try to get a bool value for a given key name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetBool(string name, out bool value)
    {
      value = false;
      return UnsafeNativeMethods.CRhParameterDictionary_GetBool(m_pNamedParams, name, ref value);
    }

    /// <summary>
    /// Set a bool value for a given key name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void SetBool(string name, bool value)
    {
      UnsafeNativeMethods.CRhParameterDictionary_SetBool(m_pNamedParams, name, value);
    }

    /// <summary>
    /// Try to get an int value for a given key name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetInt(string name, out int value)
    {
      value = 0;
      return UnsafeNativeMethods.CRhParameterDictionary_GetInt(m_pNamedParams, name, ref value);
    }

    /// <summary>
    /// Set an int value for a given key name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void SetInt(string name, int value)
    {
      UnsafeNativeMethods.CRhParameterDictionary_SetInt(m_pNamedParams, name, value);
    }

    /// <summary>
    /// Try to get a double value for a given key name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetDouble(string name, out double value)
    {
      value = 0;
      return UnsafeNativeMethods.CRhParameterDictionary_GetDouble(m_pNamedParams, name, ref value);
    }

    /// <summary>
    /// Set a double value for a given key name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void SetDouble(string name, double value)
    {
      UnsafeNativeMethods.CRhParameterDictionary_SetDouble(m_pNamedParams, name, value);
    }
  }

  /// <summary>
  /// Represents a customized environment that changes the appearance of Rhino.
  /// <para>Skin DLLs must contain a single class that derives from the Skin class.</para>
  /// </summary>
  public abstract class Skin
  {
    internal delegate void ShowSplashCallback(int mode, [MarshalAs(UnmanagedType.LPWStr)] string description);
    private static ShowSplashCallback m_ShowSplash;
    private static Skin m_theSingleSkin;

    /// <summary>
    /// Any time Rhino is running there is at most one skin being used (and
    /// possibly no skin).  If a RhinoCommon based Skin class is being used, use
    /// ActiveSkin to get at the instance of this Skin class. May return null
    /// if no Skin is being used or if the skin is not a RhinoCommon based skin.
    /// </summary>
    public static Skin ActiveSkin
    {
      get { return m_theSingleSkin; }
    }

    internal static void DeletePointer()
    {
      if( m_theSingleSkin!=null )
      {
        UnsafeNativeMethods.CRhinoSkin_Delete(m_theSingleSkin.m_pSkin);
        m_theSingleSkin.m_pSkin = IntPtr.Zero;
      }
    }

    internal void OnShowSplash(int mode, string description)
    {
      const int HIDESPLASH = 0;
      const int SHOWSPLASH = 1;
      const int SHOWHELP = 2;
      const int MAINFRAMECREATED = 1000;
      const int LICENSECHECKED = 2000;
      const int BUILTIN_COMMANDS_REGISTERED = 3000;
      const int BEGIN_LOAD_PLUGIN = 4000;
      const int END_LOAD_PLUGIN = 5000;
      const int END_LOAD_AT_START_PLUGINS = 6000;
      const int BEGIN_LOAD_PLUGINS_BASE = 100000;
      try
      {
        if (m_theSingleSkin != null)
        {
          switch (mode)
          {
            case HIDESPLASH:
              m_theSingleSkin.HideSplash();
              break;
            case SHOWSPLASH:
              m_theSingleSkin.ShowSplash();
              break;
            case SHOWHELP:
              m_theSingleSkin.ShowHelp();
              break;
            case MAINFRAMECREATED:
              m_theSingleSkin.OnMainFrameWindowCreated();
              break;
            case LICENSECHECKED:
              m_theSingleSkin.OnLicenseCheckCompleted();
              break;
            case BUILTIN_COMMANDS_REGISTERED:
              m_theSingleSkin.OnBuiltInCommandsRegistered();
              break;
            case BEGIN_LOAD_PLUGIN:
              m_theSingleSkin.OnBeginLoadPlugIn(description);
              break;
            case END_LOAD_PLUGIN:
              m_theSingleSkin.OnEndLoadPlugIn();
              break;
            case END_LOAD_AT_START_PLUGINS:
              m_theSingleSkin.OnEndLoadAtStartPlugIns();
              break;
          }
          if (mode >= BEGIN_LOAD_PLUGINS_BASE)
          {
            int count = (mode - BEGIN_LOAD_PLUGINS_BASE);
            m_theSingleSkin.OnBeginLoadAtStartPlugIns(count);
          }
        }
      }
      catch (Exception ex)
      {
        Runtime.HostUtils.DebugString("Exception caught during Show/Hide Splash");
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }
    }

    IntPtr m_pSkin;

    /// <summary>
    /// Initializes a new instance of the <see cref="Skin"/> class.
    /// </summary>
    protected Skin()
    {
      if (m_theSingleSkin != null) return;
      // set callback if it hasn't already been set
      if (null == m_ShowSplash)
      {
        m_ShowSplash = OnShowSplash;
      }

      System.Drawing.Bitmap icon = MainRhinoIcon;
      string name = ApplicationName;

      IntPtr hicon = IntPtr.Zero;
      if (icon != null)
        hicon = icon.GetHicon();

      m_pSkin = UnsafeNativeMethods.CRhinoSkin_New(m_ShowSplash, name, hicon);
      m_theSingleSkin = this;
    }
    /// <summary>Is called when the splash screen should be shown.</summary>
    protected virtual void ShowSplash() { }

    /// <summary>
    /// Called when the "help" splash screen should be shown. Default
    /// implementation just calls ShowSplash()
    /// </summary>
    protected virtual void ShowHelp() { ShowSplash(); }

    /// <summary>Is called when the splash screen should be hidden.</summary>
    protected virtual void HideSplash() { }

    /// <summary>Is called when the main frame window is created.</summary>
    protected virtual void OnMainFrameWindowCreated() { }

    /// <summary>Is called when the license check is completed.</summary>
    protected virtual void OnLicenseCheckCompleted() { }

    /// <summary>Is called when built-in commands are registered.</summary>
    protected virtual void OnBuiltInCommandsRegistered() { }

    /// <summary>Is called when the first plug-in that loads at start-up is going to be loaded.</summary>
    /// <param name="expectedCount">The complete amount of plug-ins.</param>
    protected virtual void OnBeginLoadAtStartPlugIns(int expectedCount) { }

    /// <summary>Is called when a specific plug-in is going to be loaded.</summary>
    /// <param name="description">The plug-in description.</param>
    protected virtual void OnBeginLoadPlugIn(string description) { }

    /// <summary>Is called after each plug-in has been loaded.</summary>
    protected virtual void OnEndLoadPlugIn() { }

    /// <summary>Is called after all of the load at start plug-ins have been loaded.</summary>
    protected virtual void OnEndLoadAtStartPlugIns() { }

    /// <summary>If you want to provide a custom icon for your skin.</summary>
    protected virtual System.Drawing.Bitmap MainRhinoIcon
    {
      get { return null; }
    }

    /// <summary>If you want to provide a custom name for your skin.</summary>
    protected virtual string ApplicationName
    {
      get { return string.Empty; }
    }

    PersistentSettingsManager m_SettingsManager;

    /// <summary>
    /// Gets access to the skin persistent settings.
    /// </summary>
    public PersistentSettings Settings
    {
      get
      {
        if (m_SettingsManager == null)
          m_SettingsManager = PersistentSettingsManager.Create(this);
        return m_SettingsManager.PluginSettings;
      }
    }

    static bool m_settings_written;
    internal static void WriteSettings(bool shuttingDown)
    {
      if (!m_settings_written)
      {
        if (m_theSingleSkin != null && m_theSingleSkin.m_SettingsManager != null)
        {
          if (m_theSingleSkin.m_SettingsManager.m_plugin_id == Guid.Empty)
            m_theSingleSkin.m_SettingsManager.WriteSettings(shuttingDown);
        }
      }
      m_settings_written = true;
    }
  }

  /// <summary>
  /// Represents scripting compiled code.
  /// </summary>
  public abstract class PythonCompiledCode
  {
    /// <summary>
    /// Executes the script in a specific scope.
    /// </summary>
    /// <param name="scope">The scope where the script should be executed.</param>
    public abstract void Execute(PythonScript scope);
  }

  /// <summary>
  /// Represents a Python script.
  /// </summary>
  public abstract class PythonScript
  {
    /// <summary>
    /// Constructs a new Python script context.
    /// </summary>
    /// <returns>A new Python script, or null if none could be created. Rhino 4 always returns null.</returns>
    public static PythonScript Create()
    {
      Guid ip_id = new Guid("814d908a-e25c-493d-97e9-ee3861957f49");
      object obj = Rhino.RhinoApp.GetPlugInObject(ip_id);
      if (null == obj)
        return null;
      PythonScript pyscript = obj as PythonScript;
      return pyscript;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PythonScript"/> class.
    /// </summary>
    protected PythonScript()
    {
      ScriptContextDoc = null;
      Output = RhinoApp.Write;
    }

    /// <summary>
    /// Compiles a class in a quick-to-execute proxy.
    /// </summary>
    /// <param name="script">A string text.</param>
    /// <returns>A Python compiled code instance.</returns>
    public abstract PythonCompiledCode Compile(string script);

    /// <summary>
    /// Determines if the main scripting context has a variable with a name.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <returns>true if the variable is present.</returns>
    public abstract bool ContainsVariable(string name);

    /// <summary>
    /// Retrieves all variable names in the script.
    /// </summary>
    /// <returns>An enumerable set with all names of the variables.</returns>
    public abstract System.Collections.Generic.IEnumerable<string> GetVariableNames();

    /// <summary>
    /// Gets the object associated with a variable name in the main scripting context.
    /// </summary>
    /// <param name="name">A variable name.</param>
    /// <returns>The variable object.</returns>
    public abstract object GetVariable(string name);

    /// <summary>
    /// Sets a variable with a name and an object. Object can be null (Nothing in Visual Basic).
    /// </summary>
    /// <param name="name">A valid variable name in Python.</param>
    /// <param name="value">A valid value for that variable name.</param>
    public abstract void SetVariable(string name, object value);

    /// <summary>
    /// Sets a variable for runtime introspection.
    /// </summary>
    /// <param name="name">A variable name.</param>
    /// <param name="value">A variable value.</param>
    public virtual void SetIntellisenseVariable(string name, object value) { }

    /// <summary>
    /// Removes a defined variable from the main scripting context.
    /// </summary>
    /// <param name="name">The variable name.</param>
    public abstract void RemoveVariable(string name);

    /// <summary>
    /// Evaluates statements and an expression in the main scripting context.
    /// </summary>
    /// <param name="statements">One or several statements.</param>
    /// <param name="expression">An expression.</param>
    /// <returns>The expression result.</returns>
    public abstract object EvaluateExpression(string statements, string expression);

    /// <summary>
    /// Executes a Python file.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>true if the file executed. This method can throw scripting-runtime based exceptions.</returns>
    public abstract bool ExecuteFile(string path);

    /// <summary>
    /// Executes a Python string.
    /// </summary>
    /// <param name="script">A Python text.</param>
    /// <returns>true if the file executed. This method can throw scripting-runtime based exceptions.</returns>
    public abstract bool ExecuteScript(string script);

    /// <summary>
    /// Retrieves a meaningful representation of the call stack.
    /// </summary>
    /// <param name="ex">An exception that was thrown by some of the methods in this class.</param>
    /// <returns>A string that represents the Python exception.</returns>
    public abstract string GetStackTraceFromException(Exception ex);

    /// <summary>
    /// Gets or sets the Python script "print()" target.
    /// <para>By default string output goes to the Rhino.RhinoApp.Write function.
    /// Set Output if you want to redirect the output from python to a different function
    /// while this script executes.</para>
    /// </summary>
    public Action<string> Output { get; set; }

    /// <summary>
    /// object set to variable held in scriptcontext.doc.
    /// </summary>
    public object ScriptContextDoc { get; set; }

    /// <summary>
    /// Command associated with this script. Used for localiation
    /// </summary>
    public Commands.Command ScriptContextCommand { get; set; }

    /// <summary>
    /// Gets or sets a context unique identified.
    /// </summary>
    public int ContextId
    {
      get { return m_context_id; }
      set { m_context_id = value; }
    }
    int m_context_id = 1;

    /// <summary>
    /// Creates a control where the user is able to type Python code.
    /// </summary>
    /// <param name="script">A starting script.</param>
    /// <param name="helpcallback">A method that is called when help is shown for a function, a class or a method.</param>
    /// <returns>A Windows Forms control.</returns>
    public abstract object CreateTextEditorControl(string script, Action<string> helpcallback);

    /// <summary>
    /// Setups the script context. Use a RhinoDoc instance unless unsure.
    /// </summary>
    /// <param name="doc">Document.</param>
    public virtual void SetupScriptContext(object doc)
    {
    }
  }

  /// <summary>
  /// Defines risky actions that need to be reported in crash exceptions
  /// </summary>
  public class RiskyAction : IDisposable
  {
    IntPtr m_ptr_risky_action;
    /// <summary> Always create this in a using block </summary>
    /// <param name="description"></param>
    /// <param name="file"></param>
    /// <param name="member"></param>
    /// <param name="line"></param>
    public RiskyAction(string description, [CallerFilePath] string file="", [CallerMemberName] string member="", [CallerLineNumber] int line=0)
    {
      m_ptr_risky_action = UnsafeNativeMethods.CRhRiskyActionSpy_New(description, member, file, line);
    }

    /// <summary>
    /// IDisposable implementation
    /// </summary>
    public void Dispose()
    {
      UnsafeNativeMethods.CRhRiskyActionSpy_Delete(m_ptr_risky_action);
      m_ptr_risky_action = IntPtr.Zero;
    }
  }
#endif

  /// <summary>
  /// Get platform specific services that are used internally for
  /// general cross platform funtions in RhinoCommon. This includes
  /// services like localization and GUI components that have concrete
  /// implementations in the RhinoWindows or RhinoMac assemblies
  /// </summary>
  public interface IPlatformServiceLocator
  {
    /// <summary>Used to get service of a specific type</summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T GetService<T>() where T : class;
  }

  class DoNothingLocator : IPlatformServiceLocator
  {
    public T GetService<T>() where T : class
    {
      return null;
    }
  }

  /// <summary>
  /// Interface for querying information about the operating system Rhino is running on.
  /// </summary>
  interface IOperatingSystemInformation
  {
    /// <summary>
    /// Returns Operating System Installation Type: "Client" | "Server" | "Unknown"
    /// </summary>
    string InstallationType { get; }
    /// <summary>
    /// Returns Operating System Edition: "Professional" | "ServerDatacenter" | ... | "Unknown"
    /// </summary>
    string Edition { get; }
    /// <summary>
    /// Returns Operating System Product Name "Windows 10 Pro" | "Windows Server 2008 R2 Datacenter" | ... | "Unknown"
    /// </summary>
    string ProductName { get; }
    /// <summary>
    /// Returns Operating System Version "6.1" | "6.3" | ... | "Unknown"
    /// </summary>
    string Version { get; }
    /// <summary>
    /// Returns Operating System Build Number "11763" | "7601" | ... | "Unknown"
    /// </summary>
    string BuildNumber { get; }

  }

  class DoNothingOperatingSystemInformationService : IOperatingSystemInformation
  {
    public string InstallationType => throw new NotImplementedException();

    public string Edition => throw new NotImplementedException();

    public string ProductName => throw new NotImplementedException();

    public string Version => throw new NotImplementedException();

    public string BuildNumber => throw new NotImplementedException();
  }

  /// <summary>
  /// Contains static methods to deal with teh runtime environment.
  /// </summary>
  public static class HostUtils
  {
#if RHINO_SDK
    /// <summary>
    /// Returns information about the current process. If Rhino is the top level process,
    /// processName is "Rhino". Otherwise, processName is the name, without extension, of the main
    /// module that is executing. For example, "compute.backend" or "Revit".
    /// 
    /// processVersion is the System.Version of the running process. It is the FileVersion
    /// of the executable.
    /// </summary>
    public static void GetCurrentProcessInfo(out string processName, out Version processVersion)
    {
      if (RunningOnOSX)
      {
#if RHINO_SDK
        processVersion = RhinoApp.Version;
#else
        processVersion = new Version(RhinoBuildConstants.VERSION_STRING);
#endif
        processName = "Rhino";
      }
      else
      {
        var fvi = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileVersionInfo;
        processVersion = new Version(fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart);

        var moduleName = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName;
        if (moduleName == "Rhinoceros")  // Mac Rhino returns Rhinoceros
          moduleName = "Rhino";
        processName = System.IO.Path.GetFileNameWithoutExtension(moduleName);
      }
    }
#endif

    /// <summary>
    /// Returns Operating System Edition: "Professional" | "ServerDatacenter" | ... | "Unknown"
    /// </summary>
    public static string OperatingSystemEdition
    {
      get
      {
        var psl = GetPlatformService<IOperatingSystemInformation>();
        return psl.Edition;
      }
    }

    /// <summary>
    /// Returns Operating System Installation Type: "Client" | "Server" | "Unknown"
    /// </summary>
    public static string OperatingSystemInstallationType
    {
      get
      {
        var psl = GetPlatformService<IOperatingSystemInformation>();
        return psl.InstallationType;
      }
    }
    /// <summary>
    /// Returns Operating System Edition: "Professional" | "ServerDatacenter" | ... | "Unknown"
    /// </summary>
    public static string OperatingSystemProductName
    {
      get
      {
        var psl = GetPlatformService<IOperatingSystemInformation>();
        return psl.ProductName;
      }
    }
    /// <summary>
    /// Returns Operating System Version "6.1" | "6.3" | ... | "Unknown"
    /// </summary>
    public static string OperatingSystemVersion
    {
      get
      {
        var psl = GetPlatformService<IOperatingSystemInformation>();
        return psl.Version;
      }
    }
    /// <summary>
    /// Returns Operating System Build Number "11763" | "7601" | ... | "Unknown"
    /// </summary>
    public static string OperatingSystemBuildNumber
    {
      get
      {
        var psl = GetPlatformService<IOperatingSystemInformation>();
        return psl.BuildNumber;
      }
    }



    static Dictionary<string, IPlatformServiceLocator> g_platform_locator = new Dictionary<string, IPlatformServiceLocator>();

    /// <summary>For internal use only. Loads an assembly for dependency injection via IPlatformServiceLocator.</summary>
    /// <param name="assemblyPath">The relative path of the assembly, relative to the position of RhinoCommon.dll</param>
    /// <param name="typeFullName">The full name of the type that is IPlatformServiceLocator. This is optional.</param>
    /// <typeparam name="T">The type of the service to be instantiated.</typeparam>
    /// <returns>An instance, or null.</returns>
    public static T GetPlatformService<T>(string assemblyPath=null, string typeFullName=null) where T : class
    {
      if (string.IsNullOrEmpty(assemblyPath))
      {
        assemblyPath = RunningOnWindows ? "RhinoWindows.dll" : "RhinoMac.dll";
      }

      if (!g_platform_locator.ContainsKey(assemblyPath))
      {
        g_platform_locator[assemblyPath] = new DoNothingLocator();
        if (RunningInRhino)
        {
          var service_type = typeof(IPlatformServiceLocator);
          var path_to_rhinocommon = service_type.Assembly.Location;
          var path = System.IO.Path.GetDirectoryName(path_to_rhinocommon);
          path = System.IO.Path.Combine(path, assemblyPath);
          var platform_assembly = System.Reflection.Assembly.LoadFrom(path);

          if (typeFullName == null)
          {
            Type[] types = platform_assembly.GetExportedTypes();
            foreach (var t in types)
            {
              if (!t.IsAbstract && service_type.IsAssignableFrom(t))
              {
                g_platform_locator[assemblyPath] = Activator.CreateInstance(t) as IPlatformServiceLocator;
                break;
              }
            }
          }
          else
          {
            object instantiated = platform_assembly.CreateInstance(typeFullName);
            g_platform_locator[assemblyPath] = instantiated as IPlatformServiceLocator;
          }
        }
      }
      return g_platform_locator[assemblyPath].GetService<T>();
    }

#if RHINO_SDK
    /// <summary>
    /// Inspects a dll to see if it is compiled as native code or as a .NET assembly
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsManagedDll(string path)
    {
      return UnsafeNativeMethods.RHC_IsManagedDll(path);
    }

    /// <summary>
    /// Clear FPU exception and busy flags (Intel assembly fnclex)
    /// </summary>
    public static void ClearFpuExceptionStatus()
    {
      UnsafeNativeMethods.RHC_ON_FPU_ClearExceptionStatus();
    }

    static Dictionary<string, EventHandler<NamedParametersEventArgs>> _namedCallbacks = new Dictionary<string, EventHandler<NamedParametersEventArgs>>();
    static GCHandle _namedCallbackHandle;
    internal delegate int NamedCallback(IntPtr name, IntPtr ptrNamedParams);
    static readonly NamedCallback g_named_callback = ExecuteNamedCallback;

    /// <summary>
    /// Register a named callback from C++ -> .NET
    /// </summary>
    /// <param name="name"></param>
    /// <param name="callback"></param>
    public static void RegisterNamedCallback(string name, EventHandler<NamedParametersEventArgs> callback)
    {
      _namedCallbacks[name] = callback;
      if( !_namedCallbackHandle.IsAllocated )
      {
        _namedCallbackHandle = GCHandle.Alloc(g_named_callback);
        IntPtr func = Marshal.GetFunctionPointerForDelegate(g_named_callback);
        UnsafeNativeMethods.RHC_RhRegisterNamedCallbackProc(func);
      }
    }

    static int ExecuteNamedCallback(IntPtr name, IntPtr ptrNamedParams)
    {
      try
      {
        string _name = StringWrapper.GetStringFromPointer(name);
        EventHandler<NamedParametersEventArgs> callback = null;
        if( _namedCallbacks.TryGetValue(_name, out callback) && callback != null)
        {
          NamedParametersEventArgs e = new NamedParametersEventArgs(ptrNamedParams);
          callback(null, e);
          e.m_pNamedParams = IntPtr.Zero;
          return 1;
        }
      }
      catch(Exception ex)
      {
        ExceptionReport(ex);
      }
      return 0;
    }


#endif

    /// <summary>
    /// Returns list of directory names where additional assemblies (plug-ins, DLLs, Grasshopper components)
    /// may be located
    /// </summary>
    /// <returns></returns>
    public static string[] GetAssemblySearchPaths()
    {
#if RHINO_SDK
      // inlude directory where RhinoCommon is located
      List<string> directories = new List<string>();
      string rhino_common_location = typeof(HostUtils).Assembly.Location;
      directories.Add(System.IO.Path.GetDirectoryName(rhino_common_location));
      directories.AddRange(PlugIn.GetInstalledPlugInFolders());
      // include all auto-install directories (that aren't already included)
      // grasshopper will prune the folders that it doesn't care about
      foreach (var dir in GetActivePlugInVersionFolders(true))
      {
        if (!directories.Contains(dir.FullName))
        {
          directories.Add(dir.ToString());
        }
      }
      return directories.ToArray();
#else
      return new string[0];
#endif
    }

    /// <summary>
    /// DO NOT USE UNLESS YOU ARE CERTAIN ABOUT THE IMPLICATIONS.
    /// <para>This is an expert user function which should not be needed in most
    /// cases. This function is similar to a const_cast in C++ to allow an object
    /// to be made temporarily modifiable without causing RhinoCommon to convert
    /// the class from const to non-const by creating a duplicate.</para>
    /// 
    /// <para>You must call this function with a true parameter, make your
    /// modifications, and then restore the const flag by calling this function
    /// again with a false parameter. If you have any questions, please
    /// contact McNeel developer support before using!</para>
    /// </summary>
    /// <param name="geometry">Some geometry.</param>
    /// <param name="makeNonConst">A boolean value.</param>
    public static void InPlaceConstCast(Rhino.Geometry.GeometryBase geometry, bool makeNonConst)
    {
      if (makeNonConst)
      {
        geometry.ApplyConstCast();
      }
      else
      {
        geometry.RemoveConstCast();
      }
    }

    /// <summary>
    /// Tests if this process is currently executing on the Windows platform.
    /// </summary>
    public static bool RunningOnWindows
    {
      get { return !RunningOnOSX; }
    }

    /// <summary>
    /// Tests if this process is currently executing on the Mac OSX platform.
    /// </summary>
    public static bool RunningOnOSX
    {
      get
      {
        System.PlatformID pid = System.Environment.OSVersion.Platform;
        // unfortunately Mono reports Unix when running on Mac
        return (System.PlatformID.MacOSX == pid || System.PlatformID.Unix == pid);
      }
    }

#if RHINO_SDK
    private static string m_device_name;
    /// <summary>
    /// Name of the computer running Rhino. If the computer is part of a 
    /// Windows Domain, the computer name has "@[DOMAIN]" appended.
    /// </summary>
    public static string DeviceName
    {
      get
      {
        if (string.IsNullOrEmpty(m_device_name))
        {
          var machineName = Environment.MachineName;
          var userDomain = Environment.UserDomainName;

          if (string.Equals(machineName, userDomain, StringComparison.InvariantCultureIgnoreCase))
          {
            m_device_name = machineName;
          }
          else
          {
            m_device_name = string.Format("{0}@{1}", machineName, userDomain);
          }
        }
        return m_device_name;
      }
    }

    /// <summary>
    /// Gets the serial number of the computer running Rhino.
    /// </summary>
    public static string ComputerSerialNumber
    {
      get
      {
        using (var string_holder = new Rhino.Runtime.InteropWrappers.StringWrapper())
        {
          IntPtr ptr_string = string_holder.NonConstPointer;
          UnsafeNativeMethods.RHC_GetComputerSerialNumber(ptr_string);
          return string_holder.ToString();
        }
      }
    }

#if RHINO_SDK
    /// <summary>
    /// Get the current operating system language.
    /// </summary>
    /// <returns>A Windows LCID (on Windows and macOS).  On Windows, this will be 
    /// LCID value regardless of those languages that Rhino supports.  On macOS, this only
    /// returns LCID values for languages that Rhino does support.</returns>
    [CLSCompliant(false)]
    public static uint CurrentOSLanguage => UnsafeNativeMethods.RHC_RhCurrentOSLanguage();
#endif

    private static Guid m_device_id = Guid.Empty;
    /// <summary>
    /// The DeviceId is a unique, stable ID that anonymously identifies the device
    /// that Rhino is running on. It is computed based on hardware information that
    /// should not change when the OS is upgraded, or if commonly modified hardware
    /// are added or removed from the computer. The machine-specific information is
    /// hashed using a cryptographic hash to make it anonymous.
    /// </summary>
    public static Guid DeviceId
    {
      get
      {
        bool SerialNumberIsHardwareBased = true;
        if (m_device_id == Guid.Empty)
        {
          // Base the device ID solely on the HardwareSerialNumber, if one
          // exists. Otherwise, base it on the first Ethernet MacAddress.
          // The goal here is to generate a reasonably uniqe and stable
          // ID, so very little hardware information is used.
          string data = HardwareSerialNumber;
          if (string.IsNullOrWhiteSpace(data))
          {
            SerialNumberIsHardwareBased = false;
            data = MacAddress.ToString();
          }

          var bytes = System.Text.Encoding.UTF8.GetBytes(data);

          // Compute 16-bit hash, based on SHA256, because
          // SHA256 is FIPS compliant. MD5 and SHA1 are not.
          using (var hasher = new System.Security.Cryptography.SHA256CryptoServiceProvider())
          {
            var hash = hasher.ComputeHash(bytes);
            var hash16 = hash.Take(16).ToArray();

            // Set last digit in resulting GUID to 0
            // to specify that this Device.ID was generated
            // from a valid HardwareSerialNumber.
            hash16[15] = (byte)(hash16[15] >> 4 << 4);
            if (!SerialNumberIsHardwareBased)
              hash16[15] |= 0x1;

            // Set GUID 
            m_device_id = new Guid(hash16);
          }
        }
        return m_device_id;
      }
    }

    private static string m_serial_number = "";
    private static string HardwareSerialNumber
    {
      get
      {
        if (!string.IsNullOrWhiteSpace(m_serial_number))
          return m_serial_number;

        if (RunningOnOSX)
        {
          m_serial_number = ComputerSerialNumber;
        }
        else
        {
          m_serial_number = WindowsBiosSerialNumber;
        }


        // Return serial number
        return m_serial_number;
      }
    }

    private static string WindowsBiosSerialNumber
    {
      get
      {
        try
        {
          var mc = new ManagementClass("Win32_ComputerSystemProduct");
          var coll = mc.GetInstances();
          foreach (var obj in coll)
          {
            var uuid = obj.Properties["UUID"].Value.ToString().ToLowerInvariant();
            var fullUuid = new Guid("ffffffffffffffffffffffffffffffff");
            if (uuid == fullUuid.ToString() || uuid == Guid.Empty.ToString())
            {
              continue;
            }
            return uuid;
          }
        }
        catch
        {
          // Don't crash just because there's no such management class.
        }
        return null;
      }
    }

    private static PhysicalAddress MacAddress
    {
      get
      {
        PhysicalAddress macAddress = null;

        NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
        Array.Sort(nics, (NetworkInterface x, NetworkInterface y) => { return x.Id.CompareTo(y.Id); });

        if (macAddress == null)
        {
          // First try: look for Ethernet NICs
          foreach (NetworkInterface nic in nics)
          {
            if (nic.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
              continue;

            if (nic.GetPhysicalAddress() != null && !nic.GetPhysicalAddress().Equals(PhysicalAddress.None))
            {
              macAddress = nic.GetPhysicalAddress();
              break;
            }
          }
        }

        if (macAddress == null)
        {
          // Second try: look for anything we're sure we don't want
          foreach (NetworkInterface nic in nics)
          {
            switch (nic.NetworkInterfaceType)
            {
              case NetworkInterfaceType.Tunnel:
              case NetworkInterfaceType.Unknown:
              case NetworkInterfaceType.Loopback:
                continue;
            }

            if (nic.GetPhysicalAddress() != null && !nic.GetPhysicalAddress().Equals(PhysicalAddress.None))
            {
              macAddress = nic.GetPhysicalAddress();
              break;
            }
          }
        }

        return macAddress;
      }
    }


#endif
    /// <summary>
    /// Tests if this process is currently executing under the Mono runtime.
    /// </summary>
    public static bool RunningInMono
    {
      get { return Type.GetType("Mono.Runtime") != null; }
    }

    static int m_running_in_rhino_state; //0=unknown, 1=false, 2=true
    /// <summary>
    /// Tests if RhinoCommon is currently executing inside of the Rhino.exe process.
    /// There are other cases where RhinoCommon could be running; specifically inside
    /// of Visual Studio when something like a windows form is being worked on in the
    /// resource editor or running stand-alone when compiled to be used as a version
    /// of OpenNURBS.
    /// </summary>
    public static bool RunningInRhino
    {
      get
      {
        if (m_running_in_rhino_state == 0)
        {
#if RHINO_SDK
          m_running_in_rhino_state = 1;
          try
          {
            if (0 != Rhino.RhinoApp.SdkVersion )
              m_running_in_rhino_state = 2;
          }
          catch (Exception)
          {
            m_running_in_rhino_state = 1;
          }
#else
          m_running_in_rhino_state = 1;
#endif
        }
        return (m_running_in_rhino_state == 2);
      }
    }

#if RHINO_SDK
    // 0== unknown
    // 1== loaded
    //-1== not loaded
    static int m_rdk_loadtest;
    /// <summary>
    /// Determines if the RDK is loaded.
    /// </summary>
    /// <param name="throwOnFalse">if the RDK is not loaded, then throws a
    /// <see cref="RdkNotLoadedException"/>.</param>
    /// <param name="usePreviousResult">if true, then the last result can be used instaed of
    /// performing a full check.</param>
    /// <returns>true if the RDK is loaded; false if the RDK is not loaded. Note that the
    /// <see cref="RdkNotLoadedException"/> will hinder the retrieval of any return value.</returns>
    public static bool CheckForRdk(bool throwOnFalse, bool usePreviousResult)
    {
      const int UNKNOWN = 0;
      const int LOADED = 1;
      const int NOT_LOADED = -1;

      if (UNKNOWN == m_rdk_loadtest || !usePreviousResult)
      {
        try
        {
          UnsafeNativeMethods.Rdk_LoadTest();
          m_rdk_loadtest = LOADED;
        }
        catch (Exception)
        {
          m_rdk_loadtest = NOT_LOADED;
        }
      }

      if (LOADED == m_rdk_loadtest)
        return true;

      if (throwOnFalse)
        throw new RdkNotLoadedException();
      return false;
    }

    /// <summary>
    /// Call this method to convert a relative path to an absolute path
    /// relative to the specified path.
    /// </summary>
    /// <param name="relativePath">
    /// Relative path to convert to an absolute path
    /// </param>
    /// <param name="bRelativePathisFileName">
    /// If true then lpsFrom is treated as a file name otherwise it is treated
    /// as a directory name
    /// </param>
    /// <param name="relativeTo">
    /// File or folder the path is relative to
    /// </param>
    /// <param name="bRelativeToIsFileName">
    /// If true then lpsFrom is treated as a file name otherwise it is treated
    /// as a directory name
    /// </param>
    /// <param name="pathOut">
    /// Reference to string which will receive the computed absolute path
    /// </param>
    /// <returns>
    /// Returns true if parameters are valid and lpsRelativePath is indeed
    /// relative to lpsRelativeTo otherwise returns false
    /// </returns>
    public static bool GetAbsolutePath(string relativePath, bool bRelativePathisFileName, string relativeTo,bool bRelativeToIsFileName, out string pathOut)
    {
      using (var string_holder = new StringHolder())
      {
        var string_pointer = string_holder.NonConstPointer();
        var success = UnsafeNativeMethods.CRhinoFileUtilities_PathAbsolutFromRelativeTo(relativePath, bRelativePathisFileName, relativeTo, bRelativePathisFileName, string_pointer);
        pathOut = success ? string_holder.ToString() : string.Empty;
        return success;
      }
    }
    /// <summary>
    /// Check to see if the file extension is a valid Rhino file extension.
    /// </summary>
    /// <param name="fileExtension"></param>
    /// <returns>
    /// Returns true if fileExtension is ".3dm", "3dm", ".3dx" or "3dx",
    /// ignoring case.
    /// </returns>
    public static bool IsRhinoFileExtension(string fileExtension)
    {
      return UnsafeNativeMethods.CRhinoFileUtilities_Is3dmFileExtension(System.IO.Path.GetExtension(fileExtension));
    }
    /// <summary>
    /// Strip file extension from file name and check to see if it is a valid
    /// Rhino file extension.
    /// </summary>
    /// <param name="fileName">
    /// File name to check.
    /// </param>
    /// <returns>
    /// Returns true if the file name has an extension like 3dm.
    /// </returns>
    public static bool FileNameEndsWithRhinoExtension(string fileName)
    {
      return !string.IsNullOrEmpty(fileName) && IsRhinoFileExtension(System.IO.Path.GetExtension(fileName));
    }
    /// <summary>
    /// Check to see if the file extension is a valid Rhino file extension.
    /// </summary>
    /// <param name="fileExtension"></param>
    /// <returns>
    /// Return true if fileExtension is ".3dmbak", "3dmbak", ".3dm.bak", "3dm.bak",
    /// ".3dx.bak" or "3dx.bak", ignoring case.
    /// </returns>
    public static bool IsRhinoBackupFileExtension(string fileExtension)
    {
      return UnsafeNativeMethods.CRhinoFileUtilities_Is3dmBackupFileExtension(System.IO.Path.GetExtension(fileExtension));
    }
    /// <summary>
    /// Strip file extension from file name and check to see if it is a valid
    /// Rhino backup file extension.
    /// </summary>
    /// <param name="fileName">
    /// File name to check.
    /// </param>
    /// <returns>
    /// Returns true if the file name has an extension like 3dmbak.
    /// </returns>
    public static bool FileNameEndsWithRhinoBackupExtension(string fileName)
    {
      return !string.IsNullOrEmpty(fileName) && IsRhinoBackupFileExtension(System.IO.Path.GetExtension(fileName));
    }
#endif

    static bool m_bSendDebugToRhino; // = false; initialized by runtime
    /// <summary>
    /// Prints a debug message to the Rhino Command Line. 
    /// The message will only appear if the SendDebugToCommandLine property is set to true.
    /// </summary>
    /// <param name="msg">Message to print.</param>
    public static void DebugString(string msg)
    {
#if RHINO_SDK
      if (m_bSendDebugToRhino)
        RhinoApp.WriteLine(msg);
      UnsafeNativeMethods.RHC_DebugPrint(msg);
#else
      Console.Write(msg);
#endif
    }
    /// <summary>
    /// Prints a debug message to the Rhino Command Line. 
    /// The message will only appear if the SendDebugToCommandLine property is set to true.
    /// </summary>
    /// <param name="format">Message to format and print.</param>
    /// <param name="args">An Object array containing zero or more objects to format.</param>
    public static void DebugString(string format, params object[] args)
    {
      string msg = string.Format(System.Globalization.CultureInfo.InvariantCulture, format, args);
      DebugString(msg);
    }
    /// <summary>
    /// Gets or sets whether debug messages are printed to the command line.
    /// </summary>
    public static bool SendDebugToCommandLine
    {
      get { return m_bSendDebugToRhino; }
      set { m_bSendDebugToRhino = value; }
    }

    /// <summary>
    /// Informs RhinoCommon of an exception that has been handled but that the developer wants to screen.
    /// </summary>
    /// <param name="ex">An exception.</param>
    public static void ExceptionReport(Exception ex)
    {
      ExceptionReport(null, ex);
    }

    /// <summary>
    /// Informs RhinoCommon of an exception that has been handled but that the developer wants to screen.
    /// </summary>
    /// <param name="source">An exception source text.</param>
    /// <param name="ex">An exception.</param>
    public static void ExceptionReport(string source, Exception ex)
    {
      if (null == ex)
        return;

      // Let's try and make sure exception reporting itself doesn't bring down Rhino
      try
      {
        string msg = ex.ToString();

        TypeLoadException tle = ex as TypeLoadException;
        if (tle != null)
        {
          string name = tle.TypeName;
          //if (!string.IsNullOrEmpty(name))
          msg = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}\nMissing Type = {1}", msg, name);
        }
        if (!string.IsNullOrEmpty(source))
          DebugString(source);
        DebugString(msg);

        if (OnExceptionReport != null)
          OnExceptionReport(source, ex);
      }
      catch(Exception)
      {
        // swallow it up
      }
    }

    /// <summary>
    /// Represents a reference to a method that will be called when an exception occurs.
    /// </summary>
    /// <param name="source">An exception source text.</param>
    /// <param name="ex">An exception.</param>
    public delegate void ExceptionReportDelegate(string source, Exception ex);

    /// <summary>
    /// Is raised when an exception is reported with one of the <see cref="ExceptionReport(Exception)"/> method.
    /// </summary>
    public static event ExceptionReportDelegate OnExceptionReport;

    /// <summary>
    /// Represents the type of message that is being sent to the OnSendLogMessageToCloud event
    /// </summary>
    /// 
    public enum LogMessageType : int
    {
      /// <summary>
      /// Unknown message type
      /// </summary>
      unknown = 0,
      /// <summary>
      /// Message is informational only
      /// </summary>
      information = 1,
      /// <summary>
      /// Message is a warning
      /// </summary>
      warning = 2,
      /// <summary>
      /// Message is an error
      /// </summary>
      error = 3,
      /// <summary>
      /// Message is a debug ASSERT
      /// </summary>
      assert = 4
    };

    /// <summary>
    /// Informs RhinoCommon of an message that has been handled but that the developer wants to screen.
    /// </summary>
    /// <param name="pwStringClass">The top level message type.</param>
    /// <param name="pwStringDesc">Finer grained description of the message.</param>
    /// <param name="pwStringMessage">The message.</param>
    /// <param name="msg_type">The messag type</param>
    public static void SendLogMessageToCloudCallbackProc(LogMessageType msg_type, IntPtr pwStringClass, IntPtr pwStringDesc, IntPtr pwStringMessage)
    {
      if (IntPtr.Zero == pwStringClass)
        return;

      if (IntPtr.Zero == pwStringDesc)
        return;

      if (IntPtr.Zero == pwStringMessage)
        return;

      // Let's try and make sure exception reporting itself doesn't bring down Rhino
      try
      {
        if (OnSendLogMessageToCloud != null)
        {

          string s_class = StringWrapper.GetStringFromPointer(pwStringClass);
          string s_desc = StringWrapper.GetStringFromPointer(pwStringDesc);
          string s_message  = StringWrapper.GetStringFromPointer(pwStringMessage);

          OnSendLogMessageToCloud(msg_type, s_class, s_desc, s_message);
        }
      }
      catch (Exception)
      {
        // swallow it up
      }
    }



    /// <summary>
    /// Represents a reference to a method that will be called when an exception occurs.
    /// </summary>
    /// <param name="sClass">The top level message type</param>
    /// <param name="sDesc">Finer grained description of the message.</param>
    /// <param name="sMessage">The message.</param>
    /// <param name="msg_type">The messag type</param>
    public delegate void SendLogMessageToCloudDelegate(LogMessageType msg_type, string sClass, string sDesc, string sMessage);

    /// <summary>
    /// Is raised when an exception is reported with one of the  method.
    /// </summary>
    public static event SendLogMessageToCloudDelegate OnSendLogMessageToCloud;



    /// <summary>
    /// Gets the debug dumps. This is a text description of the geometric contents.
    /// DebugDump() is intended for debugging and is not suitable for creating high
    /// quality text descriptions of an object.
    /// </summary>
    /// <param name="geometry">Some geometry.</param>
    /// <returns>A debug dump text.</returns>
    public static string DebugDumpToString(Rhino.Geometry.GeometryBase geometry)
    {
      IntPtr pConstThis = geometry.ConstPointer();
      using (var sh = new StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.ON_Object_Dump(pConstThis, pString);
        return sh.ToString();
      }
    }

    /// <summary>
    /// Gets the debug dumps. This is a text description of the geometric contents.
    /// DebugDump() is intended for debugging and is not suitable for creating high
    /// quality text descriptions of an object.
    /// </summary>
    /// <param name="bezierCurve">curve to evaluate</param>
    /// <returns>A debug dump text.</returns>
    public static string DebugDumpToString(Rhino.Geometry.BezierCurve bezierCurve)
    {
      IntPtr pConstThis = bezierCurve.ConstPointer();
      using (var sh = new StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.ON_BezierCurve_Dump(pConstThis, pString);
        return sh.ToString();
      }
    }

#if RHINO_SDK

    /// <summary>
    /// Used to help record times at startup with the -stopwatch flag to help
    /// determine bottlenecks in start up speed
    /// </summary>
    /// <param name="description"></param>
    public static void RecordInitInstanceTime(string description)
    {
      UnsafeNativeMethods.CRhinoApp_RecordInitInstanceTime(description);
    }

    /// <summary>
    /// Parses a plugin and create all the commands defined therein.
    /// </summary>
    /// <param name="plugin">Plugin to harvest for commands.</param>
    public static void CreateCommands(PlugIn plugin) 
    {
      if (plugin!=null)
        plugin.InternalCreateCommands();
    }

    /// <summary>
    /// Parses a plugin and create all the commands defined therein.
    /// </summary>
    /// <param name="pPlugIn">Plugin to harvest for commands.</param>
    /// <param name="pluginAssembly">Assembly associated with the plugin.</param>
    /// <returns>The number of newly created commands.</returns>
    public static int CreateCommands(IntPtr pPlugIn, System.Reflection.Assembly pluginAssembly)
    {
      int rc = 0;
      // This function must ONLY be called by Rhino_DotNet.Dll
      if (IntPtr.Zero == pPlugIn || null == pluginAssembly)
        return rc;

      Type[] exported_types = pluginAssembly.GetExportedTypes();
      if (null == exported_types)
        return rc;

      Type command_type = typeof(Commands.Command);
      for (int i = 0; i < exported_types.Length; i++)
      {
        if (exported_types[i].IsAbstract)
          continue;
        if (command_type.IsAssignableFrom(exported_types[i]))
        {
          if( PlugIn.CreateCommandsHelper(null, pPlugIn, exported_types[i], null))
            rc++;
        }
      }

      return rc;
    }

    /// <summary>
    /// Adds a new dynamic command to Rhino.
    /// </summary>
    /// <param name="plugin">Plugin that owns the command.</param>
    /// <param name="cmd">Command to add.</param>
    /// <returns>true on success, false on failure.</returns>
    public static bool RegisterDynamicCommand(PlugIn plugin, Commands.Command cmd)
    {
      // every command must have a RhinoId and Name attribute
      bool rc = false;
      if (plugin != null)
      {
        try
        {
          plugin.m_commands.Add(cmd);
          cmd.PlugIn = plugin;
          IntPtr ptr_plugin = plugin.NonConstPointer();
          string english_name = cmd.EnglishName;
          string local_name = cmd.LocalName;

          int command_style = 0;
          object[] styleattr = cmd.GetType().GetCustomAttributes(typeof(Commands.CommandStyleAttribute), true);
          if (styleattr != null && styleattr.Length > 0)
          {
            var a = (Commands.CommandStyleAttribute)styleattr[0];
            cmd.m_style_flags = a.Styles;
            command_style = (int)cmd.m_style_flags;
          }
          Guid id = cmd.Id;
          int sn = UnsafeNativeMethods.CRhinoCommand_New(ptr_plugin, id, english_name, local_name, command_style, 0);
          cmd.m_runtime_serial_number = sn;
          rc = sn!=0;
        }
        catch (Exception ex)
        {
          ExceptionReport(ex);
        }
      }
      return rc;
    }
#endif
    static int GetNowHelper(int localeId, IntPtr pStringHolderFormat, IntPtr pResultString)
    {
      int rc;
      try
      {
        string dateformat = StringHolder.GetString(pStringHolderFormat);
        if (string.IsNullOrEmpty(dateformat))
          return 0;
        // surround apostrophe with quotes in order to keep the formatter happy
        dateformat = dateformat.Replace("'", "\"'\"");
        System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(localeId);
        DateTime now = System.DateTime.Now;
        string s = string.IsNullOrEmpty(dateformat) ? now.ToString(ci) : now.ToString(dateformat, ci);
        UnsafeNativeMethods.ON_wString_Set(pResultString, s);
        rc = 1;
      }
      catch (Exception ex)
      {
        UnsafeNativeMethods.ON_wString_Set(pResultString, ex.Message);
        rc = 0;
      }
      return rc;
    }

    static int GetFormattedTimeHelper(int localeId, int sec, int min, int hour, int day, int month, int year, IntPtr pStringHolderFormat, IntPtr pResultString)
    {
      int rc;
      try
      {
        string dateformat = StringHolder.GetString(pStringHolderFormat);
        System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(localeId);
        DateTime dt = new DateTime(year, month, day, hour, min, sec);
        dt = dt.ToLocalTime();
        string s = string.IsNullOrEmpty(dateformat) ? dt.ToString(ci) : dt.ToString(dateformat, ci);
        UnsafeNativeMethods.ON_wString_Set(pResultString, s);
        rc = 1;
      }
      catch (Exception ex)
      {
        UnsafeNativeMethods.ON_wString_Set(pResultString, ex.Message);
        rc = 0;
      }
      return rc;
    }

    static int EvaluateExpressionHelper(IntPtr statementsAsStringHolder, IntPtr expressionAsStringHolder, uint rhinoDocSerialNumber, IntPtr pResultString)
    {
      int rc = 0;
#if RHINO_SDK
      try
      {
        // 11 July 2014 S. Baer (RH-28010)
        // Force the culture to invarient while running the evaluation
        var current = System.Threading.Thread.CurrentThread.CurrentCulture;
        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        string state = StringHolder.GetString(statementsAsStringHolder);
        string expr = StringHolder.GetString(expressionAsStringHolder);
        PythonScript py = PythonScript.Create();
        object eval_result = py.EvaluateExpression(state, expr);
        System.Threading.Thread.CurrentThread.CurrentCulture = current;
        if (null != eval_result)
        {
          string s = null;
          RhinoDoc doc = RhinoDoc.FromRuntimeSerialNumber(rhinoDocSerialNumber);
          if (eval_result is double || eval_result is float)
          {
            if (doc != null)
            {
              int display_precision = doc.DistanceDisplayPrecision;
              string format = "{0:0.";
              format = format.PadRight(display_precision + format.Length, '0') + "}";
              s = string.Format(format, eval_result);
            }
            else
              s = eval_result.ToString();
          }
          else if (eval_result is string)
          {
            s = eval_result.ToString();
          }
          System.Collections.IEnumerable enumerable = eval_result as System.Collections.IEnumerable;
          if (string.IsNullOrEmpty(s) && enumerable != null)
          {
            string format = null;
            if (doc != null)
            {
              int display_precision = doc.DistanceDisplayPrecision;
              format = "{0:0.";
              format = format.PadRight(display_precision + format.Length, '0') + "}";
            }
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (object obj in enumerable)
            {
              if (sb.Length > 0)
                sb.Append(", ");
              if ( (obj is double || obj is float) && !string.IsNullOrEmpty(format) )
              {
                sb.AppendFormat(format, obj);
              }
              else
              {
                sb.Append(obj);
              }
            }
            s = sb.ToString();
          }
          if (string.IsNullOrEmpty(s))
            s = eval_result.ToString();
          UnsafeNativeMethods.ON_wString_Set(pResultString, s);
        }
        rc = 1;
      }
      catch (Exception ex)
      {
        UnsafeNativeMethods.ON_wString_Set(pResultString, ex.Message);
        rc = 0;
      }
#endif
      return rc;
    }

#if RHINO_SDK
    /// <summary>
    /// Gets the auto install plug-in folder for machine or current user.
    /// </summary>
    /// <param name="currentUser">true if the query relates to the current user.</param>
    /// <returns>The full path to the revelant auto install plug-in directory.</returns>
    public static string AutoInstallPlugInFolder(bool currentUser)
    {
      // TODO: refactor to something more generic like GetPackageFolder
      // %programdata%\mcneel\rhinoceros\packages\6.0
      string data_dir = Rhino.ApplicationSettings.FileSettings.GetDataFolder(currentUser);
      var dir = new System.IO.DirectoryInfo(data_dir);
      // use MAJOR.0 for package folder regardless of whether this is an official build or not
      string name = $"{RhinoBuildConstants.MAJOR_VERSION_STRING}.0";
      string path = System.IO.Path.Combine(dir.Parent.FullName, "packages", name);
      return path;
    }

    static bool LocalMachineListWins
    {
      get { return true; }
    }


    static void BuildRegisteredPlugInList()
    {
      try
      {
        // set order of precedence (current user = true, local machine = false)
        bool[] dir_flags = LocalMachineListWins ? new bool[] { true, false } : new bool[] { false, true };
        foreach (bool dir_flag in dir_flags)
        {
          foreach (var active_version_directory in GetActivePlugInVersionFolders(dir_flag))
          {
            var rhps = active_version_directory.GetFiles("*.rhp", System.IO.SearchOption.TopDirectoryOnly);
            foreach(var rhp in rhps)
            {
              UnsafeNativeMethods.CRhinoPlugInManager_InstallPlugIn(rhp.FullName, true);
            }
          }
        }
      }
      catch(Exception ex)
      {
        ExceptionReport(ex);
      }
    }

    /// <summary>
    /// Recurses through the auto install plug-in folders and returns the directories containing "active" versions of plug-ins.
    /// </summary>
    /// <param name="currentUser">Current user (true) or machine (false).</param>
    /// <returns></returns>
    public static IEnumerable<System.IO.DirectoryInfo> GetActivePlugInVersionFolders(bool currentUser)
    {
      // TODO: this gets called a lot so we should probably cache the results
      string dir = AutoInstallPlugInFolder(currentUser);
      var install_directory = new System.IO.DirectoryInfo(dir);
      if (!install_directory.Exists)
        yield break;
      var child_directories = install_directory.GetDirectories();
      foreach (var child_directory in child_directories)
      {
        // find and read manifest file
        string manifest_path = System.IO.Path.Combine(child_directory.FullName, "manifest.txt");
        if (!System.IO.File.Exists(manifest_path))
          continue;
        string[] lines = System.IO.File.ReadAllLines(manifest_path);
        if (null == lines || lines.Length < 1)
          continue;
        var active_version_directory = new System.IO.DirectoryInfo(System.IO.Path.Combine(child_directory.FullName, lines[0]));
        if (!active_version_directory.Exists)
          continue;
        yield return active_version_directory;
      }
    }

    static Guid GetAssemblyId(IntPtr path)
    {
      try
      {
        string str_path = StringWrapper.GetStringFromPointer (path);
        var reflect_assembly = System.Reflection.Assembly.ReflectionOnlyLoadFrom(str_path);
        object[] idAttr = reflect_assembly.GetCustomAttributes(typeof(GuidAttribute), false);
        GuidAttribute id = (GuidAttribute)(idAttr[0]);
        return new Guid(id.Value);
      }
      catch (Exception)
      {
        return Guid.Empty;
      }
    }

    static int LoadPlugInHelper(IntPtr path, IntPtr pluginInfo, IntPtr errorMessage, int displayDebugInfo)
    {
      string str_path = StringWrapper.GetStringFromPointer (path);
      int rc = (int)PlugIn.LoadPlugInHelper(str_path, pluginInfo, errorMessage, displayDebugInfo != 0);
      return rc;
    }

    static Skin g_skin;
    static int LoadSkinHelper(IntPtr path, int displayDebugInfo)
    {
      if( g_skin!=null )
        return 0;

      string str_path = StringWrapper.GetStringFromPointer (path);
      if( string.IsNullOrWhiteSpace(str_path) )
        return 0;

      // attempt to load the assembly
      try
      {
        var assembly = System.Reflection.Assembly.LoadFrom( str_path );
        // look for a class derived from Rhino.Runtime.Skin
        var internal_types = assembly.GetExportedTypes();
        var skin_type = typeof(Skin);
        Skin skin = null;
        foreach (Type t in internal_types)
        {
          // Skip abstract classes during reflection creation
          if( t.IsAbstract )
            continue;
          if( skin_type.IsAssignableFrom(t) )
          {
            skin = Activator.CreateInstance(t) as Skin;
            if( skin!=null )
              break;
          }
        }
        g_skin = skin;
      }
      catch (Exception e)
      {
        if( displayDebugInfo!=0 )
        {
          RhinoApp.Write("(ERROR) Exception occurred in LoadSkin::ReflectionOnlyLoadFrom\n" );
          RhinoApp.WriteLine( e.Message );
        }
      }
      return (g_skin != null) ? 1:0;
    }

    internal delegate int EvaluateExpressionCallback(IntPtr statementsAsStringHolder, IntPtr expressionAsStringHolder, uint rhinoDocSerialNumber, IntPtr resultString);
    static readonly EvaluateExpressionCallback m_evaluate_callback = EvaluateExpressionHelper;
    internal delegate int GetNowCallback(int localeId, IntPtr formatAsStringHolder, IntPtr resultString);
    static readonly GetNowCallback m_getnow_callback = GetNowHelper;
    internal delegate int GetFormattedTimeCallback(int locale, int sec, int min, int hour, int day, int month, int year, IntPtr formatAsStringHolder, IntPtr resultString);
    static readonly GetFormattedTimeCallback m_getformattedtime_callback = GetFormattedTimeHelper;

    internal delegate void InitializeRDKCallback();
    static readonly InitializeRDKCallback m_rdk_initialize_callback = InitializeRhinoCommon_RDK;
    internal delegate void ShutdownRDKCallback();
    static readonly ShutdownRDKCallback m_rdk_shutdown_callback = ShutDownRhinoCommon_RDK;

    internal delegate int LoadPluginCallback(IntPtr path, IntPtr pluginInfo, IntPtr errorMessage, int displayDebugInfo);
    static readonly LoadPluginCallback m_loadplugin_callback = LoadPlugInHelper;
    internal delegate int LoadSkinCallback(IntPtr path, int displayDebugInfo);
    static readonly LoadSkinCallback m_loadskin_callback = LoadSkinHelper;
    static readonly Action m_buildplugin_list = BuildRegisteredPlugInList;
    internal delegate Guid GetAssemblyIdCallback(IntPtr path);
    static readonly GetAssemblyIdCallback m_getassembly_id = GetAssemblyId;

    internal delegate void SendLogMessageToCloudCallback(LogMessageType msg_type, IntPtr pwStringClass, IntPtr pwStringDesc, IntPtr pwStringError);
    static readonly SendLogMessageToCloudCallback m_send_log_message_to_cloud_callback = SendLogMessageToCloudCallbackProc;
#endif

    private static bool m_rhinocommoninitialized;
    private static int m_uiThreadId;
    /// <summary>
    /// Makes sure all static RhinoCommon components is set up correctly. 
    /// This happens automatically when a plug-in is loaded, so you probably won't 
    /// have to call this method.
    /// </summary>
    /// <remarks>Subsequent calls to this method will be ignored.</remarks>
    public static void InitializeRhinoCommon()
    {
      if (m_rhinocommoninitialized)
        return;
      m_rhinocommoninitialized = true;

      m_uiThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
      // Initialize exception handling
      AppDomain.CurrentDomain.UnhandledException += UnhandledDomainException;

      AssemblyResolver.InitializeAssemblyResolving();
      {
        Type t = typeof(Rhino.DocObjects.Custom.UserDictionary);
        Guid class_id = DocObjects.Custom.ClassIdAttribute.GetGuid(t);
        UnsafeNativeMethods.ON_UserData_RegisterCustomUserData(t.FullName, t.GUID, class_id);
        Rhino.DocObjects.Custom.UserData.RegisterType(t);

        t = typeof(Rhino.DocObjects.Custom.SharedUserDictionary);
        class_id = DocObjects.Custom.ClassIdAttribute.GetGuid(t);
        UnsafeNativeMethods.ON_UserData_RegisterCustomUserData(t.FullName, t.GUID, class_id);
        Rhino.DocObjects.Custom.UserData.RegisterType(t);
      }

#if RHINO_SDK
      DebugString("Initializing RhinoCommon");
      UnsafeNativeMethods.RHC_SetGetNowProc(m_getnow_callback, m_getformattedtime_callback);
      UnsafeNativeMethods.RHC_SetPythonEvaluateCallback(m_evaluate_callback);
      UnsafeNativeMethods.CRhinoCommonPlugInLoader_SetCallbacks(m_loadplugin_callback, m_loadskin_callback, m_buildplugin_list, m_getassembly_id);
      InitializeZooClient();

      UnsafeNativeMethods.RHC_SetRdkInitializationCallbacks(m_rdk_initialize_callback, m_rdk_shutdown_callback);

      UnsafeNativeMethods.RHC_SetSendLogMessageToCloudProc(m_send_log_message_to_cloud_callback);

      PersistentSettingsHooks.SetHooks();
      FileIO.FilePdf.SetHooks();
      UI.Localization.SetHooks();
      RhinoFileEventWatcherHooks.SetHooks();
#endif
    }

#if RHINO_SDK
    private static bool m_rhinocommonrdkinitialized;
    /// <summary>
    /// Makes sure all static RhinoCommon RDK components are set up correctly.
    /// This happens automatically when the RDK is loaded, so you probably won't
    /// have to call this method.
    /// </summary>
    /// <remarks>Subsequent calls to this method will be ignored.</remarks>
    public static void InitializeRhinoCommon_RDK()
    {
      if (m_rhinocommonrdkinitialized)
        return;
      m_rhinocommonrdkinitialized = true;

      Rhino.UI.Controls.CollapsibleSectionImpl.SetCppHooks(true);
      Rhino.UI.Controls.CollapsibleSectionHolderImpl.SetCppHooks(true);
      Rhino.UI.Controls.InternalRdkViewModel.SetCppHooks(true);
      Rhino.DocObjects.SnapShots.SnapShotsClient.SetCppHooks(true);
      Rhino.UI.Controls.FactoryBase.Register();

      UnsafeNativeMethods.SetRhCsInternetFunctionalityCallback(Rhino.Render.InternalUtilities.OnDownloadFileProc, Rhino.Render.InternalUtilities.OnUrlResponseProc);
    }

    /// <summary>
    /// Makes sure all static RhinoCommon RDK components are de-initialized so they aren't calling into space when the RDK is unloaded.
    /// </summary>
    /// <remarks>Subsequent calls to this method will be ignored.</remarks>
    public static void ShutDownRhinoCommon_RDK()
    {
        UnsafeNativeMethods.SetRhCsInternetFunctionalityCallback(null, null);

        Rhino.UI.Controls.CollapsibleSectionImpl.SetCppHooks(false);
        Rhino.UI.Controls.CollapsibleSectionHolderImpl.SetCppHooks(false);
        Rhino.UI.Controls.InternalRdkViewModel.SetCppHooks(false);
        Rhino.DocObjects.SnapShots.SnapShotsClient.SetCppHooks(false);
    }
#endif

    static bool g_ReportExceptions = true;
    /// <summary>
    /// For internal use only!!!
    /// Unhanded exception handler, writes stack trace to RhinoDotNet.txt file
    /// </summary>
    /// <param name="title">
    /// Exception title to write to text file
    /// </param>
    /// <param name="sender"></param>
    /// <param name="ex"></param>
    public static void RhinoCommonExceptionHandler(string title, object sender, Exception ex)
    {
      if (!g_ReportExceptions)
        return;

      // on macOS, we include the exception info in the standard crash reporter so we don't need a file on the desktop.
      if (!RunningOnOSX)
        WriteException(title, sender, ex);

      if (ex == null)
        return;
      var msg = ex.ToString (); // ToString() includes stack trace.
#if RHINO_SDK
#if DEBUG
      // only show dialog for the UI thread. Background threads dump to the console.
      if (m_uiThreadId == System.Threading.Thread.CurrentThread.ManagedThreadId)
        Rhino.UI.Dialogs.ShowMessage(msg, "Unhandled CurrentDomain Exception in .NET");
      else
        DebugString (msg);
#endif
#else
      Console.Error.Write(msg);
#endif
    }

    static void UnhandledDomainException(object sender, UnhandledExceptionEventArgs e)
    {
      RhinoCommonExceptionHandler("System::AppDomain::CurrentDomain->UnhandledException event occurred", sender, e.ExceptionObject as Exception);
    }

    /// <summary>
    /// Exception handler for exceptions occurring on the UI thread
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public static void UnhandledThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
    {
      RhinoCommonExceptionHandler("System::Windows::Forms::Application::ThreadException event occurred", sender, e.Exception);
    }

    private static bool g_write_log = true;
    private static void WriteException(string title, object sender, Exception ex )
    {
      if (!g_write_log)
        return;
      g_write_log = false;
      // create a text file on the desktop to log exception information to

      // Curtis 2018.09.04: 
      // Any changes to the output of this file should be reflected on Mac in RhinoDotNet_Mono.cpp

      var path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
      path = System.IO.Path.Combine(path, "RhinoDotNetCrash.txt");
      System.IO.StreamWriter text_file = null;
      try
      {
        text_file = new System.IO.StreamWriter(path);

        if (ex != null)
        {
          text_file.WriteLine($"[ERROR] FATAL UNHANDLED EXCEPTION: {ex}");
        }
        else
        {
          text_file.WriteLine("[ERROR] .NET STACK TRACE:");
          text_file.WriteLine(Environment.StackTrace);
        }

        text_file.WriteLine("[END ERROR]");
      }
      // ReSharper disable once EmptyGeneralCatchClause
      catch (Exception)
      {
			}
      finally
      {
        text_file?.Close();
      }
    }

    /// <summary>
    /// Initializes the ZooClient and Rhino license manager, this should get
    /// called automatically when RhinoCommon is loaded so you probably won't
    /// have to call this method.
    /// </summary>
    public static void InitializeZooClient()
    {
#if RHINO_SDK
      LicenseManager.SetCallbacks();
#endif
    }

#if RHINO_SDK
    /// <summary>
    /// Don't change this function in ANY way unless you chat with Steve first!
    /// This function is called by Rhino on initial startup and the signature
    /// must be exact
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static int CallFromCoreRhino(string task)
    {
      if( string.Equals(task, "initialize", StringComparison.InvariantCultureIgnoreCase) )
        InitializeRhinoCommon();
      else if (string.Equals(task, "shutdown", StringComparison.InvariantCultureIgnoreCase))
      {
        SetInShutDown();
      }
      return 1;
    }

    /// <summary>
    /// Instantiates a plug-in type and registers the associated commands and classes.
    /// </summary>
    /// <param name="pluginType">A plug-in type. This type must derive from <see cref="PlugIn"/>.</param>
    /// <param name="printDebugMessages">true if debug messages should be printed.</param>
    /// <returns>A new plug-in instance.</returns>
    public static PlugIn CreatePlugIn(Type pluginType, bool printDebugMessages)
    {
      return CreatePlugIn(pluginType, pluginType.Assembly, printDebugMessages, false);
    }

    /// <summary>
    /// Instantiates a plug-in type and registers the associated commands and classes.
    /// </summary>
    /// <param name="pluginType">A plug-in type. This type must derive from <see cref="PlugIn"/>.</param>
    /// <param name="pluginAssembly"></param>
    /// <param name="printDebugMessages">true if debug messages should be printed.</param>
    /// <param name="useRhinoDotNet"></param>
    /// <returns>A new plug-in instance.</returns>
    internal static PlugIn CreatePlugIn(Type pluginType, System.Reflection.Assembly pluginAssembly, bool printDebugMessages, bool useRhinoDotNet)
    {
      if (null == pluginType || !typeof(PlugIn).IsAssignableFrom(pluginType))
        return null;

      InitializeRhinoCommon();

      // If we turn on debug messages, we always get debug output
      if (printDebugMessages)
        SendDebugToCommandLine = true;

      // this function should only be called by Rhino_DotNet.dll
      // we could add some safety checks by performing validation on
      // the calling assembly
      //System.Reflection.Assembly.GetCallingAssembly();

      object[] name = pluginAssembly.GetCustomAttributes(typeof(System.Reflection.AssemblyTitleAttribute), false);
      string plugin_name = "";
      if (name.Length > 0)
        plugin_name = ((System.Reflection.AssemblyTitleAttribute)name[0]).Title;
      else
        plugin_name = pluginAssembly.GetName().Name;
      string plugin_version = pluginAssembly.GetName().Version.ToString();

      PlugIn plugin = PlugIn.Create(pluginType, plugin_name, plugin_version, useRhinoDotNet, pluginAssembly);

      if (plugin == null)
        return null;

      PlugIn.m_plugins.Add(plugin);
      return plugin;
    }

    static void DelegateReport(System.Delegate d, string name)
    {
      if (d == null) return;
      IFormatProvider fp = System.Globalization.CultureInfo.InvariantCulture;
      string title = string.Format(fp, "{0} Event\n", name);
      UnsafeNativeMethods.CRhinoEventWatcher_LogState(title);
      Delegate[] list = d.GetInvocationList();
      if (list != null && list.Length > 0)
      {
        for (int i = 0; i < list.Length; i++)
        {
          Delegate subD = list[i];
          Type t = subD.Target.GetType();
          string msg = string.Format(fp, "- Plug-In = {0}\n", t.Assembly.GetName().Name);
          UnsafeNativeMethods.CRhinoEventWatcher_LogState(msg);
        }
      }
    }

    internal delegate void ReportCallback(int c);
    internal static ReportCallback m_ew_report = EventWatcherReport;
    internal static void EventWatcherReport(int c)
    {
      UnsafeNativeMethods.CRhinoEventWatcher_LogState("RhinoCommon delegate based event watcher\n");
      DelegateReport(RhinoApp.m_init_app, "InitApp");
      DelegateReport(RhinoApp.m_close_app, "CloseApp");
      DelegateReport(RhinoApp.m_appsettings_changed, "AppSettingsChanged");
      DelegateReport(Rhino.Commands.Command.m_begin_command, "BeginCommand");
      DelegateReport(Rhino.Commands.Command.m_end_command, "EndCommand");
      DelegateReport(Rhino.Commands.Command.m_undo_event, "Undo");
      DelegateReport(RhinoDoc.m_close_document, "CloseDocument");
      DelegateReport(RhinoDoc.m_new_document, "NewDocument");
      DelegateReport(RhinoDoc.m_document_properties_changed, "DocuemtnPropertiesChanged");
      DelegateReport(RhinoDoc.m_begin_open_document, "BeginOpenDocument");
      DelegateReport(RhinoDoc.m_end_open_document, "EndOpenDocument");
      DelegateReport(RhinoDoc.m_begin_save_document, "BeginSaveDocument");
      DelegateReport(RhinoDoc.m_end_save_document, "EndSaveDocument");
      DelegateReport(RhinoDoc.m_add_object, "AddObject");
      DelegateReport(RhinoDoc.m_delete_object, "DeleteObject");
      DelegateReport(RhinoDoc.m_replace_object, "ReplaceObject");
      DelegateReport(RhinoDoc.m_undelete_object, "UndeleteObject");
      DelegateReport(RhinoDoc.m_purge_object, "PurgeObject");
    }

    internal delegate void RdkReportCallback(int c);
    internal static RdkReportCallback m_rdk_ew_report = RdkEventWatcherReport;
    internal static void RdkEventWatcherReport(int c)
    {
      UnsafeNativeMethods.CRdkCmnEventWatcher_LogState("RhinoRdkCommon delegate based event watcher\n");
      DelegateReport(Rhino.Render.RenderContent.m_content_added_event, "RenderContentAdded");
    }

    internal static object m_rhinoscript;
    internal static object GetRhinoScriptObject()
    {
      return m_rhinoscript ?? (m_rhinoscript = Rhino.RhinoApp.GetPlugInObject("RhinoScript"));
    }

    /// <summary>
    /// Defines if Ole alerts ("Server busy") alerts should be visualized.
    /// <para>This function makes no sense on Mono.</para>
    /// </summary>
    /// <param name="display">Whether alerts should be visible.</param>
    public static void DisplayOleAlerts(bool display)
    {
      UnsafeNativeMethods.RHC_DisplayOleAlerts(display);
    }
#endif

    internal static bool ContainsDelegate(MulticastDelegate source, Delegate d)
    {
      if (null != source && null != d)
      {
        Delegate[] list = source.GetInvocationList();
        if (null != list)
        {
          for (int i = 0; i < list.Length; i++)
          {
            if (list[i].Equals(d))
              return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Only works on Windows. Returns null on Mac.
    /// </summary>
    /// <returns>An assembly.</returns>
    public static System.Reflection.Assembly GetRhinoDotNetAssembly()
    {
      if (m_rhdn_assembly == null && RunningOnWindows)
      {
        System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
          if (assemblies[i].FullName.StartsWith("Rhino_DotNet", StringComparison.OrdinalIgnoreCase))
          {
            m_rhdn_assembly = assemblies[i];
            break;
          }
        }
      }
      return m_rhdn_assembly;
    }
    static System.Reflection.Assembly m_rhdn_assembly;

    /// <summary>
    /// Informs the runtime that the application is shutting down.
    /// </summary>
    public static void SetInShutDown()
    {
      // 26 June 2018 S. Baer (RH-46531)
      // Don't report exceptions after Rhino starts shutting down. It typically
      // just clutters the desktop with RhinoDotNetCrash.txt files
      g_ReportExceptions = false;
      try
      {
#if RHINO_SDK
        UnsafeNativeMethods.RHC_SetSendLogMessageToCloudProc(null);
#endif
        UnsafeNativeMethods.RhCmn_SetInShutDown();
        // Remove callbacks that should not happen after this point in time
#if RHINO_SDK
        Rhino.Render.RdkPlugIn.SetRdkCallbackFunctions(false);
        Skin.DeletePointer();
#endif
      }
      catch
      {
        //throw away, we are shutting down
      }
    }

#if RHINO_SDK
    internal static void WriteIntoSerializationInfo(IntPtr pRhCmnProfileContext, System.Runtime.Serialization.SerializationInfo info, string prefixStrip)
    {
      const int _string = 1;
      const int _multistring = 2;
      const int _uuid = 3;
      const int _color = 4;
      const int _int = 5;
      const int _double = 6;
      const int _rect = 7;
      const int _point = 8;
      const int _3dpoint = 9;
      const int _xform = 10;
      const int _3dvector = 11;
      const int _meshparams = 12;
      const int _buffer = 13;
      const int _bool = 14;
      int count = UnsafeNativeMethods.CRhCmnProfileContext_Count(pRhCmnProfileContext);
      using (StringHolder sectionholder = new StringHolder())
      using (StringHolder entryholder = new StringHolder())
      {
        IntPtr pStringSection = sectionholder.NonConstPointer();
        IntPtr pStringEntry = entryholder.NonConstPointer();
        for (int i = 0; i < count; i++)
        {
          int pctype = 0;
          UnsafeNativeMethods.CRhCmnProfileContext_Item(pRhCmnProfileContext, i, pStringSection, pStringEntry, ref pctype);
          string section = sectionholder.ToString();
          string entry = entryholder.ToString();
          if (string.IsNullOrEmpty(entry))
            continue;
          string name = string.IsNullOrEmpty(section) ? entry : section + "\\" + entry;
          if (name.StartsWith(prefixStrip + "\\"))
            name = name.Substring(prefixStrip.Length + 1);
          name = name.Replace("\\", "::");

          switch (pctype)
          {
            case _string:
              {
                UnsafeNativeMethods.CRhinoProfileContext_LoadString(pRhCmnProfileContext, section, entry, pStringEntry);
                string val = entryholder.ToString();
                info.AddValue(name, val);
              }
              break;
            case _multistring:
              {
                using (var strings = new ClassArrayString())
                {
                  IntPtr ptr_strings = strings.NonConstPointer();
                  UnsafeNativeMethods.CRhinoProfileContext_LoadStrings(pRhCmnProfileContext, section, entry, ptr_strings);
                  string[] s = strings.ToArray();
                  info.AddValue(name, s);
                }
              }
              break;
            case _uuid:
              {
                Guid id = Guid.Empty;
                UnsafeNativeMethods.CRhinoProfileContext_LoadGuid(pRhCmnProfileContext, section, entry, ref id);
                info.AddValue(name, id);
              }
              break;
            case _color:
              {
                int abgr = 0;
                UnsafeNativeMethods.CRhinoProfileContext_LoadColor(pRhCmnProfileContext, section, entry, ref abgr);
                System.Drawing.Color c = Interop.ColorFromWin32(abgr);
                //string s = System.Drawing.ColorTranslator.ToHtml(c);
                info.AddValue(name, c);
              }
              break;
            case _int:
              {
                int ival = 0;
                UnsafeNativeMethods.CRhinoProfileContext_LoadInt(pRhCmnProfileContext, section, entry, ref ival);
                info.AddValue(name, ival);
              }
              break;
            case _double:
              {
                double dval = 0;
                UnsafeNativeMethods.CRhinoProfileContext_LoadDouble(pRhCmnProfileContext, section, entry, ref dval);
                info.AddValue(name, dval);
              }
              break;
            case _rect:
              {
                int left = 0, top = 0, right = 0, bottom = 0;
                UnsafeNativeMethods.CRhinoProfileContext_LoadRect(pRhCmnProfileContext, section, entry, ref left, ref top, ref right, ref bottom);
                System.Drawing.Rectangle r = System.Drawing.Rectangle.FromLTRB(left, top, right, bottom);
                info.AddValue(name, r);
              }
              break;
            case _point:
              {
                int x = 0, y = 0;
                UnsafeNativeMethods.CRhinoProfileContext_LoadPoint(pRhCmnProfileContext, section, entry, ref x, ref y);
                System.Drawing.Point pt = new System.Drawing.Point(x, y);
                info.AddValue(name, pt);
              }
              break;
            case _3dpoint:
              {
                Rhino.Geometry.Point3d pt = new Geometry.Point3d();
                UnsafeNativeMethods.CRhinoProfileContext_LoadPoint3d(pRhCmnProfileContext, section, entry, ref pt);
                info.AddValue(name, pt);
              }
              break;
            case _xform:
              {
                Rhino.Geometry.Transform xf = new Geometry.Transform();
                UnsafeNativeMethods.CRhinoProfileContext_LoadXform(pRhCmnProfileContext, section, entry, ref xf);
                info.AddValue(name, xf);
              }
              break;
            case _3dvector:
              {
                Rhino.Geometry.Vector3d vec = new Geometry.Vector3d();
                UnsafeNativeMethods.CRhinoProfileContext_LoadVector3d(pRhCmnProfileContext, section, entry, ref vec);
                info.AddValue(name, vec);
              }
              break;
            case _meshparams:
              {
                Rhino.Geometry.MeshingParameters mp = new Geometry.MeshingParameters();
                UnsafeNativeMethods.CRhinoProfileContext_LoadMeshParameters(pRhCmnProfileContext, section, entry, mp.NonConstPointer());
                info.AddValue(name, mp);
                mp.Dispose();
              }
              break;
            case _buffer:
              {
                //not supported yet
                //int buffer_length = UnsafeNativeMethods.CRhinoProfileContext_BufferLength(pRhCmnProfileContext, section, entry);
                //byte[] buffer = new byte[buffer_length];
                //UnsafeNativeMethods.CRhinoProfileContext_LoadBuffer(pRhCmnProfileContext, section, entry, buffer_length, buffer);
                //info.AddValue(name, buffer);
              }
              break;
            case _bool:
              {
                bool b = false;
                UnsafeNativeMethods.CRhinoProfileContext_LoadBool(pRhCmnProfileContext, section, entry, ref b);
                info.AddValue(name, b);
              }
              break;
          }
        }
      }
    }

    internal static IntPtr ReadIntoProfileContext(System.Runtime.Serialization.SerializationInfo info, string sectionBase)
    {
      IntPtr pProfileContext = UnsafeNativeMethods.CRhCmnProfileContext_New();
      var e = info.GetEnumerator();
      while (e.MoveNext())
      {
        string entry = e.Name.Replace("::", "\\");
        string section = sectionBase;
        int split_index = entry.LastIndexOf("\\", System.StringComparison.Ordinal);
        if (split_index > -1)
        {
          section = sectionBase + "\\" + entry.Substring(0, split_index);
          entry = entry.Substring(split_index + 1);
        }

        
        Type t = e.ObjectType;
        if( typeof(string) == t )
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileString(pProfileContext, section, entry, e.Value as string);
        else if( typeof(Guid) == t )
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileUuid(pProfileContext, section, entry, (Guid)e.Value);
        else if( typeof(System.Drawing.Color) == t )
        {
          System.Drawing.Color c = (System.Drawing.Color)e.Value;
          int argb = c.ToArgb();
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileColor(pProfileContext, section, entry, argb);
        }
        else if( typeof(int) == t )
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileInt(pProfileContext, section, entry, (int)e.Value);
        else if( typeof(double) == t )
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileDouble(pProfileContext, section, entry, (double)e.Value);
        else if( typeof(System.Drawing.Rectangle) == t )
        {
          System.Drawing.Rectangle r = (System.Drawing.Rectangle)e.Value;
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileRect(pProfileContext, section, entry, r.Left, r.Top, r.Right, r.Bottom);
        }
        else if( typeof(System.Drawing.Point) == t )
        {
          System.Drawing.Point pt = (System.Drawing.Point)e.Value;
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfilePoint(pProfileContext, section, entry, pt.X, pt.Y);
        }
        else if( typeof(Rhino.Geometry.Point3d) == t )
        {
          Rhino.Geometry.Point3d pt = (Rhino.Geometry.Point3d)e.Value;
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfilePoint3d(pProfileContext, section, entry, pt);
        }
        else if( typeof(Rhino.Geometry.Transform) == t )
        {
          Rhino.Geometry.Transform xf = (Rhino.Geometry.Transform)e.Value;
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileXform(pProfileContext, section, entry, ref xf);
        }
        else if( typeof(Rhino.Geometry.Vector3d) == t )
        {
          Rhino.Geometry.Vector3d v = (Rhino.Geometry.Vector3d)e.Value;
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileVector3d(pProfileContext, section, entry, v);
        }
        else if( typeof(Rhino.Geometry.MeshingParameters) == t )
        {
          Rhino.Geometry.MeshingParameters mp = e.Value as Rhino.Geometry.MeshingParameters;
          if (mp != null)
          {
            IntPtr pMp = mp.ConstPointer();
            UnsafeNativeMethods.CRhinoProfileContext_SaveProfileMeshingParameters(pProfileContext, section, entry, pMp);
          }
        }
        else if( typeof(byte[]) == t )
        {
          byte[] b = e.Value as byte[];
          if (b != null)
          {
            UnsafeNativeMethods.CRhinoProfileContext_SaveProfileBuffer(pProfileContext, section, entry, b.Length, b);
          }
        }
        else if (typeof(bool) == t)
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileBool(pProfileContext, section, entry, (bool)e.Value);
        else
        {
          //try
          //{
            string s = info.GetString(e.Name);
            UnsafeNativeMethods.CRhinoProfileContext_SaveProfileString(pProfileContext, section, entry, s);
          //}
          //catch (Exception ex)
          //{
          //  throw;
          //}
        }
      }
      return pProfileContext;
    }
#endif
  }

  /// <summary>
  /// Is thrown when the RDK is not loaded.
  /// </summary>
  [Serializable]
  public class RdkNotLoadedException : Exception
  {
    /// <summary>
    /// Initializes a new instance of the RDK not loaded exception with a standard message.
    /// </summary>
    public RdkNotLoadedException() : base("The Rhino Rdk is not loaded.") { }
  }
}
