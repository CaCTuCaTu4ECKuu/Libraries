using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;


public class SocketBody
{
    public const string VERSION = "1.2"; // Версия
    protected int _port;
    protected string _adress;
    protected bool _active;
    public int Port
    {
        get { return _port; }
        set
        {
            if (!_active && checkPort(value))
                _port = value;
            else
            {
            }
        }
    }
    public bool checkPort(int nPort)
    {
        return nPort < 65536;
    }
    public bool checkIP(string nAdress)
    {
        IPAddress n;
        if (IPAddress.TryParse(nAdress, out n))
            return true;
        else
            return false;
    }
    public bool checkPIP(int sPort, string sAdress)
    {
        return (checkPort(sPort) && checkIP(sAdress));
    }
}

public class TServerSocket : SocketBody
{
    public delegate void SimpleEvent(string GUID);
    public delegate void MessageEvent(string GUID, string msg);

    public class Connection
    {
        private bool _active;
        private string _GUID;
        private TcpClient Client;
        private StreamReader Reader;
        private StreamWriter Writer;
        private Thread Watch;

        public string GUID
        {
            get { return _GUID; }
        }
        public bool Active
        {
            get { return _active; }
        }

        public event MessageEvent onRecieve;
        public event MessageEvent onError;
        public event SimpleEvent onDisconnect;

        private void _error(string msg)
        {
            if (onError != null)
                onError(_GUID, msg);
        }
        private void read()
        {
            string response;
            try
            {
               while ((response = Reader.ReadLine()) != "")
               {
                   if (response == null)
                       CloseConnection();
                   else
                       onRecieve(_GUID, response);
               }
            }
            catch (Exception Ex)
            {
                if (_active)
                {
                    _error(Ex.Message);
                    CloseConnection();
                }
            }            
        }

        public void Send(string msg)
        {
            try
            {
                Writer.WriteLine(msg);
                Writer.Flush();
            }
            catch (Exception Ex)
            {
                _error(Ex.Message);
                CloseConnection();
            }
        }

        public void CloseConnection()
        {
            _active = false;
            try
            {
                Client.Close();
                Writer.Close();
                Reader.Close();
            }
            catch (Exception Ex)
            {
                _error(Ex.Message);
            }
            finally
            {
                if (onDisconnect != null)
                    onDisconnect(GUID);
            }
        }
        public Connection(TcpClient tClient)
        {
            Client = tClient;
            try
            {
                Reader = new StreamReader(Client.GetStream());
                Writer = new StreamWriter(Client.GetStream());
                Writer.WriteLine(VERSION);
                Writer.Flush();
                _GUID = Reader.ReadLine();
            }
            catch (Exception Ex)
            {
                _error(Ex.Message);
                CloseConnection();
            }
            if (_GUID != null && GUID != "")
            {
                Watch = new Thread(read);
                _active = true;
                Watch.Start();
            }
            else
                _error("Версия не подходит или клиент закрылся");
        }
    }

    private TcpListener _tcpListener;
    private Thread _thrListener;
    private object conn = new object();
    private Hashtable _connections = new Hashtable();

    /// <summary>
    /// Включение и отключение сервера, статус прослушивания
    /// </summary>
    public bool Active
    {
        get { return _active; }
        set
        {
            switch (value)
            {
                case true:
                    if (!_active)
                        Start();
                    else
                        _error("Server", "Server already running");
                    break;
                case false:
                    if (_active)
                        Stop();
                    else
                        _error("Server", "Server not running");
                    break;
            }
        }
    }
    /// <summary>
    /// Список идентификаторов всех открытых подключений
    /// </summary>
    public List<string> Clients
    {
        get
        {
            List<string> res = new List<string>();
            if (_connections.Count > 0)
            {
                Monitor.Enter(conn);
                foreach (DictionaryEntry c in _connections)
                    res.Add((string)c.Key);
                Monitor.Exit(conn);
            }
            return res;
        }
    }
    /// <summary>
    /// Количество активных подключений
    /// </summary>
    public int Count
    {
        get { return _connections.Count; }
    }

    /// <summary> 
    /// Отправка данные клиенту по его GUID
    /// </summary>
    /// <exception>ArgumentException</exception>
    public void Send(string cGUID, string message)
    {
        if (_connections.ContainsKey(cGUID))
        {
            Connection t = (Connection)_connections[cGUID];
            t.Send(message);
        }
        else
            throw new ArgumentException("Идентификатор должен присутствовать в списке клиентов");

    }
    public void Close(string cGUID)
    {
        _disconnect(cGUID);
    }
    public void CloseAll()
    {
        foreach (string cGUID in Clients)
            _disconnect(cGUID);
    }

