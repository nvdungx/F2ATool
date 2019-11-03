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
  enum branch_type
  {
    IF,
    ELSE,
    ELSE_IF,
    DO_WHILE,
    WHILE,
    FOR,
    SWITCH,
    CASE,
    NONE,
    UNVALID
  }
  // Function class
  // name, return_type, input argument dict
  // coord(start/end row of function in file)
  // sorteddictionary of normal statement(row_index,data)
  // sorteddictionary of branch statement(row_index,class branch)
  class CParserFunction
  {
    const string st_func_name = @"\b(\w+)[\s\t]*\(";

    const string st_if_inline = @"^[\s\t]*if[\s\t]*\((.+?)\)[\s\t]+([^{]+;)$";
    const string st_while_inline = @"^[\s\t]*while[\s\t]*\((.+?)\)[\s\t]+([^{]+;)$";
    const string st_do_inline = @"^[\s\t]*do([\s\t]+)([^{]+;)$";
    const string st_else_inline = @"^[\s\t]*else([\s\t]+)([^{]+;)$";
    const string st_elseif_inline = @"^[\s\t]*else[\s\t]+if[\s\t]*\((.+?)\)[\s\t]+([^{]+;)$";

    const string st_if_block = @"^[\s\t]*if[\s\t]*\((.+)\)[\s\t]*{[\s\t]*$";
    const string st_else_block = @"^[\s\t]*else([\s\t]*){[\s\t]*$";
    const string st_elseif_block = @"^[\s\t]*else[\s\t]+if[\s\t]*\((.+?)\)[\s\t]*{[\s\t]*$";
    const string st_do_block = @"^[\s\t]*do([\s\t]*){[\s\t]*$";
    const string st_while_block = @"^[\s\t]*while[\s\t]*\((.+?)\)[\s\t]*{[\s\t]*$";
    const string st_switch_block = @"^[\s\t]*switch[\s\t]*\((.+?)\)[\s\t]*{[\s\t]*$";

    string block_string;
    bool block_flag;
    uint block_num;

    public bool open_brace;
    public bool close_brace;
    private uint brace_depth;
    private CParserBranch current_active_branch;
    public string name;
    public string return_type;
    public List<string>[] input_arg;
    public uint[] coord;
    public SortedDictionary<uint, string> statement_list;
    public SortedDictionary<uint, CParserBranch> branch_list;
    //public string raw_data { get; set; }
    public CParserFunction()
    {
      block_string = "";
      block_flag = false;
      block_num = 0;
      open_brace = false;
      close_brace = false;
      current_active_branch = null;
      brace_depth = 0;
      name = "";
      return_type = "";
      input_arg = new List<string>[2];
      input_arg[0] = new List<string>();
      input_arg[1] = new List<string>();
      coord = new uint[2];
      statement_list = new SortedDictionary<uint, string>();
      branch_list = new SortedDictionary<uint, CParserBranch>();
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
    public bool process_func_semicolon(string instring, UInt32 line_count)
    {
      bool result;
      result = false;
      string[] check_st = { "for", "switch", "if", "else", "do", "while", "case" };
      string[] output_st = new string[3];
      branch_type type_br = branch_type.UNVALID;
      CParserBranch tempBranch;
      bool return_flag = false;
      // statement outside codeblock in function >>
      // 1. normal statement int a; a = c+c; (void)function(1,2);
      // 2. if a++; else z++; for(a=0; a<10; a++) z=a;
      // NOTE: else = has to follow an if
      if (this.block_string != "")
      {
        this.block_string += instring;
        if (this.block_flag == false)
        {
          this.statement_list.Add(line_count, this.block_string);
          this.block_string = "";
        }
        result = true;
      }
      else
      {
        if (this.current_active_branch == null)
        {
          // inline branch with 1 statement
          // normal statement
          for (int i = 0; i < 7; i++)
          {
            if (instring.Contains(check_st[i]))
            {
              return_flag = parse_branch_semicolon(instring, ref output_st, ref type_br);
              if (return_flag == true)
              {
                result = add_function_branch(output_st, type_br);
                break;
              }
            }
          }
          if (return_flag == false)
          {
            this.statement_list.Add(line_count, instring);
            result = true;
          }
        }
        // statement inside codeblock
        // 1. normal statement int a; a = c+c; (void)function(1,2); >> add to  list normal statement of code block
        // 2. if a++; else z++; for(a=0; a<10; a++) z=a; >> add to  list branch of code block
        else
        {
          // inline branch with 1 statement
          // normal statement
          for (int i = 0; i < 7; i++)
          {
            if (instring.Contains(check_st[i]))
            {
              return_flag = parse_branch_semicolon(instring, ref output_st, ref type_br);
              if (return_flag == true)
              {
                result = add_sub_branch(output_st, type_br);
                break;
              }
            }
          }
          if (return_flag == false)
          {
            this.current_active_branch.sub_statement_list.Add(line_count, instring);
            result = true;
          }
        }
      }
      return result;
    }
    private bool add_function_branch(string[] string_arr, branch_type rtbranch)
    {
      bool result = true;
      return result;
    }
    private bool add_sub_branch(string[] string_arr, branch_type rtbranch)
    {
      bool result = true;
      // CASE HAS to be inside (sub branch of switch)
      /*if (this.brace_depth == 0)
      {
        result = false;
      }
      // block code inside blockcode
      else
      {
        if (this.current_active_branch.type == branch_type.SWITCH)
        {
          this.current_active_branch.sub_branch_list.Add(line_count, tempBranch);
          this.current_active_branch = this.current_active_branch.sub_branch_list.Values.Last<CParserBranch>();
          this.brace_depth++;
          result = true;
        }
        else
        {
          // active branch has to be a switch
          result = false;
        }
      }*/
      return result;
    }
    private bool parse_branch_semicolon(string instring, ref string[] output, ref branch_type rtbranch)
    {
      // case2. if else else if do while follow up by 1 statement
      // case3. for (count = 0;
      // case4. switch () (case :)+ statement; switch () default: statement;
      bool rst = false;
      // case 2
      string[] validate_string = new string[5] {st_if_inline, st_else_inline, st_elseif_inline, st_do_inline, st_while_inline };
      branch_type[] return_type = { branch_type.IF, branch_type.ELSE, branch_type.ELSE_IF, branch_type.DO_WHILE, branch_type.WHILE };
      Regex mReg;
      Match matchGroup;
      for (int i =0; i < 5; i++)
      {
        mReg = new Regex(validate_string[i]);
        matchGroup = mReg.Match(instring);
        if (matchGroup.Success == true)
        {
          rtbranch = return_type[i];
          output[0] = matchGroup.Groups[1].Value;
          output[1] = matchGroup.Groups[2].Value;
          rst = true;
          break;
        }
      }
      // case 3
      if (rst == false)
      {
        mReg = new Regex(@"[\s\t]*for[\s\t]*\((.*;)");
        matchGroup = mReg.Match(instring);
        if (matchGroup.Success == true)
        {
          // notify back this is for loop-> looking for next required data before process
          rtbranch = branch_type.FOR;
          output[0] = matchGroup.Groups[1].Value;
          output[1] = "";
          rst = true;
        }
        else
        {
          // case 4
          mReg = new Regex(@"[\s\t]*switch[\s\t]*\((.+?)\)[\s\t]*case(.+):(.+;)");
          matchGroup = mReg.Match(instring);
          if (matchGroup.Success == true)
          {
            // notify back this is switch case-> looking for next required data before process
            rtbranch = branch_type.SWITCH;
            output[0] = matchGroup.Groups[1].Value;
            output[1] = matchGroup.Groups[2].Value;
            output[2] = matchGroup.Groups[3].Value;
            rst = true;
          }
          else
          {
            mReg = new Regex(@"[\s\t]*switch[\s\t]*\((.+?)\)[\s\t]*(default):(.+;)");
            matchGroup = mReg.Match(instring);
            if (matchGroup.Success == true)
            {
              rtbranch = branch_type.SWITCH;
              output[0] = matchGroup.Groups[1].Value;
              output[1] = matchGroup.Groups[2].Value;
              output[2] = matchGroup.Groups[3].Value;
              rst = true;
            }
            else
            {
              mReg = new Regex(@"[\s\t]*case(.*):(.+;)");
              matchGroup = mReg.Match(instring);
              if (matchGroup.Success == true)
              {
                rtbranch = branch_type.CASE;
                output[0] = matchGroup.Groups[1].Value;
                output[1] = matchGroup.Groups[2].Value;
                rst = true;
              }
            }
          }
        }
      }
      return rst;
    }
    public bool process_func_openbrace(string instring, UInt32 line_count)
    {
      bool result;
      result = false;
      CParserBranch tempBranch;
      string branch_content = "";
      bool rst = false;
      branch_type block_type = branch_type.UNVALID;
      rst = parse_openbrace(instring, ref branch_content, ref block_type);
      if (rst == true)
      {
        tempBranch = new CParserBranch(null);
        tempBranch.branch_depth = this.brace_depth;
        tempBranch.open_brace = true;
        tempBranch.close_brace = false;
        tempBranch.coord[0] = line_count;
        tempBranch.name = branch_content;
        tempBranch.parent_branch = null;
        tempBranch.type = block_type;
        switch (block_type)
        {
          case branch_type.IF:
            {
              // new block code
              if (this.brace_depth == 0)
              {
                this.branch_list.Add(line_count, tempBranch);
                this.current_active_branch = this.branch_list.Values.Last<CParserBranch>();
                this.brace_depth++;
              }
              // block code inside blockcode
              else
              {
                tempBranch.parent_branch = this.current_active_branch;
                this.current_active_branch.sub_branch_list.Add(line_count, tempBranch);
                this.current_active_branch = this.current_active_branch.sub_branch_list.Values.Last<CParserBranch>();
                brace_depth++;
              }
              result = true;
            }
            break;
          case branch_type.ELSE:
            {
              // new block code
              if (this.brace_depth == 0)
              {
                if (this.branch_list.Count > 0)
                {
                  if (((this.branch_list.Values.Last<CParserBranch>().type == branch_type.IF)
                    || (this.branch_list.Values.Last<CParserBranch>().type == branch_type.ELSE_IF))
                    && (this.branch_list.Values.Last<CParserBranch>().close_brace == true))
                  {
                    this.branch_list.Add(line_count, tempBranch);
                    this.current_active_branch = this.branch_list.Values.Last<CParserBranch>();
                    this.brace_depth++;
                    result = true;
                  }
                  else
                  {
                    result = false;
                  }
                }
                else
                {
                  // no branch exist but and else is first >> error
                  result = false;
                }
              }
              // block code inside blockcode
              else
              {
                if (this.current_active_branch.sub_branch_list.Count > 0)
                {
                  if (((this.current_active_branch.sub_branch_list.Values.Last<CParserBranch>().type == branch_type.IF)
                    || (this.current_active_branch.sub_branch_list.Values.Last<CParserBranch>().type == branch_type.ELSE_IF))
                    && (this.current_active_branch.sub_branch_list.Values.Last<CParserBranch>().close_brace == true))
                  {
                    tempBranch.parent_branch = this.current_active_branch;
                    this.current_active_branch.sub_branch_list.Add(line_count, tempBranch);
                    this.current_active_branch = this.current_active_branch.sub_branch_list.Values.Last<CParserBranch>();
                    this.brace_depth++;
                    result = true;
                  }
                  else
                  {
                    result = false;
                  }
                }
                else
                {
                  // no branch exist but and else is first >> error
                  result = false;
                }
              }
            }
            break;
          case branch_type.ELSE_IF:
            {
              // new block code
              if (this.brace_depth == 0)
              {
                if (this.branch_list.Count > 0)
                {
                  if ((this.branch_list.Values.Last<CParserBranch>().type == branch_type.IF)
                    && (this.branch_list.Values.Last<CParserBranch>().close_brace == true))
                  {
                    this.branch_list.Add(line_count, tempBranch);
                    this.current_active_branch = this.branch_list.Values.Last<CParserBranch>();
                    this.brace_depth++;
                    result = true;
                  }
                  else
                  {
                    result = false;
                  }
                }
                else
                {
                  // no branch exist but and else is first >> error
                  result = false;
                }
              }
              // block code inside blockcode
              else
              {
                if (this.current_active_branch.sub_branch_list.Count > 0)
                {
                  if ((this.current_active_branch.sub_branch_list.Values.Last<CParserBranch>().type == branch_type.IF)
                    && (this.current_active_branch.sub_branch_list.Values.Last<CParserBranch>().close_brace == true))
                  {
                    tempBranch.parent_branch = this.current_active_branch;
                    this.current_active_branch.sub_branch_list.Add(line_count, tempBranch);
                    this.current_active_branch = this.current_active_branch.sub_branch_list.Values.Last<CParserBranch>();
                    this.brace_depth++;
                    result = true;
                  }
                  else
                  {
                    result = false;
                  }
                }
                else
                {
                  // no branch exist but and else is first >> error
                  result = false;
                }
              }
            }
            break;
          case branch_type.DO_WHILE:
          case branch_type.WHILE:
          case branch_type.FOR:
          case branch_type.NONE:
            {
              // new block code
              if (this.brace_depth == 0)
              {
                this.branch_list.Add(line_count, tempBranch);
                this.current_active_branch = this.branch_list.Values.Last<CParserBranch>();
                this.brace_depth++;
                result = true;
              }
              // block code inside blockcode
              else
              {
                tempBranch.parent_branch = this.current_active_branch;
                this.current_active_branch.sub_branch_list.Add(line_count, tempBranch);
                this.current_active_branch = this.current_active_branch.sub_branch_list.Values.Last<CParserBranch>();
                brace_depth++;
                result = true;
              }
            }
            break;
          case branch_type.SWITCH:
            {
              // new block code
              if (this.brace_depth == 0)
              {
                if (tempBranch.name == "switch_case_{")
                {
                  Regex tempReg = new Regex(@"[\s\t]*switch[\s\t]*\((.+)\)[\s\t]*case(.+):{");
                  tempBranch.name = tempReg.Match(instring).Groups[1].Value;
                  tempBranch.close_brace = true;
                  this.branch_list.Add(line_count, tempBranch);
                  this.brace_depth++;
                  this.current_active_branch = this.branch_list.Values.Last<CParserBranch>();

                  tempBranch = new CParserBranch(null);
                  tempBranch.branch_depth = this.brace_depth;
                  tempBranch.open_brace = true;
                  tempBranch.close_brace = false;
                  tempBranch.coord[0] = line_count;
                  tempBranch.name = tempReg.Match(instring).Groups[2].Value;
                  tempBranch.parent_branch = this.current_active_branch;
                  tempBranch.type = branch_type.CASE;
                  this.current_active_branch.sub_branch_list.Add(line_count, tempBranch);
                  this.current_active_branch = this.current_active_branch.sub_branch_list.Values.Last<CParserBranch>();
                  this.brace_depth++;
                  result = true;
                }
                else
                {
                  this.branch_list.Add(line_count, tempBranch);
                  this.current_active_branch = this.branch_list.Values.Last<CParserBranch>();
                  this.brace_depth++;
                  result = true;
                }
              }
              // block code inside blockcode
              else
              {
                if (tempBranch.name == "switch_case_{")
                {
                  Regex tempReg = new Regex(@"[\s\t]*switch[\s\t]*\((.+)\)[\s\t]*case(.+):{");
                  tempBranch.name = tempReg.Match(instring).Groups[1].Value;
                  tempBranch.close_brace = true;
                  this.current_active_branch.sub_branch_list.Add(line_count, tempBranch);
                  this.brace_depth++;
                  this.current_active_branch = this.current_active_branch.sub_branch_list.Values.Last<CParserBranch>();

                  tempBranch = new CParserBranch(null);
                  tempBranch.branch_depth = this.brace_depth;
                  tempBranch.open_brace = true;
                  tempBranch.close_brace = false;
                  tempBranch.coord[0] = line_count;
                  tempBranch.name = tempReg.Match(instring).Groups[2].Value;
                  tempBranch.parent_branch = this.current_active_branch;
                  tempBranch.type = branch_type.CASE;
                  this.current_active_branch.sub_branch_list.Add(line_count, tempBranch);
                  this.current_active_branch = this.current_active_branch.sub_branch_list.Values.Last<CParserBranch>();
                  this.brace_depth++;
                  result = true;
                }
                else
                {
                  tempBranch.parent_branch = this.current_active_branch;
                  this.current_active_branch.sub_branch_list.Add(line_count, tempBranch);
                  this.current_active_branch = this.current_active_branch.sub_branch_list.Values.Last<CParserBranch>();
                  brace_depth++;
                  result = true;
                }
              }
            }
            break;
          case branch_type.CASE:
            {
              // CASE HAS to be inside (sub branch of switch)
              if (this.brace_depth == 0)
              {
                result = false;
              }
              // block code inside blockcode
              else
              {
                if (this.current_active_branch.type == branch_type.SWITCH)
                {
                  this.current_active_branch.sub_branch_list.Add(line_count, tempBranch);
                  this.current_active_branch = this.current_active_branch.sub_branch_list.Values.Last<CParserBranch>();
                  this.brace_depth++;
                  result = true;
                }
                else
                {
                  // active branch has to be a switch
                  result = false;
                }
              }
            }
            break;
          default:
            break;
        }
      }
      else
      {
        // check if this is enum or struct .. definition
        result = check_struct(instring, line_count);
      }
      return result;
    }
    private bool check_struct(string instring, UInt32 line_num)
    {
      bool return_flag;
      return_flag = false;
      Regex tempReg = new Regex(@".*{");
      if (tempReg.IsMatch(instring))
      {
        this.block_flag = true;
        this.block_num++;
        this.block_string += instring;
        return_flag = true;
      }
      return return_flag;
    }
    private bool parse_openbrace(string parsestring, ref string output, ref branch_type rtbranch)
    {
      bool result;
      result = false;
      string[] validate_string = new string[6] { st_if_block, st_else_block, st_elseif_block, st_do_block, st_while_block, st_switch_block};
      branch_type[] return_type = { branch_type.IF, branch_type.ELSE, branch_type.ELSE_IF, branch_type.DO_WHILE, branch_type.WHILE, branch_type.SWITCH };
      Regex mReg;
      /*typical if (){  else { else if () { do { while { switch () { for {*/
      for (int i = 0; i < 6; i++)
      {
        mReg = new Regex(validate_string[i]);
        Match matchGroup = mReg.Match(parsestring);
        if (matchGroup.Success == true)
        {
          rtbranch = return_type[i];
          output = matchGroup.Groups[1].Value;
          result = true;
          break;
        }
      }
      if (result == false)
      {
        mReg = new Regex(@"[\s\t]*for[\s\t]*\((.+)\)[\s\t]*{");
        Match matchGroup = mReg.Match(parsestring);
        if (matchGroup.Success == true)
        {
          rtbranch = branch_type.FOR;
          output = matchGroup.Groups[1].Value;
          result = true;
        }
      }
      if (result == false)
      {
        mReg = new Regex(@"[\s\t]*case[\s\t]*(.+):[\s\t]*{");
        if (mReg.Match(parsestring).Success == true)
        {
          rtbranch = branch_type.CASE;
          output = mReg.Match(parsestring).Groups[1].Value;
          result = true;
        }
      }
      // specical case switch () case ADD: {
      if (result == false)
      {
        mReg = new Regex(@"[\s\t]*switch[\s\t]*\((.+)\)[\s\t]*case.*{");
        if (mReg.Match(parsestring).Success == true)
        {
          rtbranch = branch_type.SWITCH;
          output = "switch_case_{";
          result = true;
        }
      }
      // remaining case asd: {  NONE {
      if (result == false)
      {
        if (parsestring == "{")
        {
          rtbranch = branch_type.NONE;
          output = "";
          result = true;
        }
      }
      return result;
    }
    public bool process_func_closebrace(string instring, UInt32 line_count)
    {
      bool rst = false;
      if (this.block_flag == true)
      {
        // continue to next ;
        this.block_string += instring;
        this.block_num--;
        if (this.block_num == 0)
        {
          this.block_flag = false;
        }
        rst = true;
      }
      else
      {
        if (this.branch_list.Count == 0)
        {
          this.close_brace = true;
          this.coord[1] = line_count;
          rst = true;
        }
        else
        {
          if (this.brace_depth == 0)
          {
            if (this.close_brace == false)
            {
              this.close_brace = true;
              this.coord[1] = line_count;
              rst = true;
            }
            else
            {
              rst = false;
            }
          }
          else
          {
            this.current_active_branch.close_brace = true;
            this.current_active_branch.coord[1] = line_count;
            this.brace_depth--;
            rst = true;
            if (this.current_active_branch.parent_branch != null)
            {
              this.current_active_branch = this.current_active_branch.parent_branch;
            }
            else
            {
              this.current_active_branch = null;
            }
          }
        }
      }
      return rst;
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
    public bool open_brace;
    public bool close_brace;
    public string name;
    public branch_type type;
    public uint[] coord;
    public uint branch_depth;
    public SortedDictionary<uint, string> sub_statement_list;
    public SortedDictionary<uint, CParserBranch> sub_branch_list;
    public CParserBranch parent_branch;
    //public string raw_data { get; set; }
    public CParserBranch(CParserBranch parent_branch)
    {
      open_brace = false;
      close_brace = false;
      this.parent_branch = parent_branch;
      branch_depth = 0;
      name = "";
      type = branch_type.NONE;
      coord = new uint[2];
      sub_statement_list = new SortedDictionary<uint, string>();
      sub_branch_list = new SortedDictionary<uint, CParserBranch>();
      //raw_data = "";
    }
    public CParserBranch()
    {
      open_brace = false;
      close_brace = false;
      this.parent_branch = null;
      branch_depth = 0;
      name = "";
      type = branch_type.NONE;
      coord = new uint[2];
      sub_statement_list = new SortedDictionary<uint, string>();
      sub_branch_list = new SortedDictionary<uint, CParserBranch>();
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
    List<string> warning_log;
    string st_outside_func;
    UInt32 txt_line;
    UInt32 current_line;
    bool detect_func;
    bool detect_block;
    UInt32 number_brace;
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
      number_brace = 0;
      detect_func = false;
      detect_block = false;
      txt_line = 0;
      current_line = 0;
      st_outside_func = "";
      name = "";
      full_name = "";
      last_modified = "";
      warning_log = new List<string>();
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

        const Int32 BufferSize = 1024;
        using (var fileStream = File.OpenRead(this.full_name))
        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
        {
          string line = "";
          string target_string = "";
          string current_statement = "";
          bool valid_target = false;
          bool multline_cmt_flag = false;
          bool statement_end = false;
          // variable used as global scrope to track in sub function _process_statement since no static in function allowed.
          // could move to private properties
          bool macro_detect = false;
          while ((line = streamReader.ReadLine()) != null)
          {
            txt_line++;
            current_line++;
            target_string = "";
            if ((string.IsNullOrEmpty(line) == false) && (string.IsNullOrWhiteSpace(line) == false))
            {
              valid_target = false;
              if (multline_cmt_flag == false)
              {
                // limitation : error if having string "" which contain comment like syntax
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
                if (true == _check_statement(ref target_string, ref statement_end, macro_detect))
                {
                  if (statement_end == true)
                  {
                    current_statement = current_statement + " " + target_string;
                    current_statement = current_statement.Trim();
                    // process a completed statement
                    // return true/false invalid syntax in source
                    bool temp = true;
                    temp = _process_statement(current_statement);
                    if (temp == false)
                    {
                      System.Windows.Forms.MessageBox.Show(string.Format("Error syntax in file {0}, line {1}", this.name, txt_line));
                      break;
                    }
                    else
                    {
                      current_statement = "";
                    }
                  }
                  else
                  {
                    current_statement = current_statement + " " + target_string;
                  }
                }
                else
                {
                  if (statement_end == true)
                  {
                    macro_detect = false;
                    current_statement = "";
                  }
                  else
                  {
                    macro_detect = true;
                  }
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

    private bool _process_statement(string instring)
    {
      // line_number = current statement line in file
      // func_start = global scrope var of this function used to trace if function staart
      // block_start = global scrope var of this function used to trace if a block code staart
      bool return_result = true;
      string warning_msg = "";
      string[] statement_arr;
      char[] list_char = { ';', '{', '}' };
      string full_statement = "";
      bool proceed_flag = false;
      // incase of input is an multiple code statement in same line of code - split it to array
      // and put to process_statement function
      statement_arr = Regex.Split(instring, @"(?<=[;{}])");
      int arr_len = statement_arr.Length;
      // last element is empty string
      if (arr_len >= 2)
      {
        arr_len--;
      }
      if (Regex.IsMatch(instring, @"[\s\t]*for[\s\t]*\(.*\)[\s\t]*{[\s\t]*"))
      {
        return_result = _process_open_brace(instring, ref warning_msg);
        // warning message could be record for output log file later
        if (return_result == false)
        {
          warning_log.Add(warning_msg);
        }
      }
      else
      {
        for (int i = 0; i < arr_len; i++)
        {
          full_statement += statement_arr[i];
          // incase of string " " migh contain ; {}
          if (Regex.IsMatch(full_statement, @"[^\\]"""))
          {
            // contain string
            if (Regex.Matches(full_statement, @"[^\\]""").Count % 2 == 0)
            {
              proceed_flag = true;
            }
            else
            {
              proceed_flag = false;
            }
          }
          else
          {
            proceed_flag = true;
          }
          if (full_statement.Length > 2)
          {
            if (full_statement[full_statement.Length - 2] == '\'')
            {
              proceed_flag = false;
            }
          }
          if (proceed_flag == true)
          {
            switch (full_statement.Last<char>())
            {
              case ('{'):
                {
                  return_result = _process_open_brace(full_statement, ref warning_msg);
                  // warning message could be record for output log file later
                  if (return_result == false)
                  {
                    warning_log.Add(warning_msg);
                  }
                  break;
                }
              case (';'):
                {
                  return_result = _process_semicolon(full_statement, ref warning_msg);
                  // warning message could be record for output log file later
                  if (return_result == false)
                  {
                    warning_log.Add(warning_msg);
                  }
                  break;
                }
              case ('}'):
                {
                  return_result = _process_close_brace(full_statement, ref warning_msg);
                  if (return_result == false)
                  {
                    warning_log.Add(warning_msg);
                  }
                  break;
                }
              default:
                // nothing
                break;
            }
            full_statement = "";
          }
        }
      }
      if (warning_log.Count > 0)
      {
        return_result = false;
      }
      else
      {
        return_result = true;
      }
      return return_result;
    }
    private bool _process_open_brace(string instring, ref string warning_msg)
    {
      bool result_return = false;
      // if there is not function detect yet then 
      // 1. this might be a start of function
      // 2. this might be a start of block code out side of function (i.e struct,enum,union,array definition/declaration)
      if (this.detect_func == false)
      {
        Regex mReg = new Regex(@"([\s\t]*\w+[\s\t]*)+\([^=;<>]+\)[\s\t]*{");
        // this is valid function
        if (mReg.IsMatch(instring))
        {
          bool result;
          CParserFunction newFunc = new CParserFunction();
          // set start row of function
          newFunc.coord[0] = this.txt_line;
          newFunc.open_brace = true;
          // get name, input parameters, return type of function
          result = newFunc.process_function_name(instring);
          if (result == true)
          {
            // add function to list
            ParseFuncList.Add(newFunc);
            // notify flag function begin from this line
            this.detect_func = true;
            this.detect_block = false;
            result_return = true;
          }
          else
          {
            warning_msg = string.Format("Warning: Function name syntax incorrect - line {0}", txt_line);
            result_return = false;
            // error in function name
          }
        }
        // it a struct/enum/union def outside of function
        else
        {
          // set block start collect until block end
          st_outside_func += instring;
          this.detect_block = true;
          this.number_brace++;
          this.detect_func = false;
          result_return = true;
        }
      }
      // if there is function detected then
      // 1. this is a code block in side function
      else
      {
        bool result;
        CParserFunction currentFunc = ParseFuncList.Last<CParserFunction>();
        result = currentFunc.process_func_openbrace(instring, current_line);
        this.current_line++;
        if (result == false)
        {
          warning_msg = string.Format("Warning: Branch syntax incorrect - line {0}", txt_line);
          result_return = false;
        }
        else
        {
          result_return = true;
        }
      }
      return result_return;
    }
    private bool _process_semicolon(string instring, ref string warning_msg)
    {
      bool result_return = false;
      // this is end of a statement outside function
      if (this.detect_func == false)
      {
        // if block detect
        if (this.detect_block == true)
        {
          // skip wait until block close
          st_outside_func += instring;
          result_return = true;
        }
        // if block brace is closed or not exist
        else
        {
          result_return = true;
          st_outside_func += instring;
          GlobalStatement.Add(this.txt_line, st_outside_func);
          st_outside_func = "";
        }
      }
      // this is end of a statement inside function
      else
      {
        bool result;
        CParserFunction currentFunc = ParseFuncList.Last<CParserFunction>();
        result = currentFunc.process_func_semicolon(instring, current_line);
        this.current_line++;
        if (result == false)
        {
          warning_msg = string.Format("Warning: Statement syntax incorrect - line {0}", txt_line);
          result_return = false;
        }
        else
        {
          result_return = true;
        }
      }
      return result_return;
    }
    private bool _process_close_brace(string instring, ref string warning_msg)
    {
      bool result_return = false;
      // this is end of a statement outside function
      if (this.detect_func == false)
      {
        if (this.detect_block == true)
        {
          // continue to next ;
          st_outside_func += instring;
          this.number_brace--;
          if(this.number_brace == 0)
          {
            this.detect_block = false;
          }
          result_return = true;
        }
        else
        {
          // error closing brace with out open brace
          st_outside_func += instring;
          warning_msg = string.Format("Warning: Closing brace without open brace syntax incorrect - line {0}", txt_line);
          result_return = false;
        }
      }
      // this is end of a statement inside function
      else
      {
        bool result;
        CParserFunction currentFunc = ParseFuncList.Last<CParserFunction>();
        result = currentFunc.process_func_closebrace(instring, current_line);
        this.current_line++;
        if (currentFunc.close_brace == true)
        {
          this.detect_func = false;
        }
        else
        {
          // pass
        }
        if (result == false)
        {
          warning_msg = string.Format("Warning: Branch syntax incorrect - line {0}", txt_line);
          result_return = false;
        }
        else
        {
          result_return = true;
        }
      }
      return result_return;
    }
    private bool _check_statement(ref string instring, ref bool statement_end, bool macro_flag)
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
            statement_end = false;
          }
          else
          {
            statement_end = true;
          }
          result = false;
        }
      }
      else
      {
        if (endlinechar == '\\')
        {
          statement_end = false;
        }
        else
        {
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
      const string re_inline_com = @"[\s\t]*\/\/.+";
      const string re_inline_com2 = @"[\s\t]*\/\*.+\*\/";
      string temp;
      Regex mReg_inline = new Regex(re_inline_com);
      Regex mReg_inline2 = new Regex(re_inline_com2);
      temp = mReg_inline.Replace(instring, "");
      temp = mReg_inline2.Replace(temp, "");
      instring = temp;
    }
  }
}
