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
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.esriSystem;

namespace DeleteSelectedParcels
{
  public partial class dlgInconsistentRecords : Form
  {
    #region Declarations

    const string sLinesThatDoNotBelongToAParcel = "Lines that do not belong to a parcel";
    const string sOrphanLinesListINFO = "After delete, re-run this utility to check for new orphan points.";
    const string sPointsNotConnectedToLines = "Points that are not connected to lines";
    const string sLinePointsReferenceMissingPoint = "Line points with invalid point references";
    const string sDisplacedLinePoints = "Line points with displaced geometry";
    const string sLinesWithSameFromTo = "Lines with the same To and From points";
    const string sInvalidFeatureAdjustmentVectors = "Invalid feature adjustment vectors";
    const string sParcelsThatHaveNoLines = "Parcels that have no lines";
    const string sInvalidFeatureClassAssocs = "Invalid feature class associations";

    private List<int> m_OrphanPointsList;
    private List<int> m_ParcelsWithNoLinesList;
    private List<int> m_OrphanLinesList;
    private List<int> m_OrphanLinePointsList;
    private List<int> m_DisplacedLinePointsList;

    private List<int> m_InvalidVectors;
    private List<int> m_InvalidFeatureClassAssociations;
    private List<int> m_LinesWithSameFromTo;

    private string m_FabricName;
    private string m_VersionName;
    private string m_ProcessTime;

    private int m_PlanCount;
    private int m_PointCount;
    private int m_LineCount;
    private int m_ParcelCount;
    private int m_LinePointCount;
    private int m_ControlPointCount;
    private int m_VectorCount;
    private int m_AdjustmentsCount;
    private int m_LevelsCount;
    private int m_JobObjectCount;
    private int m_JobCount;
    private int m_AccuracyCount;
    private int m_TotalCount;

    private bool m_Check4PointsNotConnectedToLines;
    private bool m_Check4LinesNotPartOfParcels;
    private bool m_Check4ParcelsHaveNoLines;
    private bool m_Check4LinePointsWithNoFabricPoints;
    private bool m_Check4LinesWithSameFromAndTo;
    private bool m_Check4InvalidFeatureAdjustmentVectors;
    private bool m_Check4InvalidFeatureClassAssociations;
    private string m_OIDFieldName;
    #endregion

    public bool CheckPointsNotConnectedToLines
    {
      get
      {
        return m_Check4PointsNotConnectedToLines;
      }
    }

    public bool CheckLinesNotPartOfParcels
    {
      get
      {
        return m_Check4LinesNotPartOfParcels;
      }
    }

    public bool CheckParcelsHaveNoLines
    {
      get
      {
        return m_Check4ParcelsHaveNoLines;
      }
    }

    public bool CheckLinePointsWithNoFabricPoints
    {
      get
      {
        return m_Check4LinePointsWithNoFabricPoints;
      }
    }

    public bool CheckLinesWithSameFromAndTo
    {
      get
      {
        return m_Check4LinesWithSameFromAndTo;
      }
    }

    public bool CheckInvalidFeatureAdjustmentVectors
    {
      get
      {
        return m_Check4InvalidFeatureAdjustmentVectors;
      }
    }

    public bool CheckInvalidFeatureClassAssociations
    {
      get
      {
        return m_Check4InvalidFeatureClassAssociations;
      }
    }

    //public ICadastralFabric TheFabric
    //{
    //  set
    //  {
    //    m_pCadaFab = value;
    //  }
    //}

    //public List<int> PointOIDs
    //{
    //  set
    //  {
    //    m_PointOIDs = value;
    //  }
    //}

    //public List<int> ParcelOIDs
    //{
    //  set
    //  {
    //    m_ParcelIDs = value;
    //  }
    //}

    //public List<int> VectorOIDs
    //{
    //  set
    //  {
    //    m_VectorIDs = value;
    //  }
    //}

    //public List<int> AdjustmentLevels
    //{
    //  set
    //  {
    //    m_AdjLevels = value;
    //  }
    //}

    //public List<int> OrphanPointsList
    //{
    //  set
    //  {
    //    m_PointIDsInLineTable = value;
    //  }
    //}

    //public List<int> PointOIDsInLinesTable
    //{
    //  set
    //  {
    //    m_PointIDsInLineTable = value;
    //  }
    //}

