#include "xparameters.h"

#include "sleep.h"
#include "xil_types.h"

#include <stdio.h>
#include <stdint.h>

#include "xil_io.h"
#include "xil_cache.h"
#include "xil_printf.h"
#include "xstatus.h"

#include "xaxidma.h"
#include "xaxidma_hw.h"

#include "math.h"


// Find HW: Acquisition Core & DMA

#ifndef ACQ_BASEADDR
# ifdef XPAR_ADDCYCLE_ACQUISITION_0_S00_AXI_BASEADDR
#  define ACQ_BASEADDR XPAR_ADDCYCLE_ACQUISITION_0_S00_AXI_BASEADDR
# elif defined(XPAR_ACQUISITION_IP_0_BASEADDR)
#  define ACQ_BASEADDR XPAR_ACQUISITION_IP_0_BASEADDR
# elif defined(XPAR_ACQUISITION_IP_S00_AXI_BASEADDR)
#  define ACQ_BASEADDR XPAR_ACQUISITION_IP_S00_AXI_BASEADDR
# else
#  error "ACQ_BASEADDR nicht gefunden. Bitte xparameters.h pruefen und ACQ_BASEADDR setzen."
# endif
#endif

#if defined(XPAR_AXI_DMA_0_BASEADDR)
#define AXI_DMA_BASEADDR XPAR_AXI_DMA_0_BASEADDR
#elif defined(XPAR_AXIDMA_0_BASEADDR)
#define AXI_DMA_BASEADDR XPAR_AXIDMA_0_BASEADDR
#elif defined(XPAR_XAXIDMA_0_BASEADDR)
#define AXI_DMA_BASEADDR XPAR_XAXIDMA_0_BASEADDR
#else
#error "AXI_DMA_BASEADDR nicht gefunden. Bitte xparameters.h pruefen."
#endif

// Timeout for DMA Init
#define DMA_TIMEOUT              10000000U

#define ACQ_CLK_HZ 100000000u

#define ACQ_REG_START        0x00
#define ACQ_REG_SERIAL_DIV   0x04
#define ACQ_REG_NUM_SAMPLES  0x08
#define ACQ_REG_SAMPLE_DIV   0x0C


// Settings for DMA buffer size
#define MaxNumSamples		20000u
#define NumCards			26
#define BufferSize			NumCards * MaxNumSamples
#define BytesPerSample      8U
#define BufferBytes        (BufferSize * BytesPerSample)


// Function declarations

void Acquisition_Init();
void Acquisition_Setup(uint32_t SerialFreq, uint32_t SampleFreq, uint32_t NumSamples);
void Acquisition_Start();
void Acquisition_Process();
void Acquisition_Finish();

void Acquisition_Finished_Callback(uint64_t* pBuff, uint32_t lenBytes);

int InitDma(void);
//void Decode(uint64_t* pBuff);

void acq_write(u32 offset, u32 value);
u32 acq_read(u32 offset);

void InitRxBuffer(void);
int32_t sign_extend_20(uint32_t x);

