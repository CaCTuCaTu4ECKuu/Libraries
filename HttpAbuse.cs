using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Net.Sockets;

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
        }

        public enum RequestMethod { GET, POST }
        public class HttpAnswer
        {
            private HttpWebResponse _response;
            private Stream _responseStream;
            private List<string> _page;
            private Hashtable _headers;

            public string URL
            {
                get { return _response.ResponseUri.ToString(); }
            }
            public HttpStatusCode Code
            {
                get { return _response.StatusCode; }
            }
            public string CharacterSet
            {
                get { return _response.CharacterSet; }
            }
            public string Header(string name)
            {
                if (_headers == null)
                    _headers = Headers;
                if (_headers.ContainsKey(name))
                    return (string)_headers[name];
                else
                    return "";

            }
            public Hashtable Headers
            {
                get
                {
                    if (_headers == null)
                    {
                        _headers = new Hashtable();
                        foreach (string h in _response.Headers.AllKeys)
                            _headers.Add(h, _response.Headers[h]);
                    }
                    return _headers;
                }
            }
            public CookieCollection Cookies
            {
                get { return _response.Cookies; }
            }
            public Stream ResponseStream
            {
                get 
                {
                    if (_responseStream == null)
                        _responseStream = _response.GetResponseStream();
                    return _responseStream;
                }
            }
            public List<string> Lines
            {
                get
                {
                    if (_response != null)
                    {
                        if (_page == null)
                            _page = WebTools.GetPage(ResponseStream, _response.CharacterSet);
                        return _page;
                    }
                    return new List<string>();
                }
            }
            public string Page
            {
                get { return string.Join("\n", Lines); }
            }
            public HttpAnswer(HttpWebResponse response)
            {
                _response = response;
                _responseStream = null;
                _page = null;
                _headers = null;
            }
        }
        public class HttpRequest
        {
            private Uri _adress;
            private RequestMethod _method;
            private byte _wwwFormUrlEncoded;
            private Hashtable _headers;
            private CookieCollection _cookies;
            private Hashtable _parametrs;
            private Hashtable _data;

            private PirateHTTP _browser;
            private HttpAnswer _answer;
            private bool _requestChanged;

            private void rc()
            {
                _requestChanged = true;
            }
            public void AddParametr(string name, string value)
            {
                if (name != "")
                {
                    if (_parametrs.ContainsKey(name))
                        _parametrs[name] = value;
                    else
                        _parametrs.Add(name, value);
                    rc();
                }
            }
            public void AddParametrs(Hashtable parametrs)
            {
                foreach (DictionaryEntry e in parametrs)
                    AddParametr((string)e.Key, (string)e.Value);
            }
            public void AddHeader(string name, string value)
            {
                if (name != "")
                {
                    if (_headers.ContainsKey(name))
                        _headers[name] = value;
                    else
                        _headers.Add(name, value);
                    rc();
                }
            }
            public void AddHeaders(Hashtable headers)
            {
                foreach (DictionaryEntry e in headers)
                    AddHeader((string)e.Key, (string)e.Value);
            }
            public void AddCookies(CookieCollection cookies)
            {
                _cookies.Add(cookies);
                rc();
            }
            public void AddCookie(string name, string value)
            {
                AddCookie(new Cookie(name, value));
            }
            public void AddCookie(Cookie cookie)
            {
                _cookies.Add(cookie);
                rc();
            }
            public void AddData(string name, string contentType, byte[] data, Hashtable parametrs)
            {
                Hashtable res = new Hashtable();
                if (contentType != "")
                    res.Add("content", contentType);
                res.Add("data", data);
                if (parametrs != null)
                    res.Add("parametrs", parametrs);
                else
                    res.Add("parametrs", new Hashtable());
                
                if (_data.ContainsKey(name))
                    _data[name] = res;
                else
                    _data.Add(name, res);

                if (_wwwFormUrlEncoded == 0)
                    _wwwFormUrlEncoded = 1;
                rc();
            }
            public void AddData(string name, string contentType, byte[] data, string fileName)
            {
                Hashtable p = new Hashtable();
                p.Add("filename", fileName);
                AddData(name, contentType, data, p);
            }
            public void AddData(string name, string value)
            {
                AddData(name, "", Encoding.ASCII.GetBytes(value), (Hashtable)null);
            }

            public void ResetData()
            {
                _data = new Hashtable();
                if (_wwwFormUrlEncoded == 1)
                    _wwwFormUrlEncoded = 0;
                rc();
            }
            public void ResetCookies()
            {
                _cookies = new CookieCollection();
                rc();
            }
            public void ResetHeaders()
            {
                _headers = new Hashtable();
                rc();
            }
            public void ResetParametrs()
            {
                _parametrs = new Hashtable();
                rc();
            }

            public bool XMLHttpRequest
            {
                get { return _headers.ContainsKey("X-Requested-With"); }
                set
                {
                    if (value && !XMLHttpRequest)
                    {
                        _headers.Add("X-Requested-With", "XMLHttpRequest");
                        rc();
                    }
                    else if (!value && XMLHttpRequest)
                    {
                        _headers.Remove("X-Requested-With");
                        rc();
                    }
                }
            }
            public bool WWWFormUrlEncoded
            {
                get { return _wwwFormUrlEncoded == 0; }
                set 
                {
                    byte ov = _wwwFormUrlEncoded;
                    if (value)
                        _wwwFormUrlEncoded = 2;
                    else if (_data.Count == 0)
                        _wwwFormUrlEncoded = 0;
                    else
                        _wwwFormUrlEncoded = 1;
                    if (ov != _wwwFormUrlEncoded)
                        rc();
                }
            }
            public Uri URL
            {
                get { return _adress; }
            }
            public RequestMethod Method
            {
                get { return _method; }
            }
            public Hashtable Headers
            {
                get { return _headers; }
            }
            public CookieCollection Cookies
            {
                get { return _cookies; }
            }
            public Hashtable Parametrs
            {
                get { return _parametrs; }
            }
            public Hashtable Data
            {
                get { return _data; }
            }

            public HttpAnswer GetAnswer(string userAgent, bool force)
            {
                if (_browser == null)
                {
                    _browser = new PirateHTTP();
                    _browser.AutoCookies = false;
                    _browser.Redirect = true;
                    _requestChanged = true;
                }
                if (_browser.UserAgent != userAgent)
                {
                    _browser.UserAgent = userAgent;
                    _requestChanged = true;
                }
                if (_requestChanged || force)
                    _answer = _browser.getAnswer(this);
                _requestChanged = false;
                return _answer;
            }
            public HttpAnswer GetAnswer(string userAgent)
            { return GetAnswer(userAgent, false); }
            private void reset(Uri adr, RequestMethod m)
            {
                _adress = adr;
                _method = m;
                _wwwFormUrlEncoded = 0;
                _browser = null;
                _answer = null;
                _requestChanged = false;
                ResetHeaders();
                ResetParametrs();
                ResetCookies();
                ResetData();
            }
            public HttpRequest(RequestMethod method, string url)
            {
                reset(new Uri(url), method);
            }
            public HttpRequest(RequestMethod method, string url, Hashtable parametrs)
            {
                reset(new Uri(url), method);
                AddParametrs(parametrs);
            }
            public HttpRequest(string url)
            {
                reset(new Uri(url), RequestMethod.GET);
            }
        }

        public partial class PirateHTTP
        {
            private string _userAgent;
            private bool _autoRedirect;
            private bool _keepAlive;
            private int _timeout;
            private bool _useDefaultHeaders;
            private bool _autoCookies;
            private Hashtable _headers;
            private CookieContainer _cookies;

            public string UserAgent
            {
                get { return _userAgent; }
                set { _userAgent = value; }
            }
            public bool Redirect
            {
                get { return _autoRedirect; }
                set { _autoRedirect = value; }
            }
            public bool AutoCookies
            {
                get { return _autoCookies; }
                set { _autoCookies = value; }
            }
            public bool KeepAlive
            {
                get { return _keepAlive; }
                set { _keepAlive = value; }
            }
            public int Timeout
            {
                get { return _timeout; }
                set { _timeout = value; }
            }
            public bool UseDefaultHeaders
            {
                get { return _useDefaultHeaders; }
                set { _useDefaultHeaders = value; }
            }
            public Hashtable DefaultHeaders
            {
                get { return _headers; }
                set { _headers = value; }
            }
            public CookieCollection AllCookies
            {
                get { return WebTools.parseCookies(_cookies); }
            }
            public CookieCollection Cookies(string domain)
            {
                return _cookies.GetCookies(new Uri(domain));
            }

            private string getBoundary(string UserAgent)
            {
                Random r = new Random();
                string res = "";
                switch (UserAgent)
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
            private HttpWebRequest assambleRequest(HttpRequest info)
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; }; // Securiy? Not here
                ServicePointManager.DefaultConnectionLimit = 1000;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(info.URL);
                request.ServicePoint.Expect100Continue = false; // strange shit should be disabled
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.ProtocolVersion = HttpVersion.Version11;

                Hashtable headers = new Hashtable(info.Headers);
                // Base Settings
                request.UserAgent = _userAgent;
                request.KeepAlive = _keepAlive;
                if (_timeout > 0)
                    request.Timeout = _timeout;
                request.Method = info.Method.ToString("F");

                // Situation Settings
                request.AllowAutoRedirect = _autoRedirect;
                if (headers.ContainsKey("Accept"))
                {
                    request.Accept = (string)headers["Accept"];
                    headers.Remove("Accept");
                }
                if (headers.ContainsKey("Referer"))
                {
                    request.Referer = (string)headers["Referer"];
                    headers.Remove("Referer");
                }
                if (headers.ContainsKey("Expect"))
                {
                    request.Expect = (string)headers["Expect"];
                    headers.Remove("Expect");
                }
                if (headers.ContainsKey("Host"))
                {
                    request.Host = (string)headers["Host"];
                    headers.Remove("Host");
                }

                // Loading Headers
                foreach (DictionaryEntry h in headers)
                    request.Headers.Add((string)h.Key, (string)h.Value);
                if (_useDefaultHeaders)
                {
                    foreach (DictionaryEntry h in _headers)
                    {
                        // Watch if there is custom header already
                        if (!headers.ContainsKey(h.Key))
                            request.Headers.Add((string)h.Key, (string)h.Value);
                    }
                }

                // Loading Cookies
                if (_autoCookies)
                    request.CookieContainer = _cookies;
                else
                    request.CookieContainer = new CookieContainer();
                if (info.Cookies.Count > 0)
                {
                    foreach (Cookie c in info.Cookies)
                        request.CookieContainer.Add(c);
                }

                // Write parametr & data... multipart data
                if (info.Method == RequestMethod.POST)
                {
                    using (Stream requestStream = request.GetRequestStream())
                    {
                        if (info.WWWFormUrlEncoded)
                        {
                            request.ContentType = "application/x-www-form-urlencoded";
                            string res = "";
                            foreach (DictionaryEntry pair in info.Parametrs)
                                res += (string)pair.Key + '=' + (string)pair.Value + '&';
                            res = res.Remove(res.Length - 1, 1);
                            requestStream.Write(Encoding.ASCII.GetBytes(res), 0, Encoding.ASCII.GetByteCount(res));
                        }
                        else
                        {
                            string tmp;
                            string boundary = getBoundary(request.UserAgent);
                            request.ContentType = "multipart/form-data; boundary=" + boundary;
                            if (info.Parametrs.Count > 0)
                            {
                                tmp = "";
                                foreach (DictionaryEntry p in info.Parametrs)
                                    tmp += string.Format("--{0}\\r\\nContent-Disposition: form-data; name=\"{1}\"\\r\\n\\r\\n{1}\\r\\n", boundary, p.Key, p.Value);
                                requestStream.Write(Encoding.ASCII.GetBytes(tmp), 0, Encoding.ASCII.GetByteCount(tmp));
                            }
                            if (info.Data.Count > 0)
                            {
                                foreach (DictionaryEntry d in info.Data)
                                {
                                    tmp = string.Format("name=\"{0}\";", (string)d.Key);
                                    foreach (DictionaryEntry p in (Hashtable)((Hashtable)d.Value)["parametrs"])
                                        tmp += string.Format(" {0}=\"{1}\";", p.Key, p.Value);
                                    tmp = tmp.Remove(tmp.Length - 1, 1);
                                    tmp = string.Format("--{0}\\r\\nContent-Disposition: form-data; {1}\\r\\n", boundary, tmp);
                                    if ((string)((Hashtable)d.Value)["content"] != "")
                                        tmp += "Content-Type: " + ((Hashtable)d.Value)["content"] + "\\r\\n";
                                    tmp += "\\r\\n";
                                    requestStream.Write(Encoding.ASCII.GetBytes(tmp), 0, Encoding.ASCII.GetByteCount(tmp));
                                    requestStream.Write((byte[])((Hashtable)d.Value)["data"], 0, ((byte[])((Hashtable)d.Value)["data"]).Length);
                                }
                            }
                            // Closing boundary
                            tmp = "--" + boundary + "--\\r\\n";
                            requestStream.Write(Encoding.ASCII.GetBytes(tmp), 0, Encoding.ASCII.GetByteCount(tmp));
                        }
                    }
                }
                return request;
            }
            public HttpWebResponse getResponse(HttpRequest info)
            {
                HttpWebRequest req = null;
                HttpWebResponse res = null;
                int attempts = 0;
                bool success = false;
                while (!success && attempts < 3)
                {
                    attempts++;
                    try
                    {
                        req = assambleRequest(info);
                        res = (HttpWebResponse)req.GetResponse();
                        success = true;
                    }
                    catch (WebException WebEx)
                    {
                        if (WebEx.Status == WebExceptionStatus.ConnectFailure)
                            attempts = 3;
                        res = (HttpWebResponse)WebEx.Response;
                    }
                }
                return res;
            }
            public HttpAnswer getAnswer(HttpRequest info)
            {
                return new HttpAnswer(getResponse(info));
            }

            public PirateHTTP()
            {
                UserAgent = Browser.IE_W7_x64_v11;
                _autoRedirect = true;
                _keepAlive = true;
                _autoCookies = false;
                _timeout = 0;
                _useDefaultHeaders = false;

                _cookies = new CookieContainer();
                _headers = new Hashtable();
                _headers.Add("Accept-Encoding", "gzip, deflate");
                _headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
                _headers.Add("Accept", "*/*");
            }
        }

        public static class WebTools
        {
            public const string srcPattern = "src\\s*=\\s*(?:[\"'](?<1>[^\"']*)[\"']|(?<1>\\S+))";

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
