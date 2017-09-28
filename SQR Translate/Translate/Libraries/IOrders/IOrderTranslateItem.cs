using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Net.Mail;


namespace SpencerGifts.Translate.Plugin.TLog.IOrders
{
  public class IOrderTranslateItem : TranslateItem
  {
    /// <summary>
    /// Source file to be translated
    /// </summary>
    string _SourceFile = "";
    /// <summary>
    /// MemoryStream used to hold the converted source document
    /// </summary>
    private MemoryStream _SourceDocumentMS;
    /// <summary>
    /// Holds locatation to the orderheaderfile
    /// </summary>
    string _OrderHeaderFile = "";
    /// <summary>
    /// location to the orderdetail file
    /// </summary>
    string _OrderDetailFile = "";
    /// <summary>
    /// location to the orderpaymentfile
    /// </summary>
    string _OrderPaymentFile = "";
    /// <summary>
    /// used to determine if the transaction footer has been found
    /// </summary>
    bool FooterFound = false;
    /// <summary>
    /// holds the total $ amount of the transaction being translated
    /// </summary>
    double TransactionTotal = 0;
    /// <summary>
    /// holds the total $ discount amount of the transaction being translated
    /// </summary>
    double DiscountAmount = 0;
    bool isSaleTransaction;
    /// <summary>
    /// holds the total $ shipping amount of the transaction being translated
    /// </summary>
    double Shipping = 0;
    double TaxableShipping = 0;
    /// <summary>
    /// holds the total $ handling amount of the transaction being translated
    /// </summary>
    double Handling = 0;
    /// <summary>
    /// used to hold the current tlog line items for the current transaction
    /// </summary>
    List<XmlNode> TlogLineItems;
  
    int _TransactionNo;
    int _RegisterNo;
    StoreRegisterInfo storeRegInfo;
    XmlDocument SkuFile;


    public IOrderTranslateItem()
    {
      TlogLineItems = new List<XmlNode>();
      OnBeforeNodeAppended += new EventHandler(TLogTranslateItem_OnBeforeNodeAppended);
      OnBeforeRuleProcess += new EventHandler(TLogTranslateItem_OnBeforeRuleProcess);
      OnPreProcess += new PreProcessEventHandler(IOrderTranslateItem_OnPreProcess);      
    }

    #region Event Handlers
    
    void IOrderTranslateItem_OnPreProcess()
    {
      LogMessage("-------------------------------Translate Started-------------------------------");

      TranslateSupport support = new TranslateSupport(PluginConfig);
			
      SkuFile = support.SkuList();

      string TempFolder = PluginConfig.GetValue("TemporaryWorkFolder");      

      _OrderHeaderFile = TempFolder + "\\OrderHeaderMerge";
      _OrderDetailFile = TempFolder + "\\OrderDetailMerge";
      _OrderPaymentFile = TempFolder + "\\OrderPaymentMerge";

      LogMessage("Removing old files.");
      if (File.Exists(_OrderHeaderFile)) File.Delete(_OrderHeaderFile);
      if (File.Exists(_OrderDetailFile)) File.Delete(_OrderDetailFile);
      if (File.Exists(_OrderPaymentFile)) File.Delete(_OrderPaymentFile);

      DirectoryInfo dirinfo = new DirectoryInfo(base.SourceFileLocation);

      //if email field is left blank this check is skipped
      if (!String.IsNullOrEmpty(PluginConfig.GetValue("DuplicateFilesEmail")))
      {
        //Remove any files from the download directory if they have already been processed.  Reprocessing those files will cause problems if sent to SIRIS.
        List<string> FilesRemoved = new List<string>();
        List<string> FilesKept = new List<string>();
        TranslateSupport.RemoveProcessedFiles(dirinfo.GetFiles(), PluginConfig.GetValue("WebLincBackupFolder"), out FilesRemoved, out FilesKept);
        if (FilesRemoved.Count != 0)
        {
          StringBuilder Body = new StringBuilder();
          Body.Append("Processing of the following files was skipped because they have been previously processed. <BR>");
          foreach (string file in FilesRemoved)
            Body.Append(file + "<BR>");

          Body.Append("____________________________________________________________<BR>");
          if (FilesKept.Count > 0)
          {
            Body.Append("The following files were still processed<BR>");
            foreach (string file in FilesKept)
              Body.Append(file + "<BR>");
          }
          else
            Body.Append("NO other files were processed.<BR>");

          SendEmail("IOrders@spencergifts.com", PluginConfig.GetValue("DuplicateFilesEmail").Split(';'), "Duplicate Internet Order Files Found", Body.ToString(), null);
          LogMessage(Body.ToString().Replace("<BR>", "\r\n"));

          dirinfo = new DirectoryInfo(base.SourceFileLocation);
        }
      }

      
      LogMessage("Merging all OrderHeader files");
      TranslateSupport.MergeFiles(dirinfo.GetFiles("*orderheader*.*"), _OrderHeaderFile);
      LogMessage("Merging all OrderDetail files");
      TranslateSupport.MergeFiles(dirinfo.GetFiles("*orderdetail*.*"), _OrderDetailFile);
      LogMessage("Merging all OrderPayment files");
      TranslateSupport.MergeFiles(dirinfo.GetFiles("*orderpayment*.*"), _OrderPaymentFile);
      
    }

