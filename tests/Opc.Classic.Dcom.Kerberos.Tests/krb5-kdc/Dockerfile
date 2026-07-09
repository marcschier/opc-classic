FROM debian:bookworm-slim

ENV DEBIAN_FRONTEND=noninteractive

RUN apt-get update \
    && apt-get install -y --no-install-recommends krb5-kdc krb5-admin-server krb5-user netbase \
    && rm -rf /var/lib/apt/lists/*

COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh && mkdir -p /keytabs

EXPOSE 88/tcp

ENTRYPOINT ["/entrypoint.sh"]
