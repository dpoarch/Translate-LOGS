using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using SpencerGifts.Translate.Configuration;
using System.Text;


namespace SpencerGifts.Translate.Plugin.TLog.IOrders
{
  internal sealed class TranslateSupport
  {
    /// <summary>
    /// Stream used to hold the header file
    /// </summary>
    MemoryStream msHeader = new MemoryStream();
    /// <summary>
    /// Stream used to hold detail file
    /// </summary>
    MemoryStream msDetail = new MemoryStream();
    /// <summary>
    /// stream used to hold payment file
    /// </summary>
    MemoryStream msPayment = new MemoryStream();
    /// <summary>
    /// Used to hold the current plugin config
    /// </summary>
    TranslateConfig PluginConfig;
    /// <summary>
    /// Holds the location of the OrderHeaderFile
    /// </summary>
    string _OrderHeaderFile = "";
    /// <summary>
    /// Holds the location of the OrderDetailFile
    /// </summary>
    string _OrderDetailFile = "";
    /// <summary>
    /// Holds the location of the OrderPaymentFile
    /// </summary>
    string _OrderPaymentFile = "";
    static List<string> TotalStores;

    public TranslateSupport(TranslateConfig Config, string OrderHeaderFile, string OrderDetailFile, string OrderPaymentFile)
    {
      PluginConfig = Config;
      TotalStores = new List<string>();
      _OrderDetailFile = OrderDetailFile;
      _OrderHeaderFile = OrderHeaderFile;
      _OrderPaymentFile = OrderPaymentFile;
    }

    //Used to bubble event messages back to the calling class for logging.
    public delegate void SendMessageHandler(string Message);
    public event SendMessageHandler OnSendMessage;

    public TranslateSupport(TranslateConfig Config)
    {
      PluginConfig = Config;
    }

    private void SendMessage(string Message)
    {
      if (OnSendMessage != null)
        OnSendMessage(Message);
    }

    #region Methods to generate XML Template
    /// <summary>
    /// Takes the text order header file and converts it to an xml file and stores it in a memory stream for later use
    /// </summary>
    private void LoadHeader()
    {
      TotalStores.Clear();
      SendMessage("Loading Merged Order Header File");
      //System.IO.DirectoryInfo DirInfo = new System.IO.DirectoryInfo(base.SourceFileLocation);
      XmlDocument CustomerTemplate = new XmlDocument();
      CustomerTemplate.Load(PluginConfig.GetValue("IOrderHeaderTemplateFile"));
      XmlElement root = CustomerTemplate.DocumentElement;

      List<HeaderLineItem> SortedHeader = GetSortHeaderFile();
      XmlWriterSettings settings = new XmlWriterSettings();
      settings.Indent = true;
      settings.NewLineHandling = NewLineHandling.Entitize;
      XmlWriter writer = XmlWriter.Create(msHeader, settings);
      writer.WriteStartDocument();
      writer.WriteStartElement("IOrders");

      foreach (HeaderLineItem line in SortedHeader)
      {
        string CurrentLine = line.LineData;
        if (String.IsNullOrEmpty(CurrentLine.Trim()))
          continue;

        //check to make sure the lenght of the line being processed matches the spec
        if (CurrentLine.Length < Convert.ToInt32(root.ChildNodes[root.ChildNodes.Count - 1].Attributes["ColumnEnd"].Value))
          throw new IndexOutOfRangeException(String.Format("Unable to process the merged OrderHeader File {0} \r\n The length of the line: {1} \r\n is less than the expected length of {2}", _OrderHeaderFile, CurrentLine,root.ChildNodes[root.ChildNodes.Count - 1].Attributes["ColumnEnd"].Value));

        
        writer.WriteStartElement("CustomerOrder");
        int StoreStart = Int32.Parse(root.SelectSingleNode("//Division").Attributes["ColumnStart"].Value)-1;
        int StoreEnd = Int32.Parse(root.SelectSingleNode("//Division").Attributes["ColumnEnd"].Value);
        string CurrentStore = CurrentLine.Substring(StoreStart, StoreEnd - StoreStart).Trim();
        //Keep track of store numbers in file for use later when creating totals report
        if (!TotalStores.Contains(CurrentStore)) TotalStores.Add(CurrentStore);

        writer.WriteAttributeString("StoreNo", CurrentStore);

        writer.WriteStartElement("Customer");
        for (int i = 0; i < root.ChildNodes.Count; i++)
        {
          XmlNode childnode = root.ChildNodes[i];
          int StartPos = Int32.Parse(childnode.Attributes["ColumnStart"].Value) - 1;
          int EndPos = Int32.Parse(childnode.Attributes["ColumnEnd"].Value);
          string CurrentValue = CurrentLine.Substring(StartPos, EndPos - StartPos).Trim();
          switch (childnode.Name)
          {
            case "Shipping_Price":
              CurrentValue = CurrentValue.Insert(CurrentValue.Length - 2, ".");
              break;
            case "Taxable_Shipping_Amount":
              CurrentValue = CurrentValue.Insert(CurrentValue.Length - 5, ".");
              break;
           
          }
          writer.WriteAttributeString(childnode.Name, CurrentValue);
        }
        writer.WriteEndElement();
        writer.WriteEndElement();
      }

      writer.WriteEndElement();
      writer.Flush();
      writer.Close();

      //FileStream stream = new FileStream(@"c:\header.xml", FileMode.OpenOrCreate, FileAccess.Write);
      //msHeader.Position = 0;
      //msHeader.WriteTo(stream);

    }

