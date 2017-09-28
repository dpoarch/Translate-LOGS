using System;
using System.Collections.Generic;
using System.Text;

namespace SpencerGifts.Translate
{
  public class RuleMappingException : TranslateException
  {
    private string _MappingSourcePath;
    private string _MappdingDestinationPath;
    public string MappdingDestinationPath
    {
      get
      {
        return _MappdingDestinationPath;
      }
      set
      {
        _MappdingDestinationPath = value;
      }
    }
    public string MappingSourcePath
    {
      get
      {
        return _MappingSourcePath;
      }
      set
      {
        _MappingSourcePath = value;
      }
    }

    public RuleMappingException()
      : base()
    {
    }

    public RuleMappingException(string message)
      : base(message)
    {
    }

    public RuleMappingException(string message, Exception innerexception)
      : base(message, innerexception)
    {
    }
  }
}
