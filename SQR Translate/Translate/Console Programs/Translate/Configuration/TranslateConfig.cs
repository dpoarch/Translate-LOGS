using System;
using System.Collections.Specialized;
using System.Configuration;

namespace SpencerGifts.Translate.Configuration
{
  /// <summary>
  /// Represents the configuration settings for a running plugin.
  /// </summary>
  public sealed class TranslateConfig
  {
    #region Local Variables
    private string _RulesFile;
    private string _SourceFileLocation;
    private string _DestinationTemplate;
    private string _SourceDestination;
    private string _NewDocumentRootNode;
    private string _TranslatedFileLocation;
    private string _SourceTemplate;
    private string _SourceFile;
    private string _ExecutionName;
    private string _PluginName;
    private string _LoggingProvider;
    private NameValueCollection _ConfigCollection;
    #endregion

    public TranslateConfig(string PluginName)
    {
      LoadConfig(PluginName);
    }

    /// <summary>
    /// Gets the rules file for the plugin
    /// </summary>
    public string RulesFile
    {
      get { return _RulesFile;}
    }

    /// <summary>
    /// Gets the source file location of the file to be translated 
    /// </summary>
    public string SourceFileLocation
    {
      get { return _SourceFileLocation; }
    }

    /// <summary>
    ///Gets the template file used to create the output file
    /// </summary>
    public string DestinationTemplate
    {
      get { return _DestinationTemplate; }
    }

    /// <summary>
    /// Gets the template file of the source file to be translated 
    /// </summary>
    public string SourceTemplate
    {
      get { return _SourceTemplate; }
    }

    public string SourceDestination
    {
      get { return _SourceDestination; }
    }

    /// <summary>
    /// Gets the document root of the translated file
    /// </summary>
    public string NewDocumentRootNode
    {
      get { return _NewDocumentRootNode; }
    }

    /// <summary>
    /// Gets the location of where the translated file is to be saved
    /// </summary>
    public string TranslatedFileLocation
    {
      get { return _TranslatedFileLocation; }
    }

    /// <summary>
    /// Gets the name of the file to be translated 
    /// </summary>
    public string SourceFile
    {
      get { return _SourceFile; }
    }

    /// <summary>
    /// Gets the full assembly name and class name of the plugin to load and run
    /// </summary>
    public string ExecutionName
    {
      get
      {
        return _ExecutionName;
      }
      set
      {
        if (_ExecutionName == value)
          return;
        _ExecutionName = value;
      }
    }

    public string PluginName
    {
      get { return _PluginName; }
    }

    public string LoggingProvider
    {
      get { return _LoggingProvider; }
    }

    /// <summary>
    /// Loads the current configuration
    /// </summary>
    /// <param name="PluginName"></param>
    /// <exception cref="System.Configuration.ConfigurationErrorsException"></exception>
    private void LoadConfig(string PluginName)
    {
      _PluginName = PluginName;
      object objPlugin = System.Configuration.ConfigurationManager.GetSection("TranslatePlugins/" + PluginName);

      _ConfigCollection = objPlugin as NameValueCollection;
      if (objPlugin == null)
        throw new System.Configuration.ConfigurationErrorsException("Unable to load " + PluginName + " section");

      _RulesFile = GetConfigValue("RulesFile");
      _SourceFileLocation = GetConfigValue("SourceFileLocation");
      _DestinationTemplate = GetConfigValue("DestinationTemplate");
      _SourceDestination = GetConfigValue("SourceDestination");
      _NewDocumentRootNode = GetConfigValue("NewDocumentRootNode");
      _TranslatedFileLocation = GetConfigValue("TranslatedFileLocation");
      _SourceTemplate = GetConfigValue("SourceTemplate");
      _SourceFile = GetConfigValue("SourceFile");
      _ExecutionName = GetConfigValue("ExecutionName");
      _LoggingProvider = GetConfigValue("LoggingProvider");

    }

    /// <summary>
    /// Gets a userdefined config value form the plugin config section
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private string GetConfigValue(string name)
    {
      return String.IsNullOrEmpty(_ConfigCollection[name]) ? String.Empty : _ConfigCollection[name].ToString();
    }

    /// <summary>
    /// Gets a configuration value from the current plugin
    /// </summary>
    /// <param name="key">The config value</param>
    /// <returns></returns>
    public string GetValue(string key)
    {
      return GetConfigValue(key);
    }

  }
}
