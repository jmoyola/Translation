using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources.NetStandard;
using System.Text.RegularExpressions;
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
            .Where(v=>resourceRegex.IsMatch(v.Name));
        
        foreach (var fromLanguageResourceFile in fromLanguageResourceFiles)
        {
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
            ResxFile toLanguageResourceFileHandler = new ResxFile(toLanguageResourceFile);

            foreach (var kv in fromLanguageResourceFileHandler)
            {
                ResXDataNode rdn = kv.Value;
                if (translateKeyFilter.IsMatch(kv.Key)
                    && kv.Value.GetValueTypeName((AssemblyName[])null).StartsWith("System.String,"))
                {
                    if (kv.Key.Contains(".") && !controlPropertiesToTranslateRegex.IsMatch(kv.Key))
                        continue;

                    
                    string text = kv.Value.GetValue((AssemblyName[])null).ToString();
                    string translation = TranslationService
                        .Translate(text, fromLanguage, toLanguage).Result;

                    OnTranslationEvent(new TranslationEventArgs(fromLanguage, toLanguage, kv.Key, text, translation, fromLanguageResourceFile.FullName));
                    rdn= new ResXDataNode(kv.Key, translation);
                }
                
                toLanguageResourceFileHandler[kv.Key] = rdn;
            }
            toLanguageResourceFileHandler.Save();
        }
        
    }
}
