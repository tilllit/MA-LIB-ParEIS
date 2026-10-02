#include "xparameters.h"
#include "xcanps.h"
#include "xscugic.h"
#include "xil_exception.h"
#include "xil_printf.h"
#include "xstatus.h"
#include "sleep.h"





// ######################################################
// Make following definitions to configure the PS CAN bus
// ######################################################


// Used CAN periperie
#define CAN0
//#define CAN1

// CAN speed
//#define Baud_500k
#define Baud_1M


// ######################################################
// 					END USER Defines
// ######################################################



// HW defines
#ifdef CAN0
	#define CAN_DEVICE_ID       XPAR_XCANPS_0_DEVICE_ID
	#define CAN_INTR_ID         XPAR_XCANPS_0_INTR
#elif defined(CAN1)
	#define CAN_DEVICE_ID       XPAR_XCANPS_1_DEVICE_ID
	#define CAN_INTR_ID         XPAR_XCANPS_1_INTR
#endif

#define INTC_DEVICE_ID      XPAR_SCUGIC_SINGLE_DEVICE_ID

#define XCANPS_MAX_FRAME_SIZE_IN_WORDS 4

#define CANBUS_CIRCULAR_BUFFER_SIZE 150

typedef struct
{
	uint32_t rxCANBusID;
	uint8_t CANBusData[8];
	uint8_t CANDataLength;
} CANBusCircularBuffer_td;

/*
 * Beispiel für 500 kbit/s bei 100 MHz CAN-Clock:
 *
 * Baudrate = CAN_CLK / ((BRPR + 1) * (1 + TS1 + TS2))
 *
 * Hier:
 * 100 MHz / ((9 + 1) * (1 + 15 + 4)) = 500 kbit/s
 *
 * Achtung:
 * XCanPs_SetBitTiming() erwartet SJW, TS2 und TS1 jeweils als "actual - 1".
 */
#ifdef Baud_500k
	#define CAN_BRPR            9
	#define CAN_SJW             0
	#define CAN_TS2             3
	#define CAN_TS1             14
#elif defined(Baud_1M)
	#define CAN_BRPR            4
	#define CAN_SJW             1
	#define CAN_TS2             3
	#define CAN_TS1             14
#endif



// ##########################################
//				FUNCTIONS
// ##########################################

void CANbus_Init(void);
void CANbus_Process(void);
void CANTransmit(uint16_t canID, uint8_t *buffer, uint8_t length);

void CANbus_TX_Done(void);
void CANbus_Error(void);
void CAN_Receive_Callback(uint16_t canID, uint8_t *buffer, uint8_t length);
