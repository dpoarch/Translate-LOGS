using System;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml.XPath;
using System.Text;

namespace SpencerGifts.TlogCommon
{
  public sealed class Xml
  {
    XmlDocument RulesDoc = null;
    XmlDocument tLogTransaction = null;
    static string SaveXmlTlogConfig;
		static string XMLTLogSavePath;

    /// <summary>
    /// Convert the tab delimted TLog file to an XML formatted file for processing.
    /// </summary>
    /// <param name="sFile">The TLog File To Convert</param>
    /// <param name="sTemplate">The template file used to populate the new xml document</param>
    /// <returns>Xml representation of the file</returns>
    public XmlTextReader GetTLogToXMReader(string sFile, string sTemplate)
    {
      return GetTLogToXMReader(sFile, sTemplate, null);
    }

    /// <summary>
    /// Convert the tab delimted TLog file to an XML formatted file for processing.
    /// </summary>
    /// <param name="sFile">The TLog File To Convert</param>
    /// <param name="sTemplate">The template file used to populate the new xml document</param>
    /// <returns>Xml representation of the file</returns>
    public XmlTextReader GetTLogToXMReader(string sFile, string sTemplate, string RulesFile)
    {
#if (DEBUG)
      Console.WriteLine("Start Conversion To Xml Reader " + DateTime.Now.ToString("hh:mm:ss"));
#endif

      string RowQualifier = "";

      if (!String.IsNullOrEmpty(RulesFile))
      {
        if (RulesDoc == null)
        {
          using (TextReader inputStream = new StreamReader(RulesFile))
          {
            RulesDoc = new XmlDocument();
            RulesDoc.Load(inputStream);
          }
        }
      }


      if (tLogTransaction == null)
      {

        tLogTransaction = new XmlDocument();
        tLogTransaction.Load(sTemplate);

      }

      RowQualifier = tLogTransaction.SelectSingleNode("TrovatoTLog/RowQualifier").InnerText;

      string row = "";

      XmlDocument xmlTlogDoc = new XmlDocument();
      MemoryStream stmMemory = new MemoryStream();
      XmlWriterSettings settings = new XmlWriterSettings();
      settings.CheckCharacters = false;


      XmlWriter oXTW = XmlWriter.Create(stmMemory, settings);

      oXTW.WriteStartDocument();
      oXTW.WriteStartElement("StoreTransactions");

      int TranCount = 0;
      //System.Threading.Thread.Sleep(10);
      using (StreamReader sr = new StreamReader(File.OpenRead(sFile), Encoding.Default))
      {
        string[] strNewCols;
        int rowcount = 0;
        //Start looping through the rows we loaded into the datatable from the Tlog
        while (sr.Peek() != -1)
        {
          strNewCols = sr.ReadLine().Split("\t".ToCharArray());
          try
          {
            row = RowQualifier + strNewCols[0].ToString();
          }
          catch
          {
            //We had a problem converting the current row to a valid type
            //usually this will mean the tlog spec changed and a new row has been added.
            //For now we assume it doesnt matter until we need the new row.
            row = "NA";
          }

          //Start New Transaction
          if (row == "L10")
          {
            oXTW.WriteStartElement("Transaction");
            oXTW.WriteAttributeString("TransNumber", TranCount.ToString());

            XmlElement rowElement = tLogTransaction["TrovatoTLog"][row];
            PopulateRow(rowElement, strNewCols, oXTW, rowcount);
            rowElement = null;

            //Transaction started now advance to the next line and search for the email records
            bool isTransactionEnd = false;

            while (!isTransactionEnd)
            {
              //if (row == "L44")
              //{
              //  string s = "";
              //}
              //Keep looping until we come to the end of the transaction
              rowcount++;

              strNewCols = sr.ReadLine().Split("\t".ToCharArray());
              row = RowQualifier + strNewCols[0].ToString();

              rowElement = tLogTransaction["TrovatoTLog"][row];
              PopulateRow(rowElement, strNewCols, oXTW, rowcount);

              //Transaction has ended move on to the next
              if (row == "L99")
              {
                TranCount++;
                isTransactionEnd = true;
                break;
              }
            }
            oXTW.WriteEndElement();
          }
          else
          {
            //We will get here if we have an imcomplete transaction so append an error for use later.
            string CurrentLine = sr.ReadLine();
            if (!String.IsNullOrEmpty(CurrentLine))
            {
              strNewCols = CurrentLine.Split("\t".ToCharArray());
              string TransactionNumber = strNewCols[3].ToString();
              PopulateRow(null, strNewCols, oXTW, rowcount);
            }
          }
        }
      }
      oXTW.WriteFullEndElement();
      oXTW.Flush();
      stmMemory.Position = 0;

      if (SaveXmlTlogConfig == null)
        SaveXmlTlogConfig = System.Configuration.ConfigurationManager.AppSettings["SaveXMLTLog"] == null ? "" : System.Configuration.ConfigurationManager.AppSettings["SaveXMLTLog"].ToString();
			if (XMLTLogSavePath == null)
				XMLTLogSavePath = System.Configuration.ConfigurationManager.AppSettings["XMLTLogSavePath"] == null ? Environment.CurrentDirectory : System.Configuration.ConfigurationManager.AppSettings["XMLTLogSavePath"].ToString();

      if (!String.IsNullOrEmpty(SaveXmlTlogConfig))
      {
        bool SaveFile = Boolean.Parse(SaveXmlTlogConfig);
        if (SaveFile)
        {
					string TlogName = new FileInfo(sFile).Name;

          StreamReader sr = new StreamReader(stmMemory);
					if (!Directory.Exists(XMLTLogSavePath))
						Directory.CreateDirectory(XMLTLogSavePath);

          string File = String.Format("{0}\\{1}.xml", XMLTLogSavePath, TlogName);
					if (System.IO.File.Exists(File))
					{
						string[] foundFiles = Directory.GetFiles(XMLTLogSavePath, TlogName + "*");
						if (foundFiles.Length >= 1)
						{							
							File = XMLTLogSavePath + "\\" + TlogName + "." +  foundFiles.Length.ToString().PadLeft(4, '0') + ".xml";
						}
					}

					try
					{
						using (StreamWriter swrt = new StreamWriter(File))
						{
							while (sr.Peek() >= 0)
								swrt.WriteLine(sr.ReadLine());
						}
					}
					catch {
					}
          
        }
      }
      stmMemory.Position = 0;

#if (DEBUG)
      Console.WriteLine("Completed Conversion To Xml Reader " + DateTime.Now.ToString("hh:mm:ss"));
#endif

      return new XmlTextReader(stmMemory);

    }

