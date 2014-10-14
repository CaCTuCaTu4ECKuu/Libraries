using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;

namespace BiosFramework
{
    namespace BinarySocket
    {
        public class SocketBase
        {
            protected const UInt32 build = 4;
            protected bool _active = false;
            protected string _srvGuid;
            protected string _adress = "127.0.0.1";
            protected int _port = 15150;

            public string Adress
            {
                get { return _adress; }
                set 
                { 
                    if (_active)
                        _adress = value; 
                }
            }
            public int Port
            {
                get { return _port; }
                set 
                { 
                    if (_active)
                        _port = value; 
                }
            }
        }

        public delegate void SimpleEvent(string clGUID);
        public delegate void MessageEvent(string clGUID, string msg);
        public delegate void DataRecieve(string clGUID, byte[] data);

        public class ServerBase : SocketBase
        {
            public class Connection
            {
                private bool _active;
                private string _GUID;
                private TcpClient Client;
                private BinaryReader Reader;
                private BinaryWriter Writer;
                private Thread Watch;

                public string GUID
                {
                    get { return _GUID; }
                }
                public bool Active
                {
                    get { return _active; }
                }

                public event SimpleEvent onDisconnect;
                public event DataRecieve onRecieve;
                public event MessageEvent onError;

                private void _error(string msg)
                {
                    if (onError != null)
                        onError(_GUID, msg);
                }
                private void read()
                {
                    int length;
                    byte[] buffer;
                    while (_active)
                    {
                        try
                        {
                            length = Reader.ReadInt32();    // Читает длину передаваемых данных
                            if ((buffer = Reader.ReadBytes(length)) != null)
                                onRecieve(_GUID, buffer); // Пришли двоичные данные
                        }
                        catch (EndOfStreamException)
                        {
                            CloseConnection();
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
                }

                public void Send(byte[] data)
                {
                    try
                    {
                        Writer.Write(data.Length);
                        Writer.Write(data);
                        Writer.Flush();
                    }
                    catch (Exception Ex)
                    {
                        _error(Ex.Message);
                        CloseConnection();
                    }
                }

                public bool EstablishConnection(string srvGuid)
                {
                    try
                    {
                        Reader = new BinaryReader(Client.GetStream());
                        Writer = new BinaryWriter(Client.GetStream());
                        // Проверяем билд (версию)
                        if (build == BitConverter.ToUInt32(Reader.ReadBytes(sizeof(UInt32)), 0))
                            _GUID = Encoding.ASCII.GetString(Reader.ReadBytes(32)); // Версия подходит - считываем GUID
                        else
                            Writer.Write(false);
                    }
                    catch (Exception Ex)
                    {
                        _error(Ex.Message);
                        CloseConnection();
                    }
                    if (_GUID != Guid.Empty.ToString())
                    {
                        Watch = new Thread(read);
                        Watch.IsBackground = true;
                        _active = true;
                        Writer.Write(true);
                        Writer.Write(Encoding.ASCII.GetBytes(srvGuid));
                        Writer.Flush();
                        Watch.Start();
                    }
                    else
                        _error("Версия не подходит или клиент закрылся");
                    return _active;
                }
                public void CloseConnection()
                {
                    if (_active)
                    {
                        _active = false;
                        try
                        {
                            Writer.Close();
                            Reader.Close();
                            Client.Close();
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
                }
                public Connection(TcpClient tClient)
                {
                    Client = tClient;
                    _GUID = Guid.Empty.ToString();
                    _active = false;
                }
            }

            private TcpListener _tcpListener;
            private Thread _thrListener;
            private object conn = new object();
            private Hashtable _connections;

            public event SimpleEvent OnConnect;
            public event SimpleEvent OnDisconnect;
            public event SimpleEvent OnStartListening;
            public event SimpleEvent OnStopListening;
            public event DataRecieve OnRecieve;
            public event MessageEvent OnError;

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

            public string GUID
            {
                get { return _srvGuid; }
            }
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
            public bool isClient(string clGUID)
            {
                return _connections.ContainsKey(clGUID);
            }
            public int Count
            {
                get { return _connections.Count; }
            }

            public void Send(string cGUID, byte[] data)
            {
                if (_connections.ContainsKey(cGUID))
                {
                    Connection t = (Connection)_connections[cGUID];
                    t.Send(data);
                    t = null;
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

            private void listen()
            {
                if (OnStartListening != null)
                    OnStartListening("Server");
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
                    OnStopListening("Server");
            }
            private void _connect(TcpClient Client)
            {
                Connection t = new Connection(Client);
                t.EstablishConnection(_srvGuid);
                if (t.Active)
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
                else
                    _error("Server", "Неудачная попытка подключения клиента");
            }

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
            private void _parse(string clGUID, byte[] data)
            {
                if (OnRecieve != null)
                    OnRecieve(clGUID, data);
            }
            private void _error(string GUID, string msg)
            {
                if (OnError != null)
                    OnError(GUID, msg);
            }
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
                        _thrListener = new Thread(listen);
                        _thrListener.IsBackground = true;
                        _thrListener.Start();
                        _active = true;
                    }
                    catch (Exception Ex)
                    {
                        _error("SERVER", Ex.Message);
                    }
                }
                else
                    _error("SERVER", "Server already running");
            }
            public void Stop()
            {
                if (_active)
                {
                    _active = false;
                    _tcpListener.Stop();
                    CloseAll();
                }
                else
                    _error("SERVER", "Сервер не запущен");
            }
            public ServerBase()
            {
                _srvGuid = Guid.NewGuid().ToString().Replace("-", "");
                _connections = new Hashtable();
                _adress = "0.0.0.0";
            }
            ~ServerBase()
            {
                CloseAll();
                if (_tcpListener != null)
                    _tcpListener.Stop();
            }
        }
        public class ClientBase : SocketBase
        {
            private string _guid;
            private TcpClient tcpClient;
            private BinaryWriter Writer;
            private BinaryReader Reader;
            private Thread Watch;

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
            public string GUID
            {
                get { return _guid; }
                set { _guid = value; }
            }
            public string ServerGUID
            {
                get { return _srvGuid; }
            }

            public void Send(byte[] data)
            {
                try
                {
                    Writer.Write(data.Length);
                    Writer.Write(data);
                    Writer.Flush();
                }
                catch (Exception Ex)
                {
                    _error(Ex.Message);
                    Disconnect();
                }
            }

            private void read()
            {
                int length;
                byte[] buffer;
                while (_active)
                {
                    try
                    {
                        length = Reader.ReadInt32();    // Читает длину передаваемых данных
                        if ((buffer = Reader.ReadBytes(length)) != null)
                        {
                            if (OnRecieve != null)
                                OnRecieve(_guid, buffer); // Пришли двоичные данные
                        }
                    }
                    catch (IOException)
                    {
                        Disconnect();
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
            }

            public event SimpleEvent OnConnect;
            public event SimpleEvent OnDisconnect;
            public event DataRecieve OnRecieve;
            public event MessageEvent OnError;
    
            private void _error(string Message)
            {
                if (OnError != null)
                    OnError(_guid, Message);
            }
    
            public void Connect(int nPort, string nAdress)
            {
                _port = nPort;
                _adress = nAdress;
                Connect();
            }
            private void Connect()
            {
                if (!_active)
                {
                    bool connected = false;
                    try
                    {
                        tcpClient = new TcpClient(_adress, _port);
                        Writer = new BinaryWriter(tcpClient.GetStream());
                        Reader = new BinaryReader(tcpClient.GetStream());
                        Writer.Write(BitConverter.GetBytes(build));
                        Writer.Write(Encoding.ASCII.GetBytes(GUID));
                        Writer.Flush();
                        connected = BitConverter.ToBoolean(Reader.ReadBytes(sizeof(bool)), 0);
                        if (connected)
                            _srvGuid = Encoding.ASCII.GetString(Reader.ReadBytes(32));
                    }
                    catch (Exception Ex)
                    {
                        _error(Ex.Message);
                        Disconnect();
                    }
                    if (connected)
                    {
                        Watch = new Thread(read);
                        Watch.IsBackground = true;
                        _active = true;
                        Watch.Start();
                        if (OnConnect != null)
                            OnConnect(_srvGuid);
                    }
                    else
                        Disconnect();
                }
                else
                    _error("Клиент уже подключен");
            }
            public void Disconnect()
            {
                if (_active)
                {
                    _active = false;
                    if (tcpClient != null)
                    {
                        Writer.Close();
                        Reader.Close();
                        tcpClient.Close();
                    }
                    if (OnDisconnect != null)
                        OnDisconnect(_guid);
                }
            }

            public ClientBase()
            {
                _guid = Guid.NewGuid().ToString().Replace("-", "");
            }
            ~ClientBase()
            {
                Disconnect();
            }
        }
    }
}
