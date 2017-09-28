using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;


namespace SpencerGifts.Translate
{
  public sealed class RuleCondition
  {
    private string _Expression;
    private string _Destination;

    private RuleValue _trueValue;
    private RuleValue _falseValue;
    string _format;
    string _id;
    string _name;
    Type _formatType;

    public string format
    {
      get { return _format; }
      set
      {
        if (_format == value)
          return;
        _format = value;
      }
    }

    public string ID
    {
      get
      {
        return _id;
      }
      set
      {
        if (_id == value)
          return;
        _id = value;
      }
    }
    public string Name
    {
      get
      {
        return _name;
      }
      set
      {
        if (_name == value)
          return;
        _name = value;
      }
    }
    public Type type
    {
      get { return _formatType; }
      set
      {
        if (_formatType == value)
          return;
        _formatType = value;
      }
    }

    public string Expression
    {
      get { return _Expression; }
      set { _Expression = value; }
    }

    public string Destination
    {
      get { return _Destination; }
      set { _Destination = value; }
    }

    public RuleValue TrueValue
    {
      get
      {

        if (_trueValue == null)
          _trueValue = new RuleValue();
        return _trueValue;
      }
      set { _trueValue = value; }

    }

    public RuleValue FalseValue
    {
      get
      {

        if (_falseValue == null)
          _falseValue = new RuleValue();
        return _falseValue;
      }
      set { _falseValue = value; }

    }

    public string Evaluate()
    {
      return "";
    }
  }
  public enum ExpressionSource
  {
    SourceFile,
    DestinationFile
  }
}

