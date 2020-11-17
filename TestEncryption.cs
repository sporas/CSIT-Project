using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;

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

                
                strWFBuilderCheck.Append("Approved");
                strWFBuilderCheck.Append("_");
                strWFBuilderCheck.Append(properties.ListItem.File.Name);
   
                string WorkfFileLocationcheck;
                WorkfFileLocationcheck = strWFBuilderCheck.ToString();

                BinaryWriter binWritercheck = new BinaryWriter(File.Open(WorkfFileLocationcheck, FileMode.Create));
                byte[] itembytescheck = properties.ListItem.File.OpenBinary();
                string itemstringcheck = Encoding.ASCII.GetString(itembytescheck);
                System.Text.ASCIIEncoding encodingcheck = new System.Text.ASCIIEncoding();
                byte[] bytescheck = encodingcheck.GetBytes(itemstringcheck);

                binWritercheck.Write(itembytescheck);
                binWritercheck.Close();



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
