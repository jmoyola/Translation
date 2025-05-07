using System;

namespace TranslationService.Core;

public struct Advance
{
    public int Index { get; set; }
    public int Total { get; set; }
}
public struct TranslationInfo
{
    public string Resource { get; set; }
    public string Language { get; set; }
    public string ResourceItemName { get; set; }
    public string ResourceItemValue { get; set; }
    public Advance ResourceAdvance { get; set; }
    public Advance ResourceItemAdvance { get; set; }
} 
public class TranslationEventArgs : EventArgs
{
    private readonly TranslationInfo _fromTranslationInfo;
    private readonly TranslationInfo _toTranslationInfo;

    public TranslationEventArgs(TranslationInfo fromTranslationInfo, TranslationInfo toTranslationInfo)
    {
        _fromTranslationInfo = fromTranslationInfo;
        _toTranslationInfo=toTranslationInfo;
    }
    public TranslationInfo FromTranslationInfo=>_fromTranslationInfo;
    public TranslationInfo ToTranslationInfo=>_toTranslationInfo;

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