    /// <summary>
    /// Convert the tab delimted TLog file to an XML formatted file for processing.
    /// </summary>
    /// <param name="sFile">The TLog File To Convert</param>
    /// <param name="sTemplate">The template file used to populate the new xml document</param>
    /// <returns>Xml representation of the file</returns>
    public XmlDocument ConvertTLogToXML(string sFile, string sTemplate, string RulesFile)
    {
      XmlTextReader reader = GetTLogToXMReader(sFile, sTemplate, RulesFile);
      XmlDocument xdoc = new XmlDocument();

      reader.MoveToContent();
      xdoc.InnerXml = reader.ReadOuterXml();

      return xdoc;
    }

    public XmlDocument ConvertTLogToXML(string sFile, string sTemplate)
    {
      XmlTextReader reader = GetTLogToXMReader(sFile, sTemplate, null);
      XmlDocument xdoc = new XmlDocument();

      reader.MoveToContent();
      xdoc.InnerXml = reader.ReadOuterXml();

      return xdoc;
    }

    /// <summary>
    /// Adds a new row to the xmltextwriter
    /// </summary>
    /// <param name="rowElement"></param>
    /// <param name="dr"></param>
    /// <param name="XmlWriter"></param>
    /// <param name="LineNumber"></param>
    private void PopulateRow(XmlElement rowElement, string[] strNewCols, XmlWriter XmlWriter, int LineNumber)
    {
      //Write the unknown record out 
      if (rowElement == null)
      {
        rowElement = tLogTransaction.CreateElement("UnknownRecord");
        //Creat array to hold the know records that all line items have
        string[] KnownCols = new string[] { "record_type", "store_num", "reg_num", "trans_num", "trans_date" };

        //Create the element
        XmlWriter.WriteStartElement(rowElement.Name);
        for (int i = 0; i < strNewCols.Length; i++)
        {
          if (i >= KnownCols.Length)
            XmlWriter.WriteAttributeString("Attrib" + i.ToString(), strNewCols[i].ToString());
          else
            XmlWriter.WriteAttributeString(KnownCols[i], strNewCols[i].ToString());
        }
        XmlWriter.WriteEndElement();
        return;
      }
     
      bool RuleFound = true;
      //Check to see if the rules doc was passed in.  If it was only add the element if there is a rule
      if (RulesDoc != null)
      {
        XPathNavigator nav = RulesDoc.CreateNavigator();
        RuleFound = Convert.ToInt32(nav.Evaluate("count(//Rule[@id='" + rowElement.Name + "'])")) > 0;
      }
      //rule wansnt found so continue.
      if (!RuleFound)
        return;

      XmlWriter.WriteStartElement(rowElement.Name);
      for (int x = 0, count = rowElement.Attributes.Count; x < count; x++)
      {
        if (x >= strNewCols.Length)
          break;

        XmlWriter.WriteAttributeString(rowElement.Attributes[x].Name, strNewCols[x]);
      }

      XmlWriter.WriteEndElement();
    }

    private static bool isNumeric(string value)
    {
      return Regex.IsMatch(value, @"\d");
    }

    public enum ErrorType
    {
      IncompleteTransaction,
      Invalid_Record_Type,
      Corrupt_Record,
      Invalid_Store_No,
      Invalid_Register_No,
      Invalid_Transaction_No,
      Invalid_Transaction_Date,
      Invalid_Record_Count

    }
  }
}
