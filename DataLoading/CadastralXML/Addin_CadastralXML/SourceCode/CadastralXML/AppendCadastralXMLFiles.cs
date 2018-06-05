using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.Geometry;
//using ESRI.ArcGIS.Cadastral;
using ESRI.ArcGIS.CadastralUI;
//using ESRI.ArcGIS.GeoSurvey;
using ESRI.ArcGIS.Editor;
//using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml;

namespace CadastralXML
{
  public class AppendCadastralXMLFiles : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    Dictionary<int, String> errorCodeDict = new Dictionary<int, String>();
    public AppendCadastralXMLFiles()
    {//create a dictionary of error codes for cadastral job related activities.
      errorCodeDict.Add(-2147212278, "Job has already been committed.");
      errorCodeDict.Add(-2147212277, "Job not found.");
      errorCodeDict.Add(-2147212269, "Parcel feature is part of a job that is currently being edited.");
      errorCodeDict.Add(-2147212268, "Source datum does not match the fabric datum.");
      errorCodeDict.Add(-2147212271, "The version of XML cannot be loaded.");
      errorCodeDict.Add(-2147212274, "The specified cadastral job does not belong to the current fabric.");
      errorCodeDict.Add(-2147212284, "A job with the specified name already exists.");
      errorCodeDict.Add(-2147212283, "The status of the job is invalid for this procedure.");
      errorCodeDict.Add(-2147212282, "Schema error. Required fields are missing.");
      errorCodeDict.Add(-2147212281, "Lock already exists for cadastral feature.");
      errorCodeDict.Add(-2147212261, "Append Cadastral XML - Process cancelled."); //FIDs must not be negative.
    }

