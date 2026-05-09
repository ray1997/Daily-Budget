<script lang="ts">
  import { onMount } from 'svelte';
  import { completeOidcLogin } from '$lib/auth';

  let status = 'Finishing login…';

  onMount(async () => {
    try {
      const code = new URL(location.href).searchParams.get('code');
      if (!code) throw new Error('OIDC callback did not include a code.');
      await completeOidcLogin(code);
      location.href = '/';
    } catch (error) {
      status = error instanceof Error ? error.message : 'Login failed.';
    }
  });
</script>

<main class="flex min-h-screen items-center justify-center bg-calm-50 p-6 text-center">
  <div class="rounded-3xl bg-white p-8 shadow-sm">
    <p class="text-lg font-semibold text-calm-700">{status}</p>
  </div>
</main>
