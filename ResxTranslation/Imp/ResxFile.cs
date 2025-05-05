using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Resources.NetStandard;

namespace ResxTranslation.Imp;

public class ResxFile:Dictionary<string, ResXDataNode>
{
    private readonly FileInfo _file;

    public ResxFile(FileInfo file)
    {
        _file = file??throw new ArgumentNullException(nameof(file));
        _file.Refresh();
        if(_file.Exists)
            Load();
    }
    
    public FileInfo File=>_file;
    
    public void Load()
    {
        Clear();
        using var rr = new ResXResourceReader(_file.FullName);
        rr.UseResXDataNodes = true;
        foreach (DictionaryEntry r in rr)
            Add(r.Key.ToString(), (ResXDataNode)r.Value);
    }

    public void Save()
    {
        using var rw = new ResXResourceWriter(_file.FullName);
        foreach (var kv in this)
            rw.AddResource(kv.Value);
    }
}
