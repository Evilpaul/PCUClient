using System;
using System.Runtime.InteropServices;
using System.Threading;
using Peak.Can.Uds;

using TPCANHandle = System.UInt16;
using TPCANTimestampFD = System.UInt64;
using TPUDSCANHandle = System.UInt16;

namespace PCUClient
{
    public class Program
    {
        // a global counter to keep track of the number of failed tests (see displayMessage function)
        static int g_nbErr = 0;

        // CAN address information for this example
        private static readonly byte N_SA = ((byte)TPUDSAddress.PUDS_ISO_15765_4_ADDR_TEST_EQUIPMENT);
        private static readonly byte N_TA_PHYS = ((byte)TPUDSAddress.PUDS_ISO_15765_4_ADDR_ECU_1);
        private static readonly byte N_TA_FUNC = ((byte)TPUDSAddress.PUDS_ISO_15765_4_ADDR_OBD_FUNCTIONAL);
        private static readonly byte N_RA = ((byte)0x00);

        // console handling
        private static readonly bool USE_GETCH = false;

        // A simple function that waits for user input
        static void waitGetch(string pMsg = null)
        {
            if (USE_GETCH)
            {
                if (pMsg != null)
                    Console.Write($"\n {pMsg}\n Press <Enter> to continue...");
                else
                    Console.Write("\n Press <Enter> to continue...");
                Console.ReadKey(true);

                if (USE_GETCH) Console.Clear();
            }
        }

        // A function that displays UDS Request and Response messages (and count error if no response)
        static void displayMessage(TPUDSMsg Request, TPUDSMsg Response, bool noResponseExpected = false)
        {
            if (!Request.Equals(default(TPUDSMsg)))
            {
                string result = Request.RESULT != TPUDSResult.PUDS_RESULT_N_OK ? "ERROR !!!" : "OK!";
                Console.Write($"\nUDS request from 0x{Request.NETADDRINFO.SA:x2} (to 0x{Request.NETADDRINFO.TA:x2}, with RA 0x{Request.NETADDRINFO.RA:x2}) - result: {Request.RESULT} - {result}\n");
                // display data
                Console.Write($"\t\\-> Length: {Request.LEN}, Data= ");
                for (int i = 0; i < Request.LEN; i++)
                {
                    Console.Write($"{Request.DATA[i]:x2} ");
                }
            }
            if (!Response.Equals(default(TPUDSMsg)))
            {
                string result = Response.RESULT != TPUDSResult.PUDS_RESULT_N_OK ? "ERROR !!!" : "OK!";
                Console.Write($"\nUDS RESPONSE from 0x{Response.NETADDRINFO.SA:x2} (to 0x{Response.NETADDRINFO.TA:x2}, with RA 0x{Response.NETADDRINFO.RA:x2}) - result: {Response.RESULT} - {result}\n");
                // display data
                Console.Write($"\t\\-> Length: {Response.LEN}, Data= ");
                for (int i = 0; i < Response.LEN; i++)
                {
                    Console.Write($"{Response.DATA[i]:x2} ");
                }
                Console.Write("\n");
            }
            else if (!noResponseExpected)
            {
                Console.Write("\n /!\\ ERROR : NO UDS RESPONSE !!\n\n");
                g_nbErr++;
            }
        }

        // Inverts the bytes of a 32 bits numeric value
        //
        static uint Reverse32(uint v)
        {
            byte[] array = BitConverter.GetBytes(v);
            Array.Reverse(array);

            return BitConverter.ToUInt32(array, 0);
            //return ((v & 0x000000FF) << 24) | ((v & 0x0000FF00) << 8) | ((v & 0x00FF0000) >> 8) | ((v & 0xFF000000) >> 24);
        }

