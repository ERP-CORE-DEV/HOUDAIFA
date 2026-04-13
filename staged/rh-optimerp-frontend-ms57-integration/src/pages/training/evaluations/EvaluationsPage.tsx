import React, { useEffect, useState, useCallback } from 'react';
import {
  Table, Button, Tag, Space, Typography, Card, Modal, Form, Input, InputNumber,
  Select, message, Alert, Row, Col,
} from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import { evaluationApi } from '../../../services/trainingApiService';
import { extractApiError } from '../../../services/utils/extractApiError';
import type { TrainingEvaluation, EvaluationLevel } from '../../../types/training';

const { Title } = Typography;

const EVALUATION_LEVELS: EvaluationLevel[] = ['Satisfaction', 'Apprentissage', 'Transfert', 'Resultats'];

const LEVEL_COLORS: Record<EvaluationLevel, string> = {
  Satisfaction: 'blue',
  Apprentissage: 'cyan',
  Transfert: 'green',
  Resultats: 'purple',
};

const EvaluationsPage: React.FC = () => {
  const [evaluations, setEvaluations] = useState<TrainingEvaluation[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [form] = Form.useForm();
  const [searchForm] = Form.useForm();

  const load = useCallback(async () => {
    setLoading(false);
    setEvaluations([]);
  }, []);

  useEffect(() => { load(); }, [load]);

  const handleSearch = async (values: Record<string, unknown>) => {
    setLoading(true);
    setError(null);
    try {
      const results = await evaluationApi.getBySession(values.sessionId as string);
      setEvaluations(results);
    } catch (err: unknown) {
      setError(extractApiError(err));
      setEvaluations([]);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (values: Record<string, unknown>) => {
    setSubmitting(true);
    try {
      await evaluationApi.create(values as Partial<TrainingEvaluation>);
      message.success('Evaluation creee');
      setModalOpen(false);
      form.resetFields();
    } catch (err: unknown) {
      message.error(extractApiError(err));
    } finally {
      setSubmitting(false);
    }
  };

  const columns = [
    { title: 'ID Employe', dataIndex: 'EmployeeId', key: 'employee', ellipsis: true },
    {
      title: 'Type (Kirkpatrick)',
      dataIndex: 'EvaluationType',
      key: 'type',
      render: (t: EvaluationLevel) => <Tag color={LEVEL_COLORS[t] ?? 'default'}>{t}</Tag>,
    },
    {
      title: 'Score global',
      dataIndex: 'OverallScore',
      key: 'score',
      render: (s: number) => {
        const color = s >= 4 ? 'success' : s >= 3 ? 'warning' : 'error';
        return <Tag color={color}>{s} / 5</Tag>;
      },
    },
    { title: 'Commentaires', dataIndex: 'Comments', key: 'comments', ellipsis: true },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: unknown, record: TrainingEvaluation) => (
        <Button type="link" size="small" onClick={() => { form.setFieldsValue(record); setModalOpen(true); }}>
          Modifier
        </Button>
      ),
    },
  ];

  return (
    <Space direction="vertical" size={16} style={{ width: '100%' }}>
      <Row justify="space-between" align="middle">
        <Col><Title level={4} style={{ margin: 0 }}>Evaluations de formation (Kirkpatrick)</Title></Col>
        <Col>
          <Button type="primary" icon={<PlusOutlined />} onClick={() => { form.resetFields(); setModalOpen(true); }}>
            Nouvelle evaluation
          </Button>
        </Col>
      </Row>

      <Card>
        <Form form={searchForm} layout="inline" onFinish={handleSearch} style={{ marginBottom: 16 }}>
          <Form.Item name="sessionId" label="ID Session" rules={[{ required: true }]}>
            <Input placeholder="session-001" style={{ width: 220 }} />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={loading}>Rechercher</Button>
          </Form.Item>
        </Form>
        {error && <Alert type="error" message={error} showIcon closable style={{ marginBottom: 8 }} />}
        <Table
          dataSource={evaluations}
          columns={columns}
          rowKey="Id"
          loading={loading}
          pagination={{ pageSize: 20 }}
          locale={{ emptyText: 'Recherchez par ID session.' }}
        />
      </Card>

      <Modal
        title="Evaluation de formation"
        open={modalOpen}
        onCancel={() => setModalOpen(false)}
        footer={null}
        width={560}
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="TrainingActionId" label="ID Action" rules={[{ required: true }]}>
                <Input placeholder="action-001" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="SessionId" label="ID Session" rules={[{ required: true }]}>
                <Input placeholder="session-001" />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="EmployeeId" label="ID Employe" rules={[{ required: true }]}>
                <Input placeholder="employee-001" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="EvaluationType" label="Niveau Kirkpatrick" rules={[{ required: true }]} initialValue="Satisfaction">
                <Select options={EVALUATION_LEVELS.map(v => ({ value: v, label: v }))} />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="OverallScore" label="Score global (1-5)" rules={[{ required: true }]}>
            <InputNumber style={{ width: '100%' }} min={1} max={5} step={0.5} />
          </Form.Item>
          <Form.Item name="Comments" label="Commentaires">
            <Input.TextArea rows={3} />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={submitting} block>Enregistrer</Button>
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
};

export default EvaluationsPage;
