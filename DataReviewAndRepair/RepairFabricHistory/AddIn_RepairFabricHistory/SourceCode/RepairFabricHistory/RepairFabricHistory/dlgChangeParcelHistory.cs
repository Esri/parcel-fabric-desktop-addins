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

namespace RepairFabricHistory
{
  public partial class dlgChangeParcelHistory : Form
  {
    public dlgChangeParcelHistory()
    {
      InitializeComponent();
    }

    private void dlgChangeParcelHistory_Load(object sender, EventArgs e)
    {
      System.Drawing.Size sz = new System.Drawing.Size(358, 400);
      this.Size = sz;
      cboHistorical.SelectedIndex = 0;
      cboHistoryDateField.SelectedIndex = 0;
    }

    private void radioButton1_CheckedChanged(object sender, EventArgs e)
    {
      this.dateTimePicker1.Enabled = optChooseDate.Checked;
    }

    private void radioButton2_CheckedChanged(object sender, EventArgs e)
    {
      this.dateTimePicker1.Enabled = optChooseDate.Checked;
    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
      this.panel1.Enabled = chkHistoryDateType.Checked;
  //    this.button1.Enabled = (chkHistoryDateType.Checked || chkToggleHistoric.Checked);
    }

    private void checkBox2_CheckedChanged(object sender, EventArgs e)
    {
      cboHistorical.Enabled = chkToggleHistoric.Checked;
      this.button1.Enabled = (chkHistoryDateType.Checked || chkToggleHistoric.Checked);
    }

    private void cboHistoryDateField_SelectedIndexChanged(object sender, EventArgs e)
    {
 
    }

    private void cboHistorical_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void optChooseLSDate_CheckedChanged(object sender, EventArgs e)
    {
      this.dtLSDatePicker.Enabled = optChooseLSDate.Checked;
    }

    private void optClearLSDate_CheckedChanged(object sender, EventArgs e)
    {
      this.dtLSDatePicker.Enabled = optChooseLSDate.Checked;
    }

    private void optChooseLEDate_CheckedChanged(object sender, EventArgs e)
    {
      this.dtLEDatePicker.Enabled = optChooseLEDate.Checked;
    }

    private void optClearLEDate_CheckedChanged(object sender, EventArgs e)
    {
      this.dtLEDatePicker.Enabled = optChooseLEDate.Checked;
    }

    private void optChooseSEDate_CheckedChanged(object sender, EventArgs e)
    {
      this.dtSEDatePicker.Enabled = optChooseSEDate.Checked;
    }

    private void optClearSEDate_CheckedChanged(object sender, EventArgs e)
    {
      this.dtSEDatePicker.Enabled = optChooseSEDate.Checked;
    }

    private void chkLegalStartDate_CheckedChanged(object sender, EventArgs e)
    {
      this.panelLSD.Enabled = chkLegalStartDate.Checked;
      this.button1.Enabled = (chkLegalStartDate.Checked || chkLegalEndDate.Checked || chkSystemEndDate.Checked);
    }

    private void chkLegalEndDate_CheckedChanged(object sender, EventArgs e)
    {
      this.panelLED.Enabled = chkLegalEndDate.Checked;
      this.button1.Enabled = (chkLegalStartDate.Checked || chkLegalEndDate.Checked || chkSystemEndDate.Checked);
    }

    private void chkSystemEndDate_CheckedChanged(object sender, EventArgs e)
    {
      this.panelSED.Enabled = chkSystemEndDate.Checked;
      this.button1.Enabled = (chkLegalStartDate.Checked || chkLegalEndDate.Checked || chkSystemEndDate.Checked);
    } 
  }
}