    void TLogTranslateItem_OnBeforeRuleProcess(object sender, EventArgs e)
    {
      XmlDocument doc = (XmlDocument)sender;
      System.Xml.XPath.XPathNavigator nav = doc.CreateNavigator();
      Shipping = doc.SelectSingleNode("CustomerOrder/Customer/@Shipping_Price") == null ? 0 : Convert.ToDouble(doc.SelectSingleNode("CustomerOrder/Customer/@Shipping_Price").Value);
      TaxableShipping = doc.SelectSingleNode("CustomerOrder/Customer/@Taxable_Shipping_Amount") == null ? 0 : Convert.ToDouble(doc.SelectSingleNode("CustomerOrder/Customer/@Taxable_Shipping_Amount").Value);
      Handling = Convert.ToDouble(nav.Evaluate("sum(//*/@Handling_Charge)"));
    }

		void TLogTranslateItem_OnBeforeNodeAppended(object sender, EventArgs e)
		{
			FooterFound = false;
			XmlNode NodeToAppend = (XmlNode)sender;
			if (storeRegInfo == null || storeRegInfo.StoreNo != Int32.Parse(NodeToAppend.Attributes["store_num"].Value))
			{
				if (storeRegInfo != null)
					storeRegInfo.SaveTransactionInfo();

				storeRegInfo = new StoreRegisterInfo(Int32.Parse(NodeToAppend.Attributes["store_num"].Value), PluginConfig.GetValue("StoreRegTranConfig"));
			}

			switch (NodeToAppend.Name)
			{
				case "L10":
					InitializeCounter(1);
					//reset transaction total amounts
					TransactionTotal = 0;
					DiscountAmount = 0;
					Handling = 0;
					isSaleTransaction = NodeToAppend.Attributes["type"].Value == "0";
					_TransactionNo = storeRegInfo.NextTransactionID;
					_RegisterNo = storeRegInfo.CurrentRegisterNumber;
					NodeToAppend.Attributes["country"].Value = storeRegInfo.CountryCode.ToString();
					break;
				case "L30":
					//update the transaction total amount excluding the shipping and handling charge on a refund
					if (NodeToAppend.Attributes["item"].Value != "000000000001")
						TransactionTotal += (Convert.ToDouble(NodeToAppend.Attributes["extended"].Value));

					//Set the journal key based off the SIRIS file provided
					string Sku = NodeToAppend.Attributes["item"].Value;

					if (Sku.Length >= 8)
						Sku = Sku.Substring(Sku.Length - 8);

					XmlNode FoundNode = SkuFile.SelectSingleNode(String.Format("//SkuList/Sku[@SkuNum='{0}']", Sku));
					if (FoundNode != null)
					{
						int JournalKey = Convert.ToInt32(FoundNode.Attributes["Journal_Key"].Value);
						NodeToAppend.Attributes["journal_key"].Value = JournalKey.ToString();
					}
					break;
				case "L31":
					//update the discount amount for the transaction
					DiscountAmount += Math.Abs(Convert.ToDouble(NodeToAppend.Attributes["discount"].Value)) * -1;
					break;
				case "L32":
					DiscountAmount += Math.Abs(Convert.ToDouble(NodeToAppend.Attributes["grupdisc"].Value)) * -1;
					break;
				//case "L44":
				//case "L47":
				//  NodeToAppend.Attributes["account"].Value = NodeToAppend.Attributes["account"].Value.Replace('X', '0');
				//  break;
				case "L70":
					if (NodeToAppend.Attributes["field_type"].Value == "1")
					{
						var split = NodeToAppend.Attributes["field"].Value.Split(' ');
						if (NodeToAppend.Attributes["field"].Value.IndexOf(" ") > 0)
							NodeToAppend.Attributes["field"].Value = NodeToAppend.Attributes["field"].Value.Split(' ')[1];
						else
							NodeToAppend.Attributes["field"].Value = "";
					}
					if (NodeToAppend.Attributes["field_type"].Value == "2")
						if (NodeToAppend.Attributes["field"].Value.IndexOf(" ") > 0)
							NodeToAppend.Attributes["field"].Value = NodeToAppend.Attributes["field"].Value.Split(' ')[0];
						else
							NodeToAppend.Attributes["field"].Value = "";
					break;
				case "L99":
					//double total = (TransactionTotal - DiscountAmount) + Shipping + Handling + Convert.ToDouble(NodeToAppend.Attributes["taxes"].Value);
					double total = 0;
					if (TransactionTotal >= 0)
						total = (TransactionTotal - Math.Abs(DiscountAmount)) + Shipping + TaxableShipping + Handling + Convert.ToDouble(NodeToAppend.Attributes["taxes"].Value);
					if (TransactionTotal < 0)
						total = (TransactionTotal + Math.Abs(DiscountAmount)) + Shipping + TaxableShipping + Handling + Convert.ToDouble(NodeToAppend.Attributes["taxes"].Value);

					//add the gross amount of the transaction to the trailer record
					NodeToAppend.Attributes["gross"].Value = total.ToString();
					FooterFound = true;
					break;
				default:
					IncrementCounter();
					break;
			}
			NodeToAppend.Attributes["trans_num"].Value = _TransactionNo.ToString();
			NodeToAppend.Attributes["reg_num"].Value = _RegisterNo.ToString();
		}