    public event SimpleEvent OnConnect;
    public event SimpleEvent OnDisconnect;
    public event SimpleEvent OnStartListening;
    public event SimpleEvent OnStopListening;
    public event MessageEvent OnRecieve;
    public event MessageEvent OnError;

    /// <summary>
    /// Ждет новых подключений
    /// </summary>
    private void listen()
    {
        if (OnStartListening != null)
            OnStartListening(Guid.Empty.ToString());
        while (_active)
        {
            try
            {
                _connect(_tcpListener.AcceptTcpClient());
            }
            catch (SocketException SocketEx)
            {
                if (_active || SocketEx.SocketErrorCode != SocketError.Interrupted)
                    _error(Guid.Empty.ToString(), SocketEx.Message);
            }
            catch (Exception Ex)
            {
                _error(Guid.Empty.ToString(), Ex.Message);
            }
        }
        if (OnStopListening != null)
            OnStopListening(Guid.Empty.ToString());
    }

    /// <summary>
    /// Создает клиента и подписывается на его события. Вызывает событие подключения
    /// </summary>
    /// <param name="Client">Только что подключившийся клиент</param>
    /// <param name="GUID">Идентификатор клиента</param>
    private void _connect(TcpClient Client)
    {
        Connection t;        
        if ((t = new Connection(Client)).Active)
        {
            t.onDisconnect += _disconnect;
            t.onError += _error;
            t.onRecieve += _parse;
            Monitor.Enter(conn);
            _connections.Add(t.GUID, t);
            if (OnConnect != null)
                OnConnect(t.GUID);
            Monitor.Exit(conn);
        }
        
    }
    /// <summary>
    /// Отписывается от событий клиента и убирает клиента из списка, вызывает событие отключения
    /// </summary>
    /// <param name="GUID">Идентификатор клиента</param>
    private void _disconnect(string GUID)
    {
        Monitor.Enter(conn);
        if (_connections.ContainsKey(GUID))
        {
            Connection t = (Connection)_connections[GUID];
            t.onDisconnect -= _disconnect;
            t.onError -= _error;
            t.onRecieve -= _parse;
            t.CloseConnection();
            _connections.Remove(GUID);
            if (OnDisconnect != null)
                OnDisconnect(GUID);
        }
        Monitor.Exit(conn);
    }
    /// <summary>
    /// Вызывает событие получения сообщения
    /// </summary>
    /// <param name="GUID">Идентификатор клиента, от которого получено сообщение</param>
    /// <param name="msg">Сообщение</param>
    private void _parse(string GUID, string msg)
    {
        if (OnRecieve != null)
            OnRecieve(GUID, msg);
    }
    /// <summary>
    /// Вызывает событие ошибки
    /// </summary>
    /// <param name="GUID">Идентификатор клиента от которого получено сообщение. Если сообщение вызвано сервером то идентификатор будет содержать нули</param>
    /// <param name="msg">Текст ошибки</param>
    private void _error(string GUID, string msg)
    {
        if (OnError != null)
            OnError(GUID, msg);
    }

    /// <summary>
    /// Создает клиента на указанном порту и начинает слушать этот порт
    /// </summary>
    public void Start()
    {
        if (!_active)
        {
            try
            {
                IPAddress adr;
                if (!IPAddress.TryParse(_adress, out adr))
                    adr = IPAddress.Any;
                _tcpListener = new TcpListener(adr, _port);
                _tcpListener.Start();
                _active = true;
                _thrListener = new Thread(listen);
                _thrListener.Start();
            }
            catch (Exception Ex)
            {
                _error(Guid.Empty.ToString(), Ex.Message);
            }
        }
        else
            _error(Guid.Empty.ToString(), "Server already running");
    }
    /// <summary>
    /// Останавливает прослушивание порта и закрывает все открытые соединения
    /// </summary>
    public void Stop()
    {
        if (_active)
        {
            _active = false;
            _tcpListener.Stop();
            CloseAll();
        }
        else
            _error(Guid.Empty.ToString(), "Сервер не запущен");
    }
    public TServerSocket()
    {
        _port = 15150;
        _active = false;
    }
    ~TServerSocket()
    {
        CloseAll();
    }
}

