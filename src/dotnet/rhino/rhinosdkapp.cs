#if RHINO_SDK

using System;
using System.Collections.Generic;
using System.Text;
using Rhino.ApplicationSettings;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Runtime.InteropWrappers;

namespace Rhino.ApplicationSettings
{
  /// <summary>License node types.</summary>
  public enum LicenseNode
  {
    /// <summary>An independent node.</summary>
    Standalone = 0,
    /// <summary>Network (obtains license from Zoo server)</summary>
    Network = 1,
    /// <summary>Network (has license checked out from Zoo server)</summary>
    NetworkCheckedOut = 2
  }

  /// <summary>The type of Rhino executable that is executing</summary>
  public enum Installation
  {
    ///<summary>Unknown</summary>
    Undefined = 0,
    ///<summary></summary>
    Commercial,
    ///<summary></summary>
    Educational,
    ///<summary></summary>
    EducationalLab,
    ///<summary></summary>
    NotForResale,
    ///<summary></summary>
    NotForResaleLab,
    ///<summary></summary>
    Beta,
    ///<summary></summary>
    BetaLab,
    ///<summary>25 Save limit evaluation version of Rhino</summary>
    Evaluation,
    ///<summary></summary>
    Corporate,
    ///<summary>90 day time limit evaluation version of Rhino</summary>
    EvaluationTimed
  }
}

namespace Rhino
{
  internal class InvokeHelper
  {
    class CallbackWithArgs
    {
      public CallbackWithArgs(Delegate callback, params object[] args)
      {
        Callback = callback;
        Args = args;
      }
      public Delegate Callback { get; private set; }
      public object[] Args { get; private set; }
    }
    static readonly object g_invoke_lock = new object();
    static List<CallbackWithArgs> g_callbacks;
    internal delegate void InvokeAction();
    private static InvokeAction g_on_invoke_callback;

    public void Invoke(Delegate method, params object[] args)
    {
      lock (g_invoke_lock)
      {
        if (g_callbacks == null)
          g_callbacks = new List<CallbackWithArgs>();
        g_callbacks.Add(new CallbackWithArgs(method, args));
      }

      if (g_on_invoke_callback == null)
        g_on_invoke_callback = InvokeCallback;
      UnsafeNativeMethods.CRhMainFrame_Invoke(g_on_invoke_callback);
    }

    /// <summary>
    /// See Control.InvokeRequired
    /// </summary>
    public bool InvokeRequired
    {
      get
      {
        return UnsafeNativeMethods.CRhMainFrame_InvokeRequired();
      }
    }

    private static void InvokeCallback()
    {
      try
      {
        CallbackWithArgs[] actions = null;
        lock (g_invoke_lock)
        {
          if (g_callbacks != null)
          {
            actions = g_callbacks.ToArray();
            g_callbacks.Clear();
          }
        }
        if (actions == null || actions.Length < 1)
          return;
        foreach (var item in actions)
          item.Callback.DynamicInvoke(item.Args);
      }
      catch (Exception ex)
      {
        Runtime.HostUtils.ExceptionReport(ex);
      }
    }
  }

