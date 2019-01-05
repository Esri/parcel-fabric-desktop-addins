/*
 Copyright 1995-2019 Esri

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

//Add-in provided import library references
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
//Added Esri references
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.GeoSurvey;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseExtensions;

//Added non-Esri references
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;

namespace SampleParcelEditsViaPacket
{
  public class MergeParcels : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    public MergeParcels()
    {
    }

    protected override void OnClick()
    {

      IEditor pEd = (IEditor)ArcMap.Application.FindExtensionByName("esriEditor.Editor");
      if (pEd.EditState == esriEditState.esriStateNotEditing)
      {
        MessageBox.Show("Please start editing and try again.");
        return;
      }

      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByName("esriCadastralUI.CadastralEditorExtension");
      IParcelEditManager pParcEditorMan = (IParcelEditManager)pCadEd;
      ICadastralSelection pCadaSel = (ICadastralSelection)pCadEd;

      try
      {
        ICadastralPacketManager pCadPacketMan = (ICadastralPacketManager)pCadEd;
        bool bStartedWithPacketOpen = pCadPacketMan.PacketOpen;
        if (!bStartedWithPacketOpen)
          pEd.StartOperation();

        //1. Start map edit session
        ICadastralMapEdit pCadMapEdit = (ICadastralMapEdit)pCadEd;
        pCadMapEdit.StartMapEdit(esriMapEditType.esriMEParcelSelection, "Merge Parcel", false);

        //2.	Get job packet
        ICadastralPacket pCadaPacket = pCadPacketMan.JobPacket;

        //3.	Create Plan (new)
        string sPlanName = "My New Plan";

        //first check to ensure plan is not already in the database.
        IGSPlan pGSPlan = FindFabricPlanByName(sPlanName, pCadEd);
        ICadastralPlan pCadaPlan = (ICadastralPlan)pCadaPacket;
        if (pGSPlan == null)
        {
          //if plan is null, it was not found and can be created
          pGSPlan = new GSPlanClass();
          // 3.a set values
          pGSPlan.Accuracy = 4;
          pGSPlan.Name = sPlanName;
          pGSPlan.DirectionFormat = esriDirectionType.esriDTQuadrantBearing;
          pGSPlan.AngleUnits = esriDirectionUnits.esriDUDegreesMinutesSeconds;
          pGSPlan.AreaUnits = esriCadastralAreaUnits.esriCAUAcre;
          pGSPlan.Description = "My Test Plan for Merge";
          pGSPlan.DistanceUnits = esriCadastralDistanceUnits.esriCDUFoot;
        }

        //3.b Add the plan to the job packet
        pCadaPlan.AddPlan(pGSPlan);

        ICadastralPoints pCadaPoints = (ICadastralPoints)pCadaPacket;
        IConstructParcelFunctions3 constrParcelFunctions = new ParcelFunctions() as IConstructParcelFunctions3;

        IEnumGSParcels selectedParcels = pCadaSel.SelectedParcels; //get selected parcels AFTER calling ::StartMapEdit to get the in-mem packet representation.
        IGSParcel gsParcel = constrParcelFunctions.MergeParcels(pGSPlan, pCadaPoints, selectedParcels, pCadaPacket);
        gsParcel.Type = 7; //7 = Tax parcel in the Local Government Information Model
        gsParcel.Lot = "My Merged Tax Parcel";

        ICadastralObjectSetup pCadaObjSetup = (ICadastralObjectSetup)pParcEditorMan;
        //Make sure that any extended attributes on the new merged parcel have their default values set
        IGSAttributes pGSAttributes = (IGSAttributes)gsParcel;
        pCadaObjSetup.AddExtendedAttributes(pGSAttributes);
        pCadaObjSetup.SetDefaultValues(pGSAttributes);

        ICadastralParcel pCadaParcel = (ICadastralParcel)pCadaPacket;
        pCadaParcel.AddParcel(gsParcel);

        //set the original parcels to historic
        selectedParcels.Reset();
        IGSParcel selParcel = selectedParcels.Next();
        while (selParcel != null)
        {
          selParcel.Historical = true;
          //set SystemEndDate          
          pGSAttributes = (IGSAttributes)selParcel;
          if (pGSAttributes != null)
            pGSAttributes.SetProperty("systemenddate", DateTime.Now);
          selParcel = selectedParcels.Next();
        }

        try
        {
          pCadMapEdit.StopMapEdit(true);
        }
        catch
        {
          if (!bStartedWithPacketOpen)
            pEd.AbortOperation();
          return;
        }
        if (!bStartedWithPacketOpen)
          pEd.StopOperation("Merge Parcel");
        pCadPacketMan.PartialRefresh();
      }
      catch (Exception ex)
      {
        pEd.AbortOperation();
        MessageBox.Show(ex.Message);
      }

    }

    private IGSPlan FindFabricPlanByName(string PlanName, ICadastralEditor CadastralEditor)
    {
      ICursor pCur = null;
      try
      {
        ICadastralFabric pCadaFab = CadastralEditor.CadastralFabric;
        ITable pPlanTable = pCadaFab.get_CadastralTable(esriCadastralFabricTable.esriCFTPlans);
        int iPlanNameFldID = pPlanTable.FindField("NAME");
        string PlanNameFld = pPlanTable.Fields.get_Field(iPlanNameFldID).Name;
        IQueryFilter pQF = new QueryFilterClass();
        pQF.WhereClause = PlanNameFld + "= '" + PlanName + "'";
        pQF.SubFields = pPlanTable.OIDFieldName + PlanNameFld;
        pCur = pPlanTable.Search(pQF, false);
        IRow pPlanRow = pCur.NextRow();
        IGSPlan pGSPlanDB = null;
        IGSPlan pGSPlan = null;
        if (pPlanRow != null)
        {
          //Since plan was found, generate plan object from database:
          ICadastralFeatureGenerator pFeatureGenerator = new CadastralFeatureGeneratorClass();
          pGSPlanDB = pFeatureGenerator.CreatePlanFromRow(CadastralEditor, pPlanRow);
          //Hydrate the database plan as a new GSPlan in GeoSurvey Engine packet
          pGSPlan = new GSPlanClass();
          AssignGSPlan(pGSPlanDB, pGSPlan, true);
        }
        return pGSPlan;
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        return null;
      }
      finally
      {
      }
    }

    private void AssignGSPlan(IGSPlan pGSSrcPlan, IGSPlan pGSDestPlan, bool setDatabaseID)
    {
      if (pGSSrcPlan == null || pGSDestPlan == null)
        return;

      int lAccuracy, dbId;
      esriDirectionUnits eAngleUnits;
      esriCadastralAreaUnits eAreaUnits;
      esriDirectionType eDirectionFormat;
      esriCadastralDistanceUnits eDistanceUnits;
      esriCadastralLineParameters eLinePara;
      bool isAtGround, isLocked, isTrueMid, isInternal;
      double dblGridFactor;
      string bsCompany, bsDesc, bsName, bsSurveyor;
      object vLegalDate = DateTime.Now;
      object vSurveyDate = DateTime.Now;

      lAccuracy = pGSSrcPlan.Accuracy;
      eAngleUnits = pGSSrcPlan.AngleUnits;
      eAreaUnits = pGSSrcPlan.AreaUnits;
      dblGridFactor = pGSSrcPlan.CombinedGridFactor;
      bsCompany = pGSSrcPlan.Company;
      bsDesc = pGSSrcPlan.Description;
      eDirectionFormat = pGSSrcPlan.DirectionFormat;
      isAtGround = pGSSrcPlan.DistanceAtGround;
      eDistanceUnits = pGSSrcPlan.DistanceUnits;
      vLegalDate = pGSSrcPlan.LegalDate;
      eLinePara = pGSSrcPlan.LineParameters;
      isLocked = pGSSrcPlan.Locked;
      bsName = pGSSrcPlan.Name;
      vSurveyDate = pGSSrcPlan.SurveyDate;
      bsSurveyor = pGSSrcPlan.Surveyor;
      isTrueMid = pGSSrcPlan.TrueMid;
      isInternal = pGSSrcPlan.InternalAngles;
      dbId = pGSSrcPlan.DatabaseId;

      pGSDestPlan.Accuracy = lAccuracy;
      pGSDestPlan.AngleUnits = eAngleUnits;
      pGSDestPlan.AreaUnits = eAreaUnits;
      pGSDestPlan.CombinedGridFactor = dblGridFactor;
      pGSDestPlan.Company = bsCompany;
      pGSDestPlan.Description = bsDesc;
      pGSDestPlan.DirectionFormat = eDirectionFormat;
      pGSDestPlan.DistanceAtGround = isAtGround;
      pGSDestPlan.DistanceUnits = eDistanceUnits;
      pGSDestPlan.LegalDate = vLegalDate;
      pGSDestPlan.LineParameters = eLinePara;
      pGSDestPlan.Locked = isLocked;
      pGSDestPlan.Name = bsName;
      pGSDestPlan.SurveyDate = vSurveyDate;
      pGSDestPlan.Surveyor = bsSurveyor;
      pGSDestPlan.TrueMid = isTrueMid;
      pGSDestPlan.InternalAngles = isInternal;
      if (setDatabaseID)
        pGSDestPlan.DatabaseId = dbId;

      AssignExtendedAttributes(pGSSrcPlan as IGSAttributes, pGSDestPlan as IGSAttributes);
    }

    private void AssignExtendedAttributes(IGSAttributes pSourceAttributes, IGSAttributes pTargetAttributes)
    {
      if (pSourceAttributes == null || pTargetAttributes == null)
        return;

      IEnumBSTR pAttributeNames;
      pAttributeNames = pSourceAttributes.AttributeNames;
      if (pAttributeNames == null)
        return;

      string name = pAttributeNames.Next();
      while (name != null)
      {
        try
        {
          object valu = pSourceAttributes.GetProperty(name);
          pTargetAttributes.SetProperty(name, valu);
        }
        catch
        {
          ;
        }

        name = pAttributeNames.Next();
      }

    }

    protected override void OnUpdate()
    {
      Enabled = ArcMap.Application != null;
    }
  }
}
