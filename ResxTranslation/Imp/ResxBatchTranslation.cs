using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ResxTranslation.Resx;
using TranslationService.Core;

namespace ResxTranslation.Imp;

public class ResxBatchTranslation:BaseBatchTranslation
{
    public DirectoryInfo BaseDirectory { get; set; }
    public ISet<string> ControlPropertiesToTranslate { get; set; } = new HashSet<string>("Name|Text|Label|Caption|Comment".Split('|'));
    
    public override void Translate(string fromLanguage, string toLanguage)
    {
        if(BaseDirectory==null) throw new TranslationException("Base directory not set");
        if (!BaseDirectory.Exists) throw new TranslationException($"Base directory '{BaseDirectory.FullName}' does not exist");
        if(TranslateKeyFilter==null) throw new TranslationException("TranslateKeyFilter not set");
        if(ResourceFilter==null) throw new TranslationException("ResourceFilter not set");

        Regex controlPropertiesToTranslateRegex = new Regex("\\.(" +string.Join("|", ControlPropertiesToTranslate) + ")$");
            
        Regex translateKeyFilter = new Regex(TranslateKeyFilter.ToRegex(), RegexOptions.None, TimeSpan.FromMilliseconds(100));
        Regex resourceRegex = new Regex(ResourceFilter.ToRegex(), RegexOptions.None, TimeSpan.FromMilliseconds(100));

        var fromLanguageResourceFiles = BaseDirectory.EnumerateFiles("*.resx", SearchOption.AllDirectories)
            .Where(v=>resourceRegex.IsMatch(v.Name)).ToList();
        
        for (int iFromLanguageResourceFile=0; iFromLanguageResourceFile<fromLanguageResourceFiles.Count; iFromLanguageResourceFile++)
        {
            var fromLanguageResourceFile = fromLanguageResourceFiles[iFromLanguageResourceFile];
                
            string fromLanguageBaseName;
            string[] resxNameParts=fromLanguageResourceFile.Name.Split('.');
            if(resxNameParts.Length==2 && fromLanguage.Equals(DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                fromLanguageBaseName=resxNameParts[0];
            else if(resxNameParts.Length>2 && resxNameParts[resxNameParts.Length-2].Equals(fromLanguage, StringComparison.OrdinalIgnoreCase))
                fromLanguageBaseName=String.Join(".", resxNameParts.Take(resxNameParts.Length-2));
            else
                continue;
            
            FileInfo toLanguageResourceFile=new FileInfo(fromLanguageResourceFile.Directory?.FullName
                                                         + Path.DirectorySeparatorChar
                                                         + fromLanguageBaseName + "." + toLanguage + ".resx");
            
            ResxFile fromLanguageResourceFileHandler = new ResxFile(fromLanguageResourceFile);
            fromLanguageResourceFileHandler.Load();
            
            
            ResxFile toLanguageResourceFileHandler = fromLanguageResourceFile.Exists? new ResxFile(toLanguageResourceFile).Load():fromLanguageResourceFileHandler.Clone(toLanguageResourceFile);

            var textEntries = fromLanguageResourceFileHandler.Entries.Where(v => string.IsNullOrEmpty(v.Type)
                && v.Value!=null).ToList();
            for (int iTextEntry=0; iTextEntry<textEntries.Count; iTextEntry++)
            {
                var entry = textEntries[iTextEntry];
                if (translateKeyFilter.IsMatch(entry.Name))
                {
                    if (entry.Name.Contains(".") && !controlPropertiesToTranslateRegex.IsMatch(entry.Name))
                        continue;

                    
                    string text = entry.Value;
                    string translation = TranslationService
                        .Translate(text, fromLanguage, toLanguage).Result;

                    toLanguageResourceFileHandler.SetEntry(entry.Name, translation);
                    OnTranslationEvent(new TranslationEventArgs(
                        new TranslationInfo(){Resource = fromLanguageResourceFile.FullName, Language = fromLanguage, ResourceItemName = entry.Name, ResourceItemValue = text, ResourceAdvance = new Advance(){Index = iFromLanguageResourceFile, Total = fromLanguageResourceFiles.Count}, ResourceItemAdvance = new Advance(){Index = iTextEntry, Total = textEntries.Count}},
                        new TranslationInfo(){Resource = toLanguageResourceFile.FullName, Language  = toLanguage, ResourceItemName  = entry.Name, ResourceItemValue = translation, ResourceAdvance = new Advance(){Index = iFromLanguageResourceFile, Total = fromLanguageResourceFiles.Count}, ResourceItemAdvance = new Advance(){Index = iTextEntry, Total = textEntries.Count}}
                        ));
                }
            }
            toLanguageResourceFileHandler.Save();
        }
        
    }
}
