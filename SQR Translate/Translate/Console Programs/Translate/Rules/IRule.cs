using System;
using System.Collections.Generic;
using System.Text;

namespace SpencerGifts.Translate
{
  public interface IRule
  {
    string ID
    {
      get;
      set;
    }
    string Description
    {
      get;
      set;
    }

    string DestinationSavePath
    {
      get;
      set;
    }

    List<RuleMapping> RuleMappings
    {
      get;
      set;
    }

    List<RuleCondition> RuleConditions
    {
      get;
      set;
    }

    List<RuleAction> RuleActions
    {
      get;
      set;
    }

  }
}
