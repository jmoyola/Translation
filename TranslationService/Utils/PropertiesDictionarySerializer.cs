using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TranslationService.Utils;

public class PropertiesDictionarySerializer:IDictionarySerializer<String, string>
{
    private readonly Regex _keyValueRegex;
    private readonly string _keyValueSeparator;
    public PropertiesDictionarySerializer(string keyValueSeparator="=", string keyValueRegexPattern="(\\w+)=(\\w+)")
    {
        _keyValueSeparator = keyValueSeparator;
        _keyValueRegex = new Regex(keyValueRegexPattern??throw new ArgumentNullException(nameof(keyValueRegexPattern)), RegexOptions.None, TimeSpan.FromMilliseconds(100));
    }
    
    public void Serialize(IDictionary<String, String> value, StreamWriter sw)
    {
        foreach (KeyValuePair<string, string> translation in value)
            sw.WriteLine(translation.Key + _keyValueSeparator + translation.Value);
    }

    public void Deserialize(IDictionary<String, String> value, StreamReader sr)
    {
        string line=sr.ReadLine();
        while(line != null){
            Match m = _keyValueRegex.Match(line);
            if (m.Success && !value.ContainsKey(m.Groups[1].Value))
                value.Add(m.Groups[1].Value, m.Groups[2].Value);
                    
            line=sr.ReadLine();
        }
    }
}