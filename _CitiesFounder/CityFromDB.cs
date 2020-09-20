using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace _CitiesFinder
{
    internal class CityFromDB
    {
        [Name("name")]
        public string Name { get; set; }
        [Name("alternatenames")]
        public string Alternatenames { get; set; }
        [Name("latitude")]
        public double Latitude { get; set; }
        [Name("longitude")]
        public double Longitude { get; set; }
        [Name("feature class")]
        public string FeatureClass { get; set; }
        [Name("feature code")]
        public string FeatureCode { get; set; }
        [Name("country code")]
        public string CountryCode { get; set; }
        [Name("admin1code")]
        public string Admin1code { get; set; }
        [Name("population")]
        public int Population { get; set; }

        public CityFromDB() 
        {
            Name = "";
            Alternatenames = "";
            Latitude = 0;
            Longitude = 0;
            FeatureClass = "";
            FeatureCode = "";
            CountryCode = "";
            Admin1code = "";
            Population = 0;
        }

        public static List<CityFromDB> GetCitiesFromDB(string pathDB, CityFilter cityFilter)
        {
            List<CityFromDB> citiesFromDB = new List<CityFromDB>();
            using (StreamReader streamReader = new StreamReader(pathDB))
            using (CsvReader csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
            {
                csvReader.Configuration.BadDataFound = null;
                csvReader.Configuration.Delimiter = "\t";
                while (csvReader.Read())
                {
                    var cityFromDB = csvReader.GetRecord<CityFromDB>();
                    if (cityFilter.CheckRecord(cityFromDB))
                        citiesFromDB.Add(cityFromDB);
                }
            }
            return citiesFromDB;
        }
    }
}