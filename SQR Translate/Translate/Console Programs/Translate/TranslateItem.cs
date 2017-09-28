using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using SpencerGifts.Translate.Configuration;
using System.Collections;
using System.IO;
using System.Threading;
using SpencerGifts.Common.Logging;
using System.Linq;

namespace SpencerGifts.Translate
{

	public abstract class TranslateItem
	{
		/// <summary>
		/// Used to inform the translate if it should import the current node or not.
		/// </summary>
		bool ImportNode;
		/// <summary>
		/// Used to keep track of actions that get executed for a particular rule
		/// </summary>
		List<string> ActionsExecuted;
		/// <summary>
		/// Collection of all the rules for the currently executing translate plugin 
		/// </summary>
		List<IRule> _Rules;
		/// <summary>
		/// Document holding the entire rules file.
		/// </summary>
		XmlDocument RulesDoc = null;
		/// <summary>
		/// Used to hold the number of the identity field if a translate plugin decides to use the useIdentity configuration variable
		/// </summary>
		int _Counter = 0;
		/// <summary>
		/// holds a collection of the executed rules for faster processing if the rule is needed again.
		/// </summary>
		private Hashtable _RuleCache = null;
		/// <summary>
		/// Holds the configuration for the currently executing plugin.
		/// </summary>
		private TranslateConfig _config;
		/// <summary>
		/// Holds the template of the testination document
		/// </summary>
		private MemoryStream _DestinationDocumentTemplate;
		XmlDocument ReturnDoc = new XmlDocument();
		/// <summary>
		/// Holds a collection of all unique executed expressions.  Once compiled an expression will execute more efficiently on its next request.
		/// </summary>
		private static Dictionary<string, XPathExpression> _CahcedExpressions;
		Logger _PluginLogger;

		protected event EventHandler OnBeforeNodeAppended;
		protected event EventHandler OnAfterNodeAppended;
		protected event EventHandler OnBeforeRuleProcess;
		protected delegate void AfterRuleProcessHandler(object sender, TranslateEventArgs args);
		protected event AfterRuleProcessHandler OnAfterRuleProcess;
		protected delegate void AfterRuleLoadEventHandler(object sender, TranslateEventArgs args);
		protected delegate void PreProcessEventHandler();
		protected event AfterRuleLoadEventHandler OnAfterRuleLoad;
		protected event PreProcessEventHandler OnPreProcess;

		#region Configuration Settings

		/// <summary>
		/// Gets the Rules File that will be used to process.
		/// </summary>
		protected string RulesFile
		{
			get { return _config.RulesFile; }
		}

		protected string DestinationTemplateFile
		{
			get { return _config.DestinationTemplate; }
		}

		protected string SourceTemplateFile
		{
			get { return _config.SourceTemplate; }
		}

		protected string SourceFileLocation
		{
			get { return _config.SourceFileLocation; }
		}

		protected string NewDocumentRoot
		{
			get { return _config.NewDocumentRootNode; }
		}

		protected string TranslatedFileLocation
		{
			get { return _config.TranslatedFileLocation; }
		}

		#endregion

		#region Abstract Methods

		/// <summary>
		/// Gets the source document used for processing
		/// </summary>
		abstract public XmlReader SourceDocument
		{
			get;
		}

		public abstract string SourceFile
		{
			get;
			set;
		}

		protected virtual string TranslatedSaveFile
		{
			get
			{
				FileInfo info = new FileInfo(SourceFile);
				return TranslatedFileLocation + "\\" + info.Name + ".xml";
			}
		}

		#endregion


		/// <summary>
		/// Get's the destination document template which will be the end result 
		/// of rule processing
		/// </summary>
		///     
		virtual public XmlDocument DestinationDocumentTemplate
		{
			get
			{
				if (_DestinationDocumentTemplate == null)
				{
					//Create a new memory stream to hold the template
					_DestinationDocumentTemplate = new MemoryStream();
					TextReader TemplateFileReader = new StreamReader(DestinationTemplateFile);
					TextWriter StreamWriter = new StreamWriter(_DestinationDocumentTemplate);
					//Write the template to the stream;
					StreamWriter.Write(TemplateFileReader.ReadToEnd());
					TemplateFileReader.Close();
					StreamWriter.Flush();
				}
				//Make sure we are at the beginning of the stream before we send it back
				_DestinationDocumentTemplate.Position = 0;
				ReturnDoc.Load(_DestinationDocumentTemplate);
				return ReturnDoc;
			}
		}

