using System;
using System.Collections;
using System.ComponentModel.Design;
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
    public string FromLanguageFilePattern { get; set; }
    public string ToLanguageFilePattern { get; set; }
    public string TranslatePattern { get; set; } = ".*";
    
    public override void Translate(string fromLanguage, string toLanguage)
    {
        if(BaseDirectory==null) throw new TranslationException("Base directory not set");
        if (!BaseDirectory.Exists) throw new TranslationException($"Base directory '{BaseDirectory.FullName}' does not exist");
        if(String.IsNullOrEmpty(FromLanguageFilePattern)) throw new TranslationException("FromLanguageFilePattern not set");
        if(String.IsNullOrEmpty(ToLanguageFilePattern)) throw new TranslationException("ToLanguageFilePattern not set");
        if(String.IsNullOrEmpty(TranslatePattern)) throw new TranslationException("TranslatePattern not set");
        
        Regex translateRegex = new Regex(TranslatePattern, RegexOptions.None, TimeSpan.FromMilliseconds(100));
        
        PlaceAndHolder ph=new PlaceAndHolder()
        {
            {"{baseDirectory}", BaseDirectory.FullName},
            {"{fromLanguage}", fromLanguage},
            {"{toLanguage}", toLanguage},
            {"{fileName}", ""},
        };
        
        string fromLanguageFilePattern=ph.Replace(FromLanguageFilePattern);

        var fromLanguageResourceFiles = BaseDirectory.EnumerateFiles(FromLanguageFilePattern, SearchOption.AllDirectories);
        
        foreach (var fromLanguageResourceFile in fromLanguageResourceFiles)
        {
            ph["{fileName}"] = fromLanguageResourceFile.Name;
            string toLanguageFilePattern=ph.Replace(ToLanguageFilePattern);
            
            ResxFile fromLanguageResourceFileReader = new ResxFile(fromLanguageResourceFile);
            ResxFile toLanguageResourceFileWriter = new ResxFile(new FileInfo(toLanguageFilePattern));
            
            
            foreach (var kv in fromLanguageResourceFileReader.Where(v=>translateRegex.IsMatch(v.Key)
                     && v.Value.GetValueTypeName((ITypeResolutionService)null)=="System.String"))
            {
                string translation = TranslationService.Translate(kv.Value.GetValue((ITypeResolutionService)null).ToString(), fromLanguage, toLanguage).Result;
                
                if(toLanguageResourceFileWriter.ContainsKey(kv.Key))
                    toLanguageResourceFileWriter[kv.Key]=new ResXDataNode(kv.Key, translation);
                else
                    toLanguageResourceFileWriter.Add(kv.Key, new ResXDataNode(kv.Key, translation));
            }
            toLanguageResourceFileWriter.Save();
        }
        
    }
}
