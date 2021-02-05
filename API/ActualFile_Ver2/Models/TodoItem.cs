using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoApi.Models
{
    public class TodoItem
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Base64Data { get; set; }
        public string FileType { get; set; }
        public string Classification { get; set; }
        public bool IsComplete { get; set; }
    }
}
