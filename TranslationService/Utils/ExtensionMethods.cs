using System;
using System.IO;
using System.Linq;

namespace TranslationService.Utils;

public static class ExtensionMethods
{
    public static FileInfo Backup(this FileInfo file, string backupSufixPattern="_yyyyMMddHHmmssfff")
    {
        string baseName=Path.GetFileNameWithoutExtension(file.Name);
        string extension=Path.GetExtension(file.Name);
        string backupPattern=DateTime.Now.ToString(backupSufixPattern);
        return file.CopyTo(file.Directory?.FullName + Path.DirectorySeparatorChar + baseName + backupPattern + extension);
    }
    
    public static string FileNamePartLanguage(this FileInfo resxFile, string defaultLanguage)
    {
        string[] resxNameParts = resxFile.Name.Split('.');
        if (resxNameParts.Length <3)
            return defaultLanguage;
        
        return resxNameParts[resxNameParts.Length - 2];
    }

    public static string FileNamePartBase(this FileInfo resxFile)
    {
        string[] resxNameParts = resxFile.Name.Split('.');
        return String.Join(".", resxNameParts.Take(resxNameParts.Length<4?1:resxNameParts.Length - 2));
    }
}