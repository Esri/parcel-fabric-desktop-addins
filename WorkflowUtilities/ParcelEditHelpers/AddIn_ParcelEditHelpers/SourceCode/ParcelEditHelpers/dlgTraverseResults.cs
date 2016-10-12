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
  public partial class dlgTraverseResults : Form
  {
    public dlgTraverseResults()
    {
      InitializeComponent();
    }

    private void dataGridView1_SelectionChanged(object sender, EventArgs e)
    {
      dataGridView1.ClearSelection();
    }
  }
}
