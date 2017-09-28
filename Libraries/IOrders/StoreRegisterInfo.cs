using System;
using System.Xml;

namespace SpencerGifts.Translate.Plugin.TLog.IOrders
{
  internal sealed class StoreRegisterInfo
  {
    /// <summary>
    /// The store number the current configuration is using
    /// </summary>
    int _StoreNo;    
    /// <summary>
    /// The document holding the configuration
    /// </summary>
    XmlDocument doc = new XmlDocument();
    /// <summary>
    /// Holds the next transaction number
    /// </summary>
    int _NextTransactionID;
    /// <summary>
    /// Holds the location to the configuration file
    /// </summary>
    string _StoreFile;
    /// <summary>
    /// Holds the current register number
    /// </summary>
    int _CurrentRegisterNumber;
    /// <summary>
    /// The max transaction number
    /// </summary>
    const int TotalTransactionLimit = 9999;
    /// <summary>
    /// Holds the total amount of transactions used
    /// </summary>
    int _totalTransactions;
    XmlNode StoreNode;
    

    public StoreRegisterInfo(int StoreNo, string StoreFile)
    {
      _StoreNo = StoreNo;
      doc.Load(StoreFile);
      _StoreFile = StoreFile;
      StoreNode = doc.DocumentElement.SelectSingleNode("Store[@Number='" + _StoreNo + "']");
      _NextTransactionID = -1;
      _CurrentRegisterNumber = -1;
      _totalTransactions = 0;

    }

    /// <summary>
    /// Gets the default register number to use for this store
    /// </summary>
    public int DefaultRegister
    {
      get{return Int32.Parse(StoreNode.Attributes["DefaultRegisterNumber"].Value);}
    }

    /// <summary>
    /// Gets the current register number
    /// </summary>
    public int CurrentRegisterNumber
    {
      get
      {
        if (_CurrentRegisterNumber == -1)
          _CurrentRegisterNumber = DefaultRegister;
        return _CurrentRegisterNumber;
      }
    }

    /// <summary>
    /// Gets the Country Code to use for this store
    /// </summary>
    public int CountryCode
    {
      get{return Int32.Parse(StoreNode.Attributes["CountryCode"].Value);}
    }
    
    /// <summary>
    /// Gets the starting transaction number to use for this store
    /// </summary>
    public int StartTransactionID
    {
      get { return Int32.Parse(StoreNode.SelectSingleNode("Register[@Number='" + CurrentRegisterNumber.ToString() + "']/LastTransactionID").InnerXml); }
    }

    /// <summary>
    /// Gets the next available register number to use
    /// </summary>
    public int NextRegisterNumber
    {
      get
      {
        XmlNode node = StoreNode.SelectSingleNode("Register[@Number='" + CurrentRegisterNumber.ToString() + "']").NextSibling;
        if (node != null)
          return Int32.Parse(node.Attributes["Number"].Value);
        else
          return DefaultRegister;
      }
    }

    /// <summary>
    /// The current store number whose configuration we are reading
    /// </summary>
    public int StoreNo
    {
      get { return _StoreNo; }
    }

    /// <summary>
    /// Gets the next transaction number to use
    /// </summary>
    public int NextTransactionID
    {
      get
      {
        if (_totalTransactions == TotalTransactionLimit)
          SetCurrentRegister(NextRegisterNumber);

        _totalTransactions++;

        if (_NextTransactionID == -1)
          _NextTransactionID = StartTransactionID;
        else
        {
          //Reset the id back to 0 if we have readched the max number size allowed
          if (_NextTransactionID == TotalTransactionLimit)
            _NextTransactionID = 0;

          _NextTransactionID++;
        }
        return _NextTransactionID;
      }
    }

    /// <summary>
    /// Change the current register number for this configuration
    /// </summary>
    /// <param name="RegisterNo"></param>
    public void SetCurrentRegister(int RegisterNo)
    {
      SaveTransactionInfo();
      _NextTransactionID = -1;
      _totalTransactions = 0;
      _CurrentRegisterNumber = RegisterNo;
    }

    /// <summary>
    /// Saves the current settings 
    /// </summary>
    public void SaveTransactionInfo()
    {
      StoreNode.SelectSingleNode("Register[@Number='" + DefaultRegister.ToString() + "']/LastTransactionID").InnerXml = _NextTransactionID.ToString();
      doc.Save(_StoreFile);
    }
  }
}
