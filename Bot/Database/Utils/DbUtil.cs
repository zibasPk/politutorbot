using Serilog;

namespace Bot.Database.Utils;

#region

using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

#endregion

public static class DbUtil
{
    public static int Execute(string query, MySqlConnection connection,
        Dictionary<string, object?>? args = null)
    {
        Log.Debug($"Executing query: {query} from db util");
        return ExecuteSlave(query, connection, args);
    }

    public static int ExecuteNoLog(string query, MySqlConnection connection,
        Dictionary<string, object?>? args = null)
    {
        return ExecuteSlave(query, connection, args);
    }

    private static int ExecuteSlave(string query, MySqlConnection connection,
        Dictionary<string, object?>? args = null)
    {
        var cmd = new MySqlCommand(query, connection);
        OpenConnection(connection);

        if (args != null)
            foreach (var (key, value) in args)
                cmd.Parameters.AddWithValue(key, value);

        var numberOfRowsAffected = cmd.ExecuteNonQuery();

        return numberOfRowsAffected;
    }

    public static DataTable? ExecuteSelect(string query, MySqlConnection connection,
        Dictionary<string, object?>? args = null)
    {
        Log.Debug($"Executing select: {query} from db util");

        return ExecuteSelectSlave(query, connection, args);
    }


    public static DataTable? ExecuteSelectNoLog(string query, MySqlConnection connection,
        Dictionary<string, object?>? args = null)
    {
        return ExecuteSelectSlave(query, connection, args);
    }

    private static DataTable? ExecuteSelectSlave(string query, MySqlConnection connection,
        Dictionary<string, object?>? args = null)
    {
        var ret = new DataSet();

        var cmd = new MySqlCommand(query, connection);

        if (args != null)
            foreach (var (key, value) in args)
                cmd.Parameters.AddWithValue(key, value);

        OpenConnection(connection);

        var adapter = new MySqlDataAdapter
        {
            SelectCommand = cmd
        };
        
        adapter.Fill(ret);
        adapter.Dispose();

        return ret.Tables[0];
    }

    private static void OpenConnection(IDbConnection connection)
    {
        if (connection.State != ConnectionState.Open)
            connection.Open();
    }

    public static object? GetFirstValueFromDataTable(DataTable? dt)
    {
        if (dt == null)
            return null;

        try
        {
            return dt.Rows[0].ItemArray[0];
        }
        catch
        {
            return null;
        }
    }

    public static long? GetIntFromColumn(DataRow dr, string columnName)
    {
        var o = dr[columnName];
        if (o is null or DBNull)
            return null;

        try
        {
            return Convert.ToInt64(o);
        }
        catch
        {
            return null;
        }
    }
}