using System;
using System.Collections.Generic;
using System.Globalization;

namespace TranslationService.Core;

public struct Advance
{
    public int Index { get; set; }
    public int Total { get; set; }
}
public struct TranslationInfo
{
    public string Resource { get; set; }
    public CultureInfo Language { get; set; }
    public string ResourceItemName { get; set; }
    public string ResourceItemValue { get; set; }
} 
public class TranslationEventArgs : EventArgs
{
    private readonly TranslationInfo _fromTranslationInfo;
    private readonly TranslationInfo _toTranslationInfo;
    private readonly Advance _resourceAdvance;
    private readonly Advance _resourceItemAdvance;

    
    public TranslationEventArgs(TranslationInfo fromTranslationInfo, TranslationInfo toTranslationInfo,  Advance resourceAdvance, Advance resourceItemAdvance)
    {
        _fromTranslationInfo = fromTranslationInfo;
        _toTranslationInfo=toTranslationInfo;
        _resourceAdvance = resourceAdvance;
        _resourceItemAdvance = resourceItemAdvance;
    }
    public TranslationInfo FromTranslationInfo=>_fromTranslationInfo;
    public TranslationInfo ToTranslationInfo=>_toTranslationInfo;
    public Advance ResourceAdvance => _resourceAdvance;
    public Advance ResourceItemAdvance => _resourceItemAdvance;


}

public delegate void TranslationEventHandler(object sender, TranslationEventArgs args);
public interface IBatchTranslation
{
    event TranslationEventHandler TranslationEvent;
    ITranslationService TranslationService { get; set; }
    WillCard TranslateKeyFilter { get; set; }
    WillCard ResourceFilter { get; set; }
    CultureInfo DefaultLanguage { get; set; }
    void Translate(CultureInfo fromLanguage, CultureInfo toLanguage);
    void Translate(CultureInfo fromLanguage, IEnumerable<CultureInfo> toLanguages);
}