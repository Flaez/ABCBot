FROM microsoft/dotnet:2.1-runtime-alpine

WORKDIR /app
EXPOSE 3000

COPY bin/Release/netcoreapp2.0/publish /app

ENTRYPOINT ["dotnet", "ABCBot.dll"]