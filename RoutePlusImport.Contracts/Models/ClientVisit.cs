using RoutePlusImport.Contracts.Attributes;

namespace RoutePlusImport.Contracts.Models
{
    public class ClientVisit
    {
        [CsvColumn("ID Wizyty", Order = 1)]
        public int VisitId { get; set; }

        [CsvColumn("ID Klient", Order = 2)]
        public int ClientId { get; set; }

        [CsvColumn("ID PH", Order = 3)]
        public int ManagerId { get; set; }

        [CsvColumn("Klient Nazwa", Order = 4)]
        public string ClientName { get; set; }

        [CsvColumn("Data", Order = 5)]
        public string VisitDate { get; set; }

        [CsvColumn("Godzina rozpoczecia", Order = 6)]
        public TimeSpan VisitStartTime { get; set; }

        [CsvColumn("Godzina zakonczenia", Order = 7)]
        public TimeSpan VisitEndTime { get; set; }
    }
}
