using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Configuration;
using System.Runtime.InteropServices;



namespace ManagerNetworks
{
    


    /// <summary>
    /// класс представляющий интерфейс для управления сетевыми настройками
    /// через вызов Windows API
    /// </summary>
    public class NetworksManager
    {

        #region api constants
        public const int MAX_ADAPTER_DESCRIPTION_LENGTH = 128; // arb.
        public const int MAX_ADAPTER_NAME_LENGTH = 256;
        public const int MAX_ADAPTER_ADDRESS_LENGTH = 8;
        public const int MAX_ADAPTER_NAME=128;
        public const int ANY_SIZE=100;
        #endregion

        #region api functions
        [DllImport("Iphlpapi.dll")]
        private static extern int GetAdaptersInfo(
            [Out] IntPtr pAdapterInfo,
            [In, Out] IntPtr pOutBuffLen);

        [DllImport("Iphlpapi.dll")]
        private static extern int AddIPAddress(
            [In] IP_Addr Address,
            [In] IP_Mask IpMask,
            [In] int IfIndex,
            [Out]  IntPtr NTEContext,
            [Out]  IntPtr NTEInstance);

        [DllImport("Iphlpapi.dll")]
        private static extern int GetIpAddrTable(
            [Out] IntPtr pIpAddrTable,
            [In,Out] IntPtr pdwSize,
            [In] bool bOrder);

        [DllImport("Iphlpapi.dll")]
        private static extern int DeleteIPAddress(
            [In] int NteContext);

        /// <summary>
        /// api освобождает или делает недействительным полученный ранее ip 
        /// адрес через службу dhcp
        /// </summary>
        /// <param name="AdapterInfo">Указатель на структуру Ip_Adapter_Index_Map</param>
        /// <returns></returns>
        [DllImport("Iphlpapi.dll")]
        private static extern int IpReleaseAddress(
            [In] IntPtr AdapterInfo);

        /// <summary>
        /// Обновление или получение нового адреса с помощью службы dhcp
        /// </summary>
        /// <param name="AdapterInfo">Указатель на структуру Ip_Adapter_Index_Map</param>
        /// <returns></returns>
        [DllImport("Iphlpapi.dll")]
        private static extern int IpRenewAddress(
            [In] IntPtr AdapterInfo);

        [DllImport("Iphlpapi.dll")]
        private static extern int GetInterfaceInfo(
            [Out] IntPtr pIfTable,
            [In, Out] IntPtr pdwOutBuffer);

        [DllImport("Iphlpapi.dll")]
        private static extern int GetNumberOfInterfaces(
            [Out] IntPtr NumIntf);
        #endregion

