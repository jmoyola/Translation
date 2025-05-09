using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace ResxTranslation.Resx;

public class ResxFile
{
    private XmlDocument _xmlDocument;
    private readonly FileInfo _file;

    public ResxFile(FileInfo file)
    {
        _file = file??throw new ArgumentNullException(nameof(file));
        _xmlDocument=new XmlDocument();
        _xmlDocument.AppendChild(_xmlDocument.CreateNode(XmlNodeType.Element, "root", ""));
    }

    public FileInfo File=>_file;
    
    public ResxFile Load()
    {
        _xmlDocument = new XmlDocument();
        _xmlDocument.Load(_file.FullName);
        return this;
    }
    
    public ResxFile Save(FileInfo toFile)
    {
        _xmlDocument.Save(toFile.FullName);
        return this;
    }

    public ResxFile Save()
    {
        return Save(_file);
    }

    public ResxFile Clone(FileInfo newFile)
    {
        ResxFile ret= new ResxFile(newFile);
        ret._xmlDocument=(XmlDocument)_xmlDocument.CloneNode(true);
        return ret;
    }
    
    public IList<ResxDataNode> Entries=>GetEntries();
    public IList<ResxAssemblyNode> Assemblies=>GetAssemblies();

    private IList<ResxDataNode> GetEntries()
    {
        return _xmlDocument.SelectNodes("/root/data")?.Cast<XmlNode>().Select(v=>new ResxDataNode(v)).ToList();
    }

    private IList<ResxAssemblyNode> GetAssemblies()
    {
        return _xmlDocument.SelectNodes("/root/assembly")?.Cast<XmlNode>().Select(v=>new ResxAssemblyNode(v)).ToList();
    }
    
    private ResxDataNode AddEntry(string name, string value)
    {
        var root=_xmlDocument.SelectSingleNode("/root");
        var ret=new ResxDataNode(root?.AppendChild(_xmlDocument.CreateElement("data")));
        
        ret.Name=name;
        ret.Value = value;

        return ret;
    }
    
    private ResxAssemblyNode AddAssemblyEntry(string name, string alias)
    {
        var root=_xmlDocument.SelectSingleNode("/root");
        var ret=new ResxAssemblyNode(root?.AppendChild(_xmlDocument.CreateElement("assembly")));
        
        ret.Name=name;
        ret.Alias = alias;

        return ret;
    }
    
    public ResxDataNode GetEntry(string name)
    {
        return Entries.FirstOrDefault(v=>v.Name==name);
    }
    
    public ResxDataNode SetEntry(string name, string value)
    {
        var node=GetEntry(name);

        if (node == null)
            node = AddEntry(name, value);

        node.Value = value;
        return node;
    }
    
    public bool RemoveEntry(string key)
    {
        var node=_xmlDocument.SelectSingleNode("/root/data")?.Cast<XmlNode>()
            .FirstOrDefault(v=>v.Attributes?["name"]?.Value == key);

        if (node != null)
        {
            node.ParentNode?.RemoveChild(node);
            return true;
        }

        return false;
    }
}