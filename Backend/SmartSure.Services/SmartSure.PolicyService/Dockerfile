FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY NuGet.Config SmartSure.Services/SmartSure.PolicyService/NuGet.Config
COPY SmartSure.Shared/SmartSure.Shared.csproj SmartSure.Shared/
COPY SmartSure.Services/SmartSure.PolicyService/SmartSure.PolicyService.csproj SmartSure.Services/SmartSure.PolicyService/
RUN dotnet restore SmartSure.Services/SmartSure.PolicyService/SmartSure.PolicyService.csproj --configfile SmartSure.Services/SmartSure.PolicyService/NuGet.Config
COPY SmartSure.Shared/ SmartSure.Shared/
COPY SmartSure.Services/SmartSure.PolicyService/ SmartSure.Services/SmartSure.PolicyService/
RUN dotnet publish SmartSure.Services/SmartSure.PolicyService/SmartSure.PolicyService.csproj \
    -c Release -o /app/publish --no-restore --configfile SmartSure.Services/SmartSure.PolicyService/NuGet.Config

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SmartSure.PolicyService.dll"]