    private void LoadDetail()
    {
      SendMessage("Loading Merged Order Detail File");
      XmlDocument CustomerTemplate = new XmlDocument();
      CustomerTemplate.Load(PluginConfig.GetValue("IOrderDetailTemplateFile"));
      XmlElement root = CustomerTemplate.DocumentElement;

      using (FileStream fstream = new FileStream(_OrderDetailFile, FileMode.Open, FileAccess.Read))
      {
        using (StreamReader reader = new StreamReader(fstream, Encoding.Default))
        {
          XmlWriter writer = XmlWriter.Create(msDetail);
          writer.WriteStartDocument();
          writer.WriteStartElement("IOrders");
          while (reader.Peek() != -1)
          {
            string CurrentLine = reader.ReadLine();
            if (String.IsNullOrEmpty(CurrentLine.Trim()))
              continue;

            //check to make sure the lenght of the line being processed matches the spec
            if (CurrentLine.Length < Convert.ToInt32(root.ChildNodes[root.ChildNodes.Count - 1].Attributes["ColumnEnd"].Value))
              throw new IndexOutOfRangeException(String.Format("Unable to process the merged OrderDetail File {0} \r\n The length of the line: {1} \r\n is less than the expected length of {2}", _OrderDetailFile, CurrentLine, root.ChildNodes[root.ChildNodes.Count - 1].Attributes["ColumnEnd"].Value));


            writer.WriteStartElement("OrderDetail");
            for (int i = 0, count = root.ChildNodes.Count; i < count; i++)
            {
              XmlNode childnode = root.ChildNodes[i];
              int StartPos = Int32.Parse(childnode.Attributes["ColumnStart"].Value) - 1;
              int EndPos = Int32.Parse(childnode.Attributes["ColumnEnd"].Value);
              string CurrentValue = "";
              CurrentValue = CurrentLine.Substring(StartPos, EndPos - StartPos).Trim();

              switch (childnode.Name)
              {
                case "Handling_Charge":
                case "Line_Tax":
                case "MSRP":
                case "Unit_Price":
                case "Fee_Amount":
                case "CA_Shipping_Fee":
                  if (!String.IsNullOrEmpty(CurrentValue))
                    CurrentValue = CurrentValue.Insert(CurrentValue.Length - 5, ".");
                  break;
              }

              writer.WriteAttributeString(childnode.Name, CurrentValue);
            }
            writer.WriteEndElement();
          }
          writer.WriteEndElement();
          writer.Flush();
          writer.Close();
        }
      }
    }

