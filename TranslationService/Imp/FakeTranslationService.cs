using System;
using System.Threading.Tasks;
using TranslationService.Core;

namespace TranslationService.Imp;

public class FakeTranslationService:ITranslationService
{
    public Task<String> Translate(String text, String fromLanguage, String toLanguage)
    {
        return Task.Run(()=>toLanguage + ":" + text);
    }
}