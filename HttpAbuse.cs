using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;


namespace MyLib
{
    namespace HTTP
    {
        public static class Browser
        {
            public const string Chrome_W7_x64_v38x0x2125x101 = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/38.0.2125.101 Safari/537.36";
            public const string IE_W7_x64_v11 = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
            public const string Amigo_W7_x64_v32x0x1709x113 = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1709.113 Amigo/32.0.1709.113 MRCHROME SOC Safari/537.36";
            public const string Opera_W7_x64_v25x0x1614x50 = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/38.0.2125.101 Safari/537.36 OPR/25.0.1614.50";
            public static Hashtable getDefaultHeaders(string userAgent)
            {
                Hashtable headers = new Hashtable();
                switch (userAgent)
                {
                    case Chrome_W7_x64_v38x0x2125x101:
                        headers.Add("Accept-Encoding", "gzip,deflate");
                        headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4,uk;q=0.2");
                        headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                        break;
                    case IE_W7_x64_v11:
                        headers.Add("Accept-Encoding", "gzip,deflate");
                        headers.Add("Accept", "text/html, application/xhtml+xml, */*");
                        headers.Add("Accept-Language", "ru-RU");
                        break;
                    default:
                        break;
                        
                }
                return headers;
            }
            public static string getBoundary(string userAgent)
            {
                Random r = new Random();
                string res = "";
                switch (userAgent)
                {
                    case Browser.Chrome_W7_x64_v38x0x2125x101:
                        res = "----WebKitFormBoundary";
                        for (int i = 0; i < 16; i++)
                            res += Convert.ToChar(Convert.ToInt32(Math.Floor(26 * r.NextDouble() + 65)));
                        break;
                    default:
                        res = "--" + Guid.NewGuid().ToString().Replace("-", "");
                        break;
                }
                return res;
            }
        }

        public enum RequestMethod { GET, HEAD, POST }

        /// <summary>
        /// Данные с бинарным содержимым
        /// </summary>
        public class MultipartData
        {
            /// <summary>
            /// Весь заголовок бинарного вложения, все поля и атрибуты в одной строке
            /// https://www.ietf.org/rfc/rfc2388.txt
            /// </summary>
            public string Header
            {
                get
                {
                    string res = "content-disposition: form-data; name=\"" + Name + '"';
                    if (!string.IsNullOrEmpty(FileName))
                        res += "; filename=\"" + FileName + "\\\r\\n";

                    if (!string.IsNullOrEmpty(ContentType))
                        res += "content-type: " + ContentType;
                    if (!string.IsNullOrEmpty(Charset))
                        res += ";charset=" + Charset + "\\\r\\n";

                    if (!string.IsNullOrEmpty(ContentTransferEncoding))
                        res += "content-transfer-encoding: " + ContentTransferEncoding + "\\\r\\n";
                    return res;
                }
            }
            /// <summary>
            /// Имя управляющего элемента
            /// </summary>
            public string Name { get; private set; }
            /// <summary>
            /// Имя файла бинарного содердимого
            /// </summary>
            public string FileName { get; private set; }
            /// <summary>
            /// MIME тип содержимого
            /// </summary>
            public string ContentType { get; private set; }
            /// <summary>
            /// Кодировка. Содержимого???
            /// </summary>
            public string Charset { get; set; }
            /// <summary>
            /// Еще кодировка содердимого? Или тип сжатия содердимого?
            /// </summary>
            public string ContentTransferEncoding { get; private set; }
            /// <summary>
            /// Бинарные данные
            /// </summary>
            public byte[] Data { get; private set; }

            public MultipartData(string name, byte[] data, string fileName = "", string contentType = "", string charset = "", string encoding = "")
            {
                Name = name;
                FileName = fileName;
                ContentType = contentType;
                Charset = charset;
                ContentTransferEncoding = encoding;
                Data = data;
            }
        }
        /// <summary>
        /// Набор некоторых параметров для HttpWebRequest
        /// </summary>
        public class RequestParametrs
        {
            /// <summary>
            /// Идентификатор веб-клиента
            /// </summary>
            public string UserAgent { get; set; }
            /// <summary>
            /// Автоматическая переадресация при загрузке страницы
            /// </summary>
            public bool AllowAutoRedirect { get; set; }
            public bool KeepAlive { get; set; }
            public int MinimumTimeout { get; set; }
            public int TimeoutStep { get; set; }
            public int MaximumTimeout { get; set; }
            public int TryCount { get; set; }
            /// <summary>
            /// Пытатся загрузить страницу любой ценой
            /// </summary>
            public bool TryUntilLoad { get; set; }
            /// <summary>
            /// Набор заголовков которые добавляються вместе с этим веб-клиентом
            /// </summary>
            public Hashtable Headers;