    private void LoadPaymentDetail()
    {
      SendMessage("Loading Merged Order Payment File");
      XmlDocument CustomerTemplate = new XmlDocument();
      CustomerTemplate.Load(PluginConfig.GetValue("IOrderPaymentTemplateFile"));
      XmlElement root = CustomerTemplate.DocumentElement;

      using (FileStream fstream = new FileStream(_OrderPaymentFile, FileMode.Open, FileAccess.Read))
      {
        using (StreamReader reader = new StreamReader(fstream, Encoding.Default))
        {
          XmlWriterSettings settings = new XmlWriterSettings();
          settings.CheckCharacters = false;
          settings.Encoding = Encoding.Default;

          XmlWriter writer = XmlWriter.Create(msPayment,settings);
          writer.WriteStartDocument();
          writer.WriteStartElement("IOrders");
          while (reader.Peek() != -1)
          {
            string CurrentLine = reader.ReadLine();
            if (String.IsNullOrEmpty(CurrentLine.Trim()))
              continue;

            //check to make sure the lenght of the line being processed matches the spec
            if (CurrentLine.Length < Convert.ToInt32(root.ChildNodes[root.ChildNodes.Count - 1].Attributes["ColumnEnd"].Value))
              throw new IndexOutOfRangeException(String.Format("Unable to process the merged OrderPayment File {0} \r\n The length of the line: {1} \r\n is less than the expected length of {2}", _OrderDetailFile, CurrentLine, root.ChildNodes[root.ChildNodes.Count - 1].Attributes["ColumnEnd"].Value));


            writer.WriteStartElement("PaymentDetail");
            for (int i = 0, count = root.ChildNodes.Count; i < count; i++)
            {
              XmlNode childnode = root.ChildNodes[i];
              int StartPos = Int32.Parse(childnode.Attributes["ColumnStart"].Value) - 1;
              int EndPos = Int32.Parse(childnode.Attributes["ColumnEnd"].Value);
              string CurrentValue = CurrentLine.Substring(StartPos, EndPos - StartPos).Trim();
              switch (childnode.Name)
              {
                case "Amount":
                  CurrentValue = CurrentValue.Insert(CurrentValue.Length - 5, ".");
                  break;
              }

              writer.WriteAttributeString(childnode.Name, CurrentValue);
            }
            writer.WriteEndElement();
          }
          writer.WriteEndElement();
          writer.Flush();
          writer.Close();
        }
      }

      //FileStream stream = new FileStream(@"c:\payment.xml", FileMode.OpenOrCreate, FileAccess.Write);
      //msPayment.Position = 0;
      //msPayment.WriteTo(stream);
      //stream.Flush();

    }

    private List<HeaderLineItem> GetSortHeaderFile()
    {
      List<HeaderLineItem> headerList = new List<HeaderLineItem>();
      using (FileStream fstream = new FileStream(_OrderHeaderFile, FileMode.Open, FileAccess.Read))
      {
        using (StreamReader reader = new StreamReader(fstream, Encoding.Default))
        {
          while (reader.Peek() != -1)
          {
            HeaderLineItem lineItem = new HeaderLineItem();
            string CurrentLine = reader.ReadLine();
            if (String.IsNullOrEmpty(CurrentLine.Trim())) continue;

            int DateStartPos = 40;
            int DateEndPos = 54;
            int StoreStartPos = 10;
            int StoreEndPos = 15;
            object DateVal = Convert.ToInt64(CurrentLine.Substring(DateStartPos, DateEndPos - DateStartPos).Trim());
            DateVal = String.Format("{0:####-##-## ##:##:##}", DateVal);
            string StoreVal = CurrentLine.Substring(StoreStartPos, StoreEndPos - StoreStartPos).Trim();
            lineItem.LineData = CurrentLine;
            lineItem.TransactionDate = Convert.ToDateTime(DateVal);
            lineItem.StoreNo = Convert.ToInt32(StoreVal);
            headerList.Add(lineItem);
          }
        }
      }
      SpencerGifts.Translate.Plugin.TLog.IOrders.Comparer<HeaderLineItem> comparer = new Comparer<HeaderLineItem>();
      comparer.SortClasses.Add(new SortClass("StoreNo", SortDirection.Ascending));
      comparer.SortClasses.Add(new SortClass("TransactionDate", SortDirection.Ascending));
      headerList.Sort(comparer);
      return headerList;
    }

