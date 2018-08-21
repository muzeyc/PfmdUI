using System.Collections.Generic;

namespace PfmdUI.CommUtil
{
    public class MenuModel
    {
        public MenuModel()
        {
            this.subList = new List<MenuModel>();
        }

        public string menuTitle { get; set; }
        public string iconName { get; set; }
        public string url { get; set; }

        public List<MenuModel> subList { get; set; }


        public List<MenuModel> ConfigParse(string s)
        {
            XMLParser xp = new XMLParser();
            XMLNode xn = xp.Parse(s);
            var list = new List<MenuModel>();

            int no = 0;
            while (no < 300)
            {
                var menuParent = new MenuModel();
                menuParent.menuTitle = xn.GetValue(string.Format("Menu>0>ParentMenu>{0}>@menuTitle", no));
                menuParent.iconName = xn.GetValue(string.Format("Menu>0>ParentMenu>{0}>@iconName", no));
                if (menuParent.menuTitle.Contains("标签越界") || menuParent.menuTitle.Contains("标签不存在")
                    || menuParent.iconName.Contains("标签越界") || menuParent.iconName.Contains("标签不存在"))
                {
                    break;
                }

                int subNo = 0;
                while (subNo < 300)
                {
                    var menuChild = new MenuModel();
                    menuChild.menuTitle = xn.GetValue(string.Format("Menu>0>ParentMenu>{0}>childMenu>{1}>@menuTitle", no, subNo));
                    menuChild.iconName = xn.GetValue(string.Format("Menu>0>ParentMenu>{0}>childMenu>{1}>@iconName", no, subNo));
                    menuChild.url = xn.GetValue(string.Format("Menu>0>ParentMenu>{0}>childMenu>{1}>@url", no, subNo));

                    if (menuChild.menuTitle.Contains("标签越界") || menuChild.menuTitle.Contains("标签不存在")
                    || menuChild.iconName.Contains("标签越界") || menuChild.iconName.Contains("标签不存在")
                    || menuChild.url.Contains("标签越界") || menuChild.url.Contains("标签不存在"))
                    {
                        break;
                    }

                    menuParent.subList.Add(menuChild);
                    subNo++;
                }

                list.Add(menuParent);
                no++;
            }

            return list;
        }
    }
}
