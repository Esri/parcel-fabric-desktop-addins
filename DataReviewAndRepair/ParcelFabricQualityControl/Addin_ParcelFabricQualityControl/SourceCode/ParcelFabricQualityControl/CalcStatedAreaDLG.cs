using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ParcelFabricQualityControl
{
    public partial class CalcStatedAreaDLG : Form
    {
        public CalcStatedAreaDLG()
        {
            InitializeComponent();
            cboAreaUnit.SelectedIndex = cboAreaUnit.FindStringExact("Acres");

            Utilities Utils = new Utilities();
            string sDesktopVers = Utils.GetDesktopVersionFromRegistry();
            if (sDesktopVers.Trim() == "")
              sDesktopVers = "Desktop10.1";
            else
              sDesktopVers = "Desktop" + sDesktopVers;
            string sValues =
            Utils.ReadFromRegistry(RegistryHive.CurrentUser, "Software\\ESRI\\" + sDesktopVers + "\\ArcMap\\Cadastral",
              "AddIn.FabricQualityControl_CalculateStatedArea");
            if (sValues.Trim() == "")
              sValues = "Acres, , 2"; //Defaults
            string[] Values = sValues.Split(',');
            try
            {
              this.cboAreaUnit.SelectedIndex = cboAreaUnit.FindStringExact(Values[0]);
              this.txtSuffix.Text = Values[1];
              this.numDecPlaces.Value = Convert.ToInt16(Values[2]);
            }
            catch
            {
              this.cboAreaUnit.SelectedIndex = 0;
              this.numDecPlaces.Value = 2;
            }

        }

        private void cboAreaUnit_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
          //write the key
          Utilities Utils = new Utilities();
          string sDesktopVers = Utils.GetDesktopVersionFromRegistry();
          if (sDesktopVers.Trim() == "")
            sDesktopVers = "Desktop10.1";
          else
            sDesktopVers = "Desktop" + sDesktopVers;

          string sUnits = this.cboAreaUnit.SelectedItem.ToString();
          string sSuffix = this.txtSuffix.Text;
          string sDecimals = this.numDecPlaces.Value.ToString();
          Utils.WriteToRegistry(RegistryHive.CurrentUser, "Software\\ESRI\\" +
            sDesktopVers + "\\ArcMap\\Cadastral", "AddIn.FabricQualityControl_CalculateStatedArea",
            sUnits + "," + sSuffix + "," + sDecimals);
        }
    }
}
