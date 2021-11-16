# KEBA P30c TCPModbus command sample

This sample demonstrates the KEBA P30c TCPModbus support to change the configured charging current of the charging station, 
which is helpfull if you like to charge with available PV energy and optimze for selfconsumption without changing the Dip switch configuration.

All required settings are in the App.config so that you can define three pre-configured options for the charging current. 
Pre-configured options are default, medium, max. Values are set in Ampere. Max and Medium options could be run by providing the /max or /med commandline argument. Running without an argument configures the default. If you like to switch to fast charging, simply run the exe with /max argument.

```xml
<add key="P30DeviceIP" value="192.168.2.1"/>
<add key="P30UserCurrentAmpereDefault" value="6"/>
<add key="P30UserCurrentAmpereMax" value="16"/>
<add key="P30UserCurrentAmpereMed" value="11"/>
```

A setting of 16A equals charging with 11kw - 6A is minimum, 32A maximum, depending on the configured hardware Dip switches of your charging station.

Supported Arguments:

    /max to set the UserCurrentAmpereMax configuration 
    /med to set the UserCurrentAmpereMed configuration 
    /disable to disable charging station
    /enable to enable charging station
    /report2 to dump config via UDP

Note: If you change the UserCurrent during an active charging session the tool will re-initiate the charging session. To do this the tool will change the config / stop /wait 30sec / start charging station.  The 30sec are for the car to detect properly that new session will be initate.

This sample uses the [EasyModbusTCP.NET library](https://github.com/rossmann-engineering/EasyModbusTCP.NET) feel free to replace with your favorit TCPModbus library.

This code is in use for an BMW I3 and I have not tested other cars.

## Prerequirements

Prepeare your charging station:

- make sure the KEBA P30c charging station has the latest Firmware installed
- the Dip switch configuration is set to support TCP Modbus

Detailed information for the Dip switch configuration, latest Firmware upgrade and TCPModbus specs are available on the KEBA website.

## Getting Started

To run this sample update the App.config with your desired settings:

- Replace P30DeviceIP with the IP of your charging station.
- configure your desired Current rates for the Default, Max and Med pre-defined configurations
