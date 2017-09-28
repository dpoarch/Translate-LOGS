using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using SpencerGifts.TlogCommon;
using System.Configuration;
using System.Reflection;
using BlackHen.Threading;
using System.Collections;
using System.Xml.XPath;
using System.Threading;
using System.Linq;

namespace SpencerGifts.Translate.Plugin.TLog
{
  sealed internal class TLogTranslateItem : TranslateItem
  {
    #region Private Members
   
    private const string ResFile = "SpencerGifts.Translate.Plugin.TLog.Properties.TlogResource";

    /// <summary>
    /// The resulting xml document create by converting the tab delimted tlog file to xml
    /// </summary>
    XmlReader ConvertedTLogDoc;
    XmlDocument _SourceDocument;
    XmlNode LineNode;
    /// <summary>
    /// Holds the DateTime of the previous translated transaction from the tlog
    /// </summary>
    DateTime PreviousHeaderTime;
    /// <summary>
    /// Collection used to hold items that need to be added to the destination document until we have a valid line ("L") record
    /// </summary>
    List<XmlNode> List;
    /// <summary>
    /// Used to keep track if a line item was found yet or not
    /// </summary>
    bool LineFound = false;
    /// <summary>
    /// Holds the currently translated file name
    /// </summary>
    string _SourceFile = "";
    /// <summary>
    /// Keeps track of a head item if found yet
    /// </summary>
    bool HeadItemFound;
    /// <summary>
    /// Count of how many merchandise (M) records are in the current transaction
    /// </summary>
    int MerchandiseCount;
    /// <summary>
    /// Unique id used to keep track of the total number of discounts in a transaction.
    /// </summary>
    int UniqueDiscountId;
    /// <summary>
    /// Used to keep track of the line number that a discount record will be assigned to
    /// </summary>
    int DiscountLineNumber;
    /// <summary>
    /// Holds the country code of the current translating file
    /// </summary>
    string StoreCountryCode = "";
    /// <summary>
    /// Holds the configuration for a country while translating
    /// </summary>
    CountryConfig countryConfig;
    /// <summary>
    /// Holds the current runtime for use in the save directory names
    /// </summary>
    static string RunTime;
    /// <summary>
    /// Object used to prevent the same thread from creating an instance of the OutputDirectoriesCreated object
    /// </summary>
    static readonly object ThreadLock = new object();
    /// <summary>
    /// Used to keep track of the output directories created
    /// </summary>
    static List<string> OutputDirectoriesCreated;
    /// <summary>
    /// Holds a list of stores that will have their country code changed at runtime so it can point to a different configuration.
    /// </summary>
    List<string> ConsignmentStoreFile;
    /// <summary>
    /// Used to hold the stream of the source document;
    /// </summary>
    private MemoryStream _SourceDocumentMS;
    /// <summary>
    /// Threadpool used to porcess multiple files at a time
    /// </summary>
    private WorkQueue work;
    ExcludeFileHelper ExcludeHelper;
    //The File which PolledNetSales will be written to for outside processes.
    static string PolledNetSalesFile;
    //Lock object to prevent multiple threads from writing to the file at the same time
    static readonly object PolledFileWriteLock = new object();
    private List<XmlNode> BankNotes;

    #endregion    
    
    public TLogTranslateItem()
    {
      List = new List<XmlNode>();
      OnBeforeNodeAppended += TLogTranslateItem_OnBeforeNodeAppended;
      OnAfterRuleProcess += TLogTranslateItem_OnAfterRuleProcess;
      OnBeforeRuleProcess += TLogTranslateItem_OnBeforeRuleProcess;
      //OnAfterRuleLoad += new EventHandler(TLogTranslateItem_OnAfterRuleLoad);
      OnAfterRuleLoad += TLogTranslateItem_OnAfterRuleLoad;
      OnAfterNodeAppended += TLogTranslateItem_OnAfterNodeAppended;

      if (OutputDirectoriesCreated == null)
      {
        //lock object to prevent threads from creating more than one instance
        lock (ThreadLock)
          if (OutputDirectoriesCreated == null)
            OutputDirectoriesCreated = new List<string>();
      }
    }

    void TLogTranslateItem_OnAfterNodeAppended(object sender, EventArgs e)
    {
      XmlElement NodeAppend = (XmlElement)sender;

      if (NodeAppend.Name == "L")
      {
        if (NodeAppend.Attributes["Line_object"].Value == "600" && NodeAppend.Attributes["Line_action"].Value == "246")
        {
          //append the bank notes created in the L51 processing
          if (BankNotes != null)
          {
            for (int i = 0; i < BankNotes.Count; i++)
            {
              XmlNode note = BankNotes[i];
              note.Attributes["Line_id"].Value = NodeAppend.Attributes["Line_id"].Value;
              LastParentNodeAppendedTo.InnerXml = LastParentNodeAppendedTo.InnerXml + note.OuterXml;              
            }
          }
          BankNotes = null;
        }

				///This was not working and may need to revisit
        //Special case for Boobies make me smile t-shirts.  $2.00 of the $10.00 transaction needs to be fed in as a donation.
            //This is a temporary fix until POS figures out how to do it with the register
				//if (NodeAppend.Attributes["Line_object"].Value == "190")
				//{
          
				//  double ItemValue = Convert.ToDouble(NodeAppend.Attributes["Line_amount"].Value);
				//  ItemValue = ItemValue - 2;
				//  IncrementCounter();
				//  NodeAppend.Attributes["Line_id"].Value = Counter.ToString();
				//  NodeAppend.Attributes["Line_amount"].Value = ItemValue.ToString();
				//  XmlElement AdditionalLine = (XmlElement)NodeAppend.Clone();
				//  AdditionalLine.Attributes["Line_id"].Value = (Counter-1).ToString();
				//  AdditionalLine.Attributes["Line_object"].Value = "195";
				//  AdditionalLine.Attributes["Line_amount"].Value = "2";
				//  LastParentNodeAppendedTo.InnerXml = LastParentNodeAppendedTo.InnerXml + AdditionalLine.OuterXml;
				//}
      }

    }

