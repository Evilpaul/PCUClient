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
        private static readonly bool USE_GETCH = true;

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

                Console.Clear();
            }
        }

        // A function that displays UDS Request and Response messages (and count error if no response)
        static void displayMessage(TPUDSMsg Request, TPUDSMsg Response, bool noResponseExpected = false)
        {
            if (Request.Equals(default(TPUDSMsg)))
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
            if (Response.Equals(default(TPUDSMsg)))
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
            return ((v & 0x000000FF) << 24) | ((v & 0x0000FF00) << 8) | ((v & 0x00FF0000) >> 8) | ((v & 0xFF000000) >> 24);
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

            Console.Clear();

            Console.Write("\n\n*** UDS Service: DiagnosticSessionControl ***\n");

            // Read default session information 
            //  Server is not yet known (Status will be PUDS_ERROR_NOT_INITIALIZED)
            //	yet the API will still set lSessionInfo to default values
            IntPtr iptr = Marshal.AllocHGlobal(Marshal.SizeOf(lSessionInfo));
            Marshal.StructureToPtr(lSessionInfo, iptr, false);
            Status = UDSApi.GetValue(Channel, TPUDSParameter.PUDS_PARAM_SESSION_INFO, iptr, (uint)Marshal.SizeOf(lSessionInfo));
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
            //testECUReset(Channel, N_AI);
            //testSecurityAccess(Channel, N_AI);
            //testCommunicationControl(Channel, N_AI);
            //testTesterPresent(Channel, N_AI);
            //testSecuredDataTransmission(Channel, N_AI);
            //testControlDTCSetting(Channel, N_AI);
            //testResponseOnEvent(Channel, N_AI);
            //testLinkControl(Channel, N_AI);
            //testReadDataByIdentifier(Channel, N_AI);
            //testReadMemoryByAddress(Channel, N_AI);
            //testReadScalingDataByIdentifier(Channel, N_AI);
            //testReadDataByPeriodicIdentifier(Channel, N_AI);
            //testDynamicallyDefineDataIdentifier(Channel, N_AI);
            //testWriteDataByIdentifier(Channel, N_AI);
            //testWriteMemoryByAddress(Channel, N_AI);
            //testClearDiagnosticInformation(Channel, N_AI);
            //testReadDTCInformation(Channel, N_AI);
            //testInputOutputControlByIdentifier(Channel, N_AI);
            //testRoutineControl(Channel, N_AI);
            //testRequestDownload(Channel, N_AI);
            //testRequestUpload(Channel, N_AI);
            //testTransferData(Channel, N_AI);
            //testRequestTransferExit(Channel, N_AI);

            //// Miscellaneous examples
            //testTransferDataBigMessage(Channel, N_AI);
            //testTransferDataMultipleFunctionalMessage(Channel, N_AI);
            //testUsingEvent(Channel, N_AI);

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