        #region api structures
        /// <summary>
        /// структура описывающая информацию о сетевом адаптере
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private class IP_Adapter_Info
        {
            /// <summary>
            /// ссылка на следующую структуру IP_Adapter_Info
            /// </summary>
            public IntPtr NextAdapter;

            [MarshalAs(UnmanagedType.I4)]
            public int ComboIndex;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NetworksManager.MAX_ADAPTER_NAME_LENGTH + 4)]
            public string AdapterName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NetworksManager.MAX_ADAPTER_DESCRIPTION_LENGTH + 4)]
            public string Description;

            [MarshalAs(UnmanagedType.U4)]
            public uint AdressLeight;

            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = NetworksManager.MAX_ADAPTER_ADDRESS_LENGTH)]
            public byte[] Adress;

            [MarshalAs(UnmanagedType.I4)]
            public int Index;

            [MarshalAs(UnmanagedType.U4)]
            public uint Type;

            [MarshalAs(UnmanagedType.U4)]
            public uint DhcpEnabled;
            /// <summary>
            /// ссылка на структуру Ip_Address_String
            /// </summary>

            public IntPtr CurrentIpAddress;

            public Ip_Addr_String IpAddressList;

            public Ip_Addr_String GatewayList;

            public Ip_Addr_String DhcpServer;

            [MarshalAs(UnmanagedType.Bool)]
            public bool HaveWins;

            public Ip_Addr_String PrimaryWinsServer;

            public Ip_Addr_String SecondaryWinsServer;

            [MarshalAs(UnmanagedType.I4)]
            public int LeaseObtained;

            [MarshalAs(UnmanagedType.I4)]
            public int LeaseExpired;

        }

        /// <summary>
        /// структура, описывающая полную информацию о адресе
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private class Ip_Addr_String
        {

            public IntPtr NextAddStr;
            public Ip_Address_String IpAddress;
            public Ip_Address_String IpMask;
            public int Context;
        }

        /// <summary>
        /// структура, описывающая отдельны ip адрес в виде строки
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private class Ip_Address_String
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string String;
            
        }

        /// <summary>
        /// структура представляющая ip адрес 
         /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct IP_Addr
        {
            public IP_Addr(byte part1, byte part2, byte part3, byte part4)
            {
                b1 = part1;
                b2 = part2;
                b3 = part3;
                b4 = part4;
            }
            public byte b1;
            public byte b2;
            public byte b3;
            public byte b4;

        }

        /// <summary>
        /// структура представляющая маску
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct IP_Mask
        {
            public IP_Mask(byte part1, byte part2, byte part3, byte part4)
            {
                b1 = part1;
                b2 = part2;
                b3 = part3;
                b4 = part4;
            }
            public byte b1;
            public byte b2;
            public byte b3;
            public byte b4;

        }
        /// <summary>
        /// структура, описывающая таблицу адресов
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class Mib_IpAddrTable
        {
            public int dwNumEntries;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = NetworksManager.ANY_SIZE)]
            public Mib_IpAddrRow[] rows;
        }

        /// <summary>
        /// структура, описывающая отдельную строку в таблице адресов
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Mib_IpAddrRow
        {
            public int dwAddr;
            public int dwIndex;
            public int dwMask;
            public int dwBCastAddr;
            public int dwReasmSize;
            public ushort unused1;
            public ushort wType;
        }


        /// <summary>
        /// структура содержащая индекс и уникальное имя сетевого адаптеара
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct Ip_Adapter_Index_Map
        {
            public int Index;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NetworksManager.MAX_ADAPTER_NAME)]
            public string AdapterName;
        }

        /// <summary>
        /// структура описывающая информацию об сетевых интерфейсах в системе
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class Ip_Interface_Info
        {
            public int NumAdapters;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = NetworksManager.ANY_SIZE)]
            public Ip_Adapter_Index_Map[] Adapter;
            public void SetMapSize()
            {
                Adapter = new Ip_Adapter_Index_Map[NumAdapters];
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public struct IpAddressString
        {
           public int Context;
           public string Address;
            public string Mask;
        }

        /// <summary>
        /// 
        /// </summary>
        public struct AdapterInfo
        {
            public string AdapterName;
            public string Description;
            public int AddressLeight;
            public byte[]Address;
            public int IndexAdapter;
            public uint Type;
            public bool DhcpEnabled;
            public List<IpAddressString> ListIpAddress;
            public List<IpAddressString> GatewayList;
            public List<IpAddressString> DchpServer;
            public bool HaveWins;
            public List<IpAddressString> PrimaryWinsServer;
            public List<IpAddressString> SecondaryWinsServer;
            public DateTime LeaseObtained;
            public DateTime LeaseExpires;

        }


        /// <summary>
        /// получение информации о сетевых адаптерах
        /// </summary>
        public List<AdapterInfo> GetNetworkAdaptersInfo
        {
            get
            {
                List<AdapterInfo> listAd = new List<AdapterInfo>();
                //определяем размер стуруктуры для одного адаптера
                int sizeAdaptInfo = Marshal.SizeOf(typeof(IP_Adapter_Info));

                IntPtr sizeBuff = Marshal.AllocHGlobal(4);
                Marshal.WriteInt32(sizeBuff,sizeAdaptInfo);
                //определим количество адаптеров в системе и размер памяти,
                //которую надо выделить под информацию о них
                //Для этого производим дежурный вызов функции GetAdaptersInfo
                //чтобы получить размер выходного буфера
                IntPtr pAdapterInf = Marshal.AllocHGlobal(1);
                GetAdaptersInfo(pAdapterInf, sizeBuff);
                int outSize = Marshal.ReadInt32(sizeBuff);


                //выделяем требуюмую память и получаем ссылку на информацию
                Marshal.FreeHGlobal(pAdapterInf);
                pAdapterInf = Marshal.AllocHGlobal(outSize);
                int ret = GetAdaptersInfo(pAdapterInf, sizeBuff);

                //получаем число сетевых адаптеров в системе
                int countAdapters = outSize / sizeAdaptInfo;
                string[] names = new string[countAdapters];
                IP_Adapter_Info[] adaptersInfo = new IP_Adapter_Info[countAdapters];

                IntPtr currentInfo = pAdapterInf;
                for (int i = 0; i < countAdapters; i++)
                {
                    adaptersInfo[i] = new IP_Adapter_Info();
                    Marshal.PtrToStructure(currentInfo, adaptersInfo[i]);
                    //names[i] = adaptersInfo[i].AdapterName;
                    currentInfo = adaptersInfo[i].NextAdapter;
                    listAd.Add(this.ParseApiAdapterInfo(adaptersInfo[i]));
                }


                //освобождаем ресурсы
                Marshal.FreeHGlobal(sizeBuff);
                Marshal.FreeHGlobal(pAdapterInf);
                return listAd;

            }
        }

        /// <summary>
        /// разбор структуры для функции api в структуру для net
        /// </summary>
        /// <param name="adapterInfo"></param>
        /// <returns></returns>
        private AdapterInfo ParseApiAdapterInfo(IP_Adapter_Info adapterInfo)
        {
            AdapterInfo adinf = new AdapterInfo();
            adinf.AdapterName = adapterInfo.AdapterName;
            adinf.Address = adapterInfo.Adress;
            adinf.Description = adapterInfo.Description;
            adinf.HaveWins = adapterInfo.HaveWins;
            adinf.IndexAdapter = adapterInfo.Index;
            adinf.Type = adapterInfo.Type;
            adinf.ListIpAddress = this.ParseAddrString(adapterInfo.IpAddressList);
            adinf.PrimaryWinsServer = this.ParseAddrString(adapterInfo.PrimaryWinsServer);
            adinf.SecondaryWinsServer = this.ParseAddrString(adapterInfo.SecondaryWinsServer);
            adinf.DchpServer = this.ParseAddrString(adapterInfo.DhcpServer);
            adinf.DhcpEnabled = (adapterInfo.DhcpEnabled == 0) ? false : true;
            adinf.GatewayList = this.ParseAddrString(adapterInfo.GatewayList);
            adinf.LeaseExpires = DateTime.FromBinary(adapterInfo.LeaseExpired);//todo: разобраться со временем аренды
            adinf.LeaseObtained = DateTime.FromBinary(adapterInfo.LeaseObtained);
            
            return adinf;
        }

        /// <summary>
        /// разбор структуры списка
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<IpAddressString> ParseAddrString(Ip_Addr_String list)
        {
            List<IpAddressString> lst = new List<IpAddressString>();
            IpAddressString addStr = new IpAddressString();
            addStr.Context = list.Context;
            addStr.Address = list.IpAddress.String;
            addStr.Mask = list.IpMask.String;
            lst.Add(addStr);
            IntPtr ptr = list.NextAddStr;
            while (ptr != IntPtr.Zero)
            {
                Ip_Addr_String addr = new Ip_Addr_String();
                Marshal.PtrToStructure(ptr, addr);
                IpAddressString newAdr = new IpAddressString();
                newAdr.Context = addr.Context;
                newAdr.Address = addr.IpAddress.String;
                newAdr.Mask = addr.IpMask.String;
                lst.Add(newAdr);
                ptr = addr.NextAddStr;
            }
            return lst;
        }


        /// <summary>
        /// добавление ip адреса для сетевого адаптера в его список адресов по его индексу
        /// </summary>
        /// <param name="IndexAdapter">индекс адаптера </param>
        /// <param name="IpAddress">добавляемый ip адрес в виде структуры</param>
        /// <param name="IpMask">добавляемая маска</param>
        /// <returns>контекст адреса, используется для его удаления</returns>
        public int AddIpAddress(int IndexAdapter,IP_Addr IpAddress,IP_Mask IpMask)
        {
            IntPtr NteCont = Marshal.AllocHGlobal(4);
            IntPtr NteInst = Marshal.AllocHGlobal(4);
            int ret = AddIPAddress(IpAddress, IpMask, IndexAdapter, NteCont, NteInst);
            int NteContext = Marshal.ReadInt32(NteCont);
            Marshal.FreeHGlobal(NteInst);
            Marshal.FreeHGlobal(NteCont);
            return NteContext;
        }

        /// <summary>
        /// удаление ip адреса из списка сетевого адаптера по контексту ip aдреса
        /// </summary>
        /// <param name="NteContext"></param>
        public void RemoveIpAddress(int NteContext)
        {
            DeleteIPAddress(NteContext);
        }

        /// <summary>
        /// получение таблицы ip адресов в системе
        /// </summary>
        public Mib_IpAddrTable GetAddressTables()
        {
            // дежурный вызов функции получения таблицы ip aдресов
            // для получения размера выходного буфера для выделения памяти
            IntPtr pIpAddrTable = Marshal.AllocHGlobal(1);
            IntPtr pdwSize = Marshal.AllocHGlobal(4);
            int tableSize = Marshal.SizeOf(typeof(Mib_IpAddrTable));
            Marshal.WriteInt32(pdwSize,tableSize);
            int ret = GetIpAddrTable(pIpAddrTable, pdwSize, true);
            int outBuff = Marshal.ReadInt32(pdwSize);//здесь получаем реальный размер буфера


            Marshal.FreeHGlobal(pIpAddrTable);

            //основной вызов функции с правильными параметрами
            pIpAddrTable = Marshal.AllocHGlobal(outBuff);
            ret = GetIpAddrTable(pIpAddrTable, pdwSize, true);
            Mib_IpAddrTable table = new Mib_IpAddrTable();//объект контейнер для таблицы адресов
            Marshal.PtrToStructure(pIpAddrTable, table);

            //освобождение памяти
            Marshal.FreeHGlobal(pIpAddrTable);
            Marshal.FreeHGlobal(pdwSize);

            //сопоставляем размер массива строк относительно реального количества строк
            Mib_IpAddrRow[] rows = new Mib_IpAddrRow[table.dwNumEntries];
            for (int i = 0; i < table.dwNumEntries; i++)
            {
                rows[i] = table.rows[i];
            }
            table.rows = rows;

            return table;

        }

        
        /// <summary>
        /// получение информации по сетевым интерфейсам
        /// </summary>
        /// <returns></returns>
        public Ip_Interface_Info GetInterfaceInfo()
        {
            IntPtr pifTable = Marshal.AllocHGlobal(1);
            IntPtr pOutBuff = Marshal.AllocHGlobal(4);
            int ret = GetInterfaceInfo(pifTable, pOutBuff);
            Marshal.FreeHGlobal(pifTable);

            int sizeOut = Marshal.ReadInt32(pOutBuff);
            pifTable = Marshal.AllocHGlobal(sizeOut);

            ret = GetInterfaceInfo(pifTable, pOutBuff);

            Ip_Interface_Info iinf = new Ip_Interface_Info();
            Marshal.PtrToStructure(pifTable, iinf);
            Marshal.FreeHGlobal(pifTable);
            Marshal.FreeHGlobal(pOutBuff);


            Ip_Adapter_Index_Map[] maps = new Ip_Adapter_Index_Map[iinf.NumAdapters];
            for (int i = 0; i < iinf.NumAdapters; i++)
            {
                maps[i] = iinf.Adapter[i];
            }
            iinf.Adapter = maps;
            return iinf;
        }

        /// <summary>
        /// получение количество доступных сетевых интерфейсов
        /// (иногда не совпадает с количеством адаптеров
        /// </summary>
        /// <returns></returns>
        public int GetNumberInterfaces()
        {
            IntPtr pNum = Marshal.AllocHGlobal(4);
            GetNumberOfInterfaces(pNum);
            int num = Marshal.ReadInt32(pNum);
            Marshal.FreeHGlobal(pNum);
            return num;
        }

        /// <summary>
        /// обновление адреса через dhcp
        /// </summary>
        /// <param name="AdapterMap">структура получаема при вызове GetInterfaceInfo</param>
        public void IpRenewAddress(Ip_Adapter_Index_Map AdapterMap)
        {
            IntPtr padMap = Marshal.AllocHGlobal(Marshal.SizeOf(AdapterMap));
            Marshal.StructureToPtr(AdapterMap, padMap, true);
            int ret = IpRenewAddress(padMap);
            Marshal.FreeHGlobal(padMap);
        }

        /// <summary>
        /// освобождение адреса через dhcp
        /// </summary>
        /// <param name="AdapterMap"></param>
        public void IpReleaseAddress(Ip_Adapter_Index_Map AdapterMap)
        {
            IntPtr padMap = Marshal.AllocHGlobal(Marshal.SizeOf(AdapterMap));
            Marshal.StructureToPtr(AdapterMap, padMap, true);
            int ret = IpReleaseAddress(padMap);
            Marshal.FreeHGlobal(padMap);
        }

        /// <summary>
        /// for testing
        /// </summary>
        public void SetDhcpAddress()
        {
            Ip_Interface_Info intf = this.GetInterfaceInfo();
            //this.IpRenewAddress(intf.Adapter[2]);
            this.IpReleaseAddress(intf.Adapter[0]);
        }
    }
}