    #endregion

    /// <summary>
    /// Overide the AppendNode function to hold all line items until we get a 99 record
    /// </summary>
    /// <param name="Parent"></param>
    /// <param name="NodeToAppend"></param>
    protected override void AppendNode(XmlElement Parent, XmlNode NodeToAppend)
    {
      //add the current node to append to the output document to the collection
      TlogLineItems.Add(NodeToAppend);

      //check to see if we have a footer (99 Record) line type yet.
      //This check is done in the TLogTranslateItem_OnBeforeNodeAppended event
      if (FooterFound)
      {        
        //running count of all line items in the transaction
        int RecSeq = 1;
        //Sequence number for the current transaction
        int Seq_Num = 0;
        //Sequence number for L44 Credit Card line
        int Seq_Num_44 = 0;
        //Line items that do not have a sequence number
        string[] InValidSeqNums = new string[] {"L10","L99","L44","L70","L71"};
        List<string> SeqNumByPass = new List<string>();
        //add sequence number to a collection for faster searching
        SeqNumByPass.AddRange(InValidSeqNums);
       
        for (int i = 0; i < TlogLineItems.Count; i++)
        {
          XmlNode currentNode = TlogLineItems[i];
          string NodeName = currentNode.Name;
                 
          if (NodeName == "L10")
          {
            //update the 10 record with the totoal line items
            currentNode.Attributes["rec_count"].Value = TlogLineItems.Count.ToString();
            currentNode.Attributes["db_rec_count"].Value = TlogLineItems.Count.ToString();
          }
          //update the record sequence number for all items
          currentNode.Attributes["rec_seq"].Value = RecSeq.ToString();

          //update the record sequence number
          if (!SeqNumByPass.Contains(NodeName) && NodeName != "L44")
          {
            currentNode.Attributes["seq_num"].Value = Seq_Num.ToString();
            Seq_Num++;
          }

          //update the tender record sequence nmber
          if (NodeName == "L44")
          {
            currentNode.Attributes["seq_num"].Value = Seq_Num_44.ToString();
            Seq_Num_44++;
          }
          if (NodeName == "L44")
            currentNode.Attributes["seq_num"].Value = Seq_Num_44.ToString();

          //add the node to the output document
          base.AppendNode(Parent, currentNode);
          RecSeq++;
        }        
        TlogLineItems.Clear();
      }
      
    }   

