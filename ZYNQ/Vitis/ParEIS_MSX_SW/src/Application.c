#include "Application.h"

#include "Acquisition.h"
#include "CANbus.h"

#include "Ethernet.h"

#include "xparameters.h"

#include <stdio.h>
#include <stdint.h>

#include "xil_io.h"
#include "xil_printf.h"
#include "xstatus.h"
#include "sleep.h"
#include "xil_types.h"
#include "math.h"
#include "stdbool.h"

#include "lwip/opt.h"
#include "lwip/stats.h"
#include "lwip/memp.h"
#include "xil_printf.h"


// TCP COM
#define TCP_CMD_Port	5001
#define TCP_DATA_Port	5003
#define PACKET_SIZE_BYTES  1000	// divide DMA buffer to this TCP packet size

// - State Machine -
#define State_Idle				0x00
#define State_Busy				0x01
#define State_LoadConigured		0x02
#define State_FPGAConigured		0x04
#define State_FullConigured		0x06

int STATE = State_Idle;


// Function Declarations
int split(char *str, char delim, char **out, int max_parts);
void Decode(uint64_t* pBuff);



void App_Init()
{
	STATE = State_Busy;

	Acquisition_Init();

	CANbus_Init();

	TCP_Init(192, 168, 1, 11, TCP_CMD_Port);
	TCP_Open_Port(TCP_DATA_Port);

//	// Test CAN
//	char buffer[] = {'-', 'S', 'T', 'A', 'R', 'T', '!', '-'};
//	CANTransmit(0x105, (uint8_t*)buffer, sizeof(buffer));

	STATE = State_Idle;
}



void App_Loop()
{
	Acquisition_Process();
	CANbus_Process();
	Ethernet_Process();
}



