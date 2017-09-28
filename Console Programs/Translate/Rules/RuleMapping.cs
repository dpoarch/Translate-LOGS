using System;
using System.Collections.Generic;
using System.Text;

namespace SpencerGifts.Translate
{
  public sealed class RuleMapping
  {
    string _SourcePath;
    string _DestinationPath;
    string _format;
    Type _formatType;

    public string SourcePath
    {
      get {return _SourcePath; }
      set {_SourcePath = value; }
    }

    public string DestinationPath
    {
      get { return _DestinationPath; }
      set { _DestinationPath = value; }
    }

    public string Format
    {
      get {return _format; }
      set { _format = value; }
    }

    public Type FormatType
    {
      get { return _formatType; }
      set { _formatType = value; }
    }


  }
}