  /// <summary> Represents the top level window in Rhino </summary>
  [System.ComponentModel.Browsable(false), System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
  public class RhinoWindow : System.Windows.Forms.IWin32Window
  {
    readonly IntPtr m_handle;

    internal RhinoWindow(IntPtr handle)
    {
      m_handle = handle;
    }

    /// <summary></summary>
    public IntPtr Handle
    {
      get { return m_handle; }
    }

    /// <summary>
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public void Invoke(Delegate method)
    {
      RhinoApp.InvokeOnUiThread(method);
    }

    /// <summary> See Control.InvokeRequired </summary>
    public bool InvokeRequired
    {
      get
      {
        return RhinoApp.InvokeRequired;
      }
    }
  }

  ///<summary>.NET RhinoApp is parallel to C++ CRhinoApp.</summary>
  public static class RhinoApp
  {
    internal static int GetInt(UnsafeNativeMethods.RhinoAppInt which)
    {
      return UnsafeNativeMethods.CRhinoApp_GetInt(which);
    }

    static bool GetBool(UnsafeNativeMethods.RhinoAppBool which)
    {
      return UnsafeNativeMethods.CRhinoApp_GetBool(which);
    }

    ///<summary>
    ///Rhino SDK 9 digit SDK version number in the form YYYYMMDDn
    ///
    ///Rhino will only load plug-ins that were build with exactly the
    ///same version of the SDK.
    ///</summary>
    public static int SdkVersion
    {
      get { return GetInt(UnsafeNativeMethods.RhinoAppInt.SdkVersion); }
    }

    ///<summary>
    ///Rhino SDK 9 digit SDK service release number in the form YYYYMMDDn
    ///
    ///Service service release of the Rhino SDK supported by this executable. Rhino will only
    ///load plug-ins that require a service release of &lt;= this release number.
    ///For example, SR1 will load all plug-ins made with any SDK released up through and including
    ///the SR1 SDK. But, SR1 will not load a plug-in built using the SR2 SDK. If an &quot;old&quot; Rhino
    ///tries to load a &quot;new&quot; plug-in, the user is told that they have to get a free Rhino.exe
    ///update in order for the plug-in to load. Rhino.exe updates are available from http://www.rhino3d.com.
    ///</summary>
    public static int SdkServiceRelease
    {
      get { return GetInt(UnsafeNativeMethods.RhinoAppInt.SdkServiceRelease); }
    }

    ///<summary>
    ///Major version of Rhino executable 4, 5, ...
    ///</summary>
    public static int ExeVersion
    {
      get { return GetInt(UnsafeNativeMethods.RhinoAppInt.ExeVersion); }
    }

    ///<summary>
    ///Service release version of Rhino executable (0, 1, 2, ...)  
    ///The integer is the service release number of Rhino.  For example,
    ///this function returns &quot;0&quot; if Rhino V4SR0 is running and returns
    ///&quot;1&quot; if Rhino V4SR1 is running.
    ///</summary>
    public static int ExeServiceRelease
    {
      get { return GetInt(UnsafeNativeMethods.RhinoAppInt.ExeServiceRelease); }
    }

    /// <summary>
    /// Gets the build date.
    /// </summary>
    public static DateTime BuildDate
    {
      get
      {
        int year = 0;
        int month = 0;
        int day = 0;
        UnsafeNativeMethods.CRhinoApp_GetBuildDate(ref year, ref month, ref day);
        // debug builds are 0000-00-00
        if( year==0 && month==0 && day==0 )
          return DateTime.MinValue;
        return new DateTime(year, month, day);
      }
    }

    /// <summary>
    /// McNeel version control revision identifier at the time this version
    /// of Rhino was built.
    /// </summary>
    public static string VersionControlRevision
    {
      get
      {
        using (var sh = new StringHolder())
        {
          IntPtr ptr_string = sh.NonConstPointer();
          UnsafeNativeMethods.ON_Revision(ptr_string);
          return sh.ToString();
        }
      }
    }

    static Version g_version;
    /// <summary> File version of the main Rhino process </summary>
    public static Version Version
    {
      get { return g_version ?? (g_version = new Version(RhinoBuildConstants.VERSION_STRING)); }
    }

    /// <summary>Gets the product serial number, as seen in Rhino's ABOUT dialog box.</summary>
    public static string SerialNumber
    {
      get
      {
        using (var sh = new StringHolder())
        {
          IntPtr ptr_string = sh.NonConstPointer();
          UnsafeNativeMethods.CRhinoApp_GetString(UnsafeNativeMethods.RhinoAppString.SerialNumber, ptr_string);
          return sh.ToString();
        }
      }
    }

    /// <summary>Gets the name of the user that owns the license or lease.</summary>
    public static string LicenseUserName
    {
      get
      {
        using (var sh = new StringHolder())
        {
          IntPtr ptr_string = sh.NonConstPointer();
          UnsafeNativeMethods.CRhinoApp_GetString(UnsafeNativeMethods.RhinoAppString.LicenseUserName, ptr_string);
          return sh.ToString();
        }
      }
    }

    /// <summary>Gets the name of the organization of the user that owns the license or lease.</summary>
    public static string LicenseUserOrganization
    {
      get
      {
        using (var sh = new StringHolder())
        {
          IntPtr ptr_string = sh.NonConstPointer();
          UnsafeNativeMethods.CRhinoApp_GetString(UnsafeNativeMethods.RhinoAppString.LicenseUserOrganization, ptr_string);
          return sh.ToString();
        }
      }
    }

    /// <summary>Gets the type of installation (product edition) of the license or lease.</summary>
    public static string InstallationTypeString
    {
      get
      {
        using (var sh = new StringHolder())
        {
          IntPtr ptr_string = sh.NonConstPointer();
          UnsafeNativeMethods.CRhinoApp_GetString(UnsafeNativeMethods.RhinoAppString.InstallationTypeString, ptr_string);
          return sh.ToString();
        }
      }
    }

    /// <summary>Gets the application name.</summary>
    public static string Name
    {
      get
      {
        using (var sh = new StringHolder())
        {
          IntPtr ptr_string = sh.NonConstPointer();
          UnsafeNativeMethods.CRhinoApp_GetString(UnsafeNativeMethods.RhinoAppString.ApplicationName, ptr_string);
          return sh.ToString();
        }
      }
    }

    /// <summary>Gets license the node type.</summary>
    public static LicenseNode NodeType
    {
      get
      {
        int rc = GetInt(UnsafeNativeMethods.RhinoAppInt.NodeType);
        return (LicenseNode)rc;
      }
    }

    ///<summary>Gets the product installation type, as seen in Rhino's ABOUT dialog box.</summary>
    public static Installation InstallationType
    {
      get
      {
        int rc = GetInt(UnsafeNativeMethods.RhinoAppInt.Installation);
        return (Installation)rc;
      }
    }

    /// <summary>
    /// Gets the current Registry scheme name.
    /// </summary>
    public static string SchemeName
    {
      get
      {
        using (var sh = new StringHolder())
        {
          IntPtr ptr_string = sh.NonConstPointer();
          UnsafeNativeMethods.CRhinoApp_GetString(UnsafeNativeMethods.RhinoAppString.RegistrySchemeName, ptr_string);
          return sh.ToString();
        }

      }
    }
      
    /// <summary>
    /// Gets the data directory.
    /// </summary>
    /// <returns>The data directory.</returns>
    /// <param name="localUser">If set to <c>true</c> local user.</param>
    /// <param name="forceDirectoryCreation">If set to <c>true</c> force directory creation.</param>
    public static string GetDataDirectory(bool localUser, bool forceDirectoryCreation)
    {
      return GetDataDirectory (localUser, forceDirectoryCreation, string.Empty);
    }

    /// <summary>
    /// Gets the data directory.
    /// </summary>
    /// <returns>The data directory.</returns>
    /// <param name="localUser">If set to <c>true</c> local user.</param>
    /// <param name="forceDirectoryCreation">If set to <c>true</c> force directory creation.</param>
    /// <param name="subDirectory">
    /// Sub directory, will get appended to the end of the data directory.  if forceDirectoryCreation
    /// is true then this directory will get created and writable.
    /// </param>
    public static string GetDataDirectory(bool localUser, bool forceDirectoryCreation, string subDirectory)
    {
      if (Runtime.HostUtils.RunningOnOSX)
      {
        using (var string_holder = new StringHolder ())
        {
          IntPtr ptr = string_holder.NonConstPointer();
          if (localUser)
            UnsafeNativeMethods.RhCmn_UserRhinocerosApplicationSupportDirectory(forceDirectoryCreation, subDirectory, ptr);
          else
            UnsafeNativeMethods.RhCmn_LocalRhinocerosApplicationSupportDirectory(forceDirectoryCreation, subDirectory, ptr);
          var value = string_holder.ToString();
          return value;
        }
      }
      var special_folder = Environment.GetFolderPath (localUser ? Environment.SpecialFolder.ApplicationData : Environment.SpecialFolder.CommonApplicationData);
      var version = Version;
      var result = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(special_folder, "McNeel"), "Rhinoceros"), string.Format("{0}.{1}", version.Major, 0));
      return result;
    }

    /// <summary>
    /// directory
    /// </summary>
    public static System.IO.DirectoryInfo GetExecutableDirectory()
    {
      using (var sw = new StringWrapper())
      {
        IntPtr ptrString = sw.NonConstPointer;
        bool rc = UnsafeNativeMethods.CRhinoApp_ExecutableFolder(ptrString);
        if (!rc)
          throw new Exception("ExecutableDirectory call failed");
        string directoryName = sw.ToString();
        return new System.IO.DirectoryInfo(directoryName);
      }
    }

    //static property System::String^ Name{ System::String^ get(); }
    //static property System::String^ RegistryKeyName{ System::String^ get(); }


    ///<summary>Gets the ID of Rhino 2.</summary>
    public static Guid Rhino2Id
    {
      get { return UnsafeNativeMethods.CRhinoApp_GetGUID(UnsafeNativeMethods.RhinoAppGuid.Rhino2Id); }
    }

    ///<summary>Gets the ID of Rhino 3.</summary>
    public static Guid Rhino3Id
    {
      get { return UnsafeNativeMethods.CRhinoApp_GetGUID(UnsafeNativeMethods.RhinoAppGuid.Rhino3Id); }
    }

    ///<summary>Gets the ID of Rhino 4.</summary>
    public static Guid Rhino4Id
    {
      get { return UnsafeNativeMethods.CRhinoApp_GetGUID(UnsafeNativeMethods.RhinoAppGuid.Rhino4Id); }
    }

    ///<summary>Gets the ID of Rhino 5.</summary>
    public static Guid Rhino5Id
    {
      get { return UnsafeNativeMethods.CRhinoApp_GetGUID(UnsafeNativeMethods.RhinoAppGuid.Rhino5Id); }
    }

    ///<summary>Gets the current ID of Rhino.</summary>
    public static Guid CurrentRhinoId
    {
      get { return UnsafeNativeMethods.CRhinoApp_GetGUID(UnsafeNativeMethods.RhinoAppGuid.CurrentRhinoId); }
    }

    /// <summary>Is Rhino currently being executed through automation</summary>
    public static bool IsRunningAutomated
    {
      get { return UnsafeNativeMethods.CRhinoApp_IsAutomated(); }
    }

    /// <summary>Is Rhino currently being executed in headless mode</summary>
    public static bool IsRunningHeadless
    {
      get { return UnsafeNativeMethods.CRhinoApp_IsHeadless(); }
    }

    /// <summary>
    /// Is Rhino currently using custom, user-interface Skin.
    /// </summary>
    public static bool IsSkinned
    {
      get { return UnsafeNativeMethods.CRhinoApp_IsSkinned(); }
    }

    //static bool IsRhinoId( System::Guid id );
    static readonly object g_lock_object = new object();
    ///<summary>Print formatted text in the command window.</summary>
    public static void Write(string message)
    {
      lock (g_lock_object)
      {
        // don't allow '%' characters to be misinterpreted as format codes
        message = message.Replace("%", "%%");
        UnsafeNativeMethods.CRhinoApp_Print(message);
      }
    }

