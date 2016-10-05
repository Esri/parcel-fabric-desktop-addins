using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FabricPointMoveToFeature
{
  public partial class ToolbarHelpDLG : Form
  {
    public ToolbarHelpDLG()
    {
      InitializeComponent();
    }

    private void ToolbarHelpDLG_Load(object sender, EventArgs e)
    {

    }

    private void toolStripBtnVideoHelp_Click(object sender, EventArgs e)
    {
      //m_bInVideoMode = !m_bInVideoMode;
      //if (!m_bInVideoMode)
      {
        webBrowser1.Navigate("https://esri.app.box.com/FabricPointMoveToFeature01");
        toolStripBtnVideoHelp.Visible = false;
        toolStripBtnGoBack.Visible = true;
      }
    }

    private void toolStripBtnGoBack_Click(object sender, EventArgs e)
    {
      webBrowser1.GoBack();
      toolStripBtnGoBack.Visible = false;
      toolStripBtnVideoHelp.Visible = true;
    }

    private void toolStrip1_MouseEnter(object sender, EventArgs e)
    {
      toolStrip1.Focus();
    }
  }
}
