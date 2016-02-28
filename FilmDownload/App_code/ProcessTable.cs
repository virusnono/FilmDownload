using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Data;

namespace FilmDownload
{
    class ProcessTable
    {
        public string rootNodeName = "process";
        public string nodeName = "data";
        public string nodeID = "itemID";
        public string subNodeName1 = "itemName";
        public string subNodeName2 = "day";
        public string title = "日期";
        public string title2 = "日";

        public string[][] values = new string[][]{
            //new string[]{"title","日期"},
            new string[]{"planNumber","计划台数"},
            new string[]{"productNumber","生产台数"},
            new string[]{"NGNumber","不良台数"},
            new string[]{"sum","累计台数"}
        };

        public XmlHelper xml { get; set; }

        public ProcessTable(string xmlPath)
        {
            xml = new XmlHelper(xmlPath);
            if (!File.Exists(xmlPath))
            {
                CreateXmlFile();
            }

        }

        public void CreateXmlFile()
        {
            xml.CreateXmlDoc(rootNodeName);

            foreach (string[] value in values)
            {
                xml.AddNode(rootNodeName, nodeName, nodeID, value[0]);
                string xmlPathNode = rootNodeName + "/" + nodeName + "[@" + nodeID + "='" + value[0] + "']";
                xml.AddNode(xmlPathNode, subNodeName1, value[1]);
                for (int i = 1; i < 32; i++)
                {
                    xml.AddNode(xmlPathNode, subNodeName2 + i, "");
                    //xml.AddNode(xmlPathNode, "day" + i, value[0] == "title" ? i + "日" : "");
                }
            }

            xml.Save();
        }

        public DataTable GetTable(int beginDay, int length)
        {
            DataTable dtProcess = xml.GetData(rootNodeName);
            DataTable result = new DataTable("result");

            result.Columns.Add(title);
            for (int i = beginDay; i < beginDay + length; i++)
            {
                result.Columns.Add(i + title2);
            }

            for (int i = 0; i < dtProcess.Rows.Count; i++)
            {
                DataRow dr = result.NewRow();
                dr[title] = dtProcess.Rows[i][subNodeName1];
                for (int j = beginDay; j < beginDay + length; j++)
                { 
                    dr[j + title2] = dtProcess.Rows[i][subNodeName2 + j];
                }
                result.Rows.Add(dr);
            }

            return result;
        }

        public void UpdateProcess(string itemID, string dayID, string text)
        { 
            string xmlPathNode = rootNodeName + "/" + nodeName + "[@" + nodeID + "='" + itemID + "']/day" + dayID;
            xml.UpdateNode(xmlPathNode, text);
            if (itemID == values[1][0])
            {
                int iDayID = int.Parse(dayID);
                string xmlPathNodeSum = rootNodeName + "/" + nodeName + "[@" + nodeID + "='" + values[3][0] + "']/day";
                string xmlPathNodeProduct = rootNodeName + "/" + nodeName + "[@" + nodeID + "='" + values[1][0] + "']/day";
                
                int thisSum = 0;
                if(iDayID != 1)
                {
                    string sum = xml.GetNodeText(xmlPathNodeSum + (iDayID - 1).ToString());
                    if (!string.IsNullOrEmpty(sum))
                    {
                        thisSum = int.Parse(sum);
                    }
                    else
                    {
                        thisSum = -1;
                    }

                }

                for (int i = iDayID; i <= 31; i++)
                {
                    string productNumber = xml.GetNodeText(xmlPathNodeProduct + i);
                    if (thisSum == -1)
                    { 
                        xml.UpdateNode(xmlPathNodeSum + i, "");
                        break;
                    }

                    if (!string.IsNullOrEmpty(productNumber))
                    {
                        int iProductNumber = int.Parse(productNumber);
                        thisSum += iProductNumber;
                        xml.UpdateNode(xmlPathNodeSum + i, thisSum.ToString());
                    }
                    else
                    {
                        thisSum = -1;
                        xml.UpdateNode(xmlPathNodeSum + i, "");
                    }
                }


            }
            xml.Save();
        }
    }
}