    public override XmlReader SourceDocument
    {
      get
      {
        if (_SourceDocumentMS == null)
        {
         _SourceDocumentMS = new MemoryStream();
         TranslateSupport support = new TranslateSupport(PluginConfig,_OrderHeaderFile,_OrderDetailFile, _OrderPaymentFile);

         support.OnSendMessage += new TranslateSupport.SendMessageHandler(support_OnSendMessage);
         _SourceDocumentMS = support.CreateCustomerFile();

         //using (FileStream stream = new FileStream("c:\\users\\stein\\desktop\\merge.xml", FileMode.Create))
         //{
         //  _SourceDocumentMS.Position = 0;
         //  _SourceDocumentMS.WriteTo(stream);
         //  stream.Flush();
         //}
         LogMessage("Translating Files");
        }
        _SourceDocumentMS.Position = 0;
        XmlReaderSettings settings = new XmlReaderSettings();
        settings.CheckCharacters = false;
      
        return XmlReader.Create(_SourceDocumentMS,settings);
      }
    }
    //Used to log messages that come from the support class.
    void support_OnSendMessage(string Message)
    {
      LogMessage(Message);
    }

    public override string SourceFile
    {
      get { return _SourceFile; }
      set { _SourceFile = value; }
    }

    protected override void SaveTranslatedFile(XmlDocument TranslatedDocument)
    {
      LogMessage("Creating Totals Report");
      XmlDocument ConvertedSourceDoc = new XmlDocument();
      ConvertedSourceDoc.Load(SourceDocument);
      SendTotalsReport(TranslateSupport.GenerateSummaryReport(ConvertedSourceDoc,TranslatedDocument));
     
      XmlNodeList list = TranslatedDocument.SelectNodes("//" + NewDocumentRoot + "/*");

      //// create a writer and open the file      
      string OutFile = PluginConfig.GetValue("TemporaryWorkFolder") + "\\translated.tmp";      
      using (TextWriter tw = new StreamWriter(OutFile, false))
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
       if (storeRegInfo != null)
        storeRegInfo.SaveTransactionInfo();

      //Do out translate clean up tasks here
      SplitTranslatedFile(OutFile, 1);      
      CopyToSiris();      
      MoveFilesToBackup();
      LogMessage("-------------------------------Translate Complete-------------------------------");
      
    }

    /// <summary>
    /// Splits out the translated file into seperate files based off the store number
    /// </summary>
    /// <param name="FileName">FileName to split</param>
    /// <param name="ColDelimeter">The column in the file to use as the distinc seperator of files</param>
    private void SplitTranslatedFile(string FileName, int ColDelimeter)
    {
      string CurrentLine;
      string[] splitLine;
      int StoreLine;
      StringBuilder sbFile = new StringBuilder();
      StreamWriter writer;
      string DirToSave = PluginConfig.GetValue("TranslatedFileLocation");
      string FileToSave = "";
      int TotalFiles = 0;

      using (FileStream stream = File.OpenRead(FileName))
      {
        using (StreamReader reader = new StreamReader(stream))
        {
          int CurrentStore = -1;
          while (reader.Peek() != -1)
          {
            CurrentLine = reader.ReadLine();
						if (String.IsNullOrEmpty(CurrentLine))
							continue;

            splitLine = CurrentLine.Split('\t');
            StoreLine = Convert.ToInt32(splitLine[ColDelimeter]);
            if (CurrentStore == -1)
              CurrentStore = StoreLine;

            if (StoreLine == CurrentStore)
            {
              sbFile.Append(CurrentLine + "\r\n");
            }
            else
            {
              LogMessage(String.Format("Saving Tlog for store {0} to polling server ({1})",CurrentStore.ToString(),DirToSave));
              FileToSave = "TLog" + CurrentStore.ToString().PadLeft(5, '0');
              TotalFiles = Directory.GetFiles(DirToSave, FileToSave + "*").Length;
              if (TotalFiles > 0)
                FileToSave = FileToSave + "." + TotalFiles.ToString().PadLeft(4, '0');

              writer = new StreamWriter(DirToSave + "\\" + FileToSave);
              writer.WriteLine(sbFile.ToString().TrimEnd("\r\n".ToCharArray()));
              writer.Close();
              sbFile = new StringBuilder();
              sbFile.Append(CurrentLine + "\r\n");
              CurrentStore = StoreLine;
            }

          }
          LogMessage(String.Format("Saving Tlog for store {0} to polling server ({1})", CurrentStore.ToString(), DirToSave));
          FileToSave = "TLog" + CurrentStore.ToString().PadLeft(5, '0');
          TotalFiles = Directory.GetFiles(DirToSave, FileToSave + "*").Length;
          if (TotalFiles > 0)
            FileToSave = FileToSave + "." + TotalFiles.ToString().PadLeft(4, '0');

          //writer = new StreamWriter(PluginConfig.GetValue("TranslatedFileLocation") + "\\TLog" + CurrentStore.ToString().PadLeft(5, '0'));
          writer = new StreamWriter(DirToSave + "\\" + FileToSave);
          writer.WriteLine(sbFile.ToString().TrimEnd("\r\n".ToCharArray()));
          writer.Close();
        }
      }
    }

