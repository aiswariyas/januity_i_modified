namespace MPPGv4.Dtos
{
    public class ProcessTokenRequestDto 
    {
        public string AdditionalRequestData_Key { get; set; }
        public string AdditionalRequestData_Value { get; set; }
        public string CustomerCode { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string CustomerTransactionID { get; set; }
        public string Token { get; set; }
        public decimal? Amount { get; set; }
        public string TransactionType { get; set; }
        public string ProcessorName { get; set; }
    }
}
