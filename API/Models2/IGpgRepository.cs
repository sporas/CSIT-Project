using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoApi.Models2
{
    public interface IGpgRepository
    {
        void Add(GpgItem item);
        IEnumerable<GpgItem> GetAll();
        GpgItem Find(string Key);

    }
}