    /// <summary>
    /// Creates a single xml file consiting of the order header and related order details and payments
    /// </summary>
    /// <returns></returns>
    public MemoryStream CreateCustomerFile()
    {
      List<XmlElement> HeaderNodesToRemove = new List<XmlElement>();
      LoadHeader();
      LoadDetail();
      LoadPaymentDetail();
      //CreateSIRISReversalFiles();

      //get a listing of the reorder skus provied by siris
      XmlDocument Reorderdoc = GetReorderSkus();
      XmlDocument PaymentDetail = new XmlDocument();
      XmlDocument PaymentHeader = new XmlDocument();
      XmlDocument Payment = new XmlDocument();
      msDetail.Position = 0;
      msHeader.Position = 0;
      msPayment.Position = 0;
      PaymentDetail.Load(msDetail);
      PaymentHeader.Load(msHeader);
      Payment.Load(msPayment);

      XPathNavigator ReorderNav = Reorderdoc.DocumentElement.CreateNavigator();

      System.Collections.IEnumerator cust = PaymentHeader.DocumentElement.GetEnumerator();
      SendMessage("Merging Header/Detail/Payment Files and replacing any skus with the reorder sku");

      XPathExpression xprOrderNumber = XPathExpression.Compile("string(Customer/@Order_Number)");
      XPathExpression xprBatchNumber = XPathExpression.Compile("string(Customer/@Batch_Number)");
      XPathExpression xprStatus = XPathExpression.Compile("string(Customer/@Status)");
      XPathExpression xprTransId = XPathExpression.Compile("string(Customer/@Transaction_Type_Identifier)");

      int TotalRecords = PaymentHeader.DocumentElement.ChildNodes.Count;
      
      int counter = 0;
      int RecordCount = 0;
      //DateTime Start = DateTime.Now;
      while (cust.MoveNext())
      {
        //Console.WriteLine(DateTime.Now.Subtract(Start).Minutes + ":" + DateTime.Now.Subtract(Start).Seconds + " " + counter.ToString());
        if (counter % 100 == 0 && counter > 0)
          SendMessage(String.Format("Merged {0} of {1}", counter, TotalRecords));

        XmlElement current = (XmlElement)cust.Current;
        XPathNavigator PaymentNavigator = current.CreateNavigator();

        //string OrderNumber = current.SelectSingleNode("Customer/@Order_Number").Value;
        //string BatchNo = current.SelectSingleNode("Customer/@Batch_Number").Value;
        string OrderNumber = PaymentNavigator.Evaluate(xprOrderNumber).ToString();
        string BatchNo = PaymentNavigator.Evaluate(xprBatchNumber).ToString();
        string OrderStatus = PaymentNavigator.Evaluate(xprStatus).ToString();
        string TransTypeId = PaymentNavigator.Evaluate(xprTransId).ToString();

        if (OrderStatus == "RE")
        {
          SendMessage("Order is a Reship so skipping");
          continue;
        }
        if (OrderStatus == "SM" || OrderStatus == "ST")
        {         
          SendMessage("Order is a inventory transfer so skipping");
          HeaderNodesToRemove.Add(current);          
          continue;
        }
        
        RecordCount++;

        XmlNodeList PaymentDetailList = PaymentDetail.SelectNodes(string.Format("IOrders/OrderDetail[@Batch_Number='{0}'][@Order_Number = number('{1}')][@Transaction_Type_Identifier = '{2}']", BatchNo, OrderNumber, TransTypeId));
        //if (PaymentDetailList.Count > 0)
        //{
          foreach (XmlNode node in PaymentDetailList)
          {
            //locate the sku
            string sku = node.Attributes["UPC"].Value.Substring(7);
            //update the sku with the correct reorder sku
            string ReorderSku = ReorderNav.Evaluate(String.Format("string(Sku[@SKU_Num='{0}'][@Store_No='{1}']/@Reorder_Sku_Num)", sku, current.SelectSingleNode("//@Division").Value)).ToString();
            if (!String.IsNullOrEmpty(ReorderSku))
              node.Attributes["UPC"].Value = ReorderSku.PadLeft(15, '0');

            /*
             * REFUND DISCOUNT REMOVAL    
             * This is due to SIRIS's inability to properly read the discount
             */
            string Status = node.Attributes["Status"].Value.Trim();
            if (Status == "RF" || Status == "RD")
            {
              if (node.Attributes["MSRP"].Value.Trim() != node.Attributes["Unit_Price"].Value.Trim())
                node.Attributes["MSRP"].Value = node.Attributes["Unit_Price"].Value.Trim();
            }
            /**END REFUND DISCOUNT REMOVAL**/

            current.AppendChild(PaymentHeader.ImportNode(node, true));
          }
          //Load in the payments for the current order
          XmlNodeList PaymentList = Payment.SelectNodes(string.Format("IOrders/PaymentDetail[@Batch_Number='{0}'][@Order_Number = number('{1}')][@Transaction_Type_Identifier = '{2}']", BatchNo, OrderNumber, TransTypeId));
          if (PaymentList.Count > 0)
          {
            foreach (XmlNode node in PaymentList)
              current.AppendChild(PaymentHeader.ImportNode(node, true));
          }

          XmlNode FinalNode = PaymentHeader.CreateElement("OrderComplete");
          current.AppendChild(FinalNode);
          counter++;
        //}
        //else
          //SendMessage(String.Format("No Order details found for order# {0} Batch# {1}.  This will not be fed to the tlog.", OrderNumber, BatchNo));
      }
      XmlWriterSettings settings = new XmlWriterSettings { CheckCharacters = false };
      //settings.Encoding = Encoding.Default;

      MemoryStream ms = new MemoryStream();
      XmlWriter writer = XmlWriter.Create(ms,settings);

      foreach (XmlElement removeNode in HeaderNodesToRemove)
        PaymentHeader.DocumentElement.RemoveChild(removeNode);


      PaymentHeader.WriteTo(writer);
      writer.Flush();
     
      msDetail = null;
      msHeader = null;
      if (RecordCount > 0)
        return ms;
      else
        throw new Exception("No valid records to translate");
    }

