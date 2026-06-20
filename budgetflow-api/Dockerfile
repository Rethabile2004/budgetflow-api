FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY BudgetFlow.API/*.csproj ./
RUN dotnet restore
COPY BudgetFlow.API/ ./
RUN rm -f appsettings.Development.json
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BudgetFlow.API.dll"]