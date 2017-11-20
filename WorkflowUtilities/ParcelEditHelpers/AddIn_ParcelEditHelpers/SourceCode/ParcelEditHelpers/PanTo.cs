/*
 Copyright 1995-2017 Esri

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
using System.Text;
using System.IO;


//Added non-Esri references
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Linq;

//Added Esri references
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.CadastralUI;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.GeoSurvey;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;

namespace ParcelEditHelper
{
  public class PanTo : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    public PanTo()
    {
    }

    protected override void OnClick()
    {
      ICadastralEditor pCadEd = (ICadastralEditor)ArcMap.Application.FindExtensionByName("esriCadastralUI.CadastralEditorExtension");
      IParcelEditManager pParcEditorMan = (IParcelEditManager)pCadEd;

      if (pParcEditorMan == null)
        return;

      IEditor pEd = (IEditor)ArcMap.Application.FindExtensionByName("esri object editor");
      if (pEd.EditState == esriEditState.esriStateNotEditing)
      {
        MessageBox.Show("Please start editing and try again.");
        return;
      }

      IParcelConstruction pConstr = pParcEditorMan.ParcelConstruction;
      ICadastralPoints pCadastralPts = pConstr as ICadastralPoints;
      IGSPoint pFromPoint = null;
      IGSPoint pToPoint = null;
      IGSLine pParcelLine = null;
      List<double> xcoords= new List<double>();
      List<double> ycoords = new List<double>();
      double dX = 0; double dY = 0;
      bool bLineSelectionSequence = false;

      #region simple method as fall-back
      for (int i = 0; i < pConstr.LineCount; i++)
      {
        if (pConstr.GetLineSelection(i))
        {
          if (pConstr.GetLine(i, ref pParcelLine))
          {
            pFromPoint = pCadastralPts.GetPoint(pParcelLine.FromPoint);
            pToPoint = pCadastralPts.GetPoint(pParcelLine.ToPoint);
            xcoords.Add((pFromPoint.X + pToPoint.X)/2);
            ycoords.Add((pFromPoint.Y + pToPoint.Y) / 2);
          }
          bLineSelectionSequence = true;
        }
      }
      if (bLineSelectionSequence)
      {
        dX = xcoords.Average();
        dY = ycoords.Average();
      }
      else
        return;
      IMetricUnitConverter pMetricUnitConv = (IMetricUnitConverter)pCadEd;
      double newX = 0;
      double newY = 0;
      pMetricUnitConv.ConvertXY(esriCadastralUnitConversionType.esriCUCFromMetric, dX, dY, ref newX, ref newY);
      // Calculate center point of current map extent
      IPoint centerPoint = new PointClass();
      centerPoint.SpatialReference = ArcMap.Document.ActiveView.FocusMap.SpatialReference;
      centerPoint.PutCoords(newX, newY);

      IEnvelope envelope = ArcMap.Document.ActiveView.Extent;
      envelope.CenterAt(centerPoint);
      //envelope.Expand(zoomRatio, zoomRatio, true);
      ArcMap.Document.ActiveView.Extent = envelope;
      ArcMap.Document.ActiveView.Refresh();

      #endregion

    }

    protected override void OnUpdate()
    {
    }
  }
}