    #region Override  Methods

    /// <summary>
    /// Override the default Append Node method to handle line items that need to be held until a "L" transaction is found
    /// </summary>
    /// <param name="Parent"></param>
    /// <param name="NodeToAppend"></param> 
    XmlElement LastParentNodeAppendedTo;
    protected override void AppendNode(XmlElement Parent, XmlNode NodeToAppend)
    {
      //All cases here are specific rules that need to be applied for the translate and could not be obtained in the translate rules.
      //If we have a header record append it to the output document
      if (NodeToAppend.Name == "H")
      {
        //keep track of the header that was found for later processing if a header is found without a line item.
        HeadItemFound = true;
        XmlNode OriginalDateNote = null;
        //We need to check the transaction date and time to make sure we do not have multiple transactions with the same time.
        //This is an auditworks rule
        //DateTime CurrentTransTime = Convert.ToDateTime(NodeToAppend.Attributes["Entry_date_time"].Value);
        //DateTime TempDate = new DateTime(PreviousHeaderTime.Year, PreviousHeaderTime.Month, PreviousHeaderTime.Day, PreviousHeaderTime.Hour, PreviousHeaderTime.Minute, 0);
        //if the current transaction time is the same as the previous transaction time we increment the time by 1 and store it until the time no longer matches
        //if (CurrentTransTime == TempDate)
        //{
        //  NodeToAppend.Attributes["Entry_date_time"].Value = PreviousHeaderTime.AddSeconds(1).ToString("MM/dd/yyyy HH:mm:ss");
        //  PreviousHeaderTime = PreviousHeaderTime.AddSeconds(1);
        //}
        //else
          //Time no longer matches so reset it to the current transaction time
        //  PreviousHeaderTime = CurrentTransTime;

        //Custom rule that determines when the date cutoff is.  If a transaction is found in the tlog with a date > than this (Currently 3AM)
        //the date will be moved back to the previous day.  This will only happen if it is turned on in the configuration.
        if (PluginConfig.GetValue("ReDate").ToLower() == "true")
        {          
          DateTime TranslateCutOffTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 3, 0, 0);
          DateTime HeaderTransDate = Convert.ToDateTime(NodeToAppend.Attributes["Entry_date_time"].Value);
          if (HeaderTransDate >= TranslateCutOffTime)
          {
            NodeToAppend.Attributes["Entry_date_time"].Value = HeaderTransDate.AddDays(-1).ToString("MM/dd/yyyy") + " 23:59:59";
            OriginalDateNote = DestinationDocumentTemplate.SelectSingleNode("//N");
            OriginalDateNote.Attributes["Line_note_type"].Value = "57";
            OriginalDateNote.Attributes["Line_Note"].Value = HeaderTransDate.ToString("MM/dd/yyyy HH:mm:ss");            
            


          }
        }

        //LineNode will be null if we have a header with a line item
        if (LineNode != null)
        {
          Parent.InnerXml = Parent.InnerXml + LineNode.OuterXml;
          LineNode = null;
        }

        Parent.AppendChild(NodeToAppend);

        if (OriginalDateNote != null)
        {
          Parent.InnerXml = Parent.InnerXml + OriginalDateNote.OuterXml;
        }
          
      }
      //if we have a L record append it
      else if (NodeToAppend.Name == "L")
      {
        //keep track of line items found for later processing if necessary.
        LineFound = true;

        Parent.AppendChild(NodeToAppend);

        //now if we have any items waiting to be added we can add them now
        if (List.Count <= 0)
          return;

        for (int i = 0; i < List.Count; i++)
          Parent.InnerXml = Parent.InnerXml + List[i].OuterXml;

        List.Clear();
      }
      //A line item has already been found so keep adding
      else if (LineFound)
        Parent.AppendChild(NodeToAppend);
      //base.AppendNode(Parent, NodeToAppend);

      LastParentNodeAppendedTo = Parent;
    }

    /// <summary>
    /// Override to the default save method which saves the file as xml.  We need a tab delimited file as our end result
    /// </summary>
    /// <param name="TranslatedDocument">The final translated document</param>
    protected override void SaveTranslatedFile(XmlDocument TranslatedDocument)
    {
      XmlNodeList list = TranslatedDocument.SelectNodes("//" + NewDocumentRoot + "/*");

      if (string.IsNullOrEmpty(TranslatedSaveFile))
        return;

      // create a writer and open the file
      using (TextWriter tw = new StreamWriter(TranslatedSaveFile))
      {
        foreach (XmlNode node in list)
        {
          StringBuilder sb = new StringBuilder();
          for (int x = 0; x < node.Attributes.Count; x++)
            sb.Append(node.Attributes[x].Value.ToString() + "\t");
          tw.WriteLine(sb.ToString());
        }
        tw.Close();
      }
      //base.SaveTranslatedFile(TranslatedDocument);
    }

