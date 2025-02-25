FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App
COPY . ./

RUN dotnet restore
RUN dotnet publish -c Release -o out -r linux-x64 --self-contained false 

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .

EXPOSE 80

ENTRYPOINT ["dotnet", "HotelAppSearchApi.dll"]

