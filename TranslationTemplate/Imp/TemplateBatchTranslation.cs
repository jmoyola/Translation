using System.Transactions;
using System.Xml;
using TranslationService.Core;

namespace TranslationsWT.Imp;

public class WtTemplateBatchTranslation:BaseBatchTranslation
{
    public DirectoryInfo BaseDirectory { get; set; }
    public override void Translate(String fromLanguage, String toLanguage)
    {
        if(BaseDirectory==null) throw new TranslationException("Base directory not set");
        
        try
        {
            FileInfo template = new FileInfo(BaseDirectory.FullName
                                             + Path.DirectorySeparatorChar + "share"
                                             + Path.DirectorySeparatorChar + "Templates"
                                             + Path.DirectorySeparatorChar + "translations.xml");
            
            FileInfo templateOut = new FileInfo(BaseDirectory.FullName
                                             + Path.DirectorySeparatorChar + "share"
                                             + Path.DirectorySeparatorChar + "Templates"
                                             + Path.DirectorySeparatorChar + "translations_out.xml");

            if (!template.Exists)
                throw new TransactionException($"base '{template.FullName}' file dont exists.");

            XmlDocument xd = new XmlDocument();
            xd.Load(template.FullName);
            XmlNode? n = xd.ChildNodes[1];

            String fromLanguageWT = fromLanguage == "EN" ? "UK" : fromLanguage;
            foreach (XmlNode tn in n.ChildNodes)
            {
                XmlNode bnt = tn.ChildNodes.Cast<XmlNode>()
                    .FirstOrDefault(n => n.Name.Equals("trans")
                                         && n.Attributes.Cast<XmlAttribute>()
                                             .Any(a => a.Name == "name" && a.Value == fromLanguageWT));
                if (bnt == null)
                    continue;

                String txToTranslate = bnt.Attributes.Cast<XmlAttribute>().FirstOrDefault(a => a.Name == "value")
                    ?.Value;

                if (txToTranslate != null)
                {
                    Task<String> txTask =TranslationService.Translate(txToTranslate, fromLanguage, toLanguage);
                    txTask.Wait();
                    
                    XmlAttribute at;
                    
                    XmlNode newTxNode =tn.ChildNodes.Cast<XmlNode>()
                        .FirstOrDefault(n => n.Name.Equals("trans")
                                             && n.Attributes.Cast<XmlAttribute>()
                                                 .Any(a => a.Name == "name" && a.Value == toLanguage));
                    if (newTxNode == null)
                    {
                        newTxNode = xd.CreateNode(XmlNodeType.Element, "trans", null);
                        tn.AppendChild(newTxNode);
                        
                        at = xd.CreateAttribute("name");
                        at.Value = toLanguage;
                        newTxNode.Attributes.Append(at);
                    }

                    at = newTxNode.Attributes.Cast<XmlAttribute>().FirstOrDefault(a => a.Name == "value");
                    if (at == null)
                    {
                        at = xd.CreateAttribute("value");
                        newTxNode.Attributes.Append(at);
                    }
                    at.Value = txTask.Result;
                }

            }

            xd.Save(templateOut.FullName);
        }
        catch (Exception ex)
        {
            throw new TranslationException($"Error in translations '{BaseDirectory.FullName}' root path: {ex.Message}", ex);
        }
    }
}