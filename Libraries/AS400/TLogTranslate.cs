using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using SpencerGifts.TlogCommon;
using System.IO;
using System.Collections.Specialized;

namespace SpencerGifts.Translate.Plugin.TLog.AS400
{
  class TLogTranslateItem : TranslateItem
  {
    XmlReader ConvertedTLogDoc;
    string _SourceFile = "";
    //bool TransComplete = false;
    TextWriter OutputTextWriter = null;
    String TempFileName;

    public TLogTranslateItem()
    {      
    }

    public override XmlReader SourceDocument
    {
      get
      {
        if (ConvertedTLogDoc == null)
          ConvertedTLogDoc = new Xml().GetTLogToXMReader(_SourceFile, base.SourceTemplateFile, base.RulesFile);
        return ConvertedTLogDoc;
      }
    }

    public override string SourceFile
    {
      get { return _SourceFile; }
      set { _SourceFile = value; }
    }

    protected override string TranslatedSaveFile
    {
      get
      {
        FileInfo info = new FileInfo(_SourceFile);
        string SaveFile = base.TranslatedFileLocation + "\\" + PluginConfig.GetValue("DestinationFile");
        return SaveFile;
      }
    }

    protected override void SaveTranslatedFile(XmlDocument TranslatedDocument)
    {

      if (OutputTextWriter != null)
        OutputTextWriter.Close();

      try
      {
        //Mode the temp file to its final location
        System.IO.File.Copy(TempFileName, TranslatedSaveFile, true);
        System.IO.File.Delete(TempFileName);
      }
      catch (Exception ex)
      {
        //TODO: Handle file move exceptions  
      }
      //base.SaveTranslatedFile(TranslatedDocument);
    }

    protected override void Process()
    {
      LogMessage("Translating File");

      using (StreamReader sr = new StreamReader(File.OpenRead(_SourceFile)))
      {
        string[] strNewCols;
        //int rowcount = 0;

        string row = "";
        string RowQualifier = "L";
        while (sr.Peek() != -1)
        {
          strNewCols = sr.ReadLine().Split("\t".ToCharArray());
          
          try
          {
            row = RowQualifier + strNewCols[0].ToString();
          }
          catch
          {
          //  //We had a problem converting the current row to a valid type
          //  //usually this will mean the tlog spec changed and a new row has been added.
          //  //For now we assume it doesnt matter until we need the new row.
            row = "NA";
          }

          if (row == "L44")
          {
            try
            {
              switch (strNewCols[10])
              {
                case "7":
                case "8":
                case "9":
                case "10":
                case "14":
                case "15":
                case "19":
                case "20":
                case "22":
                  string CCNum = strNewCols[6];
                  if (!String.IsNullOrEmpty(CCNum))
                  {
                    int CCLen = CCNum.Length;
                    string MaskedCC = CCNum.Substring(CCLen - 4).PadLeft(CCLen - 4, 'X');
                    strNewCols[6] = MaskedCC;
                  }
                  break;

              }
            }
            catch (Exception ex)
            {

            }

            WriteToOutput(strNewCols);
          }
          else
          {
            WriteToOutput(strNewCols);
          }
        }
      }
      //TransComplete = true;
      SaveTranslatedFile(null);
      LogMessage("Translate Complete");
    }

    private void WriteToOutput(string[] strNewCols)
    {
      try
      {
        //Create a temporary file to hold the contents.
        if (String.IsNullOrEmpty(TempFileName))
          TempFileName = Guid.NewGuid().ToString() + ".trns";

        if (OutputTextWriter == null)
          OutputTextWriter = new StreamWriter(TempFileName);


        StringBuilder sb = new StringBuilder();
        for (int x = 0; x < strNewCols.Length; x++)
          sb.Append(strNewCols[x].ToString() + "\t");

        OutputTextWriter.WriteLine(sb.ToString().Remove(sb.Length - 1));
      }
      catch (Exception ex)
      {
        //string s = "";
      }
    }

    
  }

}
