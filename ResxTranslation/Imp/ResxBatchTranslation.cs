using System;
using System.Collections;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Resources.NetStandard;
using System.Text.RegularExpressions;
using TranslationService.Core;

namespace ResxTranslation.Imp;

public class ResxBatchTranslation:BaseBatchTranslation
{
    public DirectoryInfo BaseDirectory { get; set; }
    public bool UseDefaultForEnglish { get; set; }
    
    public string TranslatePattern { get; set; } = ".*";
    
    public override void Translate(string fromLanguage, string toLanguage)
    {
        if(BaseDirectory==null) throw new TranslationException("Base directory not set");
        if (!BaseDirectory.Exists) throw new TranslationException($"Base directory '{BaseDirectory.FullName}' does not exist");
        if(String.IsNullOrEmpty(TranslatePattern)) throw new TranslationException("TranslatePattern not set");
        
        Regex translateRegex = new Regex(TranslatePattern, RegexOptions.None, TimeSpan.FromMilliseconds(100));

        var fromLanguageResourceFiles = BaseDirectory.EnumerateFiles("*.resx", SearchOption.AllDirectories);
        Regex r=new Regex(@"(\." + fromLanguage + @")?\.resx$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));
        
        foreach (var fromLanguageResourceFile in fromLanguageResourceFiles)
        {
            Match m = r.Match(fromLanguageResourceFile.Name);
            if(!m.Success) continue;
            
            string toFileName=string.IsNullOrEmpty(m.Groups[2].Value)? fromLanguageResourceFile.Replace(".resx", $".{toLanguage}.resx")
            FileInfo toLanguageResourceFile=new FileInfo(fromLanguageResourceFile.Directory.FullName
                                                         + Path.DirectorySeparatorChar
                                                         + .r.Replace(fromLanguageResourceFile.Name, "\\." + toLanguage + "\\."));
            
            ResxFile fromLanguageResourceFileHandler = new ResxFile(fromLanguageResourceFile);
            ResxFile toLanguageResourceFileHandler = new ResxFile(toLanguageResourceFile);

            var fromLanguageResourceFileReaderFiltered = fromLanguageResourceFileHandler.Where(v =>
                translateRegex.IsMatch(v.Key)
                && v.Value.GetValueTypeName((ITypeResolutionService)null) == "System.String");
            foreach (var kv in fromLanguageResourceFileReaderFiltered)
            {
                string translation = TranslationService.Translate(kv.Value.GetValue((ITypeResolutionService)null).ToString(), fromLanguage, toLanguage).Result;
                
                if(toLanguageResourceFileHandler.ContainsKey(kv.Key))
                    toLanguageResourceFileHandler[kv.Key]=new ResXDataNode(kv.Key, translation);
                else
                    toLanguageResourceFileHandler.Add(kv.Key, new ResXDataNode(kv.Key, translation));
            }
            toLanguageResourceFileHandler.Save();
        }
        
    }
}
