using PfmdUI.CommUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace PfmdUI.View
{
    /// <summary>
    /// MainForm.xaml 的交互逻辑
    /// </summary>
    public partial class MainForm : UserControl
    {
        private string SHARE_PATH;
        private string SERVER_USERNAME;
        private string SERVER_PASSWORD;
        private string URL_HEAD;
        private string APP_MODE;
        private int ERR_TIMES = 0;
        private string LOCAL_BASE = @"C:\PFMDView";

        public MainForm()
        {
            InitializeComponent();
            //判断核心路径是否存在
            if (!Directory.Exists(LOCAL_BASE))//判断是否存在
            {
                Directory.CreateDirectory(LOCAL_BASE);//创建新路径
            }
        }

        private void Config()
        {
            //获取模式
            string keyPath = LOCAL_BASE + @"\DevelopKey";
            if (File.Exists(keyPath))
            {
                this.APP_MODE = "2";
            }
            else
            {
                this.APP_MODE = "1";
            }

            string filePath = LOCAL_BASE + @"\config.xml";
            //发布模式才需要读配置
            if (this.APP_MODE.Equals("1"))
            {
                if (File.Exists(filePath))
                {
                    // 读取配置文件
                    string content = File.ReadAllText(filePath, Encoding.UTF8);
                    XMLParser xp = new XMLParser();
                    XMLNode xn = xp.Parse(content);
                    this.SHARE_PATH = @"\\" + xn.GetValue("Config>0>SERVER_IP>0>@value") + @"\upload";
                    this.SERVER_USERNAME = xn.GetValue("Config>0>SERVER_USERNAME>0>@value");
                    this.SERVER_PASSWORD = xn.GetValue("Config>0>SERVER_PASSWORD>0>@value");
                }
                else
                {
                    MessageBox.Show("文件：" + filePath + "不存在！");
                }
            }
            else
            {
                this.SHARE_PATH = LOCAL_BASE;
            }
        }

        private void PublishInit()
        {
            try
            {
                var status = connectState(SHARE_PATH, SERVER_USERNAME, SERVER_PASSWORD);
                if (status)
                {
                    // 读取配置文件
                    string content = File.ReadAllText(SHARE_PATH + @"\Pfmd\config.xml", Encoding.UTF8);
                    XMLParser xp = new XMLParser();
                    XMLNode xn = xp.Parse(content);
                    this.URL_HEAD = xn.GetValue("Config>0>@UrlHead");

                    var config = new ConfigModel();
                    var list = config.ConfigParse(content);
                    var dic = new Dictionary<string, string>();

                    foreach (var model in list)
                    {
                        if (!dic.ContainsKey(model.clientDirectory))
                        {
                            dic.Add(model.clientDirectory, "");
                            if (!Directory.Exists(model.clientDirectory))
                            {
                                //
                                DirectoryInfo dir_info = new DirectoryInfo(model.clientDirectory);
                                DirectorySecurity dir_security = new DirectorySecurity();
                                dir_security.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Allow));
                                //dir_info.SetAccessControl(dir_security);
                                Directory.CreateDirectory(model.clientDirectory, dir_security);
                            }
                        }

                        string clientPath = model.clientDirectory + model.clientFileName;

                        if (File.Exists(clientPath))
                        {
                            // 源文件版本
                            var versionSource = FileVersionInfo.GetVersionInfo(SHARE_PATH + model.sourcePath);
                            // 客户端版本
                            var versionClient = FileVersionInfo.GetVersionInfo(clientPath);

                            if (!versionSource.FileVersion.Equals(versionClient.FileVersion))
                            {
                                File.Copy(SHARE_PATH + model.sourcePath, clientPath, true);
                            }
                        }
                        else
                        {
                            File.Copy(SHARE_PATH + model.sourcePath, clientPath, true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                RemoveConnect();
            }
        }

        private void DevelopInit()
        {
            // 读取配置文件
            try
            {
                string content = File.ReadAllText(this.SHARE_PATH + @"\Pfmd\config.xml", Encoding.UTF8);
                XMLParser xp = new XMLParser();
                XMLNode xn = xp.Parse(content);
                this.URL_HEAD = xn.GetValue("Config>0>@UrlHead");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 连接远程共享文件夹
        /// </summary>
        /// <param name="path">远程共享文件夹的路径</param>
        /// <param name="userName">用户名</param>
        /// <param name="passWord">密码</param>
        /// <returns></returns>
        private bool connectState(string path, string userName, string passWord)
        {
            bool Flag = false;
            Process proc = new Process();
            try
            {
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                string dosLine = "net use " + path + " " + passWord + " /user:" + userName;
                proc.StandardInput.WriteLine(dosLine);
                proc.StandardInput.WriteLine("exit");
                while (!proc.HasExited)
                {
                    proc.WaitForExit(1000);
                }
                string errormsg = proc.StandardError.ReadToEnd();
                proc.StandardError.Close();
                if (string.IsNullOrEmpty(errormsg))
                {
                    Flag = true;
                }
                else
                {
                    if (ERR_TIMES < 3)
                    {
                        ERR_TIMES++;
                        RemoveConnect();
                        connectState(path, userName, passWord);
                    }
                    else
                    {
                        throw new Exception(errormsg);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                proc.Close();
                proc.Dispose();
            }
            return Flag;
        }

        /// <summary>
        /// 移除链接
        /// </summary>
        private void RemoveConnect()
        {
            Process proc = new Process();
            try
            {
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                string dosLine = "net use * /del /y";
                proc.StandardInput.WriteLine(dosLine);
                proc.StandardInput.WriteLine("exit");
                while (!proc.HasExited)
                {
                    proc.WaitForExit(1000);
                }
                string errormsg = proc.StandardError.ReadToEnd();
                proc.StandardError.Close();
                if (!string.IsNullOrEmpty(errormsg))
                {
                    throw new Exception(errormsg);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                proc.Close();
                proc.Dispose();
            }
        }

        private void Grid_Initialized(object sender, EventArgs e)
        {
            //确定BaseConfig
            string baseConfigPath = LOCAL_BASE + @"\config.xml";
            if (File.Exists(baseConfigPath))
            {
                pnlConfig.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                return;
            }

            ViewInit();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtSERVER_IP.Text))
                {
                    MessageBox.Show("共享服务器IP不能为空");
                    return;
                }
                if (string.IsNullOrEmpty(txtSERVER_USERNAME.Text))
                {
                    MessageBox.Show("共享服务器用户名不能为空");
                    return;
                }
                if (string.IsNullOrEmpty(txtSERVER_PASSWORD.Text))
                {
                    MessageBox.Show("共享服务器用户密码不能为空");
                    return;
                }

                var xml = new StringBuilder();
                xml.AppendLine("<Config>");
                xml.AppendLine(string.Format("  <SERVER_IP value=\"{0}\" />", txtSERVER_IP.Text.Trim()));
                xml.AppendLine(string.Format("  <SERVER_USERNAME value=\"{0}\" />", txtSERVER_USERNAME.Text.Trim()));
                xml.AppendLine(string.Format("  <SERVER_PASSWORD value=\"{0}\" />", txtSERVER_PASSWORD.Text.Trim()));
                xml.AppendLine("</Config>");
                FileWrite(LOCAL_BASE + @"\config.xml", xml.ToString());
                ViewInit();
                pnlConfig.Visibility = System.Windows.Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FileWrite(string path, string data)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                //开始写入
                sw.Write(data);
                //清空缓冲区
                sw.Flush();
                //关闭流
                sw.Close();
                fs.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void ViewInit()
        {
            Config();
            if (APP_MODE.Equals("1"))
            {
                PublishInit();
            }
            else
            {
                DevelopInit();
            }

            // 读取配置文件
            string content = File.ReadAllText(SHARE_PATH + @"\Pfmd\menu.xml", Encoding.UTF8);
            var menuList = new MenuModel().ConfigParse(content);
            this.MainGrid.Children.Add(new CommUtil.WPFUtil().UserControlFactory(this, menuList, URL_HEAD, APP_MODE) as UserControl);
        }
    }
}
