using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace SpencerGifts.Translate
{

  public sealed class RuleValue
  {
    bool? _isExpression;
    string _ExporessionSource;
    bool? _UseDefault;
    bool? _ContunueProcessing;
    string _Value;
    string _Actionid;
    bool? _ContinueTransaction;

    public bool isExpression
    {
      get {
        if (!_isExpression.HasValue)
          return false;

        return _isExpression.Value; }
      set { _isExpression = value; }
    }

    public bool? ContinueTransaction
    {
      get
      {
        if (!_ContinueTransaction.HasValue)
          return true;

        return _ContinueTransaction;
      }
      set
      {
        if (_ContinueTransaction == value)
          return;
        _ContinueTransaction = value;
      }
    }
    public string ExpressionSource
    {
      get { return _ExporessionSource; }
      set { _ExporessionSource = value; }
    }

    public bool ContunueProcessing
    {
      get {
        if (!_ContunueProcessing.HasValue)
          return true;

        return _ContunueProcessing.Value; }
      set { _ContunueProcessing = value; }
    }

    public bool UseDefault
    {
      get {
        if (!_UseDefault.HasValue)
          return false;

        return _UseDefault.Value; }
      set { _UseDefault = value; }
    }
    public string Value
    {
      get { return _Value; }
      set { _Value = value; }
    }

    public string Actionid
    {
      get { return _Actionid; }
      set { _Actionid = value; }
    }

  }

}
