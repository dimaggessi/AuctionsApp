FROM mcr.microsoft.com/dotnet/sdk:8.0 as build

# working directory inside docker file system
WORKDIR /app
# expose port that this container should listen on.
EXPOSE 80

# copy all .csproj files and restore as distinct layers.
# same COPY command for every Dockerfile in the project 
# to take advantage of docker caching (faster build)
# COPY the .sln to workdir on docker container /src
COPY AuctionsApp.sln AuctionsApp.sln
COPY src/AuctionService/AuctionService.csproj src/AuctionService/AuctionService.csproj
COPY src/SearchService/SearchService.csproj src/SearchService/SearchService.csproj
COPY src/GatewayService/GatewayService.csproj src/GatewayService/GatewayService.csproj
COPY src/BiddingService/BiddingService.csproj src/BiddingService/BiddingService.csproj
COPY src/NotificationService/NotificationService.csproj src/NotificationService/NotificationService.csproj
COPY src/Contracts/Contracts.csproj src/Contracts/Contracts.csproj
COPY src/IdentityService/IdentityService.csproj src/IdentityService/IdentityService.csproj
COPY tests/AuctionService.UnitTests/AuctionService.UnitTests.csproj tests/AuctionService.UnitTests/AuctionService.UnitTests.csproj
COPY tests/AuctionService.IntegrationTests/AuctionService.IntegrationTests.csproj tests/AuctionService.IntegrationTests/AuctionService.IntegrationTests.csproj

# Restore package dependencies
# access to dotnet cli because the presence of sdk:8.0
RUN dotnet restore AuctionsApp.sln

# Copy the app folders over 
COPY src/SearchService src/SearchService
COPY src/Contracts src/Contracts
WORKDIR /app/src/SearchService
RUN dotnet publish -c Release -o /app/src/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/src/out .
ENTRYPOINT [ "dotnet", "SearchService.dll" ]