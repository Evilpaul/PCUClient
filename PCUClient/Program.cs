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

        // UDS Service ECUReset
//void testECUReset(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};	
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: ECUReset ***\n");

//	// Sends a Physical ECUReset Message
//	printf("\n\nSends a Physical ECUReset Message: \n");
//	Status = UDS_SvcECUReset(Channel, &Message, PUDS_SVC_PARAM_ER_SR);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcECUReset: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();
//}
//// UDS Service SecurityAccess
//void testSecurityAccess(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};	
//	DWORD dwBuffer;
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: SecurityAccess ***\n");

//	// Sends a Physical SecurityAccess Message
//	printf("\n\nSends a Physical SecurityAccess Message: \n");
//	DWORD valueLittleEndian = 0xF0A1B2C3;
//	dwBuffer = Reverse32(&valueLittleEndian);	// use helper function to set MSB as 1st byte in the buffer (Win32 uses little endian format)
//	Status = UDS_SvcSecurityAccess(Channel, &Message, PUDS_SVC_PARAM_SA_RSD_1, (BYTE*) &dwBuffer, 4);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcSecurityAccess: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();
//}
//// UDS Service CommunicationControl
//void testCommunicationControl(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};	
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: CommunicationControl ***\n");

//	// Sends a Physical CommunicationControl Message
//	printf("\n\nSends a Physical CommunicationControl Message: \n");
//	Status = UDS_SvcCommunicationControl(Channel, &Message, PUDS_SVC_PARAM_CC_ERXTX, 
//		PUDS_SVC_PARAM_CC_FLAG_APPL | PUDS_SVC_PARAM_CC_FLAG_NWM | PUDS_SVC_PARAM_CC_FLAG_DENWRIRO);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcCommunicationControl: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();
//}
//// UDS Service TesterPresent
//void testTesterPresent(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};	
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: TesterPresent ***\n");

//	// Sends a Physical TesterPresent Message
//	printf("\n\nSends a Physical TesterPresent Message: \n");
//	Status = UDS_SvcTesterPresent(Channel, &Message);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcTesterPresent: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();

//	// Sends a Physical TesterPresent Message with no positive response
//	printf("\n\nSends a Physical TesterPresent Message with no positive response :\n");
//	Message.NO_POSITIVE_RESPONSE_MSG = PUDS_SUPPR_POS_RSP_MSG_INDICATION_BIT;
//	Status = UDS_SvcTesterPresent(Channel, &Message);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcTesterPresent: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL, true);
//	waitGetch();

//	// Sends a Functional TesterPresent Message
//	printf("\n\nSends a Functional TesterPresent Message: \n");
//	Message.NETADDRINFO.TA = PUDS_ISO_15765_4_ADDR_OBD_FUNCTIONAL;
//	Message.NETADDRINFO.TA_TYPE = PUDS_ADDRESSING_FUNCTIONAL;
//	Message.NO_POSITIVE_RESPONSE_MSG = 0;
//	Status = UDS_SvcTesterPresent(Channel, &Message);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcTesterPresent: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();

//	// Sends a Functional TesterPresent Message with no positive response
//	printf("\n\nSends a Functional TesterPresent Message with no positive response :\n");
//	Message.NETADDRINFO.TA = PUDS_ISO_15765_4_ADDR_OBD_FUNCTIONAL;
//	Message.NETADDRINFO.TA_TYPE = PUDS_ADDRESSING_FUNCTIONAL;
//	Message.NO_POSITIVE_RESPONSE_MSG = PUDS_SUPPR_POS_RSP_MSG_INDICATION_BIT;
//	Status = UDS_SvcTesterPresent(Channel, &Message);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcTesterPresent: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL, true);
//	waitGetch();
//}

//// UDS Service SecuredDataTransmission
//void testSecuredDataTransmission(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};	
//	DWORD dwBuffer;
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: SecuredDataTransmission ***\n");

