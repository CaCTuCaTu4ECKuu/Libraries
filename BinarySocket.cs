using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;

namespace BinarySocket
{
    private class SocketBase
    {
        protected bool _active = false;
        protected IPAddress _adress;
        protected int _port = 15150;

        public string Adress
        {
            get { return _adress.ToString(); }
            set 
            {
                if (!_active)
                {
                    IPAddress curr = _adress;
                    if (!IPAddress.TryParse(value, out _adress))
                        _adress = curr;
                }
            }
        }
        public int Port
        {
            get { return _port; }
            set 
            { 
                if (!_active && value > 0 && value < 65536)
                    _port = value; 
            }
        }
        public string Location
        {
            get { return _adress.ToString() + ':' + _port.ToString(); }
        }
    }

    public delegate void SimpleEvent(string info);
    public delegate void DataRecieve(byte[] data);

    public class ServerBase : SocketBase
    {
        public delegate void ServerMessageEvent(string clGUID, string msg);
        public delegate void ServerDataRecieve(string clGUID, byte[] data);

        public class Connection
        {
            private TcpClient Client;
            private BinaryReader Reader;
            private BinaryWriter Writer;
            private Thread Watch;
            private string _guid;

            public string GUID
            {
                get { return _guid; }
            }
            public bool Connected
            {
                get { return Client.Connected; }
            }

            public event SimpleEvent onDisconnect;
            public event ServerDataRecieve onRecieve;
            public event ServerMessageEvent onError;