    //public List<int> PointOIDsInLinePointsTable
    //{
    //  set
    //  {
    //    m_PointIDsInLinePointsTable = value;
    //  }
    //}
    
    //public List<int> ParcelOIDsInLinesTable
    //{
    //  set
    //  {
    //    m_ParcelIDsInLinesTable = value;
    //  }
    //}

    public int PlanCount
    {
      set
      {
        m_PlanCount = value;
      }
    }

    public int PointCount
    {
      set
      {
        m_PointCount = value;
      }
    }

    public int LineCount
    {
      set
      {
        m_LineCount = value;
      }
    }

    public int ParcelCount
    {
      set
      {
        m_ParcelCount = value;
      }
    }
    
    public int LinePointCount
    {
      set
      {
        m_LinePointCount = value;
      }
    }
    
    public int ControlPointCount
    {
      set
      {
        m_ControlPointCount = value;
      }
    }

    public int VectorCount
    {
      set
      {
        m_VectorCount = value;
      }
    }

    public int LevelsCount
    {
      set
      {
        m_LevelsCount = value;
      }
    }

    public int AdjustmentsCount 
    {
      set
      {
        m_AdjustmentsCount = value;
      }
    }

    public int JobObjectCount
    {
      set
      {
        m_JobObjectCount = value;
      }
    }

    public int JobCount
    {
      set
      {
        m_JobCount = value;
      }
    }

    public int AccuracyCategoryCount
    {
      set
      {
        m_AccuracyCount = value;
      }
    }

    public string OIDFieldName
    {
      set
      {
        m_OIDFieldName= value;
      }
    }

    public List<int> ParcelsWithNoLines
    {
      set
      {
        m_ParcelsWithNoLinesList = value;
      }
    }

    public List<int> OrphanPoints
    {
      set
      {
        m_OrphanPointsList = value;
      }
    }

    public List<int> OrphanLines
    {
      set
      {
        m_OrphanLinesList = value;
      }
    }

    public List<int> OrphanLinePoints
    {
      set
      {
        m_OrphanLinePointsList = value;
      }
    }

    public List<int> DisplacedLinePoints
    {
      set
      {
        m_DisplacedLinePointsList = value;
      }
    }

    public List<int> LinesWithSameFromTo
    {
      set
      {
        m_LinesWithSameFromTo = value;
      }
    }

    public List<int> InvalidVectors
    {
      set
      {
        m_InvalidVectors = value;
      }
    }

    public List<int> InvalidFeatureClassAssociations
    {
      set
      {
        m_InvalidFeatureClassAssociations = value;
      }
    }

    public string FabricName
    {
      set
      {
        m_FabricName = value;
      }
    }

    public string VersionName
    {
      set
      {
        m_VersionName = value;
      }
    }

    public string ProcessTime
    {
      set
      {
        m_ProcessTime = value;
      }
            get 
      {
        return m_ProcessTime;
      }
    }

    public dlgInconsistentRecords()
    {
      InitializeComponent();
    }

