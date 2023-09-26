FROM mcr.microsoft.com/dotnet/sdk:3.1 as build-env
WORKDIR /app
COPY src .
RUN dotnet publish -c Release -o /publish WebUI/WebUI.csproj

FROM mcr.microsoft.com/dotnet/aspnet:3.1 as runtime
WORKDIR /publish
COPY --from=build-env /publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "WebUI.dll"]