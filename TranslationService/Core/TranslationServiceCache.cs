using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TranslationService.Utils;

namespace TranslationService.Core;

public class TranslationServiceCache:ITranslationService
{
    private readonly DirectoryInfo _cacheDirectory;
    private readonly IDictionary<string, IFileDictionary<string, string>> _translationFilesCached=new Dictionary<string, IFileDictionary<string, string>>();
    
    private readonly ITranslationService _translationService;
        
    public TranslationServiceCache(ITranslationService translationService, DirectoryInfo cacheDirectory)
    {
        _translationService = translationService??throw new ArgumentNullException(nameof(translationService));
        _cacheDirectory = cacheDirectory??throw new ArgumentNullException(nameof(cacheDirectory));
        
        if(!_cacheDirectory.Exists) Directory.CreateDirectory(_cacheDirectory.FullName);
    }
    
    public async Task<string> Translate(string text, string fromLanguage, string toLanguage)
    {
        string key = fromLanguage + "_" + toLanguage;
        
        if(!_cacheDirectory.Exists) Directory.CreateDirectory(_cacheDirectory.FullName);
        
        if(!_translationFilesCached.ContainsKey(key))
            _translationFilesCached.Add(key,
                new FileDictionary<string, string>(new FileInfo(_cacheDirectory.FullName + Path.DirectorySeparatorChar + key)));
        
        IFileDictionary<string, string> dictionary = _translationFilesCached[key];
        if(!dictionary.ContainsKey(text))
            dictionary.Add(text, _translationService.Translate(text, fromLanguage, toLanguage).Result);    
        
        return dictionary[text];
    }
}