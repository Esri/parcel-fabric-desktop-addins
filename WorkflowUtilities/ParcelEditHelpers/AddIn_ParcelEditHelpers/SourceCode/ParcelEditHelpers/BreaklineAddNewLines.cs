/*
 Copyright 1995-2016 Esri

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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;

//Added non-Esri references
using System.Windows.Forms;
using System.Runtime.InteropServices;

//Added Esri references
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.GeoSurvey;
using ESRI.ArcGIS.esriSystem;

namespace ParcelEditHelper
{
  public class BreaklineAddNewLines : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    public BreaklineAddNewLines()
    {
    }

    protected override void OnClick()
    {
      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByName("esriCadastralUI.CadastralEditorExtension");
      IParcelEditManager pParcEditorMan = (IParcelEditManager)pCadEd;
      ICadastralExtensionManager pCadMan = pCadEd as ICadastralExtensionManager;
      ICadastralPacketManager pCadPacketMan = (ICadastralPacketManager)pCadEd;
      //bool bStartedWithPacketOpen = pCadPacketMan.PacketOpen;

      if (!(pCadMan.ContextItem is IGSLine))
        return;

      IGSLine pGSLine = pCadMan.ContextItem as IGSLine;

      if (pParcEditorMan == null)
        return;

      IEditor pEd = (IEditor)ArcMap.Application.FindExtensionByName("esri object editor");
      if (pEd.EditState == esriEditState.esriStateNotEditing)
      {
        MessageBox.Show("Please start editing and try again.");
        return;
      }

      IParcelConstruction pConstr = pParcEditorMan.ParcelConstruction;
      ICadastralLinePoints2Ex pCadLPsEx = pConstr as ICadastralLinePoints2Ex;

      ILongArray pLngArrBefore = pCadLPsEx.LinePoints;
      List<int> lstLPBefore = new List<int>();
      for (int i = 0; i < pLngArrBefore.Count; i++)
        lstLPBefore.Add(pLngArrBefore.get_Element(i));
      
      //first get the current set of breakpoints
      Utilities FabUTILS = new Utilities();
      FabUTILS.ExecuteCommand("{9987F18B-8CC4-4548-8C41-7DB51F289BB3}"); //Run COTS Breakline command

      ILongArray pLngArrAfter = pCadLPsEx.LinePoints;
      List<int> lstLPAfter = new List<int>();
      for (int i = 0; i < pLngArrAfter.Count; i++)
        lstLPAfter.Add(pLngArrAfter.get_Element(i));

      List<int> lstNewBreakPoints = lstLPAfter.Except(lstLPBefore).ToList();

      if (lstNewBreakPoints.Count == 0)
        return;

      IParcelConstruction4 pConstr4 = pConstr as IParcelConstruction4;
      ICadastralPoints pCadastralPts = pConstr4 as ICadastralPoints;

      IEnumCELines pCELines = new EnumCELinesClass();
      IEnumGSLines pEnumGSLines = (IEnumGSLines)pCELines;
      pCELines.Add(pGSLine);

      //check if it's a construction line or parent line
      bool bIsParentLine = true;
      IEnumGSLines pEnumGSConstructionLines = pConstr4.GetLines(false, false);

      IGSLine pGSTestLine = null;
      IGSParcel pGSParc = null;
      pEnumGSConstructionLines.Reset();
      pEnumGSConstructionLines.Next(ref pGSParc, ref pGSTestLine);
      while (pGSTestLine != null)
      {
        if ((pGSLine.FromPoint == pGSTestLine.FromPoint) && (pGSLine.ToPoint == pGSTestLine.ToPoint))
        {
          bIsParentLine = false;
          break;
        }
        pEnumGSConstructionLines.Next(ref pGSParc, ref pGSTestLine);
      }

      IParcelLineFunctions3 pParcLineFunctions = new ParcelFunctionsClass();
      IEnumGSLines pNewLinesEnum = pParcLineFunctions.BreakLinesAtLinePoints(pEnumGSLines, pCadastralPts, true, false);

      IGSParcel pGSParcel = null;
      IGSLine pGSNewLine = null;
      ICadastralUndoRedo pCadUndoRedo = pConstr as ICadastralUndoRedo;
      pCadUndoRedo.StartUndoRedoSession("Break-line");

      pNewLinesEnum.Reset();
      pNewLinesEnum.Next(ref pGSParcel, ref pGSNewLine);
      while (pGSNewLine != null)
      {
        pConstr4.InsertGridRow(-1, pGSNewLine, true);
        pNewLinesEnum.Next(ref pGSParcel, ref pGSNewLine);
      }

      ICadastralUnbuildableLines pUnbuildable = pConstr4 as ICadastralUnbuildableLines;
      if (bIsParentLine)
        pUnbuildable.AddLine(pGSLine);
      else
        pConstr4.Planarize(0.001); //delete the original construction line

      pCadUndoRedo.WriteUndoRedoSession(true);
    }

    protected override void OnUpdate()
    {
    }
  }
}
