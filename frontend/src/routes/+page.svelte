<script lang="ts">
  import { onMount } from 'svelte';
  import { apiFetch, beginOidcLogin, getToken, logout } from '$lib/auth';
  import type { DashboardDto, TransactionDto } from '$lib/types';

  let dashboard: DashboardDto | null = null;
  let loading = true;
  let error = '';
  let amount = '';
  let note = '';
  let type = 'Expense';
  let editId: string | null = null;
  let editAmount = '';
  let editNote = '';
  let editDate = '';
  let editType = 'Expense';
  let cycleBalance = '';
  let cycleSalaryDate = '';
  let cycleMode = 'FixedDate';
  let cycleFixedDay = '1';
  let cycleCurrency = '฿';

  const today = new Date().toISOString().slice(0, 10);

  onMount(load);

  async function load() {
    loading = true;
    error = '';
    if (!getToken()) {
      loading = false;
      return;
    }
    try {
      dashboard = await apiFetch<DashboardDto>('/api/dashboard');
    } catch (err) {
      error = err instanceof Error ? err.message : 'Could not load dashboard.';
    } finally {
      loading = false;
    }
  }

  async function addTransaction() {
    if (!amount) return;
    await apiFetch('/api/transactions', {
      method: 'POST',
      body: JSON.stringify({ amount: Number(amount), note, transactionDate: today, type })
    });
    amount = '';
    note = '';
    await load();
  }

  function startEdit(transaction: TransactionDto) {
    editId = transaction.id;
    editAmount = String(transaction.amount);
    editNote = transaction.note;
    editDate = transaction.transactionDate;
    editType = String(transaction.type);
  }

  async function saveEdit() {
    if (!editId) return;
    await apiFetch(`/api/transactions/${editId}`, {
      method: 'PUT',
      body: JSON.stringify({ amount: Number(editAmount), note: editNote, transactionDate: editDate, type: editType })
    });
    editId = null;
    await load();
  }

  async function deleteTransaction(id: string) {
    await apiFetch(`/api/transactions/${id}`, { method: 'DELETE' });
    await load();
  }

  async function startCycle() {
    await apiFetch('/api/cycles', {
      method: 'POST',
      body: JSON.stringify({
        startingBalance: Number(cycleBalance),
        startDate: today,
        nextSalaryDate: cycleSalaryDate || null,
        salaryCycleMode: cycleMode,
        fixedSalaryDay: Number(cycleFixedDay),
        biweeklyAnchorDate: cycleSalaryDate || null,
        lastWeekday: 5,
        currencySymbol: cycleCurrency
      })
    });
    await load();
  }

  function money(value: number | undefined) {
    return `${Number(value ?? 0).toLocaleString(undefined, { maximumFractionDigits: 2 })}${dashboard?.cycle?.currencySymbol ?? cycleCurrency}`;
  }

  $: progressColor = (dashboard?.todayProgressPercent ?? 0) >= 100 ? 'bg-red-400' : (dashboard?.todayProgressPercent ?? 0) >= 75 ? 'bg-amber-400' : 'bg-emerald-400';
  $: graphPoints = dashboard?.allowanceHistory?.length
    ? dashboard.allowanceHistory.map((point, index, all) => {
        const values = all.map((p) => p.dailyAllowance);
        const min = Math.min(...values);
        const max = Math.max(...values);
        const x = all.length === 1 ? 0 : (index / (all.length - 1)) * 300;
        const y = 120 - ((point.dailyAllowance - min) / Math.max(1, max - min)) * 100;
        return `${x},${y}`;
      }).join(' ')
    : '';
</script>

