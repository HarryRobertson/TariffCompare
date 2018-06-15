Harry Robertson, 2018
harryrobertson@live.co.uk

TariffCompare is compatible with Windows and Linux, and should also support macOS although I do not have access to a Mac at this 
time to test this.

Compiler Requirements:
Windows: Natively supported.
Linux: https://www.microsoft.com/net/download/linux-package-manager/ubuntu16-04/sdk-current (choose your distribution)
macOS: https://download.microsoft.com/download/8/8/5/88544F33-836A-49A5-8B67-451C24709A8F/dotnet-sdk-2.1.300-osx-gs-x64.pkg

Build Instructions:
1) Navigate to the folder 'TariffCompare'
2) Run 'dotnet build -c Release'

Test Instructions:
1) Navigate to 'TariffCompare/TariffCompare.Standard.Test/bin/Release/netcoreapp2.0/'
2) Run 'dotnet vstest TariffCompare.Standard.Test.dll'

Run Instructions:
1) Navigate to 'TariffCompare/TariffCompare.Console/bin/Release/netcoreapp2.0/'
2) Run 'dotnet TariffCompare.Console.dll <arguments>'
3) Accepted arguments are:
    cost <POWER_USAGE> <GAS_USAGE>
    usage <TARIFF_NAME> <FUEL_TYPE> <TARGET_MONTHLY_SPEND>

Notes:
1) When running in "usage" mode I assumed that the TARGET_MONTHLY_SPEND includes a standing charge, as this is not otherwise defined. 
   This is configurable in 'TariffCompare.Console.dll.config' by uncommenting the setting 'targetMonthlySpend_includesStandingCharge' 
   and setting it to false.
2) This has been a fun project. I've learned a lot, including how to run .NET on Linux. Thanks.