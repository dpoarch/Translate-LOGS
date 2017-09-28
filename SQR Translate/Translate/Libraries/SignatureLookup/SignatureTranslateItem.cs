using System;
using System.Xml;
using System.IO;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace SpencerGifts.Translate.Plugin.TLog.SignatureLookup
{

	public class SignatureTranslateItem : Translate.TranslateItem
	{
		System.Collections.Generic.List<string> foundHashedLines = new System.Collections.Generic.List<string>();

		public override XmlReader SourceDocument
		{
			get { throw new NotImplementedException(); }
		}

		public override string SourceFile
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		protected override void Process()
		{
			Environment.ExitCode = 0;
			FileInfo[] TlogFiles = GetFiles(base.SourceFileLocation);
			string strConection = base.PluginConfig.GetValue("SQLConnection");
      string NavistorConection = base.PluginConfig.GetValue("NavistorSQLConnection");


			using (SqlConnection conn = new SqlConnection(strConection))
			{
				conn.Open();
				SqlCommand objCommand = null;
				string StoreNum = "";
				DateTime? TransDate = null;

				foreach (FileInfo file in TlogFiles)
				{
					objCommand = new SqlCommand("InsertTLogTransactionTotal");
					objCommand.CommandType = System.Data.CommandType.StoredProcedure;
					objCommand.Connection = conn;

					SqlTransaction objTrans = conn.BeginTransaction();
					try
					{
						using (XmlTextReader reader = new XmlTextReader(file.FullName))
						{
							while (reader.Read())
							{
								if (reader.NodeType == XmlNodeType.Element)
								{
									if (reader.Name == "L10")
									{
										if (String.IsNullOrEmpty(StoreNum))
											StoreNum = reader.GetAttribute("store_num");
										if (!TransDate.HasValue)
										{
											if (!String.IsNullOrEmpty(reader.GetAttribute("business_date")))
												TransDate = Convert.ToDateTime(reader.GetAttribute("business_date"));
											else
												TransDate = Convert.ToDateTime(Convert.ToDateTime(reader.GetAttribute("trans_date")).ToString("MM/dd/yyyy"));
										}
									}
									if (reader.Name == "L44")
									{
										if (!foundHashedLines.Contains(reader.GetAttribute("hashed_account")) && !String.IsNullOrEmpty(reader.GetAttribute("hashed_account")))
											foundHashedLines.Add(reader.GetAttribute("hashed_account"));
									}


									if (reader.Name == "L99")
									{
										SqlParameter paramStore_num = new SqlParameter("store_num", reader.GetAttribute("store_num"));
										SqlParameter paramReg_num = new SqlParameter("reg_num", reader.GetAttribute("reg_num"));
										SqlParameter paramTrans_num = new SqlParameter("trans_num", reader.GetAttribute("trans_num"));
										SqlParameter paramTrans_Date = new SqlParameter("trans_date", reader.GetAttribute("trans_date"));
										SqlParameter paramGrossAMount = new SqlParameter("transaction_total", reader.GetAttribute("gross"));
										objCommand.Transaction = objTrans;
										objCommand.Parameters.AddRange(new object[] { paramStore_num, paramReg_num, paramTrans_Date, paramTrans_num, paramGrossAMount });
										objCommand.ExecuteNonQuery();
										objCommand.Parameters.Clear();
									}
								}
							}
							objTrans.Commit();
						}
						try
						{
							XmlDocument doc = new XmlDocument();
							doc.LoadXml(System.IO.File.ReadAllText(file.FullName, System.Text.Encoding.Default));

							XmlNodeList nodeList = doc.SelectNodes(@"//L44");
							foreach (XmlNode node in nodeList)
							{
								if (node.Attributes["hashed_account"] != null && !String.IsNullOrEmpty(node.Attributes["hashed_account"].Value))
								{
									string str = node.Attributes["hashed_account"].Value;
									node.Attributes["hashed_account"].Value = ConvertToHex(str);
								}
							}


							//string TLog = System.IO.File.ReadAllText(file.FullName, System.Text.Encoding.Default);
              using (SqlConnection NavistorConn = new SqlConnection(NavistorConection))
              {
                NavistorConn.Open();
                string TLog = doc.OuterXml;
                objCommand = new SqlCommand("InsertTlog");
                objCommand.CommandType = System.Data.CommandType.StoredProcedure;
                objCommand.Connection = NavistorConn;
                SqlParameter objXml = new SqlParameter("TLog", TLog);                
                objCommand.Parameters.AddRange(new object[] { objXml });
                objCommand.ExecuteNonQuery();
                objCommand.Parameters.Clear();
                StoreNum = "";
                TransDate = null;
                System.IO.File.Delete(file.FullName);               
              }														
						}
						catch (Exception ex)
						{
							//LogMessage("Unable to delete file " + file.Name);
              System.IO.File.Move(file.FullName, String.Format("{0}\\FailedImport\\{1}", SourceFileLocation,file.Name));              
              LogMessage(String.Format("{0}: {1}", ex.Message, ex.StackTrace));

              Environment.ExitCode = 1;
              continue;
						}
					}
					catch (Exception ex)
					{
            LogMessage(String.Format("Error Processing {0}", file.Name));
            LogMessage(String.Format("{0}: {1}", ex.Message, ex.StackTrace));
						objTrans.Rollback();
            Environment.ExitCode = 1;
						continue;
					}
          LogMessage(String.Format("Processed {0}", file.Name));
				}

				try
				{
					objCommand = new SqlCommand("UpdateTenderTotal");
					objCommand.CommandType = System.Data.CommandType.StoredProcedure;
					objCommand.Connection = conn;
					objCommand.ExecuteNonQuery();
					LogMessage("Updated Tender Totals ");
				}
				catch (Exception ex)
				{
					LogMessage("Error Updating Totals ");
          LogMessage(String.Format("{0}: {1}", ex.Message, ex.StackTrace));
					Environment.ExitCode = 1;          
				}


				if (conn.State == System.Data.ConnectionState.Open)
					conn.Close();
        
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
				//LogMessage(String.Format(ResourceHelper.Instance.GetString(ResFile, "InvalidDirectory"), Location));
				return null;
			}

		}



		private string ConvertToHex(string TextToConvert)
		{
			byte[] CCBytes = System.Text.Encoding.Default.GetBytes(TextToConvert.ToCharArray());
			System.Text.Encoding.Convert(System.Text.Encoding.Default, System.Text.Encoding.UTF7, CCBytes);

			if (CCBytes == null || CCBytes.Length == 0)
				return TextToConvert;

			System.Text.StringBuilder sbToHEX = new System.Text.StringBuilder();
			foreach (byte b in CCBytes)
			{
				// Get the integer value of the character.
				int value = Convert.ToInt32(b);
				// Convert the decimal value to a hexadecimal value in string form.              
				sbToHEX.Append(String.Format("{0:X}", value).PadLeft(2, '0'));
			}
			return sbToHEX.ToString();
		}

	}
}
