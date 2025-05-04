using System;
using System.Threading.Tasks;

namespace TranslationService.Core;

public interface ITranslationService
{
    Task<String> Translate(String text, String fromLanguage, String toLanguage);
}