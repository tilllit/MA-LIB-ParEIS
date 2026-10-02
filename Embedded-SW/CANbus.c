#include "CANbus.h"

#include "xparameters.h"
#include "xcanps.h"
#include "xscugic.h"
#include "xil_exception.h"
#include "xil_printf.h"
#include "xstatus.h"
#include "sleep.h"

static void Can_SendHandler(void *CallBackRef);
static void Can_RecvHandler(void *CallBackRef);
static void Can_ErrorHandler(void *CallBackRef, u32 ErrorMask);
static void Can_EventHandler(void *CallBackRef, u32 IntrMask);


// CAN Instances
static XCanPs CanInst;
static XScuGic IntcInst;

// Variables for ring buffer
uint16_t canReadPosition = 0;
uint16_t canWritePosition = 0;
uint16_t canBufferLength = 0;
CANBusCircularBuffer_td canIncommingData[CANBUS_CIRCULAR_BUFFER_SIZE];


/* Empfangspuffer */
static u32 RxFrame[XCANPS_MAX_FRAME_SIZE_IN_WORDS];


void CANbus_Init(void)
{
	// Init CAN bus peripherie
	XCanPs_Config *Config;
	int Status;

	Config = XCanPs_LookupConfig(CAN_DEVICE_ID);
	if (Config == NULL)
	{
		xil_printf("CAN config not found\r\n");
		return;
	}

	Status = XCanPs_CfgInitialize(&CanInst, Config, Config->BaseAddr);
	if (Status != XST_SUCCESS)
	{
		xil_printf("CAN config init failed\r\n");
		return;
	}

	Status = XCanPs_SelfTest(&CanInst);
	if (Status != XST_SUCCESS)
	{
		xil_printf("CAN self test failed\r\n");
		return;
	}

	// Baudrate and Bit-Timing can only be set in Configuration Mode
	XCanPs_EnterMode(&CanInst, XCANPS_MODE_CONFIG);
	while (XCanPs_GetMode(&CanInst) != XCANPS_MODE_CONFIG);

	Status = XCanPs_SetBaudRatePrescaler(&CanInst, CAN_BRPR);
	if (Status != XST_SUCCESS)
	{
		xil_printf("Set CAN prescaler failed\r\n");
		return;
	}

	Status = XCanPs_SetBitTiming(&CanInst, CAN_SJW, CAN_TS2, CAN_TS1);
	if (Status != XST_SUCCESS)
	{
		xil_printf("Set CAN bit timing failed\r\n");
		return;
	}

	// Switch back in normal operation mode
	XCanPs_EnterMode(&CanInst, XCANPS_MODE_NORMAL);
	while (XCanPs_GetMode(&CanInst) != XCANPS_MODE_NORMAL);


	// Init CAN bus Interrupt system
	XScuGic_Config *IntcConfig;

	IntcConfig = XScuGic_LookupConfig(INTC_DEVICE_ID);
	if (IntcConfig == NULL)
	{
		xil_printf("GIC config not found\r\n");
		return;
	}

	Status = XScuGic_CfgInitialize(&IntcInst,
								   IntcConfig,
								   IntcConfig->CpuBaseAddress);
	if (Status != XST_SUCCESS)
	{
		xil_printf("GIC init failed\r\n");
		return;
	}

	/*
	 * Priorität und Trigger-Typ setzen.
	 * 0xA0 ist eine normale mittlere Priorität.
	 * 0x3 bedeutet edge-triggered.
	 *
	 * Falls Interrupts nicht kommen, testweise 0x1 level-sensitive probieren,
	 * je nach BSP/Plattform.
	 */
//	XScuGic_SetPriorityTriggerType(&IntcInst, CAN_INTR_ID, 0xA0, 0x3);
	XScuGic_SetPriorityTriggerType(&IntcInst, CAN_INTR_ID, 0xA0, 0x01);

	/*
	 * CAN-Treiber-ISR an den GIC hängen.
	 * Wichtig: Nicht direkt eigene RX/TX-Funktion hier einhängen,
	 * sondern XCanPs_IntrHandler().
	 * Dieser ruft dann die registrierten XCanPs_SetHandler()-Callbacks auf.
	 */
	Status = XScuGic_Connect(&IntcInst,
							 CAN_INTR_ID,
							 (Xil_ExceptionHandler)XCanPs_IntrHandler,
							 (void *)&CanInst);

	if (Status != XST_SUCCESS)
	{
		xil_printf("GIC connect failed\r\n");
		return;
	}

	XScuGic_Enable(&IntcInst, CAN_INTR_ID);

	// Activate CPU-Exception-System
	Xil_ExceptionInit();
	Xil_ExceptionRegisterHandler(XIL_EXCEPTION_ID_INT,
								 (Xil_ExceptionHandler)XScuGic_InterruptHandler,
								 &IntcInst);
	Xil_ExceptionEnable();


	// Register application-specific callback's
    XCanPs_SetHandler(&CanInst, XCANPS_HANDLER_SEND, (void *)Can_SendHandler, &CanInst);
    XCanPs_SetHandler(&CanInst, XCANPS_HANDLER_RECV, (void *)Can_RecvHandler, &CanInst);
    XCanPs_SetHandler(&CanInst, XCANPS_HANDLER_ERROR, (void *)Can_ErrorHandler, &CanInst);
    XCanPs_SetHandler(&CanInst, XCANPS_HANDLER_EVENT, (void *)Can_EventHandler, &CanInst);

	// Activate application-specific callback's
    XCanPs_IntrEnable(&CanInst, XCANPS_IXR_ALL);
}

