# docker build -t ytconverter:1.0 -f .\DotNetConverter\Dockerfile .
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5020

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["../DotNetConverter/DotNetConverter.csproj", "DotNetConverter/"]
COPY ["../DotNetConverter.Data/DotNetConverter.Data.csproj", "DotNetConverter.Data/"]
RUN dotnet restore "DotNetConverter/DotNetConverter.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "DotNetConverter/DotNetConverter.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DotNetConverter/DotNetConverter.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=mwader/static-ffmpeg /ffmpeg /usr/bin
ENTRYPOINT ["dotnet", "DotNetConverter.dll", "--urls", "http://0.0.0.0:5020"]
