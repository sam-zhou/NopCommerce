using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using LitJson;
using Nop.Core;
using Nop.Core.Domain.Logging;

namespace Nop.Plugin.Payments.WeiXin.Models
{
    public class WxPayData
    {
        
        public WxPayData()
        {
        }

        //采用排序的Dictionary的好处是方便对数据包进行签名，不用再签名之前再做一次排序
        private readonly SortedDictionary<string, object> _mValues = new SortedDictionary<string, object>();

        public SortedDictionary<string, object> Values
        {
            get { return _mValues; }
        }

        /**
        * 设置某个字段的值
        * @param key 字段名
         * @param value 字段值
        */
        public void SetValue(string key, object value)
        {
            _mValues[key] = value;
        }

        /**
        * 根据字段名获取某个字段的值
        * @param key 字段名
         * @return key对应的字段值
        */
        public object GetValue(string key)
        {
            object o;
            _mValues.TryGetValue(key, out o);
            return o;
        }

        /**
         * 判断某个字段是否已设置
         * @param key 字段名
         * @return 若字段key已被设置，则返回true，否则返回false
         */
        public bool IsSet(string key)
        {
            object o;
            _mValues.TryGetValue(key, out o);
            if (null != o)
                return true;
            return false;
        }

        /**
        * @将Dictionary转成xml
        * @return 经转换得到的xml串
        * @throws NopException
        **/
        public string ToXml()
        {
            //数据为空时不能转化为xml格式
            if (0 == _mValues.Count)
            {
                throw new NopException("WxPayData数据为空!");
            }

            string xml = "<xml>";
            foreach (KeyValuePair<string, object> pair in _mValues)
            {
                //字段值不能为null，会影响后续流程
                if (pair.Value == null)
                {
                    throw new NopException("WxPayData内部含有值为null的字段!");
                }

                if (pair.Value is int)
                {
                    xml += "<" + pair.Key + ">" + pair.Value + "</" + pair.Key + ">";
                }
                else if (pair.Value is string)
                {
                    xml += "<" + pair.Key + ">" + "<![CDATA[" + pair.Value + "]]></" + pair.Key + ">";
                }
                else//除了string和int类型不能含有其他数据类型
                {
                    throw new NopException("WxPayData字段数据类型错误!");
                }
            }
            xml += "</xml>";
            return xml;
        }

        /**
        * @将xml转为WxPayData对象并返回对象内部的数据
        * @param string 待转换的xml串
        * @return 经转换得到的Dictionary
        * @throws NopException
        */
        public SortedDictionary<string, object> FromXml(string xml, string appSecret)
        {
            if (string.IsNullOrEmpty(xml))
            {
                throw new NopException("将空的xml串转换为WxPayData不合法!");
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            var xmlNode = xmlDoc.FirstChild;//获取到根节点<xml>
            var nodes = xmlNode.ChildNodes;
            foreach (XmlNode xn in nodes)
            {
                var xe = (XmlElement)xn;
                _mValues[xe.Name] = xe.InnerText;//获取xml的键值对到WxPayData内部的数据中
            }

            try
            {
                //2015-06-29 错误是没有签名
                if ((string) _mValues["return_code"] != "SUCCESS")
                {
                    return _mValues;
                }
                CheckSign(appSecret);//验证签名,不通过会抛异常
            }
            catch (NopException ex)
            {
                throw new NopException(ex.Message);
            }

            return _mValues;
        }

        /**
        * @Dictionary格式转化成url参数格式
        * @ return url格式串, 该串不包含sign字段值
        */
        public string ToUrl()
        {
            string buff = "";
            foreach (KeyValuePair<string, object> pair in _mValues)
            {
                if (pair.Value == null)
                {
                    throw new NopException("WxPayData内部含有值为null的字段!");
                }

                if (pair.Key != "sign" && pair.Value.ToString() != "")
                {
                    buff += pair.Key + "=" + pair.Value + "&";
                }
            }
            buff = buff.Trim('&');
            return buff;
        }


        /**
        * @Dictionary格式化成Json
         * @return json串数据
        */
        public string ToJson()
        {
            string jsonStr = JsonMapper.ToJson(_mValues);
            return jsonStr;
        }

        /**
        * @values格式化成能在Web页面上显示的结果（因为web页面上不能直接输出xml格式的字符串）
        */
        public string ToPrintStr()
        {
            string str = "";
            foreach (KeyValuePair<string, object> pair in _mValues)
            {
                if (pair.Value == null)
                {
                    throw new NopException("WxPayData内部含有值为null的字段!");
                }

                str += string.Format("{0}={1}<br>", pair.Key, pair.Value);
            }
            return str;
        }

        /**
        * @生成签名，详见签名生成算法
        * @return 签名, sign字段不参加签名
        */
        public string MakeSign(string appSecret)
        {
            //转url格式
            string str = ToUrl();
            //在string后加入API KEY
            str += "&key=" + appSecret;
            //MD5加密
            var md5 = MD5.Create();
            var bs = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            var sb = new StringBuilder();
            foreach (byte b in bs)
            {
                sb.Append(b.ToString("x2"));
            }
            //所有字符转为大写
            return sb.ToString().ToUpper();
        }

        /**
        * 
        * 检测签名是否正确
        * 正确返回true，错误抛异常
        */
        public bool CheckSign(string appSecret)
        {
            //如果没有设置签名，则跳过检测
            if (!IsSet("sign"))
            {
                throw new NopException("WxPayData签名存在但不合法!");
            }
            //如果设置了签名但是签名为空，则抛异常
            else if (GetValue("sign") == null || GetValue("sign").ToString() == "")
            {
                throw new NopException("WxPayData签名存在但不合法!");
            }

            //获取接收到的签名
            string returnSign = GetValue("sign").ToString();

            //在本地计算新的签名
            string calSign = MakeSign(appSecret);

            if (calSign == returnSign)
            {
                return true;
            }

            throw new NopException("WxPayData签名验证错误!");
        }

        /**
        * @获取Dictionary
        */
        public SortedDictionary<string, object> GetValues()
        {
            return _mValues;
        }
    }
}
