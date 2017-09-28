using System;
using System.Collections.Generic;
using System.Text;

namespace SpencerGifts.Translate
{
  public abstract class TranslateException : Exception
  {
    private string _RuleID;
    private string _SourceFile;
    public string SourceFile
    {
      get
      {
        return _SourceFile;
      }
      set
      {
        _SourceFile = value;
      }
    }
    public string RuleID
    {
      get
      {
        return _RuleID;
      }
      set
      {
        _RuleID = value;
      }
    }

    public TranslateException(): base()
    {
    }

    public TranslateException(string message)
      : base(message)
    {
    }

    public TranslateException(string message, Exception innerexception)
      : base(message, innerexception)
    {
    }

  }

}
