using System; 
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Collections;
using System.IO.Compression;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading;


public static class UserAgent
{
    public const string InternetExplorer_10x6 = "Mozilla/5.0 (compatible; MSIE 10.6; Windows NT 6.1; Trident/5.0; InfoPath.2; SLCC1; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET CLR 2.0.50727) 3gpp-gba UNTRUSTED/1.0";
    public const string InternetExplorer_10 = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; WOW64; Trident/6.0)";
    public const string InternetExplorer_9 = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)";

    public const string Chrome_33x0x1750x154 = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/33.0.1750.154 Safari/537.36";
    public const string Chrome_32x0x1667x0 = "Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1667.0 Safari/537.36";
    public const string Chrome_31x0x1650x16 = "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.16 Safari/537.36";

    public const string Yandex13x12x1599x12785 = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.12785 YaBrowser/13.12.1599.12785 Safari/537.36";
    public const string Yandex13x12x1599x13014 = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.13014 YaBrowser/13.12.1599.13014 Safari/537.36";
    public const string Yandex14x2x1700x12599 = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1700.102 YaBrowser/14.2.1700.12599 Safari/537.36";
    public const string W7x64Yandex14x2x1700x12599 = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1700.102 YaBrowser/14.2.1700.12599 Safari/537.36";
    public const string Amigo = "Mozilla/5.0 (Windows NT 6.3) AppleWebKit/537.36 (KHTML like Gecko) Chrome/32.0.1703.124 Safari/537.36 MRCHROME SOC";
    public const string MailRu = "";

    public const string Opera_12x14 = "Opera/9.80 (Windows NT 6.0) Presto/2.12.388 Version/12.14";
    public const string Opera_11x51 = "Opera/9.80 (Windows NT 5.1; U; ru) Presto/2.9.168 Version/11.51";
    public const string Opera_9x64 = "Opera/9.64 (Windows NT 5.1; U; ru) Presto/2.1.1";

    public const string SamsungGalaxyS = "Mozilla/5.0 (Linux; U; Android 2.1-update1; ru-ru; GT-I9000 Build/ECLAIR) AppleWebKit/530.17 (KHTML, like Gecko) Version/4.0 Mobile Safari/530.17";
    public const string SamsungGalaxySAndroid_2x2 = "Mozilla/5.0 (Linux; U; Android 2.2; ru-ru; GT-I9000 Build/FROYO) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1";
    public const string IphoneOS5 = "Mozilla/5.0 (iPhone; CPU iPhone OS 5_0 like Mac OS X) AppleWebKit/534.46 (KHTML, like Gecko) Version/5.1 Mobile/9A334 Safari/7534.48.3";
    public const string iPadOS5 = "Mozilla/5.0 (iPad; CPU OS 5_0 like Mac OS X) AppleWebKit/534.46 (KHTML, like Gecko) Version/5.1 Mobile/9A334 Safari/7534.48.3";
    public const string IphoneOS6 = "AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5376e Safari/8536.25";
    public const string IphoneOS7Safari = "mozilla/5.0 (iphone; cpu iphone os 7_0_2 like mac os x) applewebkit/537.51.1 (khtml, like gecko) version/7.0 mobile/11a501 safari/9537.53";
    public const string IphoneOS7Chrome = "mozilla/5.0 (iphone; cpu iphone os 7_0_2 like mac os x) applewebkit/537.51.1 (khtml, like gecko) crios/30.0.1599.16 mobile/11a501 safari/8536.25";
    public const string IphoneOS7Mercury = "mozilla/5.0 (iphone; cpu iphone os 6_0_1 like mac os x) applewebkit/536.26 (khtml, like gecko) mercury/7.4.2 mobile/10a523 safari/8536.25";
    public const string IphoneOS7Dolphin = "mozilla/5.0 (iphone; cpu iphone os 7_0_2 like mac os x) applewebkit/537.51.1 (khtml, like gecko) version/6.0 mobile/10a523 safari/8536.25";
    public const string OperaMini_4x2x15410 = "Opera/9.80 (J2ME/MIDP; Opera Mini/4.2.15410/870; U; ru) Presto/2.4.15";
    public const string WindowsPhoneFirefox24 = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:24.0) Gecko/20100101 Firefox/24.0";
    public const string Lumia920 = "Mozilla/5.0 (compatible; MSIE 10.0; Windows Phone 8.0; Trident/6.0; IEMobile/10.0; ARM; Touch; NOKIA; Lumia 920)";
    public const string Lumia900 = "Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0; NOKIA; Lumia 900)";
    public const string Lumia820 = "Mozilla/5.0 (compatible; MSIE 10.0; Windows Phone 8.0; Trident/6.0; IEMobile/10.0; ARM; Touch; NOKIA; Lumia 820)";
    public const string Lumia520 = "Mozilla/5.0 (compatible; MSIE 10.0; Windows Phone 8.0; Trident/6.0; IEMobile/10.0; ARM; Touch; NOKIA; Lumia 520)";
    public const string Nokia311 = "Mozilla/5.0 (Series40; Nokia311/03.81; Profile/MIDP-2.1 Configuration/CLDC-1.1) Gecko/20100401 S40OviBrowser/2.2.0.0.31";
    public const string Nokia5800 = "Mozilla/5.0 (SymbianOS/9.4; U; Series60/5.0 Nokia5800d-1/21.0.025; Profile/MIDP-2.1 Configuration/CLDC-1.1 ) AppleWebKit/413 (KHTML, like Gecko) Safari/413";
    public const string NokiaOperaMini8 = "Opera/8.01 (J2ME/MIDP; Opera Mini/3.0.6306/1528; ru; U; ssr)";
}

