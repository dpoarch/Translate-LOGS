using System;
using System.Windows.Forms;
using System.Xml;
using System.Drawing;
using System.Drawing.Printing;
using System.Collections;

namespace ConfigManager
{
  public partial class ConfigMain : Form
  {
    private XmlDocument _TranslateConfig;
    string _CurrentConfigSection;
    string _OriginalFileName;
    bool _ConfigChanged;
    
    string documentContents;
    string stringToPrint;
    
    public ConfigMain()
    {
      SuspendLayout();
      _TranslateConfig = new XmlDocument();      
      InitializeComponent();
      ResumeLayout();
    }

    void _TranslateConfig_NodeChanged(object sender, XmlNodeChangedEventArgs e)
    {
      _ConfigChanged = true;
    }

    private void LoadPluginSections()
    {
      XmlNodeList list = _TranslateConfig.SelectNodes("configuration/configSections/sectionGroup[@name='TranslatePlugins']/*");
      for (int i = 0; i < list.Count; i++)
        lbPlugins.Items.Add(list[i].Attributes["name"].Value);

      if (lbPlugins.Items.Count > 0)
        lbPlugins.SelectedIndex = 0;

      //Country Config
      list = _TranslateConfig.SelectNodes("configuration/configSections/sectionGroup[@name='CountryConfig']/*");
      for (int i = 0; i < list.Count; i++)
        lbCountryConfig.Items.Add(list[i].Attributes["name"].Value);

      if (lbCountryConfig.Items.Count > 0)
        lbCountryConfig.SelectedIndex = 0;
    }

    private void lbPlugins_SelectedIndexChanged(object sender, EventArgs e)
    {
      grdOptionalParams.Rows.Clear();
      _CurrentConfigSection = "configuration/TranslatePlugins/" + lbPlugins.SelectedItem.ToString();
      XmlNodeList list = _TranslateConfig.SelectNodes(_CurrentConfigSection + "/*");
      int RowCounter = 0;
      foreach (XmlNode node in list)
      {
        Control[] FormControls = this.Controls.Find("txt" + node.Attributes["key"].Value,true);
        if (FormControls.Length > 0)
        {
          if (FormControls[0] != null && FormControls[0] is TextBox)
          {
            ((TextBox)FormControls[0]).Text = node.Attributes["value"].Value.ToString();
            object[] txtData = new object[] { "configuration/TranslatePlugins/" + lbPlugins.SelectedItem.ToString() + "/add[@key='" + node.Attributes["key"].Value + "']/@value", node.Attributes["value"].Value };
            ((TextBox)FormControls[0]).Tag = txtData;

          }
        }
        else
        {
          grdOptionalParams.Rows.Add(new object[] { node.Attributes["key"].Value, node.Attributes["value"].Value, "configuration/CountryConfig/" + lbPlugins.SelectedItem.ToString() + "/add[@key='" + node.Attributes["key"].Value + "']/@value" });
          grdOptionalParams.Rows[RowCounter].Cells[0].ReadOnly = true;
          RowCounter++;
        }
      }
    }

    #region Directory Setter Functions
    private void SetDir(TextBox txtDestination)
    {
      DialogResult result = folderBrowserDialog1.ShowDialog();  
      if (result == DialogResult.OK)
        txtDestination.Text = folderBrowserDialog1.SelectedPath;
    }
    private void btnOpenSource_Click(object sender, EventArgs e)
    {
      SetDir(txtSourceFileLocation);
    }
    private void btnOpenTranslatedFile_Click(object sender, EventArgs e)
    {
      SetDir(txtTranslatedFileLocation);
    }
    private void btnOpenRulesFile_Click(object sender, EventArgs e)
    {
      SetDir(txtRulesFile);
    }
    private void btnOpenSourceTemplate_Click(object sender, EventArgs e)
    {
      SetDir(txtSourceTemplate);
    }
    private void btnOpenDestinationTemplate_Click(object sender, EventArgs e)
    {
      SetDir(txtDestinationTemplate);
    }
    private void btnOpenSourceFIle_Click(object sender, EventArgs e)
    {
      DialogResult result = openFileDialog1.ShowDialog();
      if (result == DialogResult.OK)
        txtSourceFile.Text = openFileDialog1.FileName;
    }
    #endregion

    private void lbCountryConfig_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (lbCountryConfig.SelectedItem == null)
        return;

     XmlNodeList list = _TranslateConfig.SelectNodes("configuration/CountryConfig/" + lbCountryConfig.SelectedItem.ToString() + "/*");

     dgCountryValues.Rows.Clear();
     for (int i = 0; i < list.Count; i++)
     {
       dgCountryValues.Rows.Add(new object[] { list[i].Attributes["key"].Value, list[i].Attributes["value"].Value, "configuration/CountryConfig/" + lbCountryConfig.SelectedItem.ToString() + "/add[@key='" + list[i].Attributes["key"].Value + "']/@value" });
       dgCountryValues.Rows[i].Cells[0].ReadOnly = true;
     }     
    }
    
    private void txt_Leave(object sender, EventArgs e)
    {
      if (String.IsNullOrEmpty(_OriginalFileName))
        return;

      TextBox txtChanged = (TextBox)sender;
      object[] txtData = (object[])txtChanged.Tag;
      if (txtChanged.Text != txtData[1].ToString())
      {
        _TranslateConfig.SelectSingleNode(txtData[0].ToString()).Value = txtChanged.Text;        
      }
      txtChanged.BackColor = Color.White;
    }

