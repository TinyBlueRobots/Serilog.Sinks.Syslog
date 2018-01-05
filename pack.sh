export FrameworkPathOverride=$(dirname $(which mono))/../lib/mono/4.5/
NUGETVERSION=1.0.8
dotnet pack Serilog.Sinks.Syslog -c Release /p:PackageVersion=$NUGETVERSION
dotnet nuget push Serilog.Sinks.Syslog/bin/Release/Serilog.Sinks.SyslogServer.$NUGETVERSION.nupkg -k $NUGETKEY -s nuget.org
