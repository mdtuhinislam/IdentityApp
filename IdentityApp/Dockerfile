# Use the official .NET Core SDK image to build the app
# This is the build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project file and restore dependencies
COPY IdentityApp.csproj ./
RUN dotnet restore

# Copy the rest of the application code
COPY . ./

# Build the application
RUN dotnet publish -c Release -o out

# Use the official ASP.NET Core runtime image
# This is the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the build output from the build image
COPY --from=build /app/out .

# Set the entry point for the application
ENTRYPOINT ["dotnet", "IdentityApp.dll"]
