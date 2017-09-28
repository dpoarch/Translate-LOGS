using System;
using System.Resources;

namespace SpencerGifts.Translate.Plugin.TLog
{
  sealed internal class ResourceHelper
  {
    private static ResourceHelper _helper;
    static readonly object padlock = new object();

    public static ResourceHelper Instance
    {
      get {
        if (_helper == null)
        {
          lock (padlock)
          {
            if (_helper == null)
              _helper = new ResourceHelper();
          }
        }
        return _helper;
      }
    }

    public string GetString(string ResourceFile, string ResoucreString)
    {
      ResourceManager LocRM = new ResourceManager(ResourceFile, this.GetType().Assembly);
      return LocRM.GetString(ResoucreString);
    }
  }
}
