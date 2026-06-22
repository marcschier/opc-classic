# syntax=docker/dockerfile:1

ARG BUILDPLATFORM=linux/amd64
ARG TARGETPLATFORM=linux/amd64
ARG TARGETARCH=amd64

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /src

COPY . .

RUN set -eux; \
    case "$TARGETARCH" in \
        amd64) rid="linux-x64" ;; \
        arm64) rid="linux-arm64" ;; \
        *) echo "Unsupported TARGETARCH=$TARGETARCH. Expected amd64 or arm64." >&2; exit 1 ;; \
    esac; \
    dotnet restore "samples/Opc.Classic.Samples.AeServer/Opc.Classic.Samples.AeServer.csproj" -r "$rid"; \
    dotnet publish "samples/Opc.Classic.Samples.AeServer/Opc.Classic.Samples.AeServer.csproj" \
        -c Release \
        -r "$rid" \
        --self-contained false \
        --no-restore \
        -o /app/publish \
        /p:UseAppHost=false

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled AS runtime
WORKDIR /app

LABEL maintainer="marcschier" \
      org.opencontainers.image.title="Opc.Classic AE Server sample" \
      org.opencontainers.image.description="Containerized OPC Classic AE server sample." \
      org.opencontainers.image.source="https://github.com/marcschier/opc-classic" \
      org.opencontainers.image.licenses="MIT" \
      org.opencontainers.image.authors="marcschier"

EXPOSE 51301
COPY --from=build --chown=1654:1654 /app/publish ./

USER $APP_UID
ENTRYPOINT ["dotnet", "Opc.Classic.Samples.AeServer.dll"]
