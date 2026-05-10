FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
RUN mkdir -p /data
VOLUME ["/data"]

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DailyBudget.Api.csproj", "./"]
RUN dotnet restore "DailyBudget.Api.csproj"
COPY . ./

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DailyBudget.Api.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DailyBudget.Api.dll"]
