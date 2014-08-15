using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DeleteSelectedParcels
{
  public partial class dlgReport : Form
  {
    public dlgReport()
    {
      InitializeComponent();
    }

    private void dlgReport_Load(object sender, EventArgs e)
    {
      //textBox1.Height = dlgReport.ActiveForm.Height - btnClose.Height - btnClose.Height - 2;
      
    }

    private void dlgReport_Resize(object sender, EventArgs e)
    {
      //textBox1.Height = dlgReport.ActiveForm.Height - btnClose.Height - btnClose.Height-2;
    }
  }
}
