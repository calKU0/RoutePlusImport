using RoutePlusImport.Contracts.Attributes;

namespace RoutePlusImport.Contracts.Models
{
    public class ClientAddress
    {
        [CsvColumn("Id Klient", Order = 1)]
        public int ClientId { get; set; }

        [CsvColumn("ID PH", Order = 2)]
        public int ManagerId { get; set; }

        [CsvColumn("Nazwa", Order = 3)]
        public string ClientName { get; set; }

        [CsvColumn("Kraj", Order = 4)]
        public string Country { get; set; }

        [CsvColumn("Miejscowość", Order = 5)]
        public string City { get; set; }

        [CsvColumn("Kod", Order = 6)]
        public string PostalCode { get; set; }

        [CsvColumn("Ulica", Order = 7)]
        public string Street { get; set; }

        [CsvColumn("Ilość Odwiedzin", Order = 8)]
        public int VisitQuantity { get; set; }

        [CsvColumn("Priorytet Odwiedzin Od", Order = 9)]
        public string VisitDateStart { get; set; }

        [CsvColumn("Priorytet Odwiedzin Do", Order = 10)]
        public string VisitDateEnd { get; set; }
    }
}
