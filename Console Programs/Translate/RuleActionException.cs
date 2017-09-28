using System;
using System.Collections.Generic;
using System.Text;

namespace SpencerGifts.Translate
{
  public class RuleActionException : TranslateException
  {
    public RuleActionException()
      : base()
    {
    }

    public RuleActionException(string message)
      : base(message)
    {
    }

    public RuleActionException(string message, Exception innerexception)
      : base(message, innerexception)
    {
    }

    private string _AcctionID;
    public string AcctionID
    {
      get
      {
        return _AcctionID;
      }
      set
      {
        _AcctionID = value;
      }
    }
    private string _ExpressionSource;
    public string ExpressionSource
    {
      get
      {
        return _ExpressionSource;
      }
      set
      {
        _ExpressionSource = value;
      }
    }
  }
}
