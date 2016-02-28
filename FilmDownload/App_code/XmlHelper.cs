using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.IO;
using System.Data;
using System.Xml.Serialization;

namespace FilmDownload
{
    class XmlHelper
    {
        public string XmlPath { get; set; }

        public XmlDocument XmlDoc { get; set; }
        public XmlNode XmlNode { get; set; }
        //public XmlElement XmlEle { get; set; }

        public XmlHelper(string xmlPath)
        {
            XmlPath = xmlPath;
            try
            {
                if (File.Exists(XmlPath))
                {
                    XmlDoc = new XmlDocument();
                    XmlDoc.Load(XmlPath);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CreateXmlDoc(string rootNodeName)
        {
            XmlDoc = new XmlDocument();
            XmlDeclaration xmlDecl;
            xmlDecl = XmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            XmlNode root = XmlDoc.CreateElement(rootNodeName);
            XmlDoc.AppendChild(xmlDecl);
            XmlDoc.AppendChild(root);
            Save();
            XmlDoc.Load(XmlPath);
        }

        public DataTable GetData(string xmlPathNode)
        {
            DataSet ds = new DataSet();
            StringReader read = new StringReader(XmlDoc.SelectSingleNode(xmlPathNode).OuterXml);
            ds.ReadXml(read);
            return ds.Tables[0];
        }

        public XmlNode GetNode(string xmlPathNode)
        {
            return XmlDoc.SelectSingleNode(xmlPathNode);
        }

        public string GetNodeText(string xmlPathNode)
        {
            return XmlDoc.SelectSingleNode(xmlPathNode).InnerText;
        }

        public XmlAttribute GetAttribute(string xmlPathNode, string attributeName)
        {
            XmlNode xmlNode = XmlDoc.SelectSingleNode(xmlPathNode);

            if (xmlNode != null)
            {
                return xmlNode.Attributes[attributeName];
            }
            else
            {
                return null;
            }
        }

        //Node
        /// <summary>
        /// 插入一个节点，带一属性
        /// </summary>
        /// <param name="xmlPathNode"></param>
        /// <param name="nodeName"></param>
        /// <param name="attributeName"></param>
        /// <param name="attributeValue"></param>
        /// <returns></returns>
        public bool AddNode(string xmlPathNode, string nodeName,string attributeName, string attributeValue)
        {
            bool isSuccess = false;
            try
            {
                XmlNode mainNode = XmlDoc.SelectSingleNode(xmlPathNode);
                if (mainNode != null)
                {
                    if (mainNode.SelectNodes(nodeName + "[@" + attributeName + "='"+ attributeValue +"']").Count == 0)
                    {
                        XmlElement xmlEle = XmlDoc.CreateElement(nodeName);
                        xmlEle.SetAttribute(attributeName, attributeValue);
                        mainNode.AppendChild(xmlEle);
                    }
                    isSuccess = true;
                }
                else
                {
                    isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return isSuccess;
        }

        public bool AddNode(string xmlPathNode, string nodeName, string innerText)
        {
            bool isSuccess = false;
            try
            {
                XmlNode mainNode = XmlDoc.SelectSingleNode(xmlPathNode);
                if (mainNode != null)
                {
                    if (mainNode.SelectNodes(nodeName).Count == 0)
                    {
                        XmlElement xmlEle = XmlDoc.CreateElement(nodeName);
                        xmlEle.InnerText = innerText;
                        mainNode.AppendChild(xmlEle);
                    }
                    isSuccess = true;
                }
                else
                {
                    isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return isSuccess;
        }

        public void UpdateNode(string xmlPathNode, string innerText)
        {
            try
            {
                XmlNodeList nodeList = XmlDoc.SelectNodes(xmlPathNode);
                foreach (XmlNode xNode in nodeList)
                {
                    xNode.InnerText = innerText;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Attribute
        public void AddAttribute(string xmlPathNode, string attributeName, string value)
        {
            XmlNode xmlNode = XmlDoc.SelectSingleNode(xmlPathNode);

            if (xmlNode != null)
            {
                XmlAttribute xmlAttribute = XmlDoc.CreateAttribute(attributeName);
                xmlAttribute.Value = value;
                xmlNode.Attributes.Append(xmlAttribute);
            }
        }

        public void AddAttribute(string attributeName, string value)
        {
            if (XmlNode != null)
            {
                XmlAttribute xmlAttribute = XmlDoc.CreateAttribute(attributeName);
                xmlAttribute.Value = value;
                XmlNode.Attributes.Append(xmlAttribute);
            }
        }

        public void UpdateAttribute(string xmlPathNode, string attributeName, string value)
        {
            XmlAttribute xmlAttribute = GetAttribute(xmlPathNode, attributeName);
            if (xmlAttribute != null)
            {
                xmlAttribute.Value = value;
            }
        }

        public void Save()
        {
            XmlDoc.Save(XmlPath);
        }

        #region 反序列化
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="xml">XML字符串</param>
        /// <returns></returns>
        public static object Deserialize(Type type, string xml)
        {
            try
            {
                using (StringReader sr = new StringReader(xml))
                {
                    XmlSerializer xmldes = new XmlSerializer(type);
                    return xmldes.Deserialize(sr);
                }
            }
            catch (Exception e)
            {

                return null;
            }
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="type"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static object Deserialize(Type type, Stream stream)
        {
            XmlSerializer xmldes = new XmlSerializer(type);
            return xmldes.Deserialize(stream);
        }
        #endregion

        #region 序列化
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static string Serializer(Type type, object obj)
        {
            MemoryStream Stream = new MemoryStream();
            XmlSerializer xml = new XmlSerializer(type);
            try
            {
                //序列化对象
                xml.Serialize(Stream, obj);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            Stream.Position = 0;
            StreamReader sr = new StreamReader(Stream);
            string str = sr.ReadToEnd();

            sr.Dispose();
            Stream.Dispose();

            return str;
        }

        #endregion
    }
}
