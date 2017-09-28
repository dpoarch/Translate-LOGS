using System;

namespace SpencerGifts.TlogCommon
{
  class ValidateException : Exception
  {

    private string _TransactionNumber;    
    private string _LineNumber;
    private string _ExpectedValue;
    private string _ActualValue;

    public string ExpectedValue
    {
      get
      {
        return _ExpectedValue;
      }
      set
      {
        if (_ExpectedValue == value)
          return;
        _ExpectedValue = value;
      }
    }
    public string ActualValue
    {
      get
      {
        return _ActualValue;
      }
      set
      {
        if (_ActualValue == value)
          return;
        _ActualValue = value;
      }
    }
    public string TransactionNumber
    {
      get
      {
        return _TransactionNumber;
      }
      set
      {
        _TransactionNumber = value;
      }
    }    
    
    public string LineNumber
    {
      get
      {
        return _LineNumber;
      }
      set
      {
        _LineNumber = value;
      }
    }

    public ValidateException()
      : base()
    {
    }

    public ValidateException(string message)
      : base(message)
    {

    }

    public ValidateException(string message, Exception innerException) : base(message,innerException)
    {
    }


  }
}
