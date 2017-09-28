using System;
using System.Collections.Generic;
using System.Xml;

namespace SpencerGifts.Translate
{
  public sealed class TranslateRule : IRule
  {
    string _RuleID;
    string _RuleDescription;
    string _DestinationSavePath;
    bool _ProcessAsGroup;
    bool _MapExact;
    List<RuleMapping> _RuleMappings;
    List<RuleCondition> _RuleConditions;
    List<RuleAction> _RuleActions;    
    XmlDocument _RulesDoc = null;

    #region Constructors
    public TranslateRule(string RulesFile, string RuleID)
    {
      if (_RulesDoc == null)
      {
        _RulesDoc = new System.Xml.XmlDocument();
        _RulesDoc.Load(RulesFile);
      }
      
      _RuleMappings = new List<RuleMapping>();
      _RuleConditions = new List<RuleCondition>();
      _RuleActions = new List<RuleAction>();

      _RuleID = RuleID;
      LoadRule();
    }
    public TranslateRule(XmlDocument RulesDocument, string RuleID)
    {
      _RulesDoc = RulesDocument;
      _RuleMappings = new List<RuleMapping>();
      _RuleConditions = new List<RuleCondition>();
      _RuleActions = new List<RuleAction>();

      _RuleID = RuleID;
      LoadRule();
    }
    public TranslateRule(string RulesFile, string RuleID, bool RefreshRulesDoc) : this(RulesFile, RuleID)
    {
      if (RefreshRulesDoc)
        _RulesDoc = null;
    }
    public TranslateRule(string RulesFile)
    {
      if (_RulesDoc == null)
      {
        _RulesDoc = new System.Xml.XmlDocument();
        _RulesDoc.Load(RulesFile);
      }

      _RulesDoc.Load(RulesFile);
      _RuleMappings = new List<RuleMapping>();
      _RuleConditions = new List<RuleCondition>();
      _RuleActions = new List<RuleAction>();
    }
    public TranslateRule(string RulesFile, bool RefreshRulesDoc) : this(RulesFile)
    {
      if (RefreshRulesDoc)
      _RulesDoc = null;
  }
    #endregion

  #region IRule Members
  /// <summary>
    /// Gets or sets the id of the rule
    /// </summary>
    public string ID
    {
      get
      {
        return _RuleID;
      }
      set
      {
        _RuleID = value;
      }
    }
    /// <summary>
    /// Gets or Sets the description of the rule
    /// </summary>
    public string Description
    {
      get
      {
        return _RuleDescription;
      }
      set
      {
        _RuleDescription = value;
      }
    }
    /// <summary>
    /// Gets or sets the destination path a file will be saved to
    /// </summary>
    public string DestinationSavePath
    {
      get { return _DestinationSavePath; }
      set { _DestinationSavePath = value; }
    }
    /// <summary>
    /// Gets the value specifying the rule should be processed as a group.
    /// </summary>
    public bool ProcessAsGroup
    {
      get { return _ProcessAsGroup; }
    }
    /// <summary>
    /// Gets the value indicating the rule should map 1 to 1 to the destination.
    /// </summary>
    public bool ExactMapping
    {
      get{return _MapExact;}
    }
    /// <summary>
    /// Gets or sets the mappings for the current plugin
    /// </summary>
    public List<RuleMapping> RuleMappings
    {
      get
      {
        return _RuleMappings;
      }
      set
      {
        _RuleMappings = value;
      }
    }
    /// <summary>
    /// Gets or sets the conditions for the current plugin
    /// </summary>
    public List<RuleCondition> RuleConditions
    {
      get
      {
        return _RuleConditions;
      }
      set
      {
        _RuleConditions = value;
      }
    }
    /// <summary>
    /// Gets or sets any actions associated to the conditions.
    /// </summary>
    public List<RuleAction> RuleActions
    {
      get { return _RuleActions; }
      set { _RuleActions = value; }
    }

    #endregion