    public void CreateSIRISReversalFiles()
    {
      if (PluginConfig.PluginName == "SpencerGifts.Translate.Plugin.TLog.IOrders.Reverse")
        return;

      ReverseFile(_OrderHeaderFile, 954);
      ReverseFile(_OrderDetailFile, 443);
      ReverseFile(_OrderPaymentFile, 555);  
    }


    private void ReverseFile(string File, int StatusStartPos)
    {
      string WorkFolder = PluginConfig.GetValue("SIRISReversalWorkFolder");
      StringBuilder sb = new StringBuilder();

      int BatchStartPos = 0;
      int BatchEndPos = 10;
      string BatchNumber = "";

      using (FileStream fstream = new FileStream(File, FileMode.Open, FileAccess.Read))
      {
        using (StreamReader reader = new StreamReader(fstream, Encoding.Default))
        {
          while (reader.Peek() != -1)
          {
            string CurrentLine = reader.ReadLine();
            if (String.IsNullOrEmpty(CurrentLine.Trim())) continue;

            if (String.IsNullOrEmpty(BatchNumber))
              BatchNumber = CurrentLine.Substring(BatchStartPos, BatchEndPos - BatchStartPos).Trim();

            int StatusEndPos = StatusStartPos + 2;

            string OriginalValue = CurrentLine.Substring(StatusStartPos, StatusEndPos - StatusStartPos).Trim();
            string NewVal = OriginalValue;

            if (OriginalValue == "SA")
              NewVal = "RF";
            else if (OriginalValue == "RF")
              NewVal = "SA";
            
            if (OriginalValue != NewVal)
            {
              CurrentLine = CurrentLine.Remove(StatusStartPos, StatusEndPos - StatusStartPos);
              CurrentLine = CurrentLine.Insert(StatusStartPos, NewVal);
            }
            sb.AppendLine(CurrentLine);
          }
        }
      }

      System.IO.FileInfo file = new FileInfo(File);
      
      using (StreamWriter outfile = new StreamWriter(String.Format(@"{0}\{1}_{2}.txt", WorkFolder, BatchNumber,file.Name.Replace("Merge","")), false))
      {
        outfile.Write(sb.ToString());
      }
    }


    #endregion

