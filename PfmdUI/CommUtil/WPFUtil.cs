using System.Collections.Generic;
using System.Reflection;
using System.Web.Script.Serialization;

namespace PfmdUI.CommUtil
{
    public class WPFUtil
    {
        /// <summary>
        /// 生成ControllerUI对象
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="menus"></param>
        /// <param name="urlHead"></param>
        /// <param name="appMode">1：发布模式，2：开发模式</param>
        /// <returns></returns>
        public object UserControlFactory(object obj, List<MenuModel> menus, string urlHead, string appMode)
        {
            object[] parameters = new object[2];
            parameters[0] = Serializer(menus);
            parameters[1] = urlHead;

            string type = "ControllerUI." + obj.GetType().ToString().Split('.')[2];
            // 创建类的实例 
            if ("1".Equals(appMode))
            {
                return Assembly.LoadFile("C://PFMDView/ControllerUI.dll").CreateInstance(type, true, System.Reflection.BindingFlags.Default, null, parameters, null, null);
            }
            else
            {
                return Assembly.Load("ControllerUI").CreateInstance(type, true, System.Reflection.BindingFlags.Default, null, parameters, null, null);
            }
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        private string Serializer(object obj)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            return javaScriptSerializer.Serialize(obj);
        }
    }
}
