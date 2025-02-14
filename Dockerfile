﻿FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG TARGETARCH
WORKDIR /source

# COPY Directory.Packages.props C2Stats/*.csproj .
COPY C2Stats/*.csproj .
RUN dotnet restore -a $TARGETARCH

COPY C2Stats/. .
RUN dotnet publish --no-restore -a $TARGETARCH -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine
EXPOSE 8080
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT [ "./c2-stats" ]