    /// <summary>
    /// Provides a text writer that writes to the command line.
    /// </summary>
    public class CommandLineTextWriter : System.IO.TextWriter
    {
      /// <summary>
      /// Returns Encoding Unicode.
      /// </summary>
      public override Encoding Encoding
      {
        get
        {
          return Encoding.Unicode;
        }
      }

      /// <summary>
      /// Writes a string to the command line.
      /// </summary>
      public override void Write(string value)
      {
        RhinoApp.Write(value);
      }

      /// <summary>
      /// Writes a char to the command line.
      /// </summary>
      public override void Write(char value)
      {
        RhinoApp.Write(char.ToString(value));
      }

      /// <summary>
      /// Writes a char buffer to the command line.
      /// </summary>
      public override void Write(char[] buffer, int index, int count)
      {
        RhinoApp.Write(new string(buffer, index, count));
      }

      /// <summary>
      /// Provided to give a simple way to IronPython to call this class.
      /// </summary>
      /// <param name="str">The text.</param>
      [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
      [CLSCompliant(false)]
      public void write(string str)
      {
        Write(str);
      }
    }
    static CommandLineTextWriter g_writer = new CommandLineTextWriter();
    /// <summary>
    /// Provides a TextWriter that can write to the command line.
    /// </summary>
    public static CommandLineTextWriter CommandLineOut
    {
      get
      {
        return g_writer;
      }
    }


