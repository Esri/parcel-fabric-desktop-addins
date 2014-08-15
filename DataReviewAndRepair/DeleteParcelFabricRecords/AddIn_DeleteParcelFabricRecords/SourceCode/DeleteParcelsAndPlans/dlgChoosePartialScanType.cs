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
  public partial class dlgChoosePartialScanType : Form
  {
    public dlgChoosePartialScanType()
    {
      InitializeComponent();
    }

    static public string AssemblyDirectory
    {
      get
      {
        string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
        UriBuilder uri = new UriBuilder(codeBase);
        string path = Uri.UnescapeDataString(uri.Path);
        return System.IO.Path.GetDirectoryName(path);
      }
    }

    private void panel1_MouseEnter(object sender, EventArgs e)
    {
      panel1.Focus();
    }

    private void optOrphanPoints_HelpRequested(object sender, HelpEventArgs hlpevent)
    {
      ToolBarHelpDLG HelpInfo = new ToolBarHelpDLG();

      string fileName = AssemblyDirectory + "/Help/OrphanPoints.htm";
      HelpInfo.webBrowser1.Url = new Uri(fileName);
      HelpInfo.Location = this.Location;
      HelpInfo.Owner = this;
      HelpInfo.Show();

      hlpevent.Handled = true;
    }

    private void optSameFromTo_HelpRequested(object sender, HelpEventArgs hlpevent)
    {
      ToolBarHelpDLG HelpInfo = new ToolBarHelpDLG();

      string fileName = AssemblyDirectory + "/Help/LinesWithSameToFrom.htm";
      HelpInfo.webBrowser1.Url = new Uri(fileName);
      HelpInfo.Location = this.Location;
      HelpInfo.Owner = this;
      HelpInfo.Show();

      hlpevent.Handled = true;
    }

    private void optOrphanLines_HelpRequested(object sender, HelpEventArgs hlpevent)
    {
      ToolBarHelpDLG HelpInfo = new ToolBarHelpDLG();

      string fileName = AssemblyDirectory + "/Help/OrphanLines.htm";
      HelpInfo.webBrowser1.Url = new Uri(fileName);
      HelpInfo.Location = this.Location;
      HelpInfo.Owner = this;
      HelpInfo.Show();

      hlpevent.Handled = true;
    }

    private void optOrphanLinePoints_HelpRequested(object sender, HelpEventArgs hlpevent)
    {
      ToolBarHelpDLG HelpInfo = new ToolBarHelpDLG();

      string fileName = AssemblyDirectory + "/Help/LinepointChecks.htm";
      HelpInfo.webBrowser1.Url = new Uri(fileName);
      HelpInfo.Location = this.Location;
      HelpInfo.Owner = this;
      HelpInfo.Show();

      hlpevent.Handled = true;
    }

    private void optLinePointsDisplaced_HelpRequested(object sender, HelpEventArgs hlpevent)
    {
      ToolBarHelpDLG HelpInfo = new ToolBarHelpDLG();

      string fileName = AssemblyDirectory + "/Help/LinepointChecks.htm";
      HelpInfo.webBrowser1.Url = new Uri(fileName);
      HelpInfo.Location = this.Location;
      HelpInfo.Owner = this;
      HelpInfo.Show();

      hlpevent.Handled = true;
    }

    private void optNoLineParcels_HelpRequested(object sender, HelpEventArgs hlpevent)
    {
      ToolBarHelpDLG HelpInfo = new ToolBarHelpDLG();

      string fileName = AssemblyDirectory + "/Help/ParcelsWithNoLines.htm";
      HelpInfo.webBrowser1.Url = new Uri(fileName);
      HelpInfo.Location = this.Location;
      HelpInfo.Owner = this;
      HelpInfo.Show();

      hlpevent.Handled = true;
    }

    private void optInvalidVectors_HelpRequested(object sender, HelpEventArgs hlpevent)
    {
      ToolBarHelpDLG HelpInfo = new ToolBarHelpDLG();

      string fileName = AssemblyDirectory + "/Help/InvalidVectors.htm";
      HelpInfo.webBrowser1.Url = new Uri(fileName);
      HelpInfo.Location = this.Location;
      HelpInfo.Owner = this;
      HelpInfo.Show();

      hlpevent.Handled = true;
    }

    private void optInvalidFCAssocs_HelpRequested(object sender, HelpEventArgs hlpevent)
    {
      ToolBarHelpDLG HelpInfo = new ToolBarHelpDLG();

      string fileName = AssemblyDirectory + "/Help/InvalidFCAssociations.htm";
      HelpInfo.webBrowser1.Url = new Uri(fileName);
      HelpInfo.Location = this.Location;
      HelpInfo.Owner = this;
      HelpInfo.Show();

      hlpevent.Handled = true;
    }
  }
}