public struct postData
{
    public string ParametrName;
    public Stream DataStream;
    public string FileName;
    public postData(string ParametrName, Stream DataStream, string FileName)
    {
        this.ParametrName = ParametrName;
        this.DataStream = DataStream;
        this.FileName = FileName;
    }
}


[Description("Set of methods for working with HTTP")]
public class TWebInterface
{
    private string _UserAgent;
    private bool _Redirect;
    private bool _KeepAlive;
    private int _Timeout;
    private bool _AutoCookies;
    private bool _useDefaultHeaders;
    private string _URI = "";

    private const int _waitTimeoutStep = 1000;
    private const int _maxWaitTimeout = 30000;
    private int _waitTimeout = _waitTimeoutStep;

    public string UserAgent
    {
        get { return _UserAgent; }
        set { _UserAgent = value; }
    }
    /// <summary> 
    /// Allow auto redirect 
    /// </summary>
    public bool Redirect
    {
        get { return _Redirect; }
        set { _Redirect = value; }
    }
    public bool KeepAlive
    {
        get { return _KeepAlive; }
        set { _KeepAlive = value; }
    }
    public int Timeout
    {
        get { return _Timeout; }
        set 
        {
            if (value < 0)
            {
                _Timeout = 0;
            }
            else
            {
                if (value > 30000)
                {
                    _Timeout = 30000;
                }
            }
        }
    }
    /// <summary> 
    /// Use own CookieContainer
    /// </summary>
    public bool AutoCookies
    {
        get { return _AutoCookies; }
        set { _AutoCookies = value; }
    }
    /// <summary> 
    /// Automatically add default headers to headers list when making request 
    /// </summary>
    public bool useDefaultHeaders
    {
        get { return _useDefaultHeaders; }
        set { _useDefaultHeaders = value; }
    }
    /// <summary> 
    /// Current URI
    /// </summary>
    public string URI
    {
        get 
        {
            if (_URI != null)
                return _URI;
            else
                return "";

        }
    }
    /// <summary> 
    /// Cookies wor requests
    /// </summary>
    public CookieContainer Cookies;
    /// <summary> 
    /// Default headers list
    /// </summary>
    public Hashtable DefaultHeaders;

    private string _logFile;
    public string logFile
    {
        get { return _logFile; }
        set 
        { 
            _logFile = value; 
        }
    }
    public enum modes { Disable = 0, Errors = 1, Actions = 2, All = 3 }
    private modes _logMode;
    /// <summary> 
    /// 0: logging disabled, 1: Errors only, 2: Errors & actions, 3: Full log. Logging slow down work speed 
    /// </summary>
    public modes logMode
    {
        get { return _logMode; }
        set { _logMode = value; }
    }
    private void toLog(string message, modes mode)
    {
        if (mode == _logMode) // чето тут хунта
        {
            string desc = "";
            switch (mode)
            {
                case modes.Errors:
                    desc = "ERROR";
                    break;
                case modes.Actions:
                    desc = "ACTION";
                    break;
                case modes.All:
                    desc = "INFO";
                    break;
            }
            StreamWriter writer = new StreamWriter(@_logFile, true);
            writer.WriteLine(DateTime.Now.ToString() + " : " + desc + " : " + message);
            writer.Close();
        }
    }

