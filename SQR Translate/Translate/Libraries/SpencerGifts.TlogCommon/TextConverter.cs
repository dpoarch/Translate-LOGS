using System;
using System.IO;
using System.Collections;
using System.Data;
using System.Threading;


namespace SpencerGifts.TlogCommon
{
  internal sealed class TextConverter
  {
      
      
    public TextConverter()
    {
    }

    /// <summary>
    /// Loads a delimieted text file into a datatable.
    /// </summary>
    /// <param name="sFile">The location of the file to load</param>
    /// <param name="Delimeter">The delimiter of the file</param>
    /// <param name="FirstRowColHeaders">Not used yet</param>
    /// <exception cref="System.IO.EndOfStreamException"></exception>
    /// <exception cref="System.IO.FileNotFoundException"></exception>
    /// <returns></returns>
    public DataTable FileToDT(string sFile, string Delimeter, bool FirstRowColHeaders)
    {
      DataTable dtFile;

      //Check to make sure the requested file exists
      if (!File.Exists(sFile))
        throw new FileNotFoundException();

      dtFile = new DataTable();
      //Open the supplied textfile for reading
      StreamReader sr = new StreamReader(File.OpenRead(sFile));
      using (sr)
      {
        ArrayList RowArray = new ArrayList();
        try
        {
          string[] strNewCols;
          //Now for each row split it out based on the Delimeter passed in 
          //and add the columns to the datatable
          int Remainder = 0;
          int RowCount = 0;
          while (sr.Peek() != -1)
          {
            strNewCols = sr.ReadLine().Split(Delimeter.ToCharArray());
            AddRowToTable(strNewCols, dtFile);

            RowCount++;
            //int num = Math.DivRem(RowCount, 500, out Remainder);
            //if (Remainder == 0)
            //Thread.Sleep(5);

            strNewCols = null;
          }
          sr.Close();
        }
        catch (EndOfStreamException e)
        {
          sr.Close();
          return null;
        }
      }
      return dtFile;
    }

    /// <summary>
    /// Adds the current row in the file to the datatable
    /// </summary>
    /// <param name="NewRow">The items to add to the datatable</param>
    private static void AddRowToTable(string[] NewRow, DataTable dtFile)
    {
      //IF the current row we are adding has more columns than the table has defined
      //We will add the necessary columns to the table and make the assumption they 
      //are null for the previous records.
      if (NewRow.Length > dtFile.Columns.Count)
      {
        int ColsToAdd = NewRow.Length - dtFile.Columns.Count;
        int LastColName = dtFile.Columns.Count;

        for (int i = 0; i < ColsToAdd; i++)
        {
          LastColName++;
          dtFile.Columns.Add("Column " + LastColName.ToString(), typeof(string));
        }
      }
      DataRow dr = dtFile.NewRow();
      dr.ItemArray = NewRow;
      dtFile.Rows.Add(dr);
    }
  }
}