            /// <summary>
            /// Изменить идентификатор веб-клиента и набор его заголовков
            /// </summary>
            /// <param name="userAgent">Новый веб-клиент</param>
            /// <param name="headers">Новые заголовки</param>
            public void ChangeBrowser(string userAgent, Hashtable headers)
            {
                UserAgent = string.IsNullOrEmpty(userAgent) ? Browser.IE_W7_x64_v11 : userAgent;
                Headers = headers != null ? headers : Browser.getDefaultHeaders(UserAgent);
            }

            public RequestParametrs(string useragent = null, Hashtable headers = null, bool autoRedirect = true, bool keepAlive = true)
            {
                ChangeBrowser(useragent, headers);
                AllowAutoRedirect = autoRedirect;
                KeepAlive = keepAlive;

                MinimumTimeout = 5000;
                TimeoutStep = 5000;
                MaximumTimeout = 15000;
                TryCount = 5;
                TryUntilLoad = true;
            }
        }
        /// <summary>
        /// Данные конкретного запроса к конкретному URL
        /// </summary>
        public class RequestData
        {
            private string _url;
            /// <summary>
            /// Адрес, по которому будет выполнен запрос.
            /// Если метод - GET и указаны параметры то вернет путь с параметрыми
            /// </summary>
            public string URL
            {
                get
                {
                    if (Method == RequestMethod.GET && Parametrs.Count > 0)
                        return WebTools.MergeParametrsToURL(_url, Parametrs);
                    return _url;
                }
                set { _url = value; }
            }
            public RequestMethod Method;
            public Hashtable Parametrs;
            public Hashtable Headers;
            public List<MultipartData> Data;

            public RequestData(string url, RequestMethod method, Hashtable headers, Hashtable parametrs = null, List<MultipartData> data = null)
            {
                URL = url;
                Method = method;
                Parametrs = parametrs != null ? parametrs : new Hashtable();
                Headers = headers != null ? headers : new Hashtable();
                Data = data != null ? data : new List<MultipartData>();
            }
        }
        /// <summary>
        /// Ответ от сервера на запрос
        /// </summary>
        public class ResponseData
        {
            public HttpWebResponse Response;

            private List<string> _lines = null;
            private Encoding _lastEncoding = null;

            /// <summary>
            /// Ссылка на страницу, от которой был получен этот ответ
            /// </summary>
            public string Url
            {
                get { return Response.ResponseUri.ToString(); }
            }
            /// <summary>
            /// Cookies, полученные вместе с ответом (Отправленные cookies не содержаться здесь)
            /// </summary>
            public CookieCollection Cookies
            {
                get { return Response.Cookies; }
            }

            private void _getPage(Encoding encoding)
            {
                if (_lines == null || _lastEncoding != encoding)
                {
                    _lastEncoding = encoding;
                    _lines = new List<string>();
                    using (StreamReader reader = new StreamReader(Response.GetResponseStream(), _lastEncoding))
                    {
                        while (!reader.EndOfStream)
                            _lines.Add(reader.ReadLine());
                    }
                }
            }
            private Encoding _chooseEncoding(string encoding)
            {
                Encoding res;
                try
                {
                    res = Encoding.GetEncoding(encoding);
                }
                catch (ArgumentException)
                {
                    res = _lastEncoding == null ? Encoding.UTF8 : _lastEncoding;
                }
                return res;
            }

