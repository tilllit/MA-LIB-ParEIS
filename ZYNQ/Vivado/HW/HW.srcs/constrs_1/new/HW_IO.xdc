## Acquisition Pins

set_property -dict { PACKAGE_PIN T15   IOSTANDARD LVCMOS33 } [get_ports { conv_0 }];          # Conversion Pin
set_property -dict { PACKAGE_PIN T10   IOSTANDARD LVCMOS33 } [get_ports { meas_clk_0 }];      # CLK Pin
set_property -dict { PACKAGE_PIN T14   IOSTANDARD LVCMOS33 } [get_ports { busy_0 }];          # Busy Pin

set_property -dict { PACKAGE_PIN Y19   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[0] }];  # Data Pin 1
set_property -dict { PACKAGE_PIN Y17   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[1] }];  # Data Pin 2
set_property -dict { PACKAGE_PIN V18   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[2] }];  # Data Pin 3
set_property -dict { PACKAGE_PIN W16   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[3] }];  # Data Pin 4
set_property -dict { PACKAGE_PIN U15   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[4] }];  # Data Pin 5
set_property -dict { PACKAGE_PIN V13   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[5] }];  # Data Pin 6
set_property -dict { PACKAGE_PIN U12   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[6] }];  # Data Pin 7
set_property -dict { PACKAGE_PIN Y14   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[7] }];  # Data Pin 8
set_property -dict { PACKAGE_PIN G18   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[8] }];  # Data Pin 9
set_property -dict { PACKAGE_PIN D20   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[9] }];  # Data Pin 10

set_property -dict { PACKAGE_PIN H18   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[10] }]; # Data Pin 11
set_property -dict { PACKAGE_PIN F20   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[11] }]; # Data Pin 12
set_property -dict { PACKAGE_PIN G20   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[12] }]; # Data Pin 13
set_property -dict { PACKAGE_PIN R17   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[13] }]; # Data Pin 14
set_property -dict { PACKAGE_PIN R18   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[14] }]; # Data Pin 15
set_property -dict { PACKAGE_PIN U17   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[15] }]; # Data Pin 16
set_property -dict { PACKAGE_PIN W19   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[16] }]; # Data Pin 17
set_property -dict { PACKAGE_PIN H15   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[17] }]; # Data Pin 18
set_property -dict { PACKAGE_PIN E17   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[18] }]; # Data Pin 19
set_property -dict { PACKAGE_PIN D18   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[19] }]; # Data Pin 20

set_property -dict { PACKAGE_PIN G15   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[20] }]; # Data Pin 21
set_property -dict { PACKAGE_PIN H17   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[21] }]; # Data Pin 22
set_property -dict { PACKAGE_PIN J16   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[22] }]; # Data Pin 23
set_property -dict { PACKAGE_PIN J19   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[23] }]; # Data Pin 24
set_property -dict { PACKAGE_PIN L17   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[24] }]; # Data Pin 25
set_property -dict { PACKAGE_PIN M20   IOSTANDARD LVCMOS33 } [get_ports { card_data_0[25] }]; # Data Pin 26




## CAN BUS Pins RX & TX

set_property -dict { PACKAGE_PIN L20   IOSTANDARD LVCMOS33 } [get_ports { CAN0_PHY_TX_0 }]; # CAN BUS TX
set_property -dict { PACKAGE_PIN L19   IOSTANDARD LVCMOS33 } [get_ports { CAN0_PHY_RX_0 }]; # CAN BUS RX