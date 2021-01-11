using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;


using DidiSoft.Pgp;

namespace TodoApi.Methods
{
    public class CheckType
    {

        public int GetBase64FileSize (string base64string)
        {
            int length = base64string.Length;
            int countequal = base64string.Count(x => x == '=');
            int filesize = (int)(length * 0.75) - countequal;
            return filesize;

        }
        public string GetFileExtension (string base64String)
        {
            var data = base64String.Substring(0, 5);

            switch (data.ToUpper())
            {
                case "IVBOR":
                    return "png"; 
                case "/9J/4":
                    return "jpg"; 
                case "AAAAF":
                    return "mp4"; 
                case "JVBER":
                    return "pdf"; 
                case "U1PKC":
                    return "txt";
                case "UEsDB":
                    return "docx";
                
                case "MQOWM":
                case "77U/M":
                    return "srt";
                case "AAABA":
                    return "ico";
                case "UMFYI":
                    return "rar";
                case "E1XYD":
                    return "rtf";
                
                default:
                    return string.Empty;
            }
        }

        public string CreatePath (string key, string name, string classification, string filetype)
        {
            StringBuilder strWFBuilderCheck = new StringBuilder();
            

            strWFBuilderCheck.Append(@"C:\Pending\");
            //item.Classification.ToString().ToLower()
            strWFBuilderCheck.Append(classification);

            if (classification == "restricted" || classification == "confidential" || classification == "secret")
            { }
            else
            {
                return "NO";
            }


            strWFBuilderCheck.Append("_");
            //item.Name.ToString()
            strWFBuilderCheck.Append(key);
            strWFBuilderCheck.Append("_");
            strWFBuilderCheck.Append(name);
           

            strWFBuilderCheck.Append(".");
            //bb64type
            strWFBuilderCheck.Append(filetype);
            

            return strWFBuilderCheck.ToString();
            

        }

        public string CreateEncryptedPath(string key, string name, string classification, string filetype)
        {
            StringBuilder buildencryptedpath = new StringBuilder();
            buildencryptedpath.Append(@"\\MURAKAMIPC\Pending_Decryption\");
            // buildencryptedpath.Append(@"C:\Encrypted\");
            //item.Classification.ToString().ToLower()
            buildencryptedpath.Append(classification);
            buildencryptedpath.Append("_");
            //item.Name.ToString()
            buildencryptedpath.Append(key);
            buildencryptedpath.Append("_");
            buildencryptedpath.Append(name);
            buildencryptedpath.Append(".");
            //bb64type
            buildencryptedpath.Append(filetype);
            buildencryptedpath.Append(".pgp");
            return buildencryptedpath.ToString();
            
        }

        public bool PGPEncrypt (string path, string encryptedpath)
        {
            PGPLib pgp = new PGPLib();
            pgp.OverrideKeyAlgorithmPreferences = true;
            pgp.Cypher = CypherAlgorithm.AES_256;
            bool asciiArmor = false;
            bool withIntegrityCheck = true;

            try
            {
                pgp.EncryptFile(path,
                                @"C:\Users\JosephHeller\Documents\Server_Key_0xF81974C9_public.asc",
                                encryptedpath,
                                asciiArmor,
                                withIntegrityCheck);
                return true;
            }
            catch
            {
                return false;
            }

         
        }
    }
}
