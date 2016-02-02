using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotyScraper.Model.Tracks
{
    public interface ITrackMatch
    {
        string Title { get; }
        string[] Artists { get; }
    }
}