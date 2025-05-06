using System;

namespace TranslationService.Core;

public class TranslationEventArgs : EventArgs
{
    private readonly string _fromLanguage;
    private readonly string _toLanguage;
    private readonly string _resourceEntryId;
    private readonly string _fromLanguageText;
    private readonly string _toLanguageText;
    private readonly string _resource;

    public TranslationEventArgs(string fromLanguage, string toLanguage, string resourceEntryId, string fromLanguageText, string toLanguageText,
        string resource)
    {
        _fromLanguage = fromLanguage;
        _toLanguage = toLanguage;

        _resourceEntryId = resourceEntryId;
        _fromLanguageText = fromLanguageText;
        _toLanguageText = toLanguageText;
        _resource = resource;
    }
    public string FromLanguage => _fromLanguage;

    public string ToLanguage => _toLanguage;

    public string ResourceEntryId => _resourceEntryId;
    public string FromLanguageText=> _fromLanguageText;

    public string ToLanguageText => _toLanguageText;

    public string Resource => _resource;
}

public delegate void TranslationEventHandler(object sender, TranslationEventArgs args);
public interface IBatchTranslation
{
    event TranslationEventHandler TranslationEvent;
    ITranslationService TranslationService { get; set; }
    WillCard TranslateKeyFilter { get; set; }
    WillCard ResourceFilter { get; set; }
    string DefaultLanguage { get; set; }
    
    void Translate(String fromLanguage, String toLanguage);
}