    /// <summary>
    /// Creates a new file by merging multiple files of the same type
    /// </summary>
    /// <param name="filesToMerge">The list of files to merge</param>
    /// <param name="TempSaveFile">The resulting file created by merging all the files</param>
    public static void MergeFiles(FileInfo[] filesToMerge, string TempSaveFile)
    {
      using (StreamWriter writer = new StreamWriter(TempSaveFile, true, Encoding.Default))
      {
        char[] trimchars = new char[] { '\r', '\n' };
        foreach (FileInfo item in filesToMerge)
        {
          if (item.Length == 0) continue;

          using (StreamReader reader = new StreamReader(item.FullName, Encoding.Default))
          {
            writer.WriteLine(reader.ReadToEnd().TrimEnd(trimchars));
            reader.Close();
          }
        }
        writer.Flush();
        writer.Close();
      }
    }

   
    /// <summary>
    /// Remove files from the download directory to prevent them from being re-processed
    /// </summary>
    /// <param name="filesToMerge">Files to check</param>
    /// <param name="BackupFolder">Folder where already processed files live</param>
    /// <param name="FilesRemoved">Files Removed from directory</param>
    /// <param name="FilesKept">Files Kept</param>
    public static void RemoveProcessedFiles(FileInfo[] filesToMerge, string BackupFolder, out List<string> FilesRemoved, out List<string> FilesKept)
    {
      List<string> Remove = new List<string>();
      List<string> Keep = new List<string>();
      
      foreach (FileInfo file in filesToMerge)
      {
        string FileToCheck = BackupFolder + "\\" + file.Name;
        if (File.Exists(FileToCheck))
        {
          if (!Directory.Exists(BackupFolder + "\\DuplicateFileProcessAttempt"))
            Directory.CreateDirectory(BackupFolder + "\\DuplicateFileProcessAttempt");

          file.MoveTo(BackupFolder + "\\DuplicateFileProcessAttempt\\" + file.Name);
          Remove.Add(file.Name);
        }
        else
          Keep.Add(file.Name);
      }

      FilesRemoved = Remove;
      FilesKept = Keep;
    }

