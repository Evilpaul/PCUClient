using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

                Console.Clear();
            }
        }

        // A function that displays UDS Request and Response messages (and count error if no response)
        static void displayMessage(TPUDSMsg* Request, TPUDSMsg* Response, bool noResponseExpected = false)
        {
            char[] buffer = new char[500];

            if (Request != NULL)
            {
                printf("\nUDS request from 0x%02x (to 0x%02x, with RA 0x%02x) - result: %i - %s\n",
                    (int)Request->NETADDRINFO.SA,
                    (int)Request->NETADDRINFO.TA,
                    (int)Request->NETADDRINFO.RA,
                    (int)Request->RESULT,
                    Request->RESULT != PUDS_RESULT_N_OK ? "ERROR !!!" : "OK !");
                // display data
                setvbuf(stdout, buffer, _IOLBF, BUFSIZ);
                printf("\t\\-> Length: %i, Data= ", (int)Request->LEN);
                for (int i = 0; i < Request->LEN; i++)
                {
                    printf("%02x ", (int)Request->DATA.RAW[i]);
                    fflush(stdout);
                }
                setvbuf(stdout, NULL, _IONBF, 0);
            }
            if (Response != NULL)
            {
                printf("\nUDS RESPONSE from 0x%02x (to 0x%02x, with RA 0x%02x) - result: %i - %s\n",
                    (int)Response->NETADDRINFO.SA,
                    (int)Response->NETADDRINFO.TA,
                    (int)Response->NETADDRINFO.RA,
                    (int)Response->RESULT,
                    Response->RESULT != PUDS_RESULT_N_OK ? "ERROR !!!" : "OK !");
                // display data
                setvbuf(stdout, buffer, _IOLBF, BUFSIZ);
                printf("\t\\-> Length: %i, Data= ", (int)Response->LEN);
                for (int i = 0; i < Response->LEN; i++)
                {
                    printf("%02x ", (int)Response->DATA.RAW[i]);
                    fflush(stdout);
                }
                Console.Write("\n");
                setvbuf(stdout, NULL, _IONBF, 0);
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

            // initialization
            Message.NETADDRINFO = N_AI;
            lSessionInfo.NETADDRINFO = N_AI;

            Console.Clear();

            Console.Write("\n\n*** UDS Service: DiagnosticSessionControl ***\n");

            // Read default session information 
            //  Server is not yet known (Status will be PUDS_ERROR_NOT_INITIALIZED)
            //	yet the API will still set lSessionInfo to default values
            Status = UDS_GetValue(Channel, PUDS_PARAM_SESSION_INFO, &lSessionInfo, sizeof(lSessionInfo));
            printf("  Diagnostic Session Information: %i, 0x%02x => %d = [%04x; %04x]\n",
                Status,
                lSessionInfo.NETADDRINFO.TA,
                lSessionInfo.SESSION_TYPE,
                lSessionInfo.TIMEOUT_P2CAN_SERVER_MAX,
                lSessionInfo.TIMEOUT_ENHANCED_P2CAN_SERVER_MAX);
            waitGetch();

            // Set Diagnostic session to DEFAULT (to get session information)
            Console.Write("\n\nSetting a DEFAULT Diagnostic Session :\n");
            Status = UDS_SvcDiagnosticSessionControl(Channel, &Message, PUDS_SVC_PARAM_DSC_DS);
            if (Status == PUDS_ERROR_OK)
                Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
            printf("  UDS_SvcDiagnosticSessionControl: %i\n", (int)Status);
            if (Status == PUDS_ERROR_OK)
                displayMessage(&Message, &MessageResponse);
            else
                displayMessage(&Message, NULL);
            // Read current session information
            memset(&lSessionInfo, 0, sizeof(lSessionInfo));
            lSessionInfo.NETADDRINFO = N_AI;
            Status = UDS_GetValue(Channel, PUDS_PARAM_SESSION_INFO, &lSessionInfo, sizeof(lSessionInfo));
            printf("  Diagnostic Session Information: %i, 0x%02x => %d = [%04x; %04x]\n",
                Status,
                lSessionInfo.NETADDRINFO.TA,
                lSessionInfo.SESSION_TYPE,
                lSessionInfo.TIMEOUT_P2CAN_SERVER_MAX,
                lSessionInfo.TIMEOUT_ENHANCED_P2CAN_SERVER_MAX);
            waitGetch();

            // Set Diagnostic session to PROGRAMMING
            Console.Write("\n\nSetting a ECUPS Diagnostic Session :\n");
            Status = UDS_SvcDiagnosticSessionControl(Channel, &Message, PUDS_SVC_PARAM_DSC_ECUPS);
            if (Status == PUDS_ERROR_OK)
                Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
            printf("  UDS_SvcDiagnosticSessionControl: %i\n", (int)Status);
            if (Status == PUDS_ERROR_OK)
                displayMessage(&Message, &MessageResponse);
            else
                displayMessage(&Message, NULL);
            // Read current session information
            memset(&lSessionInfo, 0, sizeof(lSessionInfo));
            lSessionInfo.NETADDRINFO = N_AI;
            Status = UDS_GetValue(Channel, PUDS_PARAM_SESSION_INFO, &lSessionInfo, sizeof(lSessionInfo));
            printf("  Diagnostic Session Information: %i, 0x%02x => %d = [%04x; %04x]\n",
                Status,
                lSessionInfo.NETADDRINFO.TA,
                lSessionInfo.SESSION_TYPE,
                lSessionInfo.TIMEOUT_P2CAN_SERVER_MAX,
                lSessionInfo.TIMEOUT_ENHANCED_P2CAN_SERVER_MAX);
            Console.Write(" Assert that Auto TesterPresent Frame is sent...\n");
            Thread.Sleep(2000);
            Console.Write("  Should transmit an Auto TesterPresent Frame\n");
            Thread.Sleep(2000);
            Console.Write("  Should transmit an Auto TesterPresent Frame\n");

            waitGetch();
            // Set Diagnostic session back to DEFAULT
            printf("\n\nSetting a DEFAULT Diagnostic Session :\n");
            Status = UDS_SvcDiagnosticSessionControl(Channel, &Message, PUDS_SVC_PARAM_DSC_DS);
            if (Status == PUDS_ERROR_OK)
                Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
            printf("  UDS_SvcDiagnosticSessionControl: %i\n", (int)Status);
            if (Status == PUDS_ERROR_OK)
                displayMessage(&Message, &MessageResponse);
            else
                displayMessage(&Message, NULL);
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
