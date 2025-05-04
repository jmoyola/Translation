using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TranslationService.Core;

public class PlaceAndHolder:Dictionary<string,Object>
{
    public string Replace(string value)
    {
        Regex phRegex=new Regex(@"{[a-zA-Z0-9_]+}");
        
        MatchCollection mc=phRegex.Matches(value);
        foreach (var m in mc.Cast<Match>())
        {
            if (ContainsKey(m.Value))
                value = value.Replace(m.Value, this[m.Value]==null?"":this[m.Value].ToString());
        }

        return value;
    }
    
    public StringBuilder Replace(StringBuilder value)
    {
        Regex phRegex=new Regex(@"\{[a-zA-Z0-9_]+\}]\}");
        
        MatchCollection mc=phRegex.Matches(value.ToString());
        foreach (var m in mc.Cast<Match>())
        {
            if (ContainsKey(m.Value))
                value = value.Replace(m.Value, this[m.Value]==null?"":this[m.Value].ToString());
        }

        return value;
    }
}