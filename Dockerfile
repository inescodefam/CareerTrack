FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source


COPY CareerTrack.sln .
COPY CareerTrack/CareerTrack.csproj CareerTrack/


RUN dotnet restore CareerTrack/CareerTrack.csproj


COPY . .
RUN dotnet publish CareerTrack/CareerTrack.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "CareerTrack.dll"]
