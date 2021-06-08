FROM mcr.microsoft.com/dotnet/sdk:5.0.300-buster-slim AS build-env
WORKDIR /app

COPY ./*sln ./

COPY ./src/DanishAddressSeed/*.csproj ./src/DanishAddressSeed/

RUN dotnet restore --packages ./packages

COPY . ./
WORKDIR /app/src/DanishAddressSeed
RUN dotnet publish -c Release -o out --packages ./packages

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:5.0
WORKDIR /app

COPY --from=build-env /app/src/DanishAddressSeed/out .
ENTRYPOINT ["dotnet", "DanishAddressSeed.dll"]