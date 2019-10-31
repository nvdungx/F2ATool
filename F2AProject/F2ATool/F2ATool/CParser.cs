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

namespace F2ATool
{
  // Function class
  // name, return_type, input argument dict
  // coord(start/end row of function in file)
  // sorteddictionary of normal statement(row_index,data)
  // sorteddictionary of branch statement(row_index,class branch)
  class CParserFunction
  {
    const string st_func_parse = @"[^{};#<>]+\([^<>=;+-/*)]+\)[\r\n]*[\s\t]*{";
    const string st_func_name = @"\b(\w+)[\s\t]*\(";
    public string name;
    public string return_type;
    public List<string>[] input_arg;
    public uint[] coord;
    public SortedDictionary<uint, string> norm_statement;
    public SortedDictionary<uint, CParserBranch> branch_statement;
    //public string raw_data { get; set; }
    public CParserFunction()
    {
      name = "";
      return_type = "";
      input_arg = new List<string>[2];
      input_arg[0] = new List<string>();
      input_arg[1] = new List<string>();
      coord = new uint[2];
      norm_statement = new SortedDictionary<uint, string>();
      branch_statement = new SortedDictionary<uint, CParserBranch>();
      //raw_data = "";
    }
    public bool process_function_name(string instring)
    {
      bool result;
      string tempArg;
      string[] listArg;
      Regex mReg = new Regex(st_func_name);
      Match temp = mReg.Match(instring);
      // check of function is valid > \b([a-zA-Z_]\w+)[\s\t]*\([.*]\)[\s\t]*{
      // temp.Groups[1] -> shall be function name
      if (temp.Success == true)
      {
        name = temp.Groups[1].Value;
        // store original string to tempArg
        tempArg = temp.Groups[0].Value;
        // check return type here else return false
        return_type = instring.Split(new string[] { name }, StringSplitOptions.None)[0];
        // check argument list here else return false
        tempArg = instring.Split(new string[] { tempArg }, StringSplitOptions.None)[1];
        mReg = new Regex(@"[\s\t]*\)[\s\t]*{?");
        // check list argument here else return false
        tempArg = mReg.Split(tempArg)[0];
        listArg = tempArg.Split(',');
        foreach (var item in listArg)
        {
          string para_type = "";
          string para_name = "";
          mReg = new Regex(@"[a-zA-Z0-9*\[\]_\*]+");
          MatchCollection temp3 = mReg.Matches(item);
          if (temp3.Count == 1)
          {
            para_name = "";
            para_type = temp3[0].Value;
          }
          else
          {
            for (int i = 0; i < temp3.Count - 1; i++)
            {
              para_type += temp3[i].Value + " ";
            }
            para_name = temp3[temp3.Count - 1].Value;
          }
          input_arg[0].Add(para_type);
          input_arg[1].Add(para_name);
        }
        result = true;
      }
      else
      {
        // syntax of function is incorrect
        result = false;
      }
      return result;
    }
    public bool process_single_statement(string instring, UInt32 line_count, ref bool block_start_flag)
    {
      // case1: normal declaration/definition/func call int a; int a= 10; a = 0; function(asd);
      // case2: branch with single follow up statement for() statement; while() sts; if() sts; else sts; do sts;(required to follow by a while();) else
      // notify error, switch() case(): sts;
      //       //sub case: branch for(){ something;\r\n | switch(){ case(): something;\r\n | switch() { case(): { something;\r\n|
      // case3: single line of struct enum union def inside function
      bool result;
      Regex mReg;
      string[] mRegex = new string[3] { @"", @"", @"" };
      result = false;



      return result;
    }
    public bool process_branch_start(string instring, UInt32 line_count, ref bool block_start_flag)
    {
      return true;
    }
    public bool process_branch_end(string instring, UInt32 line_count, ref bool func_start_flag, ref bool block_start_flag)
    {
      return true;
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
    //public string raw_data { get; set; }
    public CParserBranch()
    {
      name = "";
      coord = new uint[2];
      norm_statement = new SortedDictionary<uint, string>();
      branch_statement = null;
      //raw_data = "";
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
    const string re_inline_com = @"[\s\t]*\/\/.+";
    const string re_inline_com2 = @"[\s\t]*\/\*.+\*\/";

    const string st_func_name = "";
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
    SortedDictionary<UInt32, string> GlobalStatement;
    List<CParserFunction> ParseFuncList;
    CDraw Draw;
    //public string raw_data { get; set; }

    // Constructor
    public CParser()
    {
      name = "";
      full_name = "";
      last_modified = "";
      ParseFuncList = new List<CParserFunction>();
      GlobalStatement = new SortedDictionary<UInt32, string>();
      Draw = new CDraw();
      //raw_data = "";
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

        const Int32 BufferSize = 128;
        using (var fileStream = File.OpenRead(this.full_name))
        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
        {
          UInt32 line_count = 0;
          UInt32 current_line = 0;
          string line = "";
          string target_string = "";
          string current_statement = "";
          bool valid_target = false;
          bool multline_cmt_flag = false;
          bool statement_end = false;
          // variable used as global scrope to track in sub function _process_statement since no static in function allowed.
          // could move to private properties
          bool detect_func = false;
          bool macro_detect = false;
          bool detect_block = false;
          while ((line = streamReader.ReadLine()) != null)
          {
            line_count++;
            target_string = "";
            if ((string.IsNullOrEmpty(line) == false) && (string.IsNullOrWhiteSpace(line) == false))
            {
              valid_target = false;
              if (multline_cmt_flag == false)
              {
                _strip_comment(ref line);
                if (_check_multline_cmt(ref line) == true)
                {
                  multline_cmt_flag = true;
                }
                else
                {
                  // pass
                }
                target_string = line;
                // check the target_string if not empty or string of whitespace then process
                valid_target = _check_valid_string(target_string);
              }
              else
              {
                _strip_comment(ref line);
                if (_check_multline_cmt_end(ref line) == true)
                {
                  if (_check_multline_cmt(ref line) == true)
                  {
                    multline_cmt_flag = true;
                  }
                  else
                  {
                    multline_cmt_flag = false;
                  }
                  target_string = line;
                  valid_target = _check_valid_string(target_string);
                }
                else
                {
                  // skipp line
                }
              }
              if (valid_target)
              {
                // check statement in target_string
                if (true == _check_statement(ref target_string, ref statement_end, ref macro_detect))
                {
                  // on valid statement start set current line
                  if (current_line == 0)
                  {
                    current_line = line_count;
                  }
                  else
                  {
                    // no update current line untill statement end
                  }
                  if (statement_end == true)
                  {
                    current_statement = current_statement + " " + target_string;
                    current_statement = current_statement.Trim();
                    // process a completed statement
                    // return true/false invalid syntax in source
                    bool temp;
                    string[] statement_arr = new string[0];
                    // incase of input is an multiple code statement in same line of code - split it to array
                    // and put to process_statement function
                    _split_current_input(current_statement, ref statement_arr);
                    temp = _process_statement(statement_arr, ref current_line, ref detect_func, ref statement_end, ref detect_block);
                    if (temp == false)
                    {
                      System.Windows.Forms.MessageBox.Show(string.Format("Error syntax in file {0}, line {1}", this.name, line_count));
                      break;
                    }
                    if (statement_end == true)
                    {
                      //1. outside function this end when complete block (global declare of struct, enum, arr,...) or end of global declare statment
                      //2. inside function this end when complete statement
                      current_statement = "";
                      current_line = 0;
                    }
                    else
                    {
                      // statement not end yet, do nothing
                    }
                  }
                  else
                  {
                    current_statement = current_statement + " " + target_string;
                  }
                }
                else
                {
                  // do nothing on macro line
                }
              }
              else
              {
                // do nothing on empty string after remove comment
              }
            }
            else
            {
              // empty line
            }
          }
        }
        sts_return = true;
      }
      else
      {
        sts_return = false;
      }
      return sts_return;
    }
    private void _split_current_input(string instring, ref string[] out_arr)
    {
      
    }
    private bool _process_statement(string[] statement_arr, ref UInt32 line_number, ref bool func_start, ref bool statement_end, ref bool block_start)
    {
      bool return_result;
      return_result = true;
      if (func_start == false)
      {
        // NEED TO IMPROVE on Checking statement validity with each end-char case in case outside of function
        // global statement
        if (statement.Last<char>() == ';')
        {
          if (block_start == true)
          {
            // check if close block exit
            Regex mReg = new Regex(@".*}[^;]*;");
            if (mReg.IsMatch(statement))
            {
              block_start = false;
              statement_end = true;
              GlobalStatement.Add(line_number, statement);
            }
            else
            {
              block_start = true;
              statement_end = false;
            }
          }
          else
          {
            block_start = false;
            statement_end = true;
            GlobalStatement.Add(line_number, statement);
          }
        }
        // this could be either start of function or start of block defintion(array,struct ..)
        else if (statement.Last<char>() == '{')
        {
          if (block_start == false)
          {
            Regex mReg = new Regex(@"[^{}=();]+\([^=;<>]+\)[\s\t]*{");
            // this is valid function or it a struct/enum/union def
            if (mReg.IsMatch(statement))
            {
              bool result;
              CParserFunction newFunc = new CParserFunction();
              // set start row of function
              newFunc.coord[0] = line_number;
              // get name, input parameters, return type of function
              result = newFunc.process_function_name(statement);
              if (result == true)
              {
                // add function to list
                ParseFuncList.Add(newFunc);
                // notify flag function begin from this line
                func_start = true;
                statement_end = true;
                block_start = false;
              }
              else
              {
                // error in function name
                return_result = false;
              }
            }
            else
            {
              block_start = true;
              statement_end = false;
            }
          }
          else
          {
            block_start = true;
            statement_end = false;
          }
        }
        // end of block code with out function start
        // check if exist function else warning syntax error of code >> warning
        else if (statement.Last<char>() == '}')
        {
          if (block_start == true)
          {
            // continue to next ;
            block_start = true;
            statement_end = false;
          }
          else
          {
            // this is valid function in 1 line or it a struct/enum/union def
            // Ex: int tempFunction(void) { int a; a = 0; while(a<5) a++; return a;}
            Regex mReg = new Regex(@"([^{}=();]+\([^=;<>]+\)[\s\t]*{)([\s\S]*)}");
            if (mReg.IsMatch(statement))
            {
              bool result;
              Match temp = mReg.Match(statement);
              CParserFunction newFunc = new CParserFunction();
              // set start row of function
              newFunc.coord[0] = line_number;
              // get name, input parameters, return type of function
              result = newFunc.process_function_name(temp.Groups[1].Value);
              if (result == true)
              {
                // process statement in function string temp.Groups[2].Value;
                //temp.Groups[2].Value;
                // dumb case
                // input code: all on same line void sampleFunction(void) { statement; statement{ statement;} statement;}
                // process statement here


                // add function to list
                ParseFuncList.Add(newFunc);
                func_start = false;
                statement_end = true;
                block_start = false;
              }
              else
              {
                // error in function name
                return_result = false;
              }
            }
            else
            {
              // continue to next ;
              block_start = true;
              statement_end = false;
            }
          }
        }
        else
        {
          // pass
        }
      }
      else
      {
        // TODO: process the statement in side function
        return_result = _process_function_statement(statement, line_number, ref func_start, ref block_start);
      }
      return return_result;
    }

    private bool _process_function_statement(string fstatement, UInt32 fline_number, ref bool ffunc_start, ref bool fblock_start)
    {
      bool return_result;
      return_result = false;
      CParserFunction currentFunc =  ParseFuncList.Last<CParserFunction>();
      // handle statement in function
      if (fstatement.Last<char>() == ';')
      {
        // everything in a line and end with ;
        // case1: normal declaration/definition/func call int a; int a= 10; a = 0; function(asd);
        // case2: branch with single follow up statement for() statement; while() sts; if() sts; else sts; do sts;(required to follow by a while();) else
        // notify error, switch() case(): sts;
        //       //sub case: branch for(){ something;\r\n | switch(){ case(): something;\r\n | switch() { case(): { something;\r\n|
        // case3: single line of struct enum union def inside function
        return_result = currentFunc.process_single_statement(fstatement, fline_number, ref fblock_start);
      }
      // case1: start of code block  {\r\n for{\r\n if{\r\n else{\r\n while{\r\n do{\r\n switch{\r\n switch () case():{\r\n
      // case2: start of struct enum union def inside function
      else if (fstatement.Last<char>() == '{')
      {
        fblock_start = true;
        return_result = currentFunc.process_branch_start(fstatement, fline_number, ref fblock_start);
      }
      // case1: end of code block or function } 
      else if (fstatement.Last<char>() == '}')
      {
        return_result = currentFunc.process_branch_end(fstatement, fline_number, ref ffunc_start, ref fblock_start);
        ffunc_start = false;
      }
      else
      {
        // pass
      }
      return return_result;
    }

    private bool _check_statement(ref string instring, ref bool statement_end, ref bool macro_flag)
    {
      bool result;
      instring = instring.Trim();
      char endlinechar = instring.Last<char>();
      if (macro_flag == false)
      {
        if (instring.First<char>() != '#')
        {
          if ((endlinechar == ';') || (endlinechar == '{') || (endlinechar == '}'))
          {
            // end of statement
            statement_end = true;
          }
          else if (endlinechar == '\\')
          {
            instring = instring.TrimEnd('\\');
            instring = instring.Trim();
            statement_end = false;
          }
          else
          {
            statement_end = false;
          }
          result = true;
        }
        else
        {
          if (endlinechar == '\\')
          {
            macro_flag = true;
            statement_end = false;
          }
          else
          {
            macro_flag = false;
            statement_end = true;
          }
          result = false;
        }
      }
      else
      {
        if (endlinechar == '\\')
        {
          macro_flag = true;
          statement_end = false;
        }
        else
        {
          macro_flag = false;
          statement_end = true;
        }
        result = false;
      }
      return result;
    }
    private bool _check_valid_string(string instring)
    {
      if ((string.IsNullOrEmpty(instring) == true) || (string.IsNullOrWhiteSpace(instring) == true))
      {
        return false;
      }
      else
      {
        return true;
      }
    }
    private bool _check_multline_cmt_end(ref string instring)
    {
      bool result;
      Regex mReg = new Regex(@"[\s\t]*\*\/");
      if (mReg.IsMatch(instring) == true)
      {
        instring = mReg.Split(instring)[1];
        result = true;
      }
      else
      {
        result = false;
      }
      return result;
    }
    private bool _check_multline_cmt(ref string instring)
    {
      bool result;
      Regex mReg = new Regex(@"[\s\t]*\/\*");
      if (mReg.IsMatch(instring) == true)
      {
        instring = mReg.Split(instring)[0];
        result = true;
      }
      else
      {
        result = false;
      }
      return result;
    }
    private void _strip_comment(ref string instring)
    {
      string temp;
      Regex mReg_inline = new Regex(re_inline_com);
      Regex mReg_inline2 = new Regex(re_inline_com2);
      temp = mReg_inline.Replace(instring, "");
      temp = mReg_inline2.Replace(temp, "");
      instring = temp;
    }
  }
}
