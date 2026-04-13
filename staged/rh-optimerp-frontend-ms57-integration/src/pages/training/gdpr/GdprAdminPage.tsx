import React, { useState } from 'react';
import {
  Button, Space, Typography, Card, Form, Input, message, Alert, Row, Col,
  Descriptions, Tag, Spin, Divider,
} from 'antd';
import {
  SafetyOutlined, ExportOutlined, DeleteOutlined, InfoCircleOutlined,
} from '@ant-design/icons';
import { gdprApi } from '../../../services/trainingApiService';
import { extractApiError } from '../../../services/utils/extractApiError';

const { Title, Text, Paragraph } = Typography;

interface RetentionStatus {
  EmployeeId: string;
  DataRetentionDays: number;
  IsAnonymized: boolean;
  LastAccessDate?: string;
  ScheduledDeletionDate?: string;
}

const GdprAdminPage: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [retentionData, setRetentionData] = useState<RetentionStatus | null>(null);
  const [exportData, setExportData] = useState<unknown>(null);
  const [form] = Form.useForm();

  const handleAnonymize = async (values: Record<string, unknown>) => {
    const employeeId = values.employeeId as string;
    setLoading(true);
    setError(null);
    try {
      await gdprApi.anonymize(employeeId);
      message.success(`Donnees anonymisees pour l\'employe ${employeeId}`);
    } catch (err: unknown) {
      setError(extractApiError(err));
    } finally {
      setLoading(false);
    }
  };

  const handleExport = async (values: Record<string, unknown>) => {
    const employeeId = values.exportEmployeeId as string;
    setLoading(true);
    setError(null);
    try {
      const data = await gdprApi.exportData(employeeId);
      setExportData(data);
      message.success('Donnees exportees avec succes');
    } catch (err: unknown) {
      setError(extractApiError(err));
    } finally {
      setLoading(false);
    }
  };

  const handleRetentionStatus = async (values: Record<string, unknown>) => {
    setLoading(true);
    setError(null);
    try {
      const status = await gdprApi.getRetentionStatus();
      setRetentionData(status as RetentionStatus);
    } catch (err: unknown) {
      setError(extractApiError(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <Row align="middle">
        <Col>
          <Space>
            <SafetyOutlined style={{ fontSize: 20, color: '#7c3aed' }} />
            <Title level={4} style={{ margin: 0 }}>Administration RGPD</Title>
          </Space>
        </Col>
      </Row>

      <Alert
        type="warning"
        icon={<InfoCircleOutlined />}
        showIcon
        message="Zone d'administration RGPD"
        description="Ces actions sont irreversibles et soumises au Reglement General sur la Protection des Donnees (RGPD). Toute action est journalisee."
      />

      {error && <Alert type="error" message={error} showIcon closable />}

      <Row gutter={16}>
        {/* Anonymisation */}
        <Col span={8}>
          <Card
            title={<Space><DeleteOutlined style={{ color: '#cf1322' }} /><span>Anonymisation</span></Space>}
            size="small"
          >
            <Paragraph type="secondary" style={{ fontSize: 12 }}>
              Supprime les donnees personnelles d'un employe en les remplacant par des valeurs anonymes.
            </Paragraph>
            <Form layout="vertical" onFinish={handleAnonymize}>
              <Form.Item name="employeeId" label="ID Employe" rules={[{ required: true }]}>
                <Input placeholder="employee-001" />
              </Form.Item>
              <Form.Item>
                <Button type="primary" danger htmlType="submit" loading={loading} block icon={<DeleteOutlined />}>
                  Anonymiser
                </Button>
              </Form.Item>
            </Form>
          </Card>
        </Col>

        {/* Export */}
        <Col span={8}>
          <Card
            title={<Space><ExportOutlined style={{ color: '#1677ff' }} /><span>Export des donnees</span></Space>}
            size="small"
          >
            <Paragraph type="secondary" style={{ fontSize: 12 }}>
              Exporte toutes les donnees personnelles d'un employe (droit a la portabilite, Art. 20 RGPD).
            </Paragraph>
            <Form layout="vertical" onFinish={handleExport}>
              <Form.Item name="exportEmployeeId" label="ID Employe" rules={[{ required: true }]}>
                <Input placeholder="employee-001" />
              </Form.Item>
              <Form.Item>
                <Button type="primary" htmlType="submit" loading={loading} block icon={<ExportOutlined />}>
                  Exporter
                </Button>
              </Form.Item>
            </Form>
          </Card>
        </Col>

        {/* Retention Status */}
        <Col span={8}>
          <Card
            title={<Space><InfoCircleOutlined style={{ color: '#52c41a' }} /><span>Statut de retention</span></Space>}
            size="small"
          >
            <Paragraph type="secondary" style={{ fontSize: 12 }}>
              Consulte le statut de retention des donnees pour l'ensemble des employes.
            </Paragraph>
            <Form layout="vertical" onFinish={handleRetentionStatus}>
              <Form.Item>
                <Button type="default" htmlType="submit" loading={loading} block icon={<InfoCircleOutlined />}>
                  Consulter le statut
                </Button>
              </Form.Item>
            </Form>
          </Card>
        </Col>
      </Row>

      {/* Export results */}
      {exportData && (
        <Card title="Donnees exportees" size="small">
          <Spin spinning={loading}>
            <pre style={{
              background: 'var(--color-background-default)',
              padding: 12,
              borderRadius: 6,
              fontSize: 12,
              maxHeight: 400,
              overflow: 'auto',
            }}>
              {JSON.stringify(exportData, null, 2)}
            </pre>
          </Spin>
        </Card>
      )}

      {/* Retention status results */}
      {retentionData && (
        <Card title="Statut de retention" size="small">
          <Descriptions bordered column={2} size="small">
            <Descriptions.Item label="ID Employe">
              <Text code>{retentionData.EmployeeId}</Text>
            </Descriptions.Item>
            <Descriptions.Item label="Anonymise">
              <Tag color={retentionData.IsAnonymized ? 'success' : 'warning'}>
                {retentionData.IsAnonymized ? 'Oui' : 'Non'}
              </Tag>
            </Descriptions.Item>
            <Descriptions.Item label="Retention (jours)">
              {retentionData.DataRetentionDays} jours
            </Descriptions.Item>
            <Descriptions.Item label="Suppression planifiee">
              {retentionData.ScheduledDeletionDate ?? '-'}
            </Descriptions.Item>
          </Descriptions>
        </Card>
      )}

      <Divider />
      <Alert
        type="info"
        message="Base legale : RGPD Art. 17 (droit a l'effacement), Art. 20 (portabilite), Art. 5(1)(e) (limitation de conservation)"
        showIcon
      />
    </>
  );
};

export default GdprAdminPage;