    /// <summary>
    /// Backs up weblinc files
    /// </summary>
    private void MoveFilesToBackup()
    {
      DirectoryInfo dirinfo = new DirectoryInfo(base.SourceFileLocation);
      FileInfo[] files = dirinfo.GetFiles("order*.*");
      string BackupLocation = PluginConfig.GetValue("WebLincBackupFolder");
      LogMessage(String.Format("Backing up files. ({0})",BackupLocation));
      foreach (FileInfo file in files)
      {
        if (!System.IO.File.Exists(BackupLocation + "\\" + file.Name))
          file.MoveTo(BackupLocation + "\\" + file.Name);
        else
          file.Delete();
      }


      //Backup the store reg info file
      //We don't move it because it is needed on every run of the translate.
      FileInfo StoreRegInfoFile = new FileInfo(PluginConfig.GetValue("StoreRegTranConfig"));
      StoreRegInfoFile.CopyTo(PluginConfig.GetValue("WebLincBackupFolder") + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "." + StoreRegInfoFile.Name);      
    }

    /// <summary>
    /// Copies our merged files from weblinc over to the siris ftp directories
    /// </summary>
    private void CopyToSiris()
    {
      string SirisDir = PluginConfig.GetValue("SIRISFTP");
      string FileTime = DateTime.Now.ToString("yyyyMMddHHmmss");

      //Write out as ASCII so that any non standard characters get converted to question marks to account for a bug with IBM not being able to handle them.
      CopyAsASCII(_OrderDetailFile, GetSaveFile(String.Format("{0}\\OrderDetail", SirisDir), "OrderDetail", FileTime));
      CopyAsASCII(_OrderDetailFile, GetSaveFile(String.Format("{0}\\OrderDetail\\archive", SirisDir), "OrderDetail", FileTime));
      CopyAsASCII(_OrderHeaderFile, GetSaveFile(String.Format("{0}\\OrderHeader", SirisDir), "OrderHeader", FileTime));
      CopyAsASCII(_OrderHeaderFile, GetSaveFile(String.Format("{0}\\OrderHeader\\archive", SirisDir), "OrderHeader", FileTime));
      LogMessage(String.Format("Copied OrderHeader and OrderDetail to SIRIS ({0})", SirisDir));

      
      //Write out as ASCII so that any non standard characters get converted to question marks to account for a bug with IBM not being able to handle them.
      StreamReader paymentReader = new StreamReader(_OrderPaymentFile, System.Text.Encoding.ASCII);
      string OrderPaymentDir = String.Format("{0}\\OrderPayment", SirisDir);
      string OrderPaymentFile = "OrderPayment";

      string PaymentSaveFile = GetSaveFile(OrderPaymentDir, OrderPaymentFile, FileTime);

      FileStream paymentWStream = new FileStream(PaymentSaveFile, FileMode.Create);
      StreamWriter paymentWriter = new StreamWriter(paymentWStream);

      while (paymentReader.Peek() != -1)
      {
        string CurrentLine = paymentReader.ReadLine();
        //Remove the hashed account from the file that goes to SIRIS because they have no use for it.

        CurrentLine = CurrentLine.Substring(0, 631);


        if (String.IsNullOrEmpty(CurrentLine.Trim()))
          continue;

        string HashedAccount = CurrentLine.Substring(602, 20);
        CurrentLine = CurrentLine.Replace(HashedAccount, "".PadLeft(HashedAccount.Length, 'X'));

        string CCType = CurrentLine.Substring(469, 4);
        if (CCType == "GIFT")
        {
          paymentWriter.WriteLine(CurrentLine);
          continue;
        }

        string CCRecord = CurrentLine.Substring(473, 24);
        int CCLen = CCRecord.Trim().Length;

        if (CCLen > 0 && CCLen > 4)
        {
          string MaskedCC = CCRecord.Substring(CCLen - 4).PadLeft(CCRecord.Length, 'X');
          CurrentLine = CurrentLine.Replace(CCRecord, MaskedCC);
        }

        if (!String.IsNullOrEmpty(CurrentLine))
          paymentWriter.WriteLine(CurrentLine);
      }
      paymentWriter.Flush();      
      paymentReader.Close();
      paymentWStream.Close();      
      //Copy new file to archive folder
      File.Copy(PaymentSaveFile, GetSaveFile(String.Format("{0}\\archive", OrderPaymentDir), OrderPaymentFile, FileTime));
      LogMessage(String.Format("Copied OrderPayment File to SIRIS and masked CC info. ({0})", SirisDir));
    }

