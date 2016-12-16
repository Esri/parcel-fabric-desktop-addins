using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ParcelFabricQualityControl
{
  public partial class ToolBarHelpDLG : Form
  {
    public ToolBarHelpDLG()
    {
      InitializeComponent();
    }

    private void toolStripBtnGoBack_Click(object sender, EventArgs e)
    {
      webBrowser1.GoBack();
      toolStripBtnGoBack.Visible = false;
      toolStripBtnVideoHelp.Visible = true;
    }

    private void toolStripBtnVideoHelp_Click(object sender, EventArgs e)
    {
      {
        webBrowser1.Navigate("https://esri.app.box.com/FabricQualityControl01");
        toolStripBtnVideoHelp.Visible = false;
        toolStripBtnGoBack.Visible = true;
      }
    }

    private void toolStripBtnGoBack_MouseEnter(object sender, EventArgs e)
    {
      toolStrip1.Focus();
    }
  }
}
