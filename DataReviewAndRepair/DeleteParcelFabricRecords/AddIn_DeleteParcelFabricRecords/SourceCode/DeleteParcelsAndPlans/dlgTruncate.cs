/*
 Copyright 1995-2011 ESRI

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
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.esriSystem;

namespace DeleteSelectedParcels
{
  public partial class dlgTruncate : Form
  {
    private ICadastralFabric m_pCadaFab;
    private IArray m_array;
    private int m_RowDropCount;
    private bool m_ResetAccuracyTableDefaults;
    private bool m_TruncateParcelsLinesPoints;
    private bool m_TruncateControl;
    private bool m_TruncateAdjustmentTables;
    private bool m_TruncateJobTables;

    public ICadastralFabric TheFabric
    {
      set
      {
        m_pCadaFab = value;
      }
    }

    public IArray TheTableArray
    {
      get
      {
        return m_array;
      }
      set
      {
        m_array = value;
      }
    }

    public int DropRowCount
    {
      get
      {
        return m_RowDropCount;
      }
     }

    public bool ResetAccuracyTableDefaults
    {
      get
      {
        return m_ResetAccuracyTableDefaults;
      }
    }

    public bool TruncateControl
    {
      get
      {
        return m_TruncateControl;
      }
    }

    public bool TruncateParcelsLinesPoints
    {
      get
      {
        return m_TruncateParcelsLinesPoints;
      }
    }

    public bool TruncateJobs
    {
      get
      {
        return m_TruncateJobTables;
      }
    }

    public bool TruncateAdjustments
    {
      get
      {
        return m_TruncateAdjustmentTables;
      }
    }
    
    public dlgTruncate()
    {
      InitializeComponent();
    }

    private void button2_Click(object sender, EventArgs e)
    {
      //Clear all check boxes
      foreach (int checkedItemIndex in checkedListBox1.CheckedIndices)
        checkedListBox1.SetItemChecked(checkedItemIndex, false);
      checkedListBox1_SelectedValueChanged(null, null); 
    }

    private void dlgTruncate_Load(object sender, EventArgs e)
    {
      this.Width = 442;
      this.Height = 273;
    }

    private void button1_Click(object sender, EventArgs e)
    {
      //Check all check boxes
      for (int iCount = 0; iCount <= checkedListBox1.Items.Count - 1; iCount++)
        checkedListBox1.SetItemChecked(iCount, true);
      checkedListBox1_SelectedValueChanged(null,null);
    }

    private void checkedListBox1_SelectedValueChanged(object sender, EventArgs e)
    {
      button4.Enabled = (checkedListBox1.CheckedItems.Count > 0);
    }

    private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
    {
      checkedListBox1.ClearSelected();
    }

    private void RefreshTableArray()
    {
      m_array.RemoveAll();
      ITable pCadastralTable;
      IDataset pDS;
      txtBoxSummary.Text = "";

      if (m_pCadaFab == null)
        return;
      string Suffix;
      m_RowDropCount = 0;
      m_ResetAccuracyTableDefaults = false;
      m_TruncateControl = false;
      m_TruncateParcelsLinesPoints = false;
      m_TruncateAdjustmentTables = false;
      m_TruncateJobTables = false;

      try
      {
        foreach (int checkedItemIndex in checkedListBox1.CheckedIndices)
        {
          if (txtBoxSummary.Text.Trim().Length == 0)
            Suffix = "";
          else
            Suffix = txtBoxSummary.Text + Environment.NewLine;

          //Drop all control points
          if (checkedItemIndex == 0)
          {
            pCadastralTable = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTControl);
            m_TruncateControl = true;
            m_array.Add(pCadastralTable);
            pDS = (IDataset)pCadastralTable;
            int RowCnt = pCadastralTable.RowCount(null);
            m_RowDropCount += RowCnt;
            string sInfo = Convert.ToString(RowCnt) + " rows";
            if (RowCnt == 0)
              sInfo = "empty";
            txtBoxSummary.Text = pDS.Name
              + " (" + sInfo + ")";
          }

          //Drop all plans, parcels, lines, points, and line-points
          if (checkedItemIndex == 1)
          {
            pCadastralTable = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTParcels);
            m_TruncateParcelsLinesPoints = true;
            m_array.Add(pCadastralTable);
            pDS = (IDataset)pCadastralTable;
            int RowCnt = pCadastralTable.RowCount(null);
            m_RowDropCount += RowCnt;
            string sInfo = Convert.ToString(RowCnt) + " rows";
            if (RowCnt == 0)
              sInfo = "empty";
            txtBoxSummary.Text = Suffix + pDS.Name
              + " (" + sInfo + ")" + Environment.NewLine;

            pCadastralTable = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTLines);
            m_array.Add(pCadastralTable);
            pDS = (IDataset)pCadastralTable;
            RowCnt = pCadastralTable.RowCount(null);
            m_RowDropCount += RowCnt;
            sInfo = Convert.ToString(RowCnt) + " rows";
            if (RowCnt == 0)
              sInfo = "empty";
            txtBoxSummary.Text = txtBoxSummary.Text + pDS.Name
              + " (" + sInfo + ")" + Environment.NewLine;

            pCadastralTable = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTPoints);
            m_array.Add(pCadastralTable);
            pDS = (IDataset)pCadastralTable;
            RowCnt = pCadastralTable.RowCount(null);
            m_RowDropCount += RowCnt;
            sInfo = Convert.ToString(RowCnt) + " rows";
            if (RowCnt == 0)
              sInfo = "empty";
            txtBoxSummary.Text = txtBoxSummary.Text + pDS.Name
              + " (" + sInfo + ")" + Environment.NewLine;

            pCadastralTable = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTLinePoints);
            m_array.Add(pCadastralTable);
            pDS = (IDataset)pCadastralTable;
            RowCnt = pCadastralTable.RowCount(null);
            m_RowDropCount += RowCnt;
            sInfo = Convert.ToString(RowCnt) + " rows";
            if (RowCnt == 0)
              sInfo = "empty";
            txtBoxSummary.Text = txtBoxSummary.Text + pDS.Name
              + " (" + sInfo + ")" + Environment.NewLine;

            pCadastralTable = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTPlans);
            m_array.Add(pCadastralTable);
            pDS = (IDataset)pCadastralTable;
            IQueryFilter pQF = new QueryFilterClass();
            IWorkspace pWS = pDS.Workspace;
            string sPref; string sSuff;
            ISQLSyntax pSQLSyntax = (ISQLSyntax)pWS;
            sPref = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierPrefix);
            sSuff = pSQLSyntax.GetSpecialCharacter(esriSQLSpecialCharacters.esriSQL_DelimitedIdentifierSuffix);
            string sFieldName = "NAME";
            //pQF.WhereClause = sPref + sFieldName + sSuff + " <> '<map>'";
            pQF.WhereClause = sFieldName + " <> '<map>'";

            try
            {
              RowCnt = pCadastralTable.RowCount(pQF);
            }
            
            catch(COMException ex)
            {
              MessageBox.Show(ex.Message + ": " + Convert.ToString(ex.ErrorCode));
              return;
            }

            m_RowDropCount += RowCnt;
            sInfo = Convert.ToString(RowCnt) + " rows";
            if (RowCnt == 0)
              sInfo = "empty";
            txtBoxSummary.Text = txtBoxSummary.Text + pDS.Name
              + " (" + sInfo + ")";
          }

          //Drop all fabric jobs
          if (checkedItemIndex == 2)
          {
            pCadastralTable = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTJobObjects);
            m_TruncateJobTables = true;
            m_array.Add(pCadastralTable);
            pDS = (IDataset)pCadastralTable;
            int RowCnt = pCadastralTable.RowCount(null);
            m_RowDropCount += RowCnt;
            string sInfo = Convert.ToString(RowCnt) + " rows";
            if (RowCnt == 0)
              sInfo = "empty";
            txtBoxSummary.Text = Suffix + pDS.Name
              + " (" + sInfo + ")" + Environment.NewLine;

            pCadastralTable = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTJobs);
            m_array.Add(pCadastralTable);
            pDS = (IDataset)pCadastralTable;
            RowCnt = pCadastralTable.RowCount(null);
            m_RowDropCount += RowCnt;
            sInfo = Convert.ToString(RowCnt) + " rows";
            if (RowCnt == 0)
              sInfo = "empty";
            txtBoxSummary.Text = txtBoxSummary.Text + pDS.Name
              + " (" + sInfo + ")";
          }

          //Drop all feature adjustment information and vectors
          if (checkedItemIndex == 3)
          {
            pCadastralTable = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTVectors);
            m_TruncateAdjustmentTables = true;
            m_array.Add(pCadastralTable);
            pDS = (IDataset)pCadastralTable;
            int RowCnt = pCadastralTable.RowCount(null);
            m_RowDropCount += RowCnt;
            string sInfo = Convert.ToString(RowCnt) + " rows";
            if (RowCnt == 0)
              sInfo = "empty";
            txtBoxSummary.Text = Suffix + pDS.Name
              + " (" + sInfo + ")" + Environment.NewLine;

            pCadastralTable = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTLevels);
            m_array.Add(pCadastralTable);
            pDS = (IDataset)pCadastralTable;
            RowCnt = pCadastralTable.RowCount(null);
            m_RowDropCount += RowCnt;
            sInfo = Convert.ToString(RowCnt) + " rows";
            if (RowCnt == 0)
              sInfo = "empty";
            txtBoxSummary.Text = txtBoxSummary.Text + pDS.Name
              + " (" + sInfo + ")" + Environment.NewLine;

            pCadastralTable = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTAdjustments);
            m_array.Add(pCadastralTable);
            pDS = (IDataset)pCadastralTable;
            RowCnt = pCadastralTable.RowCount(null);
            m_RowDropCount += RowCnt;
            sInfo = Convert.ToString(RowCnt) + " rows";
            if (RowCnt == 0)
              sInfo = "empty";
            txtBoxSummary.Text = txtBoxSummary.Text + pDS.Name
              + " (" + sInfo + ")";
          }

          //Reset Accuracy to default values
          if (checkedItemIndex == 4)
          {
            m_ResetAccuracyTableDefaults = true;
            pCadastralTable = m_pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTAccuracy);
            m_array.Add(pCadastralTable);
            pDS = (IDataset)pCadastralTable;
            int RowCnt = pCadastralTable.RowCount(null);
            m_RowDropCount += RowCnt;
            txtBoxSummary.Text = Suffix + pDS.Name +
              " (" + Convert.ToString(RowCnt) + " rows)..." + Environment.NewLine
              + "=================================" + Environment.NewLine
              + "...then will re-add these default records" + Environment.NewLine
              + " to the Accuracy table:" + Environment.NewLine +
              "==================" + Environment.NewLine +
              "1 - Highest" + Environment.NewLine +
              "2 - After 1980" + Environment.NewLine +
              "3 - 1908 to 1980" + Environment.NewLine +
              "4 - 1881 to 1907" + Environment.NewLine +
              "5 - Before 1881" + Environment.NewLine +
              "6 - 1800" + Environment.NewLine +
              "7 - Lowest" + Environment.NewLine;
          }
        }
      }

      catch(COMException ex)
      {
        MessageBox.Show(ex.Message + ": " + Convert.ToString(ex.ErrorCode));
      }

    }

    private void button4_Click(object sender, EventArgs e)
    {
      btnTruncate.Size = button4.Size;
      btnTruncate.Location = button4.Location;
      button4.Visible = false;
      btnTruncate.Visible = true;

      txtBoxSummary.Size = checkedListBox1.Size;
      txtBoxSummary.Location = checkedListBox1.Location;
      checkedListBox1.Visible = false;
      txtBoxSummary.Visible = true;

      button5.Enabled = true;
      button1.Visible = false;
      button2.Visible = false;
      this.Text = "Truncate Fabric Summary";
      label1.Text = "Truncate will delete ALL rows from these tables:";
      RefreshTableArray();
    }

    private void button5_Click(object sender, EventArgs e)
    {
      this.Text = "Truncate Fabric Classes";
      label1.Text = "Choose fabric class groups and then click Next:";
      btnTruncate.Visible = false;
      button4.Visible = true;
      checkedListBox1.Visible = true;
      txtBoxSummary.Visible = false;
      button5.Enabled = false;
      button1.Visible = true;
      button2.Visible = true;
    }

    private void btnTruncate_Click(object sender, EventArgs e)
    {

    }

  }
}
