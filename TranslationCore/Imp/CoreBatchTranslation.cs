using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using DataBaseUtils.Utils;
using TranslationService.Core;
using TranslationService.Utils;

namespace TranslationCore.Imp
{

    public class CoreBatchTranslation:BaseBatchTranslation
    {
        public IDbConnection Cnx { get; set; }
        public bool Test { get; set; } = true;

        public override void Translate(CultureInfo fromLanguage, IEnumerable<CultureInfo> toLanguages)
        {
            if(Cnx==null)throw new ArgumentNullException(nameof(Cnx));
            if(TranslateKeyFilter==null) throw new TranslationException("TranslateKeyFilter not set");
            if(ResourceFilter==null) throw new TranslationException("ResourceFilter not set");
            
            string srLanguage = fromLanguage.TwoLetterISOLanguageName.StartsWith("en")
                ? "UK"
                : fromLanguage.TwoLetterISOLanguageName;

            foreach (var toLanguage in toLanguages)
            {
                try
                {
                    string cmd =
                        $"SELECT * FROM DBCORE.DCAPPLICATIONMESSAGE WHERE TO_CHAR(IDDCMESSAGECATALOG) LIKE {ResourceFilter.ToSql()} AND IDDCLANGUAGE ='{srLanguage}' AND MESSAGECODE LIKE {TranslateKeyFilter.ToSql()}";
                    var allItems = Cnx.SelectRows(cmd);

                    var catalogItemsGroups=allItems.GroupBy(v => v.Get<decimal>("IDDCMESSAGECATALOG")).ToList();
                    for (int catalogItemsGroupIndex=0;catalogItemsGroupIndex<catalogItemsGroups.Count;catalogItemsGroupIndex++)
                    {
                        var catalogItemsGroup=catalogItemsGroups[catalogItemsGroupIndex];
                        
                        var items=catalogItemsGroup.ToList();
                        for (int iItemIndex = 0; iItemIndex < items.Count; iItemIndex++)
                        {
                            var item = items[iItemIndex];
                            string fromLngMsg = item.Get<String>("USERDESCRIPTION");
                            string fromLngIntMsg = item.Get<String>("INTERNALDESCRIPTION");
                            string messageCatalog = item.Get<String>("MESSAGECATALOG");
                            string messageCode = item.Get<String>("MESSAGECODE");


                            string toLngMsg = TranslationService.Translate(fromLngMsg,
                                fromLanguage.TwoLetterISOLanguageName, toLanguage.TwoLetterISOLanguageName).Result;
                            string toLngIntMsg = TranslationService.Translate(fromLngIntMsg,
                                    fromLanguage.TwoLetterISOLanguageName, toLanguage.TwoLetterISOLanguageName)
                                .Result;

                            Console.WriteLine($"{fromLngMsg}={toLngMsg}");

                            if (!Test)
                            {
                                using (IDbCommand insertCmd = Cnx.CreateCommand())
                                {
                                    DbCommandText cmdText = new DbCommandText();
                                    cmdText.Add("IDDCMESSAGECATALOG", messageCatalog);
                                    cmdText.Add("IDDCLANGUAGE", srLanguage);
                                    cmdText.Add("MESSAGECODE", messageCode);
                                    cmdText.Add("NOOFPARAMETERS", item["NOOFPARAMETERS"]);
                                    cmdText.Add("INTERNALDESCRIPION", toLngIntMsg);
                                    cmdText.Add("USERDESCRIPION", toLngMsg);
                                    cmdText.Add("ISCLIENTSPECIFIC", item["ISCLIENTSPECIFIC"]);

                                    cmdText.Value =
                                        "UPDATE DBCORE.DCAPPLICATIONMESSAGE SET INTERNALDESCRIPION=@INTERNALDESCRIPION, USERDESCRIPION=@USERDESCRIPION WHERE IDDCMESSAGECATALOG=@IDDCMESSAGECATALOG AND IDDCLANGUAGE=@IDDCLANGUAGE AND MESSAGECODE=@MESSAGECODE";

                                    insertCmd.CommandText = cmdText.CommandText;
                                    if (insertCmd.ExecuteNonQuery() == 0)
                                    {
                                        cmdText.Value =
                                            "INSERT INTO DBCORE.DCAPPLICATIONMESSAGE (IDDCMESSAGECATALOG, IDDCLANGUAGE, MESSAGECODE, NOOFPARAMETERS, ISCLIENTSPECIFIC, INTERNALDESCRIPION, USERDESCRIPION) VALUES (@IDDCMESSAGECATALOG, @IDDCLANGUAGE, @MESSAGECODE, @NOOFPARAMETERS, @ISCLIENTSPECIFIC, @INTERNALDESCRIPION, @USERDESCRIPION)";
                                        insertCmd.CommandText = cmdText.CommandText;
                                        insertCmd.ExecuteNonQuery();
                                    }
                                }
                            }

                            OnTranslationEvent(new TranslationEventArgs(
                                new TranslationInfo()
                                {
                                    Resource = $"DBCORE.DCAPPLICATIONMESSAGE/{catalogItemsGroup}", Language = fromLanguage,
                                    ResourceItemName = messageCatalog + "|" + srLanguage + "|" + messageCode,
                                    ResourceItemValue = fromLngMsg + "" + fromLngIntMsg,
                                },
                                new TranslationInfo()
                                {
                                    Resource = $"DBCORE.DCAPPLICATIONMESSAGE/{catalogItemsGroup}", Language = toLanguage,
                                    ResourceItemName = messageCatalog + "|" + srLanguage + "|" + messageCode,
                                    ResourceItemValue = toLngMsg + "|" + toLngIntMsg,
                                },
                                new Advance() { Index = catalogItemsGroupIndex, Total = catalogItemsGroups.Count },
                                new Advance { Index = iItemIndex, Total = items.Count }
                                )
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new TranslationException("Error translation in core:" + ex.Message, ex);
                }
            }
        }
    }
}