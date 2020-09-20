using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace _CitiesFinder
{
    internal class Region 
    {
        [Name("code")]
        public string Code { get; set; } //XX.00
        [Name("asciiname")]
        public string Name { get; set; }

        public Region() 
        {
            Code = "XX.00";
            Name = "";
        }

        public static List<Region> GetRegionsFromDB(string pathRegionsDB)
        {
            List<Region> regionsFromDB = new List<Region>();
            using (StreamReader streamReader = new StreamReader(pathRegionsDB))
            using (CsvReader csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
            {
                csvReader.Configuration.BadDataFound = null;
                csvReader.Configuration.Delimiter = "\t";
                while (csvReader.Read())
                {
                    var regionFromDB = csvReader.GetRecord<Region>();
                    string firstPartCode = regionFromDB.Code.Split(".")[0];
                    if (firstPartCode == "RU")
                        regionsFromDB.Add(regionFromDB);
                }
            }
            return regionsFromDB;
        }
    }
}