using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// 此类是关于sqlmapapi的操作
/// </summary>
namespace SqlmapHelper
{
    public class SqlmapSession : IDisposable
    {
        private string Sev_host = string.Empty;
        private int _Port = 8775;
        public SqlmapSession(string host, int port = 8775) { Sev_host = host; _Port = port; }

        public void Dispose()
        {
            Sev_host = null;
        }

        public async Task<string> VulnUrlGet(string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://" + Sev_host + ":" + _Port + url);//初始化url访问实例
            req.Method = "GET";
 
            string reqres = string.Empty;
            using (StreamReader rdr = new StreamReader(req.GetResponse().GetResponseStream()))
            {
                 reqres = await rdr.ReadToEndAsync();//获取respose全部数据
            }
            return reqres;
        }
        public async Task<string> VulnUrlPost(string url, string data)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data);//用字节数组保存post的data值
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://" + Sev_host + ":" + _Port + url);
            req.Method = "POST";
            req.ContentType = "application/json";
            req.ContentLength = buffer.Length;
            using (Stream stream=req.GetRequestStream())
            {
                stream.Write(buffer,0,buffer.Length);
            }
            string reqres = string.Empty;
            using (StreamReader rdr=new StreamReader(req.GetResponse().GetResponseStream()))
            {
                reqres = await rdr.ReadToEndAsync();
            }
            return reqres;
        }
        public string ChangeUserAgent()
        {
            string[] ua = new string[] {
                "Mozilla/5.0 (Windows NT 6.1; rv,2.0.1) Gecko/20100101 Firefox/4.0.1",
                "Opera/9.80 (Windows NT 6.1; U; en) Presto/2.8.131 Version/11.11",
                "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.25 Safari/537.36 Core/1.70.3704.400 QQBrowser/10.4.3587.400",
                "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; 360SE)",
                "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 UBrowser/6.2.4094.1 Safari/537.36",
                "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.119 Safari/537.36",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:87.0) Gecko/20100101 Firefox/87.0",
                "Mozilla/5.0 (Macintosh; U; Intel Mac OS X 10_6_8; en-us) AppleWebKit/534.50 (KHTML, like Gecko) Version/5.1 Safari/534.50",
                "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-us) AppleWebKit/534.50 (KHTML, like Gecko) Version/5.1 Safari/534.50",
                "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0;",
            };
            Random rd = new Random();
            int index = rd.Next(0, ua.Length);
            return ua[index];
        }
    }
}