    protected override void OnClick()
    {
      //go get a traverse file
      // Display .Net dialog for File selection.
      OpenFileDialog openFileDialog = new OpenFileDialog();
      // Set File Filter
      openFileDialog.Filter = "Cadastral XML file (*.xml)|*.xml";
      // Enable multi-select
      openFileDialog.Multiselect = true;
      // Don't need to Show Help
      openFileDialog.ShowHelp = false;
      // Set Dialog Title
      openFileDialog.Title = "Append Cadastral XML files";
      openFileDialog.FilterIndex = 2;
      // Display Open File Dialog
      if (openFileDialog.ShowDialog() != DialogResult.OK)
      {
        openFileDialog = null;
        return;
      }

      //Get the cadastral editor
      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByName("esriCadastralUI.CadastralEditorExtension");
      ICadastralFabric pCadFabric = pCadEd.CadastralFabric;
      IEditor pEd = ArcMap.Editor;

      ITrackCancel pTrkCan = new CancelTracker();
      // Create and display the Progress Dialog
      IProgressDialogFactory pProDlgFact = new ProgressDialogFactory();
      IProgressDialog2 pProDlg = pProDlgFact.Create(pTrkCan, 0) as IProgressDialog2;
      //Set the properties of the Progress Dialog
      pProDlg.CancelEnabled = true;
      pProDlg.Description = "    ";
      pProDlg.Title = "Append";
      pProDlg.Animation = esriProgressAnimationTypes.esriProgressGlobe;
      string sCopiedCadastralXMLFile = "";

      try
      {

        ICadastralJob CadaJob;
        bool bJobExists = false; //used to trap for the special error message condition of existing job
        pEd.StartOperation();

        #region workaround for setting Projection
        ICadastralJob CadaJobTemp;
        bJobExists = false;
        if (!CreateCadastralJob(pCadFabric, "TEMPJOB", out CadaJobTemp, true, ref bJobExists))
        {
          if (!bJobExists) // if the create job failed for some other reason than it already exists, then bail out.
          {
            MessageBox.Show("Job could not be created.");
            return;
          }
        }
        //do an extract to set the spatial reference: bug workaround
        ((ICadastralFabric3)pCadFabric).ExtractCadastralPacket(CadaJobTemp.Name, ArcMap.Document.ActiveView.FocusMap.SpatialReference as IProjectedCoordinateSystem, null, true);
        #endregion

        //make a temporary file for the edited cadastral XML that is used for the fabric update
        string sTempPath = System.IO.Path.GetTempPath();
        sCopiedCadastralXMLFile = System.IO.Path.Combine(sTempPath, "LastUpdatedCadastralXMLAppendedFromBatch.xml");
        int iFileCount = 0;
        int iTotalFiles = openFileDialog.FileNames.GetLength(0);

        List<string> lstPlans = new List<string>();
        #region get all plan names for all files
        foreach (String CadastralXMLPath in openFileDialog.FileNames)
        {

          TextReader tr = null;
          try
          {
            tr = new StreamReader(CadastralXMLPath);
          }
          catch (Exception ex)
          {
            MessageBox.Show(ex.Message);
            return;
          }

          //          string[] sFileLine = new string[0]; //define as dynamic array 
          string sLine = "";
          int iCount = 0;
          bool bInPlanData = false;
          bool bInParcel = false;

          //fill the array with the lines from the file
          while (sLine != null)
          {
            sLine = tr.ReadLine();
            try
            {
              if (sLine.Trim().Length >= 1) //test for empty lines
              {
                if (!bInParcel && !bInPlanData)
                  bInPlanData = sLine.Contains("<plan>");
                
                if (bInPlanData && sLine.Contains("<name>") && sLine.Contains("</name>"))
                {
                  string sPlanName = sLine.Replace("<name>", "").Replace("</name>", "").Replace("\t","").Trim();
                  if (!lstPlans.Contains(sPlanName))
                    lstPlans.Add(sPlanName);
                  bInPlanData = false;
                }
              }
              iCount++;

            }
            catch { }

          }
          tr.Close(); //close the file and release resources
        }
        string sInClause = "";
        foreach (string sPlan in lstPlans)
          sInClause += "'" + sPlan + "'" + ",";

        sInClause=sInClause.Remove(sInClause.LastIndexOf(","),1);

        ITable pPlansTable = pCadFabric.get_CadastralTable(esriCadastralFabricTable.esriCFTPlans);
        int iNameFld = pPlansTable.FindField("Name");
        IQueryFilter pQuFilter = new QueryFilterClass();
        pQuFilter.WhereClause = "NAME IN (" + sInClause + ")";
        ICursor pCur = pPlansTable.Search(pQuFilter, false);
        List<string> lstPlanReplace = new List<string>();

        IRow pPlan = pCur.NextRow();
        while (pPlan != null)
        {
          lstPlanReplace.Add("<name>" + (string)pPlan.Value[iNameFld] + "</name>\n\t\t<oID>" + pPlan.OID.ToString() + "</oID>");
          Marshal.ReleaseComObject(pPlan);
          pPlan = pCur.NextRow();
        }
        Marshal.ReleaseComObject(pCur);

        #endregion

        foreach (String CadastralXMLPath in openFileDialog.FileNames)
        {
          if (!pTrkCan.Continue())
          {
            pEd.AbortOperation();
            return;
          }

          //rename ALL oID tags so that they're ignored. This indicates that these are to be treated as NEW parcels coming in.
          ReplaceInFile(CadastralXMLPath, sCopiedCadastralXMLFile, "oID>", "old_xxX>");

          foreach (string sPlanTag in lstPlanReplace)
          {
            string sFirstPart = sPlanTag.Substring(0, sPlanTag.IndexOf("\n"));
            ReplaceInFile(sCopiedCadastralXMLFile, sCopiedCadastralXMLFile, sFirstPart, sPlanTag);
          }

          //TEST ONLY
          //ReplaceInFile(CadastralXMLPath, sCopiedCadastralXMLFile, "oID>", "oID>");

          //IF using PostCadastralPacket the points are not all merged. If using PostCadastralPacket, then a merge-point workaround would be to analyze coordinates 
          //of incoming file and if any are identical to existing fabric coordinates, then make the oID tag match 
          //the oID of the existing point in the target fabric. This will trigger point merging of identical points when using PostCadastralPacket.
          //code below uses InsertCadastralPacket and so point merging is handled.

          IXMLStream pStream = new XMLStream();
          pStream.LoadFromFile(sCopiedCadastralXMLFile);
          IFIDSet pFIDSet = null;//new FIDSet();

          DateTime localNow = DateTime.Now;
          string sJobName = Convert.ToString(localNow) + "_" + (iFileCount++).ToString();
          // note the create option is to NOT write job to fabric and is in-memory only initially, and then the InsertCadastralPacket will create and store the job in the fabric
          // IF using PostCadastralPacket, then this option should be set to true, so that the job already exists in the fabric.
          // Also, when using PostCadastralPacket, you can continue to use the same Target job, and a new one is not needed for each iteration.
          // With InsertCadastralPacket a job is created for each call when new parcels are being created.
          if (!CreateCadastralJob(pCadFabric, sJobName, out CadaJob, false, ref bJobExists))
          {
            pEd.AbortOperation();
            return;
          }

          pProDlg.Description = "File: " + System.IO.Path.GetFileName(CadastralXMLPath) + " (" + iFileCount.ToString() + " of " + iTotalFiles.ToString() + ")";
          //(pCadFabric as ICadastralFabric3).PostCadastralPacket(pStream, pTrkCan, esriCadastralPacketSetting.esriCadastralPacketNoSetting, ref pFIDSet);
          (pCadFabric as ICadastralFabric3).InsertCadastralPacket(CadaJob, pStream, pTrkCan, esriCadastralPacketSetting.esriCadastralPacketNoSetting, ref pFIDSet);
          //int setCnt = pFIDSet.Count();
        }
        RefreshFabricLayers(ArcMap.Document.ActiveView.FocusMap, pCadFabric);
        pEd.StopOperation("Append " + iFileCount.ToString() + " cadastral XML files");
      }
      catch (Exception ex)
      {
        pEd.AbortOperation();
        COMException c_Ex;
        int errorCode = 0;
        if (ex is COMException)
        {
          c_Ex = (COMException)ex;
          errorCode = c_Ex.ErrorCode;
        }
        if (errorCodeDict.ContainsKey(errorCode))
          MessageBox.Show(errorCodeDict[errorCode],"Append files");
        else
          MessageBox.Show("Error: " + ex.Message + Environment.NewLine + ex.HResult.ToString(),"Append files");
      }
      finally
      {
        if (sCopiedCadastralXMLFile != string.Empty)
          File.Delete(sCopiedCadastralXMLFile);
        if (pProDlg != null)
          pProDlg.HideDialog();
      }
    }


