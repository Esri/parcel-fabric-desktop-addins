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
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Desktop.AddIns;

namespace FabricPointMoveToFeature
{
  public class LayerDropdown : ESRI.ArcGIS.Desktop.AddIns.ComboBox
  {
    private static LayerDropdown s_comboBox;
    private static IFeatureLayer m_fl;
    private IMap m_map;

    public LayerDropdown()
    {
      s_comboBox = this;
      if (m_map == null)
      {
        m_map = ArcMap.Document.FocusMap;
        if (m_map == null)//if it's still null then bail
          return;
      }
      FillComboBox(m_map);
    }

    internal static void FillComboBox(IMap theMap)
    {
      LayerDropdown selCombo = LayerDropdown.GetTheComboBox();

      if (selCombo == null)
        return;

      //hold onto the currently selected layer name, before the combo box is cleared
      IFeatureLayer pFlyr = null;
      if (selCombo.items.Count > 1)
      {
        pFlyr = (IFeatureLayer)selCombo.GetItem(selCombo.Selected).Tag;
      }
      selCombo.ClearAll();

      IFeatureLayer featureLayer;
      ICompositeLayer pCompLyr = null;
      LayerManager ext_PntLyrMan = LayerManager.GetExtension();
      bool bUseLines = false;
      if (ext_PntLyrMan != null)
        bUseLines = ext_PntLyrMan.UseLines;
      // Loop through the layers in the map and add the layer's name to the combo box.
      int lLayerCount = 0;
      int cookie = 0;
      int resetCookie = 0;
      for (int i = 0; i < theMap.LayerCount; i++)
      {
        bool bIsComposite = false;
        ILayer pLayer = theMap.get_Layer(i);
        if (pLayer is ICompositeLayer)
        {
          pCompLyr = (ICompositeLayer)pLayer;
          bIsComposite = true;
        }

        int iCompositeLyrCnt = 1;
        if (bIsComposite)
          iCompositeLyrCnt = pCompLyr.Count;
        for (int j = 0; j <= (iCompositeLyrCnt - 1); j++)
        {
          if (bIsComposite)
            pLayer = pCompLyr.get_Layer(j);
          if (pLayer is IFeatureLayer)
          {
            featureLayer = pLayer as IFeatureLayer;
            if (featureLayer is ICadastralFabricSubLayer2)
              break;

            if (featureLayer.FeatureClass == null)
              continue;

            if (featureLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint && !bUseLines ||
              featureLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline && bUseLines)
            {
              cookie = selCombo.AddItem(featureLayer.Name, featureLayer);
              if (pFlyr != null)
              {
                if (featureLayer.Name == pFlyr.Name)
                  resetCookie = cookie;
              }
              lLayerCount++;
            }
          }
        }
        if (lLayerCount > 1)
          selCombo.Select(cookie);//select the last one added
      }

      if (pFlyr == null)
      {
        if (lLayerCount > 1)
          selCombo.Select(cookie);//select the last one added
        else if (lLayerCount == 0)
          m_fl = null;
      }
      else if (lLayerCount > 1)// else set the combo box to the originally selected layer
        selCombo.Select(resetCookie);
    }

    internal static LayerDropdown GetTheComboBox()
    {
      return s_comboBox;
    }

    internal static IFeatureLayer GetFeatureLayer()
    {
      try
      {
        if (m_fl == null)
          m_fl = s_comboBox.GetItem(s_comboBox.Selected).Tag as IFeatureLayer;
        return m_fl;
      }
      catch
      {
        return null;
      }
      finally
      { }
    }

    internal int AddItem(string itemName, IFeatureLayer layer)
    {
      // Add each item to combo box.
      int cookie = s_comboBox.Add(itemName, layer);
      s_comboBox.Select(cookie);//select the last one added
      return cookie;
    }

    internal void ClearAll()
    {
      s_comboBox.Clear();
    }

    protected override void OnUpdate()
    {
      this.Enabled = (s_comboBox.items.Count > 0);
    }

    protected override void OnSelChange(int cookie)
    {
      try
      {
        IFeatureLayer fl = this.GetItem(cookie).Tag as IFeatureLayer;
        m_fl = fl;
      }
      catch { ;}
    }
  }


}