		/// <summary>
		/// Used to start the translate
		/// </summary>
		/// <param name="PluginName"></param>
		public void ExecuteTranslate(string PluginName)
		{
			//_config = new TranslateConfig(PluginName);
			//if (_config.SourceFile != "")
			//  SourceFile = SourceFileLocation + "\\" + _config.SourceFile;

			//Process();

			ExecuteTranslate(PluginName, null);
		}
		/// <summary>
		/// Used to start the translate.
		/// </summary>
		/// <param name="PluginName"></param>
		public void ExecuteTranslate(object PluginName)
		{
			ExecuteTranslate(PluginName.ToString());
		}
		/// <summary>
		/// Used to start the translate.
		/// </summary>
		/// <param name="PluginName"></param>
		/// <param name="strSourceFile"></param>
		public void ExecuteTranslate(string PluginName, string strSourceFile)
		{
			_config = new TranslateConfig(PluginName);
			if (String.IsNullOrEmpty(strSourceFile))
			{
				if (_config.SourceFile != "")
					SourceFile = SourceFileLocation + "\\" + _config.SourceFile;
			}
			else
				SourceFile = strSourceFile;

			if (OnPreProcess != null)
				OnPreProcess();

			Process();

		}

		/// <summary>
		/// Processes the source and destination documents from the supplied rules file
		/// </summary>
		/// <exception cref="SpencerGifts.Translate.RuleMappingException"></exception>
		/// <exception cref="SpencerGifts.Translate.RuleConditionException"></exception>
		/// <exception cref="SpencerGifts.Translate.RuleActionException"></exception>
		/// <exception cref="SpencerGifts.Translate.RuleConditionException"></exception>
		virtual protected void Process()
		{
			//Thread.CurrentThread.Priority = ThreadPriority.Lowest;
			//The xml document that will hold the final transalted document
			XmlDocument NewDestination = new XmlDocument();

			//Start the document
			XmlDeclaration xmlDeclaration = NewDestination.CreateXmlDeclaration("1.0", "utf-8", null);
			//The Root node of the document
			XmlElement rootNode = NewDestination.CreateElement("ConvertedTLog");
			NewDestination.InsertBefore(xmlDeclaration, NewDestination.DocumentElement);
			NewDestination.AppendChild(rootNode);
			XmlElement Parent = NewDestination.CreateElement(NewDocumentRoot);
			rootNode.AppendChild(Parent);
      
			using (XmlReader reader = SourceDocument)
			{
				//Get the root level node of the source document        
				reader.MoveToContent();        
				reader.ReadStartElement();

				//Execute the before rule process event
				//BeforeProcess(list, null);
				//BeforeProcess(reader, null);
				//Collection to hold the rules from supplied rules file
				List<IRule> tRules = null;
				//Collection used to hold any actions that will need to be performed after rule processing
				List<string> ActionsToExecute = new List<string>();

				//Collection used to keep track of our actions that we have executed.  This is used for the rules that have ProcessAsGroup = True (Defaults to False);
				//If we do not keep track of these they will execute everytime the condition is met instead of only one time.
				ActionsExecuted = new List<string>();

				//Start looping through the source document and apply the rules as needed.
				int RecordCount = 0;

				//Start Reading through the xml doc
				while (!reader.EOF)
				{
					XmlDocument CurrentTransDoc = new XmlDocument();          
					try
					{
            string xml = reader.ReadOuterXml();
            if (string.IsNullOrEmpty(xml))
              continue;

						CurrentTransDoc.LoadXml(xml);
            //CurrentTransDoc.Load(reader);
#if (DEBUG)
						//Console.WriteLine("Processing Transaction " + CurrentTransDoc.SelectSingleNode("//Transaction/@TransNumber").Value + " " + DateTime.Now.ToString("hh:mm:ss"));
#endif
					}
					catch (Exception exc)
					{
            LogMessage(exc.Message);
						break;
					}
					BeforeProcess(CurrentTransDoc, null);

					XmlNode node = CurrentTransDoc.DocumentElement;
					XmlDocument NewDocTemplate = DestinationDocumentTemplate;
					//Load our rules if we haven't done so yet
					if (tRules == null)
						tRules = TranslateRules;

					//Process rules for each Line Item.
					//We need to go line by line because there could be multiple lines with the same element definition
					ImportNode = false;
					foreach (XmlNode ChildNode in node)
					{
#if (DEBUG)
						//Console.WriteLine("Processing " + ChildNode.Name + " " + DateTime.Now.ToString("hh:mm:ss"));
#endif
						TranslateRule tr = GetRule(ChildNode.Name);

						//if we dont have a rule for the current line skip it and continue to the next
						if (tr == null)
							continue;

						//Call the afterruleload even
						if (OnAfterRuleLoad != null)
						{
							TranslateEventArgs args = new TranslateEventArgs();
							args.CurrentTranslateItem = ChildNode;
							args.OutputDocument = NewDestination;
							OnAfterRuleLoad(tr, args);
						}

						if (!tr.ProcessAsGroup)
							NewDocTemplate = DestinationDocumentTemplate;

						//if ExactMapping is set on the rule we will do a 1to1 mapping of attributes prior to any translating;
						if (tr.ExactMapping)
						{
							for (int i = 0, count = ChildNode.Attributes.Count; i < count; i++)
							{
								NewDocTemplate.SelectSingleNode(tr.DestinationSavePath).Attributes[i].Value = ChildNode.Attributes[i].Value;
							}
							ImportNode = true;
						}

						XPathNavigator nav = ChildNode.CreateNavigator();
						//bool ImportNode = false;

						#region Process 1 to 1 Rule Mappings
						if (tr.RuleMappings.Count > 0)
						{
							//loop through the 1 to 1 mappings first
							foreach (RuleMapping mapping in tr.RuleMappings)
							{
								try
								{
									//if a type is defined convert it and format it to the type specified\
									NewDocTemplate.SelectSingleNode(mapping.DestinationPath).Value = FormatValue(mapping.Format, mapping.FormatType, nav.Evaluate(GetXpathExpression(mapping.SourcePath)).ToString());
									ImportNode = true;
								}
								catch (Exception ex)
								{
									RuleMappingException excep = new RuleMappingException("Rule Mapping Exception Error", ex);
									excep.RuleID = tr.ID;
									excep.SourceFile = SourceFile;
									excep.MappdingDestinationPath = mapping.DestinationPath;
									excep.MappingSourcePath = mapping.SourcePath;
									throw (excep);
								}

							}
						}
						#endregion

						#region Process Rule Conditions
						//now evaluate our specific condition rules
						bool ContinueProcessing = true;
						//XPathNavigator nav = ChildNode.CreateNavigator();

						bool ContinueTransaction = true;
						foreach (RuleCondition condition in tr.RuleConditions)
						{
							try
							{
								ExecuteConditions(condition, NewDocTemplate, nav, ActionsToExecute, ref ContinueProcessing, ref ContinueTransaction);
								if (ContinueProcessing && ContinueTransaction)
									continue;

								break;
							}
							catch (Exception ex)
							{
								RuleConditionException except = new RuleConditionException("Condition Exception", ex);
								except.RuleID = tr.ID;
								except.ConditionID = condition.ID;
								except.SourceFile = SourceFile;
								except.ConditionDestination = condition.Destination;
								except.ConditionExpression = condition.Expression;
								throw (except);
							}

						}

						#endregion

						//If the current rule has ProcessAsGroup set to True and the next line is the same as the current then continue on to the next
						if ((tr.ProcessAsGroup && ChildNode.NextSibling.Name == ChildNode.Name) && (ContinueProcessing && ContinueTransaction))
							continue;

						//if the new node has been flagged for importing into the destination document.
						if (ImportNode)
						{
							XmlNode NodeToAppend = NewDestination.ImportNode(NewDocTemplate.SelectSingleNode(tr.DestinationSavePath), true);
							//Fire the BeforeAppend event
							BeforeAppend(NodeToAppend, null);
							AppendNode(Parent, NodeToAppend);
							//Fire the afterappend event
							AfterAppend(NodeToAppend, null);
						}

						#region Process Actions Assiciated to our Rule Conditions

						//if we have actions to execute do it here
						//Clear our executed actions list incase we filled it on a previous line
						ActionsExecuted.Clear();
						foreach (string actionName in ActionsToExecute)
						{
							//First check to see if the current translate rule even has actions to perform
							if (tr.RuleActions == null)
								break;

							//it does so find the action to execute
							foreach (RuleAction act in tr.RuleActions)
							{
								if (act.ID != actionName || ActionsExecuted.Contains(actionName))
									continue;

								NewDocTemplate = DestinationDocumentTemplate;
								XmlNode NewNode = NewDocTemplate.SelectSingleNode(act.ItemToCreate);

								//Store our executed action for lookup later to make sure we do not execute this action again
								if (tr.ProcessAsGroup) //We only care about storing this value if we have the rule ProcessAsGroup = True
									ActionsExecuted.Add(actionName);

								//First take care of any attributes that get directly set.
								foreach (RuleAttribute NodeAttribute in act.Attributes)
								{
									//check to see if the attribute should use the builtin identity counter.  Incrementing the counter is user defined.
									if (!NodeAttribute.UseBuiltInIdentity)
									{
										//Handle the case where the value of an attribute is an expression instead of a standard value
										string FilteredValue = "";
										try
										{
											if (!NodeAttribute.IsExpression)
												NewNode.Attributes[NodeAttribute.ID].Value = FormatValue(NodeAttribute.format, NodeAttribute.formatType, NodeAttribute.Value);
											else
											{
												//check if the node expression should be run against the source file
                        if (NodeAttribute.ExpressionSrouce == "SourceFile")
                        {
                          //if (NodeAttribute.Value.Contains("number(@rettax) - sum(./../L38[@tax_amount &lt; 0]/@tax_amount)"))
                          //  Console.WriteLine();

                          string expressionvalue = "";
                          if (NodeAttribute.formatType == typeof(Decimal))
                            expressionvalue = Decimal.Parse(nav.Evaluate(GetXpathExpression(NodeAttribute.Value)).ToString(), System.Globalization.NumberStyles.Float).ToString("F2");
                          else
                            expressionvalue = nav.Evaluate(GetXpathExpression(NodeAttribute.Value)).ToString();

                          NewNode.Attributes[NodeAttribute.ID].Value = FormatValue(NodeAttribute.format, NodeAttribute.formatType, expressionvalue);
                        }
												//check if the node expression should be run against the destination file
												if (NodeAttribute.ExpressionSrouce == "DestinationFile")
												{
													XPathNavigator destNav = NewNode.CreateNavigator();
													NewNode.Attributes[NodeAttribute.ID].Value = FormatValue(NodeAttribute.format, NodeAttribute.formatType, destNav.Evaluate(GetXpathExpression(NodeAttribute.Value)).ToString());
												}
											}
										}
										catch (Exception ex)
										{
											RuleActionException except = new RuleActionException("Action Exception Occurred", ex);
											except.Source = SourceFile;
											except.RuleID = tr.ID;
											except.AcctionID = NodeAttribute.ID;
											except.ExpressionSource = NodeAttribute.ExpressionSrouce;
											throw (except);
										}
									}
									else
										NewNode.Attributes[NodeAttribute.ID].Value = _Counter.ToString();
								}
								//check to see if our current action has conditions it needs to execute as well
								if (act.Conditions.Count > 0)
								{
									foreach (RuleCondition condition in act.Conditions)
									{
										try
										{
											ExecuteConditions(condition, NewDocTemplate, nav, ActionsToExecute, ref ContinueProcessing);
											if (ContinueProcessing)
												continue;

											break;
										}
										catch (Exception ex)
										{
											RuleConditionException except = new RuleConditionException("Condition Exception While Processing Action", ex);
											except.RuleID = tr.ID;
											except.ConditionID = condition.ID;
											except.SourceFile = SourceFile;
											except.ConditionDestination = condition.Destination;
											except.ConditionExpression = condition.Expression;
											throw (except);
										}
									}
								}

								//Fire BeforeAppend event
								BeforeAppend(NewNode, null);
								AppendNode(Parent, NewDestination.ImportNode(NewNode, true));
								//Fire the afterappend event
								AfterAppend(NewNode, null);
							}
						}

						#endregion
						//remove our actions
						ActionsToExecute.Clear();
						ImportNode = false;
						RecordCount++;

						//Pause the thread after 5 records to keep cpu usage down.
						//int Remainder;
						//Math.DivRem(RecordCount, 4, out Remainder);
						//if (Remainder == 0)
						//Thread.Sleep(1);

						//If we shouldnt continue the current transaction break out of loop
						if (!ContinueTransaction)
							break;
					}
					TranslateEventArgs RuleProcessArgs = new TranslateEventArgs();
					RuleProcessArgs.CurrentTranslateItem = node;
					RuleProcessArgs.OutputDocument = NewDestination;
					AfterRuleProcess(node, RuleProcessArgs);
					//}
				}
			}

			//Translate complate so save the file
#if (DEBUG)
			//Console.ReadLine();
#endif

			SaveTranslatedFile(NewDestination);
			Thread.Sleep(1);
		}

