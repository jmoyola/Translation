using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TranslationService.Core;

namespace LibreTranslateTranslationService.Imp;

public class LibreTranslateTranslationService:ITranslationService
{
    private readonly HttpClient _httpClient= new HttpClient();
    
    public String Url { get; set; } = "https://libretranslate.com/translate";
    public String ApiKey { get; set; } = null;
    
    public async Task<String> Translate(String text, String fromLanguage, String toLanguage)
    {
        fromLanguage = fromLanguage.ToLower();
        toLanguage = toLanguage.ToLower();
        
        LibreTranslateRequest reqJson = new LibreTranslateRequest()
        {
            Text=text,
            Source = fromLanguage,
            Target = toLanguage,
            ApiKey = ApiKey
        };
        
        var response = await _httpClient.PostAsync(
            Url, 
            new StringContent(JsonSerializer.Serialize(reqJson), Encoding.UTF8, "application/json")
            ).ConfigureAwait(false);

        String sRet = response.Content.ReadAsStringAsync().Result;
        LibreTranslateResponse oRet = JsonSerializer.Deserialize<LibreTranslateResponse>(sRet);
        return oRet.TranslatedText;
    }

    private sealed class LibreTranslateRequest
    {
        [JsonPropertyName("q")]
        public String Text { get; set; }
        
        [JsonPropertyName("source")]
        public String Source { get; set; }
        
        [JsonPropertyName("target")]
        public String Target { get; set; }
        
        [JsonPropertyName("format")]
        public String Format { get; set; } = "text";
        
        [JsonPropertyName("api_key")]
        public String ApiKey { get; set; }
    }
    
    private sealed class LibreTranslateResponse
    {
        [JsonPropertyName("translatedText")]
        public String TranslatedText { get; set; }
    }   
}