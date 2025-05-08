using System.Globalization;
using System.Transactions;
using System.Xml;
using TranslationService.Core;
using TranslationService.Utils;

namespace TranslationsWT.Imp;

public class WtTemplateBatchTranslation:BaseBatchTranslation
{
    public DirectoryInfo BaseDirectory { get; set; }
    public override void Translate(CultureInfo fromLanguage, IEnumerable<CultureInfo> toLanguages)
    {
        if(BaseDirectory==null) throw new TranslationException("Base directory not set");
        
        String fromLanguageWT = fromLanguage.TwoLetterISOLanguageName == "en"
            ? "UK"
            : fromLanguage.TwoLetterISOLanguageName;
        
        var templateFiles = BaseDirectory.EnumerateFiles("translations.xml", SearchOption.AllDirectories).ToList();
        for (int iTemplateFileIndex = 0; iTemplateFileIndex < templateFiles.Count; iTemplateFileIndex++)
        {
            var templateFile = templateFiles[iTemplateFileIndex];
            
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(templateFile.FullName);
                

                var allTranslationsNodes = xmlDocument.SelectNodes("/root//trans").Cast<XmlNode>().ToList();

                var fromLanguageTranslationsNodes = allTranslationsNodes
                    .Where(v => v.Attributes["name"]?.Value == fromLanguageWT).ToList();

                foreach (var toLanguage in toLanguages)
                {
                    for (int iFromLanguageTranslationsNodeIndex = 0;
                         iFromLanguageTranslationsNodeIndex < fromLanguageTranslationsNodes.Count;
                         iFromLanguageTranslationsNodeIndex++)
                    {
                        var fromLanguageTranslationNode =
                            fromLanguageTranslationsNodes[iFromLanguageTranslationsNodeIndex];
                        string resourceItemName = fromLanguageTranslationNode.ParentNode?.Name;

                        XmlNode toLanguageTranslationNode = allTranslationsNodes.FirstOrDefault(v =>
                            v.Attributes["name"]?.Value == toLanguage.TwoLetterISOLanguageName);
                        if (toLanguageTranslationNode == null)
                        {
                            toLanguageTranslationNode =
                                fromLanguageTranslationNode.ParentNode?.AppendChild(xmlDocument.CreateElement("trans"));
                            toLanguageTranslationNode.Attributes.Append(xmlDocument.CreateAttribute("name")).Value =
                                toLanguage.TwoLetterISOLanguageName;
                            toLanguageTranslationNode.Attributes.Append(xmlDocument.CreateAttribute("value"));
                        }

                        String txToTranslate = fromLanguageTranslationNode.Attributes["value"].Value;

                        string translation = TranslationService.Translate(txToTranslate,
                            fromLanguage.TwoLetterISOLanguageName, toLanguage.TwoLetterISOLanguageName).Result;
                        toLanguageTranslationNode.Attributes["value"].Value = translation;

                        OnTranslationEvent(new TranslationEventArgs(
                                new TranslationInfo()
                                {
                                    Language = fromLanguage, Resource = templateFile.FullName,
                                    ResourceItemName = resourceItemName, ResourceItemValue = txToTranslate
                                },
                                new TranslationInfo()
                                {
                                    Language = toLanguage, Resource = templateFile.FullName,
                                    ResourceItemName = resourceItemName, ResourceItemValue = translation
                                },
                                new Advance(){Index = iTemplateFileIndex, Total = templateFiles.Count},
                                new Advance(){Index = iFromLanguageTranslationsNodeIndex, Total = fromLanguageTranslationsNodes.Count}
                            )
                        );
                    }
                }

                templateFile.Backup();
                xmlDocument.Save(templateFile.FullName);
            }
            catch (Exception ex)
            {
                throw new TranslationException($"Error in translations '{templateFile.FullName}': {ex.Message}", ex);
            }
        }
    }
}