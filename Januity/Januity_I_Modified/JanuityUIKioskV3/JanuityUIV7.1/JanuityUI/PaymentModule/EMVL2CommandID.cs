public class EMVL2CommandID
{
    public const int EMV_L2_START_TRANSACTION                   = 0x00;
    public const int EMV_L2_CONTACT_CONTACTLESS_DATABASE_STATUS = 0x01;
    public const int EMV_L2_USER_SELECTION_RESULT               = 0x02;
    public const int EMV_L2_ACQUIRER_RESPONSE                   = 0x03;
    public const int EMV_L2_CANCEL_TRANSACTION                  = 0x04;
    public const int EMV_L2_TRANSACTION_STATUS                  = 0x80;
    public const int EMV_L2_DISPLAY_MESSAGE_REQUEST             = 0x81;
    public const int EMV_L2_USER_SELECTION_REQUEST              = 0x82;
    public const int EMV_L2_ARQC_MESSAGE                        = 0x83;
    public const int EMV_L2_TRANSACTION_RESULT                  = 0x84;
    public const int EMV_L2_PIN_ENTRY_SHOW_PROMPT               = 0x87;
//    public const int EMV_L2_UPDATE_PIN_ENTRY_DISPLAY            = 0x88;
    public const int EMV_L2_PIN_CVM_REQUEST                     = 0x88;
    public const int EMV_L2_PIN_CVM_RESPONSE                    = 0x88;
}

