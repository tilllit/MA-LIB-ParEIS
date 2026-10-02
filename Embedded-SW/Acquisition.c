#include "Acquisition.h"

#include "xil_io.h"
#include "xil_cache.h"

#include "stdbool.h"


/*
 * 64-byte aligned address for DAM
 * Every AXI-Stream-Transfer: 64 bit = 8 byte.
 */
static uint64_t RxBuffer[BufferSize] __attribute__((aligned(64)));

static XAxiDma AxiDma;
static bool acq_active = false;
uint32_t realBufferLength = 0;



int InitDma(void)
{
    int status;
    XAxiDma_Config *cfg;

#if defined(XPAR_AXIDMA_0_DEVICE_ID)
    cfg = XAxiDma_LookupConfig(XPAR_AXIDMA_0_DEVICE_ID);
#else
    cfg = XAxiDma_LookupConfigBaseAddr(AXI_DMA_BASEADDR);
#endif

    if (cfg == NULL) {
        xil_printf("ERROR: XAxiDma_LookupConfig fehlgeschlagen\r\n");
        return XST_FAILURE;
    }

    status = XAxiDma_CfgInitialize(&AxiDma, cfg);
    if (status != XST_SUCCESS) {
        xil_printf("ERROR: XAxiDma_CfgInitialize fehlgeschlagen: %d\r\n", status);
        return XST_FAILURE;
    }

    if (XAxiDma_HasSg(&AxiDma)) {
        xil_printf("ERROR: DMA ist im Scatter-Gather Mode. Erwartet wird Simple Mode.\r\n");
        return XST_FAILURE;
    }

    XAxiDma_IntrDisable(&AxiDma, XAXIDMA_IRQ_ALL_MASK, XAXIDMA_DEVICE_TO_DMA);

    XAxiDma_Reset(&AxiDma);

    u32 timeout = DMA_TIMEOUT;
    while (!XAxiDma_ResetIsDone(&AxiDma)) {
        timeout--;
        if (timeout == 0) {
            xil_printf("ERROR: DMA Reset Timeout\r\n");
            return XST_FAILURE;
        }
    }

    xil_printf("DMA init OK\r\n");
    return XST_SUCCESS;
}



//static void acq_start(uint32_t baseaddr,
//                      uint32_t serial_hz,
//                      uint32_t sample_hz,
//                      uint32_t num_samples)
//{
//    uint32_t serial_half_period_clks;
//    uint32_t sample_period_clks;
//
//    serial_half_period_clks = ACQ_CLK_HZ / (2u * serial_hz);
//    sample_period_clks      = ACQ_CLK_HZ / sample_hz;
//
//    if (serial_half_period_clks == 0u) {
//        serial_half_period_clks = 1u;
//    }
//
//    if (sample_period_clks == 0u) {
//        sample_period_clks = 1u;
//    }
//
//    Xil_Out32(baseaddr + ACQ_REG_SERIAL_DIV,  serial_half_period_clks);
//    Xil_Out32(baseaddr + ACQ_REG_NUM_SAMPLES, num_samples);
//    Xil_Out32(baseaddr + ACQ_REG_SAMPLE_DIV,  sample_period_clks);
//
//    Xil_Out32(baseaddr + ACQ_REG_START, 1u);
//}

void Acquisition_Init()
{
	InitDma();
}

