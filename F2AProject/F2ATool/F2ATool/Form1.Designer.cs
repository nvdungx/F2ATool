namespace F2ATool
{
  partial class Form1
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.button_parse = new System.Windows.Forms.Button();
      this.listBox_inputfile = new System.Windows.Forms.ListBox();
      this.button_import = new System.Windows.Forms.Button();
      this.checkBox_generate = new System.Windows.Forms.CheckBox();
      this.comboBox_output = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.label_process_file = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // button_parse
      // 
      this.button_parse.Location = new System.Drawing.Point(536, 12);
      this.button_parse.Name = "button_parse";
      this.button_parse.Size = new System.Drawing.Size(92, 44);
      this.button_parse.TabIndex = 0;
      this.button_parse.Text = "Process";
      this.button_parse.UseVisualStyleBackColor = true;
      this.button_parse.Click += new System.EventHandler(this.button_parse_Click);
      // 
      // listBox_inputfile
      // 
      this.listBox_inputfile.FormattingEnabled = true;
      this.listBox_inputfile.Location = new System.Drawing.Point(12, 12);
      this.listBox_inputfile.Margin = new System.Windows.Forms.Padding(1);
      this.listBox_inputfile.Name = "listBox_inputfile";
      this.listBox_inputfile.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
      this.listBox_inputfile.Size = new System.Drawing.Size(454, 56);
      this.listBox_inputfile.TabIndex = 1;
      this.listBox_inputfile.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBox_inputfile_KeyDown);
      // 
      // button_import
      // 
      this.button_import.Location = new System.Drawing.Point(472, 12);
      this.button_import.Name = "button_import";
      this.button_import.Size = new System.Drawing.Size(58, 28);
      this.button_import.TabIndex = 2;
      this.button_import.Text = "Import";
      this.button_import.UseVisualStyleBackColor = true;
      this.button_import.Click += new System.EventHandler(this.button_import_Click);
      // 
      // checkBox_generate
      // 
      this.checkBox_generate.AutoSize = true;
      this.checkBox_generate.Location = new System.Drawing.Point(536, 60);
      this.checkBox_generate.Name = "checkBox_generate";
      this.checkBox_generate.Size = new System.Drawing.Size(96, 17);
      this.checkBox_generate.TabIndex = 3;
      this.checkBox_generate.Text = "Generate temp";
      this.checkBox_generate.UseVisualStyleBackColor = true;
      this.checkBox_generate.Visible = false;
      // 
      // comboBox_output
      // 
      this.comboBox_output.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBox_output.FormattingEnabled = true;
      this.comboBox_output.Items.AddRange(new object[] {
            "vsdx",
            "jpg"});
      this.comboBox_output.Location = new System.Drawing.Point(472, 58);
      this.comboBox_output.Name = "comboBox_output";
      this.comboBox_output.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.comboBox_output.Size = new System.Drawing.Size(58, 21);
      this.comboBox_output.TabIndex = 4;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(472, 43);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(62, 13);
      this.label1.TabIndex = 5;
      this.label1.Text = "Output type";
      // 
      // progressBar1
      // 
      this.progressBar1.Location = new System.Drawing.Point(180, 70);
      this.progressBar1.Margin = new System.Windows.Forms.Padding(1);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(286, 10);
      this.progressBar1.TabIndex = 6;
      // 
      // label_process_file
      // 
      this.label_process_file.Location = new System.Drawing.Point(13, 68);
      this.label_process_file.Name = "label_process_file";
      this.label_process_file.Size = new System.Drawing.Size(165, 15);
      this.label_process_file.TabIndex = 7;
      this.label_process_file.Text = "Name_Of_Source_File";
      this.label_process_file.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.label_process_file.Visible = false;
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(636, 85);
      this.Controls.Add(this.label_process_file);
      this.Controls.Add(this.progressBar1);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.comboBox_output);
      this.Controls.Add(this.checkBox_generate);
      this.Controls.Add(this.button_import);
      this.Controls.Add(this.listBox_inputfile);
      this.Controls.Add(this.button_parse);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.KeyPreview = true;
      this.MaximumSize = new System.Drawing.Size(652, 124);
      this.MinimumSize = new System.Drawing.Size(652, 124);
      this.Name = "Form1";
      this.Text = "F2ATool";
      this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Form1_KeyPress);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button button_parse;
    private System.Windows.Forms.ListBox listBox_inputfile;
    private System.Windows.Forms.Button button_import;
    private System.Windows.Forms.CheckBox checkBox_generate;
    private System.Windows.Forms.ComboBox comboBox_output;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ProgressBar progressBar1;
    private System.Windows.Forms.Label label_process_file;
  }
}

