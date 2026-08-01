#!/bin/sh
set -eu

# Replace runtime placeholder in the built index.html.
# Constraint: frontend must read VITE_API_URL dynamically at container runtime.

API_URL="${VITE_API_URL:-}"

# Always replace the token (VITE_API_URL may be empty in production).
sed -i "s|__VITE_API_URL__|$API_URL|g" /usr/share/nginx/html/index.html

exec nginx -g "daemon off;"
