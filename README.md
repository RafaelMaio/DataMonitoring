# DataMonitoring

MRTK application for the Microsoft HoloLens 2 for supporting the monitorization of real-time data of industrial assembly lines.

### Versions:

 - Unity 2021.3.9f1
 - MRTK 2.8.2
 - Visual Studio 2019
 
 ### Use the project:
 1. Open the project folder in a compatible Unity version.
 2. Build the application. Build settings:
    - Choose UWP;
    - Target device: HoloLens;
    - Architecture: ARM64;
    - Minimum platform version: 10.0.18362.0;
    - Build and Run on: USB Device;
    - Switch platform and build.
 3. Open the file with ".sln" extension in the build folder created.
 4. In the top menu, choose "Realease" and "ARM64"
 5. Run to the device with cable or remotely via wi-fi.

## Aplication usage:
1. Select the assembly line number.
2. Configure the stations constituing the assembly line.
3. Perform the real-time data monitorization of the corresponding stations.

## To use simulated data:
1. Transfer the API from https://github.com/tiagodavi70/dashboard_linha_montagem .
2. Run the server as described in the github link provided.
3. The server and the device need to be connected to the same network.
4. Change the IP4 address in code to the IP of the device.
5. Build and deploy the project to the device.
6. Select the checkbox "User Test".
