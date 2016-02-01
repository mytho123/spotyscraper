using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotyScraper.Model.StreamServices
{
    public interface IStreamServiceData
    {
        string Name { get; }
        string Description { get; }
    }
}