		/// <summary>
		/// Condition Executeion isolated here so we can reuse this code for conditions that are acciciated to multiple elements
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="NewDocTemplate"></param>
		/// <param name="nav"></param>
		/// <param name="ActionsToExecute"></param>
		/// <param name="ContinueProcessing"></param>
		/// <param name="ImportNode"></param>
		private void ExecuteConditions(RuleCondition condition, XmlDocument NewDocTemplate, XPathNavigator nav, List<string> ActionsToExecute, ref bool ContinueProcessing, ref bool ContinueTransaction)
		{
			//Temporary addition to allow for special characters in the xml template.  this should be cleaned up.
			string newCondition = condition.Expression;

			//newCondition = newCondition.Replace("&amp", "&");
			//newCondition = newCondition.Replace("&gt;", ">");
			//newCondition = newCondition.Replace("&lt;", "<");

			bool ExpressionResult;
			//Evaluate our expression
      //if (newCondition.Contains("number(@taxes) + (sum(./../L36/@rettax) * -1)"))
			  //Console.WriteLine("");
			//if (condition.Contains("string(./../Customer/@Order_Number)"))
			//Console.WriteLine("");

			//if (Convert.ToBoolean(nav.Evaluate("count(preceding-sibling::L12) > 1")))
			//Console.WriteLine();

			//ExpressionResult = Convert.ToBoolean(nav.Evaluate(newCondition));
			ExpressionResult = Convert.ToBoolean(nav.Evaluate(GetXpathExpression(newCondition)));

			RuleValue ResultConditionValue;
			if (ExpressionResult)
				ResultConditionValue = condition.TrueValue;
			else
				ResultConditionValue = condition.FalseValue;

      //if (ResultConditionValue.Value == "number(@taxes) + (sum(./../L36/@rettax) * -1) - sum(./../L38/@tax_amount)")
      //  Console.WriteLine();

			//RuleValue ResultConditionValue = condition.TrueValue;
			if (ResultConditionValue != null && ResultConditionValue.Value != null)
			{
				//condition could be left blank if we only want to execute the action assiciated with the result
				if (!String.IsNullOrEmpty(condition.Destination))
				{
					//check to see if we want to only use the default value instead of the one assigned
					if (!ResultConditionValue.UseDefault)
					{
						//Check if the result is an expression.
						if (!ResultConditionValue.isExpression)
						{
							NewDocTemplate.SelectSingleNode(condition.Destination).Value = FormatValue(condition.format, condition.type, ResultConditionValue.Value);
							ContinueProcessing = ResultConditionValue.ContunueProcessing;
							ImportNode = true;
						}
						else
						{
							if (ResultConditionValue.ExpressionSource == ExpressionSource.SourceFile.ToString())
							{
								NewDocTemplate.SelectSingleNode(condition.Destination).Value = FormatValue(condition.format, condition.type, nav.Evaluate(GetXpathExpression(ResultConditionValue.Value)).ToString());
								ImportNode = true;
							}
							if (ResultConditionValue.ExpressionSource == ExpressionSource.DestinationFile.ToString())
							{
								XPathNavigator destinationNav = NewDocTemplate.DocumentElement.CreateNavigator();
								NewDocTemplate.SelectSingleNode(condition.Destination).Value = FormatValue(condition.format, condition.type, destinationNav.Evaluate((ResultConditionValue.Value)).ToString());
								ImportNode = true;
							}
						}
					}
				}
				//Check to see if we should continue processing our conditions
				ContinueProcessing = ResultConditionValue.ContunueProcessing;
				//Check to see if we should continue processing the current transaction.
				ContinueTransaction = ResultConditionValue.ContinueTransaction.Value;

				//Assign any actions that are needed to be performed for later processing
				if (!String.IsNullOrEmpty(ResultConditionValue.Actionid))
					ActionsToExecute.Add(ResultConditionValue.Actionid);
			}
		}

