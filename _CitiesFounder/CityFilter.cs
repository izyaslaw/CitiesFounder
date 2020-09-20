using System;
using System.Collections.Generic;

namespace _CitiesFinder
{
    class CityFilter
    {
        public string FeatureClass { get; set; } = string.Empty;
        public string FeatureCodes { get; set; } = string.Empty;
        public int Population { get; set; } = 0;

        internal bool CheckRecord(CityFromDB cityFromDB)
        {
            return CheckFeatureClass(cityFromDB.FeatureClass) &&
                CheckFeatureCode(cityFromDB.FeatureCode) &&
                CheckPopulation(cityFromDB.Population);
        }

        private bool CheckFeatureClass(string featureClass)
        {
            if (FeatureClass == string.Empty) return true;
            if (FeatureClass == featureClass) return true;
            return false;
        }
        private bool CheckFeatureCode(string featureCode)
        {
            if (FeatureCodes == string.Empty) return true;
            foreach (string FeatureCode in FeatureCodes.Split(","))
                if (FeatureCode == featureCode) return true;
            return false;
        }

        private bool CheckPopulation(int population)
        {
            return (population >= Population);
        }

    }
}
