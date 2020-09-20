using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace _CitiesFinder
{
    class Program
    {
        const int RANGE = 60;
        public static string PathDB = "ru.txt";
        public static string PathRegionsDB = "admin1Codes.txt";
        public static string PathInputXL = "Города для транспортной задачи.xlsx";
        public static string PathResultXL = "Решение транспортной задачи.xlsx";
        public static int StartRowInInputXL = 3;
        public static CityFilter СityFilter = new CityFilter { 
            FeatureClass = "P",
            FeatureCodes = "PPL,PPLA,PPLA2,PPLA3,PPLA4,PPLA5,PPLC",
            Population = 100 
        };

        public static void Main(string[] args)
        {
            List<CityFromXL> citiesFromXL = CityFromXL.GetCitiesFromXL(PathInputXL, StartRowInInputXL);
            List<CityFromDB> citiesFromDB = CityFromDB.GetCitiesFromDB(PathDB, СityFilter);
            List<Region> regionsFromDB = Region.GetRegionsFromDB(PathRegionsDB);
            Dictionary<CityFromXL, List<CityAndRegion>> citiesAndRegionsInRange = 
                GetCitiesAndRegionsInRange(citiesFromXL, citiesFromDB, regionsFromDB, RANGE);
            CreateResultXL(PathResultXL, citiesAndRegionsInRange);
        }

        private static Dictionary<CityFromXL, List<CityAndRegion>> GetCitiesAndRegionsInRange
            (List<CityFromXL> citiesFromXL, List<CityFromDB> citiesFromDB, List<Region> regionsFromDB, int RANGE)
        {
            Dictionary<CityFromXL, List<CityAndRegion>> citiesAndRegionsInRange = new Dictionary<CityFromXL, List<CityAndRegion>>();
            foreach (CityFromXL cityFromXL in citiesFromXL)
            {
                CityFromDB cityFromDB = citiesFromDB
                    .FirstOrDefault((cityFromDB) => IsCityNameInAltNames(cityFromXL, cityFromDB));
                if (cityFromDB == null) citiesAndRegionsInRange.Add(cityFromXL, null);
                else
                {
                    List<CityFromDB> citiesInRange = GetCitiesInRange(cityFromDB, citiesFromDB, RANGE);
                    List<CityAndRegion> citiesAndRegions = GetCitiesAndRegions(citiesInRange, regionsFromDB);
                    Region region = regionsFromDB
                        .FirstOrDefault((region) => region.Code.Split(".")[1] == cityFromDB.Admin1code);
                    citiesAndRegionsInRange.Add(cityFromXL, citiesAndRegions);
                }
            }
            return citiesAndRegionsInRange;
        }

        private static bool IsCityNameInAltNames(CityFromXL cityFromXL, CityFromDB cityFromDB)
        {
            foreach (string altCityName in cityFromDB.Alternatenames.Split(","))
                if (altCityName == cityFromXL.Name) return true;
            return false;
        }

        private static List<CityFromDB> GetCitiesInRange(CityFromDB centerCity, List<CityFromDB> citiesFromDB, int RANGE)
        {
            return citiesFromDB
                .Where((cityFromDB) => distanceBetweenCities(centerCity, cityFromDB) < RANGE && centerCity.Name != cityFromDB.Name)
                .ToList();
        }

        private static List<CityAndRegion> GetCitiesAndRegions(List<CityFromDB> citiesInRange, List<Region> regionsFromDB)
        {
            List<CityAndRegion> citiesAndRegions = new List<CityAndRegion>();
            foreach (CityFromDB cityFromDB in citiesInRange)
            {
                Region region = regionsFromDB
                    .FirstOrDefault((region) => region.Code.Split(".")[1] == cityFromDB.Admin1code);
                citiesAndRegions.Add(new CityAndRegion {City = cityFromDB, Region = region });
            }
            return citiesAndRegions;
        }

        private static void CreateResultXL(string pathResultXL, Dictionary<CityFromXL, List<CityAndRegion>> citiesAndRegionsInRange)
        {
            XLWorkbook workbook = new XLWorkbook();
            FillWorkbook(workbook, citiesAndRegionsInRange);
            workbook.SaveAs(pathResultXL);
        }

        private static void FillWorkbook(XLWorkbook workbook, Dictionary<CityFromXL, List<CityAndRegion>> citiesAndRegionsInRange)
        {
            IXLWorksheet wsDry = workbook.Worksheets.Add("Сухие трансформаторы");
            IXLWorksheet wsOil = workbook.Worksheets.Add("Масляные трансформаторы");
            var citiesFromXL = citiesAndRegionsInRange.Keys;
            int dryRowCounter = 1;
            int oilRowCounter = 1;
            foreach (CityFromXL cityFromXL in citiesFromXL)
            {
                List<CityAndRegion> citiesAndRegions = citiesAndRegionsInRange[cityFromXL];
                if (citiesAndRegions == null) citiesAndRegions = new List<CityAndRegion> { new CityAndRegion() };
                AddCitiesAndRegionsToWorkbook(wsDry, wsOil, cityFromXL, citiesAndRegions, ref dryRowCounter, ref oilRowCounter);
            }
            wsDry.Columns().AdjustToContents();
            wsOil.Columns().AdjustToContents();
        }

        private static void AddCitiesAndRegionsToWorkbook(IXLWorksheet wsDry, IXLWorksheet wsOil, CityFromXL cityFromXL, List<CityAndRegion> citiesAndRegions, ref int dryRowCounter, ref int oilRowCounter)
        {
            foreach (CityAndRegion cityAndRegion in citiesAndRegions) 
            {
                if (cityFromXL.IsDry)
                {
                    AddCityAndRegionToWorksheet(wsDry, cityFromXL, cityAndRegion, dryRowCounter);
                    dryRowCounter++;
                }
                if (cityFromXL.IsOil)
                {
                    AddCityAndRegionToWorksheet(wsOil, cityFromXL, cityAndRegion, oilRowCounter);
                    oilRowCounter++;
                }
            }
        }

        private static void AddCityAndRegionToWorksheet(IXLWorksheet ws, CityFromXL cityFromXL, CityAndRegion cityAndRegion, int rowCounter)
        {
            ws.Cell(rowCounter, 1).Value = cityFromXL.Name;
            ws.Cell(rowCounter, 2).Value = GetCyrillicName(cityAndRegion.City);
            ws.Cell(rowCounter, 3).Value = cityAndRegion.Region?.Name;
        }

        private static object GetCyrillicName(CityFromDB city)
        {
            foreach (string altName in city.Alternatenames.Split(","))
            {
                int altNameLength = altName.Length;
                string pattern = @"[а-яА-яёЁйЙ\s\-]{" + $"{altNameLength}" + "}";
                if (Regex.IsMatch(altName, pattern, RegexOptions.IgnoreCase) &&
                    altName != "")
                    return altName;
            }
            return city.Name;
        }

        private static double distanceBetweenCities(CityFromDB city1, CityFromDB city2)
        {
            double geo_lat1 = city1.Latitude * 0.0174533;
            double geo_lat2 = city2.Latitude * 0.0174533;
            double geo_lon1 = city1.Longitude * 0.0174533;
            double geo_lon2 = city2.Longitude * 0.0174533;
            return 6371 * Math.Acos((Math.Sin(geo_lat1) * Math.Sin(geo_lat2)) + 
                (Math.Cos(geo_lat1) * Math.Cos(geo_lat2) * Math.Cos(geo_lon1 - geo_lon2)));
        }
    }
}