    private void WriteResultsToReport()
    {
      m_TotalCount = 0;
        try
        {
        //now build a report
        if (m_Check4PointsNotConnectedToLines)
        {
          m_TotalCount += m_OrphanPointsList.Count();
          string sReport = sPointsNotConnectedToLines;
          if (m_OrphanPointsList.Count() > 0)
            sReport=sPointsNotConnectedToLines.ToLower();
          sReport = "Found " + m_OrphanPointsList.Count().ToString() + " " + sReport;
          sReport = sReport.Replace("Found 0", "OK:");
          TreeNode tNode= tvOrphanRecs.Nodes.Add(sReport);
          if (m_OrphanPointsList.Count() == 0)
            tNode.ImageIndex = tNode.SelectedImageIndex = 0;
          else
            tNode.ImageIndex = tNode.SelectedImageIndex = 1;
        }

        if (m_Check4LinesWithSameFromAndTo)
        {
          m_TotalCount += m_LinesWithSameFromTo.Count();
          string sReport = sLinesWithSameFromTo;
          if (m_LinesWithSameFromTo.Count() > 0)
            sReport = sLinesWithSameFromTo.ToLower();
          sReport = "Found " + m_LinesWithSameFromTo.Count().ToString() + " " + sReport;
          sReport = sReport.Replace("Found 0", "OK:");
          TreeNode tNode = tvOrphanRecs.Nodes.Add(sReport);
          if (m_LinesWithSameFromTo.Count() == 0)
            tNode.ImageIndex = tNode.SelectedImageIndex = 0;
          else
            tNode.ImageIndex = tNode.SelectedImageIndex = 1;
        }

        if (m_Check4LinesNotPartOfParcels)
        {
          m_TotalCount += m_OrphanLinesList.Count();
          string sReport = sLinesThatDoNotBelongToAParcel;
          if (m_OrphanLinesList.Count()>0)
            sReport = sLinesThatDoNotBelongToAParcel.ToLower();
          sReport = "Found " + m_OrphanLinesList.Count().ToString() + " " + sReport;
          sReport = sReport.Replace("Found 0", "OK:");
          TreeNode tNode = tvOrphanRecs.Nodes.Add(sReport);
          if (m_OrphanLinesList.Count() == 0)
            tNode.ImageIndex = tNode.SelectedImageIndex = 0;
          else
          {
            tNode.ImageIndex = tNode.SelectedImageIndex = 1;
            TreeNode tNode2 = tNode.Nodes.Add(sOrphanLinesListINFO);
            tNode2.ImageIndex = tNode2.SelectedImageIndex = 3;
          }
        }

        if (m_Check4LinePointsWithNoFabricPoints)
        {
          m_TotalCount += m_OrphanLinePointsList.Count();
          string sReport = sLinePointsReferenceMissingPoint;
          if (m_OrphanLinePointsList.Count()>0)
            sReport = sLinePointsReferenceMissingPoint.ToLower();
          sReport = "Found " + m_OrphanLinePointsList.Count().ToString() + " " + sReport;
          sReport = sReport.Replace("Found 0", "OK:");
          TreeNode tNode = tvOrphanRecs.Nodes.Add(sReport);
          if (m_OrphanLinePointsList.Count() == 0)
            tNode.ImageIndex = tNode.SelectedImageIndex = 0;
          else
            tNode.ImageIndex = tNode.SelectedImageIndex = 1;
        }

        if (m_Check4ParcelsHaveNoLines)
        {
          m_TotalCount += m_ParcelsWithNoLinesList.Count();
          string sReport = sParcelsThatHaveNoLines;
          if(m_ParcelsWithNoLinesList.Count()>0)
            sReport = sParcelsThatHaveNoLines.ToLower();
          sReport = "Found " + m_ParcelsWithNoLinesList.Count().ToString() + " " + sReport;
          sReport = sReport.Replace("Found 0", "OK:");
          TreeNode tNode = tvOrphanRecs.Nodes.Add(sReport);
          if (m_ParcelsWithNoLinesList.Count() == 0)
            tNode.ImageIndex = tNode.SelectedImageIndex = 0;
          else
            tNode.ImageIndex = tNode.SelectedImageIndex = 1;
        }

        if (m_Check4InvalidFeatureAdjustmentVectors)
        {
          m_TotalCount += m_InvalidVectors.Count();
          string sReport = sInvalidFeatureAdjustmentVectors;
          if (m_InvalidVectors.Count() > 0)
            sReport = sInvalidFeatureAdjustmentVectors.ToLower();
          sReport = "Found " + m_InvalidVectors.Count().ToString() + " " + sReport;
          sReport = sReport.Replace("Found 0", "OK:");
          TreeNode tNode = tvOrphanRecs.Nodes.Add(sReport);
          if (m_InvalidVectors.Count() == 0)
            tNode.ImageIndex = tNode.SelectedImageIndex = 0;
          else
            tNode.ImageIndex = tNode.SelectedImageIndex = 1;          
        }

        if (m_Check4InvalidFeatureClassAssociations)
        {
          m_TotalCount += m_InvalidFeatureClassAssociations.Count();
          string sReport = sInvalidFeatureClassAssocs;
          if (m_InvalidFeatureClassAssociations.Count() > 0)
            sInvalidFeatureClassAssocs.ToLower();
          sReport = "Found " + m_InvalidFeatureClassAssociations.Count().ToString() + " " + sReport;
          sReport = sReport.Replace("Found 0", "OK:");
          TreeNode tNode = tvOrphanRecs.Nodes.Add(sReport);
          if (m_InvalidFeatureClassAssociations.Count() == 0)
            tNode.ImageIndex = tNode.SelectedImageIndex = 0;
          else
            tNode.ImageIndex = tNode.SelectedImageIndex = 1;    
        }
      }

      catch (Exception ex)
      {
        MessageBox.Show(ex.Message + ": " + Convert.ToString(ex.Source));
      }

    }

