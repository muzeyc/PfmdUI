using System.Collections.Generic;

namespace PfmdUI.CommUtil
{
    public class ConfigModel
    {
        //共享目录DLL地址
        public string sourcePath { get; set; }
        //client DLL地址
        public string clientFileName { get; set; }

        public string clientDirectory { get; set; }

        public List<ConfigModel> ConfigParse(string s)
        {
            XMLParser xp = new XMLParser();
            XMLNode xn = xp.Parse(s);
            List<ConfigModel> cms = new List<ConfigModel>();
           
            int no = 0;
            while (no < 300)
            {
                ConfigModel cm = new ConfigModel();
                cm.sourcePath = xn.GetValue(string.Format("Config>0>ConfigModel>{0}>@SourcePath", no));
                if (cm.sourcePath.Contains("标签越界") || cm.sourcePath.Contains("标签不存在"))
                {
                    break;
                }
                cm.clientFileName = xn.GetValue(string.Format("Config>0>ConfigModel>{0}>@ClientFileName", no));
                cm.clientDirectory = xn.GetValue(string.Format("Config>0>ConfigModel>{0}>@ClientDirectory", no));
                cms.Add(cm);
                no++;
            }
            
            return cms;
        }
    }

    
}
