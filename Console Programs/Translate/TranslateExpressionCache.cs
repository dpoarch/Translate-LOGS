using System;
using System.Collections.Generic;
using System.Xml.XPath;

namespace SpencerGifts.Translate
{
  /// <summary>
  /// Utility classed used for caching xpath expressions to make future requests for the same expression more efficient.
  /// </summary>
  static class TranslateExpressionCache
  {
    /// <summary>
    /// Static collection used to hold the expressions
    /// </summary>
    private static Dictionary<string, XPathExpression> _CahcedExpressions;
    /// <summary>
    /// object used to lock threads from trying to add the same expression to the collection.
    /// </summary>
    private static object threadLock = new object();

    /// <summary>
    /// Used to add an Xpath path query to the cache
    /// </summary>
    /// <param name="Xpath"></param>
    /// <returns></returns>
    public static XPathExpression GetXpathExpression(string Xpath)
    {
			Xpath = Xpath.Replace("&amp", "&");
			Xpath = Xpath.Replace("&gt;", ">");
			Xpath = Xpath.Replace("&lt;", "<");

      if (_CahcedExpressions == null)
      {
        lock (threadLock)
        {
          if (_CahcedExpressions == null)
            _CahcedExpressions = new Dictionary<string, XPathExpression>();
        }
      }

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
        lock (threadLock)
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
}
