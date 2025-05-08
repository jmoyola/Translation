using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ResxTranslation.Resx;
using TranslationService.Core;
using TranslationService.Utils;

namespace ResxTranslation.Imp;

public class ResxBatchTranslation:BaseBatchTranslation
{
    public DirectoryInfo BaseDirectory { get; set; }
    public ISet<string> ControlPropertiesToTranslate { get; set; } = new HashSet<string>("Text|Label|Caption|Comment".Split('|'));
    
    public override void Translate(CultureInfo fromLanguage, IEnumerable<CultureInfo> toLanguages)
    {
        if(BaseDirectory==null) throw new TranslationException("Base directory not set");
        if (!BaseDirectory.Exists) throw new TranslationException($"Base directory '{BaseDirectory.FullName}' does not exist");
        if(TranslateKeyFilter==null) throw new TranslationException("TranslateKeyFilter not set");
        if(ResourceFilter==null) throw new TranslationException("ResourceFilter not set");
        
        var fromLanguageResourceFiles = FindResxFiles(fromLanguage, BaseDirectory, ResourceFilter).ToList();
        for (int fromLanguageResourceFileIndex=0;fromLanguageResourceFileIndex<fromLanguageResourceFiles.Count; fromLanguageResourceFileIndex++)
        {
            var fromLanguageResourceFile = fromLanguageResourceFiles[fromLanguageResourceFileIndex];
            string fromLanguageBaseName = fromLanguageResourceFile.FileNamePartBase();

            foreach (var toLanguage in toLanguages)
            {
                FileInfo toLanguageResourceFile = new FileInfo(fromLanguageResourceFile.Directory?.FullName
                                                               + Path.DirectorySeparatorChar
                                                               + fromLanguageBaseName + "." + toLanguage + ".resx");

                ResxFile fromLanguageResourceFileHandler = new ResxFile(fromLanguageResourceFile);
                fromLanguageResourceFileHandler.Load();

                if (toLanguageResourceFile.Exists)
                    toLanguageResourceFile.Backup();
                
                ResxFile toLanguageResourceFileHandler = toLanguageResourceFile.Exists
                    ? new ResxFile(toLanguageResourceFile).Load()
                    : fromLanguageResourceFileHandler.Clone(toLanguageResourceFile);

                var textEntries = FilterResxTextEntries(fromLanguageResourceFileHandler.Entries);

                for (int iTextEntry = 0; iTextEntry < textEntries.Count; iTextEntry++)
                {
                    var entry = textEntries[iTextEntry];
                    
                    string text = entry.Value;
                    string translation = TranslationService
                        .Translate(text, fromLanguage.TwoLetterISOLanguageName, toLanguage.TwoLetterISOLanguageName)
                        .Result;

                    toLanguageResourceFileHandler.SetEntry(entry.Name, translation);
                    OnTranslationEvent(new TranslationEventArgs(
                            new TranslationInfo()
                            {
                                Resource = fromLanguageResourceFile.FullName, Language = fromLanguage,
                                ResourceItemName = entry.Name, ResourceItemValue = text,
                            },
                            new TranslationInfo()
                            {
                                Resource = toLanguageResourceFile.FullName, Language = toLanguage,
                                ResourceItemName = entry.Name, ResourceItemValue = translation,
                            },
                            new Advance() { Index = fromLanguageResourceFileIndex, Total = fromLanguageResourceFiles.Count },
                            new Advance() { Index = iTextEntry, Total = textEntries.Count }
                        )
                    );
                }

                toLanguageResourceFileHandler.Save();
            }
        }
    }

    private List<ResxDataNode> FilterResxTextEntries(IEnumerable<ResxDataNode> entries)
    {
        Regex controlPropertiesToTranslateRegex = new Regex("\\.(" +string.Join("|", ControlPropertiesToTranslate) + ")$");
            
        Regex translateKeyFilter = new Regex(TranslateKeyFilter.ToRegex(), RegexOptions.None, TimeSpan.FromMilliseconds(100));
        
        var textEntries = entries.Where(v => string.IsNullOrEmpty(v.Type)
                                             && v.Value != null).ToList();

        textEntries = textEntries.Where(v=>translateKeyFilter.IsMatch(v.Name)).ToList();
        textEntries=textEntries.Where(v=>!v.Name.Contains(".") || (v.Name.Contains(".")
                                                                   && !controlPropertiesToTranslateRegex.IsMatch(v.Name))).ToList();
        return textEntries;        
    }
    private IEnumerable<FileInfo> FindResxFiles(CultureInfo language, DirectoryInfo baseDirectory, WillCard resourceFilter)
    {
        Regex resourceRegex = new Regex(resourceFilter.ToRegex(), RegexOptions.None, TimeSpan.FromMilliseconds(100));
        
        return baseDirectory.EnumerateFiles("*.resx", SearchOption.AllDirectories)
            .Where(v=>resourceRegex.IsMatch(v.Name) && v.FileNamePartLanguage(DefaultLanguage.TwoLetterISOLanguageName).Equals(language.TwoLetterISOLanguageName));
    }
}
