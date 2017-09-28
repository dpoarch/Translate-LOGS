using System;
using System.Reflection;
using System.Configuration;
using SpencerGifts.Translate.Configuration;
using System.Diagnostics;
using SpencerGifts.Common.Logging;
using System.Collections;

namespace SpencerGifts.Translate
{
  class Program
  {
    private static ArrayList _PluningsToRun;
    static void Main(string[] args)
    {
      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
      try
      {
        
        ExecuteTranslate(args);
      }
      catch (Exception ex)
      {
        Logger log = Logger.Create();
        if (log == null)
        {
          Console.WriteLine(ex.Message);
          Environment.ExitCode = 1;

        }
        log.LogMessage("Unhandled exception during translate " + ex.Message + "\r\n" + ex.StackTrace);
        Environment.ExitCode = 1;
      }
    }

    private static void ExecuteTranslate(string[] args)
    {
       //Make sure the translate is not already runnin
      if (!isAlreadyRunning(args))
      {
        //Populate the plugins passed in that need to be run
        PopulatePlugins(args);

        //Load the current configuration
        System.Configuration.Configuration Appconfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        // Get the collection of the translate plugin groups.
        ConfigurationSectionGroup PluginConfiguration = Appconfig.GetSectionGroup("TranslatePlugins");

        //Load the plugins and execute them
        for (int i = 0; i < PluginConfiguration.Sections.Count; i++)
        {
          
          System.Configuration.ConfigurationSection TransConfigSection = PluginConfiguration.Sections[i];

          TranslateConfig transConfig = new TranslateConfig(TransConfigSection.SectionInformation.Name);
          string[] AssemInfo = transConfig.ExecutionName.Split(',');
          string CurrentPlugin = AssemInfo[0].Trim();
          string PluginFullName = AssemInfo[1].Trim();

          //Check to see if we need to execute this plugin 
          string Config = CurrentPlugin;
          if (!_PluningsToRun.Contains(CurrentPlugin.ToLower()))
            if (!_PluningsToRun.Contains(TransConfigSection.SectionInformation.Name.ToLower()))
              continue;
            else
              Config = TransConfigSection.SectionInformation.Name;

          //Load the assembly
          Assembly assembly = Assembly.Load(CurrentPlugin);
          Type ClassType = assembly.GetType(PluginFullName);
          object transItem;
          try
          {
           transItem = Activator.CreateInstance(ClassType);
          }
          catch (Exception ex)
          {
            Logger log = Logger.Create();
            log.LogMessage("Unhandled exception while trying to create an instance of " + PluginFullName + "\r\n" + ex.StackTrace);
            continue;
          }
           
          //Check to make sure the plugin is the correct type
          if (transItem is TranslateItem)
          {
            TranslateItem transPlugin = (TranslateItem)transItem;
            //Execute the translate for this plugin 
            try
            {
              transPlugin.ExecuteTranslate(Config);
            }
            catch (Exception ex)
            {
              Logger log = Logger.Create();
              log.LogMessage("Unhandled exception during translate " + ex.Message + "\r\n" + ex.StackTrace);
            }

          }
          else
          {
            Logger log = Logger.Create();
            log.LogMessage("Invalid Plugin Loaded. Plugin needs to be of type " + typeof(TranslateItem).FullName);
          }

        }
      }
      else
      {
        Logger log = Logger.Create();
        log.LogMessage("Translate Already Running.");
        log.LogMessage("Exiting.");
        System.Threading.Thread.Sleep(5000);
      }
    }

    static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      Exception exc = (Exception)e.ExceptionObject;
      Logger log = Logger.Create();
      log.LogMessage("Unhandled Exception during translate: " + exc.Message);
    }

    /// <summary>
    /// .used to see if the current program is already running.
    /// </summary>
    /// <returns></returns>
    private static bool isAlreadyRunning(string[] args)
    {
      if (args != null && args.Length > 0)
      {
        for (int i = 0; i < args.Length; i++)
        {
          if (args[i].ToLower() == "bypassrunningcheck")
            return false;

        }
      }

      Process oProcess = Process.GetCurrentProcess();
      string oProcName = oProcess.ProcessName;

      bool isRunning = Process.GetProcessesByName(oProcName).Length > 1;
      return isRunning;
    }

    private static void PopulatePlugins(string[] args)
    {
      if (_PluningsToRun == null)
        _PluningsToRun = new ArrayList();

      if (args.Length > 0)
        for (int i = 0; i < args.Length; i++)
        {
          if (args[i].ToLower() == "bypassrunningcheck")
            continue;

          _PluningsToRun.Add(args[i].ToLower());
        }
    }
  }
}

