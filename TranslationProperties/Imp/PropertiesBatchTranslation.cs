using System;
using System.IO;
using System.Linq;
using TranslationService.Core;
using TranslationService.Utils;

namespace TranslationsProperties.Imp;

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
        if(Serializer==null) throw new TranslationException("Serializer not set");
        
        PlaceAndHolder ph=new PlaceAndHolder()
        {
            {"{baseDirectory}", BaseDirectory.FullName},
            {"{fromLanguage}", fromLanguage},
            {"{toLanguage}", toLanguage},
            {"{fileName}", ""},
        };
        
        FromLanguageFilePattern=ph.Replace(FromLanguageFilePattern);

        FileInfo fromDir = new FileInfo(FromLanguageFilePattern);
        
        foreach (FileInfo fromLanguageFile in fromDir.Directory.GetFiles(fromDir.Name, SearchOption.TopDirectoryOnly))
        {
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

            foreach (var kv in fromLanguagePropertiesFile)
            {
                string translation = TranslationService.Translate(kv.Value, fromLanguage, toLanguage).Result;
                toLanguagePropertiesFile.Add(kv.Key, translation);
            }

            toLanguagePropertiesFile.Save(Serializer);
        }
    }
}