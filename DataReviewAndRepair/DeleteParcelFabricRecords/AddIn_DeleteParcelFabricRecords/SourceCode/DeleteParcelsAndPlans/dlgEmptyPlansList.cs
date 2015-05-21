/*
 Copyright 1995-2015 ESRI

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
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DeleteSelectedParcels
{
  public partial class dlgEmptyPlansList : Form
  {
    public dlgEmptyPlansList()
    {
      InitializeComponent();
    }

    private void label1_Click(object sender, EventArgs e)
    {

    }

    private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
    {
      if (e.NewValue == CheckState.Unchecked)
      {
        
      }
    }

    private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      
    }

    private void button2_Click(object sender, EventArgs e)
    {

    }

    private void button1_Click(object sender, EventArgs e)
    {
       
    }

    private void button4_Click(object sender, EventArgs e)
    {
      //Clear all check boxes
      foreach (int checkedItemIndex in checkedListBox1.CheckedIndices)
        checkedListBox1.SetItemChecked(checkedItemIndex, false);
      checkedListBox1_SelectedValueChanged(null, null);
    }

    private void button3_Click(object sender, EventArgs e)
    {
      //Check all check boxes
      for (int iCount = 0; iCount <= checkedListBox1.Items.Count - 1; iCount++)
        checkedListBox1.SetItemChecked(iCount, true);
      checkedListBox1_SelectedValueChanged(null, null);
    }

    private void checkedListBox1_Click(object sender, EventArgs e)
    {

    }

    public void checkedListBox1_SelectedValueChanged(object sender, EventArgs e)
    {
      string sCheckedCount = Convert.ToString(checkedListBox1.CheckedItems.Count);
      string sTotal = Convert.ToString(checkedListBox1.Items.Count);
      lblSelectionCount.Text = sCheckedCount + " of " + sTotal + " empty plans selected ";
      button1.Enabled = (checkedListBox1.CheckedItems.Count > 0);
    }
  }
}
