using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaCTuCaTu4ECKuu
{
    namespace TcpBinarySocket
    {
        using System.Collections;
        using System.Net;
        using System.Net.Sockets;
        using System.Threading;

        #region Delegates
        public delegate void LocationInfoDelegate(IPAddress adress, int port);
        public delegate void ClientConnectionDelegate(string clGuid);
        public delegate void ServerExceptionDelegate(string clGuid, Exception innerException);
        public delegate void ServerDataRecieve(string clGuid, byte[] data);
        #endregion

        public class ServerBase : SocketBase
        {
            private Thread _thrListener;                            // Поток в котором сервер прослушивает новые подключения
            private TcpListener _tcpListener;                       // Слушатель новых подключений
            /// <summary>
            /// Таблица всех подключеных к серверу клиентов
            /// </summary>
            internal Hashtable _connections;                 

            private object _startLock = new object();               // Защита старта и остановки сервера
            private object conn = new object();                     // Защита изменения таблицы подключений

            /// <summary>
            /// Указывает нужно ли пересылать данные всем клиентам при получении (выключено по-умолчанию)
            /// </summary>
            public bool Broadcast { get; set; }

            #region Events

            /// <summary>
            /// Сервер начал слушать подключения по заданному адресу и порту
            /// </summary>
            public event LocationInfoDelegate onStartListening;
            /// <summary>
            /// Сервер прекратил слушать подключение на указанном адресе и порте
            /// </summary>
            public event LocationInfoDelegate onStopListening;
            /// <summary>
            /// Клиент с указанным идентификатором подключен
            /// </summary>
            public event ClientConnectionDelegate onConnect;
            /// <summary>
            /// Клиент с указанным идентификатором отключен от сервера
            /// </summary>
            public event ClientConnectionDelegate onDisconnect;
            /// <summary>
            /// От клиента пришли данные
            /// </summary>
            public event ServerDataRecieve onRecieve;
            /// <summary>
            /// Возникшая некритическая ошибка
            /// </summary>
            public event ServerExceptionDelegate onInnerException;
            #endregion

            #region Статистика
            /// <summary>
            /// Передано байт
            /// </summary>
            public long DataTransmitted { get; internal set; }
            /// <summary>
            /// Передано служебных байт
            /// </summary>
            public int ServiceDataTransmitted { get; internal set; }
            /// <summary>
            /// Получено байт
            /// </summary>
            public long DataRecieved { get; internal set; }
            /// <summary>
            /// Получено служебных байт
            /// </summary>
            public long ServiceDataRecieved { get; internal set; }
            #endregion

            public ServerBase()
            {
                GUID = Guid.NewGuid().ToString().Replace("-", "");
                Broadcast = false;
                _connections = new Hashtable();
                _adress = IPAddress.Any;
            }
            /// <summary>
            /// Начать слушать новые подключения с указанными параметрами
            /// </summary>
            public void Start()
            {
                lock (_startLock)
                {
                    if (_active)
                        throw new InvalidOperationException("Сервер уже запущен");
                    _active = true;
                    try
                    {
                        _thrListener = new Thread(_listen);
                        _thrListener.Name = "SocketServerListener";
                        _thrListener.IsBackground = true;
                        _thrListener.Start();
                    }
                    catch (Exception Ex)
                    {
                        _active = false;
                        throw Ex;
                    }
                }
            }
            /// <summary>
            /// Прекратить слушать новые подключения с указанными параметрами и разорвать все подключения
            /// </summary>
            public void Stop()
            {
                lock (_startLock)
                {
                    if (_active)
                    {
                        _active = false;
                        CloseAll();
                    }
                    else
                        _error(GUID, new InvalidOperationException("Сервер уже остановлен"));
                }
            }
            private void _listen()
            {
                _tcpListener = new TcpListener(_adress, _port);
                _tcpListener.Start();
                if (onStartListening != null)
                    onStartListening(_adress, _port);
                while (_active)
                {
                    try
                    {
                        _connect(_tcpListener.AcceptTcpClient());
                    }
                    catch (SocketException SocketEx)
                    {
                        if (_active || SocketEx.SocketErrorCode != SocketError.Interrupted)
                            _error(GUID, SocketEx);
                    }
                    catch (Exception Ex)
                    {
                        _error(GUID, Ex);
                    }
                }
                _tcpListener.Stop();
                if (onStopListening != null)
                    onStopListening(_adress, _port);
            }

            private void _connect(TcpClient client)
            {
                lock (conn)
                {
                    Connection t = new Connection(this, client);
                    if (t.EstablishConnection())
                    {
                        t.onDisconnect += _disconnect;
                        if (t.Connected)
                            _connections.Add(t.GUID, t);
                        else
                            t.onDisconnect -= _disconnect;
                        if (onConnect != null)
                            onConnect(t.GUID);
                    }
                    else
                        _error(GUID, new Exception("Неудачная попытка подключения клиента"));
                }
            }
            private void _disconnect(string clGUID)
            {
                lock (conn)
                {
                    if (_connections.ContainsKey(clGUID))
                    {
                        Connection t = (Connection)_connections[clGUID];
                        t.onDisconnect -= _disconnect;
                        t.CloseConnection();
                        _connections.Remove(clGUID);
                        if (onDisconnect != null)
                            onDisconnect(clGUID);
                    }
                    else
                        _error(GUID, new ArgumentException("Идентификатор должен присутствовать в списке клиентов"));
                }
            }
            internal void _recieve(string clGUID, byte[] data)
            {
                if (onRecieve != null)
                    onRecieve(clGUID, data);
                if (Broadcast)
                {
                    lock(conn)
                    {
                        foreach (DictionaryEntry c in _connections)
                        {
                            if ((string)c.Key != clGUID)
                            {
                                Send((string)c.Key, data);
                            }
                        }
                    }
                }
            }
            internal void _error(string clGUID, Exception ex)
            {
                if (onInnerException != null)
                    onInnerException(clGUID, ex);
            }

            /// <summary>
            /// Список идентификаторов всех подключенных клиентов
            /// </summary>
            public List<string> Clients
            {
                get
                {
                    List<string> res = new List<string>();
                    if (_connections.Count > 0)
                    {
                        lock(conn)
                        {
                            foreach (DictionaryEntry c in _connections)
                                res.Add((string)c.Key);
                        }
                    }
                    return res;
                }
            }
            /// <summary>
            /// Проверить состояние подключения клиента с указанным идентификатором в данный момент
            /// </summary>
            /// <param name="clGuid">Идентификатор проверяемого клиента</param>
            /// <returns>Состояние подключения</returns>
            public bool Connected(string clGuid)
            {
                return _connections.ContainsKey(clGuid);
            }
            /// <summary>
            /// Количество работающих в данный момент подключений
            /// </summary>
            public int Count
            {
                get { return _connections.Count; }
            }

            /// <summary>
            /// Отправить данные подключеному клиенту
            /// </summary>
            /// <param name="clGUID">Идентификатор клиента</param>
            /// <param name="data">Данные (Максимальный размер - 65533 байт)</param>
            public void Send(string clGUID, byte[] data)
            {
                if (Connected(clGUID))
                {
                    Connection t = (Connection)_connections[clGUID];
                    t.Send(data);
                }
                else
                    throw new ArgumentException("Клиент не подключен");
            }
            /// <summary>
            /// Разорвать подключение с указанным клиентом
            /// </summary>
            /// <param name="clGUID"></param>
            public void Close(string clGUID)
            {
                _disconnect(clGUID);
            }
            /// <summary>
            /// Разорвать все существующие подключения
            /// </summary>
            public void CloseAll()
            {
                foreach (string cli in Clients)
                    _disconnect(cli);
            }

            ~ServerBase()
            {
                Stop();
            }
        }
    }
}

