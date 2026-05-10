export type RuntimeConfig = {
  apiBase: string;
  oidcAuthority: string;
  oidcClientId: string;
  oidcAuthorizationEndpoint: string;
  oidcScopes: string;
};

const defaultConfig: RuntimeConfig = {
  apiBase: import.meta.env.VITE_API_BASE ?? 'http://localhost:5000',
  oidcAuthority: import.meta.env.VITE_OIDC_AUTHORITY ?? '',
  oidcClientId: import.meta.env.VITE_OIDC_CLIENT_ID ?? '',
  oidcAuthorizationEndpoint: import.meta.env.VITE_OIDC_AUTHORIZATION_ENDPOINT ?? '',
  oidcScopes: import.meta.env.VITE_OIDC_SCOPES ?? 'openid profile email'
};

let runtimeConfigPromise: Promise<RuntimeConfig> | undefined;

export function getRuntimeConfig(): Promise<RuntimeConfig> {
  runtimeConfigPromise ??= loadRuntimeConfig();
  return runtimeConfigPromise;
}

async function loadRuntimeConfig(): Promise<RuntimeConfig> {
  if (typeof window === 'undefined') return defaultConfig;

  try {
    const response = await fetch('/config.json', { cache: 'no-store' });
    if (!response.ok) return defaultConfig;

    const config = await response.json() as Partial<RuntimeConfig>;
    return {
      apiBase: config.apiBase ?? defaultConfig.apiBase,
      oidcAuthority: config.oidcAuthority ?? defaultConfig.oidcAuthority,
      oidcClientId: config.oidcClientId ?? defaultConfig.oidcClientId,
      oidcAuthorizationEndpoint: config.oidcAuthorizationEndpoint ?? defaultConfig.oidcAuthorizationEndpoint,
      oidcScopes: config.oidcScopes ?? defaultConfig.oidcScopes
    };
  } catch {
    return defaultConfig;
  }
}