		private void ExecuteConditions(RuleCondition condition, XmlDocument NewDocTemplate, XPathNavigator nav, List<string> ActionsToExecute, ref bool ContinueProcessing)
		{
			bool Continue = true;
			ExecuteConditions(condition, NewDocTemplate, nav, ActionsToExecute, ref ContinueProcessing, ref Continue);
		}

		/// <summary>
		/// Use to append our nodes to the destination document
		/// </summary>
		/// <param name="Parent"></param>
		/// <param name="NodeToAppend"></param>
		protected virtual void AppendNode(XmlElement Parent, XmlNode NodeToAppend)
		{
			Parent.AppendChild(NodeToAppend);
		}

		protected virtual void SaveTranslatedFile(XmlDocument TranslatedDocument)
		{
			TranslatedDocument.Save(TranslatedSaveFile);
		}

		private static string FormatValue(string Format, Type FormatType, string value)
		{
      try
      {
        //if a type is defined convert it and format it to the type specified
        if (String.IsNullOrEmpty(Format))
        {
          Format = "{0:g}";
          FormatType = typeof(String);
        }

        if (String.IsNullOrEmpty(value))
          return value;

        if (FormatType == typeof(Decimal))
        {
         var  newvalue = Decimal.Parse(value, System.Globalization.NumberStyles.Float);
        }
        object ConvertedType = Convert.ChangeType(value, FormatType);
        value = String.Format(Format, ConvertedType);

        return value;
      }
      catch (Exception ex)
      {
        throw (ex);
        return value;
      }
		}

