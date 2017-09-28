/*
 *  Author: Luke Watson
 *  Date 11/27/2013
 */

using System;
using System.Configuration;

namespace TLogUpdater.Library
{
    /// <summary>
    /// Operators that can be used in the config. These are for comparing values in the condition definitions
    /// </summary>
    public enum LogicOperator
    {
        Eq, Lt, Gt, Geq, Leq, Neq
    };

    /// <summary>
    /// Defines the configuration section as used in the app.config
    /// </summary>
    public class TLogUpdaterSection : ConfigurationSection
    {
        public static TLogUpdaterSection GetConfig()
        {
            return (TLogUpdaterSection)ConfigurationManager.GetSection("TLogUpdater") ?? new TLogUpdaterSection();
        }

        /// <summary>
        /// the section contains a collection of definitions
        /// </summary>
        [ConfigurationPropertyAttribute("Definitions", IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public Definitions Definitions
        {
            get { return (Definitions)base["Definitions"]; }
            set { base["Definitions"] = value; }
        }
    }


    public class Definitions : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMapAlternate;
            }
        }

        protected override string ElementName
        {
            get
            {
                return "Definition";
            }
        }

        protected override bool IsElementName(string elementName)
        {
            return elementName == "Definition";
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Definition)element).UniqueId;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Definition();
        }

        public Definition this[int index]
        {
            get { return (Definition)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public new Definition this[string resposneString]
        {
            get { return (Definition)BaseGet(resposneString); }
            set
            {
                if (BaseGet(resposneString) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(resposneString)));
                }
                BaseAdd(value);
            }
        }

    }

    public class Definition : ConfigurationElement
    {
        internal Guid UniqueId { get; set; }

        public Definition()
        {
            UniqueId = Guid.NewGuid();
        }

        [ConfigurationPropertyAttribute("UpdateDefinitions", IsDefaultCollection = false)]
        public UpdateDefinitions UpdateDefinitions
        {
            get { return (UpdateDefinitions)base["UpdateDefinitions"]; }
        }

        [ConfigurationPropertyAttribute("ConditionDefinitions", IsDefaultCollection = false)]
        public ConditionDefinitions ConditionDefinitions
        {
            get { return (ConditionDefinitions)base["ConditionDefinitions"]; }
        }
    }

    [ConfigurationCollectionAttribute(typeof(UpdateDefinition), CollectionType = ConfigurationElementCollectionType.BasicMapAlternate, AddItemName = "UpdateDefinition")]
    public class UpdateDefinitions : ConfigurationElementCollection
    {
        public UpdateDefinition this[int index]
        {
            get { return (UpdateDefinition)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public new UpdateDefinition this[string responseString]
        {
            get { return (UpdateDefinition)BaseGet(responseString); }
            set
            {
                if (BaseGet(responseString) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(responseString)));
                }
                BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new UpdateDefinition();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((UpdateDefinition)element).UniqueId;
        }
    }

    [ConfigurationCollectionAttribute(typeof(ConditionDefinition), CollectionType = ConfigurationElementCollectionType.BasicMapAlternate, AddItemName = "ConditionDefinition")]
    public class ConditionDefinitions : ConfigurationElementCollection
    {
        public ConditionDefinition this[int index]
        {
            get { return (ConditionDefinition)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public new ConditionDefinition this[string resposneString]
        {
            get { return (ConditionDefinition)BaseGet(resposneString); }
            set
            {
                if (BaseGet(resposneString) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(resposneString)));
                }
                BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ConditionDefinition();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ConditionDefinition)element).UniqueId;
        }
    }

    public class UpdateDefinition : ConfigurationElement
    {
        internal Guid UniqueId { get; set; }

        [ConfigurationProperty("LineType", DefaultValue = "-1", IsRequired = true)]
        public int LineType
        {
            get { return (int)this["LineType"]; }
            set { this["LineType"] = value; }
        }

        [ConfigurationProperty("Position", DefaultValue = "0", IsRequired = true)]
        public int Position
        {
            get { return (int)this["Position"]; }
            set { this["Position"] = value; }
        }

        [ConfigurationProperty("Value", DefaultValue = "NEW VALUE", IsRequired = true)]
        public string Value
        {
            get { return (String)this["Value"]; }
            set { this["Value"] = value; }
        }

        public UpdateDefinition()
        {
            UniqueId = Guid.NewGuid();
        }
    }

    public class ConditionDefinition : ConfigurationElement
    {
        internal Guid UniqueId { get; set; }

        [ConfigurationProperty("Position", DefaultValue = "0", IsRequired = true)]
        public int Position
        {
            get { return (int)this["Position"]; }
            set { this["Position"] = value; }
        }

        [ConfigurationProperty("Value", DefaultValue = "EXISTING VALUE", IsRequired = true)]
        public string Value
        {
            get { return (String)this["Value"]; }
            set { this["Value"] = value; }
        }

        [ConfigurationProperty("ShouldProcess", DefaultValue = "false", IsRequired = true)]
        public bool ShouldProcess
        {
            get { return (bool)this["ShouldProcess"]; }
            set { this["ShouldProcess"] = value; }
        }

        [ConfigurationProperty("Logic", DefaultValue = "Eq", IsRequired = true)]
        public LogicOperator Logic
        {
            get { return (LogicOperator)this["Logic"]; }
            set { this["Logic"] = value; }
        }

        [ConfigurationProperty("Type", DefaultValue = "String", IsRequired = true)]
        public string Type
        {
            get { return (String)this["Type"]; }
            set { this["Type"] = value; }
        }

        public ConditionDefinition()
        {
            UniqueId = Guid.NewGuid();
        }

    }

}
