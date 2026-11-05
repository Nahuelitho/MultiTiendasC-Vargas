FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000
# Render define la variable PORT; se usa 10000 como valor por defecto.
ENTRYPOINT ["sh", "-c", "dotnet MultiTenantCSharpVargas.dll --urls http://+:${PORT:-10000}"]