    /// <summary>
    /// Source document template to translate
    /// </summary>
    /// 
    public override XmlReader SourceDocument
    {
      get
      {
        if (_SourceDocumentMS == null)
        {
            //Convert the source document to xml
            ConvertedTLogDoc = new Xml().GetTLogToXMReader(_SourceFile, base.SourceTemplateFile);
            _SourceDocumentMS = new MemoryStream();
            TextWriter tw = new StreamWriter(_SourceDocumentMS);
            ConvertedTLogDoc.MoveToContent();
            //Write the text of the converted xml doc to the memory stream to be accessed for future requests
            tw.Write(ConvertedTLogDoc.ReadOuterXml());
            tw.Flush();
        }
        //Set the position of the stream back to 0;
        _SourceDocumentMS.Position = 0;
        //Reaturn the new xmlreader
        XmlReaderSettings settings = new XmlReaderSettings();
        settings.CheckCharacters = false;
        return XmlReader.Create(_SourceDocumentMS,settings);
      }
    }

    /// <summary>
    /// Gets or sets the file name that will be processed.
    /// </summary>
    override public string SourceFile
    {
      get { return _SourceFile; }
      set { _SourceFile = value; }
    }

    protected override void Process()
    {
      LoadConsignmentStores();

      if (String.IsNullOrEmpty(RunTime))
        RunTime = DateTime.Now.ToString("MMddhhmm");

      //If a single file is already supplied we will only process that file instead 
      //of looping through the source directory looking for files to Translate.
      if (!String.IsNullOrEmpty(_SourceFile))
      {
        if (!File.Exists(_SourceFile))
          return;

        FileInfo file = new FileInfo(_SourceFile);
        try
        {
          TranslateFile(file);
        }
        catch (Exception ex)
        {
          LogException(ex.GetType().GetProperties(),ex);
          throw ex;
        }
        
        LogMessage(String.Format(ResourceHelper.Instance.GetString(ResFile, "TranslateComplete"), file.Name, DateTime.Now.ToString("hh:mm:ss")));
        return;
      }

      FileInfo[] TlogFiles = GetFiles(base.SourceFileLocation);

      //If there are no files to translate quit.
      if (TlogFiles == null)
      {
        LogMessage(ResourceHelper.Instance.GetString(ResFile, "NoFiles"));
        LogMessage(String.Format(ResourceHelper.Instance.GetString(ResFile, "ValidPathCheck"),SourceFileLocation));
        TranslateComplete();
        System.Threading.Thread.Sleep(1000);
        return;
      }
      else if (TlogFiles.GetLength(0) == 0)
      {
        LogMessage(ResourceHelper.Instance.GetString(ResFile, "NoFiles"));
        TranslateComplete();
        System.Threading.Thread.Sleep(1000);
        return;
      }

      LogMessage(ResourceHelper.Instance.GetString(ResFile, "TranslateStarted"));

      try
      {
        //Initialize the threadpool for the worker items soon to be sent to it
        InitializeThreadPool();
      }
      catch (Exception ex)
      {
        LogMessage("Unable to initialize the thread pool.  Ending translate");
        LogException(ex.GetType().GetProperties(),ex);
        return;
      }

      //System.Threading.Tasks.TaskFactory factory = new System.Threading.Tasks.TaskFactory();

      foreach (FileInfo file in TlogFiles)
      {
        try
        {
          Translate.TranslateItem item = new TLogTranslateItem();                   
          item.SourceFile = file.FullName;
          TranslateWorker.ExceptionLogHandler Logger = new TranslateWorker.ExceptionLogHandler(LogException);
          TranslateWorker worker = new TranslateWorker(Logger);
          worker.TransItem = item;
          worker.PluginConfigName = base.PluginConfig.PluginName;


          //var task = factory.StartNew(() => item.ExecuteTranslate(base.PluginConfig.PluginName));
          
          work.Add(worker);

          //item.ExecuteTranslate(base.PluginConfig.PluginName);

          //System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(item.ExecuteTranslate), base.PluginConfig.PluginName);
          //return;
          //TranslateFile(file);
        }
        catch (Exception ex)
        {
          LogMessage(String.Format(ResourceHelper.Instance.GetString(ResFile, "UnHandledException"), file.Name, ex.Message + "\r\n" + ex.StackTrace));
          if (!String.IsNullOrEmpty(PluginConfig.GetValue("FailedTranslatePath")))
          {
            if (Directory.Exists(PluginConfig.GetValue("FailedTranslatePath")))
            {
              LogMessage("Moving File to " + PluginConfig.GetValue("FailedTranslatePath"));
              file.MoveTo(PluginConfig.GetValue("FailedTranslatePath") + "\\" + file.Name);
            }
            else
              LogMessage("Unable to Move File.  Missing or Invalid configuration value: FailedTranslatePath");
          }
          _SourceDocument = null;
          _SourceDocumentMS = null;
        }
      }

      //int ProgramRunTime = 0;
      //while (!workcomplete)
      //{
      //  if (ProgramRunTime >= 600 && work.Count == 0)
      //  {
      //    //Used to prevent the program from infinite loop;
      //    LogMessage(ResourceHelper.Instance.GetString(ResFile, "AppTimeOut"));
      //    return;
      //  }
      //  #if (DEBUG)        
      //    Console.WriteLine("*********Total Items In ThreadPool " + work.Count.ToString() + "*********");
      //  #endif
      //  System.Threading.Thread.Sleep(5000);
      //  ProgramRunTime++;
      //}
      
      //Keep the main thread waiting untill all items have been processed in the threadpool.  Once the threadpool is finished the application will exit.
      Thread.Sleep(Timeout.Infinite);
    }

