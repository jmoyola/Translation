using System.IO;
using TranslationService.Utils;

namespace TranslationService.Core;

public class FileDictionaryCache<TK, TV>:FileDictionary<TK, TV>
{
    public FileDictionaryCache(FileInfo file) : base(file)
    {
        
    }
}