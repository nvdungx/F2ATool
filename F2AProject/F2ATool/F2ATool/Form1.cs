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
        string[] import_files;
        public Form1()
        {
            InitializeComponent();
            mCParser = new CParser();
        }

        private void button_parse_Click(object sender, EventArgs e)
        {
            
        }

        private void button_import_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFILE = new OpenFileDialog();
            openFILE.InitialDirectory = "C:\\";
            openFILE.Filter = "C source files (*.c)|*.c";
            openFILE.FilterIndex = 10;
            openFILE.RestoreDirectory = true;
            if (openFILE.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                import_files[0] = openFILE.FileName;

                //Read the contents of the file into a stream
                var fileStream = openFILE.OpenFile();
            }
        }
    }
}