void TCP_Receive_Callback(uint16_t Port, uint8_t *buffer, uint16_t length)
{
	if (Port == TCP_CMD_Port)
	{
		char payload[length];
		memcpy(payload, buffer, length);

		// ##### Acquisition Setup #####
		if (strstr(payload, "ACC-Setup") != NULL)
		{
			if (STATE == State_Busy) return;

			printf("TCP RX - Acquisition Config. \r\n");

			char *splitS[4];
			split(payload, ' ', splitS, 4);

			uint32_t Fserial = 		(uint32_t)strtoul(splitS[1], NULL, 10);
			uint32_t Fsample = 		(uint32_t)strtoul(splitS[2], NULL, 10);
			uint32_t NrSamples = 	(uint32_t)strtoul(splitS[3], NULL, 10);

			Acquisition_Setup(Fserial, Fsample, NrSamples);

			STATE |= State_FPGAConigured;
		}

		// ##### E-Load Setup #####
		else if (strstr(payload, "ELOAD-Setup") != NULL)
		{
			if (STATE == State_Busy) return;

			printf("TCP RX - E-Load Config. \r\n");

			// Extract E-Load config like this here!

			char *splitS[8];
			split(payload, ' ', splitS, 8);

			uint16_t canID = 	(uint16_t)(strtoul(splitS[1], NULL, 10) & 0x0FFF);
			uint8_t config = 	(uint8_t) (strtoul(splitS[2], NULL, 10) & 0x00FF);
			uint16_t PSC = 		(uint16_t)(strtoul(splitS[3], NULL, 10) & 0xFFFF);
			uint16_t ARR = 		(uint16_t)(strtoul(splitS[4], NULL, 10) & 0xFFFF);
			uint8_t Amplitude = (uint8_t) (strtoul(splitS[5], NULL, 10) & 0x00FF);
			uint8_t Offset = 	(uint8_t) (strtoul(splitS[6], NULL, 10) & 0x00FF);
			uint8_t Periods = 	(uint8_t) (strtoul(splitS[7], NULL, 10) & 0x00FF);

			// Send E-Load config. over CAN
			uint8_t buffer[8];// = { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
			buffer[0] = config;
			buffer[1] = ((PSC >> 8) & 0xFF);
			buffer[2] = (PSC & 0xFF);
			buffer[3] = ((ARR >> 8) & 0xFF);
			buffer[4] = (ARR & 0xFF);
			buffer[5] = Amplitude;
			buffer[6] = Offset;
			buffer[7] = Periods;

			CANTransmit(canID, (uint8_t*)&buffer, sizeof(buffer));

			STATE |= State_LoadConigured;
		}

		// ##### E-Load Setup #####
		else if (strstr(payload, "ELOAD-Start") != NULL)
		{
			// Check for E-Load  config.
			if (STATE & State_LoadConigured)
			{
				// Activate E-Load
				uint8_t buffer[] = { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
				CANTransmit(0x060, (uint8_t*)&buffer, sizeof(buffer));

				// Wait for E-Load to handle state
				msleep(10);

				// Start E-Load
				buffer[0] = 0x08;
				CANTransmit(0x060, (uint8_t*)&buffer, sizeof(buffer));

				printf("E-Load Started! (Stand-alone)");
			}
		}

		// ##### START #####
		else if (strstr(payload, "START") != NULL)
		{
			if (STATE == State_Busy) return;

			// Check for ACC config.
			if (STATE & State_FPGAConigured)
			{
				printf("TCP RX - Start Acquisition! \r\n");

				// Check for E-Load  config.
				if (STATE & State_LoadConigured)
				{
					uint8_t buffer[] = { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
					CANTransmit(0x060, (uint8_t*)&buffer, sizeof(buffer));

					// Wait for E-Load to handle state
					msleep(10);

					// Start E-Load
					buffer[0] = 0x08;
					CANTransmit(0x060, (uint8_t*)&buffer, sizeof(buffer));

				}

				// Start Acquisition
				STATE = State_Busy;
				Acquisition_Start();
			}
		}

		// ##### STOP #####
		else if (strstr(payload, "STOP") != NULL)
		{
			// Send Broadcast STOP CMD over CAN
			uint8_t buffer[] = { 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
			CANTransmit(0x060, (uint8_t*)&buffer, sizeof(buffer));

			printf("TCP RX - Stop! \r\n");
		}
	}
	if (Port == TCP_DATA_Port)
	{
		printf("TCP RX on Port 5003 \r\n");
	}
}


void CAN_Receive_Callback(uint16_t canID, uint8_t *buffer, uint8_t length)
{
	// decode status here?
	printf("RX msg from CAN ID: %i", canID);
	CANTransmit(canID + 1, buffer, length);
}


void Acquisition_Finished_Callback(uint64_t* pBuff, uint32_t lenBytes)
{
	printf("Buffer length in Bytes: %li \r\n", lenBytes);

	char msg[100];
	snprintf(msg, sizeof(msg), "Data-Out: %li Byte", lenBytes);
	TCP_Send_String_To_Port(TCP_CMD_Port, (const char*)&msg);
	Ethernet_Process();

	TCP_Send_String_To_Port(TCP_CMD_Port, "ACC-Finished!");
	Ethernet_Process();

//	// Output data on data-port
//	uint8_t *bytes = (uint8_t *)pBuff;	// Converts pointer
//	TCP_Send_To_Port(TCP_DATA_Port, bytes, lenBytes);
//	Ethernet_Process();



	// Output data on data-port
	size_t offset = 0;
	const uint8_t *bytes = (const uint8_t *)pBuff;	// Converts pointer
	//TCP_Send_To_Port(TCP_DATA_Port, bytes, lenBytes);


    while (offset < lenBytes)
    {
    	//int free_pbufs = lwip_stats.pbuf.avail - lwip_stats.pbuf.used;

        size_t remaining = lenBytes - offset;

        size_t packet_length =
            (remaining > PACKET_SIZE_BYTES)
            ? PACKET_SIZE_BYTES
            : remaining;

    	TCP_Send_To_Port(TCP_DATA_Port, &bytes[offset], packet_length);
    	Ethernet_Process();
    	Ethernet_Process();
    	Ethernet_Process();

    	offset += packet_length;
    }

	Decode(pBuff);
	STATE = State_Idle;
}


void Decode(uint64_t* pBuff)
{
	for(int i = 0; i < 3; i++)
	{
		uint32_t ADC3 = ((pBuff[i] >> 44) & 0xFFFFF);
		uint32_t ADC2 = ((pBuff[i] >> 24) & 0xFFFFF);
		uint32_t ADC1 = ((pBuff[i] >>  4) & 0xFFFFF);

		int32_t adc3_s = sign_extend_20(ADC3);
		int32_t adc2_s = sign_extend_20(ADC2);
		int32_t adc1_s = sign_extend_20(ADC1);

		float volt_1 = (float)(adc1_s * 4.09 / (pow(2,20)) / 15.0);
		float volt_2 = (float)(adc2_s * 4.09 / (pow(2,20)) / 15.0);
		float volt_3 = (float)(adc3_s * 4.09 / (pow(2,20)) / 15.0);
		printf("Calc: 64b: %llu   20b unit: %07ld   20b int: %07ld   float: %5.4f \r\n", pBuff[i], ADC3, adc3_s, volt_3);
		printf("Received: %07ld | %07ld | %07ld    |   Voltage: %5.4f | %5.4f | %5.4f 2er: %07ld | %07ld | %07ld \r\n", ADC1, ADC2, ADC3, volt_1, volt_2, volt_3, adc1_s, adc2_s, adc3_s);
	}
}





// ##################### UTILLITY FUNCTIONS #####################

int split(char *str, char delim, char **out, int max_parts)
{
    int count = 0;
    //char delim_str[2] = { delim, '\0' };
    char delim_str[2] = { delim };
    char *token = strtok(str, delim_str);


    while (token && count < max_parts) {
        out[count++] = token;
        token = strtok(NULL, delim_str);
    }
    return count;
}

int32_t sign_extend_20(uint32_t x)
{
    if (x & 0x80000)
        x |= 0xFFF00000;

    return (int32_t)x;
}
