using System;
using System.Collections.Specialized;

namespace SpencerGifts.Translate.Plugin.TLog
{
  sealed internal class CountryConfig
  {
    private string _CountryName;
    private string _SaveDirectoryPrefix;
    private string _SaveDirectoryLocation;
    private string _TranslatedFilePrefix;

    public string CountryName
    {
    	get
    	{
    		return _CountryName;
    	}
    }
    public string SaveDirectoryPrefix
    {
    	get
    	{
    		return _SaveDirectoryPrefix;
    	}
    }
    public string SaveDirectoryLocation
    {
    	get
    	{
    		return _SaveDirectoryLocation;
    	}
    }
    public string TranslatedFilePrefix
    {
    	get
    	{
    		return _TranslatedFilePrefix;
    	}
    }
    /// <summary>
    /// Gets the specified country config
    /// </summary>
    /// <param name="CountryCode"></param>
    /// <exception cref="System.Configuration.ConfigurationErrorsException"></exception>
    public CountryConfig(string CountryCode)
    {
      object objCountryConfig = System.Configuration.ConfigurationManager.GetSection("CountryConfig/Country_" + CountryCode);
      //object objCountryConfig = SpencerGifts.Translate.Configuration.ConfigManager.CurrentConfig.GetSection("CountryConfig/Country_" + CountryCode);
      NameValueCollection col = objCountryConfig as NameValueCollection;
      if (objCountryConfig == null)
        throw new System.Configuration.ConfigurationErrorsException("Unable to load Country Config Section " + CountryCode);

      _CountryName = col["Country"];
      _SaveDirectoryLocation = col["DirectoryLocation"];
      _SaveDirectoryPrefix = col["Directory_Prefix"];
      _TranslatedFilePrefix = col["TranslatedFilePrefix"];
    }
  }


}