    public IFIDSet AppendCadastralXML(ICadastralFabric Fabric, ICadastralJob CadastralJob, IProjectedCoordinateSystem TargetProjectedCoordinateSystem, string CadastralXMLPath)
    {
      try
      {
        string sTempPath = System.IO.Path.GetTempPath();
        string sCopiedCadastralXMLFile = System.IO.Path.Combine(sTempPath, CadastralJob.Name.Replace('/', '_').Replace(':', '_') + ".xml");

        //rename ALL oID tags so that they're ignored
        ReplaceInFile(CadastralXMLPath, sCopiedCadastralXMLFile, "oID>", "old_xxX>");

        //Possible TODO for merge-point workaround: analyze coordinates of incoming file and if any are identical to existing fabric coordinates, then make the oID tag match 
        //the oID of the existing point in the target fabric. This will trigger point merging of identical points

        ITrackCancel pTrkCan = new CancelTracker();
        // Create and display the Progress Dialog
        IProgressDialogFactory pProDlgFact = new ProgressDialogFactory();
        IProgressDialog2 pProDlg = pProDlgFact.Create(pTrkCan, 0) as IProgressDialog2;
        //Set the properties of the Progress Dialog
        pProDlg.CancelEnabled = false;
        pProDlg.Description = "    ";
        pProDlg.Title = "Append";
        pProDlg.Animation = esriProgressAnimationTypes.esriProgressGlobe;

        //        do an extract to set the spatial reference
        ((ICadastralFabric3)Fabric).ExtractCadastralPacket(CadastralJob.Name, TargetProjectedCoordinateSystem, null, true);



        IXMLStream pStream = new XMLStream();
        pStream.LoadFromFile(sCopiedCadastralXMLFile);
        IFIDSet pFIDSet = null;//new FIDSet();

        (Fabric as ICadastralFabric3).PostCadastralPacket(pStream, pTrkCan, esriCadastralPacketSetting.esriCadastralPacketNoSetting, ref pFIDSet);
        //(Fabric as ICadastralFabric3).InsertCadastralPacket(CadastralJob, pStream, pTrkCan, esriCadastralPacketSetting.esriCadastralPacketNoSetting, ref pFIDSet);
        int setCnt = pFIDSet.Count();

        RefreshFabricLayers(ArcMap.Document.ActiveView.FocusMap, Fabric);
        File.Delete(sCopiedCadastralXMLFile);

        if (pProDlg != null)
          pProDlg.HideDialog();

        return pFIDSet;

      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        return null;
      }
    }

