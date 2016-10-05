using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;

namespace ParcelEditHelper
{
  public partial class dlgAdjustmentResults : Form
  {
    public dlgAdjustmentResults()
    {
      InitializeComponent();
    }

    private void btnAccep_Click(object sender, EventArgs e)
    {

    }

    private void btnResultOptions_Click(object sender, EventArgs e)
    {
      CreateContextMenu(ArcMap.Application);
    }
    private void CreateContextMenu(IApplication application)
    {
      ICommandBars commandBars = application.Document.CommandBars;
      ICommandBar commandBar = commandBars.Create("TemporaryContextMenu", ESRI.ArcGIS.SystemUI.esriCmdBarType.esriCmdBarTypeShortcutMenu);

      System.Object optionalIndex = System.Type.Missing;
      UID uid = new UIDClass();

      uid.Value = ThisAddIn.IDs.AddBookMark.ToString(); //"esriArcMapUI.ZoomInFixedCommand"; // Can use CLSID or ProgID
      uid.SubType = 0;
      commandBar.Add(uid, ref optionalIndex);


      //Show the context menu at the current mouse location
      System.Drawing.Point currentLocation = System.Windows.Forms.Form.MousePosition;
      commandBar.Popup(currentLocation.X, currentLocation.Y);
    }

    private void txtReport_MouseClick(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Right)
      {
        CreateContextMenu(ArcMap.Application);
      } 

    }
  }
}