//	// Sends a Physical SecuredDataTransmission Message
//	printf("\n\nSends a Physical SecuredDataTransmission Message: \n");
//	DWORD valueLittleEndian = 0xF0A1B2C3;
//	dwBuffer = Reverse32(&valueLittleEndian);	// use helper function to set MSB as 1st byte in the buffer (Win32 uses little endian format)
//	Status = UDS_SvcSecuredDataTransmission(Channel, &Message, (BYTE*) &dwBuffer, 4);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcSecuredDataTransmission: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();
//}
//// UDS Service ControlDTCSetting
//void testControlDTCSetting(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};
//	DWORD dwBuffer;
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: ControlDTCSetting ***\n");

//	// Sends a Physical ControlDTCSetting Message
//	printf("\n\nSends a Physical ControlDTCSetting Message: \n");
//	DWORD valueLittleEndian = 0xF1A1B2EE;
//	dwBuffer = Reverse32(&valueLittleEndian);	// use helper function to set MSB as 1st byte in the buffer (Win32 uses little endian format)
//	Status = UDS_SvcControlDTCSetting(Channel, &Message, PUDS_SVC_PARAM_CDTCS_OFF, (BYTE*)&dwBuffer, 3);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcControlDTCSetting: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();
//}
//// UDS Service ResponseOnEvent
//void testResponseOnEvent(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};
//	BYTE lBuffer[50];
//	BYTE lBuffer2[50];
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: ResponseOnEvent ***\n");

//	// Sends a Physical ResponseOnEvent Message
//	printf("\n\nSends a Physical ResponseOnEvent Message: \n");
//	lBuffer[0] = 0x08;
//	lBuffer2[0] = PUDS_SI_ReadDTCInformation;
//	lBuffer2[1] = PUDS_SVC_PARAM_RDTCI_RNODTCBSM;
//	lBuffer2[2] = 0x01;
//	Status = UDS_SvcResponseOnEvent(Channel, &Message, PUDS_SVC_PARAM_ROE_ONDTCS, 
//		FALSE, 0x08, lBuffer, 1, lBuffer2, 3);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcResponseOnEvent: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();
//}
//// UDS Service LinkControl
//void testLinkControl(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: LinkControl ***\n");

//	// Sends a Physical LinkControl Message
//	printf("\n\nSends a Physical LinkControl Message (Verify Fixed Baudrate): \n");
//	Status = UDS_SvcLinkControl(Channel, &Message, PUDS_SVC_PARAM_LC_VBTWFBR, PUDS_SVC_PARAM_LC_BAUDRATE_CAN_250K, 0);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcLinkControl: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);

//	// Sends a Physical LinkControl Message
//	printf("\n\nSends a Physical LinkControl Message (Verify Specific Baudrate): \n");
//	Status = UDS_SvcLinkControl(Channel, &Message, PUDS_SVC_PARAM_LC_VBTWSBR, 0, 500000);	// 500K = 0x0007a120
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcLinkControl: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);

//	// Sends a Physical LinkControl Message
//	printf("\n\nSends a Physical LinkControl Message (Transition): \n");
//	Status = UDS_SvcLinkControl(Channel, &Message, PUDS_SVC_PARAM_LC_TB, 0, 0);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcLinkControl: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);

//	waitGetch();
//}
//// UDS Service ReadDataByIdentifier
//void testReadDataByIdentifier(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};	
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: ReadDataByIdentifier ***\n");

//	// Sends a Physical ReadDataByIdentifier Message
//	printf("\n\nSends a Physical ReadDataByIdentifier Message: \n");
//	WORD buffer[2] = {PUDS_SVC_PARAM_DI_ADSDID, PUDS_SVC_PARAM_DI_ECUMDDID};
//	Status = UDS_SvcReadDataByIdentifier(Channel, &Message, buffer, 2);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcReadDataByIdentifier: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();		
//}
//// UDS Service ReadMemoryByAddress
//void testReadMemoryByAddress(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};	
//	BYTE lBufferAddr[20] = {};
//	BYTE lBufferSize[20] = {};
//	BYTE buffAddrLen = 10;
//	BYTE buffSizeLen = 3;
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: ReadMemoryByAddress ***\n");

