using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PfmdUI.CommUtil
{
    public class XMLNode : Hashtable
    {
        public XMLNodeList GetNodeList(string path)
        {
            return GetObject(path) as XMLNodeList;
        }

        public XMLNode GetNode(string path)
        {
            return GetObject(path) as XMLNode;
        }

        public string GetValue(string path)
        {
            return GetObject(path) as string;
        }

        private object GetObject(string path)
        {
            string[] bits = path.Split('>');
            XMLNode currentNode = this;
            XMLNodeList currentNodeList = null;
            bool listMode = false;
            object ob;

            for (int i = 0; i < bits.Length; i++)
            {
                if (listMode)
                {
                    try
                    {
                        currentNode = (XMLNode)currentNodeList[int.Parse(bits[i])];
                        ob = currentNode;
                        listMode = false;
                    }
                    catch(Exception e)
                    {
                        return bits[i-1] + "#标签越界";
                    }
                }
                else
                {
                    ob = currentNode[bits[i]];
                    if (ob == null)
                    {
                        if ((i + 1) == bits.Length)
                        {
                            return bits[i] + "#属性不存在";
                        }
                        else
                        {
                            return bits[i] + "#标签不存在";
                        }
                    }

                    if (ob is ArrayList)
                    {
                        currentNodeList = (XMLNodeList)(ob as ArrayList);
                        listMode = true;
                    }
                    else
                    {
                        // reached a leaf node/attribute  
                        if (i != (bits.Length - 1))
                        {
                            // unexpected leaf node  
                            string actualPath = "";
                            for (int j = 0; j <= i; j++)
                            {
                                actualPath = actualPath + ">" + bits[j];
                            }

                            //Debug.Log("xml path search truncated. Wanted: " + path + " got: " + actualPath);  
                        }

                        return ob;
                    }
                }
            }

            if (listMode)
                return currentNodeList;
            else
                return currentNode;
        }
    }  
}