using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TranslationService.Core;
using TranslationService.Utils;

namespace TranslationProperties.Imp;

public class PropertiesBatchTranslation:BaseBatchTranslation
{
    public IDictionarySerializer<string, string> Serializer { get; set; }
    public DirectoryInfo BaseDirectory { get; set; }
    public string FromLanguageFilePattern { get; set; }
    public string ToLanguageFilePattern { get; set; }
    
    public override void Translate(string fromLanguage, string toLanguage)
    {
        if(BaseDirectory==null) throw new TranslationException("Base directory not set");
        if(String.IsNullOrEmpty(FromLanguageFilePattern)) throw new TranslationException("FromLanguageFilePattern not set");
        if(String.IsNullOrEmpty(ToLanguageFilePattern)) throw new TranslationException("ToLanguageFilePattern not set");
        if(TranslateKeyFilter==null) throw new TranslationException("TranslateKeyFilter not set");
        if(ResourceFilter==null) throw new TranslationException("ResourceFilter not set");
        if(Serializer==null) throw new TranslationException("Serializer not set");
        
        Regex translateRegex = new Regex(TranslateKeyFilter.ToRegex(), RegexOptions.None, TimeSpan.FromMilliseconds(100));
        Regex resourceRegex = new Regex(ResourceFilter.ToRegex(), RegexOptions.None, TimeSpan.FromMilliseconds(100));
        
        PlaceAndHolder ph=new PlaceAndHolder()
        {
            {"{baseDirectory}", BaseDirectory.FullName},
            {"{fromLanguage}", fromLanguage},
            {"{toLanguage}", toLanguage},
            {"{fileName}", ""},
        };
        
        string fromLanguageFilePattern=ph.Replace(FromLanguageFilePattern);

        FileInfo fromDir = new FileInfo(fromLanguageFilePattern);
        
        foreach (FileInfo fromLanguageFile in fromDir.Directory.EnumerateFiles(fromDir.Name, SearchOption.TopDirectoryOnly))
        {
            if(!resourceRegex.IsMatch(fromLanguageFile.Name)) continue;
            
            if (!fromLanguageFile.Exists)
                throw new TranslationException($"FromLanguageFile '{fromLanguageFile.FullName}' not exists");
            IFileDictionary<string, string> fromLanguagePropertiesFile =
                new FileDictionary<string, string>(fromLanguageFile);
            fromLanguagePropertiesFile.Load(Serializer);

            ph["{fileName}"] = fromLanguageFile.Name;
            
            string toLanguageFilePattern=ph.Replace(ToLanguageFilePattern);
            
            FileInfo toLanguageFile = new FileInfo(toLanguageFilePattern);
            if (!toLanguageFile.Directory.Exists)
                Directory.CreateDirectory(toLanguageFile.Directory.FullName);

            IFileDictionary<string, string> toLanguagePropertiesFile =
                new FileDictionary<string, string>(toLanguageFile);

            foreach (var kv in fromLanguagePropertiesFile.Where(v=>translateRegex.IsMatch(v.Key)))
            {
                string translation = TranslationService.Translate(kv.Value, fromLanguage, toLanguage).Result;
                OnTranslationEvent(new TranslationEventArgs(fromLanguage, toLanguage, kv.Key, kv.Value, translation, fromLanguageFile.FullName));
                
                toLanguagePropertiesFile.Add(kv.Key, translation);
            }

            toLanguagePropertiesFile.Save(Serializer);
        }
    }
}