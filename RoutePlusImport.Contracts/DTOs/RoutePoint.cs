using RoutePlusImport.Contracts.Attributes;

namespace RoutePlusImport.Contracts.DTOs
{
    public class RoutePoint
    {
        [CsvColumn("Data", Order = 1)]
        public string Date { get; set; }

        [CsvColumn("ID PH", Order = 2)]
        public int ManagerId { get; set; }
        [CsvColumn("Nazwa PH", Order = 3)]
        public string ManagerName { get; set; }

        [CsvColumn("Kolejność", Order = 4)]
        public int Lp { get; set; }

        [CsvColumn("ID Klient", Order = 5)]
        public int ClientId { get; set; }

        [CsvColumn("Nazwa klienta", Order = 6)]
        public string ClientName { get; set; }

        [CsvColumn("Godzina rozpoczęcia", Order = 7)]
        public string StartVisitTime { get; set; }

        [CsvColumn("Czas wizyty", Order = 8)]
        public string VisitMinutes { get; set; }
    }
}
