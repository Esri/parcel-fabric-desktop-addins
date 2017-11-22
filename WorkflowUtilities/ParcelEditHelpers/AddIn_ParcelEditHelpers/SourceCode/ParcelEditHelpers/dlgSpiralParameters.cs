using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParcelEditHelper
{
  public partial class dlgSpiralParameters : Form
  {
    private List<TextBox> boxes = new List<TextBox>();
    private bool nonNumberEntered = false;
    private bool iKeyPressed = false;

    public dlgSpiralParameters()
    {
      InitializeComponent();
      boxes.Add(txtEndRadius);
      boxes.Add(txtStartRadius);
      boxes.Add(txtPathLengthParameter);
      boxes.Add(txtDirection);
      enableButton();

      this.cboPathLengthParameter.SelectedIndex= 0;
    }

    private void enableButton()
    {
      foreach (var t in boxes)
      {
        if (string.IsNullOrEmpty(t.Text))
        {
          btnCreate.Enabled = false;
          return;
        }
      }
      btnCreate.Enabled = true;
    }

    private void btnTangentPointClickAndCreate_Click(object sender, EventArgs e)
    {
      
    }


    private void txtBoxHandleKeyDown(object sender, KeyEventArgs e)
    {
      // Initialize the flag to false.
      nonNumberEntered = false;

      iKeyPressed = (e.KeyCode == Keys.I);
      // Determine whether the keystroke is a number from the top of the keyboard.
      if (e.KeyCode < Keys.D0 || e.KeyCode > Keys.D9)
      {
        // Determine whether the keystroke is a number from the keypad.
        if (e.KeyCode < Keys.NumPad0 || e.KeyCode > Keys.NumPad9)
        {
          // Determine whether the keystroke is a backspace.
          if ((e.KeyCode != Keys.Back) && (e.KeyCode != Keys.Decimal)) //&& (e.KeyCode != Keys.OemPeriod))
          {
            // A non-numerical keystroke was pressed.
            // Set the flag to true and evaluate in KeyPress event.
            nonNumberEntered = true;
          }
        }
      }
    }

    private void txtBoxHandleKeyPress(object sender, KeyPressEventArgs e)
    {
      // Check for the flag being set in the KeyDown event.
      if (nonNumberEntered == true && (e.KeyChar != '-') && (e.KeyChar != '.'))
      {
        // Stop the character from being entered into the control since it is non-numerical.
        e.Handled = true;
      }
    }

    private void txtStartRadius_KeyPress(object sender, KeyPressEventArgs e)
    {
      txtBoxHandleKeyPress(sender, e);
    }

    private void txtStartRadius_KeyDown(object sender, KeyEventArgs e)
    {
      txtBoxHandleKeyDown(sender, e);
      if (iKeyPressed)
        txtStartRadius.Text = "INFINITY";
      else
      {
        if (txtStartRadius.Text.ToUpper().Contains("INFINITY"))
          txtStartRadius.Text = txtStartRadius.Text.ToUpper().Replace("INFINITY", "");

        //txtStartRadius.Text = txtStartRadius.Text.ToUpper().Replace("I", "");
        //txtStartRadius.Text = txtStartRadius.Text.ToUpper().Replace("N", "");
        //txtStartRadius.Text = txtStartRadius.Text.ToUpper().Replace("F", "");
        //txtStartRadius.Text = txtStartRadius.Text.ToUpper().Replace("T", "");
        //txtStartRadius.Text = txtStartRadius.Text.ToUpper().Replace("Y", "");
      }
    }

    private void txtEndRadius_KeyDown(object sender, KeyEventArgs e)
    {
      txtBoxHandleKeyDown(sender, e);
      if (iKeyPressed)
        txtEndRadius.Text = "INFINITY";
      else
      {
        if (txtEndRadius.Text.ToUpper().Contains("INFINITY"))
          txtEndRadius.Text = txtEndRadius.Text.ToUpper().Replace("INFINITY","");
      }
    }

    private void txtEndRadius_KeyPress(object sender, KeyPressEventArgs e)
    {
      txtBoxHandleKeyPress(sender, e);
    }

    private void txtPathLengthParameter_KeyPress(object sender, KeyPressEventArgs e)
    {
      txtBoxHandleKeyPress(sender, e);
    }

    private void txtPathLengthParameter_KeyDown(object sender, KeyEventArgs e)
    {
      txtBoxHandleKeyDown(sender, e);
    }

    private void txtStartRadius_TextChanged(object sender, EventArgs e)
    {
      enableButton();
    }

    private void txtEndRadius_TextChanged(object sender, EventArgs e)
    {
      enableButton();
    }

    private void txtPathLengthParameter_TextChanged(object sender, EventArgs e)
    {
      enableButton();
    }

    private void btnCreate_Click(object sender, EventArgs e)
    {

    }

    private void btnDensifyOptions_Click(object sender, EventArgs e)
    {
      panel1.Visible = !panel1.Visible;

      if (panel1.Location.Y > panel2.Location.Y)
      {
        panel1.Location = panel2.Location;
        System.Drawing.Point p = new System.Drawing.Point(panel1.Location.X, panel1.Location.Y + panel1.Height);
        panel2.Location = p;
        btnDensifyOptions.Text = "^";
      }
      else
      {
        panel2.Location = panel1.Location;
        System.Drawing.Point p = new System.Drawing.Point(panel2.Location.X, panel2.Location.Y + panel2.Height);
        panel1.Location = p;
        btnDensifyOptions.Text = "V";
      }
      txtDensifyValue.Enabled = cboDensificationType.Enabled = optCustomDensification.Checked;
    }

    private void optCustomDensification_CheckedChanged(object sender, EventArgs e)
    {
      txtDensifyValue.Enabled = cboDensificationType.Enabled = optCustomDensification.Checked;
    }

    private void txtDensifyValue_KeyDown(object sender, KeyEventArgs e)
    {
      txtBoxHandleKeyDown(sender, e);
    }

    private void txtDensifyValue_KeyPress(object sender, KeyPressEventArgs e)
    {
      txtBoxHandleKeyPress(sender, e);
    }

    private void optDefaultDensification_CheckedChanged(object sender, EventArgs e)
    {
      numAngleDensification.Enabled = cboDensificationType.Enabled = !optDefaultDensification.Checked;
      numAngleDensification.Value = 2;

      txtDensifyValue.Visible = false;
      System.Drawing.Point p = new System.Drawing.Point(cboDensificationType.Location.X + cboDensificationType.Width + 10, cboDensificationType.Location.Y);
      numAngleDensification.Location = p;
      numAngleDensification.Visible = true;
      //txtDensifyValue.Enabled = cboDensificationType.Enabled = !optDefaultDensification.Checked;
      //txtDensifyValue.Text = "2°";

      cboDensificationType.SelectedIndex = 1;
    }

    private void cboDensificationType_SelectedIndexChanged(object sender, EventArgs e)
    {
      System.Drawing.Point p = new System.Drawing.Point(cboDensificationType.Location.X + cboDensificationType.Width + 10, cboDensificationType.Location.Y);
      if (cboDensificationType.SelectedIndex != 1)
      {
        numAngleDensification.Visible = false;
        txtDensifyValue.Location = p;
        txtDensifyValue.Visible = true;
        txtDensifyValue.Enabled = cboDensificationType.Enabled = !optDefaultDensification.Checked;
      }
      else
      {
        txtDensifyValue.Visible = false;
        numAngleDensification.Location = p;
        numAngleDensification.Visible = true;
        numAngleDensification.Enabled = cboDensificationType.Enabled = !optDefaultDensification.Checked;
      }
    }
  }

}
