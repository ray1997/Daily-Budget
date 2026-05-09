import { apiBase, oidcAuthority, oidcAuthorizationEndpoint, oidcClientId, oidcScopes } from './config';

const tokenKey = 'daily-budget-token';
const verifierKey = 'daily-budget-pkce-verifier';

export function getToken(): string | null {
  return localStorage.getItem(tokenKey);
}

export function setToken(token: string) {
  localStorage.setItem(tokenKey, token);
}

export function logout() {
  localStorage.removeItem(tokenKey);
}

export async function apiFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = getToken();
  const headers = new Headers(options.headers);
  headers.set('Content-Type', 'application/json');
  if (token) headers.set('Authorization', `Bearer ${token}`);
  const response = await fetch(`${apiBase}${path}`, { ...options, headers });
  if (!response.ok) {
    throw new Error(await response.text() || response.statusText);
  }
  if (response.status === 204) return undefined as T;
  return response.json() as Promise<T>;
}

export async function beginOidcLogin() {
  if (!oidcAuthority || !oidcClientId) {
    throw new Error('OIDC is not configured. Set VITE_OIDC_AUTHORITY and VITE_OIDC_CLIENT_ID.');
  }
  const verifier = randomString(64);
  sessionStorage.setItem(verifierKey, verifier);
  const challenge = await sha256Base64Url(verifier);
  const authorizationEndpoint = oidcAuthorizationEndpoint || await discoverAuthorizationEndpoint();
  const url = new URL(authorizationEndpoint);
  url.searchParams.set('client_id', oidcClientId);
  url.searchParams.set('redirect_uri', `${location.origin}/auth/callback`);
  url.searchParams.set('response_type', 'code');
  url.searchParams.set('scope', oidcScopes);
  url.searchParams.set('code_challenge', challenge);
  url.searchParams.set('code_challenge_method', 'S256');
  location.href = url.toString();
}

export async function completeOidcLogin(code: string) {
  const codeVerifier = sessionStorage.getItem(verifierKey);
  if (!codeVerifier) throw new Error('Missing PKCE verifier. Please start login again.');
  const result = await apiFetch<{ token: string }>('/api/auth/oidc', {
    method: 'POST',
    body: JSON.stringify({ code, codeVerifier, redirectUri: `${location.origin}/auth/callback` })
  });
  setToken(result.token);
  sessionStorage.removeItem(verifierKey);
}

async function discoverAuthorizationEndpoint(): Promise<string> {
  const response = await fetch(`${oidcAuthority.replace(/\/$/, '')}/.well-known/openid-configuration`);
  if (!response.ok) throw new Error('Could not load OIDC discovery document.');
  const discovery = await response.json() as { authorization_endpoint?: string };
  if (!discovery.authorization_endpoint) throw new Error('OIDC discovery document is missing authorization_endpoint.');
  return discovery.authorization_endpoint;
}

function randomString(length: number): string {
  const bytes = crypto.getRandomValues(new Uint8Array(length));
  return base64Url(bytes);
}

async function sha256Base64Url(value: string): Promise<string> {
  const data = new TextEncoder().encode(value);
  const hash = await crypto.subtle.digest('SHA-256', data);
  return base64Url(new Uint8Array(hash));
}

function base64Url(bytes: Uint8Array): string {
  return btoa(String.fromCharCode(...bytes)).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}
