using System;
using System.Collections.Generic;
using System.Globalization;
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
    
    public override void Translate(CultureInfo fromLanguage, IEnumerable<CultureInfo> toLanguages)
    {
        if(BaseDirectory==null) throw new TranslationException("Base directory not set");
        if(String.IsNullOrEmpty(FromLanguageFilePattern)) throw new TranslationException("FromLanguageFilePattern not set");
        if(String.IsNullOrEmpty(ToLanguageFilePattern)) throw new TranslationException("ToLanguageFilePattern not set");
        if(TranslateKeyFilter==null) throw new TranslationException("TranslateKeyFilter not set");
        if(ResourceFilter==null) throw new TranslationException("ResourceFilter not set");
        if(Serializer==null) throw new TranslationException("Serializer not set");
        
        Regex translateRegex = new Regex(TranslateKeyFilter.ToRegex(), RegexOptions.None, TimeSpan.FromMilliseconds(100));
        Regex resourceRegex = new Regex(ResourceFilter.ToRegex(), RegexOptions.None, TimeSpan.FromMilliseconds(100));

        foreach (var toLanguage in toLanguages)
        {
            PlaceAndHolder ph = new PlaceAndHolder()
            {
                { "{baseDirectory}", BaseDirectory.FullName },
                { "{fromLanguage}", fromLanguage.TwoLetterISOLanguageName },
                { "{toLanguage}", toLanguage.TwoLetterISOLanguageName },
                { "{fileName}", "" },
            };

            string fromLanguageFilePattern = ph.Replace(FromLanguageFilePattern);

            FileInfo fromDir = new FileInfo(fromLanguageFilePattern);

            var fromLanguageFiles =
                fromDir.Directory.EnumerateFiles(fromDir.Name, SearchOption.TopDirectoryOnly).ToList();
            for (int iFromLanguageFileIndex = 0;
                 iFromLanguageFileIndex < fromLanguageFiles.Count;
                 iFromLanguageFileIndex++)
            {
                FileInfo fromLanguageFile = fromLanguageFiles[iFromLanguageFileIndex];
                if (!resourceRegex.IsMatch(fromLanguageFile.Name)) continue;

                if (!fromLanguageFile.Exists)
                    throw new TranslationException($"FromLanguageFile '{fromLanguageFile.FullName}' not exists");
                IFileDictionary<string, string> fromLanguagePropertiesFile =
                    new FileDictionary<string, string>(fromLanguageFile);
                fromLanguagePropertiesFile.Load(Serializer);

                ph["{fileName}"] = fromLanguageFile.Name;

                string toLanguageFilePattern = ph.Replace(ToLanguageFilePattern);

                FileInfo toLanguageFile = new FileInfo(toLanguageFilePattern);
                if (!toLanguageFile.Directory.Exists)
                    Directory.CreateDirectory(toLanguageFile.Directory.FullName);

                IFileDictionary<string, string> toLanguagePropertiesFile =
                    new FileDictionary<string, string>(toLanguageFile);

                var entries = fromLanguagePropertiesFile.Where(v => translateRegex.IsMatch(v.Key)).ToList();
                for (int iEntryIndex = 0; iEntryIndex < entries.Count; iEntryIndex++)
                {
                    var entry = entries[iEntryIndex];
                    string translation = TranslationService.Translate(entry.Value, fromLanguage.TwoLetterISOLanguageName, toLanguage.TwoLetterISOLanguageName).Result;
                    OnTranslationEvent(new TranslationEventArgs(
                            new TranslationInfo()
                            {
                                Resource = fromLanguageFile.FullName, Language = fromLanguage, ResourceItemName = entry.Key,
                                ResourceItemValue = entry.Value,
                            },
                            new TranslationInfo()
                            {
                                Resource = toLanguageFile.FullName, Language = toLanguage, ResourceItemName = entry.Key,
                                ResourceItemValue = translation,
                            },
                            new Advance() { Index = iFromLanguageFileIndex, Total = fromLanguageFiles.Count },
                            new Advance() { Index = iEntryIndex, Total = entries.Count }
                        )
                    );

                    toLanguagePropertiesFile.Add(entry.Key, translation);
                }

                if (toLanguagePropertiesFile.File.Exists)
                    toLanguagePropertiesFile.File.Backup();
                toLanguagePropertiesFile.Save(Serializer);
            }
        }
    }
}