﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotyScraper.Model.Scrapers
{
    public interface IScraperData
    {
        string Name { get; }
        string Description { get; }
    }
}