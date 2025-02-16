FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG TARGETARCH
WORKDIR /source

# COPY Directory.Packages.props C2Stats/*.csproj .
COPY C2Stats/*.csproj .
RUN dotnet restore -a $TARGETARCH

COPY C2Stats/. .
RUN dotnet publish --no-restore -a $TARGETARCH -o /app

FROM node:18-alpine AS frontend-build
WORKDIR /frontend
COPY C2Stats/frontend/. .
RUN npm install && npm run build

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine
EXPOSE 8080
WORKDIR /app
COPY --from=build /app .
COPY --from=frontend-build /wwwroot /app/wwwroot
USER $APP_UID
# ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT [ "./c2-stats" ]