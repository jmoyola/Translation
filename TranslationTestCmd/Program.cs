using System.Data;
using System.Globalization;
using Oracle.ManagedDataAccess.Client;
using ResxTranslation.Imp;
using TranslationCore.Imp;
using TranslationProperties.Imp;
using TranslationService.Core;
using TranslationService.Utils;
using TranslationsWT.Imp;



namespace MyProject;

class Program
{
    public Program(string[] args)
    {
        try
        {
            //TestTemplateBatchTranslation(args);
            //CoreBatchTranslation(args);
            //TestPropertiesBatchTranslations(args);
            TestResxBatchTranslations(args);
            Console.WriteLine("OK!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("KO!: " + ex);
        }
    }
    
    static void Main(string[] args)
    {
        new Program(args);
    }

    public void TestTemplateBatchTranslation(string[] args)
    {
        ITranslationService txService;
        //txService = new FakeTranslationService();
        DirectoryInfo translationSeviceCacheBaseDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "translationServiceCacheBaseDir");
        txService = new LibreTranslateTranslationService.Imp.LibreTranslateTranslationService(){Url = "http://127.0.0.1:5000/translate"};
        txService=new TranslationServiceCache(txService, translationSeviceCacheBaseDir);
        
        WtTemplateBatchTranslation bs = new WtTemplateBatchTranslation();
        bs.TranslationService = txService;
        bs.BaseDirectory= new DirectoryInfo(@"C:\\Users\\j.oyola\\source\\RS_GitLabRepos\\Configurations_Spor\\Sigma");
        bs.Translate( CultureInfo.GetCultureInfo("en"), CultureInfo.GetCultureInfo("pt"));
    }
    
    public void CoreBatchTranslation(string[] args)
    {
        IDbConnection cnx=null;
        ITranslationService txService;

        try
        {
            string DbHost = "SPORDVHQ2.dev.local";
            int DbPort = 1566;
            string DbServiceName = "HQ1_SN";
            string DbUser;
            string DbPass;

            Console.WriteLine("Enter DbUser: ");
            DbUser = Console.ReadLine();
            Console.WriteLine("Enter DbPass: ");
            DbPass = Console.ReadLine();
            
            string dbConnectionString =
                $"Data Source=(DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = {DbHost})(PORT = {DbPort})) (CONNECT_DATA = (SERVICE_NAME = {DbServiceName}) (UR = A)));User Id={DbUser};Password={DbPass};";

            //txService = new FakeTranslationService();
            DirectoryInfo translationSeviceCacheBaseDir = new DirectoryInfo(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "translationServiceCacheBaseDir");
            txService = new LibreTranslateTranslationService.Imp.LibreTranslateTranslationService()
                { Url = "http://127.0.0.1:5000/translate" };
            txService = new TranslationServiceCache(txService, translationSeviceCacheBaseDir);

            cnx = OracleClientFactory.Instance.CreateConnection();
            cnx.ConnectionString = dbConnectionString;
            
            CoreBatchTranslation bs = new CoreBatchTranslation();
            bs.TranslationService = txService;
            bs.Cnx = cnx;

            cnx.Open();
            bs.Translate( CultureInfo.GetCultureInfo("en"), CultureInfo.GetCultureInfo("pt"));
            
        }
        finally
        {
            cnx?.Close();
        }
    }
    
    public void TestPropertiesBatchTranslations(string[] args)
    {
        ITranslationService txService;
        //txService = new FakeTranslationService();
        DirectoryInfo translationSeviceCacheBaseDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "translationServiceCacheBaseDir");
        txService = new LibreTranslateTranslationService.Imp.LibreTranslateTranslationService(){Url = "http://127.0.0.1:5000/translate"};
        txService=new TranslationServiceCache(txService, translationSeviceCacheBaseDir);
        
        PropertiesBatchTranslation bs = new PropertiesBatchTranslation();
        bs.TranslationService = txService;
        bs.Serializer = new JsonDictionarySerializer();
        bs.BaseDirectory= new DirectoryInfo(@"C:\Users\j.oyola\source\RS_GitLabRepos\frontend-locales_pam\TALO");
        bs.FromLanguageFilePattern = @"{baseDirectory}\{fromLanguage}\*.json";
        bs.ToLanguageFilePattern = @"{baseDirectory}\{toLanguage}\{fileName}";
        bs.Translate( CultureInfo.GetCultureInfo("en"), CultureInfo.GetCultureInfo("pt"));
    }
    
    public void TestResxBatchTranslations(string[] args)
    {
        //var PropertiesFile = new FileDictionary<string, string>(PropertiesFile);
        
        ITranslationService txService;
        //txService = new FakeTranslationService();
        DirectoryInfo translationSeviceCacheBaseDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "translationServiceCacheBaseDir");
        txService = new LibreTranslateTranslationService.Imp.LibreTranslateTranslationService(){Url = "http://127.0.0.1:5000/translate"};
        txService=new TranslationServiceCache(txService, translationSeviceCacheBaseDir);
        
        ResxBatchTranslation bs = new ResxBatchTranslation();
        bs.TranslationService = txService;
        bs.BaseDirectory= new DirectoryInfo(@"C:\Users\j.oyola\source\RS_GitLabRepos\WorldTill - Copy");
        //bs.BaseDirectory = new DirectoryInfo("/tmp/blazor-locale-master");
        bs.DefaultLanguage =CultureInfo.GetCultureInfo("en_GB");
        
        Translate(bs, CultureInfo.GetCultureInfo("en"), CultureInfo.GetCultureInfo("pt"));
        
    }

    private void Translate(IBatchTranslation bs, CultureInfo fromLanguage, CultureInfo toLanguage)
    {
        
        bs.TranslationEvent += (sender, eventArgs) =>
        {
            Console.WriteLine($"{eventArgs.ResourceAdvance.Index}/{eventArgs.ResourceAdvance.Total} {eventArgs.ResourceItemAdvance.Index}/{eventArgs.ResourceItemAdvance.Total} {eventArgs.FromTranslationInfo.Resource}: {eventArgs.FromTranslationInfo.ResourceItemName} ({eventArgs.FromTranslationInfo.Language}->{eventArgs.ToTranslationInfo.Language}) {eventArgs.FromTranslationInfo.ResourceItemValue}->{eventArgs.ToTranslationInfo.ResourceItemValue}  " );
        };
        bs.Translate(fromLanguage, toLanguage);
    }
}




