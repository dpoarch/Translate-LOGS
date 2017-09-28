using System;
using System.Collections.Generic;
using System.Text;

namespace SpencerGifts.Translate.Plugin.TLog.IOrders
{
  internal sealed class HeaderLineItem
  {

    Int32 _StoreNo;
    DateTime _TransactionDate;
    string _LineData;
       
    public Int32 StoreNo
    {
      get
      {
        return _StoreNo;
      }
      set
      {
        if (_StoreNo == value)
          return;
        _StoreNo = value;
      }
    }
    public DateTime TransactionDate
    {
      get
      {
        return _TransactionDate;
      }
      set
      {
        if (_TransactionDate == value)
          return;
        _TransactionDate = value;
      }
    }
    public string LineData
    {
      get
      {
        return _LineData;
      }
      set
      {
        if (_LineData == value)
          return;
        _LineData = value;
      }
    }
  }
}