    private void btnSelectAll_Click(object sender, EventArgs e)
    {
      //Check all check boxes
      for (int iCount = 0; iCount <= checkedListBox1.Items.Count - 1; iCount++)
        checkedListBox1.SetItemChecked(iCount, true);
      checkedListBox1_SelectedValueChanged(null, null);
    }

    private void checkedListBox1_SelectedValueChanged(object sender, EventArgs e)
    {
      btnNext.Enabled = (checkedListBox1.CheckedItems.Count > 0);
    }

    private void btnDeselectAll_Click(object sender, EventArgs e)
    {
      //Clear all check boxes
      foreach (int checkedItemIndex in checkedListBox1.CheckedIndices)
        checkedListBox1.SetItemChecked(checkedItemIndex, false);
      checkedListBox1_SelectedValueChanged(null, null);
    }

    private void btnNext_Click(object sender, EventArgs e)
    {
      btnSaveReport.Size = btnSelectAll.Size;
      btnSaveReport.Location = btnSelectAll.Location;
      btnSaveReport.Visible = true;

      btnDelete.Size = btnNext.Size;
      btnDelete.Location = btnNext.Location;
      btnNext.Visible = false;
      btnDelete.Visible = true;

      tvOrphanRecs.Size = checkedListBox1.Size;
      tvOrphanRecs.Location = checkedListBox1.Location;
      checkedListBox1.Visible = false;
      tvOrphanRecs.Visible = true;

      btnBack.Enabled = true;
      btnSelectAll.Visible = false;
      btnDeselectAll.Visible = false;

      ReadCheckBoxes();
      WriteResultsToReport();

      if(m_TotalCount>0)
        label1.Text = "Click Delete to remove inconsistent records from the fabric:";
      else
        label1.Text = "All fabric records for the selected types are consistent:";

      btnDelete.Enabled = (m_TotalCount > 0);

    }

    private void ReadCheckBoxes()
    {
      m_Check4PointsNotConnectedToLines = false;
      m_Check4LinesNotPartOfParcels = false;
      m_Check4ParcelsHaveNoLines = false;
      m_Check4LinePointsWithNoFabricPoints = false;
      m_Check4LinesWithSameFromAndTo = false;
      m_Check4InvalidFeatureAdjustmentVectors = false;
      m_Check4InvalidFeatureClassAssociations = false;
      string sItem = "";

      foreach (int checkedItemIndex in checkedListBox1.CheckedIndices)
      {
        //these items are independent of order in the list, but depend on string
        sItem = checkedListBox1.Items[checkedItemIndex].ToString().ToLower().Trim();

        //Points that are not connected to lines (orphan points)
        if (sItem == sPointsNotConnectedToLines.ToLower())
          m_Check4PointsNotConnectedToLines = true;

        //Lines that do not belong to a parcel (orphan lines)
        if (sItem == sLinesThatDoNotBelongToAParcel.ToLower())
          m_Check4LinesNotPartOfParcels = true;

        //Parcels that have no lines
        if (sItem == sParcelsThatHaveNoLines.ToLower())
          m_Check4ParcelsHaveNoLines = true;

        //Line points that reference a missing point
        if (sItem == sLinePointsReferenceMissingPoint.ToLower())
          m_Check4LinePointsWithNoFabricPoints = true;

        //Lines with Same From and To Points
        if (sItem == sLinesWithSameFromTo.ToLower())
          m_Check4LinesWithSameFromAndTo = true;

        //Invalid feature adjustment vectors
        if (sItem == sInvalidFeatureAdjustmentVectors.ToLower())
          m_Check4InvalidFeatureAdjustmentVectors = true;

        //Invalid feature class associations
        if (sItem == sInvalidFeatureClassAssocs.ToLower())
          m_Check4InvalidFeatureClassAssociations = true;
      }
    }

    private void btnBack_Click(object sender, EventArgs e)
    {
      label1.Text = "Choose the type of records to report or delete:";
      btnSaveReport.Visible = false;
      btnDelete.Visible = false;
      btnNext.Visible = true;
      checkedListBox1.Visible = true;
      tvOrphanRecs.Visible = false;
      tvOrphanRecs.Nodes.Clear();
      btnBack.Enabled = false;
      btnSelectAll.Visible = true;
      btnDeselectAll.Visible = true;
    }

