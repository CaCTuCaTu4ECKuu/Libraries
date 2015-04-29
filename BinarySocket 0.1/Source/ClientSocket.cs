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
        using System.IO;
        using System.Threading;

        public delegate void DataRecieveDelegate(byte[] data);
        public class ClientBase : SocketBase
        {
            private TcpClient tcpClient;
            private BinaryWriter _writer;
            private BinaryReader _reader;
            private Thread _watcher;

            private object sendLocker = new object();
            private object qlLocker = new object();

            #region Статистика
            public long DataTransmitted { get; internal set; }
            public int ServiceDataTransmitted { get; internal set; }
            public long DataRecieved { get; internal set; }
            public long ServiceDataRecieved { get; internal set; }

            /// <summary>
            /// Байт в очереди на отправление (не отправленны полностю)
            /// </summary>
            public long QueueLength { get; private set; }
            #endregion

            /// <summary>
            /// Событие указывает к какому серверу был подключен клиент
            /// </summary>
            public event LocationInfoDelegate onConnect;
            /// <summary>
            /// Событие возникает в случае потери соединения с сервером
            /// </summary>
            public event LocationInfoDelegate onDisconnect;
            /// <summary>
            /// Событие возникает при получении данных от сервера
            /// </summary>
            public event DataRecieveDelegate onRecieve;

            public ClientBase()
            {
                Adress = "127.0.0.1";
                GUID = Guid.NewGuid().ToString().Replace("-", "");
            }
            /// <summary>
            /// Подключиться к серверу который указан в текущих настройках
            /// </summary>
            public void Connect()
            {
                if (_active)
                    throw new InvalidOperationException("Клиент уже подключен");
                _active = true;
                // Чтоб не зависал процесс из под которого мы вызываем подключение в случае, если процесс затянеться (неправильный адрес сервера, например)
                Thread connectThread = new Thread(_connect);
                connectThread.IsBackground = true;
                connectThread.Start();
            }
            /// <summary>
            /// Подключиться к указанному серверу
            /// </summary>
            /// <param name="port">Порт сервера</param>
            /// <param name="adress">Адрес сервера</param>
            public void Connect(int port, string adress)
            {
                Port = port;
                Adress = adress;
                Connect();
            }

            private void _connect()
            {
                try
                {
                    tcpClient = new TcpClient();
                    tcpClient.Connect(new IPEndPoint(_adress, _port));
                    _writer = new BinaryWriter(tcpClient.GetStream());
                    _reader = new BinaryReader(tcpClient.GetStream());
                    _writer.Write(GUID);
                    ServiceDataTransmitted += Encoding.ASCII.GetByteCount(GUID);
                    while (_reader.ReadBoolean() != true)
                    {
                        ServiceDataRecieved += 1;
                        GUID = Guid.NewGuid().ToString().Replace("-", "");
                        _writer.Write(GUID);
                        ServiceDataTransmitted += Encoding.ASCII.GetByteCount(GUID);
                    }
                    ServiceDataRecieved += 1;
                    _watcher = new Thread(read);
                    _watcher.IsBackground = true;
                    _watcher.Name = "SocketClient";
                    if (onConnect != null)
                        onConnect(_adress, _port);
                    _watcher.Start();
                }
                catch (Exception ex)
                {
                    Disconnect();
                    throw ex;
                }
            }

            /// <summary>
            /// Отправить данные на сервер
            /// </summary>
            /// <param name="data">Данные (Максимальный размер - 65533 байт)</param>
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
                    }
                    catch (Exception ex)
                    {
                        Disconnect();
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

            private void read()
            {
                UInt16 length;
                byte[] buffer;
                while (_active)
                {
                    try
                    {
                        length = _reader.ReadUInt16();                       // Читает длину передаваемых данных
                        ServiceDataRecieved += 2;
                        if ((buffer = _reader.ReadBytes(length)) != null)
                        {
                            DataRecieved += buffer.Length;
                            if (onRecieve != null)
                                onRecieve(buffer);                          // Пришли двоичные данные
                        }
                    }
                    catch (IOException)
                    {
                        Disconnect();
                    }
                    catch (Exception ex)
                    {
                        if (_active)
                        {
                            Disconnect();
                        }
                        else
                            throw ex;
                    }
                }
            }

            /// <summary>
            /// Разорвать соединение с сервером
            /// </summary>
            public void Disconnect()
            {
                if (_active)
                {
                    _active = false;
                    if (tcpClient != null)
                    {
                        if (_writer != null)
                            _writer.Close();
                        if (_reader != null)
                            _reader.Close();
                        tcpClient.Close();
                    }
                    if (onDisconnect != null)
                        onDisconnect(_adress, _port);
                }
            }

            ~ClientBase()
            {
                Disconnect();
            }
        }
    }
}
