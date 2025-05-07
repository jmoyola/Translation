using System;
using System.Data;
using DataBaseUtils.Utils;
using TranslationService.Core;
using TranslationService.Utils;

namespace TranslationCore.Imp
{

    public class CoreBatchTranslation:BaseBatchTranslation
    {
        public IDbConnection Cnx { get; set; }
        public bool Test { get; set; } = true;

        public override void Translate(string fromLanguage, string toLanguage)
        {
            if(Cnx==null)throw new ArgumentNullException(nameof(Cnx));
            if(TranslateKeyFilter==null) throw new TranslationException("TranslateKeyFilter not set");
            if(ResourceFilter==null) throw new TranslationException("ResourceFilter not set");
            
            string srLanguage = fromLanguage.Equals("en", StringComparison.InvariantCultureIgnoreCase)
                ? "UK"
                : fromLanguage;
            
            try
            {
                string cmd = $"SELECT * FROM DBCORE.DCAPPLICATIONMESSAGE WHERE IDDCLANGUAGE ='{srLanguage}' AND MESSAGECODE LIKE {TranslateKeyFilter.ToSql()}";
                var items = Cnx.SelectRows(cmd, true);
                
                for(int iItemIndex=0; iItemIndex<items.Count; iItemIndex++)
                {
                    var item=items[iItemIndex];
                    string fromLngMsg = item.Get<String>("USERDESCRIPTION");
                    string fromLngIntMsg = item.Get<String>("INTERNALDESCRIPTION");
                    string messageCatalog = item.Get<String>("MESSAGECATALOG");
                    string messageCode = item.Get<String>("MESSAGECODE");
                    

                    string toLngMsg = TranslationService.Translate(fromLngMsg, fromLanguage, toLanguage).Result;
                    string toLngIntMsg = TranslationService.Translate(fromLngIntMsg, fromLanguage, toLanguage).Result;
                    
                    Console.WriteLine($"{fromLngMsg}={toLngMsg}");

                    if (!Test)
                    {
                        using (IDbCommand insertCmd = Cnx.CreateCommand())
                        {
                            DbCommandText cmdText =new DbCommandText();
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
                        new TranslationInfo(){Resource = "DBCORE.DCAPPLICATIONMESSAGE", Language = fromLanguage, ResourceItemName = messageCatalog + "|" +srLanguage + "|" + messageCode, ResourceItemValue = fromLngMsg + "" + fromLngIntMsg, ResourceItemAdvance = new Advance(){Index = iItemIndex, Total = items.Count}},
                        new TranslationInfo(){Resource = "DBCORE.DCAPPLICATIONMESSAGE", Language = toLanguage, ResourceItemName = messageCatalog + "|" +srLanguage + "|" + messageCode, ResourceItemValue = toLngMsg + "|" + toLngIntMsg, ResourceItemAdvance=new Advance{Index = iItemIndex, Total = items.Count}}));
                }
            }
            catch (Exception ex)
            {
                throw new TranslationException("Error translation in core:" + ex.Message, ex);
            }
        }
    }
}