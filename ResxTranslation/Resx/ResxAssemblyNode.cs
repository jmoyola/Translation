using System;
using System.Xml;

namespace ResxTranslation.Resx;

public class ResxAssemblyNode
{
    private readonly XmlNode _node;

    public ResxAssemblyNode(XmlNode node)
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
    public string Alias
    {
        get
        {
            return _node.Attributes?["alias"]?.Value;
        }
        set
        {
            if (_node.Attributes?["alias"] == null)
                _node.Attributes?.Append(_node.OwnerDocument.CreateAttribute("alias"));

            _node.Attributes["alias"].Value=value;
        }
    }
}