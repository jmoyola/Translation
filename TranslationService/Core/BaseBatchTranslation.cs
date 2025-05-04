using System;
using System.IO;
using TranslationService.Imp;

namespace TranslationService.Core;

public abstract class BaseBatchTranslation
{
    public ITranslationService TranslationService { get; set; } = new FakeTranslationService();
    public abstract void Translate(String fromLanguage, String toLanguage);
}