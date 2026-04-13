import React, { useEffect, useState, useCallback } from 'react';
import {
  Table, Button, Tag, Space, Typography, Card, Modal, Form, Input,
  Select, message, Alert, Row, Col, DatePicker, Statistic,
} from 'antd';
import { PlusOutlined, WarningOutlined, CheckCircleOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { complianceApi } from '../../../services/trainingApiService';
import { extractApiError } from '../../../services/utils/extractApiError';

const { Title } = Typography;

const RISK_LEVEL_OPTIONS = [
  { value: 'Low', label: 'Faible' },
  { value: 'Medium', label: 'Moyen' },
  { value: 'High', label: 'Eleve' },
  { value: 'Critical', label: 'Critique' },
];

const RISK_COLORS: Record<string, string> = {
  Low: 'success', Medium: 'warning', High: 'orange', Critical: 'error',
};

interface TrainingObligation {
  Id: string;
  Title: string;
  LegalReference?: string;
  ObligationFrequencyMonths?: number;
  RiskLevel?: string;
  DueDate?: string;
  IsCompliant?: boolean;
}

const CompliancePage: React.FC = () => {
  const [obligations, setObligations] = useState<TrainingObligation[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<TrainingObligation | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [riskSummary, setRiskSummary] = useState<Record<string, number>>({});
  const [form] = Form.useForm();

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await complianceApi.getAll(page, 20);
      const items = (result as { Items?: TrainingObligation[] }).Items ?? (result as TrainingObligation[]);
      setObligations(Array.isArray(items) ? items : []);
      setTotalCount((result as { TotalCount?: number }).TotalCount ?? 0);
    } catch (err: unknown) {
      setError(extractApiError(err));
    } finally {
      setLoading(false);
    }
  }, [page]);

  const loadRiskAssessment = useCallback(async () => {
    try {
      const assessment = await complianceApi.getRiskAssessment();
      setRiskSummary(assessment as Record<string, number>);
    } catch {
      // non-blocking
    }
  }, []);

  useEffect(() => { load(); loadRiskAssessment(); }, [load, loadRiskAssessment]);

  const openEdit = (record: TrainingObligation) => {
    setEditing(record);
    form.setFieldsValue({
      ...record,
      DueDate: record.DueDate ? dayjs(record.DueDate) : undefined,
    });
    setModalOpen(true);
  };

  const handleSubmit = async (values: Record<string, unknown>) => {
    setSubmitting(true);
    try {
      const dto = {
        ...values,
        DueDate: (values.DueDate as dayjs.Dayjs)?.toISOString(),
      };
      if (editing) {
        await complianceApi.update(editing.Id, dto);
        message.success('Obligation mise a jour');
      } else {
        await complianceApi.create(dto);
        message.success('Obligation creee');
      }
      setModalOpen(false);
      form.resetFields();
      load();
    } catch (err: unknown) {
      message.error(extractApiError(err));
    } finally {
      setSubmitting(false);
    }
  };

  const columns = [
    { title: 'Titre', dataIndex: 'Title', key: 'title', ellipsis: true },
    { title: 'Ref. legale', dataIndex: 'LegalReference', key: 'legal', ellipsis: true },
    {
      title: 'Periodicite (mois)',
      dataIndex: 'ObligationFrequencyMonths',
      key: 'freq',
      width: 130,
    },
    {
      title: 'Niveau de risque',
      dataIndex: 'RiskLevel',
      key: 'risk',
      render: (r: string) => r ? <Tag color={RISK_COLORS[r] ?? 'default'}>{r}</Tag> : '-',
    },
    {
      title: 'Echeance',
      dataIndex: 'DueDate',
      key: 'due',
      render: (d: string) => {
        if (!d) return '-';
        const isOverdue = dayjs(d).isBefore(dayjs());
        return <Tag color={isOverdue ? 'error' : 'default'}>{dayjs(d).format('DD/MM/YYYY')}</Tag>;
      },
    },
    {
      title: 'Conforme',
      dataIndex: 'IsCompliant',
      key: 'compliant',
      render: (v: boolean) => v === undefined ? '-' : v ? (
        <Tag color="success" icon={<CheckCircleOutlined />}>Oui</Tag>
      ) : (
        <Tag color="error" icon={<WarningOutlined />}>Non</Tag>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: unknown, record: TrainingObligation) => (
        <Button type="link" size="small" onClick={() => openEdit(record)}>Modifier</Button>
      ),
    },
  ];

  const criticalCount = obligations.filter(o => o.RiskLevel === 'Critical' || o.RiskLevel === 'High').length;
  const nonCompliantCount = obligations.filter(o => o.IsCompliant === false).length;

  return (
    <Space direction="vertical" size={16} style={{ width: '100%' }}>
      <Row justify="space-between" align="middle">
        <Col><Title level={4} style={{ margin: 0 }}>Conformite legale formation</Title></Col>
        <Col>
          <Button type="primary" icon={<PlusOutlined />} onClick={() => { setEditing(null); form.resetFields(); setModalOpen(true); }}>
            Nouvelle obligation
          </Button>
        </Col>
      </Row>

      <Row gutter={16}>
        <Col span={6}>
          <Card>
            <Statistic title="Obligations totales" value={totalCount} />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="Non conformes"
              value={nonCompliantCount}
              valueStyle={{ color: nonCompliantCount > 0 ? '#cf1322' : '#3f8600' }}
              prefix={nonCompliantCount > 0 ? <WarningOutlined /> : <CheckCircleOutlined />}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="Risques critiques/eleves"
              value={criticalCount}
              valueStyle={{ color: criticalCount > 0 ? '#cf1322' : '#3f8600' }}
            />
          </Card>
        </Col>
      </Row>

      {error && <Alert type="error" message={error} showIcon closable />}

      <Card>
        <Table
          dataSource={obligations}
          columns={columns}
          rowKey="Id"
          loading={loading}
          pagination={{
            current: page, total: totalCount, pageSize: 20,
            onChange: setPage, showTotal: (t) => `${t} obligations`,
          }}
          locale={{ emptyText: 'Aucune obligation de formation enregistree.' }}
        />
      </Card>

      <Modal
        title={editing ? 'Modifier l\'obligation' : 'Nouvelle obligation de formation'}
        open={modalOpen}
        onCancel={() => setModalOpen(false)}
        footer={null}
        width={580}
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <Form.Item name="Title" label="Titre" rules={[{ required: true }]}>
            <Input placeholder="Formation securite incendie obligatoire" />
          </Form.Item>
          <Form.Item name="LegalReference" label="Reference legale">
            <Input placeholder="Art. L6321-1 du Code du travail" />
          </Form.Item>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="ObligationFrequencyMonths" label="Periodicite (mois)">
                <Select options={[
                  { value: 12, label: 'Annuelle (12 mois)' },
                  { value: 24, label: 'Biennale (24 mois)' },
                  { value: 36, label: 'Triennale (36 mois)' },
                  { value: 60, label: 'Quinquennale (60 mois)' },
                ]} />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="RiskLevel" label="Niveau de risque" initialValue="Medium">
                <Select options={RISK_LEVEL_OPTIONS} />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="DueDate" label="Prochaine echeance">
            <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={submitting} block>
              {editing ? 'Mettre a jour' : 'Creer l\'obligation'}
            </Button>
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
};

export default CompliancePage;
