# Staged work: MS 5.7 Training wiring into shared frontend

**Target repo:** [ERP-CORE-DEV/rh-optimerp-frontend](https://github.com/ERP-CORE-DEV/rh-optimerp-frontend)
**Target branch:** `feature/ms57-training-integration` → merged into `hatim`
**Related issues:**
- [rh-optimerp-frontend#2](https://github.com/ERP-CORE-DEV/rh-optimerp-frontend/issues/2) — full playbook
- [rh-optimerp-training-and-skill-development#7](https://github.com/ERP-CORE-DEV/rh-optimerp-training-and-skill-development/issues/7) — backend pointer

Reference pattern: [feature/ms53-hiring-integration](https://github.com/ERP-CORE-DEV/rh-optimerp-frontend/tree/feature/ms53-hiring-integration) (HOUSSINE's MS 5.3 integration)

## What this delivers

Replaces `TrainingModule` placeholder in shared App.tsx with full MS 5.7 implementation following the exact same pattern Houssine used for MS 5.3.

### Files created

| File | Lines | Purpose |
|------|------:|---------|
| `src/types/training.ts` | 226 | 17 interfaces + 14 enums (verbatim from MS 5.7 source) |
| `src/services/trainingApiService.ts` | 452 | 18 API objects using shared `trainingApi` client, routes verified against backend controllers |
| `src/services/utils/extractApiError.ts` | 4 | Copied from MS 5.3 (shared helper) |
| `src/pages/training/plans/TrainingPlansPage.tsx` | 215 | CRUD — Training plans (Title, Year, Status, Budget) |
| `src/pages/training/actions/TrainingActionsPage.tsx` | 216 | CRUD — Training actions (Type, Modality, Duration, Cost) |
| `src/pages/training/cpf/CpfPage.tsx` | 210 | CPF accounts + mobilization |
| `src/pages/training/competencies/CompetenciesPage.tsx` | 214 | Competency catalog |
| `src/pages/training/entretiens/EntretiensPage.tsx` | 199 | Entretien professionnel |
| `src/pages/training/certifications/CertificationsPage.tsx` | 322 | Dual-tab RNCP + VAE |
| `src/pages/training/elearning/ElearningPage.tsx` | 216 | E-learning catalog |
| `src/pages/training/evaluations/EvaluationsPage.tsx` | 173 | Kirkpatrick evaluations |
| `src/pages/training/providers/ProvidersPage.tsx` | 209 | Qualiopi-certified providers |
| `src/pages/training/opco/OpcoPage.tsx` | 182 | OPCO management |
| `src/pages/training/bilan/BilanPage.tsx` | 195 | Bilan de competences |
| `src/pages/training/alternance/AlternancePage.tsx` | 238 | Alternance contracts |
| `src/pages/training/compliance/CompliancePage.tsx` | 251 | Training obligations |
| `src/pages/training/gdpr/GdprAdminPage.tsx` | 207 | RGPD anonymize/export/retention |

### Files modified

| File | Change |
|------|--------|
| `src/App.tsx` | Replaced `TrainingModule` placeholder with 14 lazy imports + 14 flat routes. Added `/training` → `/training/plans` redirect. |
| `src/components/layout/MainLayout.tsx` | Expanded `FORMATION (MS 5.7)` sidebar group from 1 item to 14 items. Added icon imports: BankOutlined, SafetyCertificateOutlined, DesktopOutlined. |

**Total:** 19 files, 3,729 new lines, 64 lines replaced in App.tsx/MainLayout.tsx.

## Pattern alignment with MS 5.3

| Concern | MS 5.3 approach | MS 5.7 approach | Match |
|---------|-----------------|-----------------|-------|
| Routes | Flat in App.tsx (not nested Module) | Flat in App.tsx | YES |
| Service layer | Single `hiringApiService.ts` with multiple exports | Single `trainingApiService.ts` with 18 exports | YES |
| Types | Single `hiring.ts` | Single `training.ts` | YES |
| Page pattern | useState + useCallback loader + extractApiError | Same | YES |
| Error handling | `extractApiError` helper | Same helper (reused) | YES |
| API client | `hiringApi` from `apiClient.ts` | `trainingApi` from `apiClient.ts` | YES |
| French labels | No accents, simple ASCII | Same | YES |
| React.lazy | Every page default export | Every page default export | YES |

## Verification

- `npx tsc --noEmit` → 0 errors
- All 14 pages have `export default`
- Routes map 1:1 with sidebar items
- Backend routes verified against actual `*Controller.cs` files — no invented endpoints

## Contract references

- `contracts/ROUTES_CONTRACT.md`: `/training/*` owned by MS 5.7
- `contracts/API_CONTRACT.md`: `/api/training` → port 5300 (dev) / `training-skill-dev.svc.cluster.local` (prod)
- `contracts/ENTITY_CONTRACT.md`: EmployeeId (shared with 5.1, 5.8), TrainingId (owned by 5.7)

## Backend prerequisites

Requires backend changes from MS 5.7 session 11 (commit `a131950` on `rh-optimerp-training-and-skill-development/HOUDAIFA`):
- Port 5300 binding
- JWT symmetric key contract (shared across team)
- `POST /api/auth/dev-token` endpoint
- GlobalExceptionHandler returning ProblemDetails

## Known limitations

- Pages are MVP-functional (~150-250 lines each), not exhaustive — covers core CRUD for each entity
- No dedicated E2E tests yet (should be added after alignment with @mohammed-reda on E2 domain boundary)
- Dashboard KPIs (analytics) not yet as a separate page — can be added as `/training/analytics` if needed
- E2 modules (Competencies, Entretiens, Bilan) may need refactoring once MS 5.8 sync events are formalized
