/*
 Copyright 1995-2017 ESRI

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

namespace ParcelFabricQualityControl
{
  public partial class ToolBarHelpDLG : Form
  {
    int m_iBrowsePageCount = 0;
    public ToolBarHelpDLG()
    {
      InitializeComponent();
    }

    private void toolStripBtnGoBack_Click(object sender, EventArgs e)
    {
      m_iBrowsePageCount--;
      webBrowser1.GoBack();
      toolStripBtnGoBack.Visible = (m_iBrowsePageCount > 0);
      //toolStripBtnVideoHelp.Visible = true;
    }

    private void toolStripBtnVideoHelp_Click(object sender, EventArgs e)
    {
      m_iBrowsePageCount++;
      webBrowser1.Navigate("https://esri.app.box.com/FabricQualityControl01");
      toolStripBtnVideoHelp.Visible = false;
      toolStripBtnGoBack.Visible = true;
    }

    private void toolStripBtnGoBack_MouseEnter(object sender, EventArgs e)
    {
      toolStrip1.Focus();
    }

    private void qualityLayersVideoMenuItem_Click(object sender, EventArgs e)
    {
      m_iBrowsePageCount++;
      webBrowser1.Navigate("https://esri.box.com/v/FabricQualityControlA");
      toolStripBtnVideoHelp.Visible = false;
      toolStripBtnGoBack.Visible = true;
    }

    private void distanceToolStripMenuItem_Click(object sender, EventArgs e)
    {
      m_iBrowsePageCount++;
      webBrowser1.Navigate("https://esri.box.com/v/FabricQualityControlB");
      toolStripBtnVideoHelp.Visible = false;
      toolStripBtnGoBack.Visible = true;
    }

    private void directionInverseToolStripMenuItem_Click(object sender, EventArgs e)
    {
      m_iBrowsePageCount++;
      webBrowser1.Navigate("https://esri.box.com/v/FabricQualityControlC");
      toolStripBtnVideoHelp.Visible = false;
      toolStripBtnGoBack.Visible = true;
    }

    private void coordinateInverseToolStripMenuItem_Click(object sender, EventArgs e)
    {
      m_iBrowsePageCount++;
      webBrowser1.Navigate("https://esri.box.com/v/FabricQualityControlD");
      toolStripBtnVideoHelp.Visible = false;
      toolStripBtnGoBack.Visible = true;
    }

    private void interpolateElevationsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      m_iBrowsePageCount++;
      webBrowser1.Navigate("https://esri.box.com/v/FabricQualityControlE");
      toolStripBtnVideoHelp.Visible = false;
      toolStripBtnGoBack.Visible = true;
    }
  }
}
