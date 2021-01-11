using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using System.Threading.Tasks;
using System.Net;


using System.IO;
using System.Text;


using DidiSoft.Pgp;

namespace Tsinfotech_Intranet.TestEncryption
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class TestEncryption : SPItemEventReceiver
    {
        /// <summary>
        /// An item is being updated.
        /// </summary>
        public override void ItemUpdating(SPItemEventProperties properties)
        {
            base.ItemUpdating(properties);
        }

        /// <summary>
        /// An item was updated.
        /// </summary>
        public override void ItemUpdated(SPItemEventProperties properties)
        {
            

            base.ItemUpdated(properties);
            SPListItem Item = properties.ListItem;
            if (Item["Countercheck"].ToString() == "(1) Approved")
            { 
                Item["Title"] = "This is a test";
                Item["Countercheck"] = "(2) Encrypted";
                Item.Update();

               


                StringBuilder strWFBuilderCheck = new StringBuilder();
                strWFBuilderCheck.Append(@"C:\SP_Pending\");

                if (Item["Classified As"].ToString() == "Restricted")
                {
                    strWFBuilderCheck.Append("Restricted");
                }

                else if (Item["Classified As"].ToString() == "Confidential")
                {
                    strWFBuilderCheck.Append("Confidential");
                }

                else if (Item["Classified As"].ToString() == "Secret")
                {
                    strWFBuilderCheck.Append("Secret");
                }

                

                strWFBuilderCheck.Append("_");
                strWFBuilderCheck.Append(properties.ListItem.File.Name);
   
                string WorkfFileLocationcheck;
                WorkfFileLocationcheck = strWFBuilderCheck.ToString();

                

               // BinaryWriter binWritercheck = new BinaryWriter(File.Open(WorkfFileLocationcheck, FileMode.Create));
                byte[] itembytescheck = properties.ListItem.File.OpenBinary(); //only this is needed

                /*
                string itemstringcheck = Encoding.ASCII.GetString(itembytescheck);
                System.Text.ASCIIEncoding encodingcheck = new System.Text.ASCIIEncoding();
                byte[] bytescheck = encodingcheck.GetBytes(itemstringcheck);

                binWritercheck.Write(itembytescheck);
                binWritercheck.Close();
                */
                

                //START API CALL HERE

                string fullname = properties.ListItem.File.Name.ToString();
                string name = System.IO.Path.GetFileNameWithoutExtension(fullname);

                string classification = Item["Classified As"].ToString();

                string data = System.Convert.ToBase64String(itembytescheck);

                string type = fullname.Substring(fullname.LastIndexOf("."));

                var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://encryptionpc:5002/api/gpg");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers["ApiKey"] = "MySecretKey";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {


                    string json = "{\"name\":\"" + name + "\"," +
                            "\"classification\":\"" + classification + "\"," +
                            "\"base64Data\":\"" + data + "\"," +
                            " \"fileType\": \"" + type + "\"}";

                    streamWriter.Write(json);
                }
                
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                

               

                //   Item.Delete();



                /*   PGPLib pgp = new PGPLib();
                   bool asciiArmor = false;
                   bool withIntegrityCheck = true;

                   pgp.EncryptFile(@"C:\Users\Administrator\Documents\java color.txt",
                                   @"C:\Users\Administrator\Documents\Administrator_0x02A6816B_public.asc",
                                   @"C:\Users\Administrator\Documents\java color_encrypted.gpg",
                                   asciiArmor,
                                   withIntegrityCheck); */


            }
            else
            {
                Item["Title"] = "This test hasnt run yet";
                Item.Update();
               
            }
        }
        


    }
}