void Acquisition_Setup(uint32_t SerialFreq, uint32_t SampleFreq, uint32_t NumSamples)
{
	Xil_DCacheInvalidateRange((UINTPTR)RxBuffer, BufferBytes);

	InitRxBuffer();

	Xil_DCacheFlushRange((UINTPTR)RxBuffer, BufferBytes);

	realBufferLength = NumCards * NumSamples * BytesPerSample;
    int status = XAxiDma_SimpleTransfer(&AxiDma, (UINTPTR)RxBuffer, realBufferLength, XAXIDMA_DEVICE_TO_DMA);

	printf("Config: SampleRate: %li, NrSamples: %li, BufferSizeInBytes: %li \r\n", SampleFreq, NumSamples, realBufferLength);

    if (status != XST_SUCCESS)
    {
        xil_printf("ERROR: XAxiDma_SimpleTransfer failed: %d\r\n", status);
//        return XST_FAILURE;
    }

	// Calculate counter values from frequencys
    uint32_t serial_half_period_clks;
    uint32_t sample_period_clks;

    serial_half_period_clks = ACQ_CLK_HZ / (2u * SerialFreq);
    sample_period_clks      = ACQ_CLK_HZ / SampleFreq;

    if (serial_half_period_clks == 0u)
    {
        serial_half_period_clks = 1u;
    }

    if (sample_period_clks == 0u)
    {
        sample_period_clks = 1u;
    }

    acq_write(ACQ_REG_SERIAL_DIV,  serial_half_period_clks);
    acq_write(ACQ_REG_NUM_SAMPLES, NumSamples);
    acq_write(ACQ_REG_SAMPLE_DIV,  sample_period_clks);
}

void Acquisition_Start()
{
	acq_active = true;
	acq_write(ACQ_REG_START, 1u);
}

// This function is meant to wait for end of acquesition in non blocking mode
void Acquisition_Process()
{
	if(!XAxiDma_Busy(&AxiDma, XAXIDMA_DEVICE_TO_DMA))
	{
		if (acq_active == true)
			Acquisition_Finish();
	}
}

void Acquisition_Finish()
{
    Xil_DCacheInvalidateRange((UINTPTR)RxBuffer, BufferBytes);
	acq_active = false;

	Acquisition_Finished_Callback((uint64_t*)&RxBuffer, (uint32_t)realBufferLength);
}

__attribute__((weak)) void Acquisition_Finished_Callback(uint64_t* pBuff, uint32_t lenBytes)
{
	// Define your own routine somewhere else...
	xil_printf("\r\n--- Acquisition finished successfully!!!! ---\r\n");

//	Decode();

//	sleep(2);
//	Acquisition_Setup(100000, 50, 5);
//	// Send E-Load start over canbus here
//	Acquisition_Start();
}



// ######################################
// 			Hilfsfunktionen
// ######################################

void InitRxBuffer(void)
{
    for (u32 i = 0; i < BufferSize; i++)
    {
        RxBuffer[i] = 0xDEADBEEFDEADBEEFULL;
    }
}

//void Decode(void)
//{
//	uint32_t ADC3 = ((RxBuffer[0] >> 44) & 0xFFFFF);
//	uint32_t ADC2 = ((RxBuffer[0] >> 24) & 0xFFFFF);
//	uint32_t ADC1 = ((RxBuffer[0] >>  4) & 0xFFFFF);
//
//	int32_t adc3_s = sign_extend_20(ADC3);
//	int32_t adc2_s = sign_extend_20(ADC2);
//	int32_t adc1_s = sign_extend_20(ADC1);
//
//	float volt_1 = (float)(adc1_s * 4.09 / (pow(2,20)) / 15.0);
//	float volt_2 = (float)(adc2_s * 4.09 / (pow(2,20)) / 15.0);
//	float volt_3 = (float)(adc3_s * 4.09 / (pow(2,20)) / 15.0);
//	printf("Received: %07ld | %07ld | %07ld    |   Voltage: %5.4f | %5.4f | %5.4f 2er: %07ld | %07ld | %07ld \r\n", ADC1, ADC2, ADC3, volt_1, volt_2, volt_3, adc1_s, adc2_s, adc3_s);
//}

void acq_write(u32 offset, u32 value)
{
    Xil_Out32((UINTPTR)ACQ_BASEADDR + offset, value);
}


u32 acq_read(u32 offset)
{
    return Xil_In32((UINTPTR)ACQ_BASEADDR + offset);
}


//int32_t sign_extend_20(uint32_t x)
//{
//    if (x & 0x80000)
//        x |= 0xFFF00000;
//
//    return (int32_t)x;
//}