        // UDS Service DiagnosticSessionControl
        static void testDiagnosticSessionControl(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSSessionInfo lSessionInfo = new TPUDSSessionInfo();
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();

            // initialization
            Message.NETADDRINFO = N_AI;
            lSessionInfo.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: DiagnosticSessionControl ***\n");

            // Read default session information 
            //  Server is not yet known (Status will be PUDS_ERROR_NOT_INITIALIZED)
            //	yet the API will still set lSessionInfo to default values
            IntPtr iptr = Marshal.AllocHGlobal(Marshal.SizeOf(lSessionInfo));
            Marshal.StructureToPtr(lSessionInfo, iptr, false);
            Status = UDSApi.GetValue(Channel, TPUDSParameter.PUDS_PARAM_SESSION_INFO, iptr, (uint)Marshal.SizeOf(lSessionInfo));
            lSessionInfo = (TPUDSSessionInfo)Marshal.PtrToStructure(iptr, typeof(TPUDSSessionInfo));
            Console.Write($"  Diagnostic Session Information: {Status}, 0x{lSessionInfo.NETADDRINFO.TA:x2} => {lSessionInfo.SESSION_TYPE} = [{lSessionInfo.TIMEOUT_P2CAN_SERVER_MAX:x4}; {lSessionInfo.TIMEOUT_ENHANCED_P2CAN_SERVER_MAX:x4}]\n");
            waitGetch();

            // Set Diagnostic session to DEFAULT (to get session information)
            Console.Write("\n\nSetting a DEFAULT Diagnostic Session :\n");
            Status = UDSApi.SvcDiagnosticSessionControl(Channel, ref Message, UDSApi.TPUDSSvcParamDSC.PUDS_SVC_PARAM_DSC_DS);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcDiagnosticSessionControl: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            // Read current session information
            lSessionInfo = new TPUDSSessionInfo();
            lSessionInfo.NETADDRINFO = N_AI;
            iptr = Marshal.AllocHGlobal(Marshal.SizeOf(lSessionInfo));
            Marshal.StructureToPtr(lSessionInfo, iptr, false);
            Status = UDSApi.GetValue(Channel, TPUDSParameter.PUDS_PARAM_SESSION_INFO, iptr, (uint)Marshal.SizeOf(lSessionInfo));
            lSessionInfo = (TPUDSSessionInfo)Marshal.PtrToStructure(iptr, typeof(TPUDSSessionInfo));
            Console.Write($"  Diagnostic Session Information: {Status}, 0x{lSessionInfo.NETADDRINFO.TA:x2} => {lSessionInfo.SESSION_TYPE} = [{lSessionInfo.TIMEOUT_P2CAN_SERVER_MAX:x4}; {lSessionInfo.TIMEOUT_ENHANCED_P2CAN_SERVER_MAX:x4}]\n");
            waitGetch();

            // Set Diagnostic session to PROGRAMMING
            Console.Write("\n\nSetting a ECUPS Diagnostic Session :\n");
            Status = UDSApi.SvcDiagnosticSessionControl(Channel, ref Message, UDSApi.TPUDSSvcParamDSC.PUDS_SVC_PARAM_DSC_ECUPS);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcDiagnosticSessionControl: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            // Read current session information
            lSessionInfo = new TPUDSSessionInfo();
            lSessionInfo.NETADDRINFO = N_AI;
            iptr = Marshal.AllocHGlobal(Marshal.SizeOf(lSessionInfo));
            Marshal.StructureToPtr(lSessionInfo, iptr, false);
            Status = UDSApi.GetValue(Channel, TPUDSParameter.PUDS_PARAM_SESSION_INFO, iptr, (uint)Marshal.SizeOf(lSessionInfo));
            lSessionInfo = (TPUDSSessionInfo)Marshal.PtrToStructure(iptr, typeof(TPUDSSessionInfo));
            Console.Write($"  Diagnostic Session Information: {Status}, 0x{lSessionInfo.NETADDRINFO.TA:x2} => {lSessionInfo.SESSION_TYPE} = [{lSessionInfo.TIMEOUT_P2CAN_SERVER_MAX:x4}; {lSessionInfo.TIMEOUT_ENHANCED_P2CAN_SERVER_MAX:x4}]\n");
            Console.Write(" Assert that Auto TesterPresent Frame is sent...\n");
            Thread.Sleep(2000);
            Console.Write("  Should transmit an Auto TesterPresent Frame\n");
            Thread.Sleep(2000);
            Console.Write("  Should transmit an Auto TesterPresent Frame\n");

            waitGetch();
            // Set Diagnostic session back to DEFAULT
            Console.Write("\n\nSetting a DEFAULT Diagnostic Session :\n");
            Status = UDSApi.SvcDiagnosticSessionControl(Channel, ref Message, UDSApi.TPUDSSvcParamDSC.PUDS_SVC_PARAM_DSC_DS);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcDiagnosticSessionControl: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            Console.Write(" Assert that NO Auto TesterPresent Frame is sent...\n");
            Thread.Sleep(2000);
            Console.Write("  Should NOT transmit an Auto TesterPresent Frame\n");
            Thread.Sleep(2000);
            Console.Write("  Should NOT transmit an Auto TesterPresent Frame\n");
            waitGetch();
        }

