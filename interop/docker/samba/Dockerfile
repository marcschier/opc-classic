# SPDX-License-Identifier: MIT
# Copyright (c) 2026 Opc.Classic .NET Contributors

FROM dperson/samba:latest

USER root

RUN set -eux; \
    addgroup -S opcclassic; \
    adduser -S -D -H -G opcclassic opcuser; \
    mkdir -p /srv/samba/share /run/samba /var/cache/samba /var/lib/samba/private; \
    chown -R opcuser:opcclassic /srv/samba/share; \
    printf 'opcpass\nopcpass\n' | smbpasswd -a -s opcuser; \
    smbpasswd -e opcuser

EXPOSE 445

HEALTHCHECK --interval=5s --timeout=5s --start-period=10s --retries=20 \
    CMD smbclient -L //127.0.0.1 -U 'opcuser%opcpass' -m SMB3 >/dev/null 2>&1 || exit 1

ENTRYPOINT []
CMD ["smbd", "--foreground", "--no-process-group", "--log-stdout"]