            #region Список строк
            public List<string> GetLines(Encoding encoding)
            {
                _getPage(encoding);
                return _lines;
            }
            public List<string> GetLines(string encoding)
            {
                return GetLines(_chooseEncoding(encoding));
            }
            #endregion
            public List<string> Lines
            {
                get
                {
                    return GetLines(_chooseEncoding(""));
                }
            }            
            #region Страница строкой
            public string GetPage(Encoding encoding, string separator)
            {
                return string.Join(separator, GetLines(encoding));
            }
            public string GetPage(string encoding, string separator)
            {
                return string.Join(separator, GetLines(encoding));
            }
            public string GetPage(Encoding encoding)
            {
                return GetPage(encoding, Environment.NewLine);
            }
            public string GetPage(string encoding)
            {
                return GetPage(encoding, Environment.NewLine);
            }
            #endregion
            public string Page
            {
                get
                {
                    return GetPage(Response.ContentEncoding);
                }
            }

            public ResponseData(HttpWebResponse response)
            {
                Response = response;
            }
        }

        public class SimpleHttpBrowser
        {
            /// <summary>
            /// Параметры HTTP клиента
            /// </summary>
            public      RequestParametrs    Parametrs;
            /// <summary>
            /// Данные запроса, который будет отправлен
            /// </summary>
            public      RequestData         Data;
            /// <summary>
            /// Ответ на последний отправленный запрос
            /// </summary>
            public      ResponseData        Response = null;

            /// <summary>
            /// Подготавливает запрос исходя из указанных данных и передает ссылку на него.
            /// Метод выполняет полный пересбор запроса каждый раз
            /// </summary>
            /// <param name="cookies">Cookies для будущего запроса</param>
            /// <returns>Сформированный экземпляр HttpWebRequest</returns>
            public HttpWebRequest GetRequest(CookieCollection cookies)
            {
                return WebTools.PrepareWebRequest(cookies, Parametrs, Data);
            }
            /// <summary>
            /// Подготавливает запрос исходя из указанных данных и передает ссылку на него.
            /// Метод выполняет полный пересбор запроса каждый раз
            /// </summary>
            /// <param name="cookies">Cookies запроса</param>
            /// <returns>Ответ от выполненного запроса HttpWebRequest</returns>
            public HttpWebResponse GetResponse(ref CookieCollection cookies)
            {
                if (cookies == null)
                    cookies = new CookieCollection();
                HttpWebRequest req = GetRequest(cookies);
                HttpWebResponse response = null;
                int errors = 0;
                bool success = false;
                while (!success)
                {
                    try
                    {
                        response = (HttpWebResponse)req.GetResponse();
                        cookies.Add(response.Cookies);
                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.OK:
                                success = true;
                                break;
                            case HttpStatusCode.Redirect:
                                if (Parametrs.AllowAutoRedirect)
                                    req = WebTools.PrepareWebRequest(cookies, Parametrs, new RequestData(WebTools.RedirectURL(req.RequestUri.OriginalString, response.Headers["Location"]), RequestMethod.GET, null));
                                else
                                    goto case HttpStatusCode.OK;
                                break;
                            default:
                                goto case HttpStatusCode.OK;
                        }
                    }
                    catch (Exception)
                    {
                        if (errors <= Parametrs.TryCount)
                        {
                            errors++;
                        }
                        else
                            success = true;
                    }
                }
                Response = new ResponseData(response);
                return response;
            }
            /// <summary>
            /// Подготавливает запрос исходя из указанных данных и передает ссылку на него.
            /// Метод выполняет полный пересбор запроса каждый раз
            /// </summary>
            /// <param name="cookies">Cookies запроса</param>
            /// <param name="data">Данные запроса</param>
            /// <returns>Ответ от выполненного запроса HttpWebRequest</returns>
            public HttpWebResponse GetResponse(ref CookieCollection cookies, RequestData data)
            {
                Data = data;
                return GetResponse(ref cookies);
            }
            public ResponseData GetResponseData(ref CookieCollection cookies)
            {
                GetResponse(ref cookies);
                return Response;
            }
            public ResponseData GetResponseData(ref CookieCollection cookies, RequestData data)
            {
                Data = data;
                return GetResponseData(ref cookies);
            }

            public SimpleHttpBrowser(RequestParametrs defaultParametrs = null)
            {
                Parametrs = defaultParametrs == null ? new RequestParametrs() : defaultParametrs;
            }
            public SimpleHttpBrowser() : this(null) { }
        }

