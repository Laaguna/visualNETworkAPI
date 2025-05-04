# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /App

# Copiar archivos del proyecto y restaurar dependencias
COPY . ./
RUN dotnet restore

# Publicar en modo release
RUN dotnet publish -c Release -o out

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /App

# Copiar archivos publicados desde la etapa build
COPY --from=build /App/out .

# Copiar el archivo de configuración correcto
ARG configuration=Release
RUN echo "Current Configuration: $configuration"
COPY VisualNetworkAPI/appsettings${configuration}.json /App/appsettings.json

# Expone el puerto 80 dentro del contenedor
EXPOSE 80

# Comando de entrada
ENTRYPOINT ["dotnet", "VisualNetworkAPI.dll"]
