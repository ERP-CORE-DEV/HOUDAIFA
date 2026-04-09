import React, { useEffect, useState } from 'react';
import { Row, Col, Card, Statistic, Spin, Alert } from 'antd';
import {
  BookOutlined,
  TeamOutlined,
  EuroCircleOutlined,
  CheckCircleOutlined,
  StarOutlined,
  CalendarOutlined,
} from '@ant-design/icons';
import { analyticsService } from '../../services/training';
import { TrainingDashboardKpi } from '../../types/training';

const DashboardPage: React.FC = () => {
  const [kpis, setKpis] = useState<TrainingDashboardKpi | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const currentYear = new Date().getFullYear();

  useEffect(() => {
    const loadKpis = async () => {
      try {
        const data = await analyticsService.getDashboardKpis(currentYear);
        setKpis(data);
      } catch {
        setError('Impossible de charger les indicateurs du tableau de bord');
      } finally {
        setLoading(false);
      }
    };
    loadKpis();
  }, [currentYear]);

  if (loading) return <Spin size="large" style={{ display: 'block', margin: '100px auto' }} />;
  if (error) return <Alert type="warning" message={error} showIcon />;

  return (
    <>
      <h2>Tableau de bord Formation {currentYear}</h2>
      <Row gutter={[16, 16]} style={{ marginTop: 24 }}>
        <Col xs={24} sm={12} lg={8}>
          <Card>
            <Statistic title="Plans de formation" value={kpis?.TotalTrainingPlans ?? 0} prefix={<BookOutlined />} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={8}>
          <Card>
            <Statistic title="Actions de formation" value={kpis?.TotalTrainingActions ?? 0} prefix={<CalendarOutlined />} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={8}>
          <Card>
            <Statistic title="Sessions planifiees" value={kpis?.TotalSessions ?? 0} prefix={<TeamOutlined />} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={8}>
          <Card>
            <Statistic
              title="Budget consomme"
              value={kpis?.TotalBudgetConsumed ?? 0}
              suffix="EUR"
              prefix={<EuroCircleOutlined />}
              precision={0}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={8}>
          <Card>
            <Statistic
              title="Taux de completion"
              value={kpis?.CompletionRate ?? 0}
              suffix="%"
              prefix={<CheckCircleOutlined />}
              precision={1}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={8}>
          <Card>
            <Statistic
              title="Satisfaction moyenne"
              value={kpis?.AverageSatisfactionScore ?? 0}
              suffix="/ 5"
              prefix={<StarOutlined />}
              precision={1}
            />
          </Card>
        </Col>
      </Row>
    </>
  );
};

export default DashboardPage;