    /// <summary>
    /// Generate a summary report of the file processed from weblin
    /// </summary>
    /// <param name="SourceDocument"></param>
    /// <param name="TranslatedDocument"></param>
    /// <returns></returns>
    public static FileInfo GenerateSummaryReport(XmlDocument SourceDocument, XmlDocument TranslatedDocument)
    {
      XPathNavigator SourceNav = SourceDocument.DocumentElement.CreateNavigator();
      XPathNavigator TransNav = TranslatedDocument.DocumentElement.CreateNavigator();
      
      using (System.IO.FileStream fstream = new FileStream(@"IOrderReport.csv", FileMode.Create, FileAccess.Write))
      {
        using (StreamWriter writer = new StreamWriter(fstream))
        {
          foreach (string StoreNo in TotalStores)
          {
            object OrderTstamp = Convert.ChangeType(SourceNav.Evaluate("string(//CustomerOrder[@StoreNo='" + StoreNo + "']/Customer[1]/@Order_DateTime)"), typeof(Int64));
            DateTime OrderDate = Convert.ToDateTime(String.Format("{0:####-##-## ##:##:##}", OrderTstamp));

            writer.WriteLine(String.Format("Store# {0},,Sales ,Refunds,", StoreNo));
            writer.WriteLine(String.Format("{0},{1},{2},{3}", OrderDate.ToString("MM/dd/yyyy"), "Visa", SourceNav.Evaluate("sum(//CustomerOrder[@StoreNo = '" + StoreNo + "']/PaymentDetail[@Credit_Card_Type='VISA'][@Status='SA']/@Amount)"), SourceNav.Evaluate("sum(//CustomerOrder[@StoreNo = '" + StoreNo + "']/PaymentDetail[@Credit_Card_Type='VISA'][@Status!='SA']/@Amount)")));
            writer.WriteLine(String.Format(",{0},{1},{2}", "Master Card", SourceNav.Evaluate("sum(//CustomerOrder[@StoreNo = '" + StoreNo + "']/PaymentDetail[@Credit_Card_Type='MSTR'][@Status='SA']/@Amount)"), SourceNav.Evaluate("sum(//CustomerOrder[@StoreNo = '" + StoreNo + "']/PaymentDetail[@Credit_Card_Type='MSTR'][@Status!='SA']/@Amount)")));
            writer.WriteLine(String.Format(",{0},{1},{2}", "American Express", SourceNav.Evaluate("sum(//CustomerOrder[@StoreNo = '" + StoreNo + "']/PaymentDetail[@Credit_Card_Type='AMEX'][@Status='SA']/@Amount)"), SourceNav.Evaluate("sum(//CustomerOrder[@StoreNo = '" + StoreNo + "']/PaymentDetail[@Credit_Card_Type='AMEX'][@Status!='SA']/@Amount)")));
            writer.WriteLine(String.Format(",{0},{1},{2}", "Discover", SourceNav.Evaluate("sum(//CustomerOrder[@StoreNo = '" + StoreNo + "']/PaymentDetail[@Credit_Card_Type='DSCV'][@Status='SA']/@Amount)"), SourceNav.Evaluate("sum(//CustomerOrder[@StoreNo = '" + StoreNo + "']/PaymentDetail[@Credit_Card_Type='DSCV'][@Status!='SA']/@Amount)")));
            writer.WriteLine(String.Format(",{0},{1},{2}", "Gift Card", SourceNav.Evaluate("sum(//CustomerOrder[number(@StoreNo) = '" + StoreNo + "']/PaymentDetail[@Credit_Card_Type='GIFT'][@Status='SA']/@Amount)"), SourceNav.Evaluate("sum(//CustomerOrder[@StoreNo = '" + StoreNo + "']/PaymentDetail[@Credit_Card_Type='GIFT'][@Status!='SA']/@Amount)")));
            writer.WriteLine();

            writer.WriteLine(String.Format(",{0},{1},{2}", "Sales", TransNav.Evaluate("sum(//L30[number(@store_num) = '" + StoreNo + "'][@item != '000000000001'][number(@extended) > 0]/@extended)"), TransNav.Evaluate("sum(//L30[number(@store_num) = '" + StoreNo + "'][@item != '000000000001'][number(@extended) < 0]/@extended)")));
            writer.WriteLine(String.Format(",{0},{1},{2}", "Gift Card Sales", SourceNav.Evaluate("sum(//CustomerOrder[number(@StoreNo) = '" + StoreNo + "']/OrderDetail[@Gift_card_number!=''][@Status='SA']/@Amount)"), SourceNav.Evaluate("sum(//CustomerOrder[@StoreNo = '" + StoreNo + "']/OrderDetail[@Gift_card_number !=''][@Status!='SA']/@Amount)")));
            writer.WriteLine(String.Format(",{0},{1},{2}", "Discounts", SourceNav.Evaluate("sum(//CustomerOrder[@StoreNo = '" + StoreNo + "']/OrderDetail[@Status = 'SA']/@MSRP) - sum(//CustomerOrder[@StoreNo = '" + StoreNo + "']/OrderDetail[@Status = 'SA']/@Unit_Price)"), SourceNav.Evaluate("sum(//CustomerOrder[@StoreNo = '" + StoreNo + "']/OrderDetail[@Status != 'SA']/@MSRP) - sum(//CustomerOrder[@StoreNo = '" + StoreNo + "']/OrderDetail[@Status != 'SA']/@Unit_Price)")));
            writer.WriteLine(String.Format(",{0},{1},{2}", "Shipping/Handling", SourceNav.Evaluate("sum(//CustomerOrder[@StoreNo = '" + StoreNo + "']/Customer[@Status = 'SA']/@Shipping_Price) + sum(//CustomerOrder[@StoreNo = '" + StoreNo + "']/OrderDetail[@Status = 'SA']/@Handling_Charge)"), SourceNav.Evaluate("sum(//CustomerOrder[@StoreNo = '" + StoreNo + "']/Customer[@Status != 'SA']/@Shipping_Price) + sum(//CustomerOrder[@StoreNo = '" + StoreNo + "']/OrderDetail[@Status != 'SA']/@Handling_Charge)")));
            writer.WriteLine(String.Format(",{0},{1},{2}", "Taxes", SourceNav.Evaluate("sum(//CustomerOrder[@StoreNo = '" + StoreNo + "']/OrderDetail[@Status = 'SA']/@Line_Tax)"), SourceNav.Evaluate("sum(//CustomerOrder[@StoreNo = '" + StoreNo + "']/OrderDetail[@Status != 'SA']/@Line_Tax)")));
            writer.WriteLine();
            writer.WriteLine(String.Format(",{0},{1},{2}", "Orders", SourceNav.Evaluate("count(//CustomerOrder[@StoreNo = '" + StoreNo + "']/Customer[@Status = 'SA'])"), SourceNav.Evaluate("count(//CustomerOrder[@StoreNo = '" + StoreNo + "']/Customer[@Status != 'SA'])")));
          }
        }
      }
      return new FileInfo("IOrderReport.csv");
    }