    public static bool CreateCadastralJob(ICadastralFabric Fabric, string JobName, out ICadastralJob NewCadastralJob, bool WriteToFabric, ref bool JobExists)
    {
      try
      {
        JobExists = false; //first assume it does not exist
        //Create a job.
        ICadastralJob pJob = new CadastralJobClass();
        pJob.Name = JobName;
        pJob.Owner = System.Windows.Forms.SystemInformation.UserName;
        pJob.Description = "Append Cadastral XML";
        int jobId;
        NewCadastralJob = null;

        if (WriteToFabric)
        {
          try
          {
            jobId = Fabric.CreateJob(pJob);
            NewCadastralJob = pJob;
          }
          catch (COMException ex)
          {
            if (ex.ErrorCode == (int)fdoError.FDO_E_CADASTRAL_FABRIC_JOB_ALREADY_EXISTS)
            {
              //now set the job variable to existing job
              NewCadastralJob = Fabric.GetJob(JobName);
              JobExists = true;
            }
            return false;
          }
        }
        else
          NewCadastralJob = pJob;

        return true;
      }
      catch (Exception ex)
      {
        //create a dictionary of error codes for cadastral job related activities.
        Dictionary<int, String> errorCodeDict = new Dictionary<int, String>();   
        errorCodeDict.Add(-2147212278, "Job has already been committed.");
        errorCodeDict.Add(-2147212277, "Job not found.");
        errorCodeDict.Add(-2147212269, "Parcel feature is part of a job that is currently being edited.");
        errorCodeDict.Add(-2147212268, "Source datum does not match the fabric datum.");
        errorCodeDict.Add(-2147212271, "The version of XML cannot be loaded.");
        errorCodeDict.Add(-2147212274, "The specified cadastral job does not belong to the current fabric.");
        errorCodeDict.Add(-2147212284, "A job with the specified name already exists.");
        errorCodeDict.Add(-2147212283, "The status of the job is invalid for this procedure.");
        errorCodeDict.Add(-2147212282, "Schema error. Required fields are missing.");
        errorCodeDict.Add(-2147212281, "Lock already exists for cadastral feature.");
        COMException c_Ex = (COMException)ex;
        NewCadastralJob = null;
        MessageBox.Show(ex.Message + Environment.NewLine + errorCodeDict[c_Ex.ErrorCode]);
        return false;
      }
    }

    static public void ReplaceInFile(string inFilePath, string outFilePath, string searchText, string replaceText)
    {
      StreamReader reader = new StreamReader(inFilePath);
      string content = reader.ReadToEnd();
      reader.Close();

      content = Regex.Replace(content, searchText, replaceText);

      StreamWriter writer = new StreamWriter(outFilePath);
      writer.Write(content);
      writer.Close();
    }



    static List<CadastralXMLPoints> ReadCoordinates(string fileName)
    {
      return ReadObjects<CadastralXMLPoints>(fileName);
    }

