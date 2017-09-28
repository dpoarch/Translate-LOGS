using System;
using System.Collections.Generic;
using System.Text;

namespace SpencerGifts.Translate
{
  public sealed class RuleAttribute
  {
    string _ID;
    string _Value;
    bool _UseBuiltInIdentity;
    bool _isExpression;
    string _ExpressionSource;
    string _format;
    Type _formatType;

    public string format
    {
      get
      {
        return _format;
      }
      set
      {
        if (_format == value)
          return;
        _format = value;
      }
    }
    public Type formatType
    {
      get
      {
        return _formatType;
      }
      set
      {
        if (_formatType == value)
          return;
        _formatType = value;
      }
    }

    public string ID
    {
      get { return _ID;}
      set { _ID = value; }
    }

    public string Value
    {
      get { return _Value; }
      set { _Value = value; }
    }

    public bool UseBuiltInIdentity
    {
      get { return _UseBuiltInIdentity;}
      set { _UseBuiltInIdentity = value;}
    }

    public bool IsExpression
    {
      get{return _isExpression;}
      set {_isExpression = value;}
    }

    public string ExpressionSrouce
    {
      get { return _ExpressionSource; }
      set { _ExpressionSource = value; }
    }
  }
}