public class TClientSocket : SocketBody
{
    public delegate void SimpleEvent();
    public delegate void MessageEvent(string msg);
    public delegate void DataRecieve(byte[] data);

    private string GUID;
    private TcpClient tcpClient;
    private StreamWriter Writer;
    private StreamReader Reader;
    private Thread Watch;

    /// <summary> 
    /// Адрес сервера
    /// </summary>
    public string Adress
    {
        get { return _adress; }
        set
        {
            if (!_active)
            {
                if (checkIP(value))
                    _adress = value;
                else
                    _error("Illegal IP-Adress");
            }
            else
                _error("Can not be changed while connected!");
        }
    }
    /// <summary> 
    /// Статус работы клиента
    /// </summary>
    public bool Active
    {
        get { return _active; }
        set
        {
            switch (value)
            {
                case true:
                    if (!_active)
                        Connect(_port, _adress);
                    else
                        _error("Already connected");
                    break;
                case false:
                    if (_active)
                        Disconnect();
                    else
                        _error("Not connected");
                    break;
            }
        }
    }

    /// <summary> 
    /// Отправить серверу сообщение
    /// </summary>
    public void Send(string msg)
    {
        if (_active)
        {
            try
            {
                Writer.WriteLine(msg);
                Writer.Flush();
            }
            catch (Exception Ex)
            {
                _error(Ex.Message);
                Disconnect();
            }
        }
        else
            _error("Not Connected");
    }

    private void read()
    {
        string response;
        if (OnConnect != null)
            OnConnect();
        try
        {
            while ((response = Reader.ReadLine()) != "")
            {
                if (response == null)
                    Disconnect();
                else
                {
                    if (OnRecieve != null)
                        OnRecieve(response);
                }
            }
        }
        catch (Exception Ex)
        {
            if (_active)
            {
                _error(Ex.Message);
                Disconnect();
            }
        } 
    }

    public event SimpleEvent OnConnect;
    public event SimpleEvent OnDisconnect;
    public event MessageEvent OnRecieve;
    public event MessageEvent OnError;
    
    private void _error(string Message)
    {
        if (OnError != null)
            OnError(Message);
    }
    
    /// <summary> 
    /// Подключиться к серверу
    /// </summary>
    public void Connect(int nPort, string nAdress)
    {
        if (checkPIP(nPort, nAdress))
        {
            _port = nPort;
            _adress = nAdress;
            Connect();
        }
        else
        {
            _error("Bad IP Adress or Port. Connection not established");
            setDef();
        }
    }
    /// <summary> 
    /// Подключиться к серверу
    /// </summary>
    private void Connect()
    {
        if (!_active)
        {
            bool connected = false;
            string serverVersion;
            try
            {
                tcpClient = new TcpClient(_adress, _port);
                Reader = new StreamReader(tcpClient.GetStream());
                Writer = new StreamWriter(tcpClient.GetStream());
                serverVersion = Reader.ReadLine();
                if (serverVersion != null && serverVersion == VERSION)
                {
                    Writer.WriteLine(GUID);
                    Writer.Flush();
                    connected = true;
                }
                else
                {
                    if (serverVersion != null)
                        _error("Версия не подходит");
                    else
                        _error("Соединение потеряно");
                    Disconnect();
                }
            }
            catch (Exception Ex)
            {
                _error(Ex.Message);
                Disconnect();
            }
            if (connected)
            {
                Watch = new Thread(read);
                _active = true;
                Watch.Start();
            }
        }
        else
            _error("Клиент уже подключен");
    }
    /// <summary> 
    /// Отключиться от сервера
    /// </summary>
    public void Disconnect()
    {      
        _active = false;
        if (tcpClient != null)
        {
            tcpClient.Close();
            Writer.Close();
            Reader.Close();
        }
        if (Writer != null && Reader != null)
        {
            tcpClient = null;  Writer = null; Reader = null;
            if (OnDisconnect != null)
                OnDisconnect();
        }
    }

    private void setDef()
    {
        _port = 15150;
        _adress = "127.0.0.1";
        _active = false;
        GUID = Guid.NewGuid().ToString();
    }
    public TClientSocket()
    {
        setDef();
    }
    public TClientSocket(int sPort, string sAdress)
    {
        if (checkPIP(sPort, sAdress))
        {
            _port = sPort;
            _adress = sAdress;
            _active = false;
        }
        else
        {
            _error("Bad IP-Adress or Port. Installed default values");
            setDef();
        }
    }
}