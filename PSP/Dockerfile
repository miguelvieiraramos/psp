FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PSP/PSP.csproj", "PSP/"]
RUN dotnet restore "PSP/PSP.csproj"
COPY . .
WORKDIR "/src/PSP"
RUN dotnet build "PSP.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS development
COPY . /source
COPY ../init.sql /source
WORKDIR /source
CMD dotnet run --no-launch-profile

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PSP.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PSP.dll"]