    private void dlgInconsistentRecords_Load(object sender, EventArgs e)
    {
      /*
  Lines that do not belong to a parcel
  Points that are not connected to lines
  Line points that reference a missing point
  Lines with the same To and From points
  Invalid feature adjustment vectors
 * */
      this.tvOrphanRecs.ImageList = imageList1;

      checkedListBox1.Items.Add(sPointsNotConnectedToLines, true);
      checkedListBox1.Items.Add(sLinesWithSameFromTo, true);
      checkedListBox1.Items.Add(sLinesThatDoNotBelongToAParcel, true);
      checkedListBox1.Items.Add(sLinePointsReferenceMissingPoint,true);
      checkedListBox1.Items.Add(sParcelsThatHaveNoLines, true);
      checkedListBox1.Items.Add(sInvalidFeatureAdjustmentVectors, true);
      checkedListBox1.Items.Add(sInvalidFeatureClassAssocs, true);
    }

    private void btnSaveReport_Click(object sender, EventArgs e)
    {
      //define the file that needs to be saved
      // Display .Net dialog for File saving.
      SaveFileDialog saveFileDialog = new SaveFileDialog();
      // Set File Filter
      saveFileDialog.Filter = "Text (*.txt)|*.txt|All Files|*.*";
      saveFileDialog.FilterIndex = 1;
      saveFileDialog.RestoreDirectory = true;
      // Warn on overwrite
      saveFileDialog.OverwritePrompt = true;
      // Don't need to Show Help
      saveFileDialog.ShowHelp = false;
      // Set Dialog Title
      saveFileDialog.Title = "Save Report as File";

      // Display Open File Dialog
      if (saveFileDialog.ShowDialog() != DialogResult.OK)
      {
        saveFileDialog = null;
        return;
      }
      TextWriter tw = null;
      try
      {
        tw = new StreamWriter(saveFileDialog.FileName);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        return;
      }

      //Start writing the report based on the checked boxes
      DateTime localNow = DateTime.Now;
      string sTime = "Date: " + Convert.ToString(localNow);
      string sDashLine = "--------------------------------------------------------------------";
      string sRecordCountSuffix = " inconsistent records)";
      tw.WriteLine(sTime);
      tw.WriteLine("Process time: " + m_ProcessTime);
      tw.WriteLine("");
      tw.WriteLine("Fabric source:");
      tw.WriteLine(m_FabricName);
      tw.WriteLine(sDashLine);
      tw.WriteLine("TABLE ROW COUNT TOTALS SUMMARY");
      tw.WriteLine(sDashLine);
      tw.WriteLine("Plans: " + m_PlanCount.ToString());
      tw.WriteLine("Control: " + m_ControlPointCount.ToString());
      tw.WriteLine("Parcels: " + m_ParcelCount.ToString());
      tw.WriteLine("Lines: " + m_LineCount.ToString());
      tw.WriteLine("Points: " + m_PointCount.ToString());
      tw.WriteLine("LinePoints: " + m_LinePointCount.ToString());
      tw.WriteLine("JobObjects: " + m_JobObjectCount.ToString());
      tw.WriteLine("Jobs: " + m_JobCount.ToString());
      tw.WriteLine("Accuracy: " + m_AccuracyCount.ToString());
      tw.WriteLine("Vectors: " + m_VectorCount.ToString());
      tw.WriteLine("Adjustments: " + m_AdjustmentsCount.ToString());
      tw.WriteLine("");
      tw.WriteLine(sDashLine);
      tw.WriteLine("INCONSISTENT FABRIC RECORDS REPORT");
      tw.WriteLine(sDashLine);

      try
      {
        //now build a report
        if (m_Check4PointsNotConnectedToLines)
        {
          tw.WriteLine(" ");
          tw.WriteLine(sPointsNotConnectedToLines + " (" + m_OrphanPointsList.Count().ToString() + sRecordCountSuffix);
          tw.WriteLine(sDashLine);
          WriteToFile(tw, m_OrphanPointsList);
        }

        if (m_Check4LinesWithSameFromAndTo)
        {
          tw.WriteLine(" ");
          tw.WriteLine(sLinesWithSameFromTo + " (" + m_LinesWithSameFromTo.Count().ToString() + sRecordCountSuffix);
          tw.WriteLine(sDashLine);
          WriteToFile(tw, m_LinesWithSameFromTo);
        }

        if (m_Check4LinesNotPartOfParcels)
        {
          tw.WriteLine(" ");
          tw.WriteLine(sLinesThatDoNotBelongToAParcel + " (" + m_OrphanLinesList.Count().ToString() + sRecordCountSuffix);
          tw.WriteLine(sDashLine);
          WriteToFile(tw, m_OrphanLinesList);
        }

        if (m_Check4LinePointsWithNoFabricPoints)
        {
          tw.WriteLine(" ");
          tw.WriteLine(sLinePointsReferenceMissingPoint + " (" + m_OrphanLinePointsList.Count().ToString() + sRecordCountSuffix);
          tw.WriteLine(sDashLine);
          WriteToFile(tw, m_OrphanLinePointsList);
        }

        if (m_Check4ParcelsHaveNoLines)
        {
          tw.WriteLine(" ");
          tw.WriteLine(sParcelsThatHaveNoLines + " (" + m_ParcelsWithNoLinesList.Count().ToString() + sRecordCountSuffix);
          tw.WriteLine(sDashLine);
          WriteToFile(tw, m_ParcelsWithNoLinesList);
        }

        if (m_Check4InvalidFeatureAdjustmentVectors)
        {
          tw.WriteLine(" ");
          tw.WriteLine(sInvalidFeatureAdjustmentVectors + " (" + m_InvalidVectors.Count().ToString() + sRecordCountSuffix);
          tw.WriteLine(sDashLine);
          WriteToFile(tw, m_InvalidVectors);
        }

        if (m_Check4InvalidFeatureClassAssociations)
        {
          tw.WriteLine(" ");
          tw.WriteLine(sInvalidFeatureClassAssocs + " (" + m_InvalidFeatureClassAssociations.Count().ToString() + sRecordCountSuffix);
          tw.WriteLine(sDashLine);
          WriteToFile(tw, m_InvalidFeatureClassAssociations);
        }

      }

      catch (Exception ex)
      {
        MessageBox.Show(ex.Message + ": " + Convert.ToString(ex.Source));
      }

      tw.Close();
      saveFileDialog = null;

    }