        // UDS Service ECUReset
        static void testECUReset(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: ECUReset ***\n");

            // Sends a Physical ECUReset Message
            Console.Write("\n\nSends a Physical ECUReset Message: \n");
            Status = UDSApi.SvcECUReset(Channel, ref Message, UDSApi.TPUDSSvcParamER.PUDS_SVC_PARAM_ER_SR);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcECUReset: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service SecurityAccess
        static void testSecurityAccess(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            uint dwBuffer;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: SecurityAccess ***\n");

            // Sends a Physical SecurityAccess Message
            Console.Write("\n\nSends a Physical SecurityAccess Message: \n");
            uint valueLittleEndian = 0xF0A1B2C3;
            dwBuffer = Reverse32(valueLittleEndian);   // use helper function to set MSB as 1st byte in the buffer (Win32 uses little endian format)
            Status = UDSApi.SvcSecurityAccess(Channel, ref Message, UDSApi.PUDS_SVC_PARAM_SA_RSD_1, BitConverter.GetBytes(dwBuffer), (ushort)Marshal.SizeOf(dwBuffer));
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcSecurityAccess: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service CommunicationControl
        static void testCommunicationControl(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: CommunicationControl ***\n");

            // Sends a Physical CommunicationControl Message
            Console.Write("\n\nSends a Physical CommunicationControl Message: \n");
            Status = UDSApi.SvcCommunicationControl(Channel, ref Message, UDSApi.TPUDSSvcParamCC.PUDS_SVC_PARAM_CC_ERXTX,
                UDSApi.PUDS_SVC_PARAM_CC_FLAG_APPL | UDSApi.PUDS_SVC_PARAM_CC_FLAG_NWM | UDSApi.PUDS_SVC_PARAM_CC_FLAG_DENWRIRO);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcCommunicationControl: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service TesterPresent
        static void testTesterPresent(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: TesterPresent ***\n");

            // Sends a Physical TesterPresent Message
            Console.Write("\n\nSends a Physical TesterPresent Message: \n");
            Status = UDSApi.SvcTesterPresent(Channel, ref Message);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcTesterPresent: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();

            // Sends a Physical TesterPresent Message with no positive response
            Console.Write("\n\nSends a Physical TesterPresent Message with no positive response :\n");
            Message.NO_POSITIVE_RESPONSE_MSG = UDSApi.PUDS_SUPPR_POS_RSP_MSG_INDICATION_BIT;
            Status = UDSApi.SvcTesterPresent(Channel, ref Message);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcTesterPresent: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg(), true);
            waitGetch();

            // Sends a Functional TesterPresent Message
            Console.Write("\n\nSends a Functional TesterPresent Message: \n");
            Message.NETADDRINFO.TA = (byte)TPUDSAddress.PUDS_ISO_15765_4_ADDR_OBD_FUNCTIONAL;
            Message.NETADDRINFO.TA_TYPE = TPUDSAddressingType.PUDS_ADDRESSING_FUNCTIONAL;
            Message.NO_POSITIVE_RESPONSE_MSG = 0;
            Status = UDSApi.SvcTesterPresent(Channel, ref Message);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcTesterPresent: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();

            // Sends a Functional TesterPresent Message with no positive response
            Console.Write("\n\nSends a Functional TesterPresent Message with no positive response :\n");
            Message.NETADDRINFO.TA = (byte)TPUDSAddress.PUDS_ISO_15765_4_ADDR_OBD_FUNCTIONAL;
            Message.NETADDRINFO.TA_TYPE = TPUDSAddressingType.PUDS_ADDRESSING_FUNCTIONAL;
            Message.NO_POSITIVE_RESPONSE_MSG = UDSApi.PUDS_SUPPR_POS_RSP_MSG_INDICATION_BIT;
            Status = UDSApi.SvcTesterPresent(Channel, ref Message);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcTesterPresent: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg(), true);
            waitGetch();
        }

        // UDS Service SecuredDataTransmission
        static void testSecuredDataTransmission(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            uint dwBuffer;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: SecuredDataTransmission ***\n");

            // Sends a Physical SecuredDataTransmission Message
            Console.Write("\n\nSends a Physical SecuredDataTransmission Message: \n");
            uint valueLittleEndian = 0xF0A1B2C3;
            dwBuffer = Reverse32(valueLittleEndian);   // use helper function to set MSB as 1st byte in the buffer (Win32 uses little endian format)
            Status = UDSApi.SvcSecuredDataTransmission(Channel, ref Message, BitConverter.GetBytes(dwBuffer), (ushort)Marshal.SizeOf(dwBuffer));
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcSecuredDataTransmission: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service ControlDTCSetting
        static void testControlDTCSetting(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            uint dwBuffer;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: ControlDTCSetting ***\n");

            // Sends a Physical ControlDTCSetting Message
            Console.Write("\n\nSends a Physical ControlDTCSetting Message: \n");
            uint valueLittleEndian = 0xF1A1B2EE;
            dwBuffer = Reverse32(valueLittleEndian);   // use helper function to set MSB as 1st byte in the buffer (Win32 uses little endian format)
            Status = UDSApi.SvcControlDTCSetting(Channel, ref Message, UDSApi.TPUDSSvcParamCDTCS.PUDS_SVC_PARAM_CDTCS_OFF, BitConverter.GetBytes(dwBuffer), 3);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcControlDTCSetting: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service ResponseOnEvent
        static void testResponseOnEvent(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            byte[] lBuffer = new byte[50];
            byte[] lBuffer2 = new byte[50];
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: ResponseOnEvent ***\n");

            // Sends a Physical ResponseOnEvent Message
            Console.Write("\n\nSends a Physical ResponseOnEvent Message: \n");
            lBuffer[0] = 0x08;
            lBuffer2[0] = (byte)TPUDSService.PUDS_SI_ReadDTCInformation;
            lBuffer2[1] = (byte)UDSApi.TPUDSSvcParamRDTCI.PUDS_SVC_PARAM_RDTCI_RNODTCBSM;
            lBuffer2[2] = 0x01;
            Status = UDSApi.SvcResponseOnEvent(Channel, ref Message, UDSApi.TPUDSSvcParamROE.PUDS_SVC_PARAM_ROE_ONDTCS,
                false, 0x08, lBuffer, 1, lBuffer2, 3);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcResponseOnEvent: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service LinkControl
        static void testLinkControl(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: LinkControl ***\n");

            // Sends a Physical LinkControl Message
            Console.Write("\n\nSends a Physical LinkControl Message (Verify Fixed Baudrate): \n");
            Status = UDSApi.SvcLinkControl(Channel, ref Message, UDSApi.TPUDSSvcParamLC.PUDS_SVC_PARAM_LC_VBTWFBR, (byte)UDSApi.TPUDSSvcParamLCBaudrateIdentifier.PUDS_SVC_PARAM_LC_BAUDRATE_CAN_250K, 0);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcLinkControl: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());

            // Sends a Physical LinkControl Message
            Console.Write("\n\nSends a Physical LinkControl Message (Verify Specific Baudrate): \n");
            Status = UDSApi.SvcLinkControl(Channel, ref Message, UDSApi.TPUDSSvcParamLC.PUDS_SVC_PARAM_LC_VBTWSBR, 0, 500000);   // 500K = 0x0007a120
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcLinkControl: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());

            // Sends a Physical LinkControl Message
            Console.Write("\n\nSends a Physical LinkControl Message (Transition): \n");
            Status = UDSApi.SvcLinkControl(Channel, ref Message, UDSApi.TPUDSSvcParamLC.PUDS_SVC_PARAM_LC_TB, 0, 0);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcLinkControl: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());

            waitGetch();
        }

        // UDS Service ReadDataByIdentifier
        static void testReadDataByIdentifier(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: ReadDataByIdentifier ***\n");

            // Sends a Physical ReadDataByIdentifier Message
            Console.Write("\n\nSends a Physical ReadDataByIdentifier Message: \n");
            ushort[] buffer = new ushort[2] { (ushort)UDSApi.TPUDSSvcParamDI.PUDS_SVC_PARAM_DI_ADSDID, (ushort)UDSApi.TPUDSSvcParamDI.PUDS_SVC_PARAM_DI_ECUMDDID };
            Status = UDSApi.SvcReadDataByIdentifier(Channel, ref Message, buffer, 2);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcReadDataByIdentifier: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service ReadMemoryByAddress
        static void testReadMemoryByAddress(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            byte[] lBufferAddr = new byte[20];
            byte[] lBufferSize = new byte[20];
            byte buffAddrLen = 10;
            byte buffSizeLen = 3;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: ReadMemoryByAddress ***\n");

            // Sends a Physical ReadMemoryByAddress Message
            Console.Write("\n\nSends a Physical ReadMemoryByAddress Message: \n");
            for (int i = 0; i < buffAddrLen; i++)
            {
                lBufferAddr[i] = (byte)('A' + i);
                lBufferSize[i] = (byte)('1' + i);
            }
            Status = UDSApi.SvcReadMemoryByAddress(Channel, ref Message, lBufferAddr, buffAddrLen, lBufferSize, buffSizeLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcReadMemoryByAddress: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service ReadScalingDataByIdentifier
        static void testReadScalingDataByIdentifier(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: ReadScalingDataByIdentifier ***\n");

            // Sends a Physical ReadScalingDataByIdentifier Message
            Console.Write("\n\nSends a Physical ReadScalingDataByIdentifier Message: \n");
            Status = UDSApi.SvcReadScalingDataByIdentifier(Channel, ref Message, (ushort)UDSApi.TPUDSSvcParamDI.PUDS_SVC_PARAM_DI_BSFPDID);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcReadScalingDataByIdentifier: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service ReadDataByPeriodicIdentifier
        static void testReadDataByPeriodicIdentifier(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            byte[] lBuffer = new byte[20];
            ushort buffLen = 10;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: ReadDataByPeriodicIdentifier ***\n");

            // Sends a Physical ReadScalingDataByIdentifier Message
            Console.Write("\n\nSends a Physical ReadDataByPeriodicIdentifier Message: \n");
            for (int i = 0; i < buffLen; i++)
            {
                lBuffer[i] = (byte)('A' + i);
            }
            Status = UDSApi.SvcReadDataByPeriodicIdentifier(Channel, ref Message, UDSApi.TPUDSSvcParamRDBPI.PUDS_SVC_PARAM_RDBPI_SAMR, lBuffer, buffLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcReadDataByPeriodicIdentifier: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service DynamicallyDefineDataIdentifier
        static void testDynamicallyDefineDataIdentifier(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            ushort[] lBufferSourceDI = new ushort[20];
            byte[] lBufferMemSize = new byte[20];
            byte[] lBufferPosInSrc = new byte[20];
            ushort buffLen = 10;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: DynamicallyDefineDataIdentifier ***\n");

            // Sends a Physical DynamicallyDefineDataIdentifierDBID Message
            Console.Write("\n\nSends a Physical DynamicallyDefineDataIdentifierDBID Message: \n");
            for (int i = 0; i < buffLen; i++)
            {
                lBufferSourceDI[i] = (ushort)(((0xF0 + i) << 8) + ('A' + i));
                lBufferMemSize[i] = (byte)(i + 1);
                lBufferPosInSrc[i] = (byte)(100 + i);
            }
            Status = UDSApi.SvcDynamicallyDefineDataIdentifierDBID(Channel, ref Message,
                (ushort)UDSApi.TPUDSSvcParamDI.PUDS_SVC_PARAM_DI_CDDID, lBufferSourceDI, lBufferMemSize, lBufferPosInSrc, buffLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcDynamicallyDefineDataIdentifierDBID: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();

            // Sends a Physical UDS_SvcDynamicallyDefineDataIdentifierDBMA Message
            Console.Write("\n\nSends a Physical UDS_SvcDynamicallyDefineDataIdentifierDBMA Message: \n");
            buffLen = 3;
            byte[] lBuffsAddr = new byte[15];
            byte[] lBuffsSize = new byte[9];
            byte buffAddrLen = 5;
            byte buffSizeLen = 3;
            for (int j = 0; j < buffLen; j++)
            {
                for (int i = 0; i < buffAddrLen; i++)
                {
                    lBuffsAddr[buffAddrLen * j + i] = (byte)((10 * j) + i + 1);
                }
                for (int i = 0; i < buffSizeLen; i++)
                {
                    lBuffsSize[buffSizeLen * j + i] = (byte)(100 + (10 * j) + i + 1);
                }
            }
            Status = UDSApi.SvcDynamicallyDefineDataIdentifierDBMA(Channel, ref Message,
                (ushort)UDSApi.TPUDSSvcParamDI.PUDS_SVC_PARAM_DI_CESWNDID, buffAddrLen, buffSizeLen, lBuffsAddr, lBuffsSize, buffLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcDynamicallyDefineDataIdentifierDBMA: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();

            // Sends a Physical UDS_SvcDynamicallyDefineDataIdentifierCDDDI Message
            Console.Write("\n\nSends a Physical UDS_SvcDynamicallyDefineDataIdentifierCDDDI Message: \n");
            Status = UDSApi.SvcDynamicallyDefineDataIdentifierCDDDI(Channel, ref Message, (ushort)UDSApi.TPUDSSvcParamDI.PUDS_SVC_PARAM_DI_CESWNDID);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcDynamicallyDefineDataIdentifierCDDDI: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service WriteDataByIdentifier
        static void testWriteDataByIdentifier(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            byte[] lBuffer = new byte[20];
            ushort buffLen = 10;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: WriteDataByIdentifier ***\n");

            // Sends a Physical WriteDataByIdentifier Message
            Console.Write("\n\nSends a Physical WriteDataByIdentifier Message: \n");
            for (int i = 0; i < buffLen; i++)
            {
                lBuffer[i] = (byte)('A' + i);
            }
            Status = UDSApi.SvcWriteDataByIdentifier(Channel, ref Message, (ushort)UDSApi.TPUDSSvcParamDI.PUDS_SVC_PARAM_DI_ASFPDID, lBuffer, buffLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcWriteDataByIdentifier: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service WriteMemoryByIdentifier
        static void testWriteMemoryByAddress(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            byte[] lBuffer = new byte[50];
            byte[] lBufferMemAddr = new byte[50];
            byte[] lBufferMemSize = new byte[50];
            ushort buffLen = 50;
            byte buffAddrLen = 5;
            byte buffSizeLen = 3;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: WriteMemoryByAddress ***\n");

            // Sends a Physical WriteMemoryByAddress Message
            Console.Write("\n\nSends a Physical WriteMemoryByAddress Message: \n");
            for (int i = 0; i < buffLen; i++)
            {
                lBuffer[i] = (byte)(i + 1);
                lBufferMemAddr[i] = (byte)('A' + i);
                lBufferMemSize[i] = (byte)(10 + i);
            }
            Status = UDSApi.SvcWriteMemoryByAddress(Channel, ref Message, lBufferMemAddr, buffAddrLen,
                lBufferMemSize, buffSizeLen, lBuffer, buffLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcWriteMemoryByAddress: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service ClearDiagnosticInformation
        static void testClearDiagnosticInformation(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: ClearDiagnosticInformation ***\n");

            // Sends a Physical ClearDiagnosticInformation Message
            Console.Write("\n\nSends a Physical ClearDiagnosticInformation Message: \n");
            Status = UDSApi.SvcClearDiagnosticInformation(Channel, ref Message, 0xF1A2B3);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcClearDiagnosticInformation: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service ReadDTCInformation
        static void testReadDTCInformation(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: ReadDTCInformation ***\n");

            // Sends a Physical ReadDTCInformation Message
            Console.Write("\n\nSends a Physical ReadDTCInformation Message: \n");
            Status = UDSApi.SvcReadDTCInformation(Channel, ref Message, UDSApi.TPUDSSvcParamRDTCI.PUDS_SVC_PARAM_RDTCI_RNODTCBSM, 0xF1);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcReadDTCInformation: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();

            // Sends a Physical ReadDTCInformationRDTCSSBDTC Message
            Console.Write("\n\nSends a Physical ReadDTCInformationRDTCSSBDTC Message: \n");
            Status = UDSApi.SvcReadDTCInformationRDTCSSBDTC(Channel, ref Message, 0x00A1B2B3, 0x12);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  ReadDTCInformationRDTCSSBDTC: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();

            // Sends a Physical ReadDTCInformationRDTCSSBRN Message
            Console.Write("\n\nSends a Physical ReadDTCInformationRDTCSSBRN Message: \n");
            Status = UDSApi.SvcReadDTCInformationRDTCSSBRN(Channel, ref Message, 0x12);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcReadDTCInformationRDTCSSBRN: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();

            // Sends a Physical ReadDTCInformationReportExtended Message
            Console.Write("\n\nSends a Physical ReadDTCInformationReportExtended Message: \n");
            Status = UDSApi.SvcReadDTCInformationReportExtended(Channel, ref Message,
                UDSApi.TPUDSSvcParamRDTCI.PUDS_SVC_PARAM_RDTCI_RDTCEDRBDN, 0x00A1B2B3, 0x12);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcReadDTCInformationReportExtended: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();

            // Sends a Physical ReadDTCInformationReportSeverity Message
            Console.Write("\n\nSends a Physical ReadDTCInformationReportSeverity Message: \n");
            Status = UDSApi.SvcReadDTCInformationReportSeverity(Channel, ref Message,
                UDSApi.TPUDSSvcParamRDTCI.PUDS_SVC_PARAM_RDTCI_RNODTCBSMR, 0xF1, 0x12);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcReadDTCInformationReportSeverity: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();

            // Sends a Physical ReadDTCInformationRSIODTC Message
            Console.Write("\n\nSends a Physical ReadDTCInformationRSIODTC Message: \n");
            Status = UDSApi.SvcReadDTCInformationRSIODTC(Channel, ref Message, 0xF1A1B2B3);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcReadDTCInformationRSIODTC: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();

            // Sends a Physical ReadDTCInformationNoParam Message
            Console.Write("\n\nSends a Physical ReadDTCInformationNoParam Message: \n");
            Status = UDSApi.SvcReadDTCInformationNoParam(Channel, ref Message, UDSApi.TPUDSSvcParamRDTCI.PUDS_SVC_PARAM_RDTCI_RSUPDTC);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcReadDTCInformationNoParam: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service InputOutputControlByIdentifier
        static void testInputOutputControlByIdentifier(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            byte[] lBufferOption = new byte[20];
            byte[] lBufferEnableMask = new byte[20];
            ushort lBuffOptionLen = 10;
            ushort lBuffMaskLen = 5;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: InputOutputControlByIdentifier ***\n");

            // Sends a Physical InputOutputControlByIdentifier Message
            Console.Write("\n\nSends a Physical InputOutputControlByIdentifier Message: \n");
            for (int i = 0; i < lBuffOptionLen; i++)
            {
                lBufferOption[i] = (byte)('A' + i);
                lBufferEnableMask[i] = (byte)(10 + i);
            }
            Status = UDSApi.SvcInputOutputControlByIdentifier(Channel, ref Message, (ushort)UDSApi.TPUDSSvcParamDI.PUDS_SVC_PARAM_DI_SSECUSWVNDID,
                lBufferOption, lBuffOptionLen, lBufferEnableMask, lBuffMaskLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcInputOutputControlByIdentifier: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service RoutineControl
        static void testRoutineControl(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            byte[] lBuffer = new byte[20];
            ushort lBuffLen = 10;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: RoutineControl ***\n");

            // Sends a Physical RoutineControl Message
            Console.Write("\n\nSends a Physical RoutineControl Message: \n");
            for (int i = 0; i < lBuffLen; i++)
            {
                lBuffer[i] = (byte)('A' + i);
            }
            Status = UDSApi.SvcRoutineControl(Channel, ref Message, UDSApi.TPUDSSvcParamRC.PUDS_SVC_PARAM_RC_RRR,
                0xF1A2, lBuffer, lBuffLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcRoutineControl: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service RequestDownload
        static void testRequestDownload(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            byte[] lBufferMemAddr = new byte[50];
            byte[] lBufferMemSize = new byte[50];
            byte buffAddrLen = 15;
            byte buffSizeLen = 8;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: RequestDownload ***\n");

            // Sends a Physical RequestDownload Message
            Console.Write("\n\nSends a Physical RequestDownload Message: \n");
            for (int i = 0; i < buffAddrLen; i++)
            {
                lBufferMemAddr[i] = (byte)('A' + i);
                lBufferMemSize[i] = (byte)(10 + i);
            }
            Status = UDSApi.SvcRequestDownload(Channel, ref Message, 0x01, 0x02,
                lBufferMemAddr, buffAddrLen, lBufferMemSize, buffSizeLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcRequestDownload: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service RequestUpload
        static void testRequestUpload(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            byte[] lBufferMemAddr = new byte[50];
            byte[] lBufferMemSize = new byte[50];
            byte buffAddrLen = 21;
            byte buffSizeLen = 32;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: RequestUpload ***\n");

            // Sends a Physical RequestUpload Message
            Console.Write("\n\nSends a Physical RequestUpload Message: \n");
            for (int i = 0; i < buffSizeLen; i++)
            {
                lBufferMemAddr[i] = (byte)('A' + i);
                lBufferMemSize[i] = (byte)(10 + i);
            }
            Status = UDSApi.SvcRequestUpload(Channel, ref Message, 0x01, 0x02,
                lBufferMemAddr, buffAddrLen, lBufferMemSize, buffSizeLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcRequestUpload: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service TransferData
        static void testTransferData(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            byte[] lBuffer = new byte[50];
            byte buffLen = 50;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: TransferData ***\n");

            // Sends a Physical TransferData Message
            Console.Write("\n\nSends a Physical TransferData Message: \n");
            for (int i = 0; i < buffLen; i++)
            {
                lBuffer[i] = (byte)('A' + i);
            }
            Status = UDSApi.SvcTransferData(Channel, ref Message, 0x01, lBuffer, buffLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcTransferData: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service RequestTransferExit
        static void testRequestTransferExit(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            byte[] lBuffer = new byte[50];
            byte buffLen = 20;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: RequestTransferExit ***\n");

            // Sends a Physical RequestTransferExit Message
            Console.Write("\n\nSends a Physical RequestTransferExit Message: \n");
            for (int i = 0; i < buffLen; i++)
            {
                lBuffer[i] = (byte)('A' + i);
            }
            Status = UDSApi.SvcRequestTransferExit(Channel, ref Message, lBuffer, buffLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcRequestTransferExit: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }


        // UDS Service TransferData with MAX_DATA length
        static void testTransferDataBigMessage(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            byte[] lBuffer = new byte[4095];
            ushort buffLen = 4093;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: TransferData with MAX_DATA ***\n");

            // Sends a Physical TransferData Message with the maximum data available.
            // The goal is to show that UDS_WaitForService doesn't return a TIMEOUT error
            // although the transmit and receive time of all the data will be longer 
            // than the default time to get a response.
            Console.Write($"\n\nSends a Physical TransferData Message (LEN={buffLen}): \n");
            for (int i = 0; i < buffLen; i++)
            {
                lBuffer[i] = (byte)('A' + i);
            }
            Status = UDSApi.SvcTransferData(Channel, ref Message, 0x01, lBuffer, buffLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            Console.Write($"  UDS_SvcTransferData: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service RequestTransferExit
        static void testTransferDataMultipleFunctionalMessage(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg[] MessageBuffer = new TPUDSMsg[5] { new TPUDSMsg(), new TPUDSMsg(), new TPUDSMsg(), new TPUDSMsg(), new TPUDSMsg() };
            uint msgBufLen = 5;
            uint msgCount = 0;
            byte[] lBuffer = new byte[10];
            ushort buffLen = 5;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service: TransferData with Functional Message***\n");

            Message.NETADDRINFO.TA = (byte)TPUDSAddress.PUDS_ISO_15765_4_ADDR_OBD_FUNCTIONAL;
            Message.NETADDRINFO.TA_TYPE = TPUDSAddressingType.PUDS_ADDRESSING_FUNCTIONAL;

            // Sends a Functional TransferData Message.
            // The goal is to show that UDS_WaitForServiceFunctional waits long enough
            // to fetch all possible ECU responses.
            Console.Write("\n\nSends a Functional TransferData Message: \n");
            for (int i = 0; i < buffLen; i++)
            {
                lBuffer[i] = (byte)('A' + i);
            }
            Status = UDSApi.SvcTransferData(Channel, ref Message, 0x01, lBuffer, buffLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForServiceFunctional(Channel, MessageBuffer, msgBufLen, out msgCount, true, ref Message, out Message);
            Console.Write($"  UDS_SvcTransferData: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
            {
                displayMessage(Message, new TPUDSMsg(), true);
                Console.Write($"\n Received {msgCount} UDS responses:\n");
                for (uint i = 0; i < msgCount && i < msgBufLen; i++)
                    displayMessage(new TPUDSMsg(), MessageBuffer[i]);
            }
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // Sample to use event
        static void testUsingEvent(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            ManualResetEvent hEvent;
            bool res;
            // initialization
            Message.NETADDRINFO = N_AI;
            // set event handler
            hEvent = new ManualResetEvent(false);
            Status = UDSApi.SetValue(Channel, TPUDSParameter.PUDS_PARAM_RECEIVE_EVENT, hEvent.SafeWaitHandle.DangerousGetHandle(), (uint)Marshal.SizeOf(IntPtr.Zero));
            if (Status != TPUDSStatus.PUDS_ERROR_OK)
            {
                Console.Write("Failed to set event, aborting...");
                waitGetch();
                return;
            }

            if (USE_GETCH) Console.Clear();

            Console.Write("\n\n*** UDS Service with Event: TesterPresent ***\n");

            // Sends a Physical TesterPresent Message
            Console.Write("\n\nSends a Physical TesterPresent Message: \n");
            Status = UDSApi.SvcTesterPresent(Channel, ref Message);
            Console.Write($"  UDS_SvcTesterPresent: {Status}\n");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
            {
                // instead of calling WaitForService function,
                // this sample demonstrates how event can be used.
                //	But note that the use of a thread listening to notifications
                //	and making the read operations is preferred.
                bool bStop = false;
                // wait until we receive expected response
                do
                {
                    res = hEvent.WaitOne();
                    if (res)
                    {
                        // read all messages
                        do
                        {
                            Status = UDSApi.Read(Channel, out MessageResponse);
                            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                            {
                                // this is a simple message check (type and sender/receiver address):
                                // to filter UDS request confirmation and get first message from target,
                                // but real use-case should check that the UDS service ID matches the request
                                if (MessageResponse.MSGTYPE == TPUDSMessageType.PUDS_MESSAGE_TYPE_CONFIRM &&
                                    MessageResponse.NETADDRINFO.SA == N_AI.TA &&
                                    MessageResponse.NETADDRINFO.TA == N_AI.SA)
                                {
                                    bStop = true;
                                    displayMessage(Message, MessageResponse);
                                }
                            }
                        } while (Status != TPUDSStatus.PUDS_ERROR_NO_MESSAGE);
                    }
                } while (!bStop);
            }
            waitGetch();

            // uninitialize event
            UDSApi.SetValue(Channel, TPUDSParameter.PUDS_PARAM_RECEIVE_EVENT, IntPtr.Zero, (uint)Marshal.SizeOf(IntPtr.Zero));
        }

        public static void Main(string[] args)
        {
            TPUDSCANHandle Channel;
            TPUDSStatus Status;
            TPUDSNetAddrInfo N_AI;
            uint iBuffer;
            uint ulBuffer;
            int nbErr = 0;

            // Set the PCAN-Channel to use (PCAN-USB Channel 1)
            Channel = UDSApi.PUDS_USBBUS1;
            // Initializing of the UDS Communication session 
            Status = UDSApi.Initialize(Channel, TPUDSBaudrate.PUDS_BAUD_250K, 0, 0, 0);
            Console.Write($"Initialize UDS: {Status}\n");

            // Define Address
            iBuffer = UDSApi.PUDS_SERVER_ADDR_TEST_EQUIPMENT;
            Status = UDSApi.SetValue(Channel, TPUDSParameter.PUDS_PARAM_SERVER_ADDRESS, ref iBuffer, 1);
            Console.Write($"  Set ServerAddress: {Status} (0x{iBuffer:x2})\n");
            // Define TimeOuts
            ulBuffer = 2000;
            Status = UDSApi.SetValue(Channel, TPUDSParameter.PUDS_PARAM_TIMEOUT_REQUEST, ref ulBuffer, (uint)Marshal.SizeOf(ulBuffer));
            Console.Write($"  Set TIMEOUT_REQUEST: {Status} ({ulBuffer})\n");
            Status = UDSApi.SetValue(Channel, TPUDSParameter.PUDS_PARAM_TIMEOUT_RESPONSE, ref ulBuffer, (uint)Marshal.SizeOf(ulBuffer));
            Console.Write($"  Set TIMEOUT_REQUEST: {Status} ({ulBuffer})\n");
            waitGetch();

            // Define Network Address Information used for all the tests
            N_AI.SA = UDSApi.PUDS_SERVER_ADDR_TEST_EQUIPMENT;
            N_AI.TA = (byte)TPUDSAddress.PUDS_ISO_15765_4_ADDR_ECU_1;
            N_AI.TA_TYPE = TPUDSAddressingType.PUDS_ADDRESSING_PHYSICAL;
            N_AI.RA = 0x00;
            N_AI.PROTOCOL = TPUDSProtocol.PUDS_PROTOCOL_ISO_15765_2_11B;

            //
            // The following functions call UDS Services 
            // with the following workflow :
            // -----------------------------------------
            //	// Transmits a UDS Request
            //	Status = UDS_Svc[SERVICE_NAME](Channel, MessageRequest, ...);
            //	// Verify Status
            //	if (Status == PUDS_ERROR_OK) {
            //		// Waits for the service response
            //		Status = UDS_WaitForService(Channel, &MessageResponse, &MessageRequest);
            //	}
            // -----------------------------------------
            //
            testDiagnosticSessionControl(Channel, N_AI);
            testECUReset(Channel, N_AI);
            testSecurityAccess(Channel, N_AI);
            testCommunicationControl(Channel, N_AI);
            testTesterPresent(Channel, N_AI);
            testSecuredDataTransmission(Channel, N_AI);
            testControlDTCSetting(Channel, N_AI);
            testResponseOnEvent(Channel, N_AI);
            testLinkControl(Channel, N_AI);
            testReadDataByIdentifier(Channel, N_AI);
            testReadMemoryByAddress(Channel, N_AI);
            testReadScalingDataByIdentifier(Channel, N_AI);
            testReadDataByPeriodicIdentifier(Channel, N_AI);
            testDynamicallyDefineDataIdentifier(Channel, N_AI);
            testWriteDataByIdentifier(Channel, N_AI);
            testWriteMemoryByAddress(Channel, N_AI);
            testClearDiagnosticInformation(Channel, N_AI);
            testReadDTCInformation(Channel, N_AI);
            testInputOutputControlByIdentifier(Channel, N_AI);
            testRoutineControl(Channel, N_AI);
            testRequestDownload(Channel, N_AI);
            testRequestUpload(Channel, N_AI);
            testTransferData(Channel, N_AI);
            testRequestTransferExit(Channel, N_AI);

            // Miscellaneous examples
            testTransferDataBigMessage(Channel, N_AI);
            testTransferDataMultipleFunctionalMessage(Channel, N_AI);
//            testUsingEvent(Channel, N_AI);

            // Display a small report
            if (g_nbErr > 0)
            {
                Console.Write($"\nERROR : {g_nbErr} errors occured.\n\n");
            }
            else
            {
                Console.Write("\nALL Transmissions succeeded !\n\n");
            }
            Console.Write("\n\nPress <Enter> to quit...");
            Console.ReadKey(true);

            UDSApi.Uninitialize(Channel);
        }
    }
}