		/// <summary>
		/// Used to write out log messages based on the configured provider
		/// </summary>
		/// <param name="Text"></param>
		protected void LogMessage(string Text)
		{
			//No need to create more than one instance of this.
			if (_PluginLogger == null)
			{
				if (String.IsNullOrEmpty(PluginConfig.LoggingProvider))
					return;

				_PluginLogger = Logger.Create(PluginConfig.LoggingProvider, PluginConfig.PluginName);
			}

			if (_PluginLogger != null)
				_PluginLogger.LogMessage(Text);
		}

		#region Events

		/// <summary>
		/// Used to raise the event before a node is appended to the output document
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void BeforeAppend(object sender, EventArgs args)
		{
			if (OnBeforeNodeAppended != null)
				OnBeforeNodeAppended(sender, args);
		}

		/// <summary>
		/// Rasies the event after a node is appended to the output document
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void AfterAppend(object sender, EventArgs args)
		{
			if (OnAfterNodeAppended != null)
				OnAfterNodeAppended(sender, args);
		}

		/// <summary>
		/// Event raised just before rules start processing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void BeforeProcess(object sender, EventArgs args)
		{
			if (OnBeforeRuleProcess != null)
				OnBeforeRuleProcess(sender, args);
		}

		private void AfterRuleProcess(object sender, TranslateEventArgs args)
		{
			if (OnAfterRuleProcess != null)
				OnAfterRuleProcess(sender, args);
		}