    protected override string TranslatedSaveFile
    {
      get
      {
        FileInfo info = new FileInfo(_SourceFile);
        string DirectorySave = base.TranslatedFileLocation + "\\" + countryConfig.SaveDirectoryLocation + "\\" + countryConfig.SaveDirectoryPrefix + "." + RunTime + ".TMP";
        string SeqNum = "1";
        string FileName = info.Name;
        FileName = FileName.ToLower().Replace("tlog", "");
				FileName = FileName.Substring(0, 5);

				if (FileName.Substring(0,1) == "0")
					FileName = FileName.Substring(1, 4);
				else
					FileName = FileName.Substring(0, 5);

        FileName = String.Format("{0}{1}", countryConfig.TranslatedFilePrefix, FileName.PadLeft(5,'0'));
        if (!Directory.Exists(DirectorySave))
        {
          try
          {
            Directory.CreateDirectory(DirectorySave);
            if (!OutputDirectoriesCreated.Contains(DirectorySave))
              OutputDirectoriesCreated.Add(DirectorySave);
          }
          catch (Exception ex)
          {
            LogMessage(String.Format(ResourceHelper.Instance.GetString(ResFile, "OutputDirError"), DirectorySave, ex.Message));
            return null;
          }
        }

        string SaveFile = string.Format("{0}\\{1}{2}", DirectorySave, FileName, SeqNum.PadLeft(4, '0'));
        //check to see if the file we are about to save already exists because we do not want to write over it.
        if (File.Exists(SaveFile))
        {
          DirectoryInfo d = new DirectoryInfo(DirectorySave);
          FileInfo[] trnsFiles = d.GetFiles(FileName + "*");
          if (trnsFiles != null)
          {
            int FileCount = trnsFiles.GetUpperBound(0) + 2;
            SeqNum = FileCount.ToString();
          }
          
          //there is atleast one file existing so use the seqnum to create a new unique file name.
          SaveFile = string.Format("{0}\\{1}{2}", DirectorySave, FileName, SeqNum.PadLeft(4, '0'));
        }
        return SaveFile;
      }
    }
    #endregion

    #region Processing Events
    void TLogTranslateItem_OnBeforeRuleProcess(object sender, EventArgs e)
    {
      //Reset the HeadItemFound value before processing.
      //XmlDocument doc = (XmlDocument)sender;
      HeadItemFound = false;
      BankNotes = null;
    }

    void TLogTranslateItem_OnBeforeNodeAppended(object sender, EventArgs e)
    {
      //Any custom logic that could not be handled in the xml config file will be found here

      XmlNode NodeToAppend = (XmlNode)sender;

      switch (NodeToAppend.Name)
      {
        case "H":
          LineFound = false;
          MerchandiseCount = 0;
          if (NodeToAppend.Attributes["Cashier_no"] != null && !String.IsNullOrEmpty(NodeToAppend.Attributes["Cashier_no"].Value))
          {
						//Anything > 4 will be the new unique cashier numbers and there is no need to add the store number in front of it.
						if (NodeToAppend.Attributes["Cashier_no"].Value.Length < 4)
						{
							NodeToAppend.Attributes["Cashier_no"].Value = NodeToAppend.Attributes["Cashier_no"].Value.PadLeft(4, Convert.ToChar("0"));

							//Custom Rule to add 5 digit store number before the cashier number
							string StoreNo = NodeToAppend.Attributes["Store_no"].Value;
							StoreNo = StoreNo.PadLeft(5, Convert.ToChar("0"));
							NodeToAppend.Attributes["Cashier_no"].Value = StoreNo + NodeToAppend.Attributes["Cashier_no"].Value;
						}
          }
          //reset the uniqe counter
          InitializeCounter(100);
          break;
        case "L":
          if (LineFound)
            IncrementCounter();

            NodeToAppend.Attributes["Line_id"].Value = Counter.ToString();

          if (NodeToAppend.Attributes["Reference_no"].Value == "0")
            NodeToAppend.Attributes["Reference_no"].Value = "";

          switch (NodeToAppend.Attributes["Line_action"].Value)
          {
            //Actions of these type require the absolute value of the amount
            case "12":
            case "15":
            case "18":
            case "20":
            case "24":
            case "27":
            case "29":
            case "32":
            case "245":
                NodeToAppend.Attributes["Line_amount"].Value = Math.Abs(Convert.ToDouble(NodeToAppend.Attributes["Line_amount"].Value)).ToString();
                break;           
          }        
          //if DiscountLineNumber was reset to -1 it means we are at the start of a L30 record see event TLogTranslateItem_OnAfterRuleLoad for details.
          //So no we keep track of the current line number to apply the discount to later
          if (DiscountLineNumber == -1)
            DiscountLineNumber = Counter;

          LineFound = true;
          break;
        case "C":
          //Line_id for customer records is always 0
          NodeToAppend.Attributes["Line_id"].Value = "0";
          if (!LineFound)
            List.Add(NodeToAppend);
          break;
        case "M":
          MerchandiseCount++;
          if (!LineFound)
            List.Add(NodeToAppend);
          break;
        case "N":
          int Line_note_type = Convert.ToInt32(NodeToAppend.Attributes["Line_note_type"].Value);
          //NOTE type 50 is the location where the hashed credit card is to be stored.  
          //Auditworks was unable to properly instert the original hashed string that Trovato creates so we are converting
          //the original string into it's hex equivelant which auditworks will handle as a typical string value.
          if (Line_note_type == 50)
          {

            byte[] CCBytes = System.Text.Encoding.Default.GetBytes(NodeToAppend.Attributes["Line_Note"].Value.ToCharArray());
            System.Text.Encoding.Convert(Encoding.Default, Encoding.UTF7, CCBytes);
            
            if (CCBytes == null || CCBytes.Length == 0)
              break;

            StringBuilder sbToHEX = new StringBuilder();
            foreach (byte b in CCBytes)
            {
              // Get the integer value of the character.
              int value = Convert.ToInt32(b);
              // Convert the decimal value to a hexadecimal value in string form.              
              sbToHEX.Append(String.Format("{0:X}", value).PadLeft(2,'0'));
            }
            NodeToAppend.Attributes["Line_Note"].Value = sbToHEX.ToString();
          }
          if (!LineFound)
            List.Add(NodeToAppend);
          break;

        case "D":
          //NodeToAppend.Attributes["Line_id_adjustment"].Value = UniqueDiscountId.ToString();
          //NodeToAppend.Attributes["Line_id"].Value = String.Format("{0:g}", DiscountLineNumber - UniqueDiscountId);
          NodeToAppend.Attributes["Line_id_adjustment"].Value = "0";
          NodeToAppend.Attributes["Line_id"].Value = String.Format("{0:g}", DiscountLineNumber);

          UniqueDiscountId++;
          if (!LineFound)
            List.Add(NodeToAppend);
          break;
        default:
          if (!LineFound)
            List.Add(NodeToAppend);
          break;
      }

      if (NodeToAppend.Name == "E")
      {
        string s = NodeToAppend.Attributes["Customer_info"].Value;
        s = s.Replace("N0", "N");
        s = s.Replace("N1", "");
        NodeToAppend.Attributes["Customer_info"].Value = s;
      }
    }