//	// Sends a Physical ReadMemoryByAddress Message
//	printf("\n\nSends a Physical ReadMemoryByAddress Message: \n");
//	for (int i = 0 ; i < buffAddrLen ; i++) {
//		lBufferAddr[i] = 'A' + i;
//		lBufferSize[i] = '1' + i;
//	}
//	Status = UDS_SvcReadMemoryByAddress(Channel, &Message, lBufferAddr, buffAddrLen, lBufferSize, buffSizeLen);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcReadMemoryByAddress: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();		
//}
//// UDS Service ReadScalingDataByIdentifier
//void testReadScalingDataByIdentifier(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};	
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: ReadScalingDataByIdentifier ***\n");

//	// Sends a Physical ReadScalingDataByIdentifier Message
//	printf("\n\nSends a Physical ReadScalingDataByIdentifier Message: \n");
//	Status = UDS_SvcReadScalingDataByIdentifier(Channel, &Message, PUDS_SVC_PARAM_DI_BSFPDID);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcReadScalingDataByIdentifier: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();		
//}
//// UDS Service ReadDataByPeriodicIdentifier
//void testReadDataByPeriodicIdentifier(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};	
//	BYTE lBuffer[20] = {};
//	WORD buffLen = 10;
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: ReadDataByPeriodicIdentifier ***\n");

//	// Sends a Physical ReadScalingDataByIdentifier Message
//	printf("\n\nSends a Physical ReadDataByPeriodicIdentifier Message: \n");
//	for (int i = 0 ; i < buffLen ; i++) {
//		lBuffer[i] = 'A' + i;
//	}
//	Status = UDS_SvcReadDataByPeriodicIdentifier(Channel, &Message, PUDS_SVC_PARAM_RDBPI_SAMR, lBuffer, buffLen);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcReadDataByPeriodicIdentifier: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();		
//}
//// UDS Service DynamicallyDefineDataIdentifier
//void testDynamicallyDefineDataIdentifier(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};	
//	WORD lBufferSourceDI[20] = {};
//	BYTE lBufferMemSize[20] = {};
//	BYTE lBufferPosInSrc[20] = {};
//	WORD buffLen = 10;
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: DynamicallyDefineDataIdentifier ***\n");

//	// Sends a Physical DynamicallyDefineDataIdentifierDBID Message
//	printf("\n\nSends a Physical DynamicallyDefineDataIdentifierDBID Message: \n");
//	for (int i = 0 ; i < buffLen ; i++) {
//		lBufferSourceDI[i] = ((0xF0+i) << 8) + ('A' + i);
//		lBufferMemSize[i] = i + 1;
//		lBufferPosInSrc[i] = 100 + i;		
//	}
//	Status = UDS_SvcDynamicallyDefineDataIdentifierDBID(Channel, &Message, 
//		PUDS_SVC_PARAM_DI_CDDID, lBufferSourceDI, lBufferMemSize, lBufferPosInSrc, buffLen);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcDynamicallyDefineDataIdentifierDBID: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();

//	// Sends a Physical UDS_SvcDynamicallyDefineDataIdentifierDBMA Message
//	printf("\n\nSends a Physical UDS_SvcDynamicallyDefineDataIdentifierDBMA Message: \n");	
//	buffLen = 3;
//	BYTE lBuffsAddr[15] = {};
//	BYTE lBuffsSize[9] = {};
//	BYTE buffAddrLen = 5;
//	BYTE buffSizeLen = 3;
//	for (int j = 0 ; j < buffLen ; j++)
//	{
//		for (int i = 0 ; i < buffAddrLen ; i++) {
//			lBuffsAddr[buffAddrLen*j+i] = (10 * j) + i + 1;
//		}
//		for (int i = 0 ; i < buffSizeLen ; i++) {
//			lBuffsSize[buffSizeLen*j+i] = 100 + (10 * j) + i + 1;
//		}
//	}
//	Status = UDS_SvcDynamicallyDefineDataIdentifierDBMA(Channel, &Message, 
//		PUDS_SVC_PARAM_DI_CESWNDID, buffAddrLen, buffSizeLen, lBuffsAddr, lBuffsSize, buffLen);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcDynamicallyDefineDataIdentifierDBMA: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();