void CANbus_Process(void)
{
	if(canBufferLength > 0)
	{
		uint32_t canID = canIncommingData[canReadPosition].rxCANBusID;

		uint8_t rxdata[8];
		memcpy(&rxdata, canIncommingData[canReadPosition].CANBusData, 8);

		// circular buffer handling
		canBufferLength -= 1;
		canReadPosition += 1;
		if(canReadPosition == CANBUS_CIRCULAR_BUFFER_SIZE)
		{
			canReadPosition = 0;
		}

		CAN_Receive_Callback(canID, (uint8_t*) &rxdata, sizeof(rxdata));
	}
}

static void Can_SendHandler(void *CallBackRef)
{
	CANbus_TX_Done();
}

__attribute__((weak)) void CANbus_TX_Done(void)
{
	// Define your own routine somewhere else...
}

static void Can_ErrorHandler(void *CallBackRef, u32 ErrorMask)
{
	CANbus_Error();
}

__attribute__((weak)) void CANbus_Error(void)
{
	// Define your own routine somewhere else...
	xil_printf("CAN error accured! \r\n");
}

static void Can_EventHandler(void *CallBackRef, u32 IntrMask)
{
	// lore ipsum...
}

// Internal RX callback
static void Can_RecvHandler(void *CallBackRef)
{
	// Read frame
	if (XCanPs_Recv((XCanPs *)CallBackRef, RxFrame) == XST_SUCCESS)
	{
		// Write to RingBuffer
		canIncommingData[canWritePosition].rxCANBusID 	 = (RxFrame[0] & XCANPS_IDR_ID1_MASK) >> XCANPS_IDR_ID1_SHIFT;
		canIncommingData[canWritePosition].CANDataLength = (RxFrame[1] & XCANPS_DLCR_DLC_MASK) >> XCANPS_DLCR_DLC_SHIFT;
		canIncommingData[canWritePosition].CANBusData[0] = (RxFrame[2])       & 0xFF;
		canIncommingData[canWritePosition].CANBusData[1] = (RxFrame[2] >> 8)  & 0xFF;
		canIncommingData[canWritePosition].CANBusData[2] = (RxFrame[2] >> 16) & 0xFF;
		canIncommingData[canWritePosition].CANBusData[3] = (RxFrame[2] >> 24) & 0xFF;
		canIncommingData[canWritePosition].CANBusData[4] = (RxFrame[3])       & 0xFF;
		canIncommingData[canWritePosition].CANBusData[5] = (RxFrame[3] >> 8)  & 0xFF;
		canIncommingData[canWritePosition].CANBusData[6] = (RxFrame[3] >> 16) & 0xFF;
		canIncommingData[canWritePosition].CANBusData[7] = (RxFrame[3] >> 24) & 0xFF;

		// Handle RingBuffer
		canBufferLength += 1;
		canWritePosition += 1;
		if(canWritePosition == CANBUS_CIRCULAR_BUFFER_SIZE)
		{
			canWritePosition = 0;
		}
	}
	else
	{
		CANbus_Error();
	}
}

// External RX callback
__attribute__((weak)) void CAN_Receive_Callback(uint16_t canID, uint8_t *buffer, uint8_t length)
{
	// Define your own routine somewhere else...
}


void CANTransmit(uint16_t canID, uint8_t *buffer, uint8_t length)
{
	uint32_t TxFrame[XCANPS_MAX_FRAME_SIZE_IN_WORDS];

    if (length > 8)
    	length = 8;

    TxFrame[0] = XCanPs_CreateIdValue(canID, 0, 0, 0, 0);
    TxFrame[1] = XCanPs_CreateDlcValue(length);

    TxFrame[2] = 0;
    TxFrame[3] = 0;

    if (length > 0) TxFrame[2] |= ((u32)buffer[0]);
    if (length > 1) TxFrame[2] |= ((u32)buffer[1] <<  8);
    if (length > 2) TxFrame[2] |= ((u32)buffer[2] << 16);
    if (length > 3) TxFrame[2] |= ((u32)buffer[3] << 24);
    if (length > 4) TxFrame[3] |= ((u32)buffer[4]);
    if (length > 5) TxFrame[3] |= ((u32)buffer[5] <<  8);
    if (length > 6) TxFrame[3] |= ((u32)buffer[6] << 16);
    if (length > 7) TxFrame[3] |= ((u32)buffer[7] << 24);

	XCanPs_Send(&CanInst, TxFrame);
}


