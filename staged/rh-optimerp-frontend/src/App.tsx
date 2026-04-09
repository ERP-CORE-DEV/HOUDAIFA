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

const TrainingModule = React.lazy(() => import('./pages/training/TrainingRoutes'));

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
                      <Route path="/training/*" element={<TrainingModule />} />

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
