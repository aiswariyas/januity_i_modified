using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using MTCMS;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using MPPGv4.ServiceFactory;
using MPPGv4.UIFactory;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using JanuityPayment;
using Microsoft.Extensions.Configuration.Json;

namespace JanuityUI.Modules
{
    class oDynanoPayment
    {
        protected const int BIG_BLOCK_DATA_SIZE = 900;
        protected const int BIG_BLOCK_DATA_SMALL_SIZE = 45;

        // Selection Type
        private static byte SELECTION_TYPE_LANGUAGE = 0x01;

        // Selection Status
        private static byte SELECTION_STATUS_COMPLETED = 0x00;
        private static byte SELECTION_STATUS_CANCELLED = 0x01;
        private static byte SELECTION_STATUS_TIMED_OUT = 0x02;

        private static int m_emvARCType;
        //private bool m_enableSwipe = false;
        //private bool m_enableChip = true;
        //private bool m_enableContactless = false;

        private static byte[] ApprovedARC = new byte[] { (byte)0x8A, 0x02, 0x30, 0x30 };
        private static byte[] DeclinedARC = { (byte)0x8A, 0x02, 0x30, 0x35 };
        private static int mSubscriptionOption = 0;
        private static int mMSRRequestOption = 0;
        private static MTDevice mDevice;
        private static MTConnectionType mConnectionType;
        private static MTConnectionState mConnectionState;
        protected static string mPINEntryDisplay = "";
        protected static string mBaudRate = "9600";
        protected static string mDataBits = "8";
        protected static string mParity = "NONE";
        protected static string mStopBits = "1";
        protected static string mHandshake = "NONE";
        protected static string mStartingByte = "";
        protected static string mEndingByte = "0x0A";
        protected static bool mCRC = false;
        protected static int mReceivingBigBlockDataTotalLength;
        protected static int mReceivingBigBlockDataReceivedLength;
        protected static int mReceivingBigBlockDataLastPacketID;
        protected static byte[] mRecevingBigBlockData;
        protected static bool mSendingBigBlockData = false;
        protected static byte[] mBigBlockData = null;
        protected static int mBigBlockByteCount = 0;
        protected static int mBigBlockPacketCount = 0;
        protected static int mLatchValue = 0;
        protected static bool mRequestedPANForPINBlock = false;

        protected static bool mPendingResponse = false;
        protected static List<MTCMSRequestMessage> mRequestList = new List<MTCMSRequestMessage>();

        delegate void deviceListDispatcher(List<MTDeviceInformation> deviceList);
        delegate void updateDisplayDispatcher(string text);
        delegate void clearDispatcher();
        delegate void updateStateDispatcher(MTConnectionState state);
        delegate void userSelectionsDisptacher(string title, List<string> selectionList, long timeout);
        delegate void displayPINWindowDisptacher(long timeout);

        static int CustomerTransactionID = 0;
        static string Data = "";
        static string KSN = "";
        static string CardPresent = "";

        Version versionInfo = Assembly.GetExecutingAssembly().GetName().Version;

        //this.Title = "MagTek CMS .NET Demo " + versionInfo.ToString();
        public static void PaymentInit() {
            kioskLog.SrushtyLog_oDynamo("oDynamo PaymentInit******");
            mDevice = new MTDevice();

            mDevice.OnDeviceList += OnDeviceList;
            mDevice.OnDeviceConnectionStateChanged += OnDeviceConnectionStateChanged;
            mDevice.OnDeviceDataString += OnDeviceDataString;
            mDevice.OnDeviceDataBytes += OnDeviceDataBytes;
            mDevice.OnDeviceResponseMessage += OnDeviceResponseMessage;
            mDevice.OnDeviceNotificationMessage += OnDeviceNotificationMessage;


            updateState(MTConnectionState.Disconnected);

            
            connect();
            
        }


        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                int Start, End;
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }

