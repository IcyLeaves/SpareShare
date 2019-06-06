using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace SpareShare
{
    public class Ori_word
    {
        public string word { get; set; }
        public int offset { get; set; }
        public int length { get; set; }
    }
    public class Syn_wordsItem
    {
        public string word { get; set; }
        public double weight { get; set; }
    }
    public class Syn_tokensItem
    {
        public Ori_word ori_word { get; set; }
        public List<Syn_wordsItem> syn_words { get; set; }
    }
    public class Data
    {
        public string text { get; set; }
        public List<Syn_tokensItem> syn_tokens { get; set; }
    }
    public class Root
    {
        public int ret { get; set; }
        public string msg { get; set; }
        public Data data { get; set; }
    }

    public class QueryBody
    {
        public string app_id { get; set; }
        public string time_stamp { get; set; }
        public string nonce_str { get; set; }
        public string text { get; set; }
        public string sign { get; set; }
        public void setSign()
        {
            //1.
            Dictionary<string, string> tmp = new Dictionary<string, string>();
            tmp.Add("app_id", app_id);
            tmp.Add("time_stamp", time_stamp);
            tmp.Add("nonce_str", nonce_str);
            tmp.Add("text", text);
            Dictionary<string, string> sorted = tmp.OrderBy(x => x.Key).ToDictionary(p => p.Key, o => o.Value);
            //2.
            string T = "";
            foreach (var r in sorted)
            {
                string k = r.Key;
                string v = r.Value;
                T += k;
                T += "=";
                if (k == "text")
                {
                    T += HttpUtility.UrlEncode(v, Encoding.GetEncoding("GBK")).ToUpper();
                }
                else
                {
                    T += HttpUtility.UrlEncode(v).ToUpper();
                }
                T += "&";
            }
            //3.
            T += "app_key=";
            T += "bn7jDKqkpEqmnPuE";
            //4.
            MD5 md5 = MD5.Create();
            sign = GetMd5Hash(md5, T);
            sign = sign.ToUpper();
        }
        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
    static class TencentAI
    {
        private const string host = "https://api.ai.qq.com";
        private const string path = "/fcgi-bin/nlp/nlp_wordsyn";
        private const string method = "GET";
        private const string appid = "2115315070";
        private const string appkey = "bn7jDKqkpEqmnPuE";

        private static string getTimeStamp()
        {
            DateTime dateStart = new DateTime(1970, 1, 1, 8, 0, 0);
            int timeStamp = Convert.ToInt32((DateTime.Now - dateStart).TotalSeconds);
            return timeStamp.ToString();
        }


        public static List<string> synWords(string nplJson)
        {
            var JsonDataForClass =JsonConvert.DeserializeAnonymousType(nplJson, new Root());
            var res = new List<string>();
            foreach (var item in JsonDataForClass.data.syn_tokens)
            {
                foreach(var w in item.syn_words)
                {
                    res.Add(w.word);
                }
            }
            return res;
        }
        public static List<string> oriWords(string nplJson)
        {
            var JsonDataForClass = JsonConvert.DeserializeAnonymousType(nplJson, new Root());
            var res = new List<string>();
            foreach (var item in JsonDataForClass.data.syn_tokens)
            {
                res.Add(item.ori_word.word);
            }
            return res;
        }
        public static string tencentAI(string text)
        {
            Random r = new Random();

            QueryBody b = new QueryBody();
            b.app_id = appid;
            b.time_stamp = getTimeStamp();
            b.nonce_str = r.Next(1, 32767).ToString();
            b.text = text;
            b.setSign();
            b.text = HttpUtility.UrlEncode(b.text,Encoding.GetEncoding("GBK")).ToUpper();
            
            //string querys = "";
            //string bodys = JsonConvert.SerializeObject(b);
            #region 添加Post 参数
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("app_id", appid);
            dic.Add("time_stamp", b.time_stamp);
            dic.Add("nonce_str", b.nonce_str);
            dic.Add("text", b.text);
            dic.Add("sign", b.sign);
            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach (var item in dic)
            {
                if (i > 0)
                    builder.Append("&");
                builder.AppendFormat("{0}={1}", item.Key, item.Value);
                i++;
            }
            #endregion
            string querys = builder.ToString();
            string bodys = "";
            //string bodys = "{\"app_id\":\"2115315070\",\"nonce_str\":\"3786\",\"sign\":\"8966ADE82F4CD85B22ABAFDDB9DBD4BA\",\"text\":\"%BD%F1%CC%EC%B5%C4%CC%EC%C6%F8%D4%F5%C3%B4%D1%F9\",\"time_stamp\":\"1555575868\"}";
            string url = host + path;
            HttpWebRequest httpRequest = null;
            HttpWebResponse httpResponse = null;

            if (0 < querys.Length)
            {
                url = url + "?" + querys;
            }

            if (host.Contains("https://"))
            {
                //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                httpRequest = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
            }
            else
            {
                httpRequest = (HttpWebRequest)WebRequest.Create(url);
            }
            httpRequest.Method = method;
            //httpRequest.Headers.Add("Authorization", "APPCODE " + appcode);
            //根据API的要求，定义相对应的Content-Type
            httpRequest.ContentType = "application/json;charset=gbk";
            
            if (0 < bodys.Length)
            {
                byte[] data = Encoding.GetEncoding("GBK").GetBytes(bodys);
                byte[] data1 = Encoding.UTF8.GetBytes(bodys);
                httpRequest.ContentLength = data.Length;
                string a = Encoding.UTF8.GetString(data);
                string bbb = Encoding.GetEncoding("GBK").GetString(data);
                using (Stream stream = httpRequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            try
            {
                httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            }
            catch (WebException ex)
            {
                httpResponse = (HttpWebResponse)ex.Response;
            }

            Stream st = httpResponse.GetResponseStream();
            StreamReader reader = new StreamReader(st, Encoding.GetEncoding("GBK"));
            return reader.ReadToEnd();
        }
    }
}