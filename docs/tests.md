# 🧪 Testing Strategy

## Test Types

| Layer         | Tool        | Goal                                    |
|---------------|-------------|-----------------------------------------|
| Unit          | Jest        | Utilities, parsing, signal logic        |
| Integration   | Jest        | Components working together             |
| E2E           | Playwright  | Browser-based validation, logs, flow    |

---

## Playwright Setup

- Tests run in Chromium
- Run: `npm run test:e2e`
- Access console logs:

```ts
page.on('console', msg => {
  console.log(`[Browser] ${msg.text()}`);
});
```

---

## Coverage Goals

- Minimum 90% test coverage across entire codebase
- Run: `npm run coverage`
- Enforced via `coverageThreshold` in `jest.config.ts`

```ts
coverageThreshold: {
  global: {
    branches: 90,
    functions: 90,
    lines: 90,
    statements: 90
  }
}
```