    //int TotalBankDepositsKeyed = 0;
    //int TotalBankDepositVerifications = 0;
    //int TotalBankDepositsCreatedByTranslate = 0;
    bool NeedCashCountedLine = true;

    void TLogTranslateItem_OnAfterRuleProcess(object sender, TranslateEventArgs e)
    {
      XmlNode TransNode = (XmlNode)sender;
      string SaveType = TransNode.SelectSingleNode("//L10").Attributes["type"].Value;
      if (String.IsNullOrEmpty(TransNode.SelectSingleNode("//L10").Attributes["flgs_L10ADMIN"].Value))
        return;

      bool isAdminTrans = Convert.ToBoolean(Int32.Parse(TransNode.SelectSingleNode("//L10").Attributes["flgs_L10ADMIN"].Value));

      //We are not allowed to have a header item without a line item so we handle that here
      //if (HeadItemFound != LineFound && (isAdminTrans && (SaveType == "1" || SaveType == "144"))) 
      if ((HeadItemFound != LineFound) && isAdminTrans)
        CreateLineItem(TransNode, e.OutputDocument);


    }

    void TLogTranslateItem_OnAfterRuleLoad(object sender, TranslateEventArgs e)
    {
      TranslateRule transrule = (TranslateRule)sender;
      if (transrule.ID == "L30")
      {
        //When we reach the 30 record we reset the discount line number so we can reassign it later if the 30 record has a valid Line (L) item to create 
        DiscountLineNumber = -1;
        //Reset out counter to keep track of the number of discount line items in the transaction.
        UniqueDiscountId = 0;
      }

      if (transrule.ID == "L60")
      {
        XPathNavigator nav = e.CurrentTranslateItem.CreateNavigator();
        if (Convert.ToBoolean(nav.Evaluate(GetXpathExpression("string(@type) = '2' and string(@seq_num) = '38'"))))
        {
          if (String.IsNullOrEmpty(PolledNetSalesFile))
            PolledNetSalesFile = base.PluginConfig.GetValue("PolledNetSalesFile");

          //NetSales sales = new NetSales();

          string netsales = nav.Evaluate(GetXpathExpression("string(@amount)")).ToString();
          string storeNum = nav.Evaluate(GetXpathExpression("string(@store_num)")).ToString();
          DateTime TransDate = Convert.ToDateTime(nav.Evaluate(GetXpathExpression("string(@trans_date)")).ToString());
          WritePolledNetSales(storeNum, netsales, TransDate);
          
        }
      }
      //This will store the notes for bank deposit and will be added to the output when the translate hits the 52 record
      if (transrule.ID == "L51") 
      {
        if (BankNotes == null)
          BankNotes = new List<XmlNode>();
        
        XPathNavigator nav = e.CurrentTranslateItem.CreateNavigator();
        int transType = 10;
        while (transType<=26)
        {
          string strExpr = String.Format("count(following-sibling::L52[@Tender_id_sub1='{0}' and number(@tender_id) = 0]) > 0", transType);
          if (Convert.ToBoolean(nav.Evaluate(GetXpathExpression(strExpr))))
          {
            string LineNote = String.Format("number(following-sibling::L52[@Tender_id_sub1='{0}' and number(@tender_id) = 0]/@extended_amount)", transType);
            string NoteType = String.Format("number(100 + number(following-sibling::L52[@Tender_id_sub1='{0}' and number(@tender_id) = 0]/@Tender_id_sub1))", transType);
            XmlNode BankNote = DestinationDocumentTemplate.SelectSingleNode("//N");
            BankNote.Attributes["Line_id"].Value = "0";
            BankNote.Attributes["Line_Note"].Value = Convert.ToString(nav.Evaluate(GetXpathExpression(LineNote)));
            BankNote.Attributes["Line_note_type"].Value =  Convert.ToString(nav.Evaluate(GetXpathExpression(NoteType)));
            BankNotes.Add(BankNote);
          }
          transType++;
        }
      }

    }