            private void _error(string msg)
            {
                if (onError != null)
                    onError(_guid, msg);
            }
            private void read()
            {
                int length;
                byte[] buffer;
                while (Client.Connected)
                {
                    try
                    {
                        length = Reader.ReadInt32();    // Читает длину передаваемых данных
                        if ((buffer = Reader.ReadBytes(length)) != null)
                            onRecieve(_guid, buffer); // Пришли двоичные данные
                    }
                    catch (EndOfStreamException)
                    {
                        CloseConnection();
                    }
                    catch (Exception Ex)
                    {
                        _error(Ex.Message);
                        CloseConnection();
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

            public bool EstablishConnection()
            {
                try
                {
                    Reader = new BinaryReader(Client.GetStream());
                    Writer = new BinaryWriter(Client.GetStream());
                    Watch = new Thread(read);
                    Watch.IsBackground = true;
                    Watch.Start();
                }
                catch (Exception Ex)
                {
                    _error(Ex.Message);
                    CloseConnection();
                }
                return Client.Connected;
            }
            public void CloseConnection()
            {
                try
                {
                    if (Writer != null)
                        Writer.Close();
                    if (Reader != null)
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
                        onDisconnect(_guid);
                }
            }
            public Connection(TcpClient tClient)
            {
                Client = tClient;
                _guid = Guid.NewGuid().ToString().Replace("-", "");
                Reader = null;
                Writer = null;
            }
        }

        private TcpListener _tcpListener;
        private Thread _thrListener;
        private Hashtable _connections;
        private object conn = new object();
        private string _srvGuid;

        public event SimpleEvent OnStartListening;
        public event SimpleEvent OnStopListening;
        public event SimpleEvent OnConnect;
        public event SimpleEvent OnDisconnect;
        public event ServerMessageEvent OnError;
        public event ServerDataRecieve OnRecieve;

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
        public int Count
        {
            get { return _connections.Count; }
        }

        public void Send(string clGUID, byte[] data)
        {
            if (_connections.ContainsKey(clGUID))
            {
                Connection t = (Connection)_connections[clGUID];
                t.Send(data);
            }
            else
                throw new ArgumentException("Идентификатор должен присутствовать в списке клиентов");
        }
        public void Close(string clGUID)
        {
            _disconnect(clGUID);
        }
        public void CloseAll()
        {
            foreach (string cli in Clients)
                _disconnect(cli);
        }

        private void listen()
        {
            _tcpListener = new TcpListener(_adress, _port);
            _tcpListener.Start();
            if (OnStartListening != null)
                OnStartListening(Location);
            try
            {
                while (_active)
                {
                    _connect(_tcpListener.AcceptTcpClient());
                }
            }
            catch (SocketException SocketEx)
            {
                if (_active || SocketEx.SocketErrorCode != SocketError.Interrupted)
                    _error(_srvGuid, SocketEx.Message);
            }
            catch (Exception Ex)
            {
                _error(_srvGuid, Ex.Message);
            }
            finally
            {
                _tcpListener.Stop();
                if (OnStopListening != null)
                    OnStopListening(Location);
            }
        }

        private void _connect(TcpClient Client)
        {
            Connection t = new Connection(Client);
            if (t.EstablishConnection())
            {
                t.onDisconnect += _disconnect;
                if (t.Connected)
                {
                    Monitor.Enter(conn);
                    _connections.Add(t.GUID, t);
                    Monitor.Exit(conn);
                    t.onError += _error;
                    t.onRecieve += _parse;
                }
                else
                    t.onDisconnect -= _disconnect;
                if (OnConnect != null)
                    OnConnect(t.GUID);
            }
            else
                _error(_srvGuid, "Неудачная попытка подключения клиента");
        }
        private void _disconnect(string clGUID)
        {
            Monitor.Enter(conn);
            if (_connections.ContainsKey(clGUID))
            {
                Connection t = (Connection)_connections[clGUID];
                t.onDisconnect -= _disconnect;
                t.onError -= _error;
                t.onRecieve -= _parse;
                t.CloseConnection();
                _connections.Remove(clGUID);
                if (OnDisconnect != null)
                    OnDisconnect(clGUID);
            }
            else
                throw new ArgumentException("Идентификатор должен присутствовать в списке клиентов");
            Monitor.Exit(conn);
        }
        private void _parse(string clGUID, byte[] data)
        {
            if (OnRecieve != null)
                OnRecieve(clGUID, data);
        }
        private void _error(string clGUID, string msg)
        {
            if (OnError != null)
                OnError(clGUID, msg);
        }

        public void Start()
        {
            if (!_active)
            {
                try
                {
                    _thrListener = new Thread(listen);
                    _thrListener.IsBackground = true;
                    _active = true;
                    _thrListener.Start();
                }
                catch (Exception Ex)
                {
                    _error(_srvGuid, Ex.Message);
                }
            }
            else
                _error(_srvGuid, "Server already running");
        }
        public void Stop()
        {
            if (_active)
            {
                _active = false;
                CloseAll();
            }
            else
                _error(_srvGuid, "Сервер не запущен");
        }
        public ServerBase()
        {
            _connections = new Hashtable();
            _adress = IPAddress.Any;
        }
        ~ServerBase()
        {
            Stop();
        }
    }
    public class ClientBase : SocketBase
    {
        private TcpClient tcpClient;
        private BinaryWriter Writer;
        private BinaryReader Reader;
        private Thread Watch;

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
                            OnRecieve(buffer); // Пришли двоичные данные
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
        public event SimpleEvent OnError;
        public event DataRecieve OnRecieve;
    
        private void _error(string Message)
        {
            if (OnError != null)
                OnError(Message);
        }
    
        public void Connect(int nPort, string nAdress)
        {
            Port = nPort;
            Adress = nAdress;
            if (_port == nPort && _adress.ToString() == nAdress)
                Connect();
        }
        private void Connect()
        {
            if (!_active)
            {
                try
                {
                    tcpClient = new TcpClient(new IPEndPoint(_adress, _port));
                    Writer = new BinaryWriter(tcpClient.GetStream());
                    Reader = new BinaryReader(tcpClient.GetStream());
                    Watch = new Thread(read);
                    Watch.IsBackground = true;
                    _active = true;
                    if (OnConnect != null)
                        OnConnect(Location);
                    Watch.Start();
                }
                catch (Exception Ex)
                {
                    _error(Ex.Message);
                    Disconnect();
                }
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
                    if (Writer != null)
                        Writer.Close();
                    if (Reader != null)
                        Reader.Close();
                    tcpClient.Close();
                }
                if (OnDisconnect != null)
                    OnDisconnect(Location);
            }
        }

        public ClientBase()
        {
            Adress = "127.0.0.1";
        }
        ~ClientBase()
        {
            Disconnect();
        }
    }
}
