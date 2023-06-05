using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public partial class CloseCaptionButton : Button
    {
        #region Ctor
        
        public CloseCaptionButton()
        {
            InitializeComponent();
            ToolTip = "关闭";
        }

        #endregion

        #region Methods

        protected override void OnClick()
        {
            base.OnClick();
            Window.GetWindow(this).Close();
        }

        #endregion

    }
}
