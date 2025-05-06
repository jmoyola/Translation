using System;
using TranslationService.Imp;

namespace TranslationService.Core;

public abstract class BaseBatchTranslation:IBatchTranslation
{
    public event TranslationEventHandler TranslationEvent;
    public ITranslationService TranslationService { get; set; } = new FakeTranslationService();
    public WillCard TranslateKeyFilter { get; set; } = "*";
    public WillCard ResourceFilter { get; set; } = "*";
    public string DefaultLanguage { get; set; } = "en";
    
    public abstract void Translate(String fromLanguage, String toLanguage);

    protected void OnTranslationEvent(TranslationEventArgs e)
    {
        TranslationEvent?.Invoke(this, e);
    }
}