    /// <summary>
    /// Loads a translate rule
    /// </summary>
    public void LoadRule()
    {      
      XmlNode RuleNode = _RulesDoc.SelectSingleNode(String.Format("RuleEngine/Rules/Rule[@id='{0}']", _RuleID));     
      _RuleDescription = RuleNode.Attributes["desc"].Value;      
      _DestinationSavePath = RuleNode.Attributes["DestinationSavePath"].Value;      
      _ProcessAsGroup = Convert.ToBoolean(RuleNode.Attributes["ProcessAsGroup"].Value);     
      _MapExact = Convert.ToBoolean(RuleNode.Attributes["ExactMapping"] == null ? "false" : RuleNode.Attributes["ExactMapping"].Value);
      XmlNodeList MappingList = RuleNode.SelectNodes("Mappings/*");

      //add all mapings
      for (int x = 0, count = MappingList.Count; x < count; x++)
      {
        RuleMapping map = new RuleMapping();
        map.SourcePath = MappingList[x].SelectSingleNode("Source").InnerXml;
        map.DestinationPath = MappingList[x].SelectSingleNode("Destination").InnerXml;

        if (MappingList[x].Attributes != null && MappingList[x].Attributes["format"] != null)
        {
          map.Format = MappingList[x].Attributes["format"].Value;
          map.FormatType = Type.GetType(String.Format(MappingList[x].Attributes["type"].Value), true, true);
        }        
        _RuleMappings.Add(map);
      }

      //Add all conditions now
      XmlNodeList ConditionList = RuleNode.SelectNodes("Conditions/*");
      for (int x = 0, count = ConditionList.Count; x < count; x++)
      {
        RuleCondition condition = PopulateCondition(ConditionList[x]);

        if (condition != null)          
          _RuleConditions.Add(condition);

      }

      //Load All Actions
      XmlNodeList ActionList = RuleNode.SelectNodes("Actions/Action/*");
      for (int x = 0, count = ActionList.Count; x < count; x++)
      {

        RuleAction action = new RuleAction();
        XmlNode CurrentNode = ActionList[x];

        if (CurrentNode.Name == "CreateLineItem")
        {
          action.ID = CurrentNode.SelectSingleNode("@id").Value;
          action.ItemToCreate = CurrentNode.SelectSingleNode("@ItemToCreate").Value;

          if (CurrentNode.HasChildNodes)
          {
            foreach (XmlNode node in ActionList[x].ChildNodes)
            {
              if (node.Name == "Attribute")
              {
                RuleAttribute att = new RuleAttribute();
                //att.ID = node.SelectSingleNode("@id").Value;
                att.ID = node.Attributes["id"].Value;
                att.Value = node.InnerXml;
                att.UseBuiltInIdentity = node.Attributes["useIdentity"] != null ? Convert.ToBoolean(node.Attributes["useIdentity"].Value) : false;
                att.IsExpression = node.Attributes["isExpression"] != null ? Convert.ToBoolean(node.Attributes["isExpression"].Value) : false;
                att.ExpressionSrouce = node.Attributes["ExpressionSource"] != null ? node.Attributes["ExpressionSource"].Value : "";

                if (node.Attributes != null && node.Attributes["format"] != null)
                {
                  att.format = node.Attributes["format"].Value;
                  att.formatType = Type.GetType(String.Format(node.Attributes["type"].Value), true, true);
                }
                action.Attributes.Add(att);
              }
              if (node.Name == "Conditions")
              {
                XmlNodeList conditionList = node.SelectNodes("Condition");
                foreach (XmlNode cNode in conditionList)
                {
                  RuleCondition AttributeCondition = PopulateCondition(cNode);
                  if (AttributeCondition != null)
                    action.Conditions.Add(AttributeCondition);
                }
              }
            }
          }          
          _RuleActions.Add(action);
        }
      }
    }
    /// <summary>
    /// Loads a translate rule
    /// </summary>
    /// <param name="RuleID">The id of the rule to load</param>
    public void LoadRule(string RuleID)
    {
      _RuleID = RuleID;
      LoadRule();
    }
    /// <summary>
    /// Populates a rule condition
    /// </summary>
    /// <param name="ConditionNode"></param>
    /// <returns></returns>
    private RuleCondition PopulateCondition(XmlNode ConditionNode)
    {
      RuleCondition condition = new RuleCondition();
      System.Xml.XPath.XPathNavigator nav = ConditionNode.CreateNavigator();

      condition.Expression = nav.SelectSingleNode(TranslateExpressionCache.GetXpathExpression("Expression")).InnerXml;
      condition.ID = ConditionNode.Attributes["id"] == null ? "" : ConditionNode.Attributes["id"].Value.ToString();
      condition.Name = ConditionNode.Attributes["name"] == null ? "" : ConditionNode.Attributes["name"].Value.ToString();
      condition.Destination = nav.SelectSingleNode(TranslateExpressionCache.GetXpathExpression("Destination")).InnerXml;
      XmlNode DestinationNode = ConditionNode.SelectSingleNode("Destination");

      if (DestinationNode.Attributes != null && DestinationNode.Attributes["format"] != null)
      {
        condition.format = DestinationNode.Attributes["format"].Value;
        condition.type = Type.GetType(String.Format(DestinationNode.Attributes["type"].Value), true, true);

      }

      //Populate attributes for true values
      XmlNode ConditionTrueNode = ConditionNode.SelectSingleNode("True");
      if (ConditionTrueNode != null)
      {
        condition.TrueValue.Value = ConditionTrueNode.InnerXml;
        condition.TrueValue.ExpressionSource = ConditionTrueNode.Attributes["ExpressionSource"] == null ? "" : ConditionTrueNode.Attributes["ExpressionSource"].Value;
        condition.TrueValue.isExpression = ConditionTrueNode.Attributes["isExpression"] == null ? false : Convert.ToBoolean(ConditionTrueNode.Attributes["isExpression"].Value);
        condition.TrueValue.UseDefault = ConditionTrueNode.Attributes["UseDefault"] == null ? false : Convert.ToBoolean(ConditionTrueNode.Attributes["UseDefault"].Value);
        condition.TrueValue.ContunueProcessing = ConditionTrueNode.Attributes["ContinueProcessing"] == null ? true : Convert.ToBoolean(ConditionTrueNode.Attributes["ContinueProcessing"].Value);
        condition.TrueValue.Actionid = ConditionTrueNode.Attributes["Actionid"] == null ? "" : ConditionTrueNode.Attributes["Actionid"].Value;
        condition.TrueValue.ContinueTransaction = ConditionTrueNode.Attributes["ContinueTransaction"] == null ? true : Convert.ToBoolean(ConditionTrueNode.Attributes["ContinueTransaction"].Value);
      }
      //populate attributes for false values
      XmlNode ConditionFalseNode = ConditionNode.SelectSingleNode("False");
      if (ConditionFalseNode != null)
      {
        condition.FalseValue.Value = ConditionFalseNode.InnerXml;
        condition.FalseValue.ExpressionSource = ConditionFalseNode.Attributes["ExpressionSource"] == null ? "" : ConditionFalseNode.Attributes["ExpressionSource"].Value;
        condition.FalseValue.isExpression = ConditionFalseNode.Attributes["isExpression"] == null ? false : Convert.ToBoolean(ConditionFalseNode.Attributes["isExpression"].Value);
        condition.FalseValue.UseDefault = ConditionFalseNode.Attributes["UseDefault"] == null ? false : Convert.ToBoolean(ConditionFalseNode.Attributes["UseDefault"].Value); ;
        condition.FalseValue.ContunueProcessing = ConditionFalseNode.Attributes["ContinueProcessing"] == null ? true : Convert.ToBoolean(ConditionFalseNode.Attributes["ContinueProcessing"].Value);
        condition.FalseValue.Actionid = ConditionFalseNode.Attributes["Actionid"] == null ? "" : ConditionFalseNode.Attributes["Actionid"].Value;
        condition.FalseValue.ContinueTransaction = ConditionFalseNode.Attributes["ContinueTransaction"] == null ? true : Convert.ToBoolean(ConditionFalseNode.Attributes["ContinueTransaction"].Value);
      }

      if (condition.TrueValue != null && condition.FalseValue != null)
        return condition;
      else
        return null;      
    }
  }
}

