/*
 Copyright 1995-2014 ESRI

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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FabricPlanTools
{
  public partial class dlgFixParcelsWithNoPlan : Form
  {
    bool m_EventsOn = false;
    ArrayList m_PlansList = null;
    public dlgFixParcelsWithNoPlan()
    {
      InitializeComponent();
    }

    public ArrayList ThePlansList
    {
      set
      {
        m_PlansList = value;
      }
    }

    private void btnNext_Click(object sender, EventArgs e)
    {
      listView1.Visible = false;
      listViewByGroup.Visible = false;
      panel1.Visible = true;

      btnFix.Location= btnNext.Location;
      btnFix.Visible = true;
      btnNext.Visible = false;
      btnBack.Enabled = true;

      btnSelectAll.Visible = false;
      btnClearAll.Visible = false;

      label2.Visible = false;
      label3.Location = label1.Location;
      label3.Visible = true;

      label1.Visible = false;
      chkSelectByGroups.Visible = false;

    }

    private void dlgFixParcelsWithNoPlan_Load(object sender, EventArgs e)
    {
      panel1.Left = 19;
      panel1.Top = 26;
      listView1.Left= 12;
      listView1.Top = 47;
      listView1.Visible = true;
      btnBack.Enabled = false;
      RightAlignLabelToButton(this.lblSelectionCount, this.btnNext, this.lblSelectionCount.Text);
      chkSelectByGroups.Checked = true;
    }

    private void btnBack_Click(object sender, EventArgs e)
    {

      label1.Visible = true;
      label2.Location = label3.Location;
      label3.Visible = false;
      label2.Visible = true;

      chkSelectByGroups.Visible = true;
      btnSelectAll.Visible = true;
      btnClearAll.Visible = true;
      btnFix.Visible = false;
      btnBack.Enabled = false;
      btnNext.Visible = true;
      panel1.Visible = false;
      listView1.Visible = !chkSelectByGroups.Checked;
      listViewByGroup.Visible = chkSelectByGroups.Checked;

    }

    private void radioBtnUserDef_CheckedChanged(object sender, EventArgs e)
    {
      if (radioBtnUserDef.Checked)
      {
        btnFix.Enabled = (txtPlanName.Text.Trim().Length > 0);
        txtPlanName.Enabled = true;
      }
      else
      {
        btnFix.Enabled = true;
        txtPlanName.Enabled = false;
      }
    }

    private void radioBtnPlanID_CheckedChanged(object sender, EventArgs e)
    {
      btnFix.Enabled = true;
    }

    private void txtPlanName_TextChanged(object sender, EventArgs e)
    {
      btnFix.Enabled = (txtPlanName.Text.Length > 0);
    }

    private void btnSelectAll_Click(object sender, EventArgs e)
    {
      m_EventsOn = false;
      //Check all check boxes
      foreach (ListViewItem listItem in listView1.Items)
	      listItem.Checked = true;
      string sLabel = "(" + listView1.Items.Count.ToString() + " of " +
        listView1.Items.Count.ToString() + " selected to fix)";

      foreach (ListViewItem listItem in listViewByGroup.Items)
      {
        listItem.Checked = true;
        string[] s = Regex.Split(listItem.SubItems[1].Text, " of ");
        listItem.SubItems[1].Text = s[1] + " of " + s[1];
      }

      btnNext.Enabled = true;
      RightAlignLabelToButton(lblSelectionCount,btnNext, sLabel);
      m_EventsOn = true;
    }
    
    private void RightAlignLabelToButton(Label lbl, Button btn, string LabelText)
    {
      lbl.Visible = false;
      lbl.Text = LabelText;
      lbl.Left = btn.Left - lbl.Width + btn.Width;
      lbl.Visible = true;
    }

    private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
      int iCheckCount = 1;
      if (e.Item.Checked == false)
        iCheckCount = -1;

      if (Control.ModifierKeys == Keys.Shift && m_EventsOn)
      {
        m_EventsOn = false;
        iCheckCount = MultiCheckInGroup(e);
        m_EventsOn = true;
       }

      if (m_EventsOn)
      {
        int iChkCnt = listView1.CheckedItems.Count;
        string sLabel = "(" + iChkCnt.ToString() +
        " of " + listView1.Items.Count.ToString() + " selected to fix)";
        RightAlignLabelToButton(lblSelectionCount, btnNext, sLabel);
        btnNext.Enabled = (iChkCnt > 0);

        string sItem = e.Item.Text.Trim();
        foreach (ListViewItem listItem in listViewByGroup.Items)
        {
          if (listItem.Text.Trim() == sItem)
          {
            string[] s = Regex.Split(listItem.SubItems[1].Text, " of ");
            int iNewVal = Convert.ToInt32(s[0]);
            iNewVal+=iCheckCount;
            string sNewVal = iNewVal.ToString().Trim();
            listItem.SubItems[1].Text = sNewVal + " of " + s[1];
            m_EventsOn = false;
            listItem.Checked = (iNewVal != 0);
            m_EventsOn = true;
            break;
          }
        }
      }
    }

    private int MultiCheckInGroup(ItemCheckedEventArgs e)
    {
      //find the first instance of a box with this text
      //Check boxes that have this text
      int iPos=-1;
      bool bCheck = e.Item.Checked;
      int iCnt = 1;
      if (!bCheck)
        iCnt = -1;
      foreach (ListViewItem listItem in listView1.Items)
      {
        if (listItem.Text.Trim() == e.Item.Text.Trim())
        {
          iPos = listItem.Index;
          bool bCheckChange = !(listItem.Checked == bCheck);
          listItem.Checked = bCheck;
          if (bCheckChange && listItem.Checked==true)
            iCnt++;
          else if (bCheckChange && listItem.Checked==false)
            iCnt--;
          if (e.Item.Index == iPos)
            break;
        }
      }
      return iCnt;
    }

    private void listViewByGroup_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
      if (m_EventsOn)
      {
        string sPlanID = e.Item.Text.Trim();

          m_EventsOn = false;
          //Check boxes that have this text
          foreach (ListViewItem listItem in listView1.Items)
          {
            if (listItem.Text.Trim()==sPlanID)
              listItem.Checked = e.Item.Checked;
          }
          m_EventsOn = true;

        string[] s = Regex.Split(e.Item.SubItems[1].Text, " of ");

        if (e.Item.Checked == true)
          e.Item.SubItems[1].Text = s[1] + " of " + s[1];
        else
          e.Item.SubItems[1].Text = "0 of " + s[1];

        int iChkCnt = listView1.CheckedItems.Count;
        string sLabel = "(" + iChkCnt.ToString() +
        " of " + listView1.Items.Count.ToString() + " selected to fix)";
        RightAlignLabelToButton(lblSelectionCount, btnNext, sLabel);
        btnNext.Enabled = (iChkCnt > 0);
        RightAlignLabelToButton(lblSelectionCount, btnNext, sLabel);
      }
    }

    private void btnClearAll_Click(object sender, EventArgs e)
    {
      m_EventsOn = false;
      //Un-Check all check boxes
      foreach (ListViewItem listItem in listView1.Items)
        listItem.Checked = false;
      string sLabel = "(0 of " + listView1.Items.Count.ToString() + " selected to fix)";

      foreach (ListViewItem listItem in listViewByGroup.Items)
      {
        listItem.Checked = false;
        string[] s = Regex.Split(listItem.SubItems[1].Text, " of ");
        listItem.SubItems[1].Text = "0 of " + s[1];
      }

      btnNext.Enabled = false;
      RightAlignLabelToButton(lblSelectionCount, btnNext, sLabel);
      m_EventsOn = true;
    }

    private void chkSelectByGroups_CheckedChanged(object sender, EventArgs e)
    {
      listViewByGroup.Location = listView1.Location;
      listViewByGroup.Size = listView1.Size;
      listViewByGroup.Visible = chkSelectByGroups.Checked;
      listView1.Visible = !listViewByGroup.Visible;
    }

    private void listView1_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void listViewByGroup_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void listView1_MouseDown(object sender, MouseEventArgs e)
    {
      m_EventsOn = true;
    }

    private void listViewByGroup_MouseDown(object sender, MouseEventArgs e)
    {
      m_EventsOn = true;
    }

    private void radioBtnExistingPlan_CheckedChanged(object sender, EventArgs e)
    {
      if (radioBtnExistingPlan.Checked)
      {
        btnFix.Enabled = (listPlans.SelectedItems.Count==1);
        btnFilterList.Enabled = true;
        listPlans.Enabled = true;
        txtFilter.Enabled = true;
      }
      else
      {
        btnFilterList.Enabled = false;
        listPlans.Enabled = false;
        txtFilter.Enabled = false;
      }
    }

    private void btnFilterList_Click(object sender, EventArgs e)
    {
      FillTheListBox(this.listPlans, m_PlansList, this.txtFilter.Text);
      btnFix.Enabled = false;
    }
    
    private void FillTheListBox(ListBox TheList, ArrayList PlansList, string Filter)
    {
      if (Filter == null)
        Filter = "";

      Filter=Filter.Trim().ToUpper();
      TheList.Items.Clear();
      foreach (object s in PlansList)
      {
        string s2 = s.ToString().ToUpper();
        if (s2.StartsWith(Filter) || (Filter==""))
          TheList.Items.Add(s);
      }
    }

    private void listPlans_SelectedIndexChanged(object sender, EventArgs e)
    {
      btnFix.Enabled = (listPlans.SelectedItems.Count == 1);
    }

  }
}
