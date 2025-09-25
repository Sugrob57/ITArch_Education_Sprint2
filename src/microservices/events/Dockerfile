FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
#RUN ls

# Копируем csproj и восстанавливаем пакеты (чтобы кэшировалось)
COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish . --ignore-failed-sources --disable-parallel -c Release -o /app

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8082
ENTRYPOINT dotnet KafkaEventsApi.dll;