    public TWebInterface(string UserAgent)
	{
        _UserAgent = UserAgent;
        _Redirect = true;
        _KeepAlive = true;
        _Timeout = 3000;
        Cookies = new CookieContainer();
        _AutoCookies = true;
        DefaultHeaders = new Hashtable();
        _useDefaultHeaders = true;
        _logMode = modes.Disable;
        this.logMode = modes.Disable;
        _logFile = Environment.CurrentDirectory+Environment.CurrentDirectory+"\\WebInterfaceLog.txt";

    }
    // Каждый вызов webRequest должен быть потоком. Добавить критические секции в cookies
    private string MimeType(string FieName)
    {
        string mime = "application/octetstream";
        try
        {
            string ext = System.IO.Path.GetExtension(FieName).ToLower();
            Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (rk != null && rk.GetValue("Content Type") != null)
            {
                mime = rk.GetValue("Content Type").ToString();
            }
        }
        catch (Exception ex)
        {
            toLog(ex.Message, modes.Errors);
        }
        return mime;
    }
    public static Hashtable getParametrs(string URL)
    {
        Hashtable newParams = new Hashtable();
        string pattern = @"[_A-Za-z0-9\+%]+=[_A-Za-z0-9\+%]+";
        MatchCollection col = Regex.Matches(URL, pattern);
        foreach (Match m in col)
        {
            string[] param = m.ToString().Split(new char[] { '=' });
            newParams.Add(param[0], param[1]);
        }
        return newParams;
    }
    public string[] StreamToArray(Stream stream, string charset)
    {
        string[] Answer = new string[0];
        StreamReader reader;
        string x;
        if (stream != null)
        {
            if (charset == "")
                charset = "UTF-8";          
            try
            {                
                using (reader = new StreamReader(stream, Encoding.GetEncoding(charset)))
                {                    
                    while (!reader.EndOfStream)
                    {
                        x = reader.ReadLine();
                        Array.Resize<string>(ref Answer, Answer.Length + 1);
                        Answer[Answer.Length - 1] = x;
                    }
                }
            }
            catch (Exception ex)
            {
                toLog(ex.Message, modes.Errors);
            }
            finally
            {
                stream.Close();
            }
        }
        return Answer;
    }
    /// <summary> 
    /// Return null in case of fail
    /// </summary>
    public HttpWebResponse webRequest(string URL, Hashtable headers, Hashtable parametrs, Hashtable cookies, postData[] data, string method)
    {
        _waitTimeout = _waitTimeoutStep;
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        Uri target;
        try
        {
            target = new Uri(URL);
        }
        catch (Exception ex)
        {
            toLog(ex.Message, modes.Errors);
            return null;
        }
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
        request.ServicePoint.Expect100Continue = false; // strange shit should be disabled
        request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        request.ProtocolVersion = HttpVersion.Version11;
        request.KeepAlive = _KeepAlive;
        request.Timeout = _Timeout;
        request.Method = method;
        request.Accept = "*/*";
        request.UserAgent = _UserAgent;
        request.AllowAutoRedirect = _Redirect;

        // Loading Cookies
        if (_AutoCookies)
            request.CookieContainer = Cookies;
        else
            request.CookieContainer = new CookieContainer();
        if (cookies != null)
            foreach (DictionaryEntry cookie in cookies)
                request.CookieContainer.Add(new Cookie(cookie.Key.ToString(), cookie.Value.ToString()) { Domain = target.Host });

        // Loading Headers
        if (_useDefaultHeaders)
            foreach (DictionaryEntry header in DefaultHeaders)
                request.Headers.Add(header.Key.ToString(), header.Key.ToString());
        if (headers != null)
            foreach (DictionaryEntry header in headers)
                request.Headers.Add(header.Key.ToString(), header.Value.ToString());

        // Writing Parametrs & Data
        if (parametrs != null || data != null)
        {
            try
            {
                request.ContentType = "application/x-www-form-urlencoded";
                using (Stream requestStream = request.GetRequestStream())
                {
                    if (data == null)
                    {
                        string res = "";
                        int c = 0;
                        foreach (DictionaryEntry pair in parametrs)
                        {
                            c++;
                            res += pair.Key.ToString() + '=' + pair.Value.ToString();
                            if (c < parametrs.Count)
                                res += '&';
                        }
                        requestStream.Write(Encoding.ASCII.GetBytes(res), 0, Encoding.ASCII.GetByteCount(res));
                    }
                    else
                    {
                        string boundary = "------WebKitFormBoundary" + Guid.NewGuid().ToString();
                        request.ContentType = "multipart/form-data; boundary=" + boundary;
                        string formTemplate = "Content-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}" + "\r\n" + boundary + "\r\n";
                        string format;
                        byte[] byteData = Encoding.ASCII.GetBytes(boundary + "\r\n");
                        requestStream.Write(byteData, 0, byteData.Length);

                        // writting multipart parametrs
                        if (parametrs != null)
                            foreach (DictionaryEntry parametr in parametrs)
                            {
                                format = string.Format(formTemplate, parametr.Key.ToString(), parametr.Value.ToString());
                                byteData = Encoding.ASCII.GetBytes(format);
                                requestStream.Write(byteData, 0, byteData.Length);
                            }

                        // Writing data to Stream
                        formTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                        foreach (postData file in data)
                        {
                            format = string.Format(formTemplate, file.ParametrName, file.FileName, MimeType(file.FileName));
                            byteData = Encoding.ASCII.GetBytes(format);
                            requestStream.Write(byteData, 0, byteData.Length);
                            using (Stream theData = file.DataStream)
                            using (BinaryReader br = new BinaryReader(theData))
                                byteData = br.ReadBytes((int)theData.Length);
                            requestStream.Write(byteData, 0, byteData.Length);
                            byteData = Encoding.ASCII.GetBytes(boundary + "\r\n");
                            requestStream.Write(byteData, 0, byteData.Length);
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.Timeout)
                    return webRequest(URL, headers, parametrs, cookies, data, method);
            }
        }

        // Getting page
        try
        {
            HttpWebResponse Response = (HttpWebResponse)request.GetResponse();
            _URI = Response.ResponseUri.AbsoluteUri;
            return Response;
        }
        catch (WebException ex)
        {
            // toLog(ex.Message, modes.Errors);
            Console.WriteLine(ex.Message);
            if (ex.Status == WebExceptionStatus.Timeout)
            {
                request = null;

                if (_waitTimeout < _maxWaitTimeout)
                    _waitTimeout += _waitTimeoutStep;
                Thread.Sleep(_waitTimeout);

                return webRequest(URL, headers, parametrs, cookies, data, method);
            }
            else
            {
                WebResponse response = ex.Response;
                HttpWebResponse newResponse = (HttpWebResponse)response;
                if (newResponse != null)
                    _URI = newResponse.ResponseUri.AbsoluteUri;
                return newResponse;
            }
        }
    }

    private string[] _empty()
    {
        return new string[0];
    }
    public string[] getURL(string url)
    {
        /// download page with default cookies and headers
        return getURL(url,null, null);
    }
    public string[] getURL(string url, Hashtable headers)
    {
        return getURL(url, headers, null);
    }
    public string[] getURL(string url, Hashtable headers, Hashtable cookies)
    {
        HttpWebResponse answer = webRequest(url, headers, null, cookies, null, "GET");
        if (answer != null)
        {
            return StreamToArray(answer.GetResponseStream(), answer.CharacterSet);
        }
        else
        {
            return _empty();
        }        
    }

    public string[] postURL(string url)
    {
        return postURL(url, null);
    }
    public string[] postURL(string url, Hashtable parametrs)
    {
        return postURL(url, null, parametrs);
    }
    public string[] postURL(string url, Hashtable parametrs, postData data)
    {
        return postURL(url, null, parametrs, null, data);
    }
    public string[] postURL(string url, Hashtable headers, Hashtable parametrs)
    {
        return postURL(url, headers, parametrs, null, null);
    }
    public string[] postURL(string url, Hashtable headers, Hashtable parametrs, Hashtable cookies)
    {
        return postURL(url, headers, parametrs, cookies, null);
    }
    public string[] postURL(string url, Hashtable headers, Hashtable parametrs, Hashtable cookies, postData data)
    {
        postData[] Data = {data};
        return postURL(url, headers, parametrs, cookies, Data);
    }
    public string[] postURL(string url, Hashtable headers, Hashtable parametrs, Hashtable cookies, postData[] data)
    {
        HttpWebResponse answer = webRequest(url, headers, parametrs, cookies, data, "POST");
        if (answer != null)
            return StreamToArray(answer.GetResponseStream(), answer.CharacterSet);
        else
            return _empty();
    }

    public Stream getData(string url)
    {
        return getData(url, null, null);
    }
    public Stream getData(string url, Hashtable headers)
    {
        return getData(url, headers, null);
    }
    public Stream getData(string url, Hashtable headers, Hashtable cookies)
    {        
        HttpWebResponse answer = webRequest(url, headers, null, cookies, null, "GET");
        if (answer != null)
            return answer.GetResponseStream();
        else
            return new MemoryStream();
    }

    public static class Tools
    {
        /// <summary> 
        /// Verifies that all open brackets were closed 
        /// </summary>
        private static bool checkBrackets(string line)
        {
            int round = 0;  // ()
            int square = 0; // []
            int curly = 0;  // {}
            int angle = 0;  // <>
            foreach (char c in line)
            {
                switch (c)
                {
                    case '(': 
                        round++;
                        break;
                    case '[':
                        square++;
                        break;
                    case '{':
                        curly++;
                        break;
                    case '<':
                        angle++;
                        break;
                    case ')':
                        round--;
                        break;
                    case ']':
                        square--;
                        break;
                    case '}':
                        curly--;
                        break;
                    case '>':
                        angle--;
                        break;
                }                    
            }
            if (round == 0 && curly == 0 && square == 0 && angle == 0)
                return true;
            else
                return false;
        }
    }
}

/// <summary>
/// Импользует сайт 2ip.ru для получения и отслеживания текущего внещнего IP-адреса
/// </summary>
public static class externalIP
{
    private static TWebInterface HTTP = new TWebInterface(UserAgent.InternetExplorer_10);
    private static string url = "http://2ip.ru";
    private static string page;
    private static bool _watchIP = false;
    private static Thread _watcher;

    private static Thread loader;
    private static string l_answer;
    private const int waitStep = 100;
    private const int maxWait = 1000;
    private static int wait = waitStep;

    /// <summary>
    /// Время между проверками IP-адреса
    /// </summary>
    public static int Interval = 1000;

    public delegate void changed(string newIP);
    /// <summary>
    /// Событие вызываемое при смене IP адреса - возвращает новый адрес
    /// </summary>
    public static event changed OnChange;

    private static void Watcher()
    {
        string l_ip = Get();
        string ip = "";
        while (_watchIP)
        {                
            if ((ip = Get()) != l_ip)
            {
                l_ip = ip;
                if (OnChange != null)
                    OnChange(ip);
            }
            else
                Thread.Sleep(Interval);
        }
    }

    private static void get()
    {
        page = string.Join(" ", HTTP.getURL(url));
        page = Regex.Match(page, "<big id=\"d_clip_button\">(\\d{1,3}\\.){3}\\d{1,3}").ToString();
        if (page != "")
            l_answer = page.Remove(0, 24);
    }
    /// <summary>
    /// Получить текущий внешний IP-адрес;
    /// </summary>
    public static string Get()
    {
        l_answer = "";
        wait = waitStep;
        while (l_answer == "")
        {
            try
            {
                loader = new Thread(get);
                loader.Start();
                loader.Join(wait);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при получении ай-пи. {0}", ex.Message);
                // nothing to do here
            }
            if (wait < maxWait)
                wait += waitStep;
        }
        return page;
    }
    /// <summary>
    /// Включить\\выключить отслеживание изменения IP-адреса
    /// </summary>
    public static bool Watch
    {
        get { return _watchIP; }
        set
        {
            if (value)
            {
                if (!_watchIP)
                {
                    _watchIP = value;
                    _watcher = new Thread(Watcher);
                    _watcher.Start();
                }
            }
            else
            {
                _watchIP = value;
                try
                {
                    _watcher.Join(Interval);
                }
                catch (Exception ex)
                { 
                    //Забей и выкинь
                }
            }
        }
    }
}