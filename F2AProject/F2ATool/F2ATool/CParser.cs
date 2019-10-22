using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO.Packaging;

namespace F2ATool
{
    class CParserFunction
    {
        string name;
        string return_type;
        string[] input_arg;
        uint[] coord;
        Dictionary<uint, string> norm_statement;
        Dictionary<uint, CParserBranch> branch_statement;

        string raw_data;
        CParserFunction()
        {
            coord = new uint[2];
            norm_statement = new Dictionary<uint, string>();
        }

    }
    class CParserBranch
    {
        string name;
        uint[] coord;
        Dictionary<uint, string> norm_statement;
        Dictionary<uint, CParserBranch> branch_statement;
        string raw_data;
        CParserBranch()
        {
            coord = new uint[2];
            norm_statement = new Dictionary<uint, string>();
        }
    }
    class CParser
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
        
    }
}
