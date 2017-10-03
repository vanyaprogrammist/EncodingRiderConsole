using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace CyrillicRiderConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Tuner tuner = new Tuner();
            tuner.CreateRegistryKey();
            Console.ReadLine();
        }
    }

    public class Configuration
    {
        public string iisExe { get; set; }
        public string ideaFolder { get; set; }
        public string aspNetProject { get; set; }
        public int encoding { get; set; }
    }

    public class JsonHellper
    {
        private Configuration Deserialize(string jsonString) => JsonConvert.DeserializeObject<Configuration>(jsonString);

        public Configuration ConfFileReader()
        {
            try
            {
                using (StreamReader sr = new StreamReader("../../../configuration.json"))
                {
                    String line = sr.ReadToEnd();
                    return Deserialize(line);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("config.json not found. Error message:");
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }

    public class Tuner
    {
        private Configuration conf;
        private readonly string iisExe;
        private readonly string ideaFolder;
        private readonly string aspNetProject;
        private readonly int encoding;

        public Tuner()
        {
            conf = new JsonHellper().ConfFileReader();
            iisExe = conf.iisExe;
            ideaFolder = conf.ideaFolder;
            aspNetProject = conf.aspNetProject;
            encoding = conf.encoding;
        }

        public void CreateRegistryKey()
        {
            string userRoot = "HKEY_CURRENT_USER";
            string subKey1 = @"Console";
            string subKey2 = $@"""{iisExe}"" /config:{ideaFolder}/.idea/config/applicationhost.config /site:{aspNetProject} /apppool:Clr4IntegratedAppPool";
            string keyName = userRoot + "\\" + subKey1+ "\\" + subKey2;

            if (Registry.GetValue(keyName, "", "") != null)
            {
               Registry.SetValue(keyName,"CodePage",encoding,RegistryValueKind.DWord);
               Console.WriteLine("Encoding of Rider's console for IIS Express has been configured");
               Console.WriteLine($"Now the console encoding: {Registry.GetValue(keyName,"CodePage", "Something went wrong")}");
                Console.WriteLine("---Don't forget to configurate Rider IDE(Use external console) ;)");
               Console.WriteLine("Press any key to exit");
            }
            else
            {
               Registry.CurrentUser.OpenSubKey(subKey1,true).CreateSubKey(subKey2);
               Registry.SetValue(keyName, "CodePage", encoding, RegistryValueKind.DWord);
            }
        }
    }
}
