using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//added
using System.Text.RegularExpressions;
using System.IO.Packaging;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace F2ATool
{
  /*manipulate a Visio 2013 drawing at the file level, a solution need only to use the.NET Framework namespaces
   * and classes associated with working with ZIP files or XML, like System.IO.Packaging or System.Xml.*/
  class CDraw
  {

  }
  /*class CDraw
  {
    Microsoft.Office.Interop.Visio.Application mVisio;
    Microsoft.Office.Interop.Visio.Document mDoc;
    Microsoft.Office.Interop.Visio.Page mPage;
    CParser parent_file;
    string outfile_path;
    string outfile_name;
    string output_file;
    public CDraw()
    {
      output_file = "";
      this.parent_file = null;
      outfile_path = "";
      outfile_name = "";
      mVisio = null;
    }
    public void set_InputFile(CParser input_Cparser)
    {
      this.parent_file = input_Cparser;
      outfile_path = Path.GetDirectoryName(this.parent_file.full_name);
      outfile_name = Path.GetFileNameWithoutExtension(this.parent_file.name);
    }
    public void set_App(Microsoft.Office.Interop.Visio.Application app)
    {
      mVisio = app;
    }
    public void draw_function()
    {
      output_file = outfile_path + outfile_name + ".vsdx";
      mDoc = mVisio.Documents.Add("");
      draw_misc();
      foreach (var func in this.parent_file.ParseFuncList)
      {
        mPage = mDoc.Pages.Add();
        mPage.Name = func.name;
      }
      mVisio.ActiveDocument.SaveAs(output_file);
      mVisio.ActiveDocument.Close();
      mVisio.Quit();
    }
    private void draw_misc()
    {
      Microsoft.Office.Interop.Visio.Document mStencil = mVisio.Documents.OpenEx("Basic Flowchart Shapes.vss",
                    (short)Microsoft.Office.Interop.Visio.VisOpenSaveArgs.visOpenDocked);
      Microsoft.Office.Interop.Visio.Page miscPage;
      if (mDoc.Pages.Count >= 1)
      {
        List<Microsoft.Office.Interop.Visio.Master> master_list;
        for (int i = 0; i < mStencil.Masters.Count; i++)
        {
          //master_list.Add(mStencil.);
        }

        miscPage = mVisio.ActivePage;
        miscPage.Name = "Misc";
        miscPage.AutoSizeDrawing();
        miscPage.LayoutChangeDirection(Microsoft.Office.Interop.Visio.VisLayoutDirection.visLayoutDirFlipVert);
        //Microsoft.Office.Interop.Visio.Master visioRectMaster = mStencil.Comments.
        Microsoft.Office.Interop.Visio.Shape mRectShape;

        double posY = 0;
        double posX = this.mDoc.PaperWidth[0] / 2;
        for (int i = 0; i < this.parent_file.GlobalStatement.Count; i++)
        {
          posY += i * 2.5;
          mRectShape = miscPage.DrawRectangle(2.5, 0, 5, 2.5);
          mRectShape.Text = this.parent_file.GlobalStatement.Values.ElementAt<string>(i);
          //visioRectShape = miscPage.Drop(visioRectMaster, posX, posY);
          //visioRectShape.Text = this.parent_file.GlobalStatement.Values.ElementAt<string>(i);
        }
      }
    }
  }*/
}
