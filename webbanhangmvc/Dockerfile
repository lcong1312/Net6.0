# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.sln .
COPY webbanhangmvc/*.csproj ./webbanhangmvc/
RUN dotnet restore

# Copy everything else and build
COPY webbanhangmvc/. ./webbanhangmvc/
WORKDIR /app/webbanhangmvc
RUN dotnet publish -c Release -o /app/out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/out .

# Expose the port your app runs on (change if needed)
EXPOSE 80
ENTRYPOINT ["dotnet", "webbanhangmvc.dll"]
