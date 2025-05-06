using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace ResxTranslation.Imp;

public class ResxFile
{
    private XmlDocument _xmlDocument;
    private FileInfo _file;

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
    
    public IDictionary<XmlAttribute, XmlText> Entries=>GetEntries();

    private IDictionary<XmlAttribute, XmlText> GetEntries()
    {
        var dataNodes=_xmlDocument.SelectNodes("/root/data")?.Cast<XmlNode>();
        return dataNodes?.Where(v => v.Attributes?["type"] == null).ToDictionary(v=>v.Attributes["name"], v=>(XmlText)v.SelectSingleNode("/data/value/text()"));
    }

    private void AddEntry(string key, string value)
    {
        var root=_xmlDocument.SelectSingleNode("/root");
        XmlNode dataNode=root?.AppendChild(_xmlDocument.CreateElement("data"));
        dataNode.Attributes.Append(_xmlDocument.CreateAttribute("name")).Value=key;
        XmlNode valueNode=dataNode.AppendChild(_xmlDocument.CreateElement("value"));
        valueNode.AppendChild(_xmlDocument.CreateTextNode(value));
    }
    
    public XmlNode GetEntry(string key)
    {
        return _xmlDocument.SelectSingleNode("/root/data")?.Cast<XmlNode>()
            .FirstOrDefault(v=>v.Attributes?["name"]?.Value == key);
    }
    
    public void SetEntry(string key, string value)
    {
        var node=GetEntry(key);

        if (node != null)
        {
            XmlNode valueNode=(XmlText)node.SelectSingleNode("/data/value/text()");
            valueNode.Value=value;
            return;
        }
        
        AddEntry(key, value);
        
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