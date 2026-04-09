# Staged work for `rh-optimerp-frontend`

**Target repo:** [ERP-CORE-DEV/rh-optimerp-frontend](https://github.com/ERP-CORE-DEV/rh-optimerp-frontend)
**Target branch:** `HOUDAIFA` → merged into `hatim`
**Related PR:** [rh-optimerp-frontend#1](https://github.com/ERP-CORE-DEV/rh-optimerp-frontend/pull/1)

## What this contains

MS 5.7 (Training & Skill Development) frontend integration — wires the training module into the shared RH-OptimERP frontend, replacing the `TrainingModule` placeholder with the full 16-page implementation.

### Files

| Layer | Files | Description |
|-------|-------|-------------|
| `src/types/training.ts` | 1 | 17 interfaces + 14 enum types |
| `src/services/training/` | 16 + barrel | API services wired to `trainingApi` proxy |
| `src/components/training/modals/` | 13 + barrel | CRUD modals (all domain entities) |
| `src/pages/training/` | 15 + TrainingRoutes | Lazy-loaded pages |
| `src/App.tsx` | 1 | Replaces TrainingModule placeholder with real routes |

**Total:** 47 files, ~5,759 insertions.

## Contract alignment

- **ROUTES_CONTRACT.md** — `/training/*` prefix respected
- **API_CONTRACT.md** — `/api/training` → backend port 5300
- **ENTITY_CONTRACT.md** — `EmployeeId`, `TrainingId` shared IDs

## Workflow

This directory is the **staging mirror** for work destined for `rh-optimerp-frontend`. Per the principal-repo workflow convention, all code changes must be committed here first, then promoted to the target microservice repo.
