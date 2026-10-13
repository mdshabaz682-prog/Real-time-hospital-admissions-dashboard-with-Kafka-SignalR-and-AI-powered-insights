FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY HospitalDashboard.Api/ HospitalDashboard.Api/
COPY HospitalDashboard.Worker/ HospitalDashboard.Worker/
RUN rm -f HospitalDashboard.Api/appsettings*.json
RUN dotnet publish HospitalDashboard.Worker/HospitalDashboard.Worker.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "HospitalDashboard.Worker.dll"]