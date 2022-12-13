# namecheap-dynamic-dns-client

A windows service that dynamically updates the IP address for the specified host profiles.

## How to run

1. Clone the code locally
2. Run `BuildSetup.bat` which should build the project in the `bin` folder
3. Navigate to the `bin` folder
4. Set the config values in `DynDnsClient.dll.config`
    * Period: how often the service should check and update ip address changes
    * Domain: the name of the domain e.g., `example.com`
    * Password: the Dynamic DNS Password supplied by namecheap
    * IgnoredNetworkConnections: a comma separated list of network connections to ignore. This is useful if connecting to a VPN and you don't want the IP address change to trigger an update. The names can be found in `Control Panel\Network and Internet\Network Connections`
5. add host names to `Hosts.txt` file e.g., `www`
6. Run `InstallService.bat`
7. Run `StartService.bat`

## Troubleshooting

A log file can be viewed in the `bin\Logs` directory
