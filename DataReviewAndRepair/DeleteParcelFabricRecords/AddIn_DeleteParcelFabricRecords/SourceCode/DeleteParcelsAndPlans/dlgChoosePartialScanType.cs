/*
 Copyright 1995-2014 Esri

 All rights reserved under the copyright laws of the United States.

 You may freely redistribute and use this sample code, with or without modification.

 Disclaimer: THE SAMPLE CODE IS PROVIDED "AS IS" AND ANY EXPRESS OR IMPLIED 
 WARRANTIES, INCLUDING THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS 
 FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL ESRI OR 
 CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
 OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 INTERRUPTION) SUSTAINED BY YOU OR A THIRD PARTY, HOWEVER CAUSED AND ON ANY 
 THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT ARISING IN ANY 
 WAY OUT OF THE USE OF THIS SAMPLE CODE, EVEN IF ADVISED OF THE POSSIBILITY OF 
 SUCH DAMAGE.

 For additional information contact: Environmental Systems Research Institute, Inc.

 Attn: Contracts Dept.

 380 New York Street

 Redlands, California, U.S.A. 92373 

 Email: contracts@esri.com
*/
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

    private void dlgChoosePartialScanType_Load(object sender, EventArgs e)
    {

    }
  }
}
