# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /App

# Copiar archivos del proyecto y restaurar dependencias
COPY . ./
RUN dotnet restore

# Publicar en modo Release
RUN dotnet publish -c Release -o out

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /App

# Copiar artefactos publicados desde la etapa build
COPY --from=build /App/out .

# Exponer el puerto 80 dentro del contenedor
EXPOSE 80

# Comando de inicio
ENTRYPOINT ["dotnet", "VisualNetworkAPI.dll"]