		private void AfterRuleLoad(object sender, TranslateEventArgs args)
		{
			if (OnAfterRuleLoad != null)
				OnAfterRuleLoad(sender, args);
		}

		#endregion

		/// <summary>
		/// Gets the rules from the supplied rules file
		/// </summary>
		/// <exception cref="System.ArgumentNullException"></exception>
		protected List<IRule> TranslateRules
		{
			get
			{
				if (String.IsNullOrEmpty(RulesFile))
					throw new ArgumentNullException("RulesFile");

				if (RulesDoc == null)
				{
					RulesDoc = new XmlDocument();
					RulesDoc.Load(RulesFile);
				}

				XmlNodeList RuleNodes = RulesDoc.SelectNodes("//Rules/*");
				_Rules = new List<IRule>(RuleNodes.Count);
				foreach (XmlNode node in RuleNodes)
					_Rules.Add(new TranslateRule(RulesDoc, node.Attributes["id"].Value));

				return _Rules;
			}
		}

		/// <summary>
		/// Used to cache rules as they are searched for to make future requests for the rules faster
		/// </summary>
		/// <param name="RuleID">ID of the rule to look for</param>
		/// <returns></returns>
    private TranslateRule GetRule(string RuleID)
    {     
      if (_RuleCache == null)
        _RuleCache = new Hashtable();

      if (_RuleCache.ContainsKey(RuleID))
        return (TranslateRule)_RuleCache[RuleID];

      object foundRule = _Rules.SingleOrDefault(r => r.ID == RuleID);
      if (foundRule != null)
      {
        TranslateRule rule = (TranslateRule)foundRule;

        if (!_RuleCache.ContainsKey(RuleID))
          _RuleCache.Add(RuleID, rule);

        return rule;
      }

      return null;
    }

