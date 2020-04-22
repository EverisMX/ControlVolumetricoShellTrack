using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Runtime;
using System.ComponentModel;
//using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Principal;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Security.Permissions;
using System.IO;
using Newtonsoft.Json;

namespace ControlVolumetricoShellWS.Implementation
{
    public class IC0SaleMethod
    {
        public string var_pinpad = "";
        public string var_timeout = "";
        //static string strTimeput = String.Empty;
        //static string strIp = String.Empty;
        //static string Param_pinpad = System.IO.File.ReadAllText(@"C:\HubblePOS\Logs\PARAM_PINPAD.txt");

        MX_LogsHPTPV.LogsTPVHP exec = new MX_LogsHPTPV.LogsTPVHP();
        #region DLL'S + INICIALIZACION DEL PINPAD
        static int seconds = 0;
        public const int PORT_SIZE = 7;
        public const int ADDRESS_SIZE = 13;
        public const int MAX_NAME_SIZE = 248;

        //PCL import DLL
        [DllImport("PCLService.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool startPclService();

        [DllImport("PCLService.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool stopPclService();

        [DllImport("PCLService.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool serverStatus([MarshalAs(UnmanagedType.LPStr)] StringBuilder result);

        [DllImport("PCLService.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool doUpdate(ref UInt32 result);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class tms_ssl_parameter_t
        {
            public uint nb_ssl_profile;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 240)]
            public string ssl_profiles;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
            public string current_ssl_profile;
        }

        [DllImport("PCLService.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool tmsReadParam(StringBuilder addr, uint addrSize, StringBuilder port, uint portSize, StringBuilder identifier, uint identifierSize, [In, Out] tms_ssl_parameter_t ssl_profiles, StringBuilder result);

        [DllImport("PCLService.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool tmsWriteParam(string addr, string port, string identifier, string ssl_profile, string result);

        [DllImport("PCLService.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool getFullSerialNumber(StringBuilder fullSN, UInt32 fullSNSize);

        [DllImport("PCLService.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int addDynamicBridge(int port, int direction);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class USB_COMPANION_INFO
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = PORT_SIZE)]
            public string szPort;             //COM
            public bool fActivated;			//  Device connected/in use
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_NAME_SIZE)]
            public string szName;             //COM
        }

        //Bluetooth
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class BLUETOOTH_COMPANION_INFO
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADDRESS_SIZE)]
            public string Address;             //COM
            public bool fActivated;			//  Device connected/in use
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_NAME_SIZE)]
            public string szName;             //COM
        }


        [DllImport("PclUtilities.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern int getUSBDevices([In, Out] USB_COMPANION_INFO pCompanions, ref int pdwSize, ref int pdwEntries);

        [DllImport("PclUtilities.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern bool activateUSBDevice(string pCom);


        //Bluetooth
        [DllImport("PclUtilities.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern int getPairedDevices([In, Out] BLUETOOTH_COMPANION_INFO pCompanions, ref int pdwSize, ref int pdwEntries);

        private bool retCode = false;
        //private Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private Socket s = null;
        private bool isSckOpened = false;
        byte[] bytesRecv = new byte[10240];

        internal bool RefreshStatus()
        {
            StringBuilder result = new StringBuilder(10);

            retCode = serverStatus(result);
            string strRes = result.ToString();

            return retCode;
        }

        string lblStatus = "desconectado";
        //se agrego para la impresion 
        string AppendText = "";
        #endregion


        private string Asigna()
        {
            //string strTimeput ="";
            string Param_pinpad = var_timeout; //System.IO.File.ReadAllText(@"C:\HubblePOS\Logs\PARAM_PINPAD.txt");
            try
            {

                //if (strTimeput == String.Empty)
                //{
                string timeoutre = Param_pinpad;
                //String time_out = String.Empty;

                // string[] _arrayTmpICOR;
                //_arrayTmpICOR = new string[9];
                //string[] _array1ICOR = strTempICOR.Split('|');
                //for (int i = 0; i < _array1ICOR.Length; i++)
                //{
                //    string[] _array2ICOR = _array1ICOR[i].Split(':');
                //    if (_array2ICOR[0].ToUpper().Trim().ToString() == "TIMEOUT")
                //        time_out = _array2ICOR[1].ToString().Trim();
                //}
                //PARAMS_PINPAD REQUESIC0 = new PARAMS_PINPAD
                //{
                //    TIMEOUT = time_out
                //};
                //string res_ic0RES = JsonConvert.SerializeObject(REQUESIC0);
                //var deserializeJsonrequestic0 = JsonConvert.DeserializeObject<PARAMS_PINPAD>(res_ic0RES);
                //string timeoutre= deserializeJsonrequestic0.TIMEOUT.ToString(); 
                //exec.GeneraLogInfo("if","TIMEOUT: "+ timeoutre);


                if (timeoutre.Trim() == "" || timeoutre == String.Empty)
                {
                    return "15";
                }
                else
                {
                    return timeoutre;
                }
                //}

                //else
                //{
                //    exec.GeneraLogInfo("else", strTimeput);
                //    return strTimeput;
                //}

            }
            catch (Exception)
            {
                // exec.GeneraLogInfo("catch", "15");
                return "15";
            }
        }


        private string IPS()
        {
            string Param_pinpad = var_pinpad; //System.IO.File.ReadAllText(@"C:\HubblePOS\Logs\PARAM_PINPAD.txt");
            try
            {

                //    //if (strIp == String.Empty)
                //    //{
                string ipreturn = Param_pinpad;
                //    String ips = String.Empty;

                //    string[] _arrayTmpICOR;
                //    _arrayTmpICOR = new string[9];
                //    string[] _array1ICOR = strTempICOR.Split('|');
                //    for (int i = 0; i < _array1ICOR.Length; i++)
                //    {
                //        string[] _array2ICOR = _array1ICOR[i].Split(':');
                //        if (_array2ICOR[0].ToUpper().Trim().ToString() == "IP")
                //            ips = _array2ICOR[1].ToString().Trim();
                //    }
                //    PARAMS_PINPAD REQUESIC0 = new PARAMS_PINPAD
                //    {
                //        IP = ips
                //    };
                //    string res_ic0RES = JsonConvert.SerializeObject(REQUESIC0);
                //    var deserializeJsonrequestic0 = JsonConvert.DeserializeObject<PARAMS_PINPAD>(res_ic0RES);
                //    string ipreturn=deserializeJsonrequestic0.IP.ToString();
                // exec.GeneraLogInfo("CODE_IP", "IP ACTUAL: "+ipreturn);

                if (ipreturn.Trim() == "" || ipreturn == String.Empty)
                {
                    return "127.0.0.1";
                }
                else
                {
                    return ipreturn;
                }

                //}

                //else
                //{
                //    exec.GeneraLogInfo("CODE_IP", "IP ACTUAL: " + strIp);
                //    return strIp;
                //}

            }
            catch (Exception)
            {
                //  exec.GeneraLogInfo("CODE_IP", "IP ACTUAL: 127.0.0.1");
                return "127.0.0.1";
            }
        }


        private void ClosePCL(string cerrar)
        {
            closePCLSocket();
            stopPclService();
            //se remplazo

            //Console.WriteLine("conexion cerrada");
        }

        private void OpenPCL(string abrir)
        {

            int iAtt = 0;
            startPclService();
            while (lblStatus != "Conectado")
            {
                Thread.Sleep(10);
                if (iAtt++ < 20)
                {
                    break;
                }
                else
                {
                    return;
                }
            }

            int retStatus = addDynamicBridge(1080, 0);//0 indica conexion de windows a telium
            Thread.Sleep(10);
            //groupBox2.Enabled = true;
        }

        private int getTrxStatus(string response)
        {
            var dict = response.Substring(5).Split('|')
                            .Select(x => x.Split(':'))
                            .ToDictionary(x => x[0],
                                            x => x[1]);
            return Int32.Parse(dict["RETURN"]);
        }

        private void openPCLSocket()
        {
            if (isSckOpened == false)
            {
                //IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
                //IPAddress ipAddress = IPAddress.Parse("192.168.43.176");
                IPAddress ipAddress = IPAddress.Parse(IPS());
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 1080);

                try
                {
                    s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    s.Connect(remoteEP);
                    Thread.Sleep(100);
                    isSckOpened = true;
                    Console.WriteLine("Socket connected to {0}",
                        s.RemoteEndPoint.ToString());

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                    s.Close();
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                    s.Close();
                    throw (se);
                }
                catch (Exception el)
                {
                    Console.WriteLine("Unexpected exception : {0}", el.ToString());
                    s.Close();
                }
            }
        }

        public void closePCLSocket()
        {
            if (isSckOpened == true)
            {
                try
                {
                    Thread.Sleep(100);
                    s.Shutdown(SocketShutdown.Both);
                    s.Close();
                    isSckOpened = false;
                }
                catch (Exception el)
                {
                    Console.WriteLine("Unexpected exception : {0}", el.ToString());
                }
            }
        }

        private void refreshLog(string message)
        {
            AppendText = Convert.ToString((DateTime.Now.Hour.ToString("00") + ":" +
                                DateTime.Now.Minute.ToString("00") + ":" +
                                DateTime.Now.Second.ToString("00") + "." +
                                DateTime.Now.Millisecond.ToString("0000") + "-" +
                                "Tx: " + message));
            AppendText = (Environment.NewLine);
            AppendText = (Environment.NewLine);
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            seconds++;
        }

        public void autorizar_separado(int autorizar)
        {
            try
            {
                openPCLSocket();
                string messageIC1 = "", message = "";
                string respCode = "", auth = "";
                if (autorizar == 1)
                {
                    respCode = "09";
                    auth = "";
                }
                else if (autorizar == 1)
                {
                    respCode = "01";
                }
                var tokensIC1 = new List<tRequestToken>
                {
                    new tRequestToken() { LabelEN = "REQUEST",              Value = "IC1" },
                    new tRequestToken() { LabelEN = "AUTHORIZATION",        Value = auth},
                    new tRequestToken() { LabelEN = "RESPONSE_CODE",        Value = respCode },
                    // new tRequestToken() { LabelEN = "EMV_ISSUER_DATA",      Value = null},//se cambio por null
                    new tRequestToken() { LabelEN = "EMV_TLV",              Value = "959F269F279F369F419F5B"},
                    //new tRequestToken() { LabelEN = "EMV_TLV",              Value = "5F289F079F159F1C9F399B5F20579F06"},
                };

                foreach (tRequestToken tkn in tokensIC1)
                {
                    if (tkn.Value.Length > 0)
                    {
                        messageIC1 += "|" + tkn.LabelEN + ":" + tkn.Value;
                    }
                }
                message = messageIC1.Length.ToString("0000") + messageIC1;
                Console.WriteLine(message);
                int bytesSent = s.Send(Encoding.ASCII.GetBytes(message));
                if (bytesSent > 0)
                {
                    refreshLog(message);
                }
                s.ReceiveTimeout = 45 * 1000;
                bytesRecv = Encoding.ASCII.GetBytes(new string(' ', 10240));
                int bytesRec = 0;
                bytesRec = s.Receive(bytesRecv);
                if (bytesRec > 0)
                {
                    string dataRec = System.Text.Encoding.UTF8.GetString(bytesRecv, 0, bytesRec);
                    refreshLog(dataRec);
                    int iStatus = getTrxStatus(dataRec);
                    if (iStatus == 0)
                    {
                        var dict = dataRec.Substring(5).Split('|')
                            .Select(x => x.Split(':'))
                            .ToDictionary(x => x[0],
                                    x => x[1]);
                        int info = Int32.Parse(dict["TRANSACTION_INFO"]);
                        if (info == 4)
                        {
                            Console.WriteLine("Transacción Aprobada.");
                        }
                        else
                        {
                            Console.WriteLine("Transacción Declinada.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Transacción Cancelada: " + iStatus.ToString("00"));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected exception : {0}", ex.ToString());
            }
            closePCLSocket();
        }

        public string Code_convert(string codigo)
        {
            string retorno = "";
            switch (codigo)
            {
                case "00":
                    retorno = "Operación exitosa";
                    break;
                case "01":
                    retorno = "Mensaje inválido";
                    break;
                case "02":
                    retorno = "Formato de datos inválido";
                    break;
                case "03":
                    retorno = "Respuesta no válida";
                    break;
                case "04":
                    retorno = "Proceso invalido";
                    break;
                case "05":
                    retorno = "Error de configuración";
                    break;
                case "06":
                    retorno = "Time Out";
                    break;
                case "07":
                    retorno = "Reintentar";
                    break;
                case "08":
                    retorno = "Cancelada";
                    break;
                case "09":
                    retorno = "Error de comunicación";
                    break;
                case "10":
                    retorno = "Falla en la lectura del chip";
                    break;
                case "11":
                    retorno = "No se ha realizado inicialización de llaves";
                    break;
                case "12":
                    retorno = "CRC no coincide";
                    break;
                case "13":
                    retorno = "Check Value incorrecto";
                    break;
                case "14":
                    retorno = "Tabla vacía";
                    break;
                case "20":
                    retorno = "Insertar chip";
                    break;
                case "21":
                    retorno = "Banda magnética";
                    break;
                case "22":
                    retorno = "Error en lectura Contactless";
                    break;
                case "23":
                    retorno = "Tarjeta removida";
                    break;
                case "24":
                    retorno = "Tarjeta bloqueada";
                    break;
                case "25":
                    retorno = "Tarjeta no soportada";
                    break;
                case "26":
                    retorno = "Aplicación inválida";
                    break;
                case "27":
                    retorno = "Tarjeta Operador inválida";
                    break;
                case "29":
                    retorno = "Tarjeta con fecha vencimiento expirada";
                    break;
                case "30":
                    retorno = "Fecha Inválida";
                    break;
                case "52":
                    retorno = "Llave Inexistente";
                    break;
                case "53":
                    retorno = "Problemas de cripto HSM";
                    break;
                case "54":
                    retorno = "Límite de cifrado excedido. Inicialice llave";
                    break;
                case "55":
                    retorno = "Formato Inválido";
                    break;
                case "60":
                    retorno = "Comando no permitido";
                    break;
                case "61":
                    retorno = "Llaves inicializadas. Comando NO Reconocido";
                    break;
                case "63":
                    retorno = "Error de Lectura";
                    break;
                case "79":
                    retorno = "Comando deshabilitado";
                    break;
                case "99":
                    retorno = "Error desconocido";
                    break;
                default:
                    retorno = "error";
                    break;
            }

            return retorno;
        }

        private string MetodoIC0(string monpago)
        {
            string logs = "CODE_Ini";
            string logs2 = "Inicio en el metodo ICO";
            exec.GeneraLogInfo(logs, logs2);
            String retorno = "VACIO";//imprime la descripcion general en el tpv 
            string IC0PAN = "";
            bool FIRMA = false;//indica si se requiere firma en las tarjetas
            bool Bandera_IC0 = false;//bandera que indica si entro en el consumo de la api/SaleRequest
            bool Bandera_IC0_autorizacion = false;//Indica si se requiere cancelar la autorizacion
            //Variables global reversal 
            string No_AutorizaReversal = "";
            string No_TarjetaReversal = ""; ;
            string Desc_Reversal = "";
            bool Bandera_Reversal = false;//bandera indica si se debe imprimir ticket fallido

            string Reversal = "";
            try
            {
                openPCLSocket();
                DateTime now = DateTime.Now;
                string sDatetime = now.Year.ToString("0000") + now.Month.ToString("00") + now.Day.ToString("00") +
                    now.Hour.ToString("00") + now.Minute.ToString("00") + now.Second.ToString("00");
                string messageIC0 = "", message = "";
                var tokensIC0 = new List<tRequestToken>
                {
                    new tRequestToken() { LabelEN = "REQUEST",              Value = "IC0" },
                    new tRequestToken() { LabelEN = "DATETIME",             Value = sDatetime },
                    new tRequestToken() { LabelEN = "TIMEOUT",              Value = "30" },
                    new tRequestToken() { LabelEN = "BANK_ID",              Value = "HSBC01    "},
                    new tRequestToken() { LabelEN = "PINPAD_ENTRIES",       Value = "11111" },
                    new tRequestToken() { LabelEN = "TOTAL_AMOUNT",         Value = monpago},//monto a pagar-----------estatico
                    new tRequestToken() { LabelEN = "OTHER_AMOUNT",         Value = ""},
                    new tRequestToken() { LabelEN = "OPTIONS",              Value = "0100"},
                    new tRequestToken() { LabelEN = "TRANS_TYPE",           Value = "00"},
                    new tRequestToken() { LabelEN = "CURRENCY_CODE",        Value = "484"},
                    //new tRequestToken() { LabelEN = "EMV_TLV",              Value = "574F505F345F2A8482959A9C9F029F039F099F109F129F1A9F1E9F269F279F339F349F359F369F379F419F53"},se cambio por el de abajo
                    new tRequestToken() { LabelEN = "EMV_TLV",              Value = "574F505F345F2A8482959A9C9F029F039F099F109F129F1A9F1E9F269F279F339F349F359F369F379F419F535F289F079F159F1C9F399B5F20579F06"},
                    new tRequestToken() { LabelEN = "EMV_TLV_FORMAT",              Value = "0" }
                };

                foreach (tRequestToken tkn in tokensIC0)
                {
                    if (tkn.Value.Length > 0)
                    {
                        messageIC0 += "|" + tkn.LabelEN + ":" + tkn.Value;//se envia a la terminal pinpad
                    }
                }
                message = messageIC0.Length.ToString("0000") + messageIC0;

                string Time_Ini_ic0 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");

                int bytesSent = s.Send(Encoding.ASCII.GetBytes(message));
                if (bytesSent > 0)
                {
                    refreshLog(message);
                }
                s.ReceiveTimeout = 45 * 1000;
                bytesRecv = Encoding.ASCII.GetBytes(new string(' ', 10240));
                int bytesRec = 0;

                bytesRec = s.Receive(bytesRecv);


                if (bytesRec > 0)
                {
                    string dataRec = System.Text.Encoding.UTF8.GetString(bytesRecv, 0, bytesRec);
                    string Time_FIN_ic0 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                    string horasss = "Tiempo de respuesta del pinpad en IC0: hora_inicio: " + Time_Ini_ic0 + " Hora_termino: " + Time_FIN_ic0;
                    exec.GeneraLogInfo("CODE_IC0_PINPAD", horasss);

                    //refreshLog(dataRec);                    
                    int iStatus = getTrxStatus(dataRec);
                    if (iStatus != 0)
                    {
                        closePCLSocket();
                        string codigo_errorp = iStatus.ToString("00");
                        string desc_cerror = Code_convert(codigo_errorp);
                        string log1 = "CODE_002";
                        string log2 = "Transacción Cancelada: [" + iStatus.ToString("00") + "] " + desc_cerror;
                        exec.GeneraLogInfo(log1, log2);
                        log1 = "CODE_Fin";
                        log2 = "Finalizo Metodo IC0";
                        exec.GeneraLogInfo(log1, log2);

                        retorno = "[CODE_002]  Transacción Cancelada: [" + iStatus.ToString("00") + "] " + desc_cerror;
                        Bandera_IC0 = false;
                    }
                    else
                    {
                        string PEM = "";
                        string hora = DateTime.Now.ToString("yyyyMMddHHmmss");
                        Random ss = new Random();
                        int ran = ss.Next(10000, 99999);
                        string postime = "|" + "POSTTIMESTAMP:" + hora + "_" + Convert.ToString(ran);

                        string newdataRec = dataRec + postime;

                        #region BANDA MAGNETICA

                        //string newdataRec = dataRec;
                        //SE AGREGO PARA LO DE BANDA MAGNETICA
                        string strTempICOR = newdataRec;
                        String strResponse = String.Empty;
                        String strReturn = String.Empty;
                        String strPan = String.Empty;
                        String strPes = String.Empty;
                        String strPez = String.Empty;
                        String strPem = String.Empty;
                        String strEmv_tlv = String.Empty;
                        String strTransactionInfo = String.Empty;
                        String strPinPad = String.Empty;



                        //string strTempICOR = "0679|RESPONSE: IC0 |RETURN:00 |PAN:557910 * *****5342 |PES:IGLANEA21 IGLAN30000000392237750000000000000000000| PEZ:FFFF9876543210E0001900000240010527A0002252D15A81A4877F1160FB3B9569C8185AB621C5CD9CE2F453422B8E64D6 |PEM:05 | EMV_TLV:4F07A000000004101050104465626974204D6173746572436172645F3401005F2A0204848407A000000004101082023900950500000080009A031909129C01009F02060000000002009F03060000000000009F090200029F10120110A04003220000000000000000000000FF9F12104465626974204D6173746572436172649F1A0204849F1E0830333932323337379F260877318F24FEEFDD039F2701809F3303E0B0C89F34031E03009F3501229F3602008E9F3704972E55229F4104000000489F530152 | TRANSACTION_INFO:8001 |PINPAD_SN:190337313011108503922377";
                        string[] _arrayTmpICOR;
                        _arrayTmpICOR = new string[9];
                        strTempICOR = strTempICOR.Substring(5, strTempICOR.Length - 5);
                        string[] _array1ICOR = strTempICOR.Split('|');
                        for (int i = 0; i < _array1ICOR.Length; i++)
                        {
                            string[] _array2ICOR = _array1ICOR[i].Split(':');
                            if (_array2ICOR[0].ToUpper().Trim().ToString() == "RESPONSE")
                                strResponse = _array2ICOR[1].ToString().Trim();
                            else if (_array2ICOR[0].ToUpper().Trim().ToString() == "RETURN")
                                strReturn = _array2ICOR[1].ToString().Trim();
                            else if (_array2ICOR[0].ToUpper().Trim().ToString() == "PAN")
                                strPan = _array2ICOR[1].ToString().Trim();
                            else if (_array2ICOR[0].ToUpper().Trim().ToString() == "PES")
                                strPes = _array2ICOR[1].ToString().Trim();
                            else if (_array2ICOR[0].ToUpper().Trim().ToString() == "PEZ")
                                strPez = _array2ICOR[1].ToString().Trim();
                            else if (_array2ICOR[0].ToUpper().Trim().ToString() == "PEM")
                                strPem = _array2ICOR[1].ToString().Trim();
                            else if (_array2ICOR[0].ToUpper().Trim().ToString() == "EMV_TLV")
                                strEmv_tlv = _array2ICOR[1].ToString().Trim();
                            else if (_array2ICOR[0].ToUpper().Trim().ToString() == "TRANSACTION_INFO")
                                strTransactionInfo = _array2ICOR[1].ToString().Trim();
                            else if (_array2ICOR[0].ToUpper().Trim().ToString() == "PINPAD_SN")
                                strPinPad = _array2ICOR[1].ToString().Trim();
                        }
                        dataJson REQUESIC0 = new dataJson
                        {
                            RESPONSE = strResponse,
                            RETURN = strReturn,
                            PAN = strPan,
                            PES = strPes,
                            PEZ = strPez,
                            PEM = strPem,
                            EMV_TLV = strEmv_tlv,
                            TRANSACTION_INFO = strTransactionInfo,
                            PINPAD_SN = strPinPad
                        };
                        string res_ic0RES = JsonConvert.SerializeObject(REQUESIC0);
                        #endregion
                        var deserializeJsonrequestic0 = JsonConvert.DeserializeObject<dataJson>(res_ic0RES);
                        PEM = deserializeJsonrequestic0.PEM.ToString();
                        string PAN_reversal = deserializeJsonrequestic0.PAN.ToString();

                        if (PEM == "80" || PEM == "90")
                        {

                            newdataRec += "|TOTAL_AMOUNT:" + monpago;

                            FIRMA = true;
                        }
                        else
                        {
                            FIRMA = false;
                        }
                        //PaymentTerminal LO AGREGADO POR BANDA MAGNETICA 
                        IC0PAN = newdataRec;//icopan nuevo  QUITAR EN CASO DE QUE NO FUNCIONE


                        string sResponsePCRest = SendDataRestToPC(newdataRec, "api/SaleRequest");
                        string sResponsePCRest_sResult;
                        bool sResponsePCRest_Bandera_IC0_SR;
                        bool sResponsePCRest_Bandera_Time;
                        var deserializeJson = JsonConvert.DeserializeObject<Consumo_apis>(sResponsePCRest);
                        sResponsePCRest_sResult = deserializeJson.sResult.ToString();
                        sResponsePCRest_Bandera_IC0_SR = deserializeJson.Bandera_IC0_SR;
                        sResponsePCRest_Bandera_Time = deserializeJson.Bandera_Time;
                        //string sleep_ini = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");

                        //Thread.Sleep(15000);
                        //string sleep_fin = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                        //string hora_sleep = "Hora_inicio: "+sleep_ini+" Hora_fin: "+sleep_fin;
                        //exec.GeneraLogInfo("CODE_SLEEP", hora_sleep);

                        if (sResponsePCRest_Bandera_IC0_SR == true)
                        {
                            retorno = sResponsePCRest_sResult;
                            Bandera_IC0_autorizacion = true;
                        }
                        //if (sResponsePCRest_Bandera_IC0_SR==false)
                        else
                        {
                            autorizar_separado(1);
                            ClosePCL("false");
                            string log1 = "";
                            string log2 = "";
                            Bandera_IC0_autorizacion = false;
                            if (sResponsePCRest_Bandera_Time == true)
                            {
                                string Res_Time = Metodo_Reversal(newdataRec, "api/SaleRequest", "|REV_TYPE:0");
                                var deserializeJson_reversal = JsonConvert.DeserializeObject<Consumo_apis_reversal>(Res_Time);
                                bool bandera_rever = deserializeJson_reversal.Bandera_rever;
                                string sResult_rever = deserializeJson_reversal.sResult_rever.ToString();

                                if (bandera_rever == true)
                                {


                                    //retorno = "Se cancelaron los cargos a la tarjeta" + sResult_rever;
                                    retorno = "[CODE_011] Se cancelaron los cargos a la tarjeta";
                                    No_AutorizaReversal = "";
                                    Desc_Reversal = "Se cancelaron los cargos a la tarjeta";
                                    No_TarjetaReversal = PAN_reversal;
                                    Bandera_Reversal = true;

                                    log1 = "CODE_011";
                                    log2 = "Se cancelaron los cargos a la tarjeta";
                                    exec.GeneraLogInfo(log1, log2);
                                    log1 = "CODE_Fin";
                                    log2 = "Finalizo despues de un reversal de TYPE:0 en el metodo IC0";
                                    exec.GeneraLogInfo(log1, log2);


                                }
                                else
                                {

                                    #region Reversal REQUEST
                                    //empieza request ic1

                                    //se agrega para la impresion ic1 request
                                    string strTemp = sResult_rever;

                                    String RESPONSE_CODE3 = String.Empty;
                                    String AUTHORIZATIONCODE = String.Empty;
                                    string[] _arrayTmpr3;
                                    _arrayTmpr3 = new string[11];
                                    strTemp = strTemp.Substring(5, strTemp.Length - 5);
                                    string[] _array1ICOR3 = strTemp.Split('|');
                                    for (int i = 0; i < _array1ICOR3.Length; i++)
                                    {
                                        string[] _array2ICOR3 = _array1ICOR3[i].Split(':');
                                        if (_array2ICOR3[0].ToUpper().Trim().ToString() == "RESPONSE_CODE")
                                            RESPONSE_CODE3 = _array2ICOR3[1].ToString().Trim();
                                        else if (_array2ICOR3[0].ToUpper().Trim().ToString() == "AUTHORIZATION")
                                            AUTHORIZATIONCODE = _array2ICOR3[1].ToString().Trim();
                                    }
                                    dataJson2 dtic1r = new dataJson2
                                    {
                                        RESPONSE_CODE = RESPONSE_CODE3,
                                        AUTHORIZATION = AUTHORIZATIONCODE
                                    };

                                    string res_ic1 = JsonConvert.SerializeObject(dtic1r);

                                    //-----------------------------------------
                                    var deserializeJsonrequest = JsonConvert.DeserializeObject<dataJson2request>(res_ic1);
                                    string Response_Reversal = deserializeJsonrequest.RESPONSE_CODE.ToString();
                                    #endregion

                                    if (Response_Reversal == "07")
                                    {
                                        retorno = "[CODE_012] No se aplicaron cargos a la tarjeta";
                                        No_AutorizaReversal = "";
                                        Desc_Reversal = "No se aplicaron cargos a la tarjeta";
                                        No_TarjetaReversal = PAN_reversal;
                                        Bandera_Reversal = true;

                                        log1 = "CODE_012";
                                        log2 = "No se aplicaron cargos a la tarjeta";
                                        exec.GeneraLogInfo(log1, log2);
                                        log1 = "CODE_Fin";
                                        log2 = "Finalizo despues de un reversal de TYPE:0 en el metodo IC0";
                                        exec.GeneraLogInfo(log1, log2);
                                    }

                                    else
                                    {
                                        retorno = "[CODE_012.1] Favor de llamar a su banco, para verificar el estado de la transaccion";
                                        No_AutorizaReversal = "";
                                        Desc_Reversal = "Favor de llamar a su banco, para verificar el estado de la transaccion";
                                        No_TarjetaReversal = PAN_reversal;
                                        Bandera_Reversal = true;

                                        log1 = "CODE_012.1";
                                        log2 = retorno;
                                        exec.GeneraLogInfo(log1, log2);
                                        log1 = "CODE_Fin";
                                        log2 = "Finalizo despues de un reversal de TYPE:0 en el metodo IC0";
                                        exec.GeneraLogInfo(log1, log2);
                                    }




                                }

                            }
                            else if (sResponsePCRest_sResult == "04")//se puso en caso de reversal |REV_TYPE:1
                            {
                                string Res_Time = Metodo_Reversal(newdataRec, "api/SaleRequest", "|REV_TYPE:1");
                                var deserializeJson_reversal = JsonConvert.DeserializeObject<Consumo_apis_reversal>(Res_Time);
                                bool bandera_rever = deserializeJson_reversal.Bandera_rever;
                                string sResult_rever = deserializeJson_reversal.sResult_rever.ToString();


                                if (bandera_rever == true)
                                {


                                    //retorno = "Se cancelaron los cargos a la tarjeta" + sResult_rever;
                                    retorno = "[CODE_014] Se cancelaron los cargos a la tarjeta";
                                    No_AutorizaReversal = "";
                                    Desc_Reversal = "Se cancelaron los cargos a la tarjeta";
                                    No_TarjetaReversal = PAN_reversal;
                                    Bandera_Reversal = true;

                                    log1 = "CODE_014";
                                    log2 = "Se cancelaron los cargos a la tarjeta";
                                    exec.GeneraLogInfo(log1, log2);
                                    log1 = "CODE_Fin";
                                    log2 = "Finalizo despues de un reversal de TYPE:1 en el metodo IC0";
                                    exec.GeneraLogInfo(log1, log2);


                                }
                                else
                                {
                                    #region Reversal REQUEST
                                    //empieza request ic1

                                    //se agrega para la impresion ic1 request
                                    string strTemp = sResult_rever;

                                    String RESPONSE_CODE3 = String.Empty;
                                    String AUTHORIZATIONCODE = String.Empty;
                                    string[] _arrayTmpr3;
                                    _arrayTmpr3 = new string[11];
                                    strTemp = strTemp.Substring(5, strTemp.Length - 5);
                                    string[] _array1ICOR3 = strTemp.Split('|');
                                    for (int i = 0; i < _array1ICOR3.Length; i++)
                                    {
                                        string[] _array2ICOR3 = _array1ICOR3[i].Split(':');
                                        if (_array2ICOR3[0].ToUpper().Trim().ToString() == "RESPONSE_CODE")
                                            RESPONSE_CODE3 = _array2ICOR3[1].ToString().Trim();
                                        else if (_array2ICOR3[0].ToUpper().Trim().ToString() == "AUTHORIZATION")
                                            AUTHORIZATIONCODE = _array2ICOR3[1].ToString().Trim();
                                    }
                                    dataJson2 dtic1r = new dataJson2
                                    {
                                        RESPONSE_CODE = RESPONSE_CODE3,
                                        AUTHORIZATION = AUTHORIZATIONCODE
                                    };

                                    string res_ic1 = JsonConvert.SerializeObject(dtic1r);

                                    //-----------------------------------------
                                    var deserializeJsonrequest = JsonConvert.DeserializeObject<dataJson2request>(res_ic1);
                                    string Response_Reversal = deserializeJsonrequest.RESPONSE_CODE.ToString();
                                    #endregion

                                    if (Response_Reversal == "07")
                                    {
                                        retorno = "[CODE_015] No se aplicaron cargos a la tarjeta";
                                        No_AutorizaReversal = "";
                                        Desc_Reversal = "No se aplicaron cargos a la tarjeta";
                                        No_TarjetaReversal = PAN_reversal;
                                        Bandera_Reversal = true;

                                        log1 = "CODE_015";
                                        log2 = "No se aplicaron cargos a la tarjeta";
                                        exec.GeneraLogInfo(log1, log2);
                                        log1 = "CODE_Fin";
                                        log2 = "Finalizo despues de un reversal de TYPE:1 en el metodo IC0";
                                        exec.GeneraLogInfo(log1, log2);



                                    }

                                    else
                                    {
                                        retorno = "[CODE_015.1] Favor de llamar a su banco, para verificar el estado de la transaccion";
                                        No_AutorizaReversal = "";
                                        Desc_Reversal = "Favor de llamar a su banco, para verificar el estado de la transaccion";
                                        No_TarjetaReversal = PAN_reversal;
                                        Bandera_Reversal = true;

                                        log1 = "CODE_015.1";
                                        log2 = retorno;
                                        exec.GeneraLogInfo(log1, log2);
                                        log1 = "CODE_Fin";
                                        log2 = "Finalizo despues de un reversal de TYPE:1 en el metodo IC0";
                                        exec.GeneraLogInfo(log1, log2);
                                    }



                                }

                            }

                            else
                            {
                                string Res_Time = Metodo_Reversal(newdataRec, "api/SaleRequest", "|REV_TYPE:0");
                                var deserializeJson_reversal = JsonConvert.DeserializeObject<Consumo_apis_reversal>(Res_Time);
                                bool bandera_rever = deserializeJson_reversal.Bandera_rever;
                                string sResult_rever = deserializeJson_reversal.sResult_rever.ToString();

                                if (bandera_rever == true)
                                {


                                    //retorno = "Se cancelaron los cargos a la tarjeta" + sResult_rever;
                                    retorno = "[CODE_016] Se cancelaron los cargos a la tarjeta";
                                    No_AutorizaReversal = "";
                                    Desc_Reversal = "Se cancelaron los cargos a la tarjeta";
                                    No_TarjetaReversal = PAN_reversal;
                                    Bandera_Reversal = true;

                                    log1 = "CODE_016";
                                    log2 = "Se cancelaron los cargos a la tarjeta";
                                    exec.GeneraLogInfo(log1, log2);
                                    log1 = "CODE_Fin";
                                    log2 = "Finalizo despues de un reversal de TYPE:0 en el metodo IC0";
                                    exec.GeneraLogInfo(log1, log2);


                                }
                                else
                                {
                                    #region Reversal REQUEST
                                    //empieza request ic1

                                    //se agrega para la impresion ic1 request
                                    string strTemp = sResult_rever;

                                    String RESPONSE_CODE3 = String.Empty;
                                    String AUTHORIZATIONCODE = String.Empty;
                                    string[] _arrayTmpr3;
                                    _arrayTmpr3 = new string[11];
                                    strTemp = strTemp.Substring(5, strTemp.Length - 5);
                                    string[] _array1ICOR3 = strTemp.Split('|');
                                    for (int i = 0; i < _array1ICOR3.Length; i++)
                                    {
                                        string[] _array2ICOR3 = _array1ICOR3[i].Split(':');
                                        if (_array2ICOR3[0].ToUpper().Trim().ToString() == "RESPONSE_CODE")
                                            RESPONSE_CODE3 = _array2ICOR3[1].ToString().Trim();
                                        else if (_array2ICOR3[0].ToUpper().Trim().ToString() == "AUTHORIZATION")
                                            AUTHORIZATIONCODE = _array2ICOR3[1].ToString().Trim();
                                    }
                                    dataJson2 dtic1r = new dataJson2
                                    {
                                        RESPONSE_CODE = RESPONSE_CODE3,
                                        AUTHORIZATION = AUTHORIZATIONCODE
                                    };

                                    string res_ic1 = JsonConvert.SerializeObject(dtic1r);

                                    //-----------------------------------------
                                    var deserializeJsonrequest = JsonConvert.DeserializeObject<dataJson2request>(res_ic1);
                                    string Response_Reversal = deserializeJsonrequest.RESPONSE_CODE.ToString();
                                    #endregion

                                    if (Response_Reversal == "07")
                                    {
                                        retorno = "[CODE_017] No se aplicaron cargos a la tarjeta";
                                        No_AutorizaReversal = "";
                                        Desc_Reversal = "No se aplicaron cargos a la tarjeta";
                                        No_TarjetaReversal = PAN_reversal;
                                        Bandera_Reversal = true;

                                        log1 = "CODE_017";
                                        log2 = "No se aplicaron cargos a la tarjeta";
                                        exec.GeneraLogInfo(log1, log2);
                                        log1 = "CODE_Fin";
                                        log2 = "Finalizo despues de un reversal de TYPE:0 en el metodo IC0";
                                        exec.GeneraLogInfo(log1, log2);



                                    }

                                    else
                                    {
                                        retorno = "[CODE_017.1] Favor de llamar a su banco, para verificar el estado de la transaccion";
                                        No_AutorizaReversal = "";
                                        Desc_Reversal = "Favor de llamar a su banco, para verificar el estado de la transaccion";
                                        No_TarjetaReversal = PAN_reversal;
                                        Bandera_Reversal = true;

                                        log1 = "CODE_017.1";
                                        log2 = retorno;
                                        exec.GeneraLogInfo(log1, log2);
                                        log1 = "CODE_Fin";
                                        log2 = "Finalizo despues de un reversal de TYPE:0 en el metodo IC0";
                                        exec.GeneraLogInfo(log1, log2);
                                    }



                                }

                            }

                            var Obj_Reversal = new Reversal
                            {
                                Bandera_Reversal = Bandera_Reversal,
                                Desc_Reversal = Desc_Reversal,
                                No_AutorizaReversal = No_AutorizaReversal,
                                No_TarjetaReversal = No_TarjetaReversal
                            };

                            Reversal = JsonConvert.SerializeObject(Obj_Reversal);
                        }

                        Bandera_IC0 = true;
                        refreshLog(newdataRec);
                        refreshLog($"Respuesta REST-PC: {sResponsePCRest}");

                    }
                }
            }
            catch (Exception ex)
            {
                closePCLSocket();
                retorno = "[CODE_001] No_se_pudo_obtener_comunicacion_con_el_socket";
                Bandera_IC0 = false;
                string log1 = "CODE_001";
                string log2 = retorno;
                exec.GeneraLogInfo(log1, log2);
                log1 = "CODE_Fin";
                log2 = "Finalizo despues de una exepcion, en la comunicacion en el metodo IC0";
                exec.GeneraLogInfo(log1, log2);

            }




            var icores = new respIC0
            {
                IC0PAN = IC0PAN,
                retorno = retorno,
                FIRMA = FIRMA,
                Bandera_IC0 = Bandera_IC0,
                Bandera_IC0_autorizacion = Bandera_IC0_autorizacion,
                Reversal = Reversal
            };

            string ic0respuesta = JsonConvert.SerializeObject(icores);

            return ic0respuesta;
        }

        private string MetodoIC1(string autorizarr)
        {
            #region variables
            var ic1recbjson = JsonConvert.DeserializeObject<respIC1>(autorizarr);
            //IC0 = deserializeJsonrequestIC0.retorno.ToString();
            string autorizar = ic1recbjson.resic1.ToString();
            string resic0 = ic1recbjson.resic0.ToString();//icopan para el reversal

            string cadenarequest1 = autorizar;
            //Reversal
            string No_AutorizaReversal = "";
            string No_TarjetaReversal = ""; ;
            string Desc_Reversal = "";
            bool Bandera_Reversal = false;//bandera indica si se debe imprimir ticket fallido

            string Reversal = "";

            //------------------------------------------------------------------
            string retorno = "VACIO";

            string AFILIATION = "";
            string ARQC = "";
            string AUTHORIZACION = "";
            string CARDHOLDER_NAME = "";
            string EMV_APP_LABEL = "";
            string NUMSEQ = "";
            string PAN = "";
            string EMV_AID = "";
            string FOLIO = "";

            string log1 = "";
            string log2 = "";
            #endregion
            //string respuesta2="";

            #region targeta
            //tarjeta
            string strTempICOR1 = resic0;
            String strPan1 = String.Empty; string[] _arrayTmpICOR1;
            _arrayTmpICOR1 = new string[9];
            strTempICOR1 = strTempICOR1.Substring(5, strTempICOR1.Length - 5);
            string[] _array1ICOR1 = strTempICOR1.Split('|');
            for (int i = 0; i < _array1ICOR1.Length; i++)
            {
                string[] _array2ICOR1 = _array1ICOR1[i].Split(':');

                if (_array2ICOR1[0].ToUpper().Trim().ToString() == "PAN")
                    strPan1 = _array2ICOR1[1].ToString().Trim();

            }
            dataJson REQUESIC01 = new dataJson
            {
                PAN = strPan1,

            };
            string res_ic0RES1 = JsonConvert.SerializeObject(REQUESIC01);

            var deserializeJsonreversal = JsonConvert.DeserializeObject<dataJson>(res_ic0RES1);
            string No_TarjetaReversal_s = deserializeJsonreversal.PAN.ToString();

            //termina tarjeta
            #endregion

            try
            {


                openPCLSocket();
                string Time_Ini_ic1 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                int bytesSent = s.Send(Encoding.ASCII.GetBytes(autorizar));
                if (bytesSent > 0)
                {
                    refreshLog(autorizar);
                }
                s.ReceiveTimeout = 45 * 1000;
                bytesRecv = Encoding.ASCII.GetBytes(new string(' ', 10240));
                int bytesRec = 0;
                bytesRec = s.Receive(bytesRecv);
                if (bytesRec > 0)
                {
                    string dataRec = System.Text.Encoding.UTF8.GetString(bytesRecv, 0, bytesRec);
                    string Time_FIN_ic1 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                    string horasss = "Tiempo de respuesta del pinpad en IC1: hora_inicio: " + Time_Ini_ic1 + " Hora_termino: " + Time_FIN_ic1;
                    exec.GeneraLogInfo("CODE_IC1_PINPAD", horasss);


                    refreshLog(dataRec);


                    int iStatus = getTrxStatus(dataRec);
                    if (iStatus == 0)
                    {
                        var dict = dataRec.Substring(5).Split('|')
                            .Select(x => x.Split(':'))
                            .ToDictionary(x => x[0],
                                    x => x[1]);
                        int info = Int32.Parse(dict["TRANSACTION_INFO"]);
                        if (info == 4)
                        {

                            string sResponsePCRest = SendDataRestToPC(dataRec, "api/SaleResult");
                            string sResponsePCRest_sResult;
                            bool sResponsePCRest_Bandera_IC1_SR;
                            var deserializeJson = JsonConvert.DeserializeObject<Consumo_apis>(sResponsePCRest);
                            sResponsePCRest_sResult = deserializeJson.sResult.ToString();
                            sResponsePCRest_Bandera_IC1_SR = deserializeJson.Bandera_IC0_SR;



                            if (sResponsePCRest_Bandera_IC1_SR == true)
                            {
                                #region IC0 REQUEST PAN
                                //ico request para PAN

                                string strTempICOR = resic0;
                                String strResponse = String.Empty;
                                String strReturn = String.Empty;
                                String strPan = String.Empty;
                                String strPes = String.Empty;
                                String strPez = String.Empty;
                                String strPem = String.Empty;
                                String strEmv_tlv = String.Empty;
                                String strTransactionInfo = String.Empty;
                                String strPinPad = String.Empty;



                                //string strTempICOR = "0679|RESPONSE: IC0 |RETURN:00 |PAN:557910 * *****5342 |PES:IGLANEA21 IGLAN30000000392237750000000000000000000| PEZ:FFFF9876543210E0001900000240010527A0002252D15A81A4877F1160FB3B9569C8185AB621C5CD9CE2F453422B8E64D6 |PEM:05 | EMV_TLV:4F07A000000004101050104465626974204D6173746572436172645F3401005F2A0204848407A000000004101082023900950500000080009A031909129C01009F02060000000002009F03060000000000009F090200029F10120110A04003220000000000000000000000FF9F12104465626974204D6173746572436172649F1A0204849F1E0830333932323337379F260877318F24FEEFDD039F2701809F3303E0B0C89F34031E03009F3501229F3602008E9F3704972E55229F4104000000489F530152 | TRANSACTION_INFO:8001 |PINPAD_SN:190337313011108503922377";
                                string[] _arrayTmpICOR;
                                _arrayTmpICOR = new string[9];
                                strTempICOR = strTempICOR.Substring(5, strTempICOR.Length - 5);
                                string[] _array1ICOR = strTempICOR.Split('|');
                                for (int i = 0; i < _array1ICOR.Length; i++)
                                {
                                    string[] _array2ICOR = _array1ICOR[i].Split(':');
                                    if (_array2ICOR[0].ToUpper().Trim().ToString() == "RESPONSE")
                                        strResponse = _array2ICOR[1].ToString().Trim();
                                    else if (_array2ICOR[0].ToUpper().Trim().ToString() == "RETURN")
                                        strReturn = _array2ICOR[1].ToString().Trim();
                                    else if (_array2ICOR[0].ToUpper().Trim().ToString() == "PAN")
                                        strPan = _array2ICOR[1].ToString().Trim();
                                    else if (_array2ICOR[0].ToUpper().Trim().ToString() == "PES")
                                        strPes = _array2ICOR[1].ToString().Trim();
                                    else if (_array2ICOR[0].ToUpper().Trim().ToString() == "PEZ")
                                        strPez = _array2ICOR[1].ToString().Trim();
                                    else if (_array2ICOR[0].ToUpper().Trim().ToString() == "PEM")
                                        strPem = _array2ICOR[1].ToString().Trim();
                                    else if (_array2ICOR[0].ToUpper().Trim().ToString() == "EMV_TLV")
                                        strEmv_tlv = _array2ICOR[1].ToString().Trim();
                                    else if (_array2ICOR[0].ToUpper().Trim().ToString() == "TRANSACTION_INFO")
                                        strTransactionInfo = _array2ICOR[1].ToString().Trim();
                                    else if (_array2ICOR[0].ToUpper().Trim().ToString() == "PINPAD_SN")
                                        strPinPad = _array2ICOR[1].ToString().Trim();
                                }
                                dataJson REQUESIC0 = new dataJson
                                {
                                    RESPONSE = strResponse,
                                    RETURN = strReturn,
                                    PAN = strPan,
                                    PES = strPes,
                                    PEZ = strPez,
                                    PEM = strPem,
                                    EMV_TLV = strEmv_tlv,
                                    TRANSACTION_INFO = strTransactionInfo,
                                    PINPAD_SN = strPinPad
                                };
                                #endregion 
                                string res_ic0RES = JsonConvert.SerializeObject(REQUESIC0);
                                //TERMINA REQUEST PARA PAN

                                #region IC1 REQUEST
                                //empieza request ic1

                                //se agrega para la impresion ic1 request
                                string strTemp = cadenarequest1;


                                String REQUEST3 = String.Empty;
                                String NUMSEQ3 = String.Empty;
                                String IDPOS3 = String.Empty;
                                String AFILIATION3 = String.Empty;
                                String EMV_TLV3 = String.Empty;
                                String RESPONSE_CODE3 = String.Empty;
                                String AUTHORIZATION3 = String.Empty;
                                String FOLIO3 = String.Empty;



                                string[] _arrayTmpr3;
                                _arrayTmpr3 = new string[11];
                                strTemp = strTemp.Substring(5, strTemp.Length - 5);
                                string[] _array1ICOR3 = strTemp.Split('|');
                                for (int i = 0; i < _array1ICOR3.Length; i++)
                                {
                                    string[] _array2ICOR3 = _array1ICOR3[i].Split(':');
                                    if (_array2ICOR3[0].ToUpper().Trim().ToString() == "REQUEST")
                                        REQUEST3 = _array2ICOR3[1].ToString().Trim();
                                    else if (_array2ICOR3[0].ToUpper().Trim().ToString() == "NUMSEQ")
                                        NUMSEQ3 = _array2ICOR3[1].ToString().Trim();
                                    else if (_array2ICOR3[0].ToUpper().Trim().ToString() == "IDPOS")
                                        IDPOS3 = _array2ICOR3[1].ToString().Trim();
                                    else if (_array2ICOR3[0].ToUpper().Trim().ToString() == "AFILIATION")
                                        AFILIATION3 = _array2ICOR3[1].ToString().Trim();
                                    else if (_array2ICOR3[0].ToUpper().Trim().ToString() == "EMV_TLV")
                                        EMV_TLV3 = _array2ICOR3[1].ToString().Trim();
                                    else if (_array2ICOR3[0].ToUpper().Trim().ToString() == "RESPONSE_CODE")
                                        RESPONSE_CODE3 = _array2ICOR3[1].ToString().Trim();
                                    else if (_array2ICOR3[0].ToUpper().Trim().ToString() == "AUTHORIZATION")
                                        AUTHORIZATION3 = _array2ICOR3[1].ToString().Trim();
                                    //se agrego esto para el folio
                                    else if (_array2ICOR3[0].ToUpper().Trim().ToString() == "FOLIO")
                                        FOLIO3 = _array2ICOR3[1].ToString().Trim();

                                }
                                dataJson2 dtic1r = new dataJson2
                                {
                                    REQUEST = REQUEST3,
                                    AUTHORIZATION = AUTHORIZATION3,
                                    IDPOS = IDPOS3,
                                    NUMSEQ = NUMSEQ3,
                                    RESPONSE_CODE = RESPONSE_CODE3,
                                    EMV_TLV = EMV_TLV3,
                                    AFILIATION = AFILIATION3,
                                    FOLIO = FOLIO3
                                };
                                #endregion
                                string res_ic1 = JsonConvert.SerializeObject(dtic1r);
                                //------------------reques ic1 termina

                                #region RESPONSE IC1
                                string strTempr = dataRec;

                                String strResponse2 = String.Empty;
                                String strReturn2 = String.Empty;
                                String strCARDHOLDER_NAME2 = String.Empty;
                                String strEMV_ARQC2 = String.Empty;
                                String strEMV_TC2 = String.Empty;
                                String strEMV_AID2 = String.Empty;
                                String strEMV_APP_LABEL2 = String.Empty;
                                String strEMV_TVR2 = String.Empty;
                                String strEMV_TLV2 = String.Empty;
                                String strTRANSACTION_INFO2 = String.Empty;
                                string[] _arrayTmpr2;
                                _arrayTmpr2 = new string[11];
                                strTempr = strTempr.Substring(5, strTempr.Length - 5);
                                string[] _array1ICOR2 = strTempr.Split('|');
                                for (int i = 0; i < _array1ICOR2.Length; i++)
                                {
                                    string[] _array2ICOR2 = _array1ICOR2[i].Split(':');
                                    if (_array2ICOR2[0].ToUpper().Trim().ToString() == "RESPONSE")
                                        strResponse2 = _array2ICOR2[1].ToString().Trim();
                                    else if (_array2ICOR2[0].ToUpper().Trim().ToString() == "RETURN")
                                        strReturn2 = _array2ICOR2[1].ToString().Trim();
                                    else if (_array2ICOR2[0].ToUpper().Trim().ToString() == "CARDHOLDER_NAME")
                                        strCARDHOLDER_NAME2 = _array2ICOR2[1].ToString().Trim();
                                    else if (_array2ICOR2[0].ToUpper().Trim().ToString() == "EMV_ARQC")
                                        strEMV_ARQC2 = _array2ICOR2[1].ToString().Trim();
                                    else if (_array2ICOR2[0].ToUpper().Trim().ToString() == "EMV_TC")
                                        strEMV_TC2 = _array2ICOR2[1].ToString().Trim();
                                    else if (_array2ICOR2[0].ToUpper().Trim().ToString() == "EMV_AID")
                                        strEMV_AID2 = _array2ICOR2[1].ToString().Trim();
                                    else if (_array2ICOR2[0].ToUpper().Trim().ToString() == "EMV_APP_LABEL")
                                        strEMV_APP_LABEL2 = _array2ICOR2[1].ToString().Trim();
                                    else if (_array2ICOR2[0].ToUpper().Trim().ToString() == "EMV_TVR")
                                        strEMV_TVR2 = _array2ICOR2[1].ToString().Trim();
                                    else if (_array2ICOR2[0].ToUpper().Trim().ToString() == "EMV_TLV")
                                        strEMV_TLV2 = _array2ICOR2[1].ToString().Trim();
                                    else if (_array2ICOR2[0].ToUpper().Trim().ToString() == "TRANSACTION_INFO")
                                        strTRANSACTION_INFO2 = _array2ICOR2[1].ToString().Trim();
                                }
                                dataJson3 dtrespo = new dataJson3
                                {
                                    RESPONSE = strResponse2,
                                    RETURN = strReturn2,
                                    CARDHOLDER_NAME = strCARDHOLDER_NAME2,
                                    EMV_ARQC = strEMV_ARQC2,
                                    EMV_TC = strEMV_TC2,
                                    EMV_AID = strEMV_AID2,
                                    EMV_APP_LABEL = strEMV_APP_LABEL2,
                                    EMV_TVR = strEMV_TVR2,
                                    EMV_TLV = strEMV_TLV2,
                                    TRANSACTION_INFO = strTRANSACTION_INFO2
                                };
                                #endregion
                                string responseic1 = JsonConvert.SerializeObject(dtrespo);
                                //-------variables del JSON QUE SE ENVIA DESERIALIZADO para impresion 
                                retorno = "[CODE_023]" + sResponsePCRest_sResult;
                                log1 = "CODE_023";
                                log2 = "Transaccion Exitosa";
                                exec.GeneraLogInfo(log1, log2);


                                log1 = "CODE_Fin";
                                log2 = "La transaccion fue aprovada con exito, en el metodo IC1";
                                exec.GeneraLogInfo(log1, log2);


                                #region deserializacion
                                //primera deserializacion metodo request ic0 REQUEST
                                var deserializeJsonrequestic0 = JsonConvert.DeserializeObject<dataJson>(res_ic0RES);
                                PAN = deserializeJsonrequestic0.PAN.ToString();
                                //primera deserializacion metodo request ic1
                                var deserializeJsonrequest = JsonConvert.DeserializeObject<dataJson2request>(res_ic1);
                                AUTHORIZACION = deserializeJsonrequest.AUTHORIZATION.ToString();
                                AFILIATION = deserializeJsonrequest.AFILIATION.ToString();
                                NUMSEQ = deserializeJsonrequest.NUMSEQ.ToString();
                                FOLIO = deserializeJsonrequest.FOLIO.ToString();
                                //primera deserializacion metodo response ic1
                                var deserializeJsonresponse = JsonConvert.DeserializeObject<dataJson3response>(responseic1);
                                CARDHOLDER_NAME = deserializeJsonresponse.CARDHOLDER_NAME.ToString();
                                EMV_APP_LABEL = deserializeJsonresponse.EMV_APP_LABEL.ToString();
                                EMV_AID = deserializeJsonresponse.EMV_AID.ToString();
                                ARQC = deserializeJsonresponse.EMV_ARQC.ToString();
                                #endregion
                            }

                            else
                            {


                                string Res_Time = Metodo_Reversal(resic0, "api/SaleResult", "|REV_TYPE:0");
                                var deserializeJson_reversal = JsonConvert.DeserializeObject<Consumo_apis_reversal>(Res_Time);
                                bool bandera_rever = deserializeJson_reversal.Bandera_rever;
                                string sResult_rever = deserializeJson_reversal.sResult_rever.ToString();

                                if (bandera_rever == true)
                                {


                                    //retorno = "Se cancelaron los cargos a la tarjeta" + sResult_rever;
                                    retorno = "[CODE_024] Se cancelaron los cargos a la tarjeta";
                                    No_AutorizaReversal = "";
                                    Desc_Reversal = "Se cancelaron los cargos a la tarjeta";
                                    No_TarjetaReversal = No_TarjetaReversal_s;
                                    Bandera_Reversal = true;

                                    log1 = "CODE_024";
                                    log2 = "Se cancelaron los cargos a la tarjeta";
                                    exec.GeneraLogInfo(log1, log2);
                                    log1 = "CODE_Fin";
                                    log2 = "Finalizo despues de un reversal de TYPE:0 en el metodo IC1";
                                    exec.GeneraLogInfo(log1, log2);


                                }
                                else
                                {
                                    #region Reversal REQUEST
                                    //empieza request ic1

                                    //se agrega para la impresion ic1 request
                                    string strTemp = sResult_rever;

                                    String RESPONSE_CODE3 = String.Empty;
                                    String AUTHORIZATIONCODE = String.Empty;
                                    string[] _arrayTmpr3;
                                    _arrayTmpr3 = new string[11];
                                    strTemp = strTemp.Substring(5, strTemp.Length - 5);
                                    string[] _array1ICOR3 = strTemp.Split('|');
                                    for (int i = 0; i < _array1ICOR3.Length; i++)
                                    {
                                        string[] _array2ICOR3 = _array1ICOR3[i].Split(':');
                                        if (_array2ICOR3[0].ToUpper().Trim().ToString() == "RESPONSE_CODE")
                                            RESPONSE_CODE3 = _array2ICOR3[1].ToString().Trim();
                                        else if (_array2ICOR3[0].ToUpper().Trim().ToString() == "AUTHORIZATION")
                                            AUTHORIZATIONCODE = _array2ICOR3[1].ToString().Trim();
                                    }
                                    dataJson2 dtic1r = new dataJson2
                                    {
                                        RESPONSE_CODE = RESPONSE_CODE3,
                                        AUTHORIZATION = AUTHORIZATIONCODE
                                    };

                                    string res_ic1 = JsonConvert.SerializeObject(dtic1r);

                                    //-----------------------------------------
                                    var deserializeJsonrequest = JsonConvert.DeserializeObject<dataJson2request>(res_ic1);
                                    string Response_Reversal = deserializeJsonrequest.RESPONSE_CODE.ToString();
                                    #endregion

                                    if (Response_Reversal == "07")
                                    {
                                        retorno = "[CODE_025] No se aplicaron cargos a la tarjeta";
                                        No_AutorizaReversal = "";
                                        Desc_Reversal = "No se aplicaron cargos a la tarjeta";
                                        No_TarjetaReversal = No_TarjetaReversal_s;
                                        Bandera_Reversal = true;

                                        log1 = "CODE_025";
                                        log2 = "No se aplicaron cargos a la tarjeta";
                                        exec.GeneraLogInfo(log1, log2);
                                        log1 = "CODE_Fin";
                                        log2 = "Finalizo despues de un reversal de TYPE:0 en el metodo IC1";
                                        exec.GeneraLogInfo(log1, log2);



                                    }

                                    else
                                    {
                                        retorno = "[CODE_026] Favor de llamar a su banco, para verificar el estado de la transaccion";
                                        No_AutorizaReversal = "";
                                        Desc_Reversal = "Favor de llamar a su banco, para verificar el estado de la transaccion";
                                        No_TarjetaReversal = No_TarjetaReversal_s;
                                        Bandera_Reversal = true;

                                        log1 = "CODE_026";
                                        log2 = retorno;
                                        exec.GeneraLogInfo(log1, log2);
                                        log1 = "CODE_Fin";
                                        log2 = "Finalizo despues de un reversal de TYPE:0 en el metodo IC1";
                                        exec.GeneraLogInfo(log1, log2);
                                    }



                                }

                            }

                        }
                        else
                        {



                            string Res_Time = Metodo_Reversal(resic0, "api/SaleResult", "|REV_TYPE:0");
                            var deserializeJson_reversal = JsonConvert.DeserializeObject<Consumo_apis_reversal>(Res_Time);
                            bool bandera_rever = deserializeJson_reversal.Bandera_rever;
                            string sResult_rever = deserializeJson_reversal.sResult_rever.ToString();

                            if (bandera_rever == true)
                            {


                                //retorno = "Se cancelaron los cargos a la tarjeta" + sResult_rever;
                                retorno = "[CODE_027] Se cancelaron los cargos a la tarjeta";
                                No_AutorizaReversal = "";
                                Desc_Reversal = "Se cancelaron los cargos a la tarjeta";
                                No_TarjetaReversal = No_TarjetaReversal_s;
                                Bandera_Reversal = true;

                                log1 = "CODE_027";
                                log2 = "Se cancelaron los cargos a la tarjeta";
                                exec.GeneraLogInfo(log1, log2);
                                log1 = "CODE_Fin";
                                log2 = "Finalizo despues de un reversal de TYPE:0 en el metodo IC1";
                                exec.GeneraLogInfo(log1, log2);


                            }
                            else
                            {
                                #region Reversal REQUEST
                                //empieza request ic1

                                //se agrega para la impresion ic1 request
                                string strTemp = sResult_rever;

                                String RESPONSE_CODE3 = String.Empty;
                                String AUTHORIZATIONCODE = String.Empty;
                                string[] _arrayTmpr3;
                                _arrayTmpr3 = new string[11];
                                strTemp = strTemp.Substring(5, strTemp.Length - 5);
                                string[] _array1ICOR3 = strTemp.Split('|');
                                for (int i = 0; i < _array1ICOR3.Length; i++)
                                {
                                    string[] _array2ICOR3 = _array1ICOR3[i].Split(':');
                                    if (_array2ICOR3[0].ToUpper().Trim().ToString() == "RESPONSE_CODE")
                                        RESPONSE_CODE3 = _array2ICOR3[1].ToString().Trim();
                                    else if (_array2ICOR3[0].ToUpper().Trim().ToString() == "AUTHORIZATION")
                                        AUTHORIZATIONCODE = _array2ICOR3[1].ToString().Trim();
                                }
                                dataJson2 dtic1r = new dataJson2
                                {
                                    RESPONSE_CODE = RESPONSE_CODE3,
                                    AUTHORIZATION = AUTHORIZATIONCODE
                                };

                                string res_ic1 = JsonConvert.SerializeObject(dtic1r);

                                //-----------------------------------------
                                var deserializeJsonrequest = JsonConvert.DeserializeObject<dataJson2request>(res_ic1);
                                string Response_Reversal = deserializeJsonrequest.RESPONSE_CODE.ToString();
                                #endregion

                                if (Response_Reversal == "07")
                                {
                                    retorno = "[CODE_028] No se aplicaron cargos a la tarjeta";
                                    No_AutorizaReversal = "";
                                    Desc_Reversal = "No se aplicaron cargos a la tarjeta";
                                    No_TarjetaReversal = No_TarjetaReversal_s;
                                    Bandera_Reversal = true;

                                    log1 = "CODE_028";
                                    log2 = "No se aplicaron cargos a la tarjeta";
                                    exec.GeneraLogInfo(log1, log2);
                                    log1 = "CODE_Fin";
                                    log2 = "Finalizo despues de un reversal de TYPE:0 en el metodo IC1";
                                    exec.GeneraLogInfo(log1, log2);



                                }

                                else
                                {
                                    retorno = "[CODE_029] Favor de llamar a su banco, para verificar el estado de la transaccion";
                                    No_AutorizaReversal = "";
                                    Desc_Reversal = "Favor de llamar a su banco, para verificar el estado de la transaccion";
                                    No_TarjetaReversal = No_TarjetaReversal_s;
                                    Bandera_Reversal = true;

                                    log1 = "CODE_029";
                                    log2 = retorno;
                                    exec.GeneraLogInfo(log1, log2);
                                    log1 = "CODE_Fin";
                                    log2 = "Finalizo despues de un reversal de TYPE:0 en el metodo IC1";
                                    exec.GeneraLogInfo(log1, log2);
                                }



                            }



                        }
                    }
                    else
                    {
                        string codigo_errorp = iStatus.ToString("00");
                        string desc_cerror = Code_convert(codigo_errorp);


                        log1 = "CODE_MOTIVO_REVERSAL";
                        log2 = "Transaccion cancelada: [" + codigo_errorp + "] " + desc_cerror;
                        exec.GeneraLogInfo(log1, log2);

                        string Res_Time = Metodo_Reversal(resic0, "api/SaleResult", "|REV_TYPE:2");
                        var deserializeJson_reversal = JsonConvert.DeserializeObject<Consumo_apis_reversal>(Res_Time);
                        bool bandera_rever = deserializeJson_reversal.Bandera_rever;
                        string sResult_rever = deserializeJson_reversal.sResult_rever.ToString();

                        if (bandera_rever == true)
                        {


                            //retorno = "Se cancelaron los cargos a la tarjeta" + sResult_rever;
                            retorno = "[CODE_030] Se cancelaron los cargos a la tarjeta";
                            No_AutorizaReversal = "";
                            Desc_Reversal = "Se cancelaron los cargos a la tarjeta";
                            No_TarjetaReversal = No_TarjetaReversal_s;
                            Bandera_Reversal = true;

                            log1 = "CODE_030";
                            log2 = "Se cancelaron los cargos a la tarjeta";
                            exec.GeneraLogInfo(log1, log2);
                            log1 = "CODE_Fin";
                            log2 = "Finalizo despues de un reversal de TYPE:2 en el metodo IC1";
                            exec.GeneraLogInfo(log1, log2);


                        }
                        else
                        {
                            #region Reversal REQUEST
                            //empieza request ic1

                            //se agrega para la impresion ic1 request
                            string strTemp = sResult_rever;

                            String RESPONSE_CODE3 = String.Empty;
                            String AUTHORIZATIONCODE = String.Empty;
                            string[] _arrayTmpr3;
                            _arrayTmpr3 = new string[11];
                            strTemp = strTemp.Substring(5, strTemp.Length - 5);
                            string[] _array1ICOR3 = strTemp.Split('|');
                            for (int i = 0; i < _array1ICOR3.Length; i++)
                            {
                                string[] _array2ICOR3 = _array1ICOR3[i].Split(':');
                                if (_array2ICOR3[0].ToUpper().Trim().ToString() == "RESPONSE_CODE")
                                    RESPONSE_CODE3 = _array2ICOR3[1].ToString().Trim();
                                else if (_array2ICOR3[0].ToUpper().Trim().ToString() == "AUTHORIZATION")
                                    AUTHORIZATIONCODE = _array2ICOR3[1].ToString().Trim();
                            }
                            dataJson2 dtic1r = new dataJson2
                            {
                                RESPONSE_CODE = RESPONSE_CODE3,
                                AUTHORIZATION = AUTHORIZATIONCODE
                            };

                            string res_ic1 = JsonConvert.SerializeObject(dtic1r);

                            //-----------------------------------------
                            var deserializeJsonrequest = JsonConvert.DeserializeObject<dataJson2request>(res_ic1);
                            string Response_Reversal = deserializeJsonrequest.RESPONSE_CODE.ToString();
                            #endregion

                            if (Response_Reversal == "07")
                            {
                                retorno = "[CODE_031] No se aplicaron cargos a la tarjeta";
                                No_AutorizaReversal = "";
                                Desc_Reversal = "No se aplicaron cargos a la tarjeta";
                                No_TarjetaReversal = No_TarjetaReversal_s;
                                Bandera_Reversal = true;

                                log1 = "CODE_031";
                                log2 = "No se aplicaron cargos a la tarjeta";
                                exec.GeneraLogInfo(log1, log2);
                                log1 = "CODE_Fin";
                                log2 = "Finalizo despues de un reversal de TYPE:2 en el metodo IC1";
                                exec.GeneraLogInfo(log1, log2);



                            }

                            else
                            {
                                retorno = "[CODE_32] Favor de llamar a su banco, para verificar el estado de la transaccion";
                                No_AutorizaReversal = "";
                                Desc_Reversal = "Favor de llamar a su banco, para verificar el estado de la transaccion";
                                No_TarjetaReversal = No_TarjetaReversal_s;
                                Bandera_Reversal = true;

                                log1 = "CODE_032";
                                log2 = retorno;
                                exec.GeneraLogInfo(log1, log2);
                                log1 = "CODE_Fin";
                                log2 = "Finalizo despues de un reversal de TYPE:2 en el metodo IC1";
                                exec.GeneraLogInfo(log1, log2);
                            }



                        }


                        //retorno = "Transacción Cancelada: [" + iStatus.ToString("00") + "] " + desc_cerror;
                        //retorno = "Transacción Cancelada: " + iStatus.ToString("00");
                    }



                }
            }
            catch (Exception ex)
            {
                retorno = "[CODE_019] No se pudo obtener comunicacion con el socket";
                log1 = "CODE_019";
                exec.GeneraLogInfo(log1, retorno);
                log1 = "CODE_EXCEPCION_IC1";
                log2 = "Se lanzo una excepcion de comunicacion con el socket, en el metodo IC1";
                exec.GeneraLogInfo(log1, log2);

                string Res_Time = Metodo_Reversal(resic0, "api/SaleResult", "|REV_TYPE:0");
                var deserializeJson_reversal = JsonConvert.DeserializeObject<Consumo_apis_reversal>(Res_Time);
                bool bandera_rever = deserializeJson_reversal.Bandera_rever;
                string sResult_rever = deserializeJson_reversal.sResult_rever.ToString();

                if (bandera_rever == true)
                {


                    //retorno = "Se cancelaron los cargos a la tarjeta" + sResult_rever;
                    retorno = "[CODE_036] Se cancelaron los cargos a la tarjeta";
                    No_AutorizaReversal = "";
                    Desc_Reversal = "Se cancelaron los cargos a la tarjeta";
                    No_TarjetaReversal = No_TarjetaReversal_s;
                    Bandera_Reversal = true;

                    log1 = "CODE_036";
                    log2 = "Se cancelaron los cargos a la tarjeta";
                    exec.GeneraLogInfo(log1, log2);
                    log1 = "CODE_Fin";
                    log2 = "Finalizo despues de un reversal de TYPE:0 en el metodo IC1";
                    exec.GeneraLogInfo(log1, log2);


                }
                else
                {
                    #region Reversal REQUEST
                    //empieza request ic1

                    //se agrega para la impresion ic1 request
                    string strTemp = sResult_rever;

                    String RESPONSE_CODE3 = String.Empty;
                    String AUTHORIZATIONCODE = String.Empty;
                    string[] _arrayTmpr3;
                    _arrayTmpr3 = new string[11];
                    strTemp = strTemp.Substring(5, strTemp.Length - 5);
                    string[] _array1ICOR3 = strTemp.Split('|');
                    for (int i = 0; i < _array1ICOR3.Length; i++)
                    {
                        string[] _array2ICOR3 = _array1ICOR3[i].Split(':');
                        if (_array2ICOR3[0].ToUpper().Trim().ToString() == "RESPONSE_CODE")
                            RESPONSE_CODE3 = _array2ICOR3[1].ToString().Trim();
                        else if (_array2ICOR3[0].ToUpper().Trim().ToString() == "AUTHORIZATION")
                            AUTHORIZATIONCODE = _array2ICOR3[1].ToString().Trim();
                    }
                    dataJson2 dtic1r = new dataJson2
                    {
                        RESPONSE_CODE = RESPONSE_CODE3,
                        AUTHORIZATION = AUTHORIZATIONCODE
                    };

                    string res_ic1 = JsonConvert.SerializeObject(dtic1r);

                    //-----------------------------------------
                    var deserializeJsonrequest = JsonConvert.DeserializeObject<dataJson2request>(res_ic1);
                    string Response_Reversal = deserializeJsonrequest.RESPONSE_CODE.ToString();
                    #endregion

                    if (Response_Reversal == "07")
                    {
                        retorno = "[CODE_037] No se aplicaron cargos a la tarjeta";
                        No_AutorizaReversal = "";
                        Desc_Reversal = "No se aplicaron cargos a la tarjeta";
                        No_TarjetaReversal = No_TarjetaReversal_s;
                        Bandera_Reversal = true;

                        log1 = "CODE_037";
                        log2 = "No se aplicaron cargos a la tarjeta";
                        exec.GeneraLogInfo(log1, log2);
                        log1 = "CODE_Fin";
                        log2 = "Finalizo despues de un reversal de TYPE:0 en el metodo IC1";
                        exec.GeneraLogInfo(log1, log2);



                    }

                    else
                    {
                        retorno = "[CODE_38] Favor de llamar a su banco, para verificar el estado de la transaccion";
                        No_AutorizaReversal = "";
                        Desc_Reversal = "Favor de llamar a su banco, para verificar el estado de la transaccion";
                        No_TarjetaReversal = No_TarjetaReversal_s;
                        Bandera_Reversal = true;

                        log1 = "CODE_038";
                        log2 = retorno;
                        exec.GeneraLogInfo(log1, log2);
                        log1 = "CODE_Fin";
                        log2 = "Finalizo despues de un reversal de TYPE:0 en el metodo IC1";
                        exec.GeneraLogInfo(log1, log2);
                    }



                }

            }
            closePCLSocket();

            var Obj_Reversal = new Reversal
            {
                Bandera_Reversal = Bandera_Reversal,
                Desc_Reversal = Desc_Reversal,
                No_AutorizaReversal = No_AutorizaReversal,
                No_TarjetaReversal = No_TarjetaReversal
            };

            Reversal = JsonConvert.SerializeObject(Obj_Reversal);

            var salida = new envioimpresion
            {
                AFILIATION = AFILIATION,
                ARQC = ARQC,
                AUTHORIZACION = AUTHORIZACION,
                CARDHOLDER_NAME = CARDHOLDER_NAME,
                EMV_APP_LABEL = EMV_APP_LABEL,
                NUMSEQ = NUMSEQ,
                PAN = PAN,
                retorno = retorno,
                EMV_AID = EMV_AID,
                FOLIO = FOLIO,
                Reversal = Reversal

            };


            string ic1respuesta = JsonConvert.SerializeObject(salida);

            return ic1respuesta;
        }

        private string Metodo_Reversal(string newdataRec, string Uriapiini, string tiporeversal)
        {

            bool bandera_rever = false;
            int contador_rever = 1;
            string sResponsePCRest_sResult = "";

            while (bandera_rever == false && contador_rever <= 3)
            {

                //string newdataRecm = newdataRec.Replace("|RESPONSE:IC0", "|RESPONSE:VO1");
                //Random ss2 = new Random();
                //int ran2 = ss2.Next(10000, 99999);
                //string hora2 = DateTime.Now.ToString("yyyyMMddHHmmss");
                //int pos = newdataRecm.IndexOf("|POSTTIMESTAMP:");
                //pos = pos + 15;
                //string replace = hora2 + "_" + Convert.ToString(ran2);
                //string newdataRec2 = newdataRecm.Substring(0, pos) + replace + newdataRecm.Substring(pos + 20);
                //newdataRec2 = newdataRec2 + tiporeversal;

                string newdataRecm = newdataRec.Replace("|RESPONSE:IC0", "|RESPONSE:VO1");
                string newdataRec2 = newdataRecm + tiporeversal;

                string metodo_reverso = SendDataRestToPC_Reversal(newdataRec2, "api/VoidRequest", Uriapiini);

                var deserializeJson = JsonConvert.DeserializeObject<Consumo_apis>(metodo_reverso);
                sResponsePCRest_sResult = deserializeJson.sResult.ToString();
                bandera_rever = deserializeJson.Bandera_IC0_SR;
                bool sResponsePCRest_Bandera_Time = deserializeJson.Bandera_Time;

                contador_rever++;
            }

            var retorno = new Consumo_apis_reversal
            {
                Bandera_rever = bandera_rever,
                sResult_rever = sResponsePCRest_sResult
            };

            string retorno_reverse = JsonConvert.SerializeObject(retorno);

            return retorno_reverse;
        }

        private string SendDataRestToPC_Reversal(string sData, string sPrefix, string metodoss)
        {
            //string text = System.IO.File.ReadAllText(@"C:\HubblePOS\Logs\TIMEOUTREVERSO.txt");
            int timeoutper = Convert.ToInt32(Asigna());

            //string log1 = metodoss + " en " + sPrefix;
            string urimetodo = metodoss;
            string log1 = "";
            string log2 = "";
            string sResult = "";
            bool Bandera_IC0_SR;
            string Time_Ini = "";
            string Time_Fin = "";
            string Fecha = "";
            bool Bandera_Time = false;
            //trama borrar
            string logs1 = "CODE_TRAMA_REVERSO";
            string logs2 = "Trama enviada a la api/Reversal: " + sData;
            exec.GeneraLogInfo(logs1, logs2);

            try
            {

                //string sUriBase = "http://localhost:10298";
                //string sUriBase = "https://gatewaypay.puntoclave.mx/";//produccion
                //string sUriBase = "http://test.puntoclave.com.mx/gatewaypayweb/";//desarrollo
                string sUriBase = "http://gatewayweb.qa.puntoclave.mx/";//qa

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                System.Net.Http.FormUrlEncodedContent formUrlEncodedContent = new System.Net.Http.FormUrlEncodedContent(new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>(string.Empty, sData) });
                System.Net.Http.HttpResponseMessage response = null;


                //Thread.Sleep(5000); por si se vuelve a agregar

                using (System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient())
                {
                    //httpClient.Timeout = TimeSpan.FromSeconds(25); timeoutper
                    httpClient.Timeout = TimeSpan.FromSeconds(timeoutper);
                    httpClient.BaseAddress = new Uri(sUriBase);
                    httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    Time_Ini = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");

                    response = httpClient.PostAsync(sPrefix, formUrlEncodedContent).Result;
                    Time_Fin = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                    Fecha = "Hora inicio: " + Time_Ini + " Hora de termino: " + Time_Fin;
                }

                int respuesta = Convert.ToInt32(response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    if (urimetodo == "api/SaleRequest")
                    {
                        log1 = "CODE_007";
                        log2 = "Consumo de api/Reversal Exitoso: " + Fecha;
                    }
                    else
                    {
                        log1 = "CODE_033";
                        log2 = "Consumo de api/Reversal Exitoso: " + Fecha;
                    }

                    sResult = response.Content.ReadAsStringAsync().Result;

                    #region Reversal REQUEST
                    //empieza request ic1

                    //se agrega para la impresion ic1 request
                    string strTemp = sResult;

                    String RESPONSE_CODE3 = String.Empty;
                    String AUTHORIZATIONCODE = String.Empty;
                    string[] _arrayTmpr3;
                    _arrayTmpr3 = new string[11];
                    strTemp = strTemp.Substring(5, strTemp.Length - 5);
                    string[] _array1ICOR3 = strTemp.Split('|');
                    for (int i = 0; i < _array1ICOR3.Length; i++)
                    {
                        string[] _array2ICOR3 = _array1ICOR3[i].Split(':');
                        if (_array2ICOR3[0].ToUpper().Trim().ToString() == "RESPONSE_CODE")
                            RESPONSE_CODE3 = _array2ICOR3[1].ToString().Trim();
                    }
                    dataJson2 dtic1r = new dataJson2
                    {
                        RESPONSE_CODE = RESPONSE_CODE3
                    };

                    string res_ic1 = JsonConvert.SerializeObject(dtic1r);

                    //-----------------------------------------
                    var deserializeJsonrequest = JsonConvert.DeserializeObject<dataJson2request>(res_ic1);
                    string Response_Reversal = deserializeJsonrequest.RESPONSE_CODE.ToString();
                    #endregion

                    if (Response_Reversal == "00")
                    {
                        Bandera_IC0_SR = true;
                    }
                    //else if(Response_Reversal == "04")
                    //{
                    //    Bandera_IC0_SR = true;
                    //}
                    else
                    {
                        Bandera_IC0_SR = false;
                    }




                    //log2 = " Transaccion correcta: " + Fecha;

                    string log11 = "CODE_RESPONSE_REVERSAL";
                    string log12 = sResult;
                    exec.GeneraLogInfo(log11, "Respuesta de la api/Reversal: " + log12);

                    exec.GeneraLogInfo(log1, log2);
                }
                else
                {
                    sResult = response.Content.ReadAsStringAsync().Result;
                    Bandera_IC0_SR = false;
                    if (urimetodo == "api/SaleRequest")
                    {
                        log1 = "CODE_008";
                        log2 = "Consumo de api/Reversal Fallido: " + respuesta + " " + sResult + Fecha;
                    }
                    else
                    {
                        log1 = "CODE_034";
                        log2 = "Consumo de api/Reversal Fallido: " + respuesta + " " + sResult + Fecha;
                    }
                    //log2 = "Error: " + respuesta + " al consumir el servicio " + " Result:" + sResult + " " + Fecha;
                    exec.GeneraLogInfo(log1, log2);
                }
            }
            catch (Exception ex)
            {
                string Time_ex = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                string tareacancelada = ex.InnerException.Message.ToString();
                string ErrorLine = ex.ToString();

                if (urimetodo == "api/SaleRequest")
                {
                    log1 = "CODE_009";
                    log2 = "Se lanzo una exepcion al consumir el servicio api/Reversal : " + "Exeption Line:" + ErrorLine + " Exeption Description:" + tareacancelada + " Hora Inicio: " + Time_Ini + " Hora Fin: " + Time_ex;
                }
                else
                {
                    log1 = "CODE_035";
                    log2 = "Se lanzo una exepcion al consumir el servicio api/Reversal:  " + "Exeption Line:" + ErrorLine + " Exeption Description:" + tareacancelada + " Hora Inicio: " + Time_Ini + " Hora Fin: " + Time_ex;

                }

                if (tareacancelada == "Se canceló una tarea.")
                {
                    sResult = tareacancelada;
                    Bandera_Time = true;
                    // string log2 = "Exeption Line:" + ErrorLine + " Exeption Description:" + tareacancelada + " Hora Inicio: " + Time_Ini + " Hora Fin: " + Time_ex;

                }
                else
                {
                    sResult = tareacancelada;
                    Bandera_Time = false;
                    //logs

                    //string log2 = "Exeption Line:" + ErrorLine + " Exeption Description:" + tareacancelada + " Hora Inicio: " + Time_Ini + " Hora Fin: " + Time_ex;
                    //exec.GeneraLogInfo(log1, log2);
                }
                exec.GeneraLogInfo(log1, log2);
                Bandera_IC0_SR = false;



            }

            Consumo_apis consumoapi = new Consumo_apis
            {
                sResult = sResult,
                Bandera_IC0_SR = Bandera_IC0_SR,
                Bandera_Time = Bandera_Time
            };

            string R_consumoapi = JsonConvert.SerializeObject(consumoapi);

            return R_consumoapi;
        }
        private string SendDataRestToPC(string sData, string sPrefix)
        {
            //string text = System.IO.File.ReadAllText(@"C:\HubblePOS\Logs\TIMEOUT.txt");
            int timeoutper = Convert.ToInt32(Asigna());
            string urimetodo = sPrefix;
            string log1 = "";
            string log2 = "";
            string sResult = "";
            bool Bandera_IC0_SR;
            string Time_Ini = "";
            string Time_Fin = "";
            string Fecha = "";
            bool Bandera_Time = false;




            if (urimetodo == "api/SaleRequest")
            {
                exec.GeneraLogInfo("CODE_TRAMA_IC0", "Request enviado a api/SaleRequest: " + sData);
                log1 = "api/SaleRequest";
            }
            else
            {
                exec.GeneraLogInfo("CODE_TRAMA_IC1", "Request enviado a api/SaleResult: " + sData);
                log1 = "api/SaleResult";
            }

            try
            {

                //string sUriBase = "http://localhost:10298";
                //string sUriBase = "https://gatewaypay.puntoclave.mx/";//produccion
                //string sUriBase = "http://test.puntoclave.com.mx/gatewaypayweb/";//desarrollo
                string sUriBase = "http://gatewayweb.qa.puntoclave.mx/";//qa

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                System.Net.Http.FormUrlEncodedContent formUrlEncodedContent = new System.Net.Http.FormUrlEncodedContent(new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>(string.Empty, sData) });
                System.Net.Http.HttpResponseMessage response = null;

                using (System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient())
                {
                    //httpClient.Timeout = TimeSpan.FromSeconds(5);timeoutper
                    //httpClient.Timeout = TimeSpan.FromSeconds(timeoutper);
                    httpClient.Timeout = TimeSpan.FromSeconds(timeoutper);
                    httpClient.BaseAddress = new Uri(sUriBase);
                    httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    Time_Ini = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                    response = httpClient.PostAsync(sPrefix, formUrlEncodedContent).Result;
                    Time_Fin = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                    Fecha = "Hora inicio: " + Time_Ini + " Hora de termino: " + Time_Fin;
                }

                int respuesta = Convert.ToInt32(response.StatusCode);

                if (response.IsSuccessStatusCode)
                {

                    string sResultini = response.Content.ReadAsStringAsync().Result;
                    string sResult_com = "";



                    //termina deserializacion
                    if (urimetodo == "api/SaleRequest")
                    {
                        #region IC1 REQUEST
                        //empieza request ic1

                        //se agrega para la impresion ic1 request
                        string strTemp = sResultini;

                        String RESPONSE_CODE3 = String.Empty;

                        string[] _arrayTmpr3;
                        _arrayTmpr3 = new string[11];
                        strTemp = strTemp.Substring(5, strTemp.Length - 5);
                        string[] _array1ICOR3 = strTemp.Split('|');
                        for (int i = 0; i < _array1ICOR3.Length; i++)
                        {
                            string[] _array2ICOR3 = _array1ICOR3[i].Split(':');
                            if (_array2ICOR3[0].ToUpper().Trim().ToString() == "RESPONSE_CODE")
                                RESPONSE_CODE3 = _array2ICOR3[1].ToString().Trim();
                        }
                        dataJson2 dtic1r = new dataJson2
                        {
                            RESPONSE_CODE = RESPONSE_CODE3
                        };
                        #endregion
                        string res_ic1 = JsonConvert.SerializeObject(dtic1r);

                        //-----------------------------------------
                        var deserializeJsonrequest = JsonConvert.DeserializeObject<dataJson2request>(res_ic1);
                        sResult_com = deserializeJsonrequest.RESPONSE_CODE.ToString();

                        log1 = "CODE_003";
                        log2 = " Consumo de api/SaleRequest Exitoso: " + Fecha;
                    }
                    else
                    {
                        log1 = "CODE_020";
                        log2 = " Consumo de api/SaleResult Exitoso: " + Fecha;
                    }


                    if (sResult_com == "04")
                    {
                        sResult = sResult_com;
                        Bandera_IC0_SR = false;
                        log1 = "CODE_004";
                        log2 = " Consumo de api/SaleRequest con code 04: " + Fecha;
                    }
                    else
                    {
                        sResult = sResultini;
                        Bandera_IC0_SR = true;
                    }

                    //log2 = " Transaccion correcta: " + Fecha;
                    exec.GeneraLogInfo(log1, log2);



                }
                else
                {
                    if (urimetodo == "api/SaleRequest")
                    {
                        log1 = "CODE_005";
                        log2 = " Consumo de api/SaleRequest fallido: Error" + respuesta + " " + Fecha;
                    }
                    else
                    {
                        log1 = "CODE_021";
                        log2 = " Consumo de api/SaleResult fallido: Error" + respuesta + " " + Fecha;
                    }

                    sResult = response.Content.ReadAsStringAsync().Result;
                    Bandera_IC0_SR = false;
                    //log2 = "Error: " + respuesta + " al consumir el servicio " + " Result:" + sResult + " " + Fecha;
                    exec.GeneraLogInfo(log1, log2);
                }
            }
            catch (Exception ex)
            {
                string Time_ex = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                string tareacancelada = ex.InnerException.Message.ToString();
                string ErrorLine = ex.ToString();

                if (urimetodo == "api/SaleRequest")
                {
                    log1 = "CODE_006";
                    log2 = "Se lanzo una exepcion al consumir api/SaleRequest: " + "Exeption Line:" + ErrorLine + " Exeption Description:" + tareacancelada + " Hora Inicio: " + Time_Ini + " Hora Fin: " + Time_ex; ;
                }
                else
                {
                    log1 = "CODE_022";
                    log2 = "Se lanzo una exepcion al consumir api/SaleResult: " + "Exeption Line:" + ErrorLine + " Exeption Description:" + tareacancelada + " Hora Inicio: " + Time_Ini + " Hora Fin: " + Time_ex; ;

                }

                if (tareacancelada == "Se canceló una tarea.")
                {
                    sResult = tareacancelada;
                    Bandera_Time = true;
                }
                else
                {
                    sResult = tareacancelada;
                    Bandera_Time = false;
                }
                exec.GeneraLogInfo(log1, log2);
                Bandera_IC0_SR = false;



            }

            Consumo_apis consumoapi = new Consumo_apis
            {
                sResult = sResult,
                Bandera_IC0_SR = Bandera_IC0_SR,
                Bandera_Time = Bandera_Time
            };

            string R_consumoapi = JsonConvert.SerializeObject(consumoapi);

            return R_consumoapi;
        }

        public async Task<string> IC0SaleMethods(Request_IC0 request)
        {

            string socket = JsonConvert.SerializeObject(request);
            var deserializeJson = JsonConvert.DeserializeObject<Request_IC0>(socket);
            string enlace = deserializeJson.enlacepinpad.ToString();
            string monto = deserializeJson.Monto.ToString();
            int autorizar = deserializeJson.autorizar;
            var_pinpad = deserializeJson.var_pinpad.ToString();
            var_timeout = deserializeJson.var_timeout.ToString();
            string IC0 = "";
            string IC1 = "";
            string IC0des = "";

            //se agrego para impresion
            string retorno = "VACIO";

            string AFILIATION = "";
            string ARQC = "";
            string AUTHORIZACION = "";
            string CARDHOLDER_NAME = "";
            string EMV_APP_LABEL = "";
            string NUMSEQ = "";
            string PAN = "";
            string EMV_AID = "";
            bool firma = false;
            string FOLIO = "";
            bool Bandera_IC0;

            //se cambio las siguientes variables
            //string descripcion = "";
            //bool respuesta = true;

            string descripcion = "No se pudo octener comunicacion con el pinpad";
            bool respuesta = false;

            OpenPCL(enlace);
            IC0des = MetodoIC0(monto);
            var deserializeJsonrequestIC0 = JsonConvert.DeserializeObject<respIC0>(IC0des);
            IC0 = deserializeJsonrequestIC0.retorno.ToString();
            Bandera_IC0 = deserializeJsonrequestIC0.Bandera_IC0;
            bool Bandera_IC0_autorizacion = deserializeJsonrequestIC0.Bandera_IC0_autorizacion;
            string ic0aic1 = deserializeJsonrequestIC0.IC0PAN.ToString();
            string Obj_Reversal = deserializeJsonrequestIC0.Reversal.ToString();
            firma = deserializeJsonrequestIC0.FIRMA;
            //reversal
            string No_AutorizaReversal = "";
            string Desc_Reversal = "";
            bool Bandera_Reversal = false;

            //if (IC0 != "No_se_pudo_obtener_comunicacion_con_el_socket" && IC0 != "Transaccion_Declinada")
            if (Bandera_IC0 == true)
            {
                //if (IC0 != "error_api" && IC0 != "ERROR_DESCONOCIDO")
                if (Bandera_IC0_autorizacion == true)
                {

                    respIC1 IC1entrada = new respIC1
                    {
                        resic0 = ic0aic1,
                        resic1 = IC0
                    };

                    string ENVIC1 = JsonConvert.SerializeObject(IC1entrada);

                    IC1 = MetodoIC1(ENVIC1);
                    //se agrego para deserializar el ic1 response
                    var deserializeJsonrequest = JsonConvert.DeserializeObject<envioimpresion>(IC1);

                    retorno = deserializeJsonrequest.retorno.ToString();
                    string ApiReversal = deserializeJsonrequest.Reversal.ToString();


                    //se termino de agregar
                    //if (retorno.Trim() == "" || retorno == String.Empty)
                    if (retorno == "[CODE_023]")
                    {
                        descripcion = retorno + " Transaccion Exitosa";
                        respuesta = true;

                        AFILIATION = deserializeJsonrequest.AFILIATION.ToString();
                        ARQC = deserializeJsonrequest.ARQC.ToString();
                        AUTHORIZACION = deserializeJsonrequest.AUTHORIZACION.ToString();
                        CARDHOLDER_NAME = deserializeJsonrequest.CARDHOLDER_NAME.ToString();
                        EMV_APP_LABEL = deserializeJsonrequest.EMV_APP_LABEL.ToString();
                        NUMSEQ = deserializeJsonrequest.NUMSEQ.ToString();
                        PAN = deserializeJsonrequest.PAN.ToString();
                        EMV_AID = deserializeJsonrequest.EMV_AID.ToString();
                        FOLIO = deserializeJsonrequest.FOLIO.ToString();


                    }
                    else
                    {
                        var deserializeJsonReversal2 = JsonConvert.DeserializeObject<Reversal>(ApiReversal);
                        No_AutorizaReversal = deserializeJsonReversal2.No_AutorizaReversal.ToString();
                        PAN = deserializeJsonReversal2.No_TarjetaReversal.ToString();
                        Desc_Reversal = deserializeJsonReversal2.Desc_Reversal.ToString();
                        Bandera_Reversal = deserializeJsonReversal2.Bandera_Reversal;
                        descripcion = retorno;
                        respuesta = false;
                    }

                    ClosePCL(enlace);

                }
                else
                {
                    //autorizar_separado(autorizar);
                    descripcion = IC0;
                    var deserializeJsonReversal = JsonConvert.DeserializeObject<Reversal>(Obj_Reversal);
                    No_AutorizaReversal = deserializeJsonReversal.No_AutorizaReversal.ToString();
                    PAN = deserializeJsonReversal.No_TarjetaReversal.ToString();
                    Desc_Reversal = deserializeJsonReversal.Desc_Reversal.ToString();
                    Bandera_Reversal = deserializeJsonReversal.Bandera_Reversal;
                    respuesta = false;
                }
            }
            else
            {
                descripcion = IC0;
                respuesta = false;
            }

            // ClosePCL(enlace);

            respuestapinpad respuestas = new respuestapinpad
            {
                descripcion = descripcion,
                respuesta = respuesta,
                AFILIATION = AFILIATION,
                ARQC = ARQC,
                PAN = PAN,
                AUTHORIZACION = AUTHORIZACION,
                CARDHOLDER_NAME = CARDHOLDER_NAME,
                EMV_APP_LABEL = EMV_APP_LABEL,
                NUMSEQ = NUMSEQ,
                EMV_AID = EMV_AID,
                //firma = firma,
                firma = true,
                FOLIO = FOLIO,
                Bandera_Reversal = Bandera_Reversal,
                Desc_Reversal = Desc_Reversal,
                No_AutorizaReversal = No_AutorizaReversal
            };
            string Respuestapinpad = JsonConvert.SerializeObject(respuestas);
            return Respuestapinpad;
        }

    }
}

