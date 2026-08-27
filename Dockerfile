FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/FinanceFlow.Domain/FinanceFlow.Domain.csproj src/FinanceFlow.Domain/
COPY src/FinanceFlow.Infrastructure/FinanceFlow.Infrastructure.csproj src/FinanceFlow.Infrastructure/
COPY src/FinanceFlow.Api/FinanceFlow.Api.csproj src/FinanceFlow.Api/
RUN dotnet restore src/FinanceFlow.Api/FinanceFlow.Api.csproj

COPY . .
RUN dotnet publish src/FinanceFlow.Api/FinanceFlow.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FinanceFlow.Api.dll"]

# FinanceFlow dashboard repair trigger
