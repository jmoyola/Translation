using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TranslationService.Imp;

namespace TranslationService.Core;

public abstract class BaseBatchTranslation:IBatchTranslation
{
    public event TranslationEventHandler TranslationEvent;
    public ITranslationService TranslationService { get; set; } = new FakeTranslationService();
    public WillCard TranslateKeyFilter { get; set; } = "*";
    public WillCard ResourceFilter { get; set; } = "*";
    public CultureInfo DefaultLanguage { get; set; } = CultureInfo.GetCultureInfo("en_GB");

    public void Translate(CultureInfo fromLanguage, CultureInfo toLanguage)
    {
        Translate(fromLanguage, new List<CultureInfo>(){toLanguage});
    }
    
    public abstract void Translate(CultureInfo fromLanguage, IEnumerable<CultureInfo> toLanguages);

    protected void OnTranslationEvent(TranslationEventArgs e)
    {
        TranslationEvent?.Invoke(this, e);
    }
}