# MVP implementation notes

## Included features

- Generic OIDC authorization-code-with-PKCE login from the SvelteKit frontend.
- Backend OIDC code redemption and local long-lived JWT app sessions.
- Active budget cycles with a starting balance and salary date calculation settings.
- Fully editable transaction CRUD for amount, note, date, and type.
- Dynamic dashboard calculations for remaining money, days until salary, current daily budget, today progress, projection, and allowance drift history.
- Mobile-first dashboard with a fixed bottom transaction input.
- SQLite persistence through EF Core and Docker volume storage.

## Salary cycle modes

The backend model supports:

- fixed day of month;
- biweekly anchor date;
- last selected weekday of the month.

Holiday overrides are modeled for future UI expansion and are considered by the salary-date calculator when records exist.

## Non-goals kept out of the MVP

No banking integrations, categories, ads, social features, gamification, crypto tracking, or AI finance advice are included.
