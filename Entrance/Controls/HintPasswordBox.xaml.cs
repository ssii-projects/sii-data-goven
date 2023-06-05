using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Catpain.Agriculture.DataGovern
{
    /// <summary>
    /// HintPasswordBox.xaml 的交互逻辑
    /// </summary>
    public partial class HintPasswordBox : UserControl
    {
        public string Hint
        {
            get;
            set;
        }
        public string Password
        {
            get
            {
                return pbPwd.Password;
            }
            set
            {
                pbPwd.Password = value;
            }
        }
        public HintPasswordBox()
        {
            Hint = "请输入密码";
            InitializeComponent();
            pbPwd.PasswordChanged += (s, e) =>
            {
                if (pbPwd.Password.Length > 0)
                {
                    tbHint.Text = "";
                }else
                {
                    tbHint.Text = Hint;
                }
            };
        }
    }
}
