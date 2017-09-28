using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;

namespace SpencerGifts.Translate.Plugin.TLog.IOrders
{
  /// <summary>
  /// Class used to sort objects
  /// </summary>
  public class Comparer<T> : IComparer<T>
  {
    private List<SortClass> _sortClasses;

    /// <summary>
    /// The collection of sorting classes
    /// </summary>
    public List<SortClass> SortClasses
    {
      get { return _sortClasses; }
    }

    /// <summary>
    /// Default Constructor
    /// </summary>
    public Comparer()
    {
      _sortClasses = new List<SortClass>();
    }

    /// <summary>
    /// Constructor that takes a collection of sorting classes
    /// </summary>
    /// <param name="SortClasses">The prebuilt collection of sort information</param>
    public Comparer(List<SortClass> SortClasses)
    {
      _sortClasses = SortClasses;
    }

    /// <summary>
    /// Constructor that takes the information about one sort
    /// </summary>
    /// <param name="SortColumn">The column to sort on</param>
    /// <param name="SortDirection">The direction to sort</param>
    public Comparer(string SortColumn, SortDirection SortDirection)
    {
      _sortClasses = new List<SortClass>();
      _sortClasses.Add(new SortClass(SortColumn, SortDirection));
    }

    /// <summary>
    /// IComparer interface implementation to compare two objects
    /// </summary>
    /// <param name="x">Object 1</param>
    /// <param name="y">Object 2</param>
    /// <returns></returns>
    public int Compare(T x, T y)
    {
      if (SortClasses.Count == 0)
        return 0;

      return CheckSort(0, x, y);
    }

    /// <summary>
    /// Recursive function to do sorting
    /// </summary>
    /// <param name="SortLevel">The current level we are sorting at</param>
    /// <param name="MyObject1">Object 1</param>
    /// <param name="MyObject2">Object 2</param>
    /// <returns></returns>
    private int CheckSort(int SortLevel, T MyObject1, T MyObject2)
    {
      int returnVal = 0;

      if (SortClasses.Count - 1 >= SortLevel)
      {
        object valueOf1 = MyObject1.GetType().GetProperty(SortClasses[SortLevel].SortColumn).GetValue(MyObject1, null);
        object valueOf2 = MyObject2.GetType().GetProperty(SortClasses[SortLevel].SortColumn).GetValue(MyObject2, null);

        if (SortClasses[SortLevel].SortDirection == SortDirection.Ascending)
          returnVal = ((IComparable)valueOf1).CompareTo(valueOf2);
        else
          returnVal = ((IComparable)valueOf2).CompareTo(valueOf1);

        if (returnVal == 0)
          returnVal = CheckSort(SortLevel + 1, MyObject1, MyObject2);
      }
      return returnVal;
    }
  }

  /// <summary>
  /// Enumeration to determine sorting direction
  /// </summary>
  public enum SortDirection
  {
    /// <summary>Sort Ascending</summary>
    Ascending = 1,
    /// <summary>Sort Descending</summary>
    Descending = 2
  }

  /// <summary>
  /// Class used to hold sort information
  /// </summary>
  public class SortClass
  {
    /// <summary>
    /// Default constructor taking a column and a direction
    /// </summary>
    /// <param name="SortColumn">The column to sort on</param>
    /// <param name="SortDirection">The direction to sort.</param>
    public SortClass(string SortColumn, SortDirection SortDirection)
    {
      this.SortColumn = SortColumn;
      this.SortDirection = SortDirection;
    }

    private string _sortColumn;

    /// <summary>
    /// The column to sort on
    /// </summary>
    public string SortColumn
    {
      get { return _sortColumn; }
      set { _sortColumn = value; }
    }

    private SortDirection _sortDirection;

    /// <summary>
    /// The direction to sort
    /// </summary>
    public SortDirection SortDirection
    {
      get { return _sortDirection; }
      set { _sortDirection = value; }
    }
  }
}