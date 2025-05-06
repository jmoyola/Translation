using System;

namespace TranslationService.Core;

public class WillCard
{
    private readonly string _value;

    public WillCard(string value)
    {
        _value = value??throw new ArgumentNullException(nameof(value));
    }
    
    public string Value => _value;

    public string ToRegex()
    {
        return _value.Replace("*", ".*").Replace("?", ".");
    }
    public string ToSql()
    {
        return _value.Replace("*", "%").Replace("?", "_");
    }

    public override string ToString()
    {
        return _value;
    }

    public override bool Equals(object obj)
    {
        return obj is WillCard  v &&  _value.Equals(v._value);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static implicit operator string(WillCard v)
    {
        return v.ToString();
    }
    
    public static implicit operator WillCard (string v)
    {
        return new WillCard(v);
    }
}