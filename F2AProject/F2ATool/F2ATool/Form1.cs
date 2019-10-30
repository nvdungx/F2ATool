using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//added
using System.IO;

namespace F2ATool
{
  public partial class Form1 : Form
  {
    private List<CParser> CParser_List;
    OpenFileDialog openFILE;
    public Form1()
    {
      InitializeComponent();
      // default value for comboBox
      this.comboBox_output.SelectedItem = this.comboBox_output.Items[0];

      // init Cparser class
      CParser_List = new List<CParser>();

      // init OpenFile object, for import data
      openFILE = new OpenFileDialog();
      openFILE.InitialDirectory = "C:\\";
      openFILE.Filter = "C source files(*.c)|*.c|All files(*.*)|*.*";
      openFILE.Title = "Import C source files";
      openFILE.Multiselect = true;
      openFILE.RestoreDirectory = true;
    }

    private void button_parse_Click(object sender, EventArgs e)
    {
      #region 1.Check if list box has data (return bool flag and List<object>:string filepath)
      bool input_file_sts = false;
      List<string> file_list = new List<string>();
      // 1. Check if list box has data selected
      // check if there is any item in listbox
      if (this.listBox_inputfile.Items.Count > 0)
      {
        // if there is no selected item ( process all item)
        if (this.listBox_inputfile.SelectedItem == null)
        {
          // get all file path to an array
          file_list = this.listBox_inputfile.Items.Cast<string>().ToList<string>();
        }
        // else there are specific item selected ( process the selected item only)
        else
        {
          file_list = this.listBox_inputfile.SelectedItems.Cast<string>().ToList<string>();
        }
        // loop check if the item is availabled(exist)
        for (int i = file_list.Count - 1; i >= 0; i--)
        {
          // if file not exist during process, remove it from list
          if (File.Exists(file_list[i].ToString()) == false)
          {
            MessageBox.Show(string.Format("File {0} does not exist!", file_list[i]), "Warning");
            this.listBox_inputfile.Items.Remove(file_list[i]);
            file_list.Remove(file_list[i]);
          }
          else
          {
            // pass
          }
        }
        // there is atlest 1 file available to process.
        if (file_list.Count > 0)
        {
          input_file_sts = true;
        }
      }
      else
      {
        input_file_sts = false;
      }
      #endregion

      // 2. Check if combobox has selected value
      if ((this.comboBox_output.SelectedItem == null) || (input_file_sts == false))
      {
        MessageBox.Show("Please check the input file list or output type", "Warning");
      }
      else
      {
        // During process: update the name of parse item/ progress bar
        // disable the control button/checkbox/combobox/listbox
        // Disable access to control element
        this.listBox_inputfile.Enabled = false;
        this.checkBox_generate.Enabled = false;
        this.button_import.Enabled = false;
        this.button_parse.Enabled = false;
        this.comboBox_output.Enabled = false;

        #region 2. collect data of file list to list of object CParser (input List<object>:string filepath=>update CParser_List list object of Form1 )
        // read each file and update data to CParser_List
        // read text from file could use StreamReader(read each line/memory efficient)
        // or use File.ReadAllText(read all file/not effecicient when comming to an large file)
        // in this case source file is relative small >> use File.ReadAllText for easier process(regex)

        // Check the previous data - if CParser_List already has data then 
        if (CParser_List.Count > 0)
        {
          // Remove the CParser that not selected in file_list
          for (int i = CParser_List.Count - 1; i >= 0 ; i--)
          {
            if (false == file_list.Exists(ele => ele == CParser_List[i].full_name))
            {
              CParser_List.RemoveAt(i);
            }
          }
        }
        else
        {
          // CParser_List is empty, pass and update data from file_list
        }

        try
        {
          foreach (var item in file_list)
          {
            CParser temp_parser = new CParser();
            bool valid_flag = false;
            bool modified_flag = false;
            string temp_time;
            temp_time = string.Concat(File.GetLastWriteTime(item.ToString()).ToString(), "-", File.GetLastWriteTime(item.ToString()).Ticks.ToString());
            // if list empty or item is not read yet
            if ((CParser_List.Count == 0) || (false == CParser_List.Exists(ele => ele.full_name == item.ToString())))
            {
              valid_flag = true;
            }
            // if list not empty and item already read
            else
            {
              valid_flag = false;
              if (CParser_List.Exists(ele => ele.last_modified == temp_time))
              {
                modified_flag = false;
              }
              else
              {
                modified_flag = true;
              }
            }
            if (valid_flag == true)
            {
              temp_parser.full_name = item.ToString();
              temp_parser.name = Path.GetFileName(item.ToString());
              temp_parser.last_modified = temp_time;
              //temp_parser.raw_data = File.ReadAllText(item.ToString());
              CParser_List.Add(temp_parser);
            }
            else
            {
              if (modified_flag == true)
              {
                int ele_index;
                ele_index = CParser_List.FindIndex(ele => ele.full_name == item.ToString());
                CParser_List[ele_index].last_modified = temp_time;
                //CParser_List[ele_index].raw_data = File.ReadAllText(item.ToString());
              }
            }
          }
        }
        catch (Exception err)
        {
          MessageBox.Show(string.Format("Error during collect file data: {0}", err.Message));
          throw;
        }
        #endregion

        // 3. Parse the selected item
        #region 3. loop all object in list and process
        foreach (var item in CParser_List)
        {
          this.label_process_file.Visible = true;
          this.label_process_file.Text = item.name;
          if(item.Parse_Data() == true)
          {
            MessageBox.Show("Updated data is parsed");
          }
          else
          {
            MessageBox.Show("Data is not updated");
          }
        }
        //this.label_process_file.Visible = false;
        #endregion
        // 4. Output the parsed data to temp file if checkbox selected
        if (this.checkBox_generate.CheckState == CheckState.Checked)
        {
          MessageBox.Show("Generated temp file");
        }
        else
        {
          // pass
        }
        // 5. Process draw data
        //CDraw

        // All data and output shall be saved to current tool dir

        // Re-enable access to control element
        this.listBox_inputfile.Enabled = true;
        this.checkBox_generate.Enabled = true;
        this.button_import.Enabled = true;
        this.button_parse.Enabled = true;
        this.comboBox_output.Enabled = true;
      }
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
