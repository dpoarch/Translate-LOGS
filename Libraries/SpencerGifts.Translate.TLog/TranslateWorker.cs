using System;
using System.Collections.Generic;
using System.Text;
using BlackHen.Threading;

namespace SpencerGifts.Translate.Plugin.TLog
{

  /// <summary>
  /// Class used to pass the plugin into the thread pool to be translated
  /// </summary>
  sealed internal class TranslateWorker : WorkItem 
  {
    private SpencerGifts.Translate.TranslateItem _TransItem;
    private string _PluginConfigName;    
    ExceptionLogHandler _ExceptHandler;  
    public delegate void ExceptionLogHandler(System.Reflection.PropertyInfo[] info, object Exception);

    public TranslateWorker(ExceptionLogHandler ExceptionLogHandler)
    {
      _ExceptHandler = ExceptionLogHandler;
    }

    public SpencerGifts.Translate.TranslateItem TransItem
    {
      get
      {
        return _TransItem;
      }
      set
      {
        _TransItem = value;
      }
    }
    
    public string PluginConfigName
    {
      get
      {
        return _PluginConfigName;
      }
      set
      {
        _PluginConfigName = value;
      }
    }
    
    public override void Perform()
    {
      try
      {
        TransItem.ExecuteTranslate(_PluginConfigName);      
      }
      catch (Exception ex)
      {
        string BadTlogLocation = TransItem.PluginConfig.GetValue("BadTlogLocation");
        System.IO.FileInfo file = new System.IO.FileInfo(TransItem.SourceFile);
        System.IO.File.Move(TransItem.SourceFile, BadTlogLocation + "\\" + file.Name);

        if (_ExceptHandler != null)        
          _ExceptHandler.Invoke(ex.GetType().GetProperties(), ex);                          
      }
      
    }

  }
}
