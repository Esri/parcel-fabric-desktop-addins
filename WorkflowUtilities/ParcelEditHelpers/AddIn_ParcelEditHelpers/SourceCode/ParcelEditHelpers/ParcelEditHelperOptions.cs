using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ParcelEditHelper
{
  public class ParcelEditHelperOptions : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    public ParcelEditHelperOptions()
    {
    }

    protected override void OnClick()
    {
      dlgParcEditHelperOptions pParcelEditHelperOptions = new dlgParcEditHelperOptions();
           
      //Display the dialog
      DialogResult pDialogResult = pParcelEditHelperOptions.ShowDialog();

      if (pDialogResult != DialogResult.OK)
        return;

    }

    protected override void OnUpdate()
    {
    }
  }
}
