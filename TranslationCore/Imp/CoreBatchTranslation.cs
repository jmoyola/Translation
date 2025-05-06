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


            IDictionarySerializer<string, string> serializer = new PropertiesDictionarySerializer();
            
            string srLanguage = fromLanguage.Equals("en", StringComparison.InvariantCultureIgnoreCase)
                ? "UK"
                : fromLanguage;

            ConnectionState oldState = Cnx.State;
            try
            {
                IDbCommand cmd = Cnx.CreateCommand();
                cmd.CommandText = $"SELECT * FROM DBCORE.DCAPPLICATIONMESSAGE WHERE IDDCLANGUAGE ='{srLanguage}' AND MESSAGECODE LIKE {TranslateKeyFilter.ToSql()}";
                IDataReader dr = cmd.ExecuteReader();
                
                while (dr.Read())
                {
                    string fromLngMsg = dr.Get<String>("USERDESCRIPTION");
                    string fromLngIntMsg = dr.Get<String>("INTERNALDESCRIPTION");
                    string messageCatalog = dr.Get<String>("MESSAGECATALOG");
                    string messageCode = dr.Get<String>("MESSAGECODE");
                    

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
                            cmdText.Add("NOOFPARAMETERS", dr, "NOOFPARAMETERS");
                            cmdText.Add("INTERNALDESCRIPION", toLngIntMsg);
                            cmdText.Add("USERDESCRIPION", toLngMsg);
                            cmdText.Add("ISCLIENTSPECIFIC", dr, "ISCLIENTSPECIFIC");

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
                    OnTranslationEvent(new TranslationEventArgs(fromLanguage, toLanguage, messageCatalog + "|" +srLanguage + "|" + messageCode, fromLngMsg + "" + fromLngIntMsg , toLngMsg + "|" + toLngIntMsg, "DBCORE.DCAPPLICATIONMESSAGE"));
                }
            }
            catch (Exception ex)
            {
                throw new TranslationException("Error translation in core:" + ex.Message, ex);
            }
        }
    }
}