# Session 11 — MS 5.7 backend integration for shared frontend

**Target repo:** [ERP-CORE-DEV/rh-optimerp-training-and-skill-development](https://github.com/ERP-CORE-DEV/rh-optimerp-training-and-skill-development)
**Target branch:** `HOUDAIFA`
**Related issues:**
- [rh-optimerp-frontend#2](https://github.com/ERP-CORE-DEV/rh-optimerp-frontend/issues/2) — full playbook
- [rh-optimerp-training-and-skill-development#7](https://github.com/ERP-CORE-DEV/rh-optimerp-training-and-skill-development/issues/7) — backend pointer

## What this delivers

Backend changes required by Issue #2 playbook Part 1 (backend requirements) to wire MS 5.7 into the shared `rh-optimerp-frontend` at `/training/*`.

### Files (6)

| File | Change | Purpose |
|------|--------|---------|
| `Properties/launchSettings.json` | NEW | Binds backend to `http://localhost:5300` per API_CONTRACT.md |
| `Configuration/JwtSettings.cs` | MODIFIED | Swapped Azure AD Authority config for symmetric key (Key, Issuer, Audience, ExpirationHours) |
| `appsettings.json` | MODIFIED | Shared JWT dev key per team convention: `DevOnlyKey-RH-OptimERP-2026-MinLength32Chars!`, issuer `RH-OptimERP`, audience `RH-OptimERP-Client` |
| `Program.cs` | MODIFIED | 4 changes: (1) JWT symmetric key validation, (2) CORS restricted to GET/POST/PUT/DELETE + Content-Type/Authorization, (3) security headers always on, (4) GlobalExceptionHandler registered |
| `Controllers/Auth/DevAuthController.cs` | NEW | `[AllowAnonymous] POST /api/auth/dev-token` — issues 8h JWT with NameIdentifier/Name/Email/Role claims. Returns 403 outside Development. |
| `Extensions/GlobalExceptionHandler.cs` | NEW | `IExceptionHandler` returning RFC 7807 `ProblemDetails` JSON |

## Verification

- Backend build: 0 errors, 0 warnings
- Functional tests: **389/389 PASS** (all unit + integration + sync)
- Performance tests: 1 flaky (`GetCompetencyById_SingleResource_ShouldRespondWithin100ms` — known timing flake, unrelated)
- End-to-end smoke test:
  - `POST /api/auth/dev-token` → returns 701-char JWT with proper claims
  - `GET /api/training-plans` without token → **401**
  - `GET /api/training-plans` with Bearer token → **200**
  - `GET /health/live` → **200**

## JWT contract (shared across team)

Per Issue #2 Part 1.2, Option A (unified JWT across all 5 backends):

| Field | Value |
|-------|-------|
| `Jwt:Key` | `DevOnlyKey-RH-OptimERP-2026-MinLength32Chars!` |
| `Jwt:Issuer` | `RH-OptimERP` |
| `Jwt:Audience` | `RH-OptimERP-Client` |
| Expiration | 8 hours |
| Algorithm | HMAC-SHA256 |
| Claims | NameIdentifier, Name, Email, Role, Jti, Iat |

A token issued by any microservice's `/api/auth/dev-token` endpoint is valid across all microservices that share this key. This enables the shared frontend to boot a single token and use it for `/api/sourcing/*`, `/api/hiring/*`, `/api/training/*`, etc.

## Next steps (per Issue #2 Part 2)

The frontend wiring (pages under `src/pages/training/`, services, routes in `App.tsx`, sidebar in `MainLayout.tsx`) will be done in the shared frontend repo via a separate `HOUDAIFA` branch, also staged here first.

## Notes

- CORS `OPTIONS` method added explicitly — browsers require it for preflight on requests with custom headers
- `ClockSkew` tightened from 1 min to 30 seconds (defense-in-depth per security review LOW-02)
- Security headers (`Content-Security-Policy`, `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`, `Referrer-Policy`, `X-XSS-Protection`) now applied in **all** environments (previous code only applied them in production, per security review HIGH-04)
- `ValidateIssuerSigningKey = true` explicitly set (per security review MED-05)
