using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Xml.XPath;

namespace SpencerGifts.Translate.Plugin.TLog
{
  sealed internal class ExcludeFileHelper
  {
    static XmlDocument ExcludeStoresDoc;
    string _ExcludeFile;
    string _SavePath;
    string[] _BeforeTransTypes;
    private string _NonTranslateSavePath;
    private static readonly object ExlcudeLock = new object();

    public ExcludeFileHelper(string ExcludeFile, string SavePath, string[] BeforeTransTypes)
    {
      _ExcludeFile = ExcludeFile;
      _SavePath = SavePath;
      _BeforeTransTypes = BeforeTransTypes;

      lock (ExlcudeLock)
      {
        if (ExcludeStoresDoc == null)
        {
          ExcludeStoresDoc = new XmlDocument();
          LoadExcludeStores();
        }
      }
    }
    
    public string NonTranslateSavePath
    {
      get
      {
        return _NonTranslateSavePath;
      }
      set
      {
        _NonTranslateSavePath = value;
      }
    }

    private void LoadExcludeStores()
    {      
      XmlDeclaration xmlDeclaration = ExcludeStoresDoc.CreateXmlDeclaration("1.0", "utf-8", null);
      XmlElement rootNode = ExcludeStoresDoc.CreateElement("ExcludeStoreList");
      ExcludeStoresDoc.InsertBefore(xmlDeclaration, ExcludeStoresDoc.DocumentElement);
      ExcludeStoresDoc.AppendChild(rootNode);

      using (FileStream stream = new FileStream(_ExcludeFile, FileMode.Open))
      {
        using (StreamReader reader = new StreamReader(stream))
        {
          while (reader.Peek() != -1)
          {
            XmlElement parentNode = ExcludeStoresDoc.CreateElement("ExcludeRecord");

            string CurrentLine = reader.ReadLine();
            if (CurrentLine.StartsWith(";") || String.IsNullOrEmpty(CurrentLine.Trim()))
              continue;

            string[] exceptString = CurrentLine.Split(' ');
            if (exceptString[0] == "T")
              continue;

            XmlElement StoreNode = ExcludeStoresDoc.CreateElement("StoreNum");
            XmlElement RecortTypeNode = ExcludeStoresDoc.CreateElement("RecordType");
            XmlElement DestinationNode = ExcludeStoresDoc.CreateElement("SavePath");
            StoreNode.InnerText = exceptString[1];
            RecortTypeNode.InnerText = exceptString[0];
            DestinationNode.InnerText = String.Format("{0}\\Exclude{1}", _SavePath, exceptString[0]);
            parentNode.AppendChild(StoreNode);
            parentNode.AppendChild(RecortTypeNode);
            parentNode.AppendChild(DestinationNode);
            rootNode.AppendChild(parentNode);
          }
        }
      }
    }

    /// <summary>
    /// Used to determine if the current store should be translated.  This is driven off the exceptions file
    /// </summary>
    /// <param name="StoreNum"></param>
    /// <returns></returns>
    public bool ShouldTranslate(string StoreNum)
    {
      //while (ExcludeStoresDoc == null || _BeforeTransTypes == null)
        //System.Threading.Thread.Sleep(100);

      XPathNavigator nav = ExcludeStoresDoc.DocumentElement.CreateNavigator();
      //Most stores will not be in this file so speed things up by checking to see if it is even there
      int recordcount = Convert.ToInt32(nav.Evaluate(String.Format("count(//ExcludeRecord[StoreNum={0}])", StoreNum)));
      if (recordcount == 0)
        return true;
      
      foreach (string s in _BeforeTransTypes)
      {
        if (Convert.ToBoolean(nav.Evaluate(String.Format("count(ExcludeRecord[StoreNum={0}][RecordType='{1}']) > 0", StoreNum, s))))
        {
          _NonTranslateSavePath = nav.Evaluate(String.Format("string(ExcludeRecord[StoreNum={0}][RecordType='{1}']/SavePath)",StoreNum,s)).ToString();
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// Returns value indicating if it is ok to backup the tlog to its default location or not.  This is driven
    /// off of the exceptions file
    /// </summary>
    /// <param name="StoreNum"></param>
    /// <returns></returns>
    public bool OkToBackup(string StoreNum)
    {
      XPathNavigator nav = ExcludeStoresDoc.DocumentElement.CreateNavigator();
      XmlNodeList list = ExcludeStoresDoc.SelectNodes(String.Format("//ExcludeRecord[StoreNum={0}]",StoreNum));
      if (list.Count == 0)
        return true;

      List<string> BeforeTransLst = new List<string>();
      BeforeTransLst.AddRange(_BeforeTransTypes);
      
      for (int i = 0; i < list.Count; i++)
      {
        XmlNode currentNode = list[i];
        if (BeforeTransLst.Contains(currentNode.SelectSingleNode("RecordType").InnerText.ToString()))
          continue;
        else
        {
          _NonTranslateSavePath = currentNode.SelectSingleNode("SavePath").InnerText;
          return false;
        }
      }


      return true;
    }


  }
}