//	// Sends a Physical UDS_SvcDynamicallyDefineDataIdentifierCDDDI Message
//	printf("\n\nSends a Physical UDS_SvcDynamicallyDefineDataIdentifierCDDDI Message: \n");
//	Status = UDS_SvcDynamicallyDefineDataIdentifierCDDDI(Channel, &Message, PUDS_SVC_PARAM_DI_CESWNDID);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcDynamicallyDefineDataIdentifierCDDDI: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();		
//}
//// UDS Service WriteDataByIdentifier
//void testWriteDataByIdentifier(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};	
//	BYTE lBuffer[20] = {};
//	WORD buffLen = 10;
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: WriteDataByIdentifier ***\n");

//	// Sends a Physical WriteDataByIdentifier Message
//	printf("\n\nSends a Physical WriteDataByIdentifier Message: \n");
//	for (int i = 0 ; i < buffLen ; i++) {
//		lBuffer[i] = 'A' + i;
//	}
//	Status = UDS_SvcWriteDataByIdentifier(Channel, &Message, PUDS_SVC_PARAM_DI_ASFPDID, lBuffer, buffLen);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcWriteDataByIdentifier: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();		
//}
//// UDS Service WriteDataByIdentifier
//void testWriteMemoryByAddress(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};	
//	BYTE lBuffer[50] = {};
//	BYTE lBufferMemAddr[50] = {};
//	BYTE lBufferMemSize[50] = {};
//	WORD buffLen = 50;
//	BYTE buffAddrLen = 5;
//	BYTE buffSizeLen = 3;
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: WriteMemoryByAddress ***\n");

//	// Sends a Physical WriteMemoryByAddress Message
//	printf("\n\nSends a Physical WriteMemoryByAddress Message: \n");
//	for (int i = 0 ; i < buffLen ; i++) {
//		lBuffer[i] = i+1;
//		lBufferMemAddr[i] = 'A' + i;
//		lBufferMemSize[i] = 10 + i;
//	}
//	Status = UDS_SvcWriteMemoryByAddress(Channel, &Message, lBufferMemAddr, buffAddrLen, 
//		lBufferMemSize, buffSizeLen, lBuffer, buffLen);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcWriteMemoryByAddress: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();		
//}
//// UDS Service ClearDiagnosticInformation
//void testClearDiagnosticInformation(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};	
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: ClearDiagnosticInformation ***\n");

//	// Sends a Physical ClearDiagnosticInformation Message
//	printf("\n\nSends a Physical ClearDiagnosticInformation Message: \n");
//	Status = UDS_SvcClearDiagnosticInformation(Channel, &Message, 0xF1A2B3);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcClearDiagnosticInformation: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();
//}
//// UDS Service ReadDTCInformation
//void testReadDTCInformation(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};	
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: ReadDTCInformation ***\n");

//	// Sends a Physical ReadDTCInformation Message
//	printf("\n\nSends a Physical ReadDTCInformation Message: \n");
//	Status = UDS_SvcReadDTCInformation(Channel, &Message, PUDS_SVC_PARAM_RDTCI_RNODTCBSM, 0xF1);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcReadDTCInformation: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();

//	// Sends a Physical ReadDTCInformationRDTCSSBDTC Message
//	printf("\n\nSends a Physical ReadDTCInformationRDTCSSBDTC Message: \n");
//	Status = UDS_SvcReadDTCInformationRDTCSSBDTC(Channel, &Message, 0x00A1B2B3, 0x12);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  ReadDTCInformationRDTCSSBDTC: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();

