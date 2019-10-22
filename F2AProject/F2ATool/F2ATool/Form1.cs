using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace F2ATool
{
  public partial class Form1 : Form
  {
    private CParser mCParser;
    OpenFileDialog openFILE;
    public Form1()
    {
      InitializeComponent();
      // default value for comboBox
      this.comboBox_output.SelectedItem = this.comboBox_output.Items[0];

      // init Cparser class
      mCParser = new CParser();

      // init OpenFile object, for import data
      openFILE = new OpenFileDialog();
      openFILE.InitialDirectory = "C:\\";
      openFILE.Filter = "C source files(*.c)|*.c|C header files(*.h)|*.h|txt files(*.txt)|*.txt";
      openFILE.Title = "Import C source files";
      openFILE.Multiselect = true;
      openFILE.RestoreDirectory = true;
    }

    private void button_parse_Click(object sender, EventArgs e)
    {
      // 1. Check if list box has data

      // 2. Check if combobox has selected value

      // 3. Check the selected item of list box

      // During process: update the name of parse item/ progress bar
      // disable the control button/checkbox/combobox/listbox
      // 4. Parse the selected item

      // 5. Output the parsed data to temp file if checkbox selected

      // 6. Process draw data

      // All data and output shall be saved to current tool dir
    }

    private void button_import_Click(object sender, EventArgs e)
    {
      if (openFILE.ShowDialog() == DialogResult.OK)
      {
        // Get the path of specified file
        string[] file_list = openFILE.FileNames;
        // Update list file to listbox text
        foreach (var item in file_list)
        {
          if (this.listBox_inputfile.Items.Contains(item) == false)
          {
            this.listBox_inputfile.Items.Add(item);
          }
        }
      }
      else
      {
        // pass
      }
    }

    private void Form1_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (e.KeyChar == (char)Keys.Escape)
      {
        if (this.listBox_inputfile.SelectedItem != null)
        {
          this.listBox_inputfile.ClearSelected();
        }
      }
      else
      {
        // pass
      }
    }

    private void listBox_inputfile_KeyDown(object sender, KeyEventArgs e)
    {
      ListBox sendLb = (ListBox)sender;
      if ((e.KeyCode == Keys.A) && e.Control)
      {
        for (int i = 0; i < sendLb.Items.Count; i++)
        {
          sendLb.SetSelected(i, true);
        }
      }
      else if (e.KeyCode == Keys.Delete)
      {
        if (sendLb.SelectedIndices.Count > 0)
        {
          // get number of selected items
          int num_select = sendLb.SelectedIndices.Count;
          for (int i = 0; i < num_select; i++)
          {
            // remove the current selected items
            sendLb.Items.Remove(sendLb.SelectedItem);
          }
        }
      }
      else
      {
        // pass
      }
    }
  }
}