            return "";
        }

        private static void sendToDisplay(string source)
        {
            //OutputTextBox.AppendText("source = " + source + "\n");
            try
            {
                CardPresent = getBetween(source, "Card Present=[", "]").Replace(" ", string.Empty);
                if (CardPresent.Contains("1"))
                {
                    CustomerTransactionID = (int)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                    //OutputTextBox.AppendText("Card Present = " + CardPresent + "\n");
                }


                if (source.Contains("MSR Data=["))
                {
                    Data = getBetween(source, "MSR Data=[", "]").Replace(" ", string.Empty);
                }


                if (source.Contains("KSN="))
                {
                    kioskLog.SrushtyLog_oDynamo("oDynamo KSN ******"+ source);

                    KSN = getBetween(source, "KSN=", "\nCard.EncodeType=01").Replace(" ", string.Empty);

                    IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();

                    IServiceCollection services = new ServiceCollection();
                    services.AddSingleton<IConfiguration>(config);
                    services.AddSingleton<IMppgv4UIFactory, Mppgv4UIfactory>();
                    services.AddSingleton<IProcessCardSwipeClient, ProcessCardSwipeClient>();
                    services.AddSingleton<IProcessKeyPadEntryClient, ProcessKeyPadEntryClient>();
                    services.AddSingleton<IProcessDataClient, ProcessDataClient>();
                    services.AddSingleton<IProcessManualEntryClient, ProcessManualEntryClient>();
                    services.AddSingleton<IProcessTokenClient, ProcessTokenClient>();
                    IServiceProvider serviceProvider = services.BuildServiceProvider();
                    var uiFactory = serviceProvider.GetService<IMppgv4UIFactory>();

                    int Amount = 00015;
                    //string Data = "0243F982023FDFDF0B03010001DFDF540A950002000F51A82000C3DFDF550182DFDF251031313131303030303030303032464541FA82020D70820209DFDF5301005F201A524147554C20202020202020202020202020202020202020202F5F30020226DFDF4D273B353336303139303030303030313037313D32353032323236303030303030303030303030303FDFDF520105F88201AEDFDF59820190639039ECF21AA80F796F4A54DB6C2325C96C339C0E5BDCB3D49EE2A4A5296421123B3F469247DA94570E667E3384A051C4B7BD80AB42A33C9721E93D747C096652BAD5AB9B10975AB28D05BB9840653205234E1539801B0BC5E53B036298FD1B7B0C15C058341259E09B6DF26626CB4578941F717F6A2AB1646B6165154923FDE1491436558E858E121AD35CCCE116D1EE8EB662301446AD7955BB24585FDA5E1714C70455C1E3E73DDD4889732E239D7721F5B0A1E79A4AAE932A8DF9E09F4FACA0B6235B510AF55718D2A5101C4A5B9463C324403E087CEA2EA3B7AA17C00AC064A2BD0FEA68ECA4D7C78165EA8B89D48B48E12AB7F2ABCDBCA32923D2CC312F9AC802602703B121B0F42E1850D582D6950B33F4F1DB6C782D2B98F7D4D82B4AEAF28A62BC7743942A413D5AB41635C4CFE5EF0BB25B998F2D29B0176B3D5A9E138A9A6E3B527E8341E9BB0BF4C481429CE7A8407F04BFAF63C6B1C6267F84C7BA01B865FF40E1451CCBADA63D8C2BDF57556902415C0D40065C6659CF0916107A6A6B86A78B600CEA7A94A45ABB2ADFDF560A950002000F51A82000C3DFDF570180DFDF580106000000000097B2B392";               
                    //string KSN = "950002000F51A82000C3";
                    //string CustomerTransactionID = "354542354366556";

                    //OutputTextBox.AppendText("source = " + source + "\n");
                    // OutputTextBox.AppendText("CustomerTransactionID = " + CustomerTransactionID + "\n");
                    //OutputTextBox.AppendText("MSR Data = " + Data + "\n" + Data.Length);
                    // OutputTextBox.AppendText("KSN = " + KSN + "\n");

                    uiFactory.ShowUI(MPPGv4UI.PROCESSDATA, Data, KSN, Amount, CustomerTransactionID.ToString());
                    
                }

                //OutputTextBox.ScrollToCaret();
            }
            catch (Exception)
            {
            }
        }

        private static MTConnectionType getConnectionType()
        {
            int connectionTypeIndex = 0;// DeviceTypeCB.SelectedIndex;

            MTConnectionType connectionType = MTConnectionType.USB;

            switch (connectionTypeIndex)
            {
                case 0:
                    connectionType = MTConnectionType.USB;
                    break;
                case 1:
                    connectionType = MTConnectionType.IP;
                    break;
                case 2:
                    connectionType = MTConnectionType.Serial;
                    break;
            }

            return connectionType;
        }

        private static void connect()
        {

            kioskLog.SrushtyLog_oDynamo("oDynamo connect ******");
            if (mConnectionState == MTConnectionState.Connected)
            {
                return;
            }

            mSendingBigBlockData = false;

            mConnectionType = getConnectionType();

            mDevice.setConnectionType(mConnectionType);

            //string address = getAddress(mConnectionType);

            //mDevice.setAddress(address);

            mDevice.openDevice();
        }

        private static void displayDeviceInformation()
        {

        }

        protected static void OnDeviceList(object sender, MTConnectionType connectionType, List<MTDeviceInformation> deviceList)
        {
            updateDeviceList(deviceList);
        }

        protected static void OnDeviceConnectionStateChanged(object sender, MTConnectionState state)
        {
            updateState(state);
        }

        protected static void OnDeviceDataString(object sender, string dataString)
        {
            sendToDisplay("[Device Data]");
            sendToDisplay(dataString);
        }

        protected static void OnDeviceDataBytes(object sender, byte[] dataBytes)
        {
        }

        protected static void OnDeviceResponseMessage(object sender, MTCMSResponseMessage response)
        {
            sendToDisplay("[Response From Magtek Device]");

            processResponseMessage(response);

            if (mPendingResponse)
            {
                mPendingResponse = false;
            }

            sendPendingRequests();
        }

        protected static void OnDeviceResponseMessageCustom(MTCMSResponseMessage response)
        {
            sendToDisplay("[Response From Magtek Device]");

            processResponseMessage(response);

            if (mPendingResponse)
            {
                mPendingResponse = false;
            }

            sendPendingRequests();
        }

        protected static void OnDeviceNotificationMessage(object sender, MTCMSNotificationMessage notification)
        {
            //sendToDisplay("[Device Notification]");OnDeviceNotificationMessageCustom

            processNotificationMessage(notification);
        }
        protected static void OnDeviceNotificationMessageCustom(MTCMSNotificationMessage notification)
        {
            sendToDisplay("[Device Notification]");

            processNotificationMessage(notification);
        }
        private static void updateState(MTConnectionState state)
        {
            mConnectionState = state;

            try
            {
                switch (state)
                {
                    case MTConnectionState.Connecting:
                        updateControlStates(false);
                        //OutputTextBox.BackColor = Brushes.Gray;
                        kioskLog.SrushtyLog_oDynamo("oDynamo Connecting ******");
                        displayDeviceInformation();
                        break;
                    case MTConnectionState.Connected:
                        resetReceivingBigBlockData();
                        updateControlStates(true);                        
                        //OutputTextBox.Background = Brushes.White;
                        sendToDisplay("[Connected]");
                        //clearMessage();
                        //clearMessage2();
                        JanuityKiosk.StaticUI("label37", "Port Detected");// Status Set
                        kioskLog.SrushtyLog_oDynamo("oDynamo Connected ******");
                        if (isAutoSubscriptionEnabled())
                        {
                            setupSubscription(true);
                        }
                        break;
                    case MTConnectionState.Disconnecting:
                        updateControlStates(false);
                        //OutputTextBox.Background = Brushes.Gray;
                        sendToDisplay("[Disconnecting....]");
                        kioskLog.SrushtyLog_oDynamo("oDynamo Disconnecting ******");
                        break;
                    case MTConnectionState.Disconnected:
                        updateControlStates(false);
                        //OutputTextBox.Background = Brushes.Gray;
                        sendToDisplay("[Disconnected]");
                        kioskLog.SrushtyLog_oDynamo("oDynamo Disconnected ******");
                        JanuityKiosk.StaticUI("label37", "Port Disconnected");
                        break;
                }

            }
            catch (Exception)
            {
            }

        }

        protected static void updateControlStates(bool connected)
        {

        }

        protected void clearDeviceList()
        {
            try
            {
                // DeviceAddressCB.Items.Clear();
            }
            catch (Exception)
            {
            }
        }

        protected static void updateDeviceList(List<MTDeviceInformation> deviceList)
        {
            try
            {
                // DeviceAddressCB.Items.Clear();

                if (deviceList.Count > 0)
                {
                    foreach (var device in deviceList)
                    {
                        //DeviceAddressCB.Items.Add(device);
                    }

                    // DeviceAddressCB.Visible = true;

                    // DeviceAddressCB.SelectedIndex = 0;
                }

                // DeviceAddressCB.Enabled = true;
            }
            catch (Exception)
            {
            }

        }

        private static bool isAutoSubscriptionEnabled()
        {
            return (mSubscriptionOption == 0);
        }

        private static bool isAutoMSRRequestEnabled()
        {
            return (mMSRRequestOption == 0);
        }

        private static void setupSubscription(bool subscribe)
        {
            sendToDisplay("[Subscribe] = " + subscribe);

            byte[] dataBytes = new byte[2];
            dataBytes[0] = subscribe ? (byte)1 : (byte)2;
            dataBytes[1] = 0;

            MTCMSRequestMessage request = new MTCMSRequestMessage(ApplicationID.GENERAL_COMMAND, GeneralCommandID.MESSAGE_SUBSCRIPTION, DataTypeTag.PRIMITIVE, dataBytes);

            sendToDevice(request);
        }

        private static void requestMSRData()
        {
            sendToDisplay("[Send MSR Card Data Request]");

            MTCMSRequestMessage request = new MTCMSRequestMessage(ApplicationID.MSR, MSRCommandID.MSR_DATA, DataTypeTag.PRIMITIVE, null);

            sendToDevice(request);
        }

        private static void requestPAN()
        {
            sendToDisplay("[Send PAN Request]");

            MTCMSRequestMessage request = new MTCMSRequestMessage(ApplicationID.PAN, PANCommandID.REQUEST_PAN, DataTypeTag.PRIMITIVE, null);

            sendToDevice(request);
        }

        private static void sendPendingRequests()
        {
            if (mPendingResponse)
                return;

            mPendingResponse = false;

            if (mRequestList.Count > 0)
            {
                MTCMSRequestMessage request = mRequestList[0];
                mRequestList.RemoveAt(0);

                mPendingResponse = true;
                sendToDevice(request);
            }
        }

        private static void processResponseMessage(MTCMSResponseMessage response)
        {
            int applicationID = response.getApplicationID();
            int commandID = response.getCommandID();
            int resultCode = response.getResultCode();
            byte[] data = response.getData();

            //sendToDisplay("Response received for ApplicationID=0x" + getHexString(applicationID) + ", CommandID=0x" + getHexString(commandID) + ", ResultCode=0x" + getHexString(resultCode) + ", Data=[" + getHexString(data) + "]");

            switch (applicationID)
            {
                case ApplicationID.DEVICE_INFORMATION:
                    processDeviceInfoResponse(commandID, resultCode, data);
                    break;
                case ApplicationID.DEVICE_CONFIGURATION:
                    processDeviceConfigurationResponse(commandID, resultCode, data);
                    break;
                case ApplicationID.GENERAL_COMMAND:
                    processGeneralResponse(commandID, resultCode, data);
                    break;
                case ApplicationID.MSR:
                    processMSRResponse(commandID, resultCode, data);
                    break;
                case ApplicationID.PAN:
                    processPANResponse(commandID, resultCode, data);
                    break;
                case ApplicationID.EMV_L2:
                    processEMVL2Response(commandID, resultCode, data);
                    break;
                case ApplicationID.PIN_PAD:
                    processPINPADResponse(commandID, resultCode, data);
                    break;
            }
        }

        private static void processDeviceInfoResponse(int commandID, int resultCode, byte[] data)
        {
            sendToDisplay("Device Info Application Response CommandID=0x" + getHexString(commandID) + ", ResultCode=0x" + getHexString(resultCode) + ", Data=[" + getHexString(data) + "]\n");

            switch (commandID)
            {
                case DeviceInfoCommandID.PRODUCT_NAME:
                    string productName = System.Text.Encoding.UTF8.GetString(data);
                    sendToDisplay("Product Name: " + productName);
                    break;
                case DeviceInfoCommandID.FIRMWARE_NUMBER:
                    string fwInfo = System.Text.Encoding.UTF8.GetString(data);
                    sendToDisplay("Firmware Number: " + fwInfo);
                    break;
                case DeviceInfoCommandID.BUILD_INFO:
                    string buildInfo = System.Text.Encoding.UTF8.GetString(data);
                    sendToDisplay("Build Info: " + buildInfo);
                    break;
                case DeviceInfoCommandID.NETWORK_INFO:
                    if (data.Length > 1)
                    {
                        byte infoType = data[0];
                        int infoLen = data.Length - 1;
                        if (infoLen > 0)
                        {
                            byte[] infoBytes = new byte[infoLen];
                            Array.Copy(data, 1, infoBytes, 0, infoLen);
                            string infoString = System.Text.Encoding.UTF8.GetString(infoBytes);

                            switch (infoType)
                            {
                                case 0:
                                    sendToDisplay("MAC Address: " + infoString);
                                    break;
                                case 1:
                                    sendToDisplay("IP Address: " + infoString);
                                    break;
                            }
                        }
                    }
                    break;
                case DeviceInfoCommandID.MAIN_SW_VERSION:
                    string commAppInfo = System.Text.Encoding.UTF8.GetString(data);
                    sendToDisplay("Main App Version: " + commAppInfo);
                    break;
                case DeviceInfoCommandID.EMV_L2_VERSION:
                    string emvL2Info = System.Text.Encoding.UTF8.GetString(data);
                    sendToDisplay("EMV L2 Version: " + emvL2Info);
                    break;
            }
        }

        private static void processGeneralResponse(int commandID, int resultCode, byte[] data)
        {
            //sendToDisplay("General Application Response CommandID=0x" + getHexString(commandID) + ", ResultCode=0x" + getHexString(resultCode) + ", Data=[" + getHexString(data) + "]\n");

            switch (commandID)
            {
                case GeneralCommandID.DEVICE_STATUS:
                    processDeviceStatusResponse(data);
                    break;
                case GeneralCommandID.SEND_BIG_BLOCK_DATA:
                    processSendBigBlockDataResponse(resultCode);
                    break;
            }
        }

        private static void processDeviceStatusResponse(byte[] data)
        {
            //            sendToDisplay("Device Status=[" + getHexString(data) + "]\n");

            showDeviceStatus(data);
        }

        private static void processSendBigBlockDataResponse(int resultCode)
        {
            if (mSendingBigBlockData)
            {
                if (resultCode == 0)
                {
                    uploadNextDataBlock();
                }
                else
                {
                    mSendingBigBlockData = false;
                }
            }
        }

        private static void processMSRResponse(int commandID, int resultCode, byte[] data)
        {
            //sendToDisplay("MSR Application Response CommandID=0x" + getHexString(commandID) + ", ResultCode=0x" + getHexString(resultCode) + ", Data=[" + getHexString(data) + "]\n");

            switch (commandID)
            {
                case MSRCommandID.MSR_DATA:
                    sendToDisplay("[MSR Card Data Received]");
                    processMSRData(resultCode, data);
                    break;
            }
        }

        private static void processPANResponse(int commandID, int resultCode, byte[] data)
        {
            //sendToDisplay("PAN Response CommandID=0x" + getHexString(commandID) + ", ResultCode=0x" + getHexString(resultCode) + ", Data=[" + getHexString(data) + "]\n");

            switch (commandID)
            {
                case PANCommandID.REQUEST_PAN:
                    processRequestPANResponse(data);
                    break;
            }
        }

        private static void processDeviceConfigurationResponse(int commandID, int resultCode, byte[] data)
        {
            //sendToDisplay("Device Configuration Response CommandID=0x" + getHexString(commandID) + ", ResultCode=0x" + getHexString(resultCode) + ", Data=[" + getHexString(data) + "]\n");

            if (commandID == DeviceConfigCommandID.CARD_LATCH_CONTROL)
            {
                if (mLatchValue == 0)
                {
                    sendToDisplay("[Latched]\n");
                }
                else
                {
                    sendToDisplay("[Unlatched]\n");
                }
            }
        }

        private static void processEMVL2Response(int commandID, int resultCode, byte[] data)
        {
            sendToDisplay("EMV L2 Application Response CommandID=0x" + getHexString(commandID) + ", ResultCode=0x" + getHexString(resultCode) + ", Data=[" + getHexString(data) + "]\n");
        }

        private static void processPINPADResponse(int commandID, int resultCode, byte[] data)
        {
            sendToDisplay("PINPAD Response CommandID=0x" + getHexString(commandID) + ", ResultCode=0x" + getHexString(resultCode) + ", Data=[" + getHexString(data) + "]\n");
        }

        private static void processRequestPANResponse(byte[] data)
        {
            sendToDisplay("[Response for Request PAN]");
            sendToDisplay(MTParser.getHexString(data));

            if (mRequestedPANForPINBlock)
            {
                mRequestedPANForPINBlock = false;
                getPINCVM();
            }
        }

        private static void processNotificationMessage(MTCMSNotificationMessage notification)
        {
            int applicationID = notification.getApplicationID();
            int commandID = notification.getCommandID();
            int resultCode = notification.getResultCode();
            byte[] data = notification.getData();

            //sendToDisplay("Notification received for ApplicationID=0x" + getHexString(applicationID) + ", CommandID=0x" + getHexString(commandID) + ", ResultCode=0x" + getHexString(resultCode) + ", Data=[" + getHexString(data) + "]");

            switch (applicationID)
            {
                case ApplicationID.GENERAL_COMMAND:
                    processGeneralNotification(commandID, resultCode, data);
                    break;
                case ApplicationID.MSR:
                    processMSRNotification(commandID, resultCode, data);
                    break;
                case ApplicationID.EMV_L2:
                    processEMVL2Notification(commandID, resultCode, data);
                    break;
                case ApplicationID.PIN_PAD:
                    processPINPADNotification(commandID, resultCode, data);
                    break;
            }
        }

        private static void processGeneralNotification(int commandID, int resultCode, byte[] data)
        {
            sendToDisplay("General Application Notification CommandID=0x" + getHexString(commandID) + ", ResultCode=0x" + getHexString(resultCode) + ", Data=[" + getHexString(data) + "]");

            switch (commandID)
            {
                case GeneralCommandID.DEVICE_STATUS:
                    processDeviceStatus(data);
                    break;
                case GeneralCommandID.SEND_BIG_BLOCK_DATA:
                    processBigBlockDataNotification(data);
                    break;
                case GeneralCommandID.CARD_STATUS:
                    processCardStatus(data);
                    break;
            }
        }

        private static void processMSRNotification(int commandID, int resultCode, byte[] data)
        {
            //sendToDisplay("MSR Application Notification CommandID=0x" + getHexString(commandID) + ", ResultCode=0x" + getHexString(resultCode) + ", Data=[" + getHexString(data) + "]");

            switch (commandID)
            {
                case MSRCommandID.MSR_CARD_DATA_AVAILABLE:
                    sendToDisplay("[MSR Card Data Available]");
                    sendToDisplay("Data =[ " + getHexString(data) + "]");
                    if (isAutoMSRRequestEnabled())
                    {
                        requestMSRData();
                    }
                    break;
                case MSRCommandID.MSR_DATA:
                    sendToDisplay("[MSR Card Data Received]");
                    processMSRData(resultCode, data);
                    break;
            }
        }

        private static void processEMVL2Notification(int commandID, int resultCode, byte[] data)
        {
            sendToDisplay("EMV L2 Application Notification CommandID=0x" + getHexString(commandID) + ", ResultCode=0x" + getHexString(resultCode) + ", Data=[" + getHexString(data) + "]\n");

            switch (commandID)
            {
                case EMVL2CommandID.EMV_L2_TRANSACTION_STATUS:
                    processTransactionStatus(data);
                    break;
                case EMVL2CommandID.EMV_L2_DISPLAY_MESSAGE_REQUEST:
                    processDisplayMessageRequest(data);
                    break;
                case EMVL2CommandID.EMV_L2_USER_SELECTION_REQUEST:
                    //processUserSelectionRequest(data);
                    break;
                case EMVL2CommandID.EMV_L2_ARQC_MESSAGE:
                    processARQC(data);
                    break;
                case EMVL2CommandID.EMV_L2_CANCEL_TRANSACTION:
                    processCancelTransaction(data);
                    break;
                case EMVL2CommandID.EMV_L2_TRANSACTION_RESULT:
                    processTransactionResult(data);
                    break;
                case EMVL2CommandID.EMV_L2_PIN_ENTRY_SHOW_PROMPT:
                    processPINEntryShowPrompt(data);
                    break;
                case EMVL2CommandID.EMV_L2_PIN_CVM_REQUEST:
                    processPINCVMRequest();
                    break;
            }
        }

        private static void processPINPADNotification(int commandID, int resultCode, byte[] data)
        {
            //sendToDisplay("PINPAD Notification CommandID=0x" + getHexString(commandID) + ", ResultCode=0x" + getHexString(resultCode) + ", Data=[" + getHexString(data) + "]\n");
        }

        private static void processTransactionStatus(byte[] data)
        {
            sendToDisplay("[Transaction Status]");
            sendToDisplay(MTParser.getHexString(data));
        }

        private static void processDisplayMessageRequest(byte[] data)
        {
            sendToDisplay("[Display Message Request]");

            String message = "";

            if ((data != null) && (data.Length > 0))
            {
                message = System.Text.Encoding.UTF8.GetString(data);
            }

            sendToDisplay(message);
            //displayMessage(message);
        }
              

        protected static List<string> getSelectionList(byte[] data, int offset)
        {
            List<string> selectionList = new List<string>();

            if (data != null)
            {
                int dataLen = data.Length;

                if (dataLen >= offset)
                {
                    int start = offset;

                    for (int i = offset; i < dataLen; i++)
                    {
                        if (data[i] == 0x00)
                        {
                            int len = i - start;

                            if (len >= 0)
                            {
                                byte[] selectionBytes = new byte[len];

                                Array.Copy(data, start, selectionBytes, 0, len);

                                string selectionString = System.Text.Encoding.UTF8.GetString(selectionBytes);

                                selectionList.Add(selectionString);
                            }

                            start = i + 1;
                        }
                    }
                }
            }

            return selectionList;
        }

        protected static void displayUserSelections(string title, List<string> selectionList, long timeout)
        {


        }

        protected static void displayPINWindow(long timeout)
        {


        }

        public static void setUserSelectionResult(byte status, byte selection)
        {
            sendToDisplay("[Sending Selection Result] Status=" + status + " Selection=" + selection);

            byte[] data = new byte[2];

            data[0] = status;
            data[1] = selection;

            MTCMSRequestMessage request = new MTCMSRequestMessage(ApplicationID.EMV_L2, EMVL2CommandID.EMV_L2_USER_SELECTION_RESULT, DataTypeTag.PRIMITIVE, data);

            sendToDevice(request);
        }

        private static void processARQC(byte[] data)
        {
            sendToDisplay("[ARQC Received]");

            sendToDisplay(MTParser.getHexString(data));

            Console.WriteLine("processARQC Data************** " + MTParser.getHexString(data));

            List<Dictionary<String, String>> parsedTLVList = MTParser.parseEMVData(data, true, "");

            if (parsedTLVList != null)
            {
                String deviceSNString = MTParser.getTagValue(parsedTLVList, "DFDF25");
                byte[] deviceSN = MTParser.getByteArrayFromHexString(deviceSNString);

                sendToDisplay("SN Bytes=" + deviceSNString);

                byte[] response = null;

                byte[] arc = null;

                if (m_emvARCType == 0)
                {
                    arc = ApprovedARC;
                }
                else if (m_emvARCType == 1)
                {
                    arc = DeclinedARC;
                }
                else if (m_emvARCType == 2)
                {
                    sendToDisplay("Quick Chip Mode (not sending Acquirer Response)");
                    return;
                }

                String macKSNString = MTParser.getTagValue(parsedTLVList, "DFDF54");
                byte[] macKSN = MTParser.getByteArrayFromHexString(macKSNString);

                String macEncryptionTypeString = MTParser.getTagValue(parsedTLVList, "DFDF55");
                byte[] macEncryptionType = MTParser.getByteArrayFromHexString(macEncryptionTypeString);

                Console.WriteLine("processARQC macKSNString************** " + macKSNString);
                Console.WriteLine("processARQC macEncryptionTypeString************** " + macEncryptionTypeString);
                Console.WriteLine("processARQC deviceSNString************** " + deviceSNString);

                String NumberOfPaddedBytes = MTParser.getTagValue(parsedTLVList, "DFDF58");
                Console.WriteLine("processARQC NumberOfPaddedBytes************** " + NumberOfPaddedBytes);
                response = buildAcquirerResponse(macKSN, macEncryptionType, deviceSN, arc, false);

                setAcquirerResponse(response);
            }
        }

        /// <summary>
        /// ProcessDataResponseDto process the input request and returns the response
        /// </summary>
        /// <param name="processDataRequestDto"></param>
        /// <returns></returns>

        protected static byte[] buildAcquirerResponse(byte[] macKSN, byte[] macEncryptionType, byte[] deviceSN, byte[] arc, bool useLengthHeader)
        {
            byte[] response = null;

            int lenMACKSN = 0;
            int lenMACEncryptionType = 0;
            int lenSN = 0;

            if (macKSN != null)
            {
                lenMACKSN = macKSN.Length;
            }

            if (macEncryptionType != null)
            {
                lenMACEncryptionType = macEncryptionType.Length;
            }

            if (deviceSN != null)
            {
                lenSN = deviceSN.Length;
            }

            byte[] macKSNTag = new byte[] { (byte)0xDF, (byte)0xDF, 0x54, (byte)lenMACKSN };
            byte[] macEncryptionTypeTag = new byte[] { (byte)0xDF, (byte)0xDF, 0x55, (byte)lenMACEncryptionType };
            byte[] snTag = new byte[] { (byte)0xDF, (byte)0xDF, 0x25, (byte)lenSN };
            byte[] container = new byte[] { (byte)0xFA, 0x06, 0x70, 0x04 };

            int lenTLV = 2 + macKSNTag.Length + lenMACKSN + macEncryptionTypeTag.Length + lenMACEncryptionType + snTag.Length + lenSN + container.Length + arc.Length;

            if (useLengthHeader)
                lenTLV += 2;

            int lenPadding = 0;

            if ((lenTLV % 8) > 0)
            {
                lenPadding = (8 - lenTLV % 8);
            }

            int lenData = lenTLV + lenPadding + 4;

            response = new byte[lenData];

            int i = 0;

            if (useLengthHeader)
            {
                response[i++] = (byte)(((lenData - 2) >> 8) & 0xFF);
                response[i++] = (byte)((lenData - 2) & 0xFF);
            }

            response[i++] = (byte)0xF9;
            response[i++] = (byte)(lenTLV - 2);
            Array.Copy(macKSNTag, 0, response, i, macKSNTag.Length);
            i += macKSNTag.Length;
            Array.Copy(macKSN, 0, response, i, macKSN.Length);
            i += macKSN.Length;
            Array.Copy(macEncryptionTypeTag, 0, response, i, macEncryptionTypeTag.Length);
            i += macEncryptionTypeTag.Length;
            Array.Copy(macEncryptionType, 0, response, i, macEncryptionType.Length);
            i += macEncryptionType.Length;
            Array.Copy(snTag, 0, response, i, snTag.Length);
            i += snTag.Length;
            Array.Copy(deviceSN, 0, response, i, deviceSN.Length);
            i += deviceSN.Length;
            Array.Copy(container, 0, response, i, container.Length);
            i += container.Length;

            if (arc != null)
            {
                Array.Copy(arc, 0, response, i, arc.Length);
            }

            return response;
        }

        private static void setAcquirerResponse(byte[] response)
        {
            sendToDisplay("[Sending Acquirer Response]\n" + MTParser.getHexString(response));

            MTCMSRequestMessage request = new MTCMSRequestMessage(ApplicationID.EMV_L2, EMVL2CommandID.EMV_L2_ACQUIRER_RESPONSE, DataTypeTag.PRIMITIVE, response);

            if (response.Length > getBigBlockDataSize())
            {
                sendBigBlocksToDevice(request);
            }
            else
            {
                sendToDevice(request);
            }

        }

        private static void processCancelTransaction(byte[] data)
        {
            sendToDisplay("[Cancel Transaction Status]");
            sendToDisplay(MTParser.getHexString(data));
        }

        private static void processTransactionResult(byte[] data)
        {
            sendToDisplay("[Transaction Result]");

            sendToDisplay(MTParser.getHexString(data));

            if (data != null)
            {
                if (data.Length > 0)
                {
                    bool signatureRequired = (data[0] != 0);

                    int lenBatchData = data.Length - 3;
                    if (lenBatchData > 0)
                    {
                        byte[] batchData = new byte[lenBatchData];

                        Array.Copy(data, 3, batchData, 0, lenBatchData);

                        sendToDisplay("(Parsed Batch Data)");

                        List<Dictionary<String, String>> parsedTLVList = MTParser.parseTLVData(batchData);

                        displayParsedTLV(parsedTLVList);

                        bool approved = false;

                        String statusString = MTParser.getTagValue(parsedTLVList, "dfdf1a");
                        byte[] statusValue = MTParser.getByteArrayFromHexString(statusString);


                        if (statusValue != null)
                        {
                            if (statusValue.Length > 0)
                            {
                                if (statusValue[0] == 0)
                                {
                                    approved = true;
                                }
                            }
                        }

                        if (approved)
                        {
                            if (signatureRequired)
                            {
                                //displayMessage2("( Signature Required )");
                            }
                            else
                            {
                                //displayMessage2("( No Signature Required )");
                            }
                        }
                    }
                }
            }
        }

        private static void processPINEntryShowPrompt(byte[] data)
        {
            sendToDisplay("[PIN Entry Show Prompt]");

            sendToDisplay(MTParser.getHexString(data));

            if (data != null)
            {
                if (data.Length > 0)
                {
                    byte promptID = data[0];

                    switch (promptID)
                    {
                        case 0x09:
                            //displayMessage("[ENTER PIN]");
                            break;
                    }

                    //mPINEntryDisplay = "";
                    //clearMessage2();
                }
            }
        }

        private static void processPINCVMRequest()
        {
            sendToDisplay("[PIN CVM Request]");

            mRequestedPANForPINBlock = true;
            requestPAN();
        }

        private static void getPINCVM()
        {
            sendToDisplay("[Get PIN CVM]");

            displayPINWindow(30000);
        }

        private static void processDeviceStatus(byte[] data)
        {
            sendToDisplay("Device Status=[" + getHexString(data) + "]\n");
        }

        private static void processCardStatus(byte[] data)
        {
            sendToDisplay("Card Status=[" + getHexString(data) + "]\n");

            if (data != null)
            {
                int len = data.Length;

                if (len >= 2)
                {
                    byte present = data[0];
                    byte type = data[1];

                    sendToDisplay("Card Present=[" + present + "]\n");
                    sendToDisplay("Card Type=[" + type + "]\n");
                }
            }
        }

        private static void resetReceivingBigBlockData()
        {
            mReceivingBigBlockDataTotalLength = 0;
            mReceivingBigBlockDataReceivedLength = 0;
            mReceivingBigBlockDataLastPacketID = 0;
            mRecevingBigBlockData = null;
        }
        dynamic thisclass = new oDynanoPayment();
        private static void processBigBlockDataNotification(byte[] data)
        {
            //sendToDisplay("Big Block Data=[" + getHexString(data) + "]\n");

            if (data != null)
            {
                int len = data.Length;

                if (len >= 2)
                {
                    int packetID = (int)(data[1] << 8) + (int)data[0];

                    if (packetID == 0)
                    {
                        mReceivingBigBlockDataLastPacketID = 0;
                        mReceivingBigBlockDataReceivedLength = 0;

                        if (len >= 4)
                        {
                            int lenData = (int)(data[3] << 8) + (int)data[2];

                            if (lenData > 0)
                            {
                                if (len >= (lenData + 4))
                                {
                                    byte[] dataBytes = new byte[lenData];
                                    Array.Copy(data, 4, dataBytes, 0, lenData);

                                    int totalLen = 0;

                                    for (int i = 0; i < lenData; i++)
                                    {
                                        totalLen += (((int)(dataBytes[i] & 0xFF)) << (i * 8));
                                    }

                                    if (totalLen > 0)
                                    {
                                        mRecevingBigBlockData = new byte[totalLen];
                                        Array.Clear(mRecevingBigBlockData, 0, totalLen);

                                        mReceivingBigBlockDataTotalLength = totalLen;
                                    }
                                }
                                else
                                {
                                    // Insufficient data available
                                }
                            }
                        }
                    }
                    else
                    {
                        if (packetID == (mReceivingBigBlockDataLastPacketID + 1))
                        {
                            mReceivingBigBlockDataLastPacketID = packetID;

                            if (len >= 4)
                            {
                                int lenData = (int)(data[3] << 8) + (int)data[2];

                                if (lenData > 0)
                                {
                                    if (len >= (lenData + 4))
                                    {
                                        Array.Copy(data, 4, mRecevingBigBlockData, mReceivingBigBlockDataReceivedLength, lenData);

                                        mReceivingBigBlockDataReceivedLength += lenData;

                                        if (mReceivingBigBlockDataReceivedLength >= mReceivingBigBlockDataTotalLength)
                                        {
                                            MTCMSMessage message = new MTCMSMessage(mRecevingBigBlockData);

                                            if (message.getMessageType() == 2)
                                            {
                                                MTCMSResponseMessage response = new MTCMSResponseMessage(message.getApplicationID(), message.getCommandID(), message.getResultCode(), message.getDataTag(), message.getData());

                                                // OnDeviceResponseMessage(this, response);
                                                OnDeviceResponseMessageCustom(response);
                                                kioskLog.SrushtyLog_oDynamo("oDynamo OnDeviceResponseMessage ******");
                                            }
                                            else if (message.getMessageType() == 3)
                                            {
                                                MTCMSNotificationMessage notification = new MTCMSNotificationMessage(message.getApplicationID(), message.getCommandID(), message.getResultCode(), message.getDataTag(), message.getData());
                                                kioskLog.SrushtyLog_oDynamo("oDynamo OnDeviceNotificationMessage ******");

                                                // OnDeviceNotificationMessage(this, notification);
                                                OnDeviceNotificationMessageCustom(notification);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Insufficient data available
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Unexpected packetID received
                        }
                    }
                }
            }
        }

        private static void processMSRData(int resultCode, byte[] data)
        {
            sendToDisplay("MSR Data=[" + getHexString(data) + "]\n");

            showMSRData(data);
        }

        private static void displayParsedTLV(List<Dictionary<string, string>> parsedTLVList)
        {
            if (parsedTLVList != null)
            {
                foreach (Dictionary<String, String> map in parsedTLVList)
                {
                    string tagString;
                    string valueString;

                    if (map.TryGetValue("tag", out tagString))
                    {
                        if (map.TryGetValue("value", out valueString))
                        {
                            sendToDisplay("  " + tagString + "=" + valueString);
                        }
                    }
                }
            }
        }

        private static void showDeviceStatus(byte[] data)
        {
            List<Dictionary<String, String>> parsedTLVList = MTParser.parseTLVData(data);

            if (parsedTLVList != null)
            {
                string deviceStatus = "";

                deviceStatus += string.Format("Device.State={0}\n", MTParser.getTagValue(parsedTLVList, "DF50"));
                deviceStatus += string.Format("Device.Status={0}\n", MTParser.getTagValue(parsedTLVList, "DF51"));
                deviceStatus += string.Format("Device.Centificate+Key.Status={0}\n", MTParser.getTagValue(parsedTLVList, "DF52"));

                sendToDisplay(deviceStatus);
            }
        }

        private static void showMSRData(byte[] data)
        {
            sendToDisplay("MSR Data:\n");

            List<Dictionary<String, String>> parsedTLVList = MTParser.parseTLVData(data);

            if (parsedTLVList != null)
            {
                string msrData = "";

                msrData += string.Format("Track1.Masked={0}\n", MTParser.getTagTextValue(parsedTLVList, "DFDF31"));
                msrData += string.Format("Track2.Masked={0}\n", MTParser.getTagTextValue(parsedTLVList, "DFDF33"));
                msrData += string.Format("Track3.Masked={0}\n", MTParser.getTagTextValue(parsedTLVList, "DFDF35"));

                msrData += string.Format("Track1.Status={0}\n", MTParser.getTagValue(parsedTLVList, "DFDF36"));
                msrData += string.Format("Track2.Status={0}\n", MTParser.getTagValue(parsedTLVList, "DFDF38"));
                msrData += string.Format("Track3.Status={0}\n", MTParser.getTagValue(parsedTLVList, "DFDF3A"));

                msrData += string.Format("MagnePrint.Status={0}\n", MTParser.getTagValue(parsedTLVList, "DFDF43"));

                msrData += string.Format("KSN={0}\n", MTParser.getTagValue(parsedTLVList, "DFDF56"));
                msrData += string.Format("Card.EncodeType={0}\n", MTParser.getTagValue(parsedTLVList, "DFDF4F"));
                msrData += string.Format("EncryptionType={0}\n", MTParser.getTagValue(parsedTLVList, "DFDF51"));

                String encryptedDataHexString = MTParser.getTagValue(parsedTLVList, "DFDF59");
                msrData += string.Format("EncryptedData={0}\n", encryptedDataHexString);

                /*
                                byte[] encryptedDataBytes = MTParser.getByteArrayFromHexString(encryptedDataHexString);

                                List<Dictionary<String, String>> encryptedDataTLVList = MTParser.parseTLVData(encryptedDataBytes);

                                if (encryptedDataTLVList != null)
                                {
                                    String encryptedTrackHexString = MTParser.getTagValue(encryptedDataTLVList, "FA");

                                    byte[] encryptedTrackBytes = MTParser.getByteArrayFromHexString(encryptedTrackHexString);

                                    List<Dictionary<String, String>> encryptedTrackTLVList = MTParser.parseTLVData(encryptedTrackBytes);

                                    if (encryptedTrackTLVList != null)
                                    {
                                        msrData += string.Format("Track1.Encrypted={0}\n", MTParser.getTagValue(encryptedTrackTLVList, "DF41"));
                                        msrData += string.Format("Track2.Encrypted={0}\n", MTParser.getTagValue(encryptedTrackTLVList, "DF42"));
                                        msrData += string.Format("Track3.Encrypted={0}\n", MTParser.getTagValue(encryptedTrackTLVList, "DF43"));
                                        msrData += string.Format("MagnePrint={0}\n", MTParser.getTagValue(encryptedTrackTLVList, "DF44"));
                                    }
                                }
                */

                sendToDisplay(msrData);
            }
        }

        private static int sendToDevice(MTCMSMessage message)
        {
            byte[] data = message.getMessageBytes();

            string dataHexString = getHexString(data);

            sendToDisplay("Sending to device: [" + dataHexString + "]");

            int result = mDevice.sendMTCMSMessage(message);

            sendToDisplay("Send result=" + result);

            return result;
        }

        private static int sendBigBlocksToDevice(MTCMSMessage message)
        {
            if (mSendingBigBlockData)
            {
                return -1;
            }

            mBigBlockData = message.getMessageBytes();

            mBigBlockByteCount = 0;
            mBigBlockPacketCount = 0;

            sendBigBlockDataLength();

            return 0;
        }

        private static int getBigBlockDataLength()
        {
            int dataLen = 0;

            if (mBigBlockData != null)
            {
                dataLen = mBigBlockData.Length; ;
            }

            return dataLen;
        }

        private static void sendBigBlockDataLength()
        {
            int totalDataLen = getBigBlockDataLength();

            if (totalDataLen > 0)
            {
                mSendingBigBlockData = true;

                byte[] packetIDBytes = getPacketIDByteArray(0);
                byte[] valueLenBytes = getLengthByteArray(2, 4);
                byte[] valueBytes = getLengthByteArray(4, totalDataLen);

                byte[] dataBytes = getArrayBytes(packetIDBytes, valueLenBytes, valueBytes);

                mBigBlockPacketCount = 1;

                sendBigBlockDataCommand(dataBytes);
            }
        }

        private static void sendBigBlockDataCommand(byte[] data)
        {
            MTCMSRequestMessage request = new MTCMSRequestMessage(ApplicationID.GENERAL_COMMAND, GeneralCommandID.SEND_BIG_BLOCK_DATA, DataTypeTag.PRIMITIVE, data);

            sendToDevice(request);
        }

        private static byte[] buildBigBlockDataBytes(int packetID, byte[] value)
        {
            byte[] dataBytes = null;

            if (value != null)
            {
                byte[] packetIDBytes = getPacketIDByteArray(packetID);
                byte[] valueLenBytes = getLengthByteArray(2, value.Length);

                dataBytes = getArrayBytes(packetIDBytes, valueLenBytes, value);
            }

            return dataBytes;
        }

        private static int getBigBlockDataSize()
        {
            int blockSize = BIG_BLOCK_DATA_SIZE;

            if (mConnectionType == MTConnectionType.USB)
            {
                blockSize = BIG_BLOCK_DATA_SMALL_SIZE;
            }

            return blockSize;
        }

        private static bool uploadNextDataBlock()
        {
            int totalDataLen = getBigBlockDataLength();

            if (mBigBlockByteCount < totalDataLen)
            {
                int dataLen = totalDataLen - mBigBlockByteCount;

                int blockSize = getBigBlockDataSize();

                if (dataLen > blockSize)
                {
                    dataLen = blockSize;
                }

                byte[] data = new byte[dataLen];

                if (data != null)
                {
                    Array.Copy(mBigBlockData, mBigBlockByteCount, data, 0, dataLen);

                    byte[] dataBytes = buildBigBlockDataBytes(mBigBlockPacketCount, data);

                    mBigBlockPacketCount++;

                    mBigBlockByteCount += dataLen;

                    sendBigBlockDataCommand(dataBytes);
                }
            }
            else
            {
                mSendingBigBlockData = false;

                return false;
            }

            return true;
        }

        private static byte[] getLengthByteArray(int lenBytesNeeded, int len)
        {
            byte[] lengthArray;

            lengthArray = new byte[lenBytesNeeded];
            Array.Clear(lengthArray, 0, lengthArray.Length);
            if (len > 0)
            {
                for (int i = 0; i < lenBytesNeeded; i++)
                {
                    lengthArray[i] = (byte)((len >> (i * 8)) & (0xFF));
                }
            }

            return lengthArray;
        }

        private static byte[] getPacketIDByteArray(int id)
        {
            return getLengthByteArray(2, id);
        }

        private static byte[] getArrayBytes(byte[] array1, byte[] array2, byte[] array3)
        {
            int len = 0;

            if (array1 != null)
            {
                len += array1.Length;
            }

            if (array2 != null)
            {
                len += array2.Length;
            }

            if (array3 != null)
            {
                len += array3.Length;
            }

            byte[] valueBytes = null;

            if (len > 0)
            {
                int i = 0;

                valueBytes = new byte[len];

                if (array1 != null)
                {
                    Array.Copy(array1, 0, valueBytes, i, array1.Length);
                    i += array1.Length;
                }

                if (array2 != null)
                {
                    Array.Copy(array2, 0, valueBytes, i, array2.Length);
                    i += array2.Length;
                }

                if (array3 != null)
                {
                    Array.Copy(array3, 0, valueBytes, i, array3.Length);
                    i += array3.Length;
                }
            }

            return valueBytes;
        }

        public static string getHexString(int value)
        {
            byte byteValue = (byte)(value & 0xFF);

            return byteValue.ToString("X2");
        }

        public static string getHexString(byte[] data)
        {
            return MTParser.getHexString(data);
        }

        public static byte[] getByteArrayFromHexString(string str)
        {
            return MTParser.getByteArrayFromHexString(str);
        }



    }
}
