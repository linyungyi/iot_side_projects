# Albert's iot side projects
let's play and be great!!


Chipset

1) M051\NuvotonPlatform_Keil\ISP\UART is the ISP for M051 series

2) NUC100\NuvotonPlatform_Keil\Slave\UsbUART is the ISP for NUC100 series and communication with "NuMicro ISP Programming Tool.exe"
	
3) NUC100\NuvotonPlatform_Keil\Slave\I2C0 and I2C1 is the device application loaded into LDROM for two MCU communication if update APROM using I2C interface
	 
4) NUC100\NuvotonPlatform_Keil\Slave\SPI is the device application loaded into LDROM for two MCU communication if update APROM using SPI interface

5) NUC100\NuvotonPlatform_Keil\Master is the host application for two MCU communication using I2C or SPI

6) NUC100\NuvotonPlatform_Keil\Bridge\Usb2IIC is the host application for two MCU communication using I2C. the bridge code transmit data received from USB to I2C

7) NUC100\NuvotonPlatform_Keil\Slave\Aprom is a example of device application loaded into APROM for two MCU communication

Host

8) MyHidLib is a dll to transmit data over HID.

9) MyHidLibTest is a simple GUI tester with MyHidLib.

10) TKToolkit is a toolkit to program iot sensor through NUC100 bridge. 

Chipset

1) MTK linklt Connect 7681 demo how to support station and ap mode.

Host

2) LinkIt_Connect_7681_android is a android app to connect 7681.

3) LinkIt_Connect_7681_ios is a ios app to connect 7681.