//	// Sends a Physical ReadDTCInformationRDTCSSBRN Message
//	printf("\n\nSends a Physical ReadDTCInformationRDTCSSBRN Message: \n");
//	Status = UDS_SvcReadDTCInformationRDTCSSBRN(Channel, &Message, 0x12);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcReadDTCInformationRDTCSSBRN: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();

//	// Sends a Physical ReadDTCInformationReportExtended Message
//	printf("\n\nSends a Physical ReadDTCInformationReportExtended Message: \n");
//	Status = UDS_SvcReadDTCInformationReportExtended(Channel, &Message, 
//		PUDS_SVC_PARAM_RDTCI_RDTCEDRBDN, 0x00A1B2B3, 0x12);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcReadDTCInformationReportExtended: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();

//	// Sends a Physical ReadDTCInformationReportSeverity Message
//	printf("\n\nSends a Physical ReadDTCInformationReportSeverity Message: \n");
//	Status = UDS_SvcReadDTCInformationReportSeverity(Channel, &Message, 
//		PUDS_SVC_PARAM_RDTCI_RNODTCBSMR, 0xF1, 0x12);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcReadDTCInformationReportSeverity: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();

//	// Sends a Physical ReadDTCInformationRSIODTC Message
//	printf("\n\nSends a Physical ReadDTCInformationRSIODTC Message: \n");
//	Status = UDS_SvcReadDTCInformationRSIODTC(Channel, &Message, 0xF1A1B2B3);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcReadDTCInformationRSIODTC: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();

//	// Sends a Physical ReadDTCInformationNoParam Message
//	printf("\n\nSends a Physical ReadDTCInformationNoParam Message: \n");
//	Status = UDS_SvcReadDTCInformationNoParam(Channel, &Message, PUDS_SVC_PARAM_RDTCI_RSUPDTC);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcReadDTCInformationNoParam: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();
//}
//// UDS Service InputOutputControlByIdentifier
//void testInputOutputControlByIdentifier(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};	
//	BYTE lBufferOption[20] = {};	
//	BYTE lBufferEnableMask[20] = {};	
//	WORD lBuffOptionLen = 10;
//	WORD lBuffMaskLen = 5;
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: InputOutputControlByIdentifier ***\n");

//	// Sends a Physical InputOutputControlByIdentifier Message
//	printf("\n\nSends a Physical InputOutputControlByIdentifier Message: \n");
//	for (int i = 0 ; i < lBuffOptionLen ; i++) {
//		lBufferOption[i] = 'A' + i;
//		lBufferEnableMask[i] = 10 + i;
//	}
//	Status = UDS_SvcInputOutputControlByIdentifier(Channel, &Message, PUDS_SVC_PARAM_DI_SSECUSWVNDID,
//		lBufferOption, lBuffOptionLen, lBufferEnableMask, lBuffMaskLen);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcInputOutputControlByIdentifier: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();
//}
//// UDS Service RoutineControl
//void testRoutineControl(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};	
//	BYTE lBuffer[20] = {};	
//	WORD lBuffLen = 10;
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: RoutineControl ***\n");

//	// Sends a Physical RoutineControl Message
//	printf("\n\nSends a Physical RoutineControl Message: \n");
//	for (int i = 0 ; i < lBuffLen ; i++) {
//		lBuffer[i] = 'A' + i;
//	}
//	Status = UDS_SvcRoutineControl(Channel, &Message, PUDS_SVC_PARAM_RC_RRR,
//		0xF1A2, lBuffer, lBuffLen);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcRoutineControl: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();
//}
//// UDS Service RequestDownload
//void testRequestDownload(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};	
//	BYTE lBufferMemAddr[50] = {};
//	BYTE lBufferMemSize[50] = {};
//	BYTE buffAddrLen = 15;
//	BYTE buffSizeLen = 8;
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: RequestDownload ***\n");

