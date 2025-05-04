using System;
using System.Runtime.Serialization;

namespace TranslationService.Core;

[Serializable]
public class TranslationServiceException:Exception
{
    protected TranslationServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public TranslationServiceException(string message) : base(message)
    {
    }

    public TranslationServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}