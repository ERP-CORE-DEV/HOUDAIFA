import React, { Suspense, lazy } from 'react';
import { BrowserRouter, Routes, Route, Navigate, Outlet } from 'react-router-dom';
import { ConfigProvider } from 'antd';
import frFR from 'antd/locale/fr_FR';
import { AuthProvider, AppProvider, useAuth } from './contexts';
import AppLayout from './components/layout/AppLayout';
import ErrorBoundary from './components/common/ErrorBoundary';
import PageLoading from './components/common/PageLoading';

const ProtectedRoute: React.FC = () => {
  const { isAuthenticated, isLoading } = useAuth();
  if (isLoading) return <PageLoading />;
  return isAuthenticated ? <Outlet /> : <Navigate to="/login" replace />;
};

const LoginPage = lazy(() => import('./pages/LoginPage'));
const DashboardPage = lazy(() => import('./pages/dashboard/DashboardPage'));
const TrainingPlansPage = lazy(() => import('./pages/training/TrainingPlansPage'));
const TrainingActionsPage = lazy(() => import('./pages/training/TrainingActionsPage'));
const CpfPage = lazy(() => import('./pages/cpf/CpfPage'));
const CompetenciesPage = lazy(() => import('./pages/competency/CompetenciesPage'));
const EntretiensPage = lazy(() => import('./pages/entretien/EntretiensPage'));
const CertificationsPage = lazy(() => import('./pages/certification/CertificationsPage'));
const ElearningPage = lazy(() => import('./pages/elearning/ElearningPage'));
const EvaluationPage = lazy(() => import('./pages/evaluation/EvaluationPage'));
const ProvidersPage = lazy(() => import('./pages/provider/ProvidersPage'));
const OpcoPage = lazy(() => import('./pages/opco/OpcoPage'));
const BilanPage = lazy(() => import('./pages/bilan/BilanPage'));
const AlternancePage = lazy(() => import('./pages/alternance/AlternancePage'));
const CompliancePage = lazy(() => import('./pages/compliance/CompliancePage'));
const GdprAdminPage = lazy(() => import('./pages/gdpr/GdprAdminPage'));

const App: React.FC = () => {
  return (
    <ConfigProvider locale={frFR} theme={{ token: { colorPrimary: '#1890ff' } }}>
      <ErrorBoundary>
        <BrowserRouter>
          <AuthProvider>
            <AppProvider>
              <Suspense fallback={<PageLoading />}>
                <Routes>
                  <Route path="/login" element={<LoginPage />} />
                  <Route element={<ProtectedRoute />}>
                  <Route element={<AppLayout />}>
                    <Route path="/" element={<DashboardPage />} />
                    <Route path="/training-plans" element={<TrainingPlansPage />} />
                    <Route path="/training-actions" element={<TrainingActionsPage />} />
                    <Route path="/cpf" element={<CpfPage />} />
                    <Route path="/competencies" element={<CompetenciesPage />} />
                    <Route path="/entretiens" element={<EntretiensPage />} />
                    <Route path="/certifications" element={<CertificationsPage />} />
                    <Route path="/elearning" element={<ElearningPage />} />
                    <Route path="/evaluations" element={<EvaluationPage />} />
                    <Route path="/providers" element={<ProvidersPage />} />
                    <Route path="/opco" element={<OpcoPage />} />
                    <Route path="/bilan" element={<BilanPage />} />
                    <Route path="/alternance" element={<AlternancePage />} />
                    <Route path="/compliance" element={<CompliancePage />} />
                    <Route path="/gdpr" element={<GdprAdminPage />} />
                    <Route path="*" element={<Navigate to="/" replace />} />
                  </Route>
                  </Route>
                </Routes>
              </Suspense>
            </AppProvider>
          </AuthProvider>
        </BrowserRouter>
      </ErrorBoundary>
    </ConfigProvider>
  );
};

export default App;
