using System;
using System.Runtime.Serialization;

namespace TranslationService.Core;

[Serializable]
public class TranslationException:Exception
{
    protected TranslationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public TranslationException(string message) : base(message)
    {
    }

    public TranslationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}