    private void txt_Enter(object sender, EventArgs e)
    {
      if (String.IsNullOrEmpty(_OriginalFileName))
        return;

      TextBox txtChanged = (TextBox)sender;
      txtChanged.BackColor = Color.WhiteSmoke;
    }

    private void ConfigMain_FormClosing(object sender, FormClosingEventArgs e)
    {
      if (_ConfigChanged)
      {
        DialogResult result = MessageBox.Show("You have made changes to the configuration file.  Are you sure you want to quit without saving?", "Save Changes?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result == DialogResult.No)
          e.Cancel = true;
      }
    }


    #region Menu Events

    private void openToolStripMenuItem_Click(object sender, EventArgs e)
    {
      openFileDialog1.Filter = "Config Files|*.config";
      openFileDialog1.FileName = "";
      DialogResult result = openFileDialog1.ShowDialog();
      openFileDialog1.Filter = "";
      if (result != DialogResult.OK)
        return;

      try
      {
        _TranslateConfig.Load(openFileDialog1.FileName);
        LoadPluginSections();
        _OriginalFileName = openFileDialog1.FileName;

        //Add the node change events after the xml doc has already been loaded.
        _TranslateConfig.NodeChanged += new XmlNodeChangedEventHandler(_TranslateConfig_NodeChanged);
        _TranslateConfig.NodeInserted += new XmlNodeChangedEventHandler(_TranslateConfig_NodeChanged);
        _TranslateConfig.NodeRemoved += new XmlNodeChangedEventHandler(_TranslateConfig_NodeChanged);
      }
      catch (Exception ex)
      {
        MessageBox.Show("Unable to load config file.  Make sure you are loading the correct file type.", "File Load Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
      }     
    }

    private void saveToolStripMenuItem_Click(object sender, EventArgs e)
    {
      try
      {
        _TranslateConfig.Save(_OriginalFileName);
        _ConfigChanged = false;
      }
      catch (Exception ex)
      {
        MessageBox.Show("Unable to save file at this time: \r\n" + ex.Message, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (String.IsNullOrEmpty(_OriginalFileName))
        return;
      
      documentContents = _TranslateConfig.OuterXml;
      stringToPrint = _TranslateConfig.OuterXml;

      printPreviewDialog1.Document = printDocument1;
      printPreviewDialog1.ShowDialog();
    }

    void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
    {
      int charactersOnPage = 0;
      int linesPerPage = 0;


      // Sets the value of charactersOnPage to the number of characters 
      // of stringToPrint that will fit within the bounds of the page.
      e.Graphics.MeasureString(stringToPrint, this.Font,
          e.MarginBounds.Size, StringFormat.GenericTypographic,
          out charactersOnPage, out linesPerPage);

      // Draws the string within the bounds of the page.
      e.Graphics.DrawString(stringToPrint, this.Font, Brushes.Black,
      e.MarginBounds, StringFormat.GenericTypographic);

      // Remove the portion of the string that has been printed.
      stringToPrint = stringToPrint.Substring(charactersOnPage);

      // Check to see if more pages are to be printed.
      e.HasMorePages = (stringToPrint.Length > 0);

      // If there are no more pages, reset the string to be printed.
      if (!e.HasMorePages)
        stringToPrint = documentContents;

    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    #endregion

    #region Country Config Datagrid Events
    void dgCountryValues_CellEndEdit(object sender, DataGridViewCellEventArgs e)
    {      
      if (dgCountryValues.Columns[e.ColumnIndex].Name == "Key")
      {
        if (String.IsNullOrEmpty(dgCountryValues.Rows[e.RowIndex].Cells[e.ColumnIndex].FormattedValue.ToString()))
        {
          DataGridViewRow dgr = dgCountryValues.Rows[e.RowIndex];
          dgCountryValues.Rows.Remove(dgr);
        }
      }      
    }

    private void dgCountryValues_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
    {
      
    }

    private void dgCountryValues_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
    {
    }

    private void dgCountryValues_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      //if (e.RowIndex < 0 || dgCountryValues.Columns[e.ColumnIndex].Name != "Key")
        //return;
      
      if (dgCountryValues.Columns[e.ColumnIndex].Name == "Value")
      {
        string Loc = dgCountryValues.Rows[e.RowIndex].Cells["KeyLocation"].Value.ToString();
        _TranslateConfig.SelectSingleNode(Loc).Value = dgCountryValues.Rows[e.RowIndex].Cells[e.ColumnIndex].FormattedValue.ToString();
      }

    }
    #endregion

    private void button1_Click(object sender, EventArgs e)
    {
      textBox2.Text = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(textBox1.Text));
    }

    private void button4_Click(object sender, EventArgs e)
    {
      try
      {
        textBox2.Text = System.Text.Encoding.Default.GetString(Convert.FromBase64String(textBox2.Text));       
        
      }
      catch (Exception ex)
      {
        
      }
      
    }

    private void button2_Click(object sender, EventArgs e)
    {
      AddCountryFrm frm = new AddCountryFrm();
      frm.StartPosition = FormStartPosition.CenterParent;
      frm.ShowDialog(this);
      string config = frm.CountryConfig;
      if (!String.IsNullOrEmpty(config))
        lbCountryConfig.Items.Add(config);


    }



  }
}