		/// <summary>
		/// increase the counter by 1.
		/// </summary>
		protected void IncrementCounter()
		{
			_Counter++;
		}

		/// <summary>
		/// Decrease the counter by 1
		/// </summary>
		protected void DecrementCounter()
		{
			_Counter--;
		}

		/// <summary>
		/// Reset counter
		/// </summary>
		protected void ResetCounter()
		{
			_Counter = 0;
		}

		/// <summary>
		/// Initialize Counter to be used for unique number.  Defaults to 0
		/// </summary>
		/// <param name="value">Value to initialize counter with as the starting point</param>
		protected void InitializeCounter(int value)
		{
			_Counter = value;
		}
		/// <summary>
		/// Gets the configuration of the current executing plugin 
		/// </summary>
		public TranslateConfig PluginConfig
		{
			get { return _config; }
		}

		protected int Counter
		{
			get { return _Counter; }
		}

		/// <summary>
		/// Used to cache the compiled expression for future use.
		/// </summary>
		/// <param name="Xpath">The Xpath query to cache</param>
		/// <returns></returns>
		protected static XPathExpression GetXpathExpression(string Xpath)
		{
			Xpath = Xpath.Replace("&amp", "&");
			Xpath = Xpath.Replace("&gt;", ">");
			Xpath = Xpath.Replace("&lt;", "<");

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

			try
			{
				return _CahcedExpressions[Xpath];
			}
			catch
			{
				return XPathExpression.Compile(Xpath);
			}
		}



	}

	public class TranslateEventArgs : EventArgs
	{
		private XmlNode _CurrentTranslateItem;
		private XmlDocument _CurrentOutputDoc;
		public TranslateEventArgs()
		{
		}

		public XmlNode CurrentTranslateItem
		{
			get
			{
				return _CurrentTranslateItem;
			}
			set
			{
				if (_CurrentTranslateItem == value)
					return;
				_CurrentTranslateItem = value;
			}
		}

		public XmlDocument OutputDocument
		{
			get { return _CurrentOutputDoc; }
			set
			{
				_CurrentOutputDoc = value;
			}
		}
	}

}