    /// <summary>
    /// Creates a line item.
    /// </summary>
    /// <param name="TransNode"></param>
    private void CreateLineItem(XmlNode TransNode, XmlDocument CurrentTransDocument)
    {
      LineNode = DestinationDocumentTemplate.SelectSingleNode("//L");

      string LineObject;
      string LineAction;

      LineNode.Attributes["Line_id"].Value = "100";

      if (TransNode.SelectSingleNode("//L10").Attributes["flgs_L10AVOID"].Value == "1")
      {
        LineObject = "1101";
        LineAction = "35";
        LineNode.Attributes["Voiding_reversal_flag"].Value = "1";
      }
      else if (TransNode.SelectSingleNode("//L10").Attributes["type"].Value == "83")
      {
        LineObject = "600";
        LineAction = "32";
      }
      else
      {
        LineObject = "1407";
        LineAction = "38";
      }
      LineNode.Attributes["Line_object"].Value = LineObject;
      LineNode.Attributes["Line_action"].Value = LineAction;
           
      AppendNode(LastParentNodeAppendedTo, CurrentTransDocument.ImportNode(LineNode,true));
      LineNode = null;
    }
    #endregion

    /// <summary>
    /// This method does the validation and is the entry point for the translating.
    /// </summary>
    /// <param name="file">The file to be translated and validated</param>
    private void TranslateFile(FileInfo file)
    {
      ConvertedTLogDoc = null;
      this._SourceFile = file.FullName;
      if (ExcludeHelper == null)
        ExcludeHelper = new ExcludeFileHelper(PluginConfig.GetValue("TranslateExludeFile"), PluginConfig.GetValue("TranslateExludePath"), PluginConfig.GetValue("TranslateExludeType_BeforeTrans").Split(','));

      if (_SourceDocument == null)
      {        
        _SourceDocument = new XmlDocument();
        using (XmlReader SourceReader = SourceDocument)
        {
          SourceReader.MoveToContent();
          _SourceDocument.LoadXml(SourceReader.ReadOuterXml());
        }
      }

      bool isvalid = true;    
        Validate TranslateValidator = null;
      bool validate = (PluginConfig.GetValue("ValidateTLog") == "") ? false : Convert.ToBoolean(PluginConfig.GetValue("ValidateTLog"));
      if (validate)
      {
          try
          {
              TranslateValidator = new SpencerGifts.TlogCommon.Validate();

              //Before we can process the file we need to check to make sure its a valid tlog\
              LogMessage(String.Format(ResourceHelper.Instance.GetString(ResFile, "ValidateFile"), file.Name));
              //isvalid = TranslateValidator.isValidTLog(_SourceDocument, _SourceFile);        
              isvalid = TranslateValidator.isValidTLog(_SourceDocument, file.FullName);
          }
          catch (Exception ex)
          {
              LogMessage("Validator Exception " + ex.Message + " " + ex.StackTrace);
              //Defaulting to true since the validator blew up and we don't necessarly want to prevent tlogs from getting held up
              isvalid = true;
          }
        
      }
      if (!isvalid)
      {
        string BadTlogLocation = PluginConfig.GetValue("BadTlogLocation");
       
          if (TranslateValidator != null)
          {
              Console.WriteLine(TranslateValidator.ErrorMessage);
              LogMessage(String.Format(ResourceHelper.Instance.GetString(ResFile, "InvalidFile"), file.Name));
              LogMessage("\r\n------------------------------------------\r\n" + TranslateValidator.ErrorMessage + "------------------------------------------");
              //Logging.LogMessage(String.Format(ResourceHelper.Instance.GetString(ResFile, "MovingBadTLog"), BadTlogLocation));
          }
      
        File.Move(_SourceFile, BadTlogLocation + "\\" + file.Name);
      }
      else
      {

        //We need to check the exclude file first before we translate.  If the store is in the exclude file and
        //has a type that specifies not to translate the file then move it to the appropriate non translating directory.       
        if (!ExcludeHelper.ShouldTranslate(file.Name.Substring(4, 5)))
        {
          MoveFile(file, ExcludeHelper.NonTranslateSavePath);
          return;
        }

        LogMessage(String.Format(ResourceHelper.Instance.GetString(ResFile, "FileIsValid"), file.Name));
        LogMessage(String.Format(ResourceHelper.Instance.GetString(ResFile, "TranslatingFile"), file.Name));
        try
        {
          StoreCountryCode = _SourceDocument.SelectSingleNode("//L10/@country").Value;
          if (isConsignmentStore(file.Name))
            countryConfig = new CountryConfig(PluginConfig.GetValue("ConsignmentCountryCode"));
          else
            countryConfig = new CountryConfig(StoreCountryCode);
          
          base.Process();
          //now we need to check the exclude file again to verify we can move the translated file to the backup location
          if (ExcludeHelper.OkToBackup(file.Name.Substring(4, 5)))
            BackupFile(file);
          else
            MoveFile(file, ExcludeHelper.NonTranslateSavePath);
        }
        catch (RuleConditionException ex)
        {
          PropertyInfo[] info = ex.GetType().GetProperties();
          LogException(info, ex);
          throw ex;
        }
        catch (RuleActionException ex)
        {
          PropertyInfo[] info = ex.GetType().GetProperties();
          LogException(info, ex);
          throw ex;
        }
        catch (RuleMappingException ex)
        {
          PropertyInfo[] info = ex.GetType().GetProperties();
          LogException(info, ex);
          throw ex;
        }
        catch (Exception ex)
        {
          LogMessage(String.Format(ResourceHelper.Instance.GetString(ResFile, "UnHandledException"), file.Name, ex.ToString()));
          throw ex;
        }
        finally
        {
          _SourceDocument = null;
          _SourceDocumentMS = null;
        }

      }
    }

