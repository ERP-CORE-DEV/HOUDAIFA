import React, { useEffect, useState, useCallback } from 'react';
import {
  Table, Button, Tag, Space, Typography, Card, Modal, Form, Input, InputNumber,
  Select, message, Popconfirm, Alert, Row, Col,
} from 'antd';
import { PlusOutlined, CheckOutlined, DeleteOutlined } from '@ant-design/icons';
import { trainingPlanApi } from '../../../services/trainingApiService';
import { extractApiError } from '../../../services/utils/extractApiError';
import type { TrainingPlan, TrainingPlanStatus } from '../../../types/training';

const { Title } = Typography;

const STATUS_COLORS: Record<TrainingPlanStatus, string> = {
  Draft: 'default',
  PendingApproval: 'processing',
  Approved: 'success',
  InExecution: 'blue',
  Completed: 'green',
  Archived: 'default',
};

const STATUS_OPTIONS: TrainingPlanStatus[] = [
  'Draft', 'PendingApproval', 'Approved', 'InExecution', 'Completed', 'Archived',
];

const TrainingPlansPage: React.FC = () => {
  const [plans, setPlans] = useState<TrainingPlan[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<TrainingPlan | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [form] = Form.useForm();

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await trainingPlanApi.getAll(page, 20);
      setPlans(result.Items);
      setTotalCount(result.TotalCount);
    } catch (err: unknown) {
      setError(extractApiError(err));
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => { load(); }, [load]);

  const openCreate = () => { setEditing(null); form.resetFields(); setModalOpen(true); };
  const openEdit = (record: TrainingPlan) => {
    setEditing(record);
    form.setFieldsValue(record);
    setModalOpen(true);
  };

  const handleSubmit = async (values: Record<string, unknown>) => {
    setSubmitting(true);
    try {
      if (editing) {
        await trainingPlanApi.update(editing.Id, values as Partial<TrainingPlan>);
        message.success('Plan mis a jour');
      } else {
        await trainingPlanApi.create(values as Partial<TrainingPlan>);
        message.success('Plan cree');
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

  const handleApprove = async (id: string) => {
    try {
      await trainingPlanApi.approve(id, 'Admin RH');
      message.success('Plan approuve');
      load();
    } catch (err: unknown) {
      message.error(extractApiError(err));
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await trainingPlanApi.delete(id);
      message.success('Plan supprime');
      load();
    } catch (err: unknown) {
      message.error(extractApiError(err));
    }
  };

  const columns = [
    { title: 'Titre', dataIndex: 'Title', key: 'title', ellipsis: true },
    { title: 'Annee', dataIndex: 'Year', key: 'year', width: 80 },
    {
      title: 'Statut',
      dataIndex: 'Status',
      key: 'status',
      render: (s: TrainingPlanStatus) => <Tag color={STATUS_COLORS[s]}>{s}</Tag>,
    },
    {
      title: 'Budget alloue',
      dataIndex: 'BudgetAllocated',
      key: 'budget',
      render: (v: number) => `${v.toLocaleString('fr-FR')} EUR`,
    },
    {
      title: 'Budget consomme',
      dataIndex: 'BudgetConsumed',
      key: 'consumed',
      render: (v: number) => `${v.toLocaleString('fr-FR')} EUR`,
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 180,
      render: (_: unknown, record: TrainingPlan) => (
        <Space size="small">
          <Button type="link" size="small" onClick={() => openEdit(record)}>Modifier</Button>
          {record.Status === 'PendingApproval' && (
            <Popconfirm title="Approuver ce plan ?" onConfirm={() => handleApprove(record.Id)}>
              <Button type="link" size="small" icon={<CheckOutlined />}>Approuver</Button>
            </Popconfirm>
          )}
          <Popconfirm title="Supprimer ce plan ?" onConfirm={() => handleDelete(record.Id)}>
            <Button type="link" size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <Space direction="vertical" size={16} style={{ width: '100%' }}>
      <Row justify="space-between" align="middle">
        <Col><Title level={4} style={{ margin: 0 }}>Plans de formation</Title></Col>
        <Col>
          <Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>Nouveau plan</Button>
        </Col>
      </Row>

      {error && <Alert type="error" message={error} showIcon closable />}

      <Card>
        <Table
          dataSource={plans}
          columns={columns}
          rowKey="Id"
          loading={loading}
          pagination={{
            current: page, total: totalCount, pageSize: 20,
            onChange: setPage, showTotal: (t) => `${t} plans`,
          }}
          locale={{ emptyText: 'Aucun plan de formation.' }}
        />
      </Card>

      <Modal
        title={editing ? 'Modifier le plan' : 'Nouveau plan de formation'}
        open={modalOpen}
        onCancel={() => setModalOpen(false)}
        footer={null}
        width={600}
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <Form.Item name="Title" label="Titre" rules={[{ required: true }]}>
            <Input placeholder="Plan de formation 2026" />
          </Form.Item>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="Year" label="Annee" rules={[{ required: true }]}>
                <InputNumber style={{ width: '100%' }} min={2020} max={2030} placeholder="2026" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="Status" label="Statut" initialValue="Draft">
                <Select options={STATUS_OPTIONS.map(v => ({ value: v, label: v }))} />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="BudgetAllocated" label="Budget alloue (EUR)">
                <InputNumber style={{ width: '100%' }} min={0} />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="MasseSalariale" label="Masse salariale (EUR)">
                <InputNumber style={{ width: '100%' }} min={0} />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="Description" label="Description">
            <Input.TextArea rows={3} />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={submitting} block>
              {editing ? 'Mettre a jour' : 'Creer le plan'}
            </Button>
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
};

export default TrainingPlansPage;