    private void WriteToFile(TextWriter tw, List<int> theList)
    {
      int iToken = 995;
      int iCounter = 0;
      int iChunkCount = (int)Math.Floor((double)theList.Count() / (double)iToken);

      if (theList.Count() > 0)
      {
        tw.WriteLine("In clause for ObjectIDs:");
        string sFileDump = m_OIDFieldName +" IN (";

        foreach (int i in theList)
        {
          sFileDump += i.ToString() + ",";
          iCounter++;
          if (iCounter >= iToken)
          {
            int iPos = sFileDump.LastIndexOf(",");
            sFileDump = sFileDump.Remove(iPos);
            sFileDump += ")";
            tw.WriteLine(sFileDump);
            tw.WriteLine("");
            sFileDump = m_OIDFieldName + " IN (";
            iCounter = 0;
          }
        }
        int iPos2 = sFileDump.LastIndexOf(",");
        sFileDump=sFileDump.Remove(iPos2);
        sFileDump += ")";
        tw.WriteLine(sFileDump);

      }
    }

    private void tvOrphanRecs_AfterSelect(object sender, TreeViewEventArgs e)
    {

    }

    private void btnSaveReport2_Click(object sender, EventArgs e)
    {
      //define the file that needs to be saved
      // Display .Net dialog for File saving.
      SaveFileDialog saveFileDialog = new SaveFileDialog();
      // Set File Filter
      saveFileDialog.Filter = "Text (*.txt)|*.txt|All Files|*.*";
      saveFileDialog.FilterIndex = 1;
      saveFileDialog.RestoreDirectory = true;
      // Warn on overwrite
      saveFileDialog.OverwritePrompt = true;
      // Don't need to Show Help
      saveFileDialog.ShowHelp = false;
      // Set Dialog Title
      saveFileDialog.Title = "Save Report as File";

      // Display Open File Dialog
      if (saveFileDialog.ShowDialog() != DialogResult.OK)
      {
        saveFileDialog = null;
        return;
      }
      TextWriter tw = null;
      try
      {
        tw = new StreamWriter(saveFileDialog.FileName);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        return;
      }

      //Start writing the report from the txt field
      DateTime localNow = DateTime.Now;
      string sTime = "Date: " + Convert.ToString(localNow);
      string sDashLine = "--------------------------------------------------------------------";
      tw.WriteLine(sTime);
      tw.WriteLine("Process time: " + m_ProcessTime);
      tw.WriteLine("");
      tw.WriteLine("Fabric source:");
      tw.WriteLine(m_FabricName);

      if (m_VersionName.Trim()!="")
      {
        tw.WriteLine("Version: " + m_VersionName);
      }

      tw.WriteLine(sDashLine);
      tw.WriteLine("INCONSISTENT FABRIC RECORDS REPORT");
      tw.WriteLine(sDashLine);

      try
      {
        //now build a report
        if (m_OrphanPointsList!=null)
        {
          string sRecordCountSuffix = " orphan point records)";
          tw.WriteLine(" ");
          tw.WriteLine(sPointsNotConnectedToLines + " (" + m_OrphanPointsList.Count().ToString() + sRecordCountSuffix);
          tw.WriteLine(sDashLine);
          WriteToFile(tw, m_OrphanPointsList);
        }

        if (m_ParcelsWithNoLinesList != null)
        {
          string sRecordCountSuffix = " parcel records)";
          tw.WriteLine(" ");
          tw.WriteLine(sParcelsThatHaveNoLines + " (" + m_ParcelsWithNoLinesList.Count().ToString() + sRecordCountSuffix);
          tw.WriteLine(sDashLine);
          WriteToFile(tw, m_ParcelsWithNoLinesList);
        }

        if (m_OrphanLinePointsList != null)
        {
          string sRecordCountSuffix = " invalid linepoint records)";
          tw.WriteLine(" ");
          tw.WriteLine(sLinePointsReferenceMissingPoint + " (" + m_OrphanLinePointsList.Count().ToString() + sRecordCountSuffix);
          tw.WriteLine(sDashLine);
          WriteToFile(tw, m_OrphanLinePointsList);
        }

        if (m_DisplacedLinePointsList != null)
        {
          string sRecordCountSuffix = " displaced linepoints)";
          tw.WriteLine(" ");
          tw.WriteLine(sDisplacedLinePoints + " (" + m_DisplacedLinePointsList.Count().ToString() + sRecordCountSuffix);
          tw.WriteLine(sDashLine);
          WriteToFile(tw, m_DisplacedLinePointsList);
        }

        if (m_OrphanLinesList != null)
        {
          string sRecordCountSuffix = " orphan line records)";
          tw.WriteLine(" ");
          tw.WriteLine(sLinesThatDoNotBelongToAParcel + " (" + m_OrphanLinesList.Count().ToString() + sRecordCountSuffix);
          tw.WriteLine(sDashLine);
          WriteToFile(tw, m_OrphanLinesList);
        }
        
        if (m_LinesWithSameFromTo != null)
        {
          string sRecordCountSuffix = " invalid line records)";
          tw.WriteLine(" ");
          tw.WriteLine(sLinesWithSameFromTo + " (" + m_LinesWithSameFromTo.Count().ToString() + sRecordCountSuffix);
          tw.WriteLine(sDashLine);
          WriteToFile(tw, m_LinesWithSameFromTo);
        }

        if (m_InvalidFeatureClassAssociations != null)
        {
          string sRecordCountSuffix = " invalid records)";
          tw.WriteLine(" ");
          tw.WriteLine(sInvalidFeatureClassAssocs + " (" + m_InvalidFeatureClassAssociations.Count().ToString() + sRecordCountSuffix);
          tw.WriteLine(sDashLine);
          WriteToFile(tw, m_InvalidFeatureClassAssociations);
        }

        if (m_InvalidVectors != null)
        {
          string sRecordCountSuffix = " invalid vector records)";
          tw.WriteLine(" ");
          tw.WriteLine(sInvalidFeatureAdjustmentVectors + " (" + m_InvalidVectors.Count().ToString() + sRecordCountSuffix);
          tw.WriteLine(sDashLine);
          WriteToFile(tw, m_InvalidVectors);
        }

      }

      catch (Exception ex)
      {
        MessageBox.Show(ex.Message + ": " + Convert.ToString(ex.Source));
      }

      tw.Close();
      saveFileDialog = null;
    }

    private void txtInClauseReport_MouseEnter(object sender, EventArgs e)
    {
      txtInClauseReport.Focus();
    }
  }
}
