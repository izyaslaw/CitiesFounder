using System;
using System.Collections.Generic;

namespace _CitiesFinder
{
    internal class CityAndRegion
    {
        public CityFromDB City { get; set; }
        public Region Region { get; set; }

        public CityAndRegion() 
        {
            City = new CityFromDB();
            Region = new Region();
        }
    }
}