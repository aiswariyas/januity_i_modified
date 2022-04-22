using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using System.Windows.Forms;
using JanuityUI;

namespace SuperWebSocket.PubSubProtocol
{
    public class JanuityWebSocketServer : WebSocketServer
    {
        JanuityUI.JanuityKiosk connection_object;
        JanuityUI.Modules.SpO2 SpO2_Connect;
        JanuityUI.Modules.FingerPrint Finger_Connect;
        JanuityUI.Modules.WeightScale WeightScale_Connect;
        JanuityUI.Modules.BloodPressure Bloodpressure_Connect;
        JanuityUI.Modules.Temperature Temperature_Connect;
        JanuityUI.Modules.PortDetection PortDetection_Connect;
        JanuityUI.Modules.LightLed Light_Connect;
        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public Dictionary<string, List<SubscriberConstrains>> Subscriptions { get; set; }
        public WebSocketSession sessionofapp;
        public JanuityWebSocketServer(JanuityUI.JanuityKiosk newobj)
        {
            connection_object = newobj;
            SpO2_Connect = new JanuityUI.Modules.SpO2();
            WeightScale_Connect = new JanuityUI.Modules.WeightScale();
            Bloodpressure_Connect = new JanuityUI.Modules.BloodPressure();
            Temperature_Connect = new JanuityUI.Modules.Temperature();
            Finger_Connect = new JanuityUI.Modules.FingerPrint();
            PortDetection_Connect = new JanuityUI.Modules.PortDetection();
            Light_Connect = new JanuityUI.Modules.LightLed();


            this.Subscriptions = new Dictionary<string, List<SubscriberConstrains>>();
            this.SessionClosed += CPWebSocketServer_SessionClosed;
            this.NewMessageReceived += CPWebSocketServer_NewMessageReceived;
        }


        public void SetSupportedTopics(List<string> topics)
        {
            foreach (var topic in topics)
            {
               this.Subscriptions.Add(topic, new List<SubscriberConstrains>());
            }

        }

        void CPWebSocketServer_NewMessageReceived(WebSocketSession session, string value)
        {
            
            if (value.Contains("StartBp"))
            {
               // MessageBox.Show("BP Started 1");
                sessionofapp = session;
                Bloodpressure_Connect.StartBP(session);
                Light_Connect.BP_LED();
            }
            else if (value.Contains("StopBp"))
            {
                sessionofapp = session;
                Bloodpressure_Connect.StopBP(session);
                Light_Connect.Stop_LED();
            }
            else if (value.Contains("GetPortStatus"))
            {
                sessionofapp = session;
                //PortDetection_Connect.PortStatus(session);
                //SpO2_Connect.getSpO2(session);
            }
            else if (value.Contains("GetWeight"))
            {
                
                sessionofapp = session;
                WeightScale_Connect.getweight(session);               
            }
            else if (value.Contains("GetSpO2Status"))
            {
                sessionofapp = session;
                SpO2_Connect.getSpO2Status(session);
            }
            else if (value.Contains("GetSpO2"))
            {
                sessionofapp = session;
                Light_Connect.SpO2_LED();
                SpO2_Connect.getSpO2(session);
            }
            else if (value.Contains("GetFingerPrint"))
            {
                sessionofapp = session;
                Light_Connect.FingerPrint_LED();
                Finger_Connect.StartcaptureFingerPrint(session);
            }else if (value.Contains("ShowWindow"))
            {
                JanuityKiosk.ShowUI();
            }
            else if (value.Contains("check_device_temp"))
            {
                sessionofapp = session;
                Temperature_Connect.Check_Temperature_Device(session);
            }
            else if (value.Contains("GetTemp"))
            {
                sessionofapp = session;
                
                Light_Connect.Temp_LED();
            }

            else if (value.Contains("check_weight_connect"))
            {
                sessionofapp = session;
                WeightScale_Connect.check_weight_connect(session);
            }

            else if (value.Contains("check_fingerprint_connect"))
            {
                sessionofapp = session;
                Finger_Connect.check_fingerprint_connect(session);
            }

            else if (value.Contains("check_bp_connect"))
            {
                sessionofapp = session;
                Bloodpressure_Connect.check_bp_connect(session);
            }
            else if (value.Contains("CloseTemp"))
            {
                Light_Connect.Stop_LED();
            }

            else
            {
                sessionofapp = session;
                //PortDetection_Connect.PortStatus(session);
                session.Send("WS Response: " + value + " V1.0.0");
            }
        }


        void CPWebSocketServer_SessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason value)
        {

        }


        public bool SubscribedToTopic(ClientRequest clientRequest, string clientId)
        {          
            try
            {
                if (this.Subscriptions.ContainsKey(clientRequest.Topic))
                {
                    var subscribers = (List<SubscriberConstrains>)this.Subscriptions[clientRequest.Topic];
                    subscribers.Add(new SubscriberConstrains(clientId, clientRequest.Constrains));

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            } 
            return true;
        }

        public bool UnsubribeFromTopic(ClientRequest clientRequest, string clientId)
        {
            return true;
        }
        
        public void Publich(ClientRequest clientRequest, string msg)
        {

        }
       
        public void SendToAll(List<string> subscribers, string msg)
        {
            try
            {
                foreach (var sub in subscribers)
                {  
                    this.GetAppSessionByID(sub).Send(msg);
                }
            }
            catch (Exception ex)
            {
               // log.Error("Error on SendToAll ", ex);
            }
        }
    }
    
    public class SubscriberConstrains
    {
        public string SubsciberId { get; set; }
        public List<Constrain> Constrains { get; set; }

        public SubscriberConstrains(string subsciberId, List<Constrain> constrains)
        {
            this.SubsciberId = subsciberId;
            this.Constrains = constrains;
        }
    }
}