//	// Sends a Physical RequestDownload Message
//	printf("\n\nSends a Physical RequestDownload Message: \n");
//	for (int i = 0 ; i < buffAddrLen ; i++) {
//		lBufferMemAddr[i] = 'A' + i;
//		lBufferMemSize[i] = 10 + i;
//	}
//	Status = UDS_SvcRequestDownload(Channel, &Message, 0x01, 0x02,
//		lBufferMemAddr, buffAddrLen, lBufferMemSize, buffSizeLen);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcRequestDownload: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();
//}
//// UDS Service RequestUpload
//void testRequestUpload(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};	
//	BYTE lBufferMemAddr[50] = {};
//	BYTE lBufferMemSize[50] = {};
//	BYTE buffAddrLen = 21;
//	BYTE buffSizeLen = 32;
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: RequestUpload ***\n");

//	// Sends a Physical RequestUpload Message
//	printf("\n\nSends a Physical RequestUpload Message: \n");
//	for (int i = 0 ; i < buffSizeLen ; i++) {
//		lBufferMemAddr[i] = 'A' + i;
//		lBufferMemSize[i] = 10 + i;
//	}
//	Status = UDS_SvcRequestUpload(Channel, &Message, 0x01, 0x02,
//		lBufferMemAddr, buffAddrLen, lBufferMemSize, buffSizeLen);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcRequestUpload: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();
//}
//// UDS Service TransferData
//void testTransferData(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};	
//	BYTE lBuffer[50] = {};
//	BYTE buffLen = 50;
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: TransferData ***\n");

//	// Sends a Physical TransferData Message
//	printf("\n\nSends a Physical TransferData Message: \n");
//	for (int i = 0 ; i < buffLen ; i++) {
//		lBuffer[i] = 'A' + i;
//	}
//	Status = UDS_SvcTransferData(Channel, &Message, 0x01, lBuffer, buffLen);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcTransferData: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();
//}
//// UDS Service RequestTransferExit
//void testRequestTransferExit(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};	
//	BYTE lBuffer[50] = {};
//	BYTE buffLen = 20;
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: RequestTransferExit ***\n");

//	// Sends a Physical RequestTransferExit Message
//	printf("\n\nSends a Physical RequestTransferExit Message: \n");
//	for (int i = 0 ; i < buffLen ; i++) {
//		lBuffer[i] = 'A' + i;
//	}
//	Status = UDS_SvcRequestTransferExit(Channel, &Message, lBuffer, buffLen);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcRequestTransferExit: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();
//}



//// UDS Service TransferData with MAX_DATA length
//void testTransferDataBigMessage(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};	
//	BYTE lBuffer[4095] = {};
//	WORD buffLen = 4093;
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: TransferData with MAX_DATA ***\n");

//	// Sends a Physical TransferData Message with the maximum data available.
//	// The goal is to show that UDS_WaitForService doesn't return a TIMEOUT error
//	// although the transmit and receive time of all the data will be longer 
//	// than the default time to get a response.
//	printf("\n\nSends a Physical TransferData Message (LEN=%d): \n", buffLen);
//	for (int i = 0 ; i < buffLen ; i++) {
//		lBuffer[i] = 'A' + i;
//	}
//	Status = UDS_SvcTransferData(Channel, &Message, 0x01, lBuffer, buffLen);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForService(Channel, &MessageResponse, &Message);
//	printf("  UDS_SvcTransferData: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//		displayMessage(&Message, &MessageResponse);
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();
//}

//// UDS Service RequestTransferExit
//void testTransferDataMultipleFunctionalMessage(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};
//	TPUDSMsg MessageBuffer[5] = {};	
//	DWORD msgBufLen = 5;
//	DWORD msgCount = 0;
//	BYTE lBuffer[10] = {};
//	WORD buffLen = 5;
//	// initialization
//	Message.NETADDRINFO = N_AI;

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service: TransferData with Functional Message***\n");

