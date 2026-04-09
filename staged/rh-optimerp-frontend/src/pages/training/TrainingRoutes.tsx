import React, { lazy, Suspense } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { Spin } from 'antd';

const DashboardPage = lazy(() => import('./DashboardPage'));
const TrainingPlansPage = lazy(() => import('./TrainingPlansPage'));
const TrainingActionsPage = lazy(() => import('./TrainingActionsPage'));
const CpfPage = lazy(() => import('./CpfPage'));
const CompetenciesPage = lazy(() => import('./CompetenciesPage'));
const EntretiensPage = lazy(() => import('./EntretiensPage'));
const CertificationsPage = lazy(() => import('./CertificationsPage'));
const ElearningPage = lazy(() => import('./ElearningPage'));
const EvaluationPage = lazy(() => import('./EvaluationPage'));
const ProvidersPage = lazy(() => import('./ProvidersPage'));
const OpcoPage = lazy(() => import('./OpcoPage'));
const BilanPage = lazy(() => import('./BilanPage'));
const AlternancePage = lazy(() => import('./AlternancePage'));
const CompliancePage = lazy(() => import('./CompliancePage'));
const GdprAdminPage = lazy(() => import('./GdprAdminPage'));

const TrainingRoutes: React.FC = () => (
  <Suspense
    fallback={
      <div style={{ display: 'flex', justifyContent: 'center', padding: 100 }}>
        <Spin size="large" />
      </div>
    }
  >
    <Routes>
      <Route path="/" element={<DashboardPage />} />
      <Route path="/plans" element={<TrainingPlansPage />} />
      <Route path="/actions" element={<TrainingActionsPage />} />
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
      <Route path="*" element={<Navigate to="/training" replace />} />
    </Routes>
  </Suspense>
);

export default TrainingRoutes;
