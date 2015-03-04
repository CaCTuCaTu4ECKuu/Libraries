using System;
using System.Data;
using MySql.Data;
using System.Globalization;
using MySql.Data.MySqlClient;

public class TMySQL
{
    public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
    private bool _ready;
    public bool autoRetryOnTimeout;
    public int minTimeout;
    public int maxTimeout;
    public int stepOnTimeout;
    private string connectionString;
    public delegate void MessageEvent(string SQL, string Message);
    public event MessageEvent ErrorOccured;
    private void Error(string sql, string msg)
    {
        if (ErrorOccured != null)
            ErrorOccured(sql, msg);
    }

    public bool Ready
    {
        get { return _ready; }
    }

    public bool ExecSQL(string SQL)
    {
        return ExecSQL(SQL, minTimeout);
    }
    public bool ExecSQL(string SQL, int timeout)
    {
        int count = 0;
        bool end = false;
        if (_ready)
        {
            while (!end && count < 5)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        MySqlCommand cmd = new MySqlCommand(@SQL, connection);
                        cmd.CommandTimeout = timeout;
                        cmd.ExecuteNonQuery();
                    }
                    end = true;
                }
                catch (Exception Ex)
                {
                    count++;
                    if (Ex.Message.ToLower().IndexOf("timeout") >= 0)
                    {
                        if (autoRetryOnTimeout)
                        {
                            if (minTimeout < maxTimeout)
                                end = ExecSQL(SQL, minTimeout + stepOnTimeout);
                        }
                    }
                    else
                        Error(SQL, Ex.Message);
                }
            }
        }
        else
            Error("", "Not connected!");
        return end;
    }
    public DataTable GetData(string SQL)
    {
        return GetData(SQL, minTimeout);
    }
    public DataTable GetData(string SQL, int timeout)
    {
        if (_ready)
        {
            try
            {
                DataTable result = new DataTable();
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(@SQL, connection);
                    cmd.CommandTimeout = timeout;
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    adapter.Fill(result);
                }
                return result;
            }
            catch (Exception Ex)
            {
                if (Ex.Message.IndexOf("Timeout expired") == 0)
                {
                    if (autoRetryOnTimeout)
                    {
                        if (minTimeout < maxTimeout)
                            return GetData(SQL, minTimeout + stepOnTimeout);
                        else
                            return null;
                    }
                    else
                        return null;
                }
                else
                {
                    Error(SQL, Ex.Message);
                    return null;
                }
            }
        }
        else
        {
            Error("", "Not connected!");
            return null;
        }
    }
    /// <summary>
    /// Создает подключение к БД с заданными параметрами
    /// </summary>
    /// <param name="host">Хост</param>
    /// <param name="login">Имя пользователя</param>
    /// <param name="password">Пароль</param>
    /// <param name="database">База данных к которой нужно выполнить подключение</param>
    public TMySQL(string host, string login, string password, string database)
	{
        connectionString = string.Format("Data source={0};UserId={1};Password={2};database={3};Convert Zero Datetime=true;", host, login, password, database);
        try
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();
            _ready = true;
            autoRetryOnTimeout = true;
            minTimeout = 30;
            maxTimeout = 120;
            stepOnTimeout = 30;
        }
        catch (Exception Ex)
        {
            Error(connectionString, Ex.Message);
            _ready = false;
        }
	}

    public static DataTable Filter(DataTable Table, string WHERE)
    {
        if (Table != null)
        {
            DataView Filter = new DataView(Table);
            Filter.RowFilter = WHERE;
            return Filter.ToTable();
        }
        else
            return null;
    }
    public static string toSQLDateTime(DateTime datetime)
    {
        return datetime.ToString(DateTimeFormat);
    }

}