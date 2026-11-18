# Imagem do serviço de Quadras para o Render (o Render só roda .NET via Docker).

# Etapa 1: compila com o SDK completo.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Quadras.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app --no-restore

# Etapa 2: roda só com o runtime, que é bem menor.
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

# Pasta do banco, que pertence ao usuário sem privilégios da imagem.
# Num plano com disco persistente, é aqui que o disco seria montado.
RUN mkdir /app/data && chown $APP_UID /app/data
USER $APP_UID

ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/quadras.db"

# O Render informa a porta na variável PORT (padrão 10000).
ENTRYPOINT ["sh", "-c", "ASPNETCORE_HTTP_PORTS=${PORT:-10000} exec dotnet Quadras.dll"]
