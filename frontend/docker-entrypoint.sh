#!/bin/sh
set -eu

cat > /usr/share/nginx/html/config.json <<EOF_CONFIG
{
  "apiBase": "${API_BASE:-http://localhost:5000}",
  "oidcAuthority": "${OIDC_AUTHORITY:-}",
  "oidcClientId": "${OIDC_CLIENT_ID:-}",
  "oidcAuthorizationEndpoint": "${OIDC_AUTHORIZATION_ENDPOINT:-}",
  "oidcScopes": "${OIDC_SCOPES:-openid profile email}"
}
EOF_CONFIG

exec nginx -g 'daemon off;'