    /// <summary>
    /// Use to load skus with their reorder sku from a file produced by siris
    /// </summary>
    /// <returns></returns>
    private XmlDocument GetReorderSkus()
    {
      string ReorderTxtFile = PluginConfig.GetValue("ReorderSkuFile");
      SendMessage("Loading ReorderSkus from " + ReorderTxtFile);
      MemoryStream msSku = new MemoryStream();
      XmlDocument ReorderSkuTemplate = new XmlDocument();
      XmlDocument returnDoc = new XmlDocument();
      ReorderSkuTemplate.Load(PluginConfig.GetValue("IOrderReorderSkuFile"));

      XmlElement root = ReorderSkuTemplate.DocumentElement;

      using (FileStream fstream = new FileStream(ReorderTxtFile, FileMode.Open, FileAccess.Read))
      {
        using (StreamReader reader = new StreamReader(fstream))
        {
          XmlWriter writer = XmlWriter.Create(msSku);
          writer.WriteStartDocument();
          writer.WriteStartElement("ReorderSkus");

          while (reader.Peek() != -1)
          {
            string CurrentLine = reader.ReadLine();
            writer.WriteStartElement("Sku");
            for (int i = 0, count = root.ChildNodes.Count; i < count; i++)
            {
              XmlNode childnode = root.ChildNodes[i];
              int StartPos = Int32.Parse(childnode.Attributes["ColumnStart"].Value) - 1;
              int EndPos = Int32.Parse(childnode.Attributes["ColumnEnd"].Value);
              string CurrentValue = CurrentLine.Substring(StartPos, EndPos - StartPos).Trim();
              writer.WriteAttributeString(childnode.Name, CurrentValue);
            }
            writer.WriteEndElement();
          }
          writer.WriteEndElement();
          writer.Flush();
          msSku.Position = 0;
          returnDoc.Load(msSku);
          writer.Close();
        }
      }
      return returnDoc;
    }

    public  XmlDocument SkuList()
    {
      string SkuTextFile = PluginConfig.GetValue("SkuFile");
      SendMessage("Loading ReorderSkus from " + SkuTextFile);
      MemoryStream msSku = new MemoryStream();
      XmlDocument SkuTemplate = new XmlDocument();
      XmlDocument returnDoc = new XmlDocument();
      SkuTemplate.Load(PluginConfig.GetValue("IOrderSkuList"));

      XmlElement root = SkuTemplate.DocumentElement;

      using (FileStream fstream = new FileStream(SkuTextFile, FileMode.Open, FileAccess.Read))
      {
        using (StreamReader reader = new StreamReader(fstream))
        {
          XmlWriter writer = XmlWriter.Create(msSku);
          writer.WriteStartDocument();
          writer.WriteStartElement("SkuList");

          while (reader.Peek() != -1)
          {
            string CurrentLine = reader.ReadLine();
            string LOB = CurrentLine.Substring(0, 2);
            if (LOB != "07")
              continue;

            writer.WriteStartElement("Sku");
            for (int i = 0, count = root.ChildNodes.Count; i < count; i++)
            {
              XmlNode childnode = root.ChildNodes[i];
              int StartPos = Int32.Parse(childnode.Attributes["ColumnStart"].Value) - 1;
              int EndPos = Int32.Parse(childnode.Attributes["ColumnEnd"].Value);
              string CurrentValue = CurrentLine.Substring(StartPos, EndPos - StartPos).Trim();
              writer.WriteAttributeString(childnode.Name, CurrentValue);
            }
            writer.WriteEndElement();
          }
          writer.WriteEndElement();
          writer.Flush();
          msSku.Position = 0;
          returnDoc.Load(msSku);
          writer.Close();
        }
      }
      return returnDoc;
    }

  }
}