        public static class WebTools
        {
            public const string srcPattern = "src\\s*=\\s*(?:[\"'](?<1>[^\"']*)[\"']|(?<1>\\S+))";

            /// <summary>
            /// Задает начальные настройки для запроса
            /// </summary>
            private static void _baseSettings(HttpWebRequest webRequest, RequestParametrs parametrs)
            {
                webRequest.ServicePoint.Expect100Continue = false; // strange shit should be disabled
                webRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                webRequest.ProtocolVersion = HttpVersion.Version11;

                webRequest.AllowAutoRedirect = false;
                webRequest.UserAgent = parametrs.UserAgent;
                webRequest.KeepAlive = parametrs.KeepAlive;
                webRequest.Timeout = parametrs.MinimumTimeout;
            }
            /// <summary>
            /// Объединяет заголовки браузера и заголовки данных запроса с приоритетом заголовков данных запроса.
            /// Переносит константные заголовки из общего списка в параметры запроса и добавляет заголовки в запрос.
            /// </summary>
            private static void _addHeaders(HttpWebRequest webRequest, Hashtable defaultHeaders, Hashtable headers)
            {
                Hashtable newHeaders = new Hashtable(headers);
                foreach (DictionaryEntry h in defaultHeaders)
                    if (!headers.ContainsKey(h.Key))
                        headers.Add(h.Key, h.Value);

                if (newHeaders.ContainsKey("Accept"))
                {
                    webRequest.Accept = (string)newHeaders["Accept"];
                    newHeaders.Remove("Accept");
                }
                if (newHeaders.ContainsKey("Referer"))
                {
                    webRequest.Referer = (string)newHeaders["Referer"];
                    newHeaders.Remove("Referer");
                }
                if (newHeaders.ContainsKey("Expect"))
                {
                    webRequest.Expect = (string)newHeaders["Expect"];
                    newHeaders.Remove("Expect");
                }
                if (newHeaders.ContainsKey("Host"))
                {
                    webRequest.Host = (string)newHeaders["Host"];
                    newHeaders.Remove("Host");
                }

                foreach (DictionaryEntry h in newHeaders)
                    webRequest.Headers.Add((string)h.Key, (string)h.Value);

                newHeaders.Clear();
            }
            /// <summary>
            /// Добавляет cookies к запросу если они есть
            /// </summary>
            /// <param name="cookies">Cookies</param>
            private static void _addCookies(HttpWebRequest webRequest, CookieCollection cookies)
            {
                webRequest.CookieContainer = new CookieContainer();
                if (cookies != null)
                {
                    foreach (Cookie c in cookies)
                        webRequest.CookieContainer.Add(c);
                }
            }
            private static void _processPOST(HttpWebRequest webRequest, RequestData data, string userAgent)
            {
                string boundary = Browser.getBoundary(userAgent);
                string tmp;
                byte[] tb;
                using (Stream requestStream = webRequest.GetRequestStream())
                {
                    if (data.Data.Count > 0)
                    {
                        webRequest.ContentType = "Content-Type: multipart/form-data; " + boundary;
                        if (data.Parametrs.Count > 0)
                        {
                            tmp = "";
                            foreach (DictionaryEntry p in data.Parametrs)
                            {
                                tmp += string.Format("--{0}\\r\\nContent-Disposition: form-data; name=\"{1}\"\\r\\n\\r\\n{2}\\r\\n",
                                    boundary, p.Key, p.Value);
                            }
                            tb = Encoding.ASCII.GetBytes(tmp);
                            requestStream.Write(tb, 0, tb.Length);

                        }
                        tmp = "";
                        foreach (MultipartData d in data.Data)
                        {
                            tmp += string.Format("--{0}\\r\\n{1}\\r\\n", boundary, d.Header);
                            tb = Encoding.ASCII.GetBytes(tmp);
                            requestStream.Write(tb, 0, tb.Length);
                            requestStream.Write(d.Data, 0, d.Data.Length);
                        }

                        // Closing boundary
                        string b_end = "--" + boundary + "--\\r\\n";
                        requestStream.Write(Encoding.ASCII.GetBytes(b_end), 0, Encoding.ASCII.GetByteCount(b_end));
                    }
                    else if (data.Parametrs.Count > 0)
                    {
                        webRequest.ContentType = "application/x-www-form-urlencoded";
                        string t = "";
                        foreach (DictionaryEntry p in data.Parametrs)
                            t += string.Format("{0}={1}&", boundary, p.Key, p.Value);
                        requestStream.WriteAsync(Encoding.ASCII.GetBytes(t), 0, Encoding.ASCII.GetByteCount(t));
                    }
                }
            }
            public static HttpWebRequest PrepareWebRequest(CookieCollection cookies, RequestParametrs parametrs, RequestData data)
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(data.URL);
                webRequest.Method = data.Method.ToString("F");
                _baseSettings(webRequest, parametrs);

