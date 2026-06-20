FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY budgetflow-api/*.csproj ./
RUN dotnet restore

COPY budgetflow-api/ ./
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BudgetFlow.API.dll"]
