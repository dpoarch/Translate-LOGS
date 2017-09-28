using System;
using System.Collections.Generic;
using System.Text;

namespace SpencerGifts.Translate
{
  public class RuleConditionException : TranslateException
  {
    private string _ConditionExpression;
    private string _ConditionID;
    private string _ConditionDestination;
    public string ConditionDestination
    {
      get
      {
        return _ConditionDestination;
      }
      set
      {
        _ConditionDestination = value;
      }
    }
    public string ConditionID
    {
      get
      {
        return _ConditionID;
      }
      set
      {
        _ConditionID = value;
      }
    }
    public string ConditionExpression
    {
      get
      {
        return _ConditionExpression;
      }
      set
      {
        _ConditionExpression = value;
      }
    }
    public RuleConditionException()
      : base()
    {
    }

    public RuleConditionException(string message)
      : base(message)
    {
    }

    public RuleConditionException(string message, Exception innerexception)
      : base(message, innerexception)
    {
    }
  }
}
