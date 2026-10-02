# 
# Usage: To re-create this platform project launch xsct with below options.
# xsct C:\Users\tillkorsmeier\Desktop\MA\Z7-Nano\ParEIS-MSX\SW\HW_Export_V1\platform.tcl
# 
# OR launch xsct and run below command.
# source C:\Users\tillkorsmeier\Desktop\MA\Z7-Nano\ParEIS-MSX\SW\HW_Export_V1\platform.tcl
# 
# To create the platform in a different location, modify the -out option of "platform create" command.
# -out option specifies the output directory of the platform project.

platform create -name {HW_Export_V1}\
-hw {C:\Users\tillkorsmeier\Desktop\MA\Z7-Nano\ParEIS-MSX\SW\HW_Export_V1.xsa}\
-out {C:/Users/tillkorsmeier/Desktop/MA/Z7-Nano/ParEIS-MSX/SW}

platform write
domain create -name {standalone_ps7_cortexa9_0} -display-name {standalone_ps7_cortexa9_0} -os {standalone} -proc {ps7_cortexa9_0} -runtime {cpp} -arch {32-bit} -support-app {empty_application}
platform generate -domains 
platform active {HW_Export_V1}
domain active {zynq_fsbl}
domain active {standalone_ps7_cortexa9_0}
platform generate -quick
bsp reload
bsp setlib -name lwip220 -ver 1.1
bsp setlib -name xiltimer -ver 2.1
bsp config mem_size "1000000"
bsp config memp_n_tcp_seg "65001"
bsp config phy_link_speed "CONFIG_LINKSPEED100"
bsp config tcp_snd_buf "65000"
bsp write
bsp reload
catch {bsp regenerate}
platform generate
platform active {HW_Export_V1}
platform config -updatehw {C:/Users/tillkorsmeier/Desktop/MA/Z7-Nano/ParEIS-MSX/SW/HW_Export_V2.xsa}
bsp reload
bsp config phy_link_speed "CONFIG_LINKSPEED1000"
bsp write
bsp reload
catch {bsp regenerate}
platform generate -domains standalone_ps7_cortexa9_0 
platform clean
platform generate
platform active {HW_Export_V1}
platform generate -domains 
bsp reload
bsp config phy_link_speed "CONFIG_LINKSPEED100"
bsp write
bsp reload
catch {bsp regenerate}
platform generate -domains standalone_ps7_cortexa9_0 
platform clean
platform generate
platform active {HW_Export_V1}
