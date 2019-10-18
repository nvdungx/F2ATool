using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Packaging;
using System.Diagnostics;
using Microsoft.Office.Interop.Visio;

namespace F2ATool
{
    class Program
    {
        static Application mVisio = new Application();
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Create the VSDX file ...");
                // Need to get the folder path for the Desktop
                // where the file is saved.
                string filePath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Desktop) + @"MyDrawing.vsdx";
                mVisio.Documents.Add("");

                Documents visioDocs = mVisio.Documents;
                Document visioStencil = visioDocs.OpenEx("Basic Shapes.vss",
                    (short)Microsoft.Office.Interop.Visio.VisOpenSaveArgs.visOpenDocked);

                Page visioPage = mVisio.ActivePage;

                Master visioRectMaster = visioStencil.Masters.get_ItemU(@"Rectangle");
                Shape visioRectShape = visioPage.Drop(visioRectMaster, 4.25, 5.5);

                visioRectShape.Text = @"Rectangle text.";

                mVisio.ActiveDocument.sa
                mVisio.ActiveDocument.SaveAs(filePath);
                mVisio.ActiveDocument.Close();
                mVisio.Quit();
            }
            catch (Exception err)
            {
                Console.WriteLine("Error: {0}", err.Message);
            }
            finally
            {
                Console.Write("\nPress any key to continue ...");
                Console.ReadKey();
            }
        }
    }
}
