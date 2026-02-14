# EAGLES Kickoff — HOUDAIFA

## Assigned Microservice: [5.7] Training & Skill Development

> **IMPORTANT**: Your microservice is **Training & Skill Development**, NOT Integration & Onboarding.

**Repo**: [rh-optimerp-training-and-skill-development](https://github.com/ERP-CORE-DEV/rh-optimerp-training-and-skill-development)
**Issue**: [#1](https://github.com/ERP-CORE-DEV/rh-optimerp-training-and-skill-development/issues/1)

---

## Phase 1 — Plan (uses 1 planner agent, isolated context)
```
/plan training-and-skill-development microservice — Controller-Service-Repository, .NET 8, CosmosDB SDK 3.54, React 18 + Ant Design, French HR compliance (CPF, OPCO, RNCP/RS codes). Reference: rh-optimerp-sourcing-candidate-attraction repo.
```

## Phase 2 — Scaffold (after plan approval)
```
/scaffold training-and-skill-development
```

## Phase 3 — TDD first feature
```
/tdd training-program-management
```

## Phase 4 — Pre-PR validation
```
/code-review
/security-scan
/gdpr-check
```

---

## Key Rules
- **Pattern**: Controller-Service-Repository (NOT CQRS/MediatR)
- **Database**: CosmosDB SDK 3.54 direct (NOT EF Core)
- **GDPR**: AnonymizeXxx() on personal data models + IsAnonymized flag
- **French HR**: CPF eligibility, OPCO financing, RNCP/RS certification codes
- **Tests**: xUnit + FluentAssertions + Moq, minimum 80% coverage
- **Commits**: Conventional commits (feat/fix/refactor/test/docs)
- **Integration dependency**: Watches [5.6] Retention & Loyalty and [5.8] Talent Management

## Reference Repo
Use [rh-optimerp-sourcing-candidate-attraction](https://github.com/ERP-CORE-DEV/rh-optimerp-sourcing-candidate-attraction) as the architecture reference.