    /// <summary>
    /// Get a listing of all tlogs that need to be translated 
    /// </summary>
    /// <param name="Location"></param>
    /// <returns></returns>    
    FileInfo[] GetFiles(string Location)
    {
      if (Directory.Exists(Location))
      {
        DirectoryInfo dinfo = new DirectoryInfo(Location);
        return dinfo.GetFiles("tlog*");
      }
      else
      {
        LogMessage(String.Format(ResourceHelper.Instance.GetString(ResFile,"InvalidDirectory"),Location));
        return null;
      }

    }

    /// <summary>
    /// Used to move the translated file to the backup location.
    /// </summary>
    /// <param name="srcFile"></param>
    private void BackupFile(FileInfo srcFile)
    {
      string BackupFolder = PluginConfig.GetValue("BackupFolder");

      string BackupFile = String.Format("{0}\\{1}", BackupFolder, srcFile.Name);
      if (File.Exists(BackupFile))
      {
        DirectoryInfo backupDir = new DirectoryInfo(BackupFolder);
        FileInfo[] filecount = backupDir.GetFiles(srcFile.Name + "*");
        int NextNum = 1;
        if (filecount != null)
          NextNum = filecount.GetUpperBound(0) + 1;

        BackupFile = String.Format("{0}\\{1}.{2}", BackupFolder, srcFile.Name,NextNum.ToString().PadLeft(4, '0'));
      }
      srcFile.MoveTo(BackupFile);
    }

    private void MoveFile(FileInfo srcFile, string DestinationPath)
    {
      string BackupFolder = DestinationPath;
      string BackupFile = String.Format("{0}\\{1}", BackupFolder, srcFile.Name);

      if (File.Exists(BackupFile))
      {
        DirectoryInfo backupDir = new DirectoryInfo(BackupFolder);
        FileInfo[] filecount = backupDir.GetFiles(srcFile.Name + "*");
        int NextNum = filecount.GetUpperBound(0) + 1;
        BackupFile = String.Format("{0}\\{1}.{2}", BackupFolder, srcFile.Name, NextNum.ToString().PadLeft(4, '0'));
      }
      srcFile.MoveTo(BackupFile);
    }

    /// <summary>
    /// Use to process post translate actions
    /// </summary>
    private void TranslateComplete()
    {
        //US & CA are both in the same path
        countryConfig = new CountryConfig("0");
        //Get a complete list of temp folders.
        DirectoryInfo tmpInfo = new DirectoryInfo(base.TranslatedFileLocation + "\\" + countryConfig.SaveDirectoryLocation);

        List<DirectoryInfo> tmpdirs = new List<DirectoryInfo>();

        if (Directory.Exists(tmpInfo.FullName))
            tmpdirs = tmpInfo.GetDirectories("*.TMP", SearchOption.TopDirectoryOnly).ToList();

        //Consignment
        countryConfig = new CountryConfig("2");
        //Get a complete list of temp folders.
        tmpInfo = new DirectoryInfo(base.TranslatedFileLocation + "\\" + countryConfig.SaveDirectoryLocation);

        if (Directory.Exists(tmpInfo.FullName))
            tmpdirs.AddRange(tmpInfo.GetDirectories("*.TMP", SearchOption.TopDirectoryOnly).ToList());


        foreach (DirectoryInfo dirInfo in tmpdirs)
        {
            //If a directory exists that wasn't created on this run of the translate store it to keep files from not being fed into auditworks.
            if (!OutputDirectoriesCreated.Contains(dirInfo.FullName))
                OutputDirectoriesCreated.Add(dirInfo.FullName);
        }



        foreach (string dir in OutputDirectoriesCreated)
        {
            if (!Directory.Exists(dir))
                continue;

            //Rename the temp file to the extension required by auditworks
            string NewDir = dir.Replace(".TMP", ".IP");
            //Create the Done file requred by auditworks
            try
            {
                LogMessage(String.Format(ResourceHelper.Instance.GetString(ResFile, "CreatingDoneFile"), dir));
                using (File.Create(String.Format("{0}\\{1}", dir, "DONE.DONE")))
                {
                }
            }
            catch (Exception ex)
            {
                LogMessage(String.Format(ResourceHelper.Instance.GetString(ResFile, "DoneFileError"), ex.Message));
                continue;
            }

            try
            {
                LogMessage(String.Format(ResourceHelper.Instance.GetString(ResFile, "RenameDir"), dir, NewDir));
                System.IO.Directory.Move(dir, NewDir);
            }
            catch (Exception ex)
            {
                LogMessage(String.Format(ResourceHelper.Instance.GetString(ResFile, "RenameDirError"), ex.Message));
            }

        }
        System.Threading.Thread.Sleep(3000);
        LogMessage(ResourceHelper.Instance.GetString(ResFile, "AppDone"));
        System.Threading.Thread.Sleep(2000);
    }