//	Message.NETADDRINFO.TA = PUDS_ISO_15765_4_ADDR_OBD_FUNCTIONAL;
//	Message.NETADDRINFO.TA_TYPE = PUDS_ADDRESSING_FUNCTIONAL;

//	// Sends a Functional TransferData Message.
//	// The goal is to show that UDS_WaitForServiceFunctional waits long enough
//	// to fetch all possible ECU responses.
//	printf("\n\nSends a Functional TransferData Message: \n");
//	for (int i = 0 ; i < buffLen ; i++) {
//		lBuffer[i] = 'A' + i;
//	}
//	Status = UDS_SvcTransferData(Channel, &Message, 0x01, lBuffer, buffLen);
//	if (Status == PUDS_ERROR_OK)
//		Status = UDS_WaitForServiceFunctional(Channel, MessageBuffer, msgBufLen, &msgCount, TRUE, &Message, &Message);
//	printf("  UDS_SvcTransferData: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK) 
//	{
//		displayMessage(&Message, NULL, true);
//		printf("\n Received %d UDS responses:\n", msgCount);
//		for (DWORD i = 0 ; i < msgCount && i < msgBufLen ; i++)
//			displayMessage(NULL, &MessageBuffer[i]);
//	}
//	else
//		displayMessage(&Message, NULL);
//	waitGetch();
//}

//// Sample to use event
//void testUsingEvent(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
//{	
//	TPUDSStatus Status;
//	TPUDSMsg Message = {};
//	TPUDSMsg MessageResponse = {};	
//	HANDLE hEvent, hTemp;
//	DWORD res;
//	// initialization
//	Message.NETADDRINFO = N_AI;
//	// set event handler
//	hEvent = CreateEvent(NULL, false, false, NULL);
//	Status = UDS_SetValue(Channel, PUDS_PARAM_RECEIVE_EVENT, &hEvent, sizeof(hEvent));
//	if (Status != PUDS_ERROR_OK)
//	{
//		printf("Failed to set event, aborting...");
//		CloseHandle(hEvent);
//		waitGetch();
//		return;
//	}

//	CLEAR_CONSOLE
//		printf("\n\n*** UDS Service with Event: TesterPresent ***\n");

//	// Sends a Physical TesterPresent Message
//	printf("\n\nSends a Physical TesterPresent Message: \n");
//	Status = UDS_SvcTesterPresent(Channel, &Message);
//	printf("  UDS_SvcTesterPresent: %i\n", (int)Status);
//	if (Status == PUDS_ERROR_OK)
//	{
//		// instead of calling WaitForService function,
//		// this sample demonstrates how event can be used.
//		//	But note that the use of a thread listening to notifications
//		//	and making the read operations is preferred.
//		bool bStop = false;
//		// wait until we receive expected response
//		do
//		{
//			res = WaitForSingleObject(hEvent, INFINITE);
//			if (res == WAIT_OBJECT_0)
//			{
//				// read all messages
//				do
//				{
//					Status = UDS_Read(Channel, &MessageResponse);
//					if (Status == PUDS_ERROR_OK) {
//						// this is a simple message check (type and sender/receiver address):
//						// to filter UDS request confirmation and get first message from target,
//						// but real use-case should check that the UDS service ID matches the request
//						if (MessageResponse.MSGTYPE == PUDS_MESSAGE_TYPE_CONFIRM && 
//							MessageResponse.NETADDRINFO.SA == N_AI.TA &&
//							MessageResponse.NETADDRINFO.TA == N_AI.SA) {
//							bStop = true;
//							displayMessage(&Message, &MessageResponse);
//						}
//					}
//				} while (Status != PUDS_ERROR_NO_MESSAGE);
//			}
//		} while (!bStop);
//	}
//	waitGetch();

//	// uninitialize event
//	hTemp = 0;
//	UDS_SetValue(Channel, PUDS_PARAM_RECEIVE_EVENT, &hTemp, sizeof(hTemp));
//	CloseHandle(hEvent);
//}

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
