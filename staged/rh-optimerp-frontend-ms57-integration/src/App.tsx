import React, { Suspense } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ConfigProvider, Spin } from 'antd';
import frFR from 'antd/locale/fr_FR';
import MainLayout from './components/layout/MainLayout';
import DashboardWorkspace from './components/layout/DashboardWorkspace';
import ProtectedRoute from './components/auth/ProtectedRoute';
import ErrorBoundary from './components/common/ErrorBoundary';
import ModulePlaceholder from './pages/modules/ModulePlaceholder';

// Lazy-loaded module routes
const SourcingModule = React.lazy(() =>
  Promise.resolve({
    default: () => (
      <ModulePlaceholder
        moduleName="Sourcing & Recrutement"
        msNumber="5.1"
        owner="HATIM"
        status="ready"
      />
    ),
  })
);

const TalentModule = React.lazy(() =>
  Promise.resolve({
    default: () => (
      <ModulePlaceholder
        moduleName="Gestion des Talents"
        msNumber="5.8"
        owner="MOHAMMED-REDA"
        status="ready"
      />
    ),
  })
);

const EvaluationModule = React.lazy(() =>
  Promise.resolve({
    default: () => (
      <ModulePlaceholder
        moduleName="Evaluation des Candidats"
        msNumber="5.2"
        owner="HASSAN"
        status="coming-soon"
      />
    ),
  })
);

const HiringModule = React.lazy(() =>
  Promise.resolve({
    default: () => (
      <ModulePlaceholder
        moduleName="Processus de Recrutement"
        msNumber="5.3"
        owner="HOUSSINE"
        status="coming-soon"
      />
    ),
  })
);

// MS 5.7 — Training & Skill Development (HOUDAIFA) — flat routes
const TrainingPlansPage = React.lazy(() => import('./pages/training/plans/TrainingPlansPage'));
const TrainingActionsPage = React.lazy(() => import('./pages/training/actions/TrainingActionsPage'));
const CpfPage = React.lazy(() => import('./pages/training/cpf/CpfPage'));
const CompetenciesPage = React.lazy(() => import('./pages/training/competencies/CompetenciesPage'));
const EntretiensPage = React.lazy(() => import('./pages/training/entretiens/EntretiensPage'));
const CertificationsPage = React.lazy(() => import('./pages/training/certifications/CertificationsPage'));
const ElearningPage = React.lazy(() => import('./pages/training/elearning/ElearningPage'));
const EvaluationsPage = React.lazy(() => import('./pages/training/evaluations/EvaluationsPage'));
const ProvidersPage = React.lazy(() => import('./pages/training/providers/ProvidersPage'));
const OpcoPage = React.lazy(() => import('./pages/training/opco/OpcoPage'));
const BilanPage = React.lazy(() => import('./pages/training/bilan/BilanPage'));
const AlternancePage = React.lazy(() => import('./pages/training/alternance/AlternancePage'));
const CompliancePage = React.lazy(() => import('./pages/training/compliance/CompliancePage'));
const GdprAdminPage = React.lazy(() => import('./pages/training/gdpr/GdprAdminPage'));

const ANTD_THEME = {
  token: {
    colorPrimary: '#7c3aed',
    colorLink: '#7c3aed',
    colorLinkHover: '#6d28d9',
    borderRadius: 8,
    fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', system-ui, sans-serif",
  },
};

const LoadingFallback: React.FC = () => (
  <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 300 }}>
    <Spin size="large" />
  </div>
);

const App: React.FC = () => (
  <ConfigProvider theme={ANTD_THEME} locale={frFR}>
    <BrowserRouter>
      <Routes>
        <Route
          path="/*"
          element={
            <ProtectedRoute>
              <MainLayout>
                <ErrorBoundary>
                  <Suspense fallback={<LoadingFallback />}>
                    <Routes>
                      <Route path="/" element={<DashboardWorkspace />} />

                      <Route path="/sourcing/*" element={<SourcingModule />} />
                      <Route path="/talent/*" element={<TalentModule />} />
                      <Route path="/evaluation/*" element={<EvaluationModule />} />
                      <Route path="/hiring/*" element={<HiringModule />} />
                      <Route path="/training" element={<Navigate to="/training/plans" replace />} />
                      <Route path="/training/plans" element={<TrainingPlansPage />} />
                      <Route path="/training/actions" element={<TrainingActionsPage />} />
                      <Route path="/training/cpf" element={<CpfPage />} />
                      <Route path="/training/competencies" element={<CompetenciesPage />} />
                      <Route path="/training/entretiens" element={<EntretiensPage />} />
                      <Route path="/training/certifications" element={<CertificationsPage />} />
                      <Route path="/training/elearning" element={<ElearningPage />} />
                      <Route path="/training/evaluations" element={<EvaluationsPage />} />
                      <Route path="/training/providers" element={<ProvidersPage />} />
                      <Route path="/training/opco" element={<OpcoPage />} />
                      <Route path="/training/bilan" element={<BilanPage />} />
                      <Route path="/training/alternance" element={<AlternancePage />} />
                      <Route path="/training/compliance" element={<CompliancePage />} />
                      <Route path="/training/gdpr" element={<GdprAdminPage />} />

                      <Route path="*" element={<Navigate to="/" replace />} />
                    </Routes>
                  </Suspense>
                </ErrorBoundary>
              </MainLayout>
            </ProtectedRoute>
          }
        />
      </Routes>
    </BrowserRouter>
  </ConfigProvider>
);

export default App;
