using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.GeoSurvey;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseExtensions;

namespace ParcelEditHelper
{
  /// <summary>
  /// Designer class of the dockable window add-in. It contains user interfaces that
  /// make up the dockable window.
  /// </summary>
  public partial class AdjustmentDockWindow : UserControl
  {
    private static System.Windows.Forms.TextBox s_TxtToler;
    private static System.Windows.Forms.Button s_BtnRun;
    private static System.Windows.Forms.Button s_BtnSettings;
    private static System.Windows.Forms.CheckBox s_ChkUseLinePoints;
    private static System.Windows.Forms.Label s_LblDistUnits;

    private static bool s_enabled;

    public AdjustmentDockWindow(object hook)
    {
      InitializeComponent();
      this.Hook = hook;
      s_TxtToler = txtMainDistResReport;
      s_BtnRun = btnRun;
      s_BtnSettings=btnSettings;
      s_ChkUseLinePoints = chkUseLinePoints;
      s_LblDistUnits=lblUnits1;
      //read from registry
      Utilities FabUTILS = new Utilities();
      string sVersion = FabUTILS.GetDesktopVersionFromRegistry();
      string sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
     "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
     "LSADistanceToleranceReport", true);
      if (sVal.Trim() != "")
        this.txtMainDistResReport.Text = sVal;
      
      SetEnabled(false);

      s_LblDistUnits.Text="";
      FabUTILS=null;
    }

    internal static void SetEnabled(bool enabled)
    {
      s_enabled = enabled;
        Utilities FabUTILS = new Utilities();
        string sVersion = FabUTILS.GetDesktopVersionFromRegistry();
      // if the dockable window was never displayed, text box could be null
      if (s_TxtToler == null)
        return;
      s_TxtToler.Enabled= enabled;

      if(!enabled)
        s_TxtToler.Text="";
      else
      {
        string sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
        "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
        "LSADistanceToleranceReport", true);
        if (sVal.Trim() != "")
          s_TxtToler.Text= sVal;
      }

      if (s_BtnRun == null)
        return;
      s_BtnRun.Enabled = enabled;

      s_BtnSettings.Enabled = enabled;

      s_ChkUseLinePoints.Enabled=enabled;

      s_LblDistUnits.Visible=enabled;

