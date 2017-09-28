using System;
using System.Collections.Generic;
using System.Text;

namespace SpencerGifts.Translate
{
  public sealed class RuleAction
  {
    string _ID;
    string _ItemToCreate;
    List<RuleAttribute> _Attributes;
    List<RuleCondition> _Conditions;

    public string ID
    {
      get { return _ID; }
      set { _ID = value; }
    }

    public string ItemToCreate
    {
      get { return _ItemToCreate; }
      set { _ItemToCreate = value; }
    }

    public List<RuleAttribute> Attributes
    {
      get {
        if (_Attributes == null)
          _Attributes = new List<RuleAttribute>();

        return _Attributes; }
      set { _Attributes = value; }
    }

    public List<RuleCondition> Conditions
    {
      get{
        if (_Conditions == null)
          _Conditions = new List<RuleCondition>();
        
        return _Conditions;}
        set { _Conditions = value; }
    }

  }
}
