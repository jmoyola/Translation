using System.Resources;
using TranslationService.Core;

namespace ResxTranslation.Imp;

public class ResxBatchTranslation:BaseBatchTranslation
{
    public override void Translate(string fromLanguage, string toLanguage)
    {
        var rr = new ResourceReader("res");
        
    }
}
