#!/bin/sh
set -eu

REALM="${KRB5_REALM:-OPCCLASSIC.LOCAL}"
DOMAIN=$(printf '%s' "$REALM" | tr '[:upper:]' '[:lower:]')
MASTER_PASSWORD="${KRB5_MASTER_PASSWORD:-testcontainers-master}"
TESTUSER_PASSWORD="${KRB5_TESTUSER_PASSWORD:-correct horse battery staple}"

cat > /etc/krb5.conf <<EOF
[libdefaults]
    default_realm = $REALM
    dns_lookup_kdc = false
    dns_lookup_realm = false
    dns_canonicalize_hostname = false
    udp_preference_limit = 1
    default_tgs_enctypes = aes256-cts-hmac-sha1-96 aes128-cts-hmac-sha1-96
    default_tkt_enctypes = aes256-cts-hmac-sha1-96 aes128-cts-hmac-sha1-96
    permitted_enctypes = aes256-cts-hmac-sha1-96 aes128-cts-hmac-sha1-96

[realms]
    $REALM = {
        kdc = localhost
        admin_server = localhost
    }

[domain_realm]
    .$DOMAIN = $REALM
    $DOMAIN = $REALM
EOF

cat > /etc/krb5kdc/kdc.conf <<EOF
[kdcdefaults]
    kdc_ports = 88
    kdc_tcp_ports = 88

[realms]
    $REALM = {
        database_name = /var/lib/krb5kdc/principal
        admin_keytab = FILE:/etc/krb5kdc/kadm5.keytab
        acl_file = /etc/krb5kdc/kadm5.acl
        key_stash_file = /etc/krb5kdc/stash
        max_life = 10h 0m 0s
        max_renewable_life = 7d 0h 0m 0s
        default_principal_flags = +preauth
        supported_enctypes = aes256-cts-hmac-sha1-96:normal aes128-cts-hmac-sha1-96:normal
    }
EOF

printf '*/admin@%s *\n' "$REALM" > /etc/krb5kdc/kadm5.acl
rm -f /var/lib/krb5kdc/principal /var/lib/krb5kdc/principal.kadm5 /var/lib/krb5kdc/principal.kadm5.lock /var/lib/krb5kdc/principal.ok
kdb5_util create -s -r "$REALM" -P "$MASTER_PASSWORD"

kadmin.local -q "addprinc -pw $TESTUSER_PASSWORD testuser@$REALM"
kadmin.local -q "addprinc -randkey host/opcserver.opcclassic.local@$REALM"
kadmin.local -q "addprinc -randkey host/opcclient.opcclassic.local@$REALM"
kadmin.local -q "addprinc -randkey -maxlife '1 minute' host/short.opcclassic.local@$REALM"

kadmin.local -q "ktadd -norandkey -k /keytabs/testuser.keytab testuser@$REALM"
kadmin.local -q "ktadd -k /keytabs/server.keytab host/opcserver.opcclassic.local@$REALM"
kadmin.local -q "ktadd -k /keytabs/client.keytab host/opcclient.opcclassic.local@$REALM"
kadmin.local -q "ktadd -k /keytabs/short.keytab host/short.opcclassic.local@$REALM"
chmod 0644 /keytabs/*.keytab

exec krb5kdc -n