    /// <summary>
    /// Load a list of stores to exlude from translating
    /// </summary>
    /// <exception cref="System.IO.FileNotFoundException"></exception>
    private void LoadConsignmentStores()
    {
      if (ConsignmentStoreFile == null)
        ConsignmentStoreFile = new List<string>();

      string ConsignmentFile = PluginConfig.GetValue("ConsignmentStoreFile");

      if (!File.Exists(ConsignmentFile))
        throw new FileNotFoundException(ResourceHelper.Instance.GetString(ResFile,"NoConsignmentStoreFile"));

      string[] ConsignmentStores = File.ReadAllLines(ConsignmentFile);
      for (int i = 0; i < ConsignmentStores.Length; i++)
      {
        string store = ConsignmentStores[i];
        ConsignmentStoreFile.Add(store.Substring(0, 5));
      }

    }

    /// <summary>
    /// Determines if the supplied store is a consignment store or not
    /// </summary>
    /// <param name="FileName">File to check</param>
    /// <returns>True if the file is a consignment storeCountryCode</returns>
    private bool isConsignmentStore(string FileName)
    {
      FileName = FileName.ToLower().Replace("tlog", "");
      if (ConsignmentStoreFile.Contains(FileName.Substring(0,5)))
        return true;
      else
        return false;
    }

    /// <summary>
    /// Use to log our exceptions.
    /// </summary>
    /// <param name="info">Property info of the exception to log</param>
    /// <param name="Exception">The exception that we are logging</param>
    private void LogException(PropertyInfo[] info, object Exception)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("Exception While Translating File\n");
      foreach (PropertyInfo prop in info)
      {
        sb.Append(prop.Name + ": " + prop.GetValue(Exception, null));
        sb.Append("\r\n");
      }
      LogMessage(sb.ToString());
    }

    private void InitializeThreadPool()
    {      
      work = new WorkQueue();
      ((WorkThreadPool)work.WorkerPool).MaxThreads = Int32.Parse(PluginConfig.GetValue("MaxThreads"));
      ((WorkThreadPool)work.WorkerPool).MinThreads = Int32.Parse(PluginConfig.GetValue("MinThreads"));
      work.ConcurrentLimit = Int32.Parse(PluginConfig.GetValue("ConcurrentThreads"));
      work.AllWorkCompleted += new EventHandler(work_AllWorkCompleted);
      work.WorkerException += new ResourceExceptionEventHandler(work_WorkerException);
      work.FailedWorkItem += new WorkItemEventHandler(work_FailedWorkItem);      
    }

    void work_FailedWorkItem(object sender, WorkItemEventArgs e)
    {
      LogException(e.WorkItem.FailedException.GetType().GetProperties(),e.WorkItem.FailedException);
    }

    void work_WorkerException(object sender, ResourceExceptionEventArgs e)
    {
      LogException(e.Exception.GetType().GetProperties(),e.Exception);
    }

    void work_AllWorkCompleted(object sender, EventArgs e)
    {      
      TranslateComplete();      
      Console.WriteLine("Work Done");      
      Environment.Exit(0);      
    }

    static void WritePolledNetSales(string storeNum, string Amount, DateTime TransDate)
    {
      NetSales sales = new NetSales();
      sales.StoreNum = storeNum;
      sales.NetSalesAmount = Amount;
      sales.TransDate = TransDate;
      ThreadPool.QueueUserWorkItem(new WaitCallback(WritePolledNetSalesAsync), sales);
    }

    static void WritePolledNetSalesAsync(object SalesAmount)
    {
      //No need to continue if a file is not specified to write to.
      if (String.IsNullOrEmpty(PolledNetSalesFile))
        return;

      //Write the polled sales to file
      //Lock our static object to prevent threads from trying to write to the file at the same time which would cause an exception      
      lock (PolledFileWriteLock)
      {
        try
        {
          NetSales sales = (NetSales)SalesAmount;
          using (FileStream fstream = new FileStream(PolledNetSalesFile, FileMode.Append, FileAccess.Write))
          {
            using (StreamWriter writer = new StreamWriter(fstream))
              writer.WriteLine(sales.StoreNum + "\t" + sales.TransDate.ToString("MM/dd/yyyy") + "\t" + sales.NetSalesAmount.PadLeft(9, '0'));
          }
        }
        catch
        {
        }
      }
    }

    private struct NetSales
    {
      public string StoreNum;
      public string NetSalesAmount;
      public DateTime TransDate;
    }
  }
}
