using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaCTuCaTu4ECKuu
{
    namespace TcpBinarySocket
    {
        using System.Net;
        using System.Net.Sockets;

        public class SocketBase
        {
            public const UInt16 maxSendPacketSize = 65533;

            protected IPAddress _adress;
            protected bool _active = false;
            protected int _port = 15150;
            protected int _bytesBuffer;

            /// <summary>
            /// Состояние
            /// </summary>
            public bool Active
            {
                get { return _active; }
            }
            public string Adress
            {
                get { return _adress.ToString(); }
                set
                {
                    if (_active)
                        throwActiveException("Ip адрес");

                    IPAddress newaddr;
                    if (IPAddress.TryParse(value, out newaddr))
                        _adress = newaddr;
                    else
                        throwInvalidException("адрес");                
                }
            }
            public int Port
            {
                get { return _port; }
                set
                {
                    if (_active)
                        throwActiveException("порт");

                    if (value > 0 && value < 65536)
                        _port = value;
                    else
                        throwInvalidException("порт. Диапазон 0-65535");
                }
            }
            public string Location
            {
                get { return Adress + ':' + Port.ToString(); }
            }
            /// <summary>
            /// Уникальный идентификатор
            /// </summary>
            public string GUID { get; internal set; }

            private void throwActiveException(string part)
            {
                throw new InvalidOperationException("Нельзя установить новый " + part + " при открытом подключении");
            }
            private void throwInvalidException(string part)
            {
                throw new ArgumentException("Недопустимый " + part);
            }
        }
    }
}