                _addHeaders(webRequest, parametrs.Headers, data.Headers);
                _addCookies(webRequest, cookies);

                // Write parametr & data/multipart data
                switch (data.Method)
                {
                    case RequestMethod.POST:
                        _processPOST(webRequest, data, parametrs.UserAgent);
                        break;
                }
                return webRequest;
            }

            public static string RedirectURL(string oldUrl, string redirectUrl)
            {
                if (redirectUrl[0] == '/')
                {
                    Uri oldUri = new Uri(oldUrl);
                    return oldUri.Scheme + "://" + oldUri.Host + redirectUrl;
                }
                else
                    return redirectUrl;
            }

            public static CookieCollection parseCookies(CookieContainer _cookies)
            {
                CookieCollection res = new CookieCollection();
                FieldInfo fiDomainTable = typeof(CookieContainer).GetField("m_domainTable", BindingFlags.NonPublic | BindingFlags.Instance);
                Hashtable domainTable = (Hashtable)fiDomainTable.GetValue(_cookies);
                foreach (string uri in domainTable.Keys)
                {
                    object obPathList = domainTable[uri];
                    FieldInfo fiList = obPathList.GetType().GetField("m_list", BindingFlags.NonPublic | BindingFlags.Instance);
                    SortedList pathList = (SortedList)fiList.GetValue(obPathList);
                    foreach (string key in pathList.Keys)
                    {
                        CookieCollection cookies = (CookieCollection)pathList[key];
                        foreach (Cookie cookie in cookies)
                            res.Add(cookie);
                    }
                }
                return res;
            }
            private static string ToUriInfo(Hashtable parametrs)
            {
                string res = "";
                foreach (DictionaryEntry e in parametrs)
                    res += (string)e.Key + '=' + (string)e.Value + '&';
                return res.Remove(res.Length - 1, 1);
            }
            public static string MergeParametrsToURL(string url, Hashtable parametrs)
            {
                string res = url + '?' + ToUriInfo(parametrs);
                return res;
            }
            public static string MergeParametrsToURL(Uri url, Hashtable parametrs)
            {
                return MergeParametrsToURL(url.OriginalString, parametrs);
            }
            public static Hashtable ParseUrlQuery(string url)
            {
                Hashtable res = new Hashtable();
                if (url.IndexOf('?') > 0)
                {
                    url = url.Remove(0, url.IndexOf('?') + 1);
                    string[] p;
                    foreach (string pair in url.Split('&'))
                    {
                        p = pair.Split('=');
                        res.Add(p[0], p[1]);
                    }
                }
                return res;
            }
            public static List<string> GetPage(Stream stream, string charset)
            {
                List<string> res = new List<string>();
                if (stream != null)
                {
                    StreamReader reader;
                    string x;
                    using (reader = new StreamReader(stream, Encoding.GetEncoding(charset)))
                    {
                        while (!reader.EndOfStream)
                        {
                            x = reader.ReadLine();
                            res.Add(x);
                        }
                    }
                    stream.Close();
                }
                return res;
            }
            public static List<string> GetPage(Stream stream)
            {
                return GetPage(stream, "UTF-8");
            }

            public static List<string> DumpFrames(string inputString)
            {
                List<string> res = new List<string>();
                Match m;
                m = Regex.Match(inputString, srcPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                while (m.Success)
                {
                    res.Add(m.Groups[1].ToString());
                    m = m.NextMatch();
                }
                return res;
            }
        }

    }
}