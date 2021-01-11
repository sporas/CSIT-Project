using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TodoApi.Models2;
using System.IO;
using System.Text;
using DidiSoft.Pgp;
using TodoApi.Filters;
using Serilog;
using TodoApi.Methods;


namespace TodoApi.Controllers
{
    [Route("api/gpg")]
    [ApiController]
    [ApiKeyAuth]
    public class GpgController : ControllerBase
    {
        public GpgController(IGpgRepository gpgItemsTemp)
        {
            GpgItems = gpgItemsTemp;
        }
        public IGpgRepository GpgItems { get; set; }

        public IEnumerable<GpgItem> GetAll()
        {
            return GpgItems.GetAll();
        }

        
        [HttpGet("{id}", Name = "GetGpg")]
        public IActionResult GetById(string id)
        {
            var item = GpgItems.Find(id);
            if(item==null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }
        


        [HttpPost]
        [DisableRequestSizeLimit] //Disables the size limit of the request body. Default is 30MB
        public IActionResult Create([FromBody] GpgItem item)
        {
            //Creates new logger using Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(@"C:\Serilogs\mylogs.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            /*
            //Send 400 if no item found
            if(item == null)
            {
                Log.Error("Status Code 400: Bad Request");
                Log.CloseAndFlush();
                return "testing");
            }
            */
         

            GpgItems.Add(item);
            Log.Information("GUID Key: {A}", item.Key.ToString());

            //Using CheckType class to access methods
            CheckType testing = new CheckType();


            //Remove fullstop from given filetype if applicable
            string itemFileType = item.FileType.ToString().ToLower();
            if (itemFileType.Contains('.'))
            {
                itemFileType = itemFileType.Replace(".", "");
                Log.Information("fullstop has been removed from itemFileType. Current value: {A}", itemFileType);
            }
          

            //Calculate filesize based on Base64 encoded data
            int filesize = testing.GetBase64FileSize(item.Base64Data.ToString());
            Log.Information("Calculated Base64File Size: {A} bytes", filesize.ToString());


            //Check if filesize exceeds 20 million. If it does, returns 413 status code
            if(filesize > 20000000) //More than 20 million
            {
                Log.Error("Filesize exceeded 20 milion bytes, getting deleted");
                Log.CloseAndFlush();
                return StatusCode((int)413,"Filesize exceeded 20 million bytes, send smaller size");

            }


            //Executes GetFileExtension method: Checks Base64 data and retrieves filetype from it 
            string bb64type = "";
            string gettingtype = testing.GetFileExtension(item.Base64Data.ToString());
           /* if (bb64type == "FALSE")
            {
                Log.Error("File submitted is not of valid type");
                Log.CloseAndFlush();
                return StatusCode((int)415, "File submitted is not of valid type. Please submit either docx, jpg, mp4, pdf, png or txt");
            }
            else
            {
                Log.Information("Filetype of Base64 data is : {A}", bb64type);
            } */
            
            if (gettingtype == itemFileType) 
            {
                bb64type = gettingtype;
                Log.Information("Entered #1: User filetype given is same as Base64 filetype");
            }
            else
            {
                if (gettingtype == "")
                {
                    bb64type = itemFileType;
                    Log.Information("Entered #2: Base64 filetype not found in switch statement - Using user filetype instead");
                }
                else
                {
                    bb64type = gettingtype;
                    Log.Information("Entered #3: User entered the wrong filetype for some reason - Trusting Base64 filetype");
                }
            }
            Log.Information("bb64type: {A}", bb64type);  
                

            //Creates the INPUT filepath to be used in the encryption process
            string path = testing.CreatePath(item.Key.ToString(), item.Name.ToString(), item.Classification.ToString().ToLower(), bb64type);


            //If Classification is wrong, return a 422 error
            if (path == "NO")
            {
                Log.Error("Classification has been wrongly set by user. Incorrect input: {classification}", item.Classification.ToString().ToLower());
                Log.CloseAndFlush();
                return StatusCode(422,"Error: Classification attribute is wrongly set. Ensure that Classification is either 'Restricted', 'Confidential', or 'Secret'.");
            }
            Log.Information("Full Built Path: {path}", path);


            //Creates the OUTPUT filepath to be used in the encryption process
            string encryptedpath = testing.CreateEncryptedPath(item.Key.ToString(), item.Name.ToString(), item.Classification.ToString().ToLower(), bb64type);
            Log.Information("Final Encrypted Path: {path}", encryptedpath);


            //Attempt decoding of base64 string, if it doesn't work, return 422 error
            try
            {
                byte[] ByteArray = Convert.FromBase64String(item.Base64Data.ToString());
                Log.Information("Base64 string successfully converted to ByteArray");
                System.IO.File.WriteAllBytes(path, ByteArray);
                Log.Information("ByteArray successfully written to filepath");
            }
            catch
            {
                Log.Error("Data unable to be written. Check Base64 encoding");
                Log.CloseAndFlush();
                return StatusCode(422,"Error: Data could not be written. " +
                                "The input is not a valid Base64 string as it contains a non-Base64 character, more than two padding characters, or an illegal character among the padding characters.");
            }
            

            // PGP ENCRYPTION STARTS   
            bool pgpstatus = testing.PGPEncrypt(path, encryptedpath);
            if (pgpstatus == true)
            {
                Log.Information("File has been encrypted with public key");
            }
            
            
            //Delete file at input path after decryption is successful
            System.IO.File.Delete(path);
            Log.Information("File has been deleted from: {path}\n\n", path);


            //Close serilog
            Log.CloseAndFlush();
           //  return CreatedAtRoute("GetGpg", new { id = item.Key }, item);


            //Return 200 OK
            return StatusCode(200, item);
        }

     
    }
}
