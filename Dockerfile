FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY CarMarketplace.sln ./
COPY src ./src
RUN dotnet restore CarMarketplace.sln
RUN dotnet publish src/CarMarketplace.API/CarMarketplace.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "CarMarketplace.API.dll"]