    static List<T> ReadObjects<T>(string fileName)
    {
      var list = new List<T>();

      var serializer = new XmlSerializer(typeof(T));
      var settings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment };
      using (var textReader = new StreamReader(fileName))
      using (var xmlTextReader = XmlReader.Create(textReader, settings))
      {
        while (xmlTextReader.Read())
        {   // Skip whitespace
          if (xmlTextReader.NodeType == XmlNodeType.Element)
          {
            using (var subReader = xmlTextReader.ReadSubtree())
            {
              string x = xmlTextReader.ReadElementContentAsString();
              var CadastralXMLPoint = (T)serializer.Deserialize(subReader);
              list.Add(CadastralXMLPoint);
            }
          }
        }
      }

      return list;
    }




    protected override void OnUpdate()
    {
      Enabled = ArcMap.Application != null && CustomizeHelper.CommandIsEnabled;
    }

    public void RefreshFabricLayers(IMap Map, ICadastralFabric Fabric)
    {
      IArray CFParcelLyrs;
      IFeatureLayer CFPtLyr;
      IFeatureLayer CFLineLyr;
      IFeatureLayer CFCtrlLyr;
      IFeatureLayer CFLinePtLyr;

      if (!GetFabricSubLayersFromFabric(Map, Fabric, out CFPtLyr, out CFLineLyr,
       out CFParcelLyrs, out CFCtrlLyr, out CFLinePtLyr))
        return;
      else
        RefreshMap(ArcMap.Document.ActiveView, CFParcelLyrs, CFPtLyr, CFLineLyr, CFCtrlLyr, CFLinePtLyr);
    }

    private bool GetFabricSubLayersFromFabric(IMap Map, ICadastralFabric Fabric, out IFeatureLayer CFPointLayer, out IFeatureLayer CFLineLayer,
  out IArray CFParcelLayers, out IFeatureLayer CFControlLayer, out IFeatureLayer CFLinePointLayer)
    {
      ICadastralFabricLayer pCFLayer = null;
      ICadastralFabricSubLayer pCFSubLyr = null;
      ICompositeLayer pCompLyr = null;
      IArray CFParcelLayers2 = new ArrayClass();

      IDataset pDS = (IDataset)Fabric;
      IName pDSName = pDS.FullName;
      string FabricNameString = pDSName.NameString;

      long layerCount = Map.LayerCount;
      CFPointLayer = null; CFLineLayer = null; CFControlLayer = null; CFLinePointLayer = null;
      IFeatureLayer pParcelLayer = null;
      for (int idx = 0; idx <= (layerCount - 1); idx++)
      {
        ILayer pLayer = Map.get_Layer(idx);
        bool bIsComposite = false;
        if (pLayer is ICompositeLayer)
        {
          pCompLyr = (ICompositeLayer)pLayer;
          bIsComposite = true;
        }

        int iCompositeLyrCnt = 1;
        if (bIsComposite)
          iCompositeLyrCnt = pCompLyr.Count;

        for (int i = 0; i <= (iCompositeLyrCnt - 1); i++)
        {
          if (bIsComposite)
            pLayer = pCompLyr.get_Layer(i);
          if (pLayer is ICadastralFabricLayer)
          {
            pCFLayer = (ICadastralFabricLayer)pLayer;
            break;
          }
          if (pLayer is ICadastralFabricSubLayer)
          {
            pCFSubLyr = (ICadastralFabricSubLayer)pLayer;
            IDataset pDS2 = (IDataset)pCFSubLyr.CadastralFabric;
            IName pDSName2 = pDS2.FullName;
            if (pDSName.NameString.ToLower() == pDSName2.NameString.ToLower() &&
            pCFSubLyr.CadastralTableType == esriCadastralFabricTable.esriCFTParcels)
            {
              pParcelLayer = (IFeatureLayer)pCFSubLyr;
              CFParcelLayers2.Add(pParcelLayer);
            }
            if (CFLineLayer == null && pDSName.NameString.ToLower() == pDSName2.NameString.ToLower() &&
            pCFSubLyr.CadastralTableType == esriCadastralFabricTable.esriCFTLines)
              CFLineLayer = (IFeatureLayer)pCFSubLyr;
            if (CFPointLayer == null && pDSName.NameString.ToLower() == pDSName2.NameString.ToLower() &&
            pCFSubLyr.CadastralTableType == esriCadastralFabricTable.esriCFTPoints)
              CFPointLayer = (IFeatureLayer)pCFSubLyr;
            if (CFLinePointLayer == null && pDSName.NameString.ToLower() == pDSName2.NameString.ToLower() &&
            pCFSubLyr.CadastralTableType == esriCadastralFabricTable.esriCFTLinePoints)
              CFLinePointLayer = (IFeatureLayer)pCFSubLyr;
            if (CFControlLayer == null && pDSName.NameString.ToLower() == pDSName2.NameString.ToLower() &&
            pCFSubLyr.CadastralTableType == esriCadastralFabricTable.esriCFTControl)
              CFControlLayer = (IFeatureLayer)pCFSubLyr;
          }
        }

        //Check that the fabric layer belongs to the requested fabric
        if (pCFLayer != null)
        {
          if (pCFLayer.CadastralFabric.Equals(Fabric))
          {
            CFPointLayer = (IFeatureLayer)pCFLayer.get_CadastralSubLayer(esriCadastralFabricRenderer.esriCFRPoints);
            CFLineLayer = (IFeatureLayer)pCFLayer.get_CadastralSubLayer(esriCadastralFabricRenderer.esriCFRLines);
            pParcelLayer = (IFeatureLayer)pCFLayer.get_CadastralSubLayer(esriCadastralFabricRenderer.esriCFRParcels);
            CFParcelLayers2.Add(pParcelLayer);
            CFControlLayer = (IFeatureLayer)pCFLayer.get_CadastralSubLayer(esriCadastralFabricRenderer.esriCFRControlPoints);
            CFLinePointLayer = (IFeatureLayer)pCFLayer.get_CadastralSubLayer(esriCadastralFabricRenderer.esriCFRLinePoints);
          }
          CFParcelLayers = CFParcelLayers2;
          return true;
        }
      }
      //at the minimum, just need to make sure we have a parcel sublayer for the requested fabric
      if (pParcelLayer != null)
      {
        CFParcelLayers = CFParcelLayers2;
        return true;
      }
      else
      {
        CFParcelLayers = null;
        return false;
      }
    }

    private void RefreshMap(IActiveView ActiveView, IArray ParcelLayers, IFeatureLayer PointLayer,
  IFeatureLayer LineLayer, IFeatureLayer ControlLayer, IFeatureLayer LinePointLayer)
    {
      try
      {
        for (int z = 0; z <= ParcelLayers.Count - 1; z++)
        {
          if (ParcelLayers.get_Element(z) != null)
          {
            IFeatureSelection pFeatSel = (IFeatureSelection)ParcelLayers.get_Element(z);
            pFeatSel.Clear();//refreshes the parcel explorer
            ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, ParcelLayers.get_Element(z), ActiveView.Extent);
            ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, ParcelLayers.get_Element(z), ActiveView.Extent);
          }
        }
        if (PointLayer != null)
          ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, PointLayer, ActiveView.Extent);
        if (LineLayer != null)
          ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, LineLayer, ActiveView.Extent);
        if (ControlLayer != null)
          ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, ControlLayer, ActiveView.Extent);
        if (LinePointLayer != null)
          ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, LinePointLayer, ActiveView.Extent);
      }
      catch
      { }
    }


  }




  public class CadastralXMLPoints
  {
    public int pointNo { get; set; }
    public int oID { get; set; }
    public double x { get; set; }
    public double y { get; set; }

    //[XmlElement("point")]

    [XmlIgnore]
    public double maxDE { get; set; }
    public double maxDN { get; set; }

    //public string X { get; set; }
    //public string Y { get; set; }


  }



}

