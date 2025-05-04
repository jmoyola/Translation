using System;
using System.Collections.Generic;
using System.IO;

namespace TranslationService.Utils;

public interface IDictionarySerializer<TK, TV>
{
    void Serialize(IDictionary<TK, TV> value, StreamWriter sw);
    void Deserialize(IDictionary<TK, TV> value, StreamReader sr);
}