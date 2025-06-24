# 1. Çalışma zamanı (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# 2. Derleme zamanı (build)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["wedding-memory.csproj", "./"]
RUN dotnet restore "wedding-memory.csproj"
COPY . .
RUN dotnet build "wedding-memory.csproj" -c Release -o /app/build
RUN dotnet publish "wedding-memory.csproj" -c Release -o /app/publish

# 3. Yayın (final image)
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "wedding-memory.dll"]