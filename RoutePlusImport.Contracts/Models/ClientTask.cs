namespace RoutePlusImport.Contracts.Models
{
    public class ClientTask
    {
        public DateTime StartDate { get; set; }                // TERMINROZPOCZECIA
        public DateTime EndDate { get; set; }                  // TERMINZAKONCZENIA
        public int Status { get; set; } = 0;                   // STATUS
        public int GroupId { get; set; } = -1;                 // IDGRUPY
        public int Completed { get; set; } = 0;                // UKONCZONO
        public int ContractorId { get; set; }                  // IDKONTRAHENTA (ID from query)
        public int Priority { get; set; } = 0;                 // PRIORYTET
        public int Done { get; set; } = 0;                     // WYKONANE
        public int Edit { get; set; } = 0;                     // EDIT
        public int ForGroup { get; set; } = 0;                 // DLAGRUPY
        public int TaskTypeId { get; set; } = -1;              // IDTYPZADANIA
        public int TaskKindId { get; set; } = 5611;            // IDRODZAJZADANIA
        public int OrderingPartyId { get; set; } = 78;         // IDZLECENIODAWCY
        public int AssigneeId { get; set; }                    // IDZLECENIOBIORCY
        public DateTime CompletionDate { get; set; } = new DateTime(1899, 12, 29); // DATAUKONCZENIA
        public int IsPrivate { get; set; } = 0;                // PRYWATNE
        public DateTime CreatedDate { get; set; }              // DATAWPISU
        public int OperatorId { get; set; }                    // IDOPERATORA
        public int IsCyclic { get; set; } = 0;                 // CYKLICZNE
        public int CycleEveryDays { get; set; } = -1;          // COILEDNI
        public int Deleted { get; set; } = 0;                  // USUNIETY
        public int StageId { get; set; } = -1;                 // IDETAPU
        public string Info { get; set; } = "";                 // INFO
        public int Archived { get; set; } = 0;                 // ARC
        public int Sync { get; set; } = 1;                     // SYNC
        public int ExternalId { get; set; } = -1;              // EXXID
        public int BranchId { get; set; } = 1;                 // IDODDZIALU
        public int Attachments { get; set; } = 0;              // ZALACZNIKI
        public DateTime ActualStart { get; set; } = new DateTime(1899, 12, 29);  // FAKTSTART
        public DateTime ActualEnd { get; set; } = new DateTime(1899, 12, 29);    // FAKTKONIEC
        public int ActualTimer { get; set; } = 0;              // FAKTTIMER
        public int CycleTaskType { get; set; } = -1;           // TYPCYKLZADANIA
        public decimal Value { get; set; } = 0.00m;            // WARTOSC
        public int CorrespondenceRootId { get; set; } = -1;    // IDKORESPONDENCJADZIENNIKROOT
        public int InstallationId { get; set; } = -1;          // IDINSTALACJI
        public int DeviceId { get; set; } = -1;                // IDURZADZENIA
        public int AddresseeId { get; set; } = -1;             // IDADRESATA
        public int ColorId { get; set; } = -1;                 // IDKOLORU
        public int NotificationOptions { get; set; } = 1;      // OPCJEPOWIADOMIEN
        public double? Latitude { get; set; }                  // LAT
        public double? Longitude { get; set; }                 // LNG
        public int RootTaskId { get; set; } = -1;              // IDZADANIAROOT
        public int TableId { get; set; } = -1;                 // IDTABELI
        public int RecordId { get; set; } = -1;                // IDREKORDU
        public int TimerResolved { get; set; } = 0;            // ROZLTIMER
    }
}
