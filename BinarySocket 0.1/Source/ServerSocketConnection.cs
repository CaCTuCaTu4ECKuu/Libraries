using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaCTuCaTu4ECKuu
{
    namespace TcpBinarySocket
    {
        using System.Net.Sockets;
        using System.IO;
        using System.Threading;

        public class Connection
        {
            private ServerBase _server;
            private TcpClient _client;

            private BinaryReader _reader = null;
            private BinaryWriter _writer = null;
            private Thread _watcher;

            private static object statsLocker = new object();

            private object sendLocker = new object();
            private object qlLocker = new object();

            #region Статистика
            public long DataTransmitted { get; private set; }
            public int ServiceDataTransmitted { get; private set; }
            public long DataRecieved { get; private set; }
            public long ServiceDataRecieved { get; private set; }

            public long QueueLength { get; private set; }
            #endregion

            /// <summary>
            /// Идентификатор клиента
            /// </summary>
            public string GUID { get; private set; }
            /// <summary>
            /// Состояние подключения клиента
            /// </summary>
            public bool Connected
            {
                get { return _client.Connected; }
            }

            /// <summary>
            /// Событие возникает если возникает ошибка в работе и соединение сразу же разрываеться
            /// </summary>
            public event ClientConnectionDelegate onDisconnect;

            public Connection(ServerBase server, TcpClient client)
            {
                _server = server;
                _client = client;
                GUID = Guid.Empty.ToString();
            }
            /// <summary>
            /// Попытаться установить соединение с клиентом
            /// </summary>
            /// <returns>Состояние подключения клиента</returns>
            public bool EstablishConnection()
            {
                try
                {
                    _reader = new BinaryReader(_client.GetStream());
                    _writer = new BinaryWriter(_client.GetStream());
                    while (_server._connections.ContainsKey(GUID = _reader.ReadString()))
                    {
                        ServiceDataRecieved += Encoding.ASCII.GetByteCount(GUID);
                        lock (statsLocker)
                        {
                            _server.ServiceDataRecieved += Encoding.ASCII.GetByteCount(GUID);
                        }

                        _writer.Write(false);

                        ServiceDataTransmitted += 1;
                        lock (statsLocker)
                        {
                            _server.ServiceDataTransmitted += 1;
                        }
                    }
                    ServiceDataRecieved += Encoding.ASCII.GetByteCount(GUID);
                    lock (statsLocker)
                    {
                        _server.ServiceDataRecieved += Encoding.ASCII.GetByteCount(GUID);
                    }

                    _writer.Write(true);

                    ServiceDataTransmitted += 1;
                    lock (statsLocker)
                    {
                        _server.ServiceDataTransmitted += 1;
                    }

                    _watcher = new Thread(read);
                    _watcher.Name = "ClientListener " + GUID;
                    _watcher.IsBackground = true;
                    _watcher.Start();
                }
                catch (Exception ex)
                {
                    _server._error(GUID, ex);
                    CloseConnection();
                }
                return _client.Connected;
            }
            /// <summary>
            /// Закрывает это соединение
            /// </summary>
            public void CloseConnection()
            {
                try
                {
                    if (_writer != null)
                        _writer.Close();
                    if (_reader != null)
                        _reader.Close();
                    _client.Close();
                }
                catch (Exception ex)
                {
                    _server._error(GUID, ex);
                }
                finally
                {
                    if (onDisconnect != null)
                        onDisconnect(GUID);
                }
            }

            private void read()
            {
                UInt16 length;
                byte[] buffer;
                while (_client.Connected)
                {
                    try
                    {
                        length = _reader.ReadUInt16();                               // Длинна данных

                        ServiceDataRecieved += 2;
                        lock (statsLocker)
                        {
                            _server.ServiceDataRecieved += 2;
                        }
                        if ((buffer = _reader.ReadBytes(length)) != null)
                        {
                            DataRecieved += buffer.Length;
                            lock (statsLocker)
                            {
                                _server.DataRecieved += buffer.Length;
                            }

                            _server._recieve(GUID, buffer);                         // Пришли двоичные данные
                        }
                    }
                    catch (EndOfStreamException eosEx)
                    {
                        _server._error(GUID, eosEx);
                        CloseConnection();
                    }
                    catch (Exception ex)
                    {
                        _server._error(GUID, ex);
                        CloseConnection();
                    }
                }
            }

            /// <summary>
            /// Отправить клиенту данные
            /// </summary>
            /// <param name="data">Данные для отправки</param>
            public void Send(byte[] data)
            {
                if (data.Length <= ServerBase.maxSendPacketSize)
                {
                    Thread sendThread = new Thread(_send);
                    sendThread.IsBackground = true;
                    lock (qlLocker)
                    {
                        QueueLength += data.Length + 2;
                    }
                    sendThread.Start(data);
                }
                else
                    throw new ArgumentOutOfRangeException("data", "Длина данных не должна привышать 65533 байта");
            }
            
            private void _send(object data)
            {
                lock (sendLocker)
                {
                    byte[] d = (byte[])data;
                    try
                    {
                        _writer.Write((UInt16)d.Length);
                        _writer.Write(d);
                        _writer.Flush();

                        ServiceDataTransmitted += 2;
                        DataTransmitted += d.Length;
                        lock (statsLocker)
                        {
                            _server.ServiceDataTransmitted += 2;
                            _server.DataTransmitted += d.Length;
                        }
                    }
                    catch (Exception ex)
                    {
                        CloseConnection();
                        throw ex;
                    }
                    finally
                    {
                        lock (qlLocker)
                        {
                            QueueLength -= d.Length + 2;
                        }
                    }
                }
            }
        }
    }
}
