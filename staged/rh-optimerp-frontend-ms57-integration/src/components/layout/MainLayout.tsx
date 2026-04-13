import React, { useState } from 'react';
import { Layout, Menu, Avatar, Typography, Tooltip, Button, Breadcrumb } from 'antd';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import {
  DashboardOutlined,
  FileTextOutlined,
  TeamOutlined,
  SolutionOutlined,
  FlagOutlined,
  CalendarOutlined,
  RocketOutlined,
  ApartmentOutlined,
  TrophyOutlined,
  SwapOutlined,
  BookOutlined,
  SafetyOutlined,
  AuditOutlined,
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  LogoutOutlined,
  BulbOutlined,
  BankOutlined,
  SafetyCertificateOutlined,
  DesktopOutlined,
} from '@ant-design/icons';
import { useAuth } from '../../hooks/useAuth';
import { useColorScheme } from '../../hooks/useColorScheme';

const { Sider, Content, Header } = Layout;
const { Text } = Typography;

interface NavItem {
  key: string;
  path: string;
  label: string;
  icon: React.ReactNode;
}

interface NavGroup {
  groupLabel: string;
  items: NavItem[];
}

const NAV_GROUPS: NavGroup[] = [
  {
    groupLabel: 'VUE D\'ENSEMBLE',
    items: [
      { key: '/', path: '/', label: 'Tableau de bord', icon: <DashboardOutlined /> },
    ],
  },
  {
    groupLabel: 'RECRUTEMENT (MS 5.1)',
    items: [
      { key: '/sourcing/jobs', path: '/sourcing/jobs', label: 'Offres d\'emploi', icon: <FileTextOutlined /> },
      { key: '/sourcing/candidates', path: '/sourcing/candidates', label: 'Candidats', icon: <TeamOutlined /> },
      { key: '/sourcing/matching', path: '/sourcing/matching', label: 'Matching', icon: <SwapOutlined /> },
      { key: '/sourcing/campaigns', path: '/sourcing/campaigns', label: 'Campagnes', icon: <FlagOutlined /> },
      { key: '/sourcing/postings', path: '/sourcing/postings', label: 'Publications', icon: <RocketOutlined /> },
      { key: '/sourcing/analytics', path: '/sourcing/analytics', label: 'Analytiques', icon: <AuditOutlined /> },
      { key: '/sourcing/scheduling', path: '/sourcing/scheduling', label: 'Planification', icon: <CalendarOutlined /> },
    ],
  },
  {
    groupLabel: 'GESTION DES TALENTS (MS 5.8)',
    items: [
      { key: '/talent/jobs', path: '/talent/jobs', label: 'Postes', icon: <FileTextOutlined /> },
      { key: '/talent/career', path: '/talent/career', label: 'Plans de carriere', icon: <TrophyOutlined /> },
      { key: '/talent/succession', path: '/talent/succession', label: 'Succession', icon: <ApartmentOutlined /> },
      { key: '/talent/pools', path: '/talent/pools', label: 'Viviers de talents', icon: <TeamOutlined /> },
    ],
  },
  {
    groupLabel: 'EVALUATION (MS 5.2)',
    items: [
      { key: '/evaluation', path: '/evaluation', label: 'Evaluations', icon: <SolutionOutlined /> },
    ],
  },
  {
    groupLabel: 'RECRUTEMENT PROCESS (MS 5.3)',
    items: [
      { key: '/hiring', path: '/hiring', label: 'Processus de recrutement', icon: <SafetyOutlined /> },
    ],
  },
  {
    groupLabel: 'FORMATION (MS 5.7)',
    items: [
      { key: '/training/plans', path: '/training/plans', label: 'Plans de formation', icon: <BookOutlined /> },
      { key: '/training/actions', path: '/training/actions', label: 'Actions de formation', icon: <CalendarOutlined /> },
      { key: '/training/cpf', path: '/training/cpf', label: 'CPF', icon: <BankOutlined /> },
      { key: '/training/competencies', path: '/training/competencies', label: 'Competences', icon: <TrophyOutlined /> },
      { key: '/training/entretiens', path: '/training/entretiens', label: 'Entretiens pro', icon: <SolutionOutlined /> },
      { key: '/training/certifications', path: '/training/certifications', label: 'Certifications / VAE', icon: <SafetyCertificateOutlined /> },
      { key: '/training/elearning', path: '/training/elearning', label: 'E-Learning', icon: <DesktopOutlined /> },
      { key: '/training/evaluations', path: '/training/evaluations', label: 'Evaluations', icon: <AuditOutlined /> },
      { key: '/training/providers', path: '/training/providers', label: 'Organismes', icon: <ApartmentOutlined /> },
      { key: '/training/opco', path: '/training/opco', label: 'OPCO', icon: <BankOutlined /> },
      { key: '/training/bilan', path: '/training/bilan', label: 'Bilan de competences', icon: <FileTextOutlined /> },
      { key: '/training/alternance', path: '/training/alternance', label: 'Alternance', icon: <TeamOutlined /> },
      { key: '/training/compliance', path: '/training/compliance', label: 'Conformite', icon: <SafetyOutlined /> },
      { key: '/training/gdpr', path: '/training/gdpr', label: 'RGPD', icon: <AuditOutlined /> },
    ],
  },
];

interface MainLayoutProps {
  children: React.ReactNode;
}

