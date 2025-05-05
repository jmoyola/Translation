using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Resources.NetStandard;
using System.Text.RegularExpressions;
using TranslationService.Core;

namespace ResxTranslation.Imp;

public class ResxBatchTranslation:BaseBatchTranslation
{
    public DirectoryInfo BaseDirectory { get; set; }
    public string DefaultLanguage { get; set; } = "en";
    
    public string TranslatePattern { get; set; } = ".*";
    
    public override void Translate(string fromLanguage, string toLanguage)
    {
        if(BaseDirectory==null) throw new TranslationException("Base directory not set");
        if (!BaseDirectory.Exists) throw new TranslationException($"Base directory '{BaseDirectory.FullName}' does not exist");
        if(String.IsNullOrEmpty(TranslatePattern)) throw new TranslationException("TranslatePattern not set");
        
        Regex translateRegex = new Regex(TranslatePattern, RegexOptions.None, TimeSpan.FromMilliseconds(100));

        var fromLanguageResourceFiles = BaseDirectory.EnumerateFiles("*.resx", SearchOption.AllDirectories);
        //Regex r=new Regex(@"(\." + fromLanguage + @")?\.resx$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));
        
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

            var fromLanguageResourceFileReaderFiltered = fromLanguageResourceFileHandler.Where(v =>
                translateRegex.IsMatch(v.Key)
                && v.Value.GetValueTypeName((AssemblyName[])null).StartsWith("System.String,"));

            foreach (var kv in fromLanguageResourceFileReaderFiltered)
            {
                string translation = TranslationService.Translate(kv.Value.GetValue((AssemblyName[])null).ToString(), fromLanguage, toLanguage).Result;
                
                if(toLanguageResourceFileHandler.ContainsKey(kv.Key))
                    toLanguageResourceFileHandler[kv.Key]=new ResXDataNode(kv.Key, translation);
                else
                    toLanguageResourceFileHandler.Add(kv.Key, new ResXDataNode(kv.Key, translation));
            }
            toLanguageResourceFileHandler.Save();
        }
        
    }
}
