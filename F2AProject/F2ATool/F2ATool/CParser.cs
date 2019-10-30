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
    public void process_function_name(string instring)
    {
      string tempArg;
      string[] listArg;
      Regex mReg = new Regex(st_func_name);
      name = mReg.Match(instring).Groups[1].Value;
      tempArg = mReg.Match(instring).Groups[0].Value;
      return_type = instring.Split(new string[] { name },StringSplitOptions.None)[0];
      tempArg = instring.Split(new string[] { tempArg }, StringSplitOptions.None)[1];
      mReg = new Regex(@"[\s\t]*\)[\s\t]*{?");
      tempArg = mReg.Split(tempArg)[0];
      listArg = tempArg.Split(',');
      foreach (var item in listArg)
      {
        string para_type = "";
        string para_name = "";
        mReg = new Regex(@"[a-zA-Z0-9*\[\]_\*]+");
        MatchCollection temp3 = mReg.Matches(item);
        //for (int i = 0; i < temp.Length - 1; i++)
        //{
        //  temp2 += temp[i];
        //}
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
          string line = "";
          string target_string = "";
          string current_statement = "";
          bool valid_target = false;
          bool multline_cmt_flag = false;
          bool statement_end = false;
          // variable used to track in sub function
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
                  if (statement_end == true)
                  {
                    current_statement = current_statement + " " + target_string;
                    current_statement = current_statement.Trim();
                    // process a completed statement
                    _process_statement(current_statement, line_count, ref detect_func, ref statement_end, ref detect_block);
                    if (statement_end == true)
                    {
                      current_statement = "";
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
    private void _process_statement(string statement, UInt32 line_number, ref bool func_start, ref bool statement_end, ref bool block_start)
    {
      if (func_start == false)
      {
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
            GlobalStatement.Add(line_number, statement);
          }
        }
        // this could be either start of function or start of block defintion(array,struct ..)
        else if (statement.Last<char>() == '{')
        {
          if (block_start == false)
          {
            Regex mReg = new Regex(@"[^{}=();]+\([^=;<>]+\)[\s\t]*{");
            if (mReg.IsMatch(statement))
            {
              CParserFunction newFunc = new CParserFunction();
              // set start row of function
              newFunc.coord[0] = line_number;
              // get name, input parameters, return type of function
              newFunc.process_function_name(statement);
              // add function to list
              ParseFuncList.Add(newFunc);
              // notify flag function begin from this line
              func_start = true;
              statement_end = true;
              block_start = false;
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
            // process function
            block_start = true;
            statement_end = false;
          }
          else
          {
            Regex mReg = new Regex(@"([^{}=();]+\([^=;<>]+\))[\s\t]*{([\s\S]*)}");
            if (mReg.IsMatch(statement))
            {
              Match temp = mReg.Match(statement);
              CParserFunction newFunc = new CParserFunction();
              // set start row of function
              newFunc.coord[0] = line_number;
              // get name, input parameters, return type of function
              newFunc.process_function_name(temp.Groups[1].Value);
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
        // handle statement in function
        if (statement.Last<char>() == ';')
        {

        }
        // start of code block
        else if (statement.Last<char>() == '{')
        {

        }
        // end of codd block or function
        else if (statement.Last<char>() == '}')
        {

          func_start = false;
        }
        else
        {
          // pass
        }
      }
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
