using System;
using System.Threading.Tasks;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.Translate.V3;
using TranslationService.Core;

namespace GoogleTranslateTranslationService.Imp;

public class GoogleTranslateTranslationService:ITranslationService
{
    private readonly TranslationServiceClient _client= TranslationServiceClient.Create();
    
    public String ProjectId { get; set; } = null;
    
    public async Task<String> Translate(String text, String fromLanguage, String toLanguage)
    {
        fromLanguage = fromLanguage.ToLower();
        toLanguage = toLanguage.ToLower();
        
        TranslateTextRequest request = new TranslateTextRequest
        {
            Contents = { text },
            SourceLanguageCode = fromLanguage,
            TargetLanguageCode = toLanguage,
            Parent = new ProjectName(ProjectId).ToString()
        };
        TranslateTextResponse response = await _client.TranslateTextAsync(request);
        // response.Translations will have one entry, because request.Contents has one entry.
        Translation translation = response.Translations[0];

        return translation.TranslatedText;
    }
}