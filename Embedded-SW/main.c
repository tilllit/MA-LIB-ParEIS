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



#include <stdio.h>
#include "Application.h"

int main()
{
	printf("Application - START \r\n");

	App_Init();

	while(1)
	{
		App_Loop();
	}
}
