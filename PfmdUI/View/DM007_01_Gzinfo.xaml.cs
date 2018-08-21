using System;
using System.Windows.Controls;

namespace PfmdUI.View
{
    /// <summary>
    /// DM007_01_Gzinfo.xaml 的交互逻辑
    /// </summary>
    public partial class DM007_01_GzInfo : UserControl
    {
        public DM007_01_GzInfo()
        {
            InitializeComponent();
        }

        private void Grid_Initialized(object sender, EventArgs e)
        {
            this.MainGrid.Children.Add(CommUtil.WPFUtil.UserControlFactory(this) as UserControl);
        }
    }
}
