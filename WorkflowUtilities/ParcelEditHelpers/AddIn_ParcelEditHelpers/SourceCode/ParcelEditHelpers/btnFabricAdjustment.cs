using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;

namespace ParcelEditHelper
{
  public class btnFabricAdjustment : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    public btnFabricAdjustment()
    {
    }

    protected override void OnClick()
    {

      IDockableWindow dockWindow = ParcelEditHelperExtension.GetFabricAdjustmentWindow();
      if (dockWindow == null)
        return;

      dockWindow.Show(!dockWindow.IsVisible());

    }

    protected override void OnUpdate()
    {
      Enabled = ArcMap.Application != null;
    }
  }

}