    private static void CopyAsASCII(string source, string destination)
    {
      using (StreamReader FileReader = new StreamReader(source, System.Text.Encoding.ASCII))
      {
        using (FileStream fsDestination = new FileStream(destination, FileMode.Create))
        {
          using (StreamWriter paymentWriter = new StreamWriter(fsDestination, Encoding.ASCII))
            paymentWriter.WriteLine(FileReader.ReadToEnd().Trim());
        }
      }
    }

    protected override void Process()
    {
      FileInfo headerFileInfo = new FileInfo(_OrderHeaderFile);
      if (!headerFileInfo.Exists || headerFileInfo.Length == 0)
      {
     
        //if there are no addresses specified no need to continue sending the message.
        if (String.IsNullOrEmpty(PluginConfig.GetValue("NoOrdersEmail").Trim()))
          return;

        SendEmail("IOrders@spencergifts.com", PluginConfig.GetValue("NoOrdersEmail").Split(';'), "No Internet Orders", "There were no orders to process on " + DateTime.Now.ToString("MM/dd/yyyy"),null);

        LogMessage("No Orders to process");
        LogMessage("-------------------------------Translate Complete-------------------------------");
        return;
      }
      try
      {
        base.Process();
        Environment.ExitCode = 0;
      }
      catch (Exception ex)
      {
        LogMessage("Unable to process internet orders \r\n Message: " + ex.Message + "\r\nStackTrace: " + ex.StackTrace);

        if (ex.Message != "No valid records to translate")
          Environment.ExitCode = 1;
      }
      
    }

    void SendTotalsReport(FileInfo rpt)
    {
      //if there are no addresses specified no need to send email
      if (String.IsNullOrEmpty(PluginConfig.GetValue("TotalsReportEmail").Trim()))
        return;

      SendEmail("IOrders@spencergifts.com", PluginConfig.GetValue("TotalsReportEmail").Split(';'), "INTERNET ORDER COUNTS", "Attached are the counts from the internet files processed on " + DateTime.Now.ToString("MM/dd/yyyy"), rpt.FullName);
    }

    string GetSaveFile(string Directory, string FileName, string FileTime)
    {
      DirectoryInfo info = new DirectoryInfo(Directory);
      if (info.GetFiles(FileName + "*").Length > 0 || Directory.ToLower().Contains("archive"))
        FileName = String.Concat(FileName, ".", FileTime, ".txt");
      else
        FileName = String.Concat(FileName + ".txt");

      return Directory + "\\" + FileName;
    }


    private void SendEmail(string From, string[] To, string Subject, string Body, string AttachmentName)
    {      
      MailMessage msg = new MailMessage();
      msg.Subject = Subject;
      msg.Body = Body;
      msg.From = new MailAddress(From);
      msg.IsBodyHtml = true;
      if (!String.IsNullOrEmpty(AttachmentName))
        msg.Attachments.Add(new Attachment(AttachmentName));
      
      foreach (String addr in To)
        msg.To.Add(addr);

      try
      {
        SmtpClient smtp = new SmtpClient(PluginConfig.GetValue("MailServer"));
        smtp.Send(msg);
      }
      catch (Exception ex)
      {
        LogMessage("Unable to send email " + Subject + ". Check to make sure the \"MailServer\" value exists in the config section and is correct.\r\n" + ex.Message + "\r\n" + ex.StackTrace);
      }
    }


  }
}
