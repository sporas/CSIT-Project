using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace TodoApi.Models2
{
    public class GpgRepository : IGpgRepository
    {
        private static ConcurrentDictionary<string, GpgItem> gpgdic = new ConcurrentDictionary<string, GpgItem>();

        public GpgRepository()
        {
            Add(new GpgItem { Key = string.Empty });
        }

        public IEnumerable<GpgItem> GetAll()
        {
            return gpgdic.Values;
        }

        public void Add(GpgItem item)
        {
               item.Key = Guid.NewGuid().ToString();
               gpgdic[item.Key] = item;
          //  gpgdic[item.Name] = item;
          
        }
        
        public GpgItem Find(string Key)
        {
            GpgItem item;
            gpgdic.TryGetValue(Key, out item);
            return item;
        }
        
    }
}