      IActiveView pActView = ArcMap.Document.ActiveView;
      if (pActView!=null)
        s_LblDistUnits.Text = FabUTILS.UnitNameFromSpatialReference(pActView.FocusMap.SpatialReference);
      FabUTILS = null;

    }

    internal static void SetTextOnDistanceTolerance(string Tolerance)
    {
      // if the dockable window was never displayed, text box could be null
      if (s_TxtToler == null)
        return;
      s_TxtToler.Text= Tolerance;

    }

    private double m_iRepeatCount = 3;
    private double m_dConvergenceValue = 0.003;
    private double m_dDivergenceValue = 0.2;
    private int m_iReportType=1;
    private string m_sBrowseFilePath = "c:\\FabricLSAtemp.txt";

    private double m_dDistToleranceReport = 0.3;
    private double m_dBearingToleranceReport = 6000; 
    private double m_dLinePtsOffsetToleranceReport = 0.6;
    private double m_dClosePointsToleranceReport = 0.6;

    private bool m_bBendLines = false;
    private double m_dBendLinesTolerance = 3;
    private bool m_bIncludeDependentLines = false;
    private bool m_bSnapLinePointsToLines = false;
    private double m_dSnapLinePointTolerance = 0.3;
    private bool m_bStraightenRoadFrontages = false;
    private double m_dStraightRoadOffsetTolerance = 0.5;
    private double m_dStraightRoadAngleTolerance = 300;

    /// <summary>
    /// Host object of the dockable window
    /// </summary>
    /// 

    private object Hook
    {
      get;
      set;
    }

    /// <summary>
    /// Implementation class of the dockable window add-in. It is responsible for 
    /// creating and disposing the user interface class of the dockable window.
    /// </summary>
    public class AddinImpl : ESRI.ArcGIS.Desktop.AddIns.DockableWindow
    {
      private AdjustmentDockWindow m_windowUI;

      public AddinImpl()
      {
      }

      protected override IntPtr OnCreateChild()
      {
        m_windowUI = new AdjustmentDockWindow(this.Hook);
        return m_windowUI.Handle;
      }

      protected override void Dispose(bool disposing)
      {
        if (m_windowUI != null)
          m_windowUI.Dispose(disposing);

        base.Dispose(disposing);
      }

    }

    internal void ChangeControlStatus(bool status)
    {
      foreach (Control c in this.Controls)
      {
        foreach (Control ctrl in c.Controls)
          ctrl.Enabled = status;
      }
    }

    private void btnRun_Click(object sender, EventArgs e)
    {

      double dConvTol=0.003;
      int iRepeatCount=2;
      double dMaxShift = 0;

      double dVal=0;
      if(Double.TryParse(this.txtMainDistResReport.Text, out dVal))
      {
      //write to registry
        Utilities FabUTILS= new Utilities();
        string sVersion = FabUTILS.GetDesktopVersionFromRegistry();
        FabUTILS.WriteToRegistry(RegistryHive.CurrentUser,
        "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
        "LSADistanceToleranceReport", this.txtMainDistResReport.Text, true);
        FabUTILS=null;
      }

      bool bAdjResult=true;
      string sSummary="";

      #region Setup Adjustment & Verify that there's enough information
      
       ICadastralPacketManager pCadastralPacketManager = null;
       ICadastralAdjustment pCadAdj = null;
       ICadastralAdjustment3 pCadAdjEx = null;
       ICadastralMapEdit pCadMapEdit =null;

        LoadValuesFromRegistry();

        UID pUID = new UIDClass();
        pUID.Value = "{114D685F-99B7-4B63-B09F-6D1A41A4DDC1}";
        ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByCLSID(pUID);
        ICadastralFabric pCadFabric = pCadEd.CadastralFabric;

        pCadastralPacketManager = (ICadastralPacketManager)pCadEd;
        bool open = pCadastralPacketManager.PacketOpen;

        ISelectionSet pBeforeSS = null;
        IFeatureSelection pParcelSelection = null;

        if (!open)
        {
          ICadastralSelection pSelection = (ICadastralSelection)pCadEd;
          IEnumGSParcels pParcels = pSelection.SelectedParcels;
          IEnumCEParcels pCEParcels = (IEnumCEParcels)pParcels;
          if (pCEParcels != null)
          {
            long count = pCEParcels.Count;
            if (count == 0)
            {
              MessageBox.Show("There are no parcels selected to adjust." + Environment.NewLine
              + "Please select parcels and try again.", "Fabric Adjustment");
              return;
            }
          }

          ICadastralFabricLayer pCFLayer = pCadEd.CadastralFabricLayer;
          if (pCFLayer != null)
          {
            IFeatureLayer pParcelLayer = pCFLayer.get_CadastralSubLayer(esriCadastralFabricRenderer.esriCFRParcels);
            pParcelSelection = (IFeatureSelection)pParcelLayer;
            if (pParcelSelection != null)
              pBeforeSS = pParcelSelection.SelectionSet;
          }
        }

        pCadMapEdit = (ICadastralMapEdit)pCadEd;
        pCadMapEdit.StartMapEdit(esriMapEditType.esriMEParcelSelection, "Fabric Adjustment", false);

        ICadastralPacket pCadastralPacket = pCadastralPacketManager.JobPacket;
        int cpCount = 0;

        ICadastralControlPoints pCPts = (ICadastralControlPoints)pCadastralPacket;
        if (pCPts != null)
        {
          IGeometry pGeom = null;
          IEnumGSControlPoints pEnumCPs = pCPts.GetControlPoints(pGeom);
          pEnumCPs.Reset();
          IGSControlPoint pCP;
          if (pEnumCPs != null)
          {
            pCP = pEnumCPs.Next();
            while ((pCP != null) && (cpCount < 2))
            {
              if (pCP.Active)
                cpCount++;
            }
          }
        }

        if (cpCount < 2)
        {
          MessageBox.Show("Please make sure that at least 2 control points are" +
          Environment.NewLine + "attached to the selected parcels.", "Fabric Adjustment");
          return;
        }
        pCadAdj = (ICadastralAdjustment)pCadastralPacket;
        pCadAdjEx = (ICadastralAdjustment3)pCadastralPacket;
        ApplyAdjustmentSettings(pCadAdj, pCadAdjEx);
      
      #endregion

        double dHighestMaxShift = 0;

      //// Change display text depending on count
      //string itemText = count > 1 ? "items" : "item";

      for (int i =1; i<=iRepeatCount; i++)
      {
        if(!RunAdjustment(pCadastralPacketManager,pCadAdj,pCadAdjEx, i, out dMaxShift, out sSummary))
        {
          bAdjResult=false;
          break;
        }
        if(dHighestMaxShift > dMaxShift)
          dHighestMaxShift=dMaxShift;

        pCadAdj.AcceptAdjustment();
        if(dMaxShift<dConvTol)
          break;
      }

      if(bAdjResult)
        lblAdjResult.Text="Adjustment Complete";
      else
        lblAdjResult.Text="Adjustment Failed";

      lblAdjResult.Visible=true;

      dlgAdjustmentResults AdjResults = new dlgAdjustmentResults();
      AdjResults.txtReport.Text = sSummary;

      //Display the dialog
      DialogResult pDialogResult = AdjResults.ShowDialog();
      if (pDialogResult != DialogResult.OK)
      {
        AdjResults = null;
        pCadMapEdit.StopMapEdit(false);
      }
      else
      {
        pCadMapEdit.StopMapEdit(true);
      }

      pParcelSelection.SelectionSet = pBeforeSS;

      Utilities FabUTILS2= new Utilities();
      FabUTILS2.RefreshFabricLayers(ArcMap.Document.FocusMap,pCadFabric);
      FabUTILS2=null;

    }

    private void btnSettings_Click(object sender, EventArgs e)
    {
      CreateContextMenu(ArcMap.Application);
    }

    private void CreateContextMenu(IApplication application)
    {
      ICommandBars commandBars = application.Document.CommandBars;
      ICommandBar commandBar = commandBars.Create("TemporaryContextMenu", ESRI.ArcGIS.SystemUI.esriCmdBarType.esriCmdBarTypeShortcutMenu);

      System.Object optionalIndex = System.Type.Missing;
      UID uid = new UIDClass();

      uid.Value = ThisAddIn.IDs.AdjSettings.ToString(); //"esriArcMapUI.ZoomInFixedCommand"; // Can use CLSID or ProgID
      uid.SubType = 0;
      commandBar.Add(uid, ref optionalIndex);

      uid.Value = "{FBF8C3FB-0480-11D2-8D21-080009EE4E51}"; // Can use CLSID or ProgID
      uid.SubType = 1;
      commandBar.Add(uid, ref optionalIndex);

      uid.Value = "{FBF8C3FB-0480-11D2-8D21-080009EE4E51}"; // Can use CLSID or ProgID
      uid.SubType = 2;
      commandBar.Add(uid, ref optionalIndex);

      //Show the context menu at the current mouse location
      System.Drawing.Point currentLocation = System.Windows.Forms.Form.MousePosition;
      commandBar.Popup(currentLocation.X, currentLocation.Y);
    }

    private void ApplyAdjustmentSettings(ICadastralAdjustment pCadAdj,ICadastralAdjustment3 pCadAdjEx)
    {

      pCadAdj.DistanceTolerance=m_dDistToleranceReport;

      pCadAdj.BearingTolerance=m_dBearingToleranceReport;
      pCadAdj.ClosePointsTolerance=m_dClosePointsToleranceReport;

      pCadAdj.LinePointsTolerance = m_dLinePtsOffsetToleranceReport;

      if (m_iReportType == 0)
        pCadAdj.ListingDetailLevel = enumGSLSAListingDetailLevel.enumGSAdjustmentSimple;

      if (m_iReportType==1)
        pCadAdj.ListingDetailLevel = enumGSLSAListingDetailLevel.enumGSAdjustmentStandard;

      if (m_iReportType == 2)
        pCadAdj.ListingDetailLevel = enumGSLSAListingDetailLevel.enumGSAdjustmentExtended;

      //Main Line point Check-control
      pCadAdjEx.IncludeLinePoint = chkUseLinePoints.Checked;

      if (chkUseLinePoints.Checked)
      {
        pCadAdj.ForceLinePoints=m_bSnapLinePointsToLines;
        pCadAdj.ForceLinePointsTolerance=m_dSnapLinePointTolerance;

        pCadAdjEx.FlexLinesOutOfLinePointTolerance = m_bBendLines;
        pCadAdjEx.FlexLinePointTolerance = m_dBendLinesTolerance;
      }
      else
      {
        pCadAdj.ForceLinePoints = false;
        pCadAdj.ForceLinePointsTolerance = m_dSnapLinePointTolerance;
        pCadAdjEx.FlexLinesOutOfLinePointTolerance = false;
        pCadAdjEx.FlexLinePointTolerance = m_dBendLinesTolerance;
      }
      pCadAdj.ForceStraights=m_bStraightenRoadFrontages;
      pCadAdj.ForceStraightsAngleTolerance=m_dStraightRoadAngleTolerance;
      pCadAdj.ForceStraightsTolerance = m_dStraightRoadOffsetTolerance;
        
      pCadAdj.IncludeEasements=m_bIncludeDependentLines;
    }

    private bool RunAdjustment(ICadastralPacketManager pCadastralPacketManager,
      ICadastralAdjustment pCadAdj, ICadastralAdjustment3 pCadAdjEx, int Iteration, out double MaxShift, out string Summary)
    {
      esriCadastralDistanceUnits eDistUnits =esriCadastralDistanceUnits.esriCDUUSSurveyFoot;
      esriDirectionType eDirectType  =esriDirectionType.esriDTNorthAzimuth;
      esriCadastralAreaUnits eAreaUnits = esriCadastralAreaUnits.esriCAUAcre;

      ICadastralPlan pCadPlan = (ICadastralPlan)pCadAdj;
      IEnumGSPlans pGSPlans =pCadPlan.GetPlans(enumGSPlansType.enumGSPlansWithParcels);
      pGSPlans.Reset();
      IGSPlan pGSPlan=pGSPlans.Next();

      while (pGSPlan!=null)
      {
        eDistUnits = pGSPlan.DistanceUnits;
        eDirectType = pGSPlan.DirectionFormat;
        eAreaUnits = esriCadastralAreaUnits.esriCAUAcre;
        break; //just use the first one
      }

      double dMaxShift=-999;
      string sSummary="";
      MaxShift=dMaxShift;
      try
      {
        //get the units from the map plan
                
        sSummary=
        pCadAdj.PerformAdjustment(false,m_sBrowseFilePath,null,eDistUnits, eDirectType,eAreaUnits);

        double dMaxEast=-999;
        double dMaxNorth=-999;
        int iPt=-999;
        pCadAdjEx.GetMaxShiftData(ref iPt, ref dMaxEast, ref dMaxNorth);

        double dDistEast=-999;
        double dDistNorth=-999;

        IMetricUnitConverter pUnitConversion = (IMetricUnitConverter)pCadastralPacketManager;
        pUnitConversion.ConvertDistance(esriCadastralUnitConversionType.esriCUCFromMetric, 
        dMaxEast, ref dDistEast);
        pUnitConversion.ConvertDistance(esriCadastralUnitConversionType.esriCUCFromMetric, 
        dMaxNorth, ref dDistNorth);

        dMaxShift = Math.Sqrt((dDistEast * dDistEast) + (dDistNorth * dDistNorth));

        return true;
      }
      catch(Exception ex)
      {
        MessageBox.Show(ex.Message);
        return false;
      }

      finally
      {
        MaxShift = dMaxShift;
        Summary=sSummary;
      }
    }

    private void LoadValuesFromRegistry()
    {
      Utilities FabUTILS = new Utilities();
      try
      {
        string sVal ="";
        string sVersion = FabUTILS.GetDesktopVersionFromRegistry();
        sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
        "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
        "LSADistanceToleranceReport", false);
        if (sVal.Trim() != "")
          m_dDistToleranceReport = Convert.ToDouble(sVal);
        

        sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
        "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
        "LSABearingToleranceReport", false);
        if (sVal.Trim() != "")
          m_dBearingToleranceReport = Convert.ToDouble(sVal); 

        sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
        "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
        "LSALinePointsOffsetToleranceReport", false);
        if (sVal.Trim() != "")
          m_dLinePtsOffsetToleranceReport = Convert.ToDouble(sVal);

        sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
        "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
        "LSAClosePointsToleranceReport", false);
        if (sVal.Trim() != "")
          m_dClosePointsToleranceReport = Convert.ToDouble(sVal);

        sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
        "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
        "LSANumberRepeatCountIteration", false);
        if (sVal.Trim() != "")
          m_iRepeatCount = Convert.ToInt32(sVal);

        sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
        "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
        "LSAConvergenceValue", false);
        if (sVal.Trim() != "")
          m_dConvergenceValue = Convert.ToDouble(sVal);

        sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
        "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
        "LSADivergenceValue", false);
        if (sVal.Trim() != "")
          m_dDivergenceValue = Convert.ToDouble(sVal);

        sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
       "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
       "LSAReportType", false);
        if (sVal.Trim() != "")
          m_iReportType = Convert.ToInt32(sVal);

        sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
       "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
       "LSABrowseFilePath", false);
        if (sVal.Trim() != "")
          m_sBrowseFilePath = sVal;

        sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
       "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
       "LSAChkBendLinesOn", false);
        if (sVal.Trim() != "")
          m_bBendLines = ((sVal.ToUpper().Trim()) == "TRUE");

        sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
       "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
       "LSABendLinesTolerance", false);
        if (sVal.Trim() != "")
          m_dBendLinesTolerance = Convert.ToDouble(sVal);

        sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
       "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
       "LSAChkIncludeDependentLines", false);
        if (sVal.Trim() != "")
          m_bIncludeDependentLines = ((sVal.ToUpper().Trim()) == "TRUE");

        sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
       "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
       "LSAChkSnapLinePointsToLines", false);
        if (sVal.Trim() != "")
          m_bSnapLinePointsToLines = ((sVal.ToUpper().Trim()) == "TRUE");

        sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
       "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
       "LSASnapLinePtsToLinesTolerance", false);
        if (sVal.Trim() != "")
          m_dSnapLinePointTolerance = Convert.ToDouble(sVal);

        sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
       "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
       "LSAChkStraightenRoadFrontages", false);
        if (sVal.Trim() != "")
          m_bStraightenRoadFrontages = ((sVal.ToUpper().Trim()) == "TRUE");

        sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
       "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
       "LSAStraightenRoadFrontagesOffset", false);
        if (sVal.Trim() != "")
          m_dStraightRoadOffsetTolerance = Convert.ToDouble(sVal);

        sVal = FabUTILS.ReadFromRegistry(RegistryHive.CurrentUser,
       "Software\\ESRI\\Desktop" + sVersion + "\\ArcMap\\Cadastral\\AddIn.ParcelEditHelper",
       "LSAStraightenRoadFrontagesAngle", false);
        if (sVal.Trim() != "")
          m_dStraightRoadAngleTolerance = Convert.ToDouble(sVal);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Fabric Adjustment Settings");
      }
      finally
      {
        FabUTILS = null;
      }
    }

    private void button1_Click(object sender, EventArgs e)
    {

    }

    private void label1_Click(object sender, EventArgs e)
    {

    }

    private void panel1_Paint(object sender, PaintEventArgs e)
    {

    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      //http://desktop.arcgis.com/en/arcmap/latest/manage-data/editing-parcels/about-accuracy-in-the-parcel-fabric.htm
      Process.Start("http://desktop.arcgis.com/en/arcmap/latest/manage-data/editing-parcels/about-accuracy-in-the-parcel-fabric.htm");
    }

  }
}
