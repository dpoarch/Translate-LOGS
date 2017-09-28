using System;
using System.Text;
using System.Xml;
using System.IO;
using System.Collections;
using System.Xml.XPath;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace SpencerGifts.TlogCommon
{
  /// <summary>
  /// Used for validating a tlog
  /// </summary>
  public class Validate
  {
    private StringBuilder _sbErrorMessage;
    private FileInfo FileProcessed;
    private static Dictionary<string, XPathExpression> _CahcedExpressions;
    private static List<string> _StoreExceptions;

    Regex DateReg;

    public Validate()
    {
      _sbErrorMessage = new StringBuilder();
      LoadStoreExceptions();
    }

    public string ErrorMessage
    {
      get { return _sbErrorMessage.ToString(); }
    }

    public List<string> StoreExceptions
    {
      get
      {
        if (_StoreExceptions == null)
          _StoreExceptions = new List<string>();

        return _StoreExceptions;
      }
      set
      {
        if (_StoreExceptions == null)
          _StoreExceptions = new List<string>();

        if (_StoreExceptions == value)
          return;
        _StoreExceptions = value;
      }
    }

    public bool isValidTLog(XmlDocument tLog, string FileName)
    {
      FileProcessed = new FileInfo(FileName);
      try
      {
        return isValid(tLog);
      }
      catch (ValidateException ex)
      {
        _sbErrorMessage.Append(String.Format("Validation Failed for File {0}\r\n", FileProcessed.Name));
        _sbErrorMessage.Append(String.Format("Validation Error:  {0}\r\n", ex.Message));
        _sbErrorMessage.Append(String.Format("Line Number:  {0}\r\n", ex.LineNumber));
        if (!String.IsNullOrEmpty(ex.TransactionNumber))
          _sbErrorMessage.Append(String.Format("Transaction Number:  {0}\r\n", ex.TransactionNumber));
        if (!String.IsNullOrEmpty(ex.ExpectedValue))
          _sbErrorMessage.Append(String.Format("Expected Result:  {0}\r\n", ex.ExpectedValue));
        if (!String.IsNullOrEmpty(ex.ActualValue))
          _sbErrorMessage.Append(String.Format("Actual Result:  {0}\r\n", ex.ActualValue));

        return false;
      }
      catch (Exception ex)
      {
        _sbErrorMessage.Append(String.Format("Validation Failed for File {0}\r\n", FileProcessed.Name));
        _sbErrorMessage.Append("An unhandled exception has occurred \r\n");
        _sbErrorMessage.Append(String.Format("Validation Error:  {0}\r\n", ex.Message));
        _sbErrorMessage.Append(String.Format("Stack Trace:  {0}\r\n", ex.StackTrace));
        return false;
      }
    }

    public bool isValidTLog(string FileName, string TemplateFile)
    {
      XmlDocument xdoc = new TlogCommon.Xml().ConvertTLogToXML(FileName, TemplateFile);
      return isValidTLog(xdoc, FileName);
    }

    private bool isValid(XmlDocument tlogDoc)
    {
      XmlElement root = tlogDoc.DocumentElement;
      string FileStoreNumber = FileProcessed.Name.Substring(4, 5);
      IEnumerator transEnum = root.GetEnumerator();
      int LineNumber = 1;
      while (transEnum.MoveNext())
      {
        XmlNode CurrentTransaction = (XmlNode)transEnum.Current;
        CheckTransaction(CurrentTransaction, FileStoreNumber, ref LineNumber);
      }

      return true;
    }

    private void CheckTransaction(XmlNode CurrentTransaction, string FileStoreNum, ref int LineNumber)
    {
      // Set Minimum Field Totals
      // These are how many total Fields (columns) are required per line
      int intMinFields_All = 4;  // No matter what type of record
      int intMinFields_H_Office = 9; // Office Side Header Records
      int intMinFields_H_Reg = 38; // Register Side Header Records
      if (DateReg == null)
        DateReg = new Regex(@"^(?=\d)(?:(?!(?:1582(?:\.|-|\/)10(?:\.|-|\/)(?:0?[5-9]|1[0-4]))|(?:1752(?:\.|-|\/)0?9(?:\.|-|\/)(?:0?[3-9]|1[0-3])))(?=(?:(?!000[04]|(?:(?:1[^0-6]|[2468][^048]|[3579][^26])00))(?:(?:\d\d)(?:[02468][048]|[13579][26]))\D0?2\D29)|(?:\d{4}\D(?!(?:0?[2469]|11)\D31)(?!0?2(?:\.|-|\/)(?:29|30))))(\d{4})([-\/.])(0?\d|1[012])\2((?!00)[012]?\d|3[01])(?:$|(?=\x20\d)\x20))?((?:(?:0?[1-9]|1[012])(?::[0-5]\d){0,2}(?:\x20[aApP][mM]))|(?:[01]\d|2[0-3])(?::[0-5]\d){1,2})?$",RegexOptions.Compiled);

      if (CurrentTransaction.Name == "UnknownRecord")
        QuitWithError(CurrentTransaction.Attributes["trans_num"].Value.ToString(), LineNumber.ToString(), "Incomplete Transaction");

      string CurrentTransNumber = CurrentTransaction.Attributes["TransNumber"].Value.ToString();
      XPathNavigator TransNavigator = CurrentTransaction.CreateNavigator();

      IEnumerator enumerator = CurrentTransaction.GetEnumerator();
      while (enumerator.MoveNext())
      {

        XmlNode LineItem = (XmlNode)enumerator.Current;
        XPathNavigator LineItemNavigator = LineItem.CreateNavigator();

        string TransNum = LineItem.Attributes["trans_num"].Value.ToString();
        string StoreNo = LineItem.Attributes["store_num"].Value.ToString();
        string Reg_Num = LineItem.Attributes["reg_num"].Value.ToString();
        string Trans_Date = LineItem.Attributes["trans_date"].Value.ToString();
        string record_type = LineItem.Attributes["record_type"].Value.ToString();


        //Make sure the line item as the min number of fields
        if (LineItem.Attributes.Count < intMinFields_All)
          QuitWithError(TransNum, LineNumber.ToString(), "Record is corrupt.");

        //Check to make sure we have a valid store number
        int ConvertedStore = 0;
        if (!Int32.TryParse(StoreNo, out ConvertedStore))
          QuitWithError(TransNum.ToString(), LineNumber.ToString(), "Invalid Store Number.", FileStoreNum, StoreNo);
        else if (ConvertedStore == 0)
          QuitWithError(TransNum.ToString(), LineNumber.ToString(), "Invalid Store Number.", FileStoreNum, StoreNo);

        //Make sure we have a valid register number
        int ConvertedRegNum = 0;
        if (!Int32.TryParse(Reg_Num, out ConvertedRegNum))
          QuitWithError(TransNum.ToString(), LineNumber.ToString(), "Invalid Registure Number.", null, Reg_Num);

        //make sure we have a valid transaction number
        int ConvertedTransNum = 0;
        if (!Int32.TryParse(TransNum, out ConvertedTransNum))
          QuitWithError(TransNum.ToString(), LineNumber.ToString(), "Invalid Transaction Number.", null, TransNum);

        DateTime ConvertedDate;
        if (!DateTime.TryParse(Trans_Date, out ConvertedDate))
          QuitWithError(TransNum, LineNumber.ToString(), "Invalid Transaction Date", "Date Format XXXX-XX-XX XX:XX:XX", Trans_Date);
        else if (!DateReg.IsMatch(Trans_Date) || Trans_Date.Length != 19)
          QuitWithError(TransNum, LineNumber.ToString(), "Invalid Transaction Date", "Date Format XXXX-XX-XX XX:XX:XX", Trans_Date);

        //Make sure we have a valid transaction with a header and footer        
        int HeaderCount = Convert.ToInt32(TransNavigator.Evaluate(GetXpathExpression("count(L10)")));
        int FooterCount = Convert.ToInt32(TransNavigator.Evaluate(GetXpathExpression("count(L99)")));
        if (HeaderCount != 1)
          QuitWithError(TransNum, LineNumber.ToString(), "Unexpected transaction header", "1", HeaderCount.ToString());
        if (FooterCount != 1)
          QuitWithError(TransNum, LineNumber.ToString(), "Unexpected transaction footer", "1", FooterCount.ToString());


        //Validations for the header record
        if (record_type == "10")
        {
          //Check Exceptions File here
          if (!isInExceptionsFile(FileStoreNum) && Convert.ToInt32(FileStoreNum) != ConvertedStore)
            QuitWithError(TransNum, LineNumber.ToString(), "Store number in file does not match filename", Convert.ToInt32(FileStoreNum).ToString(), ConvertedStore.ToString());

          string RecordFlag = "";
          switch (ConvertedRegNum)
          {
            case 0: //Office Side Transasction
              //check to make sure we have the correct number of records
              if (LineItem.Attributes.Count < intMinFields_H_Office)
                QuitWithError(TransNum.ToString(), LineNumber.ToString(), "Record is corrupt.", String.Format("{0} columns in record", intMinFields_H_Office), LineItem.Attributes.Count.ToString() + " columns found.");

              RecordFlag = "@rec_count";
              break;
            default: //Register Side Transaction
              if (!isInExceptionsFile(FileStoreNum))
              {
                if (LineItem.Attributes.Count < intMinFields_H_Reg)
                  QuitWithError(TransNum, LineNumber.ToString(), "Record is corrupt", intMinFields_H_Reg.ToString() + " columns in record", String.Format("{0} columns found.", LineItem.Attributes.Count));
                RecordFlag = "@db_rec_count";
              }
              else
                RecordFlag = "@rec_count";

              break;
          }

          //Make sure we have a valid record count 
          if (!Convert.ToBoolean(LineItemNavigator.Evaluate(GetXpathExpression(String.Format("number({0}) > 0", RecordFlag)))))
            QuitWithError(TransNum, LineNumber.ToString(), "Invalid Record Count");

          //Make sure the number of records matches the number in the header           
          //if (!Convert.ToBoolean(TransNavigator.Evaluate(GetXpathExpression(String.Format("count(//Transaction[@TransNumber = {0}]/*) = //Transaction[@TransNumber = {1}]/L10/attribute::{2}", CurrentTransNumber, CurrentTransNumber, RecordFlag)))))
          if (TransNavigator.Evaluate(GetXpathExpression("string(count(*))")).ToString() != TransNavigator.Evaluate(GetXpathExpression(String.Format("number(L10/{0})", RecordFlag))).ToString())
            QuitWithError(TransNum, LineNumber.ToString(), "Total records in transaction does not match the number specified in the header");
        }
        else
        {
          //Check to make sure the values in the remaining trans records match the header;
          bool SameRegNum = LineItemNavigator.Evaluate(GetXpathExpression("string(@reg_num)")).ToString().Equals(TransNavigator.Evaluate(GetXpathExpression("string(L10/@reg_num)")).ToString());
          if (!SameRegNum)
            QuitWithError(TransNum, LineNumber.ToString(), "Register Number does not match register number in header");

          bool SameStoreNum = LineItemNavigator.Evaluate(GetXpathExpression("string(@store_num)")).ToString().Equals(TransNavigator.Evaluate(GetXpathExpression("string(L10/@store_num)")).ToString());
          if (!SameStoreNum)
            QuitWithError(TransNum, LineNumber.ToString(), "Store Number does not match store number in header");

          bool SameTransNum = LineItemNavigator.Evaluate(GetXpathExpression("string(@trans_num)")).ToString().Equals(TransNavigator.Evaluate(GetXpathExpression("string(L10/@trans_num)")).ToString());
          if (!SameTransNum)
            QuitWithError(TransNum, LineNumber.ToString(), "Transaction Number does not match transaction number in header");

          bool SameTransDate = LineItemNavigator.Evaluate(GetXpathExpression("string(@trans_date)")).ToString().Equals(TransNavigator.Evaluate(GetXpathExpression("string(L10/@trans_date)")).ToString());
          if (!SameTransDate)
            QuitWithError(TransNum, LineNumber.ToString(), "Transaction Date does not match transaction date in header");
        }
        LineNumber++;
      }
    }

    private bool ContainsUnknownRecords(XmlNode CurrentTrans)
    {
      return CurrentTrans.SelectNodes("/UnknownRecord").Count > 0;
      //while (nodeEnum.MoveNext())
      //{
      //  System.Xml.XPath.XPathNavigator CurrentNave = nodeEnum.Current();
      //}
    }

    private void LoadStoreExceptions()
    {
      //Hard coded for now should be changes to a parameter
        try
        {
            string Filename = "ValidationExceptions.xml";
            string ExceptionFile = Environment.CurrentDirectory + "\\TranslateDependencies\\" + Filename;
            using (FileStream fstream = new FileStream(ExceptionFile, FileMode.Open, FileAccess.Read))
            {
                XmlReader reader = XmlReader.Create(fstream);
                reader.MoveToContent();
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        string store = reader.GetAttribute("id");
                        StoreExceptions.Add(store);
                    }
                }
            }
        }
        catch (Exception ex)
        {

        }
     
    }

    private bool isInExceptionsFile(string StoreNum)
    {
      if (_StoreExceptions == null)
        return false;

      return _StoreExceptions.Contains(StoreNum);
    }

    /// <summary>
    /// Used to raise exception when validation of the current tlog has failed
    /// </summary>
    /// <param name="TransactionNumber">The Current Transaction Number</param>
    /// <param name="LineNumber">The Current position in the file</param>
    /// <param name="ErrorMessage">The Error Message To send back</param>
    private void QuitWithError(string TransactionNumber, string LineNumber, string ErrorMessage)
    {
      ValidateException ex = new ValidateException(ErrorMessage);
      ex.LineNumber = LineNumber;
      ex.TransactionNumber = TransactionNumber;
      throw ex;
    }

    /// <summary>
    ///  Used to raise exception when validation of the current tlog has failed
    /// </summary>
    /// <param name="TransactionNumber">The Current Transaction Number</param>
    /// <param name="LineNumber">The Current position in the file</param>
    /// <param name="ErrorMessage">The Error Message To send back</param>
    /// <param name="ExpectedValue">The Value expected</param>
    /// <param name="ActualValue">The value that was returned</param>
    private void QuitWithError(string TransactionNumber, string LineNumber, string ErrorMessage, string ExpectedValue, string ActualValue)
    {
      ValidateException ex = new ValidateException(ErrorMessage);
      ex.LineNumber = LineNumber;
      ex.TransactionNumber = TransactionNumber;
      ex.ExpectedValue = ExpectedValue;
      ex.ActualValue = ActualValue;
      throw ex;
    }

    /// <summary>
    /// Provieds a means to keep the used xpath expressions compiled for more efficient use when used again.
    /// </summary>
    /// <param name="xpath"></param>
    /// <returns></returns>
    private static XPathExpression GetXpathExpression(string Xpath)
    {
      if (_CahcedExpressions == null)
        _CahcedExpressions = new Dictionary<string, XPathExpression>();


      if (_CahcedExpressions.ContainsKey(Xpath))
      {
        try
        {
          return _CahcedExpressions[Xpath];
        }
        catch
        {
          return XPathExpression.Compile(Xpath);
        }
      }
      else
      {
        try
        {
          _CahcedExpressions.Add(Xpath, XPathExpression.Compile(Xpath));
        }
        catch
        {
          return XPathExpression.Compile(Xpath);
        }
      }

      return _CahcedExpressions[Xpath];

    }

  }
}
