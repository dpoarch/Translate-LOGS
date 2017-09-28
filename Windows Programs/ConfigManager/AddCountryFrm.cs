using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ConfigManager
{
  public partial class AddCountryFrm : Form
  {
    private string _CountryConfig;
    public string CountryConfig
    {
      get
      {
        return _CountryConfig;
      }
      set
      {
        _CountryConfig = value;
      }
    }
    public AddCountryFrm()
    {
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      _CountryConfig = textBox1.Text;
      this.Close();
    }

    private void button2_Click(object sender, EventArgs e)
    {
      this.Close();
    }
  }
}