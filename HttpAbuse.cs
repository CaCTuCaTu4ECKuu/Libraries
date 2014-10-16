using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MyLib
{
    namespace HTTP
    {
        public static class UserAgentCollection
        {
            public static class PC
            {
                public static class Chrome
                {
                    public const string v33_0_1750_154 = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/33.0.1750.154 Safari/537.36";
                    public const string x64v32_0_1667_0 = "Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1667.0 Safari/537.36";
                    public const string v31_0_1650_16 = "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.16 Safari/537.36";
                }
                public static class Firefox
                {
                    public const string v31_0 = "Mozilla/5.0 (Windows NT 5.1; rv:31.0) Gecko/20100101 Firefox/31.0";
                    public const string x86v31_0 = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:29.0) Gecko/20120101 Firefox/29.0";
                    public const string x64v31_0 = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:25.0) Gecko/20100101 Firefox/29.0";
                    public const string v23 = "Mozilla/5.0 (Windows NT 6.2; rv:22.0) Gecko/20130405 Firefox/23.0";
                    public const string v21_0_1 = "Mozilla/5.0 (Windows NT 6.2; Win64; x64; rv:16.0.1) Gecko/20121011 Firefox/21.0.1";
                }
                public static class Opera
                {
                    public const string v12_14 = "Opera/9.80 (Windows NT 6.0) Presto/2.12.388 Version/12.14";
                    public const string v11_51 = "Opera/9.80 (Windows NT 5.1; U; ru) Presto/2.9.168 Version/11.51";
                    public const string v9_64 = "Opera/9.64 (Windows NT 5.1; U; ru) Presto/2.1.1";
                }
                public static class IE
                {
                    public const string v10_6 = "Mozilla/5.0 (compatible; MSIE 10.6; Windows NT 6.1; Trident/5.0; InfoPath.2; SLCC1; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET CLR 2.0.50727) 3gpp-gba UNTRUSTED/1.0";
                    public const string v10 = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; WOW64; Trident/6.0)";
                    public const string v9 = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)";
                }
            }
            public static class Mobile
            {
                public static class Android
                {
                    public const string SamsungGalaxyS = "Mozilla/5.0 (Linux; U; Android 2.1-update1; ru-ru; GT-I9000 Build/ECLAIR) AppleWebKit/530.17 (KHTML, like Gecko) Version/4.0 Mobile Safari/530.17";
                    public const string SamsungGalaxySAndroid_2_2 = "Mozilla/5.0 (Linux; U; Android 2.2; ru-ru; GT-I9000 Build/FROYO) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1";
                }
                public static class iOS
                {
                    public const string IphoneOS5 = "Mozilla/5.0 (iPhone; CPU iPhone OS 5_0 like Mac OS X) AppleWebKit/534.46 (KHTML, like Gecko) Version/5.1 Mobile/9A334 Safari/7534.48.3";
                    public const string iPadOS5 = "Mozilla/5.0 (iPad; CPU OS 5_0 like Mac OS X) AppleWebKit/534.46 (KHTML, like Gecko) Version/5.1 Mobile/9A334 Safari/7534.48.3";
                    public const string IphoneOS6 = "AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5376e Safari/8536.25";
                    public const string IphoneOS7Safari = "mozilla/5.0 (iphone; cpu iphone os 7_0_2 like mac os x) applewebkit/537.51.1 (khtml, like gecko) version/7.0 mobile/11a501 safari/9537.53";
                    public const string IphoneOS7Chrome = "mozilla/5.0 (iphone; cpu iphone os 7_0_2 like mac os x) applewebkit/537.51.1 (khtml, like gecko) crios/30.0.1599.16 mobile/11a501 safari/8536.25";
                    public const string IphoneOS7Mercury = "mozilla/5.0 (iphone; cpu iphone os 6_0_1 like mac os x) applewebkit/536.26 (khtml, like gecko) mercury/7.4.2 mobile/10a523 safari/8536.25";
                    public const string IphoneOS7Dolphin = "mozilla/5.0 (iphone; cpu iphone os 7_0_2 like mac os x) applewebkit/537.51.1 (khtml, like gecko) version/6.0 mobile/10a523 safari/8536.25";
                }
                public static class WindowsPhone
                {
                    public const string WindowsPhoneFirefox24 = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:24.0) Gecko/20100101 Firefox/24.0";
                    public const string Lumia920 = "Mozilla/5.0 (compatible; MSIE 10.0; Windows Phone 8.0; Trident/6.0; IEMobile/10.0; ARM; Touch; NOKIA; Lumia 920)";
                    public const string Lumia900 = "Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0; NOKIA; Lumia 900)";
                    public const string Lumia820 = "Mozilla/5.0 (compatible; MSIE 10.0; Windows Phone 8.0; Trident/6.0; IEMobile/10.0; ARM; Touch; NOKIA; Lumia 820)";
                    public const string Lumia520 = "Mozilla/5.0 (compatible; MSIE 10.0; Windows Phone 8.0; Trident/6.0; IEMobile/10.0; ARM; Touch; NOKIA; Lumia 520)";
                }
                public static class Other
                {
                    public const string OperaMini_4_2_15410 = "Opera/9.80 (J2ME/MIDP; Opera Mini/4.2.15410/870; U; ru) Presto/2.4.15";
                    public const string Nokia311 = "Mozilla/5.0 (Series40; Nokia311/03.81; Profile/MIDP-2.1 Configuration/CLDC-1.1) Gecko/20100401 S40OviBrowser/2.2.0.0.31";
                    public const string Nokia5800 = "Mozilla/5.0 (SymbianOS/9.4; U; Series60/5.0 Nokia5800d-1/21.0.025; Profile/MIDP-2.1 Configuration/CLDC-1.1 ) AppleWebKit/413 (KHTML, like Gecko) Safari/413";
                    public const string NokiaOperaMini8 = "Opera/8.01 (J2ME/MIDP; Opera Mini/3.0.6306/1528; ru; U; ssr)";
                }
            }
        }
        public class PirateHTTP
        {
            private string _userAgent;
            private bool _autoRedirect;
            private bool _keepAlive;
            private int _timeout;
            private bool _autoCookies;
            private bool _useDefaultHeaders;

            public Hashtable _defaultHeaders;

            public PirateHTTP()
            {
                _userAgent = UserAgentCollection.PC.IE.v10;
                _autoRedirect = true;
                _keepAlive = true;
                _timeout = 300;
                _autoCookies = true;
                _useDefaultHeaders = true;
                _defaultHeaders = new Hashtable();
            }
        }
    }
}
