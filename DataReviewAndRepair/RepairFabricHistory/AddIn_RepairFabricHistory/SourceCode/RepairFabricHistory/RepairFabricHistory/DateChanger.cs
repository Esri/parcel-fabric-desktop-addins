/*
 Copyright 1995-2012 ESRI

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
using System.Collections;

using System.Windows.Forms;

namespace RepairFabricHistory
{
  public partial class DateChanger : Form
  {
    private Hashtable m_FieldMap;
    public DateChanger()
    {
      InitializeComponent();
    }
    public Hashtable FieldMap
    {
      set { m_FieldMap = value; }
    }

    private void cboBoxFields_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void cboBoxFabricClasses_SelectedIndexChanged(object sender, EventArgs e)
    {
      FillFieldComboFromFabClassComboIndex(this.cboBoxFabricClasses.SelectedIndex, m_FieldMap);
    }

    internal void FillFieldComboFromFabClassComboIndex(int ComboIndex, Hashtable FieldMap)
    {
      this.cboBoxFields.Items.Clear();
      string s = (string)FieldMap[ComboIndex];
      if (s.Trim() == "")
      {
        this.dateTimePicker1.Enabled = false;
        this.button1.Enabled = false;
        this.cboBoxFields.Items.Add("<There are no date fields for this layer>");
        this.cboBoxFields.SelectedIndex = 0;
        return;
      }
      else
      {
        this.dateTimePicker1.Enabled = radioButton1.Checked; 
        this.button1.Enabled = true;
        string[] sOID = s.Split(',');
        foreach (string s2 in sOID)
        {
          if (s2.ToLower()!="systemstartdate")
            this.cboBoxFields.Items.Add(s2);
        }
        this.cboBoxFields.SelectedIndex = 0;
      }
    }

    private void DateChanger_Load(object sender, EventArgs e)
    {

    }

    private void radioButton2_CheckedChanged(object sender, EventArgs e)
    {
      this.dateTimePicker1.Enabled = radioButton1.Checked;
    }

    private void radioButton1_CheckedChanged(object sender, EventArgs e)
    {
      this.dateTimePicker1.Enabled = radioButton1.Checked &&
      this.cboBoxFields.Text != "<There are no date fields for this layer>";

    }

  }
}
