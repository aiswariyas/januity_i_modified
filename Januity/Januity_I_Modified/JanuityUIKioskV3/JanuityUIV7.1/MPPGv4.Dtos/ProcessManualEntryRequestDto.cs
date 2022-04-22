namespace MPPGv4.Dtos
{
    public class ProcessManualEntryRequestDto 
    {
        public decimal? Amount { get; set; }
        public string TransactionType { get; set; }
        public string ProcessorName { get; set; }
        public string State { get; set; }
        public string PAN { get; set; }
        public string NameOnCard { get; set; }
        public string ExpirationDate { get; set; }
        public string CVV { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine1 { get; set; }
        public string CustomerTransactionID { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string CustomerCode { get; set; }
    }
}
