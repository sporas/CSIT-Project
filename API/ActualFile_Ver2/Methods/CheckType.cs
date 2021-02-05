using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;


using DidiSoft.Pgp;
using DidiSoft.Pgp.Exceptions;

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
                case "UESDB":
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
            buildencryptedpath.Append(@"\\192.168.1.43\Pending_Decryption\");
          //   buildencryptedpath.Append(@"C:\Encrypted\");
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
                               @"C:\Users\Administrator.LAB1\Documents\Administrator_0xA5813C50_public.asc",
                                encryptedpath,
                                asciiArmor,
                                withIntegrityCheck);
                return true;
            }
            catch (PGPException e)
            {
                // Here we can try to identify the exception
                // and take recovery actions
                if (e is NonPGPDataException)
                {
                    Console.WriteLine("The passed encrypted file is not a valid OpenPGP archive");
                }
                else if (e is IntegrityCheckException)
                {
                    Console.WriteLine("The passed encrypted file is corrupted");
                }
                else if (e is FileIsPBEEncryptedException)
                {
                    Console.WriteLine("The passed encrypted file is encrypted with a password " +
                                      "but we try to decrypt it with a private key");
                }
                else if (e is WrongPrivateKeyException)
                {
                    Console.WriteLine(e.Message);
                }
                else if (e is WrongPasswordException)
                {
                    Console.WriteLine("The password for the provided private key is wrong");
                }
                else if (e is WrongPublicKeyException)
                {
                    Console.WriteLine(e.Message);
                }
                else
                {
                    Console.WriteLine("General decryption error not among the above ones ");
                    Console.WriteLine(e.Message);
                }
                return false;
            }


        }
    }
}
