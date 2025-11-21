FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["UserService.Api/UserService.Api.csproj", "UserService.Api/"]
COPY ["UserService.Contract/UserService.Contract.csproj", "UserService.Contract/"]
COPY ["UserService.Model/UserService.Model.csproj", "UserService.Model/"]
COPY ["UserService.Service/UserService.Service.csproj", "UserService.Service/"]
COPY ["UserService.Data.Core/UserService.Data.Core.csproj", "UserService.Data.Core/"]
COPY ["UserService.Data.Repositories/UserService.Data.Repositories.csproj", "UserService.Data.Repositories/"]
RUN dotnet restore "UserService.Api/UserService.Api.csproj"
COPY . .
WORKDIR "/src/UserService.Api"
RUN dotnet build "UserService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "UserService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserService.Api.dll"]
