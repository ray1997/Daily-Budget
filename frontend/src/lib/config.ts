export const apiBase = import.meta.env.VITE_API_BASE ?? 'http://localhost:8080';
export const oidcAuthority = import.meta.env.VITE_OIDC_AUTHORITY ?? '';
export const oidcClientId = import.meta.env.VITE_OIDC_CLIENT_ID ?? '';
export const oidcScopes = import.meta.env.VITE_OIDC_SCOPES ?? 'openid profile email';
export const oidcAuthorizationEndpoint = import.meta.env.VITE_OIDC_AUTHORIZATION_ENDPOINT ?? '';
