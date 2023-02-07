using System.Data;
using Bot.Database.Utils;
using MySql.Data.MySqlClient;

namespace Bot.Database.Dao;

public class HistoryDAO : DAO
{
  public HistoryDAO(MySqlConnection connection) : base(connection)
  {
  }

  /// <summary>
  /// Finds tutoring history from db.
  /// </summary>
  /// <returns>List of rows in tutoring history.</returns>
  public List<object> FindTutoringHistory(out List<string> columnNames)
  {
    return FindHistory("SELECT * from tutoring_history", out columnNames);
  }

  /// <summary>
  /// Finds active tutoring history from db.
  /// </summary>
  /// <returns>List of rows in active tutoring history.</returns>
  public List<object> FindActiveTutoringHistory(out List<string> columnNames)
  {
    return FindHistory("SELECT * from active_tutoring_history", out columnNames);
  }

  /// <summary>
  /// Finds reservation history from db.
  /// </summary>
  /// <returns>List of rows in reservation history.</returns>
  public List<object> FindReservationHistory(out List<string> columnNames)
  {
    return FindHistory("SELECT * from reservation_history", out columnNames);
  }

  private List<object> FindHistory(string query, out List<string> columnNames)
  {
    Connection.Open();
    var objRows = new List<object>();
    columnNames = new List<string>();
    try
    {
      var data = DbUtil.ExecuteSelect(query, Connection);
      if (data == null)
      {
        Connection.Close();
        return objRows;
      }

      var rows = data.Rows;
      objRows = (from DataRow row in rows select row.ItemArray).Cast<object>().ToList();
      columnNames = data.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToList();
      Connection.Close();
      return objRows;
    }
    catch (Exception)
    {
      Connection.Close();
      throw;
    }
  }
}