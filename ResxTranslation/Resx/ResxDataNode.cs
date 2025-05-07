using System;
using System.Xml;

namespace ResxTranslation.Resx;

public class ResxDataNode
{
    private readonly XmlNode _node;

    public ResxDataNode(XmlNode node)
    {
        _node = node??throw new ArgumentNullException(nameof(node));
    }

    public string Name
    {
        get
        {
            return _node.Attributes?["name"]?.Value;
        }
        set
        {
            if (_node.Attributes?["name"] == null)
                _node.Attributes?.Append(_node.OwnerDocument.CreateAttribute("name"));
            _node.Attributes["name"].Value=value;
        }
    }
    public string Type
    {
        get
        {
            return _node.Attributes?["type"]?.Value;
        }
        set
        {
            if (_node.Attributes?["type"] == null)
                _node.Attributes?.Append(_node.OwnerDocument.CreateAttribute("type"));

            _node.Attributes["type"].Value=value;
        }
    }
    public string MimeType
    {
        get
        {
            return _node.Attributes?["mimetype"]?.Value;
        }
        set
        {
            if (_node.Attributes?["mimetype"] == null)
                _node.Attributes?.Append(_node.OwnerDocument.CreateAttribute("mimetype"));
            
            _node.Attributes["mimetype"].Value=value;
        }
    }
    
    public string Value
    {
        get
        {
            return _node["value"]?.InnerText;
        }
        set
        {
            if (_node["value"] == null)
                _node.AppendChild(_node.OwnerDocument.CreateElement("value"));
            
            _node["value"].InnerText=value;
        }
    }
    public string Comment
    {
        get
        {
            return _node["comment"]?.InnerText;
        }
        set
        {
            if (_node["comment"] == null)
                _node.AppendChild(_node.OwnerDocument.CreateElement("comment"));
            
            _node["comment"].InnerText=value;
        }
    }
}