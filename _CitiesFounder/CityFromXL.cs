using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Text;

namespace _CitiesFinder
{
    class CityFromXL
    {
        public string Name { get; set; }
        public bool IsDry { get; set; } = false;
        public bool IsOil { get; set; } = false;
        public static List<CityFromXL> GetCitiesFromXL(string pathInputXL, int startRowInInputXL)
        {
            var workbook = new XLWorkbook(pathInputXL);
            var firstWorksheet = workbook.Worksheet(1);
            var rows = firstWorksheet.RangeUsed().RowsUsed();
            List<CityFromXL> citiesFromXL = new List<CityFromXL>();

            foreach (var row in rows)
            {
                if (row.RowNumber() > startRowInInputXL)
                {
                    string dryCityName = row.Cell(1).Value.ToString();
                    string oilCityName = row.Cell(3).Value.ToString();

                    if (dryCityName != string.Empty) AddToCitiesFromXL(citiesFromXL, dryCityName, true);
                    if (oilCityName != string.Empty) AddToCitiesFromXL(citiesFromXL, oilCityName, false);
                }
            }
            return citiesFromXL;
        }

        private static void AddToCitiesFromXL(List<CityFromXL> citiesFromXL, string cityName, bool isDry)
        {
            CityFromXL cityAlreadyAdded = citiesFromXL.Find((city) => city.Name == cityName);
            if (isDry)
            {
                if (cityAlreadyAdded != null)
                    cityAlreadyAdded.IsDry = true;
                else
                    citiesFromXL.Add(new CityFromXL { Name = cityName, IsDry = true });
            }
            else
            {
                if (cityAlreadyAdded != null)
                    cityAlreadyAdded.IsOil = true;
                else
                    citiesFromXL.Add(new CityFromXL { Name = cityName, IsOil = true });
            }
        }

    }
}