<main class="min-h-screen bg-calm-50 pb-36 text-slate-800">
  <div class="mx-auto flex max-w-md flex-col gap-4 p-4">
    <header class="flex items-center justify-between py-2">
      <div>
        <p class="text-sm text-slate-500">Simple Survival</p>
        <h1 class="text-2xl font-bold text-calm-700">Daily Budget</h1>
      </div>
      {#if getToken()}
        <button class="rounded-full bg-white px-4 py-2 text-sm shadow-sm" on:click={() => { logout(); dashboard = null; }}>Log out</button>
      {/if}
    </header>

    {#if !getToken()}
      <section class="rounded-3xl bg-white p-6 shadow-sm">
        <h2 class="text-xl font-semibold">Know what is safe to spend today.</h2>
        <p class="mt-2 text-slate-500">A calm, low-friction tracker for getting to the next salary.</p>
        <button class="mt-6 w-full rounded-2xl bg-calm-700 px-4 py-3 font-semibold text-white" on:click={beginOidcLogin}>Login with OIDC</button>
      </section>
    {:else if loading}
      <p class="rounded-3xl bg-white p-6 shadow-sm">Loading your budget…</p>
    {:else if error}
      <p class="rounded-3xl bg-red-50 p-6 text-red-700">{error}</p>
    {:else if !dashboard?.cycle}
      <section class="rounded-3xl bg-white p-6 shadow-sm">
        <h2 class="text-xl font-semibold">Start fresh</h2>
        <p class="mt-1 text-sm text-slate-500">Set the money you actually have now. No guilt, just a new baseline.</p>
        <div class="mt-4 grid gap-3">
          <input class="rounded-2xl border p-3" bind:value={cycleBalance} inputmode="decimal" placeholder="Current money" />
          <input class="rounded-2xl border p-3" bind:value={cycleSalaryDate} type="date" aria-label="Next salary date" />
          <select class="rounded-2xl border p-3" bind:value={cycleMode}>
            <option>FixedDate</option>
            <option>Biweekly</option>
            <option>LastWeekdayOfMonth</option>
          </select>
          <input class="rounded-2xl border p-3" bind:value={cycleFixedDay} inputmode="numeric" placeholder="Salary day, e.g. 1" />
          <input class="rounded-2xl border p-3" bind:value={cycleCurrency} placeholder="Currency symbol" />
          <button class="rounded-2xl bg-calm-700 p-3 font-semibold text-white" on:click={startCycle}>Begin cycle</button>
        </div>
      </section>
    {:else}
      <section class="rounded-3xl bg-white p-5 shadow-sm">
        <p class="text-sm text-slate-500">Remaining Money</p>
        <p class="text-4xl font-bold text-calm-700">{money(dashboard.remainingMoney)}</p>
        <div class="mt-4 grid grid-cols-2 gap-3 text-sm">
          <div class="rounded-2xl bg-calm-50 p-3"><p class="text-slate-500">Salary in</p><p class="text-xl font-semibold">{dashboard.daysUntilSalary} days</p></div>
          <div class="rounded-2xl bg-calm-50 p-3"><p class="text-slate-500">Daily budget</p><p class="text-xl font-semibold">{money(dashboard.dailyBudget)}</p></div>
          <div class="rounded-2xl bg-calm-50 p-3"><p class="text-slate-500">Salary date</p><p class="font-semibold">{dashboard.cycle.nextSalaryDate}</p></div>
          <div class="rounded-2xl bg-calm-50 p-3"><p class="text-slate-500">Projection</p><p class="font-semibold">{money(dashboard.projectedRemaining)}</p></div>
        </div>
      </section>

      <section class="rounded-3xl bg-white p-5 shadow-sm">
        <div class="mb-2 flex justify-between text-sm"><span>Today</span><span>{money(dashboard.spentToday)} / {money(dashboard.dailyBudget)} ({dashboard.todayProgressPercent}%)</span></div>
        <div class="h-4 overflow-hidden rounded-full bg-slate-100"><div class={`h-full ${progressColor}`} style={`width: ${Math.min(100, dashboard.todayProgressPercent)}%`}></div></div>
      </section>

      <section class="rounded-3xl bg-white p-5 shadow-sm">
        <h2 class="font-semibold">Daily allowance drift</h2>
        <svg viewBox="0 0 300 130" class="mt-3 h-36 w-full overflow-visible">
          <polyline fill="none" stroke="#4a9b92" stroke-width="4" stroke-linecap="round" stroke-linejoin="round" points={graphPoints} />
        </svg>
      </section>

      <section class="rounded-3xl bg-white p-5 shadow-sm">
        <div class="mb-3 flex items-center justify-between">
          <h2 class="font-semibold">Recent transactions</h2>
        </div>
        <div class="grid gap-3">
          {#each dashboard.transactions as transaction}
            <article class="rounded-2xl border border-slate-100 p-3">
              {#if editId === transaction.id}
                <div class="grid gap-2">
                  <select class="rounded-xl border p-2" bind:value={editType}><option>Expense</option><option>Income</option></select>
                  <input class="rounded-xl border p-2" bind:value={editAmount} inputmode="decimal" />
                  <input class="rounded-xl border p-2" bind:value={editNote} placeholder="Note" />
                  <input class="rounded-xl border p-2" bind:value={editDate} type="date" />
                  <div class="flex gap-2"><button class="flex-1 rounded-xl bg-calm-700 p-2 text-white" on:click={saveEdit}>Save</button><button class="flex-1 rounded-xl bg-slate-100 p-2" on:click={() => editId = null}>Cancel</button></div>
                </div>
              {:else}
                <div class="flex items-start justify-between gap-3">
                  <div><p class="font-semibold">{transaction.type === 'Income' ? '+' : '-'}{money(transaction.amount)}</p><p class="text-sm text-slate-500">{transaction.note || 'No note'} · {transaction.transactionDate}</p></div>
                  <div class="flex gap-2"><button class="text-sm text-calm-700" on:click={() => startEdit(transaction)}>Edit</button><button class="text-sm text-red-500" on:click={() => deleteTransaction(transaction.id)}>Delete</button></div>
                </div>
              {/if}
            </article>
          {/each}
        </div>
      </section>

      <section class="rounded-3xl bg-white p-5 shadow-sm">
        <h2 class="font-semibold">Fresh baseline</h2>
        <p class="mt-1 text-sm text-slate-500">If you missed days or want a reset, set your actual money now and begin a clean cycle.</p>
        <div class="mt-3 grid gap-2">
          <input class="rounded-2xl border p-3" bind:value={cycleBalance} placeholder={String(dashboard.remainingMoney)} inputmode="decimal" />
          <input class="rounded-2xl border p-3" bind:value={cycleSalaryDate} type="date" aria-label="New salary date" />
          <button class="rounded-2xl bg-slate-100 p-3 font-semibold text-calm-700" on:click={startCycle}>Start fresh cycle</button>
        </div>
      </section>
    {/if}
  </div>

  {#if getToken() && dashboard?.cycle}
    <form class="fixed inset-x-0 bottom-0 mx-auto max-w-md rounded-t-3xl bg-white p-4 shadow-[0_-10px_35px_rgba(15,23,42,0.08)]" on:submit|preventDefault={addTransaction}>
      <div class="flex gap-2">
        <select class="w-24 rounded-2xl border p-3" bind:value={type}><option>Expense</option><option>Income</option></select>
        <input class="min-w-0 flex-1 rounded-2xl border p-3 text-lg" bind:value={amount} inputmode="decimal" placeholder="Amount" />
      </div>
      <input class="mt-2 w-full rounded-2xl border p-3" bind:value={note} placeholder="Note (optional)" />
      <button class="mt-2 w-full rounded-2xl bg-calm-700 p-3 font-semibold text-white">Save softly</button>
    </form>
  {/if}
</main>
