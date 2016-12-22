using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParcelEditHelper
{
  public partial class ToolbarHelpDlg : Form
  {
    public ToolbarHelpDlg()
    {
      InitializeComponent();
    }

    private void toolStrip1_MouseEnter(object sender, EventArgs e)
    {
      toolStrip1.Focus();
    }

    private void toolStripBtnVideoHelp_Click(object sender, EventArgs e)
    {
      webBrowser1.Navigate("https://esri.app.box.com/CurvesAndLines01");
      toolStripBtnVideoHelp.Visible = false;
      toolStripBtnGoBack.Visible = true;
    }

    private void toolStripBtnGoBack_Click(object sender, EventArgs e)
    {
      webBrowser1.GoBack();
      toolStripBtnGoBack.Visible = false;
      toolStripBtnVideoHelp.Visible = true;
    }
  }
}
