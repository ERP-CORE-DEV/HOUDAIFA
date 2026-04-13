import React, { useState } from 'react';
import {
  Table, Button, Tag, Space, Typography, Card, Modal, Form, Input,
  Select, message, Alert, Row, Col, DatePicker,
} from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { bilanApi } from '../../../services/trainingApiService';
import { extractApiError } from '../../../services/utils/extractApiError';
import type { BilanDeCompetences, BilanStatus } from '../../../types/training';

const { Title } = Typography;

const STATUS_OPTIONS: BilanStatus[] = ['Requested', 'InProgress', 'Completed', 'Cancelled'];

const STATUS_COLORS: Record<BilanStatus, string> = {
  Requested: 'processing',
  InProgress: 'blue',
  Completed: 'success',
  Cancelled: 'error',
};

const BilanPage: React.FC = () => {
  const [bilans, setBilans] = useState<BilanDeCompetences[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<BilanDeCompetences | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [form] = Form.useForm();
  const [searchForm] = Form.useForm();

  const handleSearch = async (values: Record<string, unknown>) => {
    setLoading(true);
    setError(null);
    try {
      const results = await bilanApi.getByEmployee(values.employeeId as string);
      setBilans(results);
    } catch (err: unknown) {
      setError(extractApiError(err));
      setBilans([]);
    } finally {
      setLoading(false);
    }
  };

  const openEdit = (record: BilanDeCompetences) => {
    setEditing(record);
    form.setFieldsValue({
      ...record,
      StartDate: record.StartDate ? dayjs(record.StartDate) : undefined,
      EndDate: record.EndDate ? dayjs(record.EndDate) : undefined,
    });
    setModalOpen(true);
  };

  const handleSubmit = async (values: Record<string, unknown>) => {
    setSubmitting(true);
    try {
      const dto = {
        ...values,
        StartDate: (values.StartDate as dayjs.Dayjs)?.toISOString(),
        EndDate: (values.EndDate as dayjs.Dayjs)?.toISOString(),
      };
      if (editing) {
        await bilanApi.update(editing.Id, dto as Partial<BilanDeCompetences>);
        message.success('Bilan mis a jour');
      } else {
        await bilanApi.create(dto as Partial<BilanDeCompetences>);
        message.success('Bilan cree');
      }
      setModalOpen(false);
      form.resetFields();
      if (bilans.length > 0) {
        handleSearch({ employeeId: bilans[0].EmployeeId });
      }
    } catch (err: unknown) {
      message.error(extractApiError(err));
    } finally {
      setSubmitting(false);
    }
  };

  const columns = [
    { title: 'ID Employe', dataIndex: 'EmployeeId', key: 'employee', ellipsis: true },
    {
      title: 'Statut',
      dataIndex: 'Status',
      key: 'status',
      render: (s: BilanStatus) => <Tag color={STATUS_COLORS[s]}>{s}</Tag>,
    },
    {
      title: 'Debut',
      dataIndex: 'StartDate',
      key: 'start',
      render: (d: string) => d ? dayjs(d).format('DD/MM/YYYY') : '-',
    },
    {
      title: 'Fin',
      dataIndex: 'EndDate',
      key: 'end',
      render: (d: string) => d ? dayjs(d).format('DD/MM/YYYY') : '-',
    },
    { title: 'Prestataire', dataIndex: 'ProviderName', key: 'provider', ellipsis: true },
    { title: 'Financement', dataIndex: 'FundingSource', key: 'funding' },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: unknown, record: BilanDeCompetences) => (
        <Button type="link" size="small" onClick={() => openEdit(record)}>Modifier</Button>
      ),
    },
  ];

  return (
    <Space direction="vertical" size={16} style={{ width: '100%' }}>
      <Row justify="space-between" align="middle">
        <Col><Title level={4} style={{ margin: 0 }}>Bilan de competences</Title></Col>
        <Col>
          <Button type="primary" icon={<PlusOutlined />} onClick={() => { setEditing(null); form.resetFields(); setModalOpen(true); }}>
            Nouveau bilan
          </Button>
        </Col>
      </Row>

      <Card>
        <Form form={searchForm} layout="inline" onFinish={handleSearch} style={{ marginBottom: 16 }}>
          <Form.Item name="employeeId" label="ID Employe" rules={[{ required: true }]}>
            <Input placeholder="employee-001" style={{ width: 220 }} />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={loading}>Rechercher</Button>
          </Form.Item>
        </Form>
        {error && <Alert type="error" message={error} showIcon closable style={{ marginBottom: 8 }} />}
        <Table
          dataSource={bilans}
          columns={columns}
          rowKey="Id"
          loading={loading}
          pagination={{ pageSize: 20 }}
          locale={{ emptyText: 'Recherchez par ID employe.' }}
        />
      </Card>

      <Modal
        title={editing ? 'Modifier le bilan' : 'Nouveau bilan de competences'}
        open={modalOpen}
        onCancel={() => setModalOpen(false)}
        footer={null}
        width={560}
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <Form.Item name="EmployeeId" label="ID Employe" rules={[{ required: true }]}>
            <Input placeholder="employee-001" />
          </Form.Item>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="Status" label="Statut" rules={[{ required: true }]} initialValue="Requested">
                <Select options={STATUS_OPTIONS.map(v => ({ value: v, label: v }))} />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="StartDate" label="Date de debut" rules={[{ required: true }]}>
                <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="ProviderName" label="Prestataire">
                <Input placeholder="Cabinet conseil" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="FundingSource" label="Source financement">
                <Input placeholder="CPF, OPCO, ..." />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="Comments" label="Commentaires">
            <Input.TextArea rows={3} />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={submitting} block>
              {editing ? 'Mettre a jour' : 'Creer le bilan'}
            </Button>
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
};

export default BilanPage;