const MainLayout: React.FC<MainLayoutProps> = ({ children }) => {
  const [collapsed, setCollapsed] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const { user, logout } = useAuth();
  const { isDark, toggle } = useColorScheme();

  const selectedKey = location.pathname === '/'
    ? '/'
    : NAV_GROUPS.flatMap(g => g.items).find(item =>
        item.path !== '/' && location.pathname.startsWith(item.path)
      )?.key ?? location.pathname;

  const menuItems = NAV_GROUPS.flatMap(group => [
    {
      key: `group-${group.groupLabel}`,
      type: 'group' as const,
      label: collapsed ? null : (
        <Text
          style={{
            fontSize: 10,
            fontWeight: 600,
            letterSpacing: '0.08em',
            color: 'rgba(148, 163, 184, 0.7)',
            textTransform: 'uppercase',
          }}
        >
          {group.groupLabel}
        </Text>
      ),
      children: group.items.map(item => ({
        key: item.key,
        icon: item.icon,
        label: item.label,
        onClick: () => navigate(item.path),
      })),
    },
  ]);

  const breadcrumbParts = location.pathname.split('/').filter(Boolean);
  const breadcrumbItems = [
    { title: <Link to="/">Accueil</Link> },
    ...breadcrumbParts.map((part, index) => {
      const path = '/' + breadcrumbParts.slice(0, index + 1).join('/');
      const label = part.charAt(0).toUpperCase() + part.slice(1);
      return { title: index === breadcrumbParts.length - 1 ? label : <Link to={path}>{label}</Link> };
    }),
  ];

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider
        collapsible
        collapsed={collapsed}
        trigger={null}
        width={240}
        collapsedWidth={64}
        style={{
          background: 'var(--sidebar-bg)',
          position: 'fixed',
          left: 0,
          top: 0,
          bottom: 0,
          zIndex: 100,
          overflow: 'auto',
          display: 'flex',
          flexDirection: 'column',
        }}
      >
        {/* Logo */}
        <div
          style={{
            height: 56,
            display: 'flex',
            alignItems: 'center',
            padding: collapsed ? '0 20px' : '0 20px',
            borderBottom: '1px solid rgba(255,255,255,0.06)',
            gap: 10,
          }}
        >
          <div
            style={{
              width: 28,
              height: 28,
              borderRadius: 8,
              background: 'linear-gradient(135deg, #7c3aed, #5b21b6)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              flexShrink: 0,
              fontSize: 14,
              fontWeight: 700,
              color: '#fff',
            }}
          >
            RH
          </div>
          {!collapsed && (
            <Text style={{ color: '#fff', fontWeight: 600, fontSize: 14, letterSpacing: '-0.01em' }}>
              OptimERP
            </Text>
          )}
        </div>

        {/* Navigation */}
        <Menu
          mode="inline"
          selectedKeys={[selectedKey]}
          items={menuItems}
          style={{
            background: 'transparent',
            border: 'none',
            flex: 1,
            padding: '8px 0',
          }}
          theme="dark"
        />

        {/* User profile at bottom */}
        <div
          style={{
            padding: collapsed ? '12px 16px' : '12px 16px',
            borderTop: '1px solid rgba(255,255,255,0.06)',
            display: 'flex',
            alignItems: 'center',
            gap: 10,
          }}
        >
          <Avatar
            size={32}
            style={{ background: 'linear-gradient(135deg, #7c3aed, #5b21b6)', flexShrink: 0, fontSize: 12 }}
          >
            {user ? `${user.FirstName[0]}${user.LastName[0]}` : 'U'}
          </Avatar>
          {!collapsed && (
            <div style={{ flex: 1, minWidth: 0 }}>
              <Text
                style={{ color: '#f8fafc', fontSize: 12, fontWeight: 600, display: 'block', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}
              >
                {user ? `${user.FirstName} ${user.LastName}` : 'Utilisateur'}
              </Text>
              <Text style={{ color: 'rgba(148,163,184,0.8)', fontSize: 11 }}>
                {user?.Role ?? 'Role'}
              </Text>
            </div>
          )}
          {!collapsed && (
            <Tooltip title="Deconnexion">
              <Button
                type="text"
                icon={<LogoutOutlined />}
                onClick={() => logout()}
                size="small"
                style={{ color: 'rgba(148,163,184,0.7)' }}
              />
            </Tooltip>
          )}
        </div>
      </Sider>

      <Layout style={{ marginLeft: collapsed ? 64 : 240, transition: 'margin-left 0.2s' }}>
        <Header
          style={{
            background: 'var(--color-background-surface)',
            borderBottom: '1px solid var(--color-border-default)',
            padding: '0 24px',
            display: 'flex',
            alignItems: 'center',
            gap: 16,
            position: 'sticky',
            top: 0,
            zIndex: 99,
            height: 56,
          }}
        >
          <Button
            type="text"
            icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
            onClick={() => setCollapsed(!collapsed)}
            style={{ color: 'var(--color-text-secondary)' }}
          />
          <Breadcrumb items={breadcrumbItems} style={{ flex: 1 }} />
          <Tooltip title={isDark ? 'Mode clair' : 'Mode sombre'}>
            <Button
              type="text"
              icon={<BulbOutlined />}
              onClick={toggle}
              style={{ color: 'var(--color-text-secondary)' }}
            />
          </Tooltip>
        </Header>

        <Content
          style={{
            padding: 24,
            background: 'var(--color-background-default)',
            minHeight: 'calc(100vh - 56px)',
          }}
        >
          {children}
        </Content>
      </Layout>
    </Layout>
  );
};

export default MainLayout;
