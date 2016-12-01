using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace ParcelEditHelper
{
  public class ToolbarHelp : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    
    static public string AssemblyDirectory
    {
      get
      {
        string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
        UriBuilder uri = new UriBuilder(codeBase);
        string path = Uri.UnescapeDataString(uri.Path);
        return System.IO.Path.GetDirectoryName(path);
      }
    }

    public ToolbarHelp()
    {
    }

    protected override void OnClick()
    {
      ToolbarHelpDlg HelpInfo = new ToolbarHelpDlg();
      HelpInfo.Width = 900;

      string fileName = AssemblyDirectory + "/Help/Help.htm";
      HelpInfo.webBrowser1.Url = new Uri(fileName);
      HelpInfo.Show();
    }

    protected override void OnUpdate()
    {
    }
  }
}
