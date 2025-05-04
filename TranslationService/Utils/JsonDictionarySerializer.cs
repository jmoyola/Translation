using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using Newtonsoft.Json;

namespace TranslationService.Utils;

public class JsonDictionarySerializer:IDictionarySerializer<String, string>
{
    public void Serialize(IDictionary<String, String> value, StreamWriter sw)
    {
        var ser=JsonSerializer.Create();
        ser.Formatting=Formatting.Indented;
        ser.Serialize(sw, value);
    }

    public void Deserialize(IDictionary<String, String> value, StreamReader sr)
    {
        var ser=JsonSerializer.Create();
        IDictionary<string, string> ret=(Dictionary<string, string>)ser.Deserialize(sr, typeof(Dictionary<string, string>));
        ret?.ToList().ForEach(v=>value.Add(v.Key, v.Value));
    }
}