    ///<summary>Print formatted text in the command window.</summary>
    public static void Write(string format, object arg0)
    {
      Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, format, arg0));
    }
    ///<summary>Print formatted text in the command window.</summary>
    public static void Write(string format, object arg0, object arg1)
    {
      Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, format, arg0, arg1));
    }
    ///<summary>Print formatted text in the command window.</summary>
    public static void Write(string format, object arg0, object arg1, object arg2)
    {
      Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, format, arg0, arg1, arg2));
    }

    ///<summary>Print a newline in the command window.</summary>
    public static void WriteLine()
    {
      Write("\n");
    }
    ///<summary>Print text in the command window.</summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addlayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlayer.py' lang='py'/>
    /// </example>
    public static void WriteLine(string message)
    {
      Write(message + "\n");
    }
    ///<summary>Print formatted text with a newline in the command window.</summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addlayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlayer.py' lang='py'/>
    /// </example>
    public static void WriteLine(string format, object arg0)
    {
      Write(format + "\n", arg0);
    }
    ///<summary>Print formatted text with a newline in the command window.</summary>
    public static void WriteLine(string format, object arg0, object arg1)
    {
      Write(format + "\n", arg0, arg1);
    }
    ///<summary>Print formatted text with a newline in the command window.</summary>
    public static void WriteLine(string format, object arg0, object arg1, object arg2)
    {
      Write(format + "\n", arg0, arg1, arg2);
    }

    /// <summary>
    /// Print a string to the Visual Studio Output window, if a debugger is attached.
    ///
    /// Note that the developer needs to add a newline manually if the next output should
    /// come on a separate line.
    /// </summary>
    /// <param name="str">The string to print to the Output window.</param>
    public static void OutputDebugString(string str)
    {
      UnsafeNativeMethods.RHC_OutputDebugString(str);
    }

    /// <summary>
    /// Set the text that appears in the Rhino command prompt.
    /// In general, you should use the SetCommandPrompt functions. 
    /// In rare cases, like worker thread messages, the message that 
    /// appears in the prompt has non-standard formatting. In these 
    /// rare cases, SetCommandPromptMessage can be used to literally 
    /// specify the text that appears in the command prompt window.
    /// </summary>
    /// <param name="prompt">A literal text for the command prompt window.</param>
    public static void SetCommandPromptMessage(string prompt)
    {
      UnsafeNativeMethods.CRhinoApp_SetCommandPromptMessage(prompt);
      RhinoApp.Wait();
    }

    ///<summary>Sets the command prompt in Rhino.</summary>
    ///<param name="prompt">The new prompt text.</param>
    ///<param name="promptDefault">
    /// Text that appears in angle brackets and indicates what will happen if the user pressed ENTER.
    ///</param>
    public static void SetCommandPrompt(string prompt, string promptDefault)
    {
      UnsafeNativeMethods.CRhinoApp_SetCommandPrompt(prompt, promptDefault);
      RhinoApp.Wait();
    }
    ///<summary>Set Rhino command prompt.</summary>
    ///<param name="prompt">The new prompt text.</param>
    public static void SetCommandPrompt(string prompt)
    {
      UnsafeNativeMethods.CRhinoApp_SetCommandPrompt(prompt, null);
      RhinoApp.Wait();
    }

    ///<summary>Rhino command prompt.</summary>
    public static string CommandPrompt
    {
      get
      {
        using (var sh = new StringHolder())
        {
          IntPtr ptr_string = sh.NonConstPointer();
          UnsafeNativeMethods.CRhinoApp_GetString(UnsafeNativeMethods.RhinoAppString.CommandPrompt, ptr_string);
          return sh.ToString();
        }
      }
      set
      {
        UnsafeNativeMethods.CRhinoApp_SetCommandPrompt(value, null);
      }
    }

    /// <summary>
    /// Text in Rhino's command history window.
    /// </summary>
    public static string CommandHistoryWindowText
    {
      get
      {
        using (var holder = new StringHolder())
        {
          UnsafeNativeMethods.CRhinoApp_GetCommandHistoryWindowText(holder.NonConstPointer());
          string rc = holder.ToString();
          if (string.IsNullOrEmpty(rc))
            return string.Empty;
          rc = rc.Replace('\r', '\n');
          return rc;
        }
      }
    }
    /// <summary>
    /// Clear the text in Rhino's command history window.
    /// </summary>
    public static void ClearCommandHistoryWindow()
    {
      UnsafeNativeMethods.CRhinoApp_ClearCommandHistoryWindowText();
    }

    ///<summary>Sends a string of printable characters, including spaces, to Rhino&apos;s command line.</summary>
    ///<param name='characters'>[in] A string to characters to send to the command line. This can be null.</param>
    ///<param name='appendReturn'>[in] Append a return character to the end of the string.</param>
    public static void SendKeystrokes(string characters, bool appendReturn)
    {
      UnsafeNativeMethods.CRhinoApp_SendKeystrokes(characters, appendReturn);
    }

    ///<summary>Sets the focus to the main window.</summary>
    public static void SetFocusToMainWindow()
    {
      UnsafeNativeMethods.CRhinoApp_SetFocusToMainWindow();
    }

    ///<summary>Releases the mouse capture.</summary>
    public static bool ReleaseMouseCapture()
    {
      return UnsafeNativeMethods.CRhinoApp_ReleaseCapture();
    }

    //[DllImport(Import.lib)]
    //static extern IntPtr CRhinoApp_DefaultRenderer([MarshalAs(UnmanagedType.LPWStr)] string str);
    /////<summary>Rhino's current, or default, render plug-in.</summary>
    //public static string DefaultRenderer
    //{
    //  get
    //  {
    //    IntPtr rc = CRhinoApp_DefaultRenderer(null);
    //    if (IntPtr.Zero == rc)
    //      return null;
    //    return Marshal.PtrToStringUni(rc);
    //  }
    //  set
    //  {
    //    CRhinoApp_DefaultRenderer(value);
    //  }
    //}

    ///<summary>Exits, or closes, Rhino.</summary>
    public static void Exit()
    {
      UnsafeNativeMethods.CRhinoApp_Exit();
    }

    internal static bool InEventWatcher { get; set; }

    ///<summary>Runs a Rhino command script.</summary>
    ///<param name="script">[in] script to run.</param>
    ///<param name="echo">
    /// Controls how the script is echoed in the command output window.
    /// false = silent - nothing is echoed.
    /// true = verbatim - the script is echoed literally.
    ///</param>
    ///<remarks>
    /// Rhino acts as if each character in the script string had been typed in the command prompt.
    /// When RunScript is called from a &quot;script runner&quot; command, it completely runs the
    /// script before returning. When RunScript is called outside of a command, it returns and the
    /// script is run. This way menus and buttons can use RunScript to execute complicated functions.
    ///</remarks>
    ///<exception cref="System.ApplicationException">
    /// If RunScript is being called while inside an event watcher.
    ///</exception>
    public static bool RunScript(string script, bool echo)
    {
      if (InEventWatcher)
      {
        const string msg = "Do not call RunScript inside of an event watcher.  Contact steve@mcneel.com to dicuss why you need to do this.";
        throw new ApplicationException(msg);
      }
      int echo_mode = echo ? 1 : 0;
      return UnsafeNativeMethods.CRhinoApp_RunScript1(script, echo_mode);
    }

    /// <summary>
    /// Execute a Rhino command.
    /// </summary>
    /// <param name="document">Document to execute the command for</param>
    /// <param name="commandName">Name of command to run.  Use command's localized name or preface with an underscore.</param>
    /// <returns>Returns the reult of the command.</returns>
    public static Commands.Result ExecuteCommand(RhinoDoc document, string commandName)
    {
      return (Commands.Result)UnsafeNativeMethods.CRhinoApp_ExecuteCommand(document.RuntimeSerialNumber, commandName);
    }

    ///<summary>Runs a Rhino command script.</summary>
    ///<param name="script">[in] script to run.</param>
    ///<param name="mruDisplayString">[in] String to display in the most recent command list.</param>
    ///<param name="echo">
    /// Controls how the script is echoed in the command output window.
    /// false = silent - nothing is echoed.
    /// true = verbatim - the script is echoed literally.
    ///</param>
    ///<remarks>
    /// Rhino acts as if each character in the script string had been typed in the command prompt.
    /// When RunScript is called from a &quot;script runner&quot; command, it completely runs the
    /// script before returning. When RunScript is called outside of a command, it returns and the
    /// script is run. This way menus and buttons can use RunScript to execute complicated functions.
    ///</remarks>
    ///<exception cref="System.ApplicationException">
    /// If RunScript is being called while inside an event watcher.
    ///</exception>
    public static bool RunScript(string script, string mruDisplayString, bool echo)
    {
      if (InEventWatcher)
      {
        const string msg = "Do not call RunScript inside of an event watcher.  Contact steve@mcneel.com to dicuss why you need to do this.";
        throw new ApplicationException(msg);
      }
      int echo_mode = echo ? 1 : 0;
      return UnsafeNativeMethods.CRhinoApp_RunScript2(script, mruDisplayString, echo_mode);
    }

    /// <summary>
    ///   Run a Rhino menu item script.  Will add the selected menu string to the MRU command menu.
    /// </summary>
    ///<param name="script">[in] script to run.</param>
    ///<remarks>
    /// Rhino acts as if each character in the script string had been typed in the command prompt.
    /// When RunScript is called from a &quot;script runner&quot; command, it completely runs the
    /// script before returning. When RunScript is called outside of a command, it returns and the
    /// script is run. This way menus and buttons can use RunScript to execute complicated functions.
    ///</remarks>
    ///<exception cref="System.ApplicationException">
    /// If RunScript is being called while inside an event watcher.
    ///</exception>
    /// <returns></returns>
    public static bool RunMenuScript(string script)
    {
      if (InEventWatcher)
      {
        const string msg = "Do not call RunMenuScript inside of an event watcher.  Contact steve@mcneel.com to dicuss why you need to do this.";
        throw new ApplicationException(msg);
      }
      return UnsafeNativeMethods.CRhinoApp_RunMenuScript(script);
    }

    /// <summary>
    /// Pauses to keep Windows message pump alive so views will update
    /// and windows will repaint.
    /// </summary>
    public static void Wait()
    {
      UnsafeNativeMethods.CRhinoApp_Wait(0);
    }


    static readonly InvokeHelper g_invoke_helper = new InvokeHelper();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="method"></param>
    /// <param name="args"></param>
    public static void InvokeOnUiThread(Delegate method, params object[] args)
    {
      g_invoke_helper.Invoke(method, args);
    }

    static bool m_done;
    static Exception m_ex;
    /// <summary>
    /// Work-In-Progess. Testing this with our unit test framework
    /// </summary>
    /// <param name="action"></param>
    public static void InvokeAndWait(Action action)
    {
      // lame implementation, just a start
      m_done = false;
      m_ex = null;
      InvokeOnUiThread(new Action<Action>(ActionWrapper), action);
      while (!m_done)
        System.Threading.Thread.Sleep(100);
      m_done = false;
      if (m_ex != null)
        throw m_ex;
    }

    static void ActionWrapper(Action action)
    {
      try
      {
        action();
      }
      catch(Exception ex)
      {
        m_ex = ex;
      }
      m_done = true;
    }

    /// <summary>
    /// Returns true if we are currently not running on the main user interface thread
    /// </summary>
    public static bool InvokeRequired
    {
      get
      {
        return g_invoke_helper.InvokeRequired;
      }
    }


    /// <summary>
    /// Gets the WindowHandle of the Rhino main window.
    /// </summary>
    public static IntPtr MainWindowHandle()
    {
      return UnsafeNativeMethods.CRhinoApp_GetMainFrameHWND();
    }

    static RhinoWindow g_main_window;

    /// <summary> Main Rhino Window </summary>
    [System.ComponentModel.Browsable(false), System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [Obsolete("Use MainWindowHandle or RhinoEtoApp.MainWindow in Rhino.UI")]
    public static System.Windows.Forms.IWin32Window MainWindow()
    {
      if (null == g_main_window)
      {
        IntPtr handle = MainWindowHandle();
        if (IntPtr.Zero != handle)
          g_main_window = new RhinoWindow(handle);
      }
      return g_main_window;
    }

    /// <summary>
    /// Same as MainWindow function, but provides the concrete class instead of an interface
    /// </summary>
    [System.ComponentModel.Browsable(false), System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [Obsolete("Use MainWindowHandle or RhinoEtoApp.MainWindow in Rhino.UI")]
    public static RhinoWindow MainApplicationWindow
    {
      get { return MainWindow() as RhinoWindow; }
    }

    /// <summary>
    /// Gets the object that is returned by PlugIn.GetPlugInObject for a given
    /// plug-in. This function attempts to find and load a plug-in with a given Id.
    /// When a plug-in is found, it's GetPlugInObject function is called and the
    /// result is returned here.
    /// Note the plug-in must have already been installed in Rhino or the plug-in manager
    /// will not know where to look for a plug-in with a matching id.
    /// </summary>
    /// <param name="pluginId">Guid for a given plug-in.</param>
    /// <returns>
    /// Result of PlugIn.GetPlugInObject for a given plug-in on success.
    /// </returns>
    public static object GetPlugInObject(Guid pluginId)
    {
      if (pluginId == Guid.Empty)
        return null;

      // see if the plug-in is already loaded before doing any heavy lifting
      PlugIns.PlugIn p = PlugIns.PlugIn.GetLoadedPlugIn(pluginId);
      if (p != null)
        return p.GetPlugInObject();


      // load plug-in
      UnsafeNativeMethods.CRhinoPlugInManager_LoadPlugIn(pluginId, true, false);
      p = PlugIns.PlugIn.GetLoadedPlugIn(pluginId);
      if (p != null)
        return p.GetPlugInObject();

      IntPtr iunknown = UnsafeNativeMethods.CRhinoApp_GetPlugInObject(pluginId);
      if (IntPtr.Zero == iunknown)
        return null;

      object rc;
      try
      {
        rc = System.Runtime.InteropServices.Marshal.GetObjectForIUnknown(iunknown);
      }
      catch (Exception)
      {
        rc = null;
      }
      return rc;
    }

    /// <summary>
    /// Gets the object that is returned by PlugIn.GetPlugInObject for a given
    /// plug-in. This function attempts to find and load a plug-in with a given name.
    /// When a plug-in is found, it's GetPlugInObject function is called and the
    /// result is returned here.
    /// Note the plug-in must have already been installed in Rhino or the plug-in manager
    /// will not know where to look for a plug-in with a matching name.
    /// </summary>
    /// <param name="plugin">Name of a plug-in.</param>
    /// <returns>
    /// Result of PlugIn.GetPlugInObject for a given plug-in on success.
    /// </returns>
    public static object GetPlugInObject(string plugin)
    {
      Guid plugin_id;
      if (!Guid.TryParse(plugin, out plugin_id))
        plugin_id = Guid.Empty;

      if (plugin_id == Guid.Empty)
        plugin_id = UnsafeNativeMethods.CRhinoPlugInManager_GetPlugInId(plugin);

      return GetPlugInObject(plugin_id);
    }

    /// <summary>
    /// If licenseType is an evaluation license, returns true. An evaluation license limits the ability of
    /// Rhino to save based on either the number of saves or a fixed period of time.
    /// </summary>
    /// <seealso cref="Installation"/>
    /// <param name="licenseType"></param>
    /// <returns>true if licenseType is an evaluation license. false otherwise</returns>
    public static bool IsInstallationEvaluation(Installation licenseType)
    {
      return (licenseType == Installation.Evaluation ||
              licenseType == Installation.EvaluationTimed);
    }

    /// <summary>
    /// If licenseType is a commercial license, returns true. A commercial license grants
    /// full use of the product.
    /// </summary>
    /// <param name="licenseType"></param>
    /// <seealso cref="Installation"/>
    /// <returns>true if licenseType is a commercial license. false otherwise</returns>
    public static bool IsInstallationCommercial(Installation licenseType)
    {
      return (licenseType == Installation.Commercial     ||
              licenseType == Installation.Corporate      ||
              licenseType == Installation.Educational    ||
              licenseType == Installation.EducationalLab ||
              licenseType == Installation.NotForResale   ||
              licenseType == Installation.NotForResaleLab);
    }

    /// <summary>
    /// If licenseType is a beta license, returns true. A beta license grants
    /// full use of the product during the pre-release development period.
    /// </summary>
    /// <param name="licenseType"></param>
    /// <seealso cref="Installation"/>
    /// <returns>true if licenseType is a beta license. false otherwise</returns>
    public static bool IsInstallationBeta(Installation licenseType)
    {
      return (licenseType == Installation.Beta || licenseType == Installation.BetaLab);
    }

    /// <summary>
    /// Returns 
    ///   true if the license will expire
    ///   false otherwise
    /// </summary>
    public static bool LicenseExpires
    {
      get { return GetBool(UnsafeNativeMethods.RhinoAppBool.LicenseExpires); }
    }

    /// <summary>
    /// Returns
    ///   true if Rhino is compiled a s Pre-release build (Beta, WIP)
    ///   false otherwise
    /// </summary>
    public static bool IsPreRelease
    {
      get { return GetBool(UnsafeNativeMethods.RhinoAppBool.IsPreRelease); }
    }

    /// <summary>
    /// Returns 
    ///   true if the license is validated
    ///   false otherwise
    /// </summary>
    public static bool IsLicenseValidated
    {
      get { return GetBool(UnsafeNativeMethods.RhinoAppBool.IsLicenseValidated); }
    }

    /// <summary>
    /// Returns 
    ///   true if rhino is currently using the Cloud Zoo
    ///   false otherwise
    /// </summary>
    public static bool IsCloudZooNode
    {
      get { return GetBool(UnsafeNativeMethods.RhinoAppBool.IsCloudZooNode); }
    }

    /// <summary>
    /// Returns number of days within which validation must occur. Zero when
    ///   validation grace period has expired.
    /// Raises InvalidLicenseTypeException if LicenseType is one of:
    ///   EvaluationSaveLimited
    ///   EvaluationTimeLimited
    ///   Viewer
    ///   Unknown
    /// </summary>
    public static int ValidationGracePeriodDaysLeft
    {
      get { return GetInt(UnsafeNativeMethods.RhinoAppInt.ValidationGracePeriodDaysLeft); }
    }

    /// <summary>
    /// Returns number of days until license expires. Zero when
    ///   license is expired.
    /// Raises InvalidLicenseTypeException if LicenseExpires
    /// would return false.
    /// </summary>
    public static int DaysUntilExpiration
    {
      get { return GetInt(UnsafeNativeMethods.RhinoAppInt.DaysUntilExpiration); }
    }

    /// <summary>
    /// Display UI asking the user to enter a license for Rhino or use one from the Zoo.
    /// </summary>
    /// <param name="standAlone">True to ask for a stand-alone license, false to ask the user for a license from the Zoo</param>
    /// <param name="parentWindow">Parent window for the user interface dialog.</param>
    /// <returns></returns>
    public static bool AskUserForRhinoLicense(bool standAlone, object parentWindow)
    {
      var handle_parent = UI.Dialogs.Service.ObjectToWindowHandle(parentWindow, true);
      return UnsafeNativeMethods.CRhinoApp_AskUserForRhinoLicense(standAlone, handle_parent);
    }


    /// <summary>
    /// Display UI asking the user to enter a license for the product specified by licenseId.
    /// </summary>
    /// <param name="pluginId">Guid identifying the plugin that is requesting a change of license key</param>
    /// <returns>true on success, false otherwise</returns>
    public static bool ChangeLicenseKey(Guid pluginId)
    {
      return UnsafeNativeMethods.CRhinoApp_ChangeLicenseKey(pluginId);
    }

    /// <summary>
    /// Refresh the license used by Rhino. This allows any part of Rhino to ensure that the most current version of the license file on disk is in use.
    /// </summary>
    /// <returns></returns>
    public static bool RefreshRhinoLicense()
    {
      return UnsafeNativeMethods.CRhinoApp_RefreshRhinoLicense();
    }

    /// <summary>
    /// Logs in to the cloud zoo.
    /// </summary>
    public static bool LoginToCloudZoo()
    {
      return Rhino.PlugIns.LicenseUtils.ZooClient.LoginToCloudZoo();
    }

    /// <summary>
    /// Returns the name of the logged in user, or null if the user is not logged in.
    /// </summary>
    public static string LoggedInUserName
    {
      get
      {
        return Rhino.PlugIns.LicenseUtils.ZooClient.LoggedInUserName;
      }
    }

    /// <summary>
    /// Returns the logged in user's avatar picture. 
    /// Returns a default avatar if the user does not have an avatar or if the avatar could not be fetched.
    /// </summary>
    public static System.Drawing.Image LoggedInUserAvatar
    {
      get
      {
        return Rhino.PlugIns.LicenseUtils.ZooClient.LoggedInUserAvatar;
      }
    }

    /// <summary>
    /// Returns true if the user is logged in; else returns false.
    /// A logged in user does not guarantee that the auth tokens managed by the CloudZooManager instance are valid.
    /// </summary>
    public static bool UserIsLoggedIn
    {
      get
      {
        return Rhino.PlugIns.LicenseUtils.ZooClient.UserIsLoggedIn;
      }
    }

    #region events
    // Callback that doesn't pass any parameters or return values
    internal delegate void RhCmnEmptyCallback();

    private static RhCmnEmptyCallback m_OnEscapeKey;
    private static void OnEscapeKey()
    {
      if (m_escape_key != null)
      {
        try
        {
          m_escape_key(null, System.EventArgs.Empty);
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    private static EventHandler m_escape_key;

    /// <summary>
    /// Can add or removed delegates that are raised when the escape key is clicked.
    /// </summary>
    public static event EventHandler EscapeKeyPressed
    {
      add
      {
        if (Runtime.HostUtils.ContainsDelegate(m_escape_key, value))
          return;

        m_escape_key += value;
        m_OnEscapeKey = OnEscapeKey;
        UnsafeNativeMethods.RHC_SetEscapeKeyCallback(m_OnEscapeKey);
      }
      remove
      {
        m_escape_key -= value;
        if (null == m_escape_key)
        {
          UnsafeNativeMethods.RHC_SetEscapeKeyCallback(null);
          m_OnEscapeKey = null;
        }
      }
    }

    // Callback that doesn't pass any parameters or return values
    /// <summary>
    /// KeyboardEvent delegate
    /// </summary>
    /// <param name="key"></param>
    public delegate void KeyboardHookEvent(int key);

    private static KeyboardHookEvent m_OnKeyboardEvent;
    private static void OnKeyboardEvent(int key)
    {
      if (m_keyboard_event != null)
      {
        try
        {
          m_keyboard_event(key);
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    private static KeyboardHookEvent m_keyboard_event;

    /// <summary>
    /// Can add or removed delegates that are raised by a keyboard event.
    /// </summary>
    public static event KeyboardHookEvent KeyboardEvent
    {
      add
      {
        if (Runtime.HostUtils.ContainsDelegate(m_escape_key, value))
          return;

        m_keyboard_event += value;
        m_OnKeyboardEvent = OnKeyboardEvent;
        UnsafeNativeMethods.RHC_SetKeyboardCallback(m_OnKeyboardEvent);
      }
      remove
      {
        m_keyboard_event -= value;
        if (null == m_escape_key)
        {
          UnsafeNativeMethods.RHC_SetKeyboardCallback(null);
          m_OnEscapeKey = null;
        }
      }
    }

    private static RhCmnEmptyCallback m_OnInitApp;
    private static void OnInitApp()
    {
      if (m_init_app != null)
      {
        try
        {
          m_init_app(null, EventArgs.Empty);
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    internal static EventHandler m_init_app;

    private static readonly object m_event_lock = new object();

    /// <summary>
    /// Is raised when the apllication is fully initialized.
    /// </summary>
    public static event EventHandler Initialized
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_init_app == null)
          {
            m_OnInitApp = OnInitApp;
            UnsafeNativeMethods.CRhinoEventWatcher_SetInitAppCallback(m_OnInitApp, Runtime.HostUtils.m_ew_report);
          }
          m_init_app -= value;
          m_init_app += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_init_app -= value;
          if (m_init_app == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetInitAppCallback(null, Runtime.HostUtils.m_ew_report);
            m_OnInitApp = null;
          }
        }
      }
    }


    private static RhCmnEmptyCallback m_OnCloseApp;
    private static void OnCloseApp()
    {
      if (m_close_app != null)
      {
        try
        {
          m_close_app(null, EventArgs.Empty);
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }

    internal static EventHandler m_close_app;

    /// <summary>
    /// Is raised when the application is about to close.
    /// </summary>
    public static event EventHandler Closing
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_close_app == null)
          {
            m_OnCloseApp = OnCloseApp;
            UnsafeNativeMethods.CRhinoEventWatcher_SetCloseAppCallback(m_OnCloseApp, Runtime.HostUtils.m_ew_report);
          }
          m_close_app -= value;
          m_close_app += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_close_app -= value;
          if (m_close_app == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetCloseAppCallback(null, Runtime.HostUtils.m_ew_report);
            m_OnCloseApp = null;
          }
        }
      }
    }


    private static RhCmnEmptyCallback m_OnAppSettingsChanged;
    private static void OnAppSettingsChanged()
    {
      if (m_appsettings_changed != null)
      {
        try
        {
          m_appsettings_changed(null, EventArgs.Empty);
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }

    internal static EventHandler m_appsettings_changed;

    /// <summary>
    /// Is raised when settings are changed.
    /// </summary>
    public static event EventHandler AppSettingsChanged
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_appsettings_changed == null)
          {
            m_OnAppSettingsChanged = OnAppSettingsChanged;
            UnsafeNativeMethods.CRhinoEventWatcher_SetAppSettingsChangeCallback(m_OnAppSettingsChanged, Runtime.HostUtils.m_ew_report);
          }
          m_appsettings_changed -= value;
          m_appsettings_changed += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_appsettings_changed -= value;
          if (m_appsettings_changed == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetAppSettingsChangeCallback(null, Runtime.HostUtils.m_ew_report);
            m_OnAppSettingsChanged = null;
          }
        }
      }
    }

    private static RhCmnEmptyCallback m_OnIdle;
    private static void OnIdle()
    {
      if (m_idle_occured != null)
      {
        try
        {
          m_idle_occured(null, EventArgs.Empty);
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    private static EventHandler m_idle_occured;

    /// <summary>
    /// Occurs when the application finishes processing and is about to enter the idle state
    /// </summary>
    public static event EventHandler Idle
    {
      add
      {
        lock (m_event_lock)
        {
          if (m_idle_occured == null)
          {
            m_OnIdle = OnIdle;
            UnsafeNativeMethods.CRhinoEventWatcher_SetOnIdleCallback(m_OnIdle);
          }
          m_idle_occured -= value;
          m_idle_occured += value;
        }
      }
      remove
      {
        lock (m_event_lock)
        {
          m_idle_occured -= value;
          if (m_idle_occured == null)
          {
            UnsafeNativeMethods.CRhinoEventWatcher_SetOnIdleCallback(null);
            m_OnIdle = null;
          }
        }
      }
    }


    #endregion

    #region RDK events

    internal delegate void RhCmnOneUintCallback(uint docSerialNumber);
    private static RhCmnOneUintCallback m_OnNewRdkDocument;
    private static void OnNewRdkDocument(uint docSerialNumber)
    {
      if (m_new_rdk_document != null)
      {
        var doc = RhinoDoc.FromRuntimeSerialNumber(docSerialNumber);
        try                     { m_new_rdk_document(doc, EventArgs.Empty); }
        catch (Exception ex)    { Runtime.HostUtils.ExceptionReport(ex); }
      }
    }
    internal static EventHandler m_new_rdk_document;

    /// <summary>
    /// Monitors when RDK document information is rebuilt.
    /// </summary>
    public static event EventHandler RdkNewDocument
    {
      add
      {
        if (m_new_rdk_document == null)
        {
          m_OnNewRdkDocument = OnNewRdkDocument;
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetNewRdkDocumentEventCallback(m_OnNewRdkDocument, Rhino.Runtime.HostUtils.m_rdk_ew_report);
        }
        m_new_rdk_document += value;
      }
      remove
      {
        m_new_rdk_document -= value;
        if (m_new_rdk_document == null)
        {
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetNewRdkDocumentEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
          m_OnNewRdkDocument = null;
        }
      }
    }



    private static RhCmnEmptyCallback m_OnRdkGlobalSettingsChanged;
    private static void OnRdkGlobalSettingsChanged()
    {
      if (m_rdk_global_settings_changed != null)
      {
        try { m_rdk_global_settings_changed(null, System.EventArgs.Empty); }
        catch (Exception ex) { Runtime.HostUtils.ExceptionReport(ex); }
      }
    }
    internal static EventHandler m_rdk_global_settings_changed;

    /// <summary>
    /// Monitors when RDK global settings are modified.
    /// </summary>
    public static event EventHandler RdkGlobalSettingsChanged
    {
      add
      {
        if (m_rdk_global_settings_changed == null)
        {
          m_OnRdkGlobalSettingsChanged = OnRdkGlobalSettingsChanged;
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetGlobalSettingsChangedEventCallback(m_OnRdkGlobalSettingsChanged, Rhino.Runtime.HostUtils.m_rdk_ew_report);
        }
        m_rdk_global_settings_changed += value;
      }
      remove
      {
        m_rdk_global_settings_changed -= value;
        if (m_rdk_global_settings_changed == null)
        {
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetGlobalSettingsChangedEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
          m_OnRdkGlobalSettingsChanged = null;
        }
      }
    }
    

    private static RhCmnOneUintCallback m_OnRdkUpdateAllPreviews;
    private static void OnRdkUpdateAllPreviews(uint docSerialNumber)
    {
      if (m_rdk_update_all_previews != null)
      {
        var doc = RhinoDoc.FromRuntimeSerialNumber(docSerialNumber);
        try { m_rdk_update_all_previews(doc, EventArgs.Empty); }
        catch (Exception ex) { Runtime.HostUtils.ExceptionReport(ex); }
      }
    }
    internal static EventHandler m_rdk_update_all_previews;

    /// <summary>
    /// Monitors when RDK thumbnails are updated.
    /// </summary>
    public static event EventHandler RdkUpdateAllPreviews
    {
      add
      {
        if (m_rdk_update_all_previews == null)
        {
          m_OnRdkUpdateAllPreviews = OnRdkUpdateAllPreviews;
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetUpdateAllPreviewsEventCallback(m_OnRdkUpdateAllPreviews, Rhino.Runtime.HostUtils.m_rdk_ew_report);
        }
        m_rdk_update_all_previews += value;
      }
      remove
      {
        m_rdk_update_all_previews -= value;
        if (m_rdk_update_all_previews == null)
        {
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetUpdateAllPreviewsEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
          m_OnRdkUpdateAllPreviews = null;
        }
      }
    }
    

    private static RhCmnEmptyCallback m_OnCacheImageChanged;
    private static void OnRdkCacheImageChanged()
    {
      if (m_rdk_cache_image_changed != null)
      {
        try { m_rdk_cache_image_changed(null, System.EventArgs.Empty); }
        catch (Exception ex) { Runtime.HostUtils.ExceptionReport(ex); }
      }
    }
    internal static EventHandler m_rdk_cache_image_changed;

    /// <summary>
    /// Monitors when the RDK thumbnail cache images are changed.
    /// </summary>
    public static event EventHandler RdkCacheImageChanged
    {
      add
      {
        if (m_rdk_cache_image_changed == null)
        {
          m_OnCacheImageChanged = OnRdkCacheImageChanged;
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetCacheImageChangedEventCallback(m_OnCacheImageChanged, Rhino.Runtime.HostUtils.m_rdk_ew_report);
        }
        m_rdk_cache_image_changed += value;
      }
      remove
      {
        m_rdk_cache_image_changed -= value;
        if (m_rdk_cache_image_changed == null)
        {
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetCacheImageChangedEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
          m_OnCacheImageChanged = null;
        }
      }
    }

    private static RhCmnEmptyCallback m_OnRendererChanged;
    private static void OnRendererChanged()
    {
      if (m_renderer_changed != null)
      {
        try { m_renderer_changed(null, System.EventArgs.Empty); }
        catch (Exception ex) { Runtime.HostUtils.ExceptionReport(ex); }
      }
    }
    internal static EventHandler m_renderer_changed;

    /// <summary>
    /// Monitors when Rhino's current renderer changes.
    /// </summary>
    public static event EventHandler RendererChanged
    {
      add
      {
        if (m_renderer_changed == null)
        {
          m_OnRendererChanged = OnRendererChanged;
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetRendererChangedEventCallback(m_OnRendererChanged, Rhino.Runtime.HostUtils.m_rdk_ew_report);
        }
        m_renderer_changed += value;
      }
      remove
      {
        m_renderer_changed -= value;
        if (m_renderer_changed == null)
        {
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetRendererChangedEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
          m_OnRendererChanged = null;
        }
      }
    }



    internal delegate void ClientPlugInUnloadingCallback(Guid plugIn);
    private static ClientPlugInUnloadingCallback m_OnClientPlugInUnloading;
    private static void OnClientPlugInUnloading(Guid plugIn)
    {
      if (m_client_plugin_unloading != null)
      {
        try { m_renderer_changed(null, System.EventArgs.Empty); }
        catch (Exception ex) { Runtime.HostUtils.ExceptionReport(ex); }
      }
    }
    internal static EventHandler m_client_plugin_unloading;

    /// <summary>
    /// Monitors when RDK client plugins are unloaded.
    /// </summary>
    public static event EventHandler RdkPlugInUnloading
    {
      add
      {
        if (m_client_plugin_unloading == null)
        {
          m_OnClientPlugInUnloading = OnClientPlugInUnloading;
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetClientPlugInUnloadingEventCallback(m_OnClientPlugInUnloading, Rhino.Runtime.HostUtils.m_rdk_ew_report);
        }
        m_renderer_changed += value;
      }
      remove
      {
        m_client_plugin_unloading -= value;
        if (m_client_plugin_unloading == null)
        {
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetClientPlugInUnloadingEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
          m_OnClientPlugInUnloading = null;
        }
      }
    }

    #endregion

    static UI.ToolbarFileCollection m_toolbar_files;
    /// <summary>
    /// Collection of currently open toolbar files in the application
    /// </summary>
    public static UI.ToolbarFileCollection ToolbarFiles
    {
      get { return m_toolbar_files ?? (m_toolbar_files = new Rhino.UI.ToolbarFileCollection()); }
    }

    /// <summary>
    /// Verifies that Rhino is running in full screen mode. 
    /// </summary>
    /// <returns>true if Rhino is running full screen, false otherwise.</returns>
    public static bool InFullScreen()
    {
      return UnsafeNativeMethods.CRhinoApp_InFullscreen();
    }

    /// <summary>
    /// Default font used to render user interface
    /// </summary>
    public static Font DefaultUiFont
    {
      get { return null; }
    }

    /// <summary>
    /// Verifies that Rhino is running on VMWare
    /// </summary>
    /// <returns>true if Rhino is running in Windows on VMWare, false otherwise</returns>
    public static bool RunningOnVMWare()
    {
      return UnsafeNativeMethods.Rh_RunningOnVMWare();
    }

    /// <summary>
    /// Find out if Rhino is running in a remote session
    /// </summary>
    /// <returns>true if Rhino is running in a RDP session, false otherwise</returns>
    public static bool RunningInRdp()
    {
      return UnsafeNativeMethods.Rh_RunningInRdp();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="formula"></param>
    /// <param name="obj"></param>
    /// <param name="topParentObject"></param>
    /// <returns></returns>
    public static string ParseTextField(string formula, RhinoObject obj, RhinoObject topParentObject )
    {
      if (topParentObject == null)
        topParentObject = obj;
      using (var sh = new Runtime.InteropWrappers.StringWrapper())
      {
        IntPtr ptr_string = sh.NonConstPointer;
        IntPtr ptr_obj = IntPtr.Zero;
        if (obj != null)
          ptr_obj = obj.ConstPointer();
        IntPtr ptr_parent_obj = IntPtr.Zero;
        if (topParentObject != null)
          ptr_parent_obj = topParentObject.ConstPointer();
        UnsafeNativeMethods.CRhinoApp_RhParseTextField(formula, ptr_string, ptr_obj, ptr_parent_obj);
        return sh.ToString();
      }
    }
  }
}

namespace Rhino.UI
{
  /// <summary>
  /// Contains static methods to control the mouse icon.
  /// </summary>
  public static class MouseCursor
  {
    /// <summary>
    /// Sets a cursor tooltip string shown next to the mouse cursor.
    /// Overrides all cursor tooltip panes.
    /// </summary>
    /// <param name="tooltip">The text to show.</param>
    public static void SetToolTip(string tooltip)
    {
      UnsafeNativeMethods.CRhinoApp_SetCursorTooltip(tooltip);
    }

    /// <summary>
    /// Retrieves the position of the mouse cursor, in screen coordinates
    /// </summary>
    public static Point2d Location
    {
      get
      {
        UnsafeNativeMethods.Point pt;
        UnsafeNativeMethods.GetCursorPos(out pt);
        return new Point2d(pt.X, pt.Y);
      }
    }
  }

  /// <summary>
  /// Contains static methods to control the application status bar.
  /// </summary>
  public static class StatusBar
  {
    /// <summary>
    /// Sets the distance pane to a distance value.
    /// </summary>
    /// <param name="distance">The distance value.</param>
    public static void SetDistancePane(double distance)
    {
      UnsafeNativeMethods.CRhinoApp_SetStatusBarDistancePane(distance);
    }

    /// <summary>
    /// Sets the number pane to a number value
    /// </summary>
    /// <param name="number"></param>
    public static void SetNumberPane(double number)
    {
      UnsafeNativeMethods.CRhinoApp_SetStatusBarNumberPane(number);
    }

    /// <summary>
    /// Sets the point pane to a point value.
    /// </summary>
    /// <param name="point">The point value.</param>
    public static void SetPointPane(Point3d point)
    {
      UnsafeNativeMethods.CRhinoApp_SetStatusBarPointPane(point);
    }

    /// <summary>
    /// Sets the message pane to a message.
    /// </summary>
    /// <param name="message">The message value.</param>
    public static void SetMessagePane(string message)
    {
      UnsafeNativeMethods.CRhinoApp_SetStatusBarMessagePane(message);
    }

    /// <summary>
    /// Removes the message from the message pane.
    /// </summary>
    public static void ClearMessagePane()
    {
      SetMessagePane(null);
    }

    /// <summary>
    /// Starts, or shows, Rhino's status bar progress meter.
    /// </summary>
    /// <param name="lowerLimit">The lower limit of the progress meter's range.</param>
    /// <param name="upperLimit">The upper limit of the progress meter's range.</param>
    /// <param name="label">The short description of the progress (e.g. "Calculating", "Meshing", etc)</param>
    /// <param name="embedLabel">
    /// If true, then the label will be embeded in the progress meter.
    /// If false, then the label will appear to the left of the progress meter.
    /// </param>
    /// <param name="showPercentComplete">
    /// If true, then the percent complete will appear in the progress meter.
    /// </param>
    /// <returns>
    /// 1 - The progress meter was created successfully.
    /// 0 - The progress meter was not created.
    /// -1 - The progress meter was not created because some other process has already created it.
    /// </returns>
    public static int ShowProgressMeter(int lowerLimit, int upperLimit, string label, bool embedLabel, bool showPercentComplete)
    {
      return ShowProgressMeter(0, lowerLimit, upperLimit, label, embedLabel, showPercentComplete);
    }

    /// <summary>
    /// Starts, or shows, Rhino's status bar progress meter.
    /// </summary>
    /// <param name="docSerialNumber">The document runtime serial number.</param>
    /// <param name="lowerLimit">The lower limit of the progress meter's range.</param>
    /// <param name="upperLimit">The upper limit of the progress meter's range.</param>
    /// <param name="label">The short description of the progress (e.g. "Calculating", "Meshing", etc)</param>
    /// <param name="embedLabel">
    /// If true, then the label will be embeded in the progress meter.
    /// If false, then the label will appear to the left of the progress meter.
    /// </param>
    /// <param name="showPercentComplete">
    /// If true, then the percent complete will appear in the progress meter.
    /// </param>
    /// <returns>
    /// 1 - The progress meter was created successfully.
    /// 0 - The progress meter was not created.
    /// -1 - The progress meter was not created because some other process has already created it.
    /// </returns>
    [CLSCompliant(false)]
    public static int ShowProgressMeter(uint docSerialNumber, int lowerLimit, int upperLimit, string label, bool embedLabel, bool showPercentComplete)
    {
      return UnsafeNativeMethods.CRhinoApp_StatusBarProgressMeterStart(docSerialNumber, lowerLimit, upperLimit, label, embedLabel, showPercentComplete);
    }

    /// <summary>
    /// Sets the current position of Rhino's status bar progress meter.
    /// </summary>
    /// <param name="position">The new value. This can be stated in absolute terms, or relative compared to the current position.
    /// <para>The interval bounds are specified when you first show the bar.</para></param>
    /// <param name="absolute">
    /// If true, then the progress meter is moved to position.
    /// If false, then the progress meter is moved position from the current position (relative).
    /// </param>
    /// <returns>
    /// The previous position if successful.
    /// </returns>
    public static int UpdateProgressMeter(int position, bool absolute)
    {
      return UpdateProgressMeter(0, position, absolute);
    }

    /// <summary>
    /// Sets the current position of Rhino's status bar progress meter.
    /// </summary>
    /// <param name="docSerialNumber">The document runtime serial number.</param>
    /// <param name="position">The new value. This can be stated in absolute terms, or relative compared to the current position.
    /// <para>The interval bounds are specified when you first show the bar.</para></param>
    /// <param name="absolute">
    /// If true, then the progress meter is moved to position.
    /// If false, then the progress meter is moved position from the current position (relative).
    /// </param>
    /// <returns>
    /// The previous position if successful.
    /// </returns>
    [CLSCompliant(false)]
    public static int UpdateProgressMeter(uint docSerialNumber, int position, bool absolute)
    {
      return UnsafeNativeMethods.CRhinoApp_StatusBarProgressMeterPos(docSerialNumber, position, absolute);
    }

    /// <summary>
    /// Ends, or hides, Rhino's status bar progress meter.
    /// </summary>
    public static void HideProgressMeter()
    {
      HideProgressMeter(0);
    }

    /// <summary>
    /// Ends, or hides, Rhino's status bar progress meter.
    /// </summary>
    /// <param name="docSerialNumber">The document runtime serial number.</param>
    [CLSCompliant(false)]
    public static void HideProgressMeter(uint docSerialNumber)
    {
      UnsafeNativeMethods.CRhinoApp_StatusBarProgressMeterEnd(docSerialNumber);
    }

  }
}
#endif