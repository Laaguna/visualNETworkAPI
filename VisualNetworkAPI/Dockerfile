# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /App

# Copiar archivo .csproj
COPY VisualNetworkAPI.csproj ./
RUN dotnet restore

# Copiar Program.cs explícitamente (para probar)
COPY Program.cs ./

# Copiar el resto del proyecto
COPY . ./

# Publicar en modo Release
RUN dotnet publish -c Release -o out

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /App

# Copiar el SDK desde la etapa de build
COPY --from=build /usr/share/dotnet /usr/share/dotnet

# Asegurar que la ruta a las herramientas de dotnet esté en el PATH
ENV PATH="$PATH:/usr/share/dotnet"

# Instalar la herramienta dotnet ef globalmente
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Copiar el archivo .csproj desde la etapa de construcción
COPY --from=build /App/VisualNetworkAPI.csproj ./

# Copiar artefactos publicados desde la etapa build
COPY --from=build /App/out .

# Exponer el puerto 80 dentro del contenedor
EXPOSE 80

# Comando de inicio
ENTRYPOINT ["dotnet", "VisualNetworkAPI.dll"]
