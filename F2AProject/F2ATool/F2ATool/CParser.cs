using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//added
using System.Text.RegularExpressions;
using System.IO.Packaging;
using System.ComponentModel;

namespace F2ATool
{
  // Function class
  // name, return_type, input argument dict
  // coord(start/end row of function in file)
  // sorteddictionary of normal statement(row_index,data)
  // sorteddictionary of branch statement(row_index,class branch)
  class CParserFunction
  {
    string name;
    string return_type;
    Dictionary<string,string> input_arg;
    uint[] coord;
    SortedDictionary<uint, string> norm_statement;
    SortedDictionary<uint, CParserBranch> branch_statement;
    public string raw_data { get; set; }
    public CParserFunction()
    {
      name = "";
      return_type = "";
      input_arg = new Dictionary<string, string>();
      coord = new uint[2];
      norm_statement = new SortedDictionary<uint, string>();
      branch_statement = new SortedDictionary<uint, CParserBranch>();
      raw_data = "";
    }

  }
  // Branch class
  // name
  // coord(start/end row of branch relative to function)
  // sub element in this branch
  // sorteddictionary of normal statement(row_index,data)
  // sorteddictionary of branch statement(row_index,class branch)
  class CParserBranch
  {
    string name;
    uint[] coord;
    SortedDictionary<uint, string> norm_statement;
    SortedDictionary<uint, CParserBranch> branch_statement;
    public string raw_data { get; set; }
    public CParserBranch()
    {
      name = "";
      coord = new uint[2];
      norm_statement = new SortedDictionary<uint, string>();
      branch_statement = null;
      raw_data = "";
    }
  }
  // CParser class equal the source file
  // it contain: regex value for filter
  // List of class ParserFunction
  // string raw_data = content of input file
  // CParser also contain class draw used to draw the flow chart
  // input source file name = 
  // 1. output vsdx file name (each function flowchart shall be in 1 sheet)
  // or 2. folder name (each function flowchart shall be an image)
  class CParser : IEquatable<CParser>
  {
    const string st_func_parse = "";
    const string st_statement_parse = "";
    const string st_if_parse = "";
    const string st_while_parse = "";
    const string st_do_parse = "";
    const string st_for_parse = "";
    const string st_switch_parse = "";
    const string st_enum_parse = "";
    const string st_struct_parse = "";
    const string st_union_parse = "";
    public string name { get; set; }
    public string full_name { get; set; }
    // last modified change => new data is import =>  update the notify flag for parser function
    private string hid_last_modified;
    public string last_modified
    {
      get { return this.hid_last_modified; }
      set
      {
        this.flag_updated = true;
        this.hid_last_modified = value;
      }
    }
    private bool flag_updated;
    List<CParserFunction> ParseFuncList;
    CDraw Draw;
    public string raw_data { get; set; }

    // Constructor
    public CParser()
    {
      name = "";
      full_name = "";
      last_modified = "";
      ParseFuncList = new List<CParserFunction>();
      Draw = new CDraw();
      raw_data = "";
      flag_updated = false;
    }

    // delegate for checkexist of CParser with same full_name : IEquatable<CParser>
    public bool Equals(CParser other)
    {
      if (other == null) return false;
      return (this.full_name.Equals(other.full_name));
    }
    // parse data in raw_data
    public bool Parse_Data()
    {
      bool sts_return = false;
      //new data available => parse
      if (flag_updated == true)
      {
        // parse data set flag_updated back to false(equal data is processed)
        flag_updated = false;

        // Using RegualarExpressions Namespace class to process data
        Regex mReg = new Regex(@"^\s+(\w+)\s+([a-zA-Z0-9_)+\s+\(");












        sts_return = true;
      }
      else
      {
        sts_return = false;
      }
      return sts_return;
    }
    private void _strip_comment()
    {
      const string re_inline_com = @"[\s\t]*\/\/.+$";
      const string re_multline_com = @"\/\*([\s\S]*?)\*\/";
    }
  }
}
