using System;
using System.Data;
using System.Data.Common;
using System.IO;
using DataBaseUtils.Utils;
using TranslationService.Core;
using TranslationService.Utils;

namespace TranslationCore.Imp
{

    public class CoreBatchTranslation:BaseBatchTranslation
    {
        public IDbConnection Cnx { get; set; }
        public bool Test { get; set; } = true;
        public string Filter { get; set; } = string.Empty;
        public FileInfo PropertiesFile {get;set; }
        public override void Translate(string fromLanguage, string toLanguage)
        {
            if(Cnx==null)throw new ArgumentNullException(nameof(Cnx));

            IDictionarySerializer<string, string> serializer = new PropertiesDictionarySerializer();
            
            string srLanguage = fromLanguage.Equals("en", StringComparison.InvariantCultureIgnoreCase)
                ? "UK"
                : fromLanguage;

            ConnectionState oldState = Cnx.State;
            try
            {
                IDbCommand cmd = Cnx.CreateCommand();
                cmd.CommandText = $"SELECT * FROM DBCORE.DCAPPLICATIONMESSAGE WHERE IDDCLANGUAGE ='{srLanguage}'" + (string.IsNullOrEmpty(Filter)?"":" AND "+ Filter);
                IDataReader dr = cmd.ExecuteReader();
                IFileDictionary<string, string> dic = null;
                if (PropertiesFile != null) dic = new FileDictionary<string, string>(PropertiesFile);
                while (dr.Read())
                {
                    string fromLngMsg = dr.Get<String>("USERDESCRIPTION");
                    string fromLngIntMsg = dr.Get<String>("INTERNALDESCRIPTION");
                    string messageCode = dr.Get<String>("MESSAGECODE");

                    string toLngMsg = TranslationService.Translate(fromLngMsg, fromLanguage, toLanguage).Result;
                    string toLngIntMsg = TranslationService.Translate(fromLngIntMsg, fromLanguage, toLanguage).Result;
                    dic?.Add(messageCode, toLngIntMsg+"|"+toLngMsg);
                    
                    Console.WriteLine($"{fromLngMsg}={toLngMsg}");

                    if (!Test)
                    {
                        using (IDbCommand insertCmd = Cnx.CreateCommand())
                        {
                            DbCommandText cmdText =new DbCommandText();
                            cmdText.Add("IDDCMESSAGECATALOG", messageCode);
                            cmdText.Add("IDDCLANGUAGE", toLanguage);
                            cmdText.Add("MESSAGECODE", dr, "MESSAGECODE");
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
                }

                dic?.Save(serializer);
            }
            catch (Exception ex)
            {
                throw new TranslationException("Error translation in core:" + ex.Message, ex);
            }
        }
    }
}