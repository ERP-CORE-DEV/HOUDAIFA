import React, { useEffect, useState, useCallback } from 'react';
import {
  Table, Button, Tag, Space, Typography, Card, Modal, Form, Input,
  Select, message, Alert, Row, Col, DatePicker,
} from 'antd';
import { PlusOutlined, WarningOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { interviewApi } from '../../../services/trainingApiService';
import { extractApiError } from '../../../services/utils/extractApiError';
import type { ProfessionalInterview, InterviewType } from '../../../types/training';

const { Title } = Typography;

const INTERVIEW_TYPE_OPTIONS: InterviewType[] = ['Biennial', 'SixYearReview', 'PostAbsence', 'Voluntary'];

const EntretiensPage: React.FC = () => {
  const [interviews, setInterviews] = useState<ProfessionalInterview[]>([]);
  const [overdueCount, setOverdueCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<ProfessionalInterview | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [employeeId, setEmployeeId] = useState('');
  const [form] = Form.useForm();
  const [searchForm] = Form.useForm();

  const loadOverdue = useCallback(async () => {
    try {
      const results = await interviewApi.getOverdue();
      setOverdueCount(results.length);
    } catch {
      // non-blocking
    }
  }, []);

  useEffect(() => { loadOverdue(); }, [loadOverdue]);

  const handleSearch = async (values: Record<string, unknown>) => {
    setLoading(true);
    setError(null);
    try {
      const results = await interviewApi.getByEmployee(values.employeeId as string);
      setInterviews(results);
      setEmployeeId(values.employeeId as string);
    } catch (err: unknown) {
      setError(extractApiError(err));
      setInterviews([]);
    } finally {
      setLoading(false);
    }
  };

  const openCreate = () => { setEditing(null); form.resetFields(); setModalOpen(true); };
  const openEdit = (record: ProfessionalInterview) => {
    setEditing(record);
    form.setFieldsValue({
      ...record,
      ScheduledDate: record.ScheduledDate ? dayjs(record.ScheduledDate) : undefined,
    });
    setModalOpen(true);
  };

  const handleSubmit = async (values: Record<string, unknown>) => {
    setSubmitting(true);
    try {
      const dto = {
        ...values,
        ScheduledDate: (values.ScheduledDate as dayjs.Dayjs)?.toISOString(),
      };
      if (editing) {
        await interviewApi.update(editing.Id, dto as Partial<ProfessionalInterview>);
        message.success('Entretien mis a jour');
      } else {
        await interviewApi.create(dto as Partial<ProfessionalInterview>);
        message.success('Entretien cree');
      }
      setModalOpen(false);
      if (employeeId) {
        const results = await interviewApi.getByEmployee(employeeId);
        setInterviews(results);
      }
    } catch (err: unknown) {
      message.error(extractApiError(err));
    } finally {
      setSubmitting(false);
    }
  };

  const columns = [
    { title: 'ID Employe', dataIndex: 'EmployeeId', key: 'employeeId', ellipsis: true },
    {
      title: 'Type',
      dataIndex: 'Type',
      key: 'type',
      render: (t: InterviewType) => <Tag color="blue">{t}</Tag>,
    },
    {
      title: 'Date prevue',
      dataIndex: 'ScheduledDate',
      key: 'scheduled',
      render: (d: string) => d ? dayjs(d).format('DD/MM/YYYY') : '-',
    },
    {
      title: 'Date realisee',
      dataIndex: 'ConductedDate',
      key: 'conducted',
      render: (d: string) => d ? dayjs(d).format('DD/MM/YYYY') : <Tag color="warning">Non realise</Tag>,
    },
    { title: 'Statut', dataIndex: 'Status', key: 'status' },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: unknown, record: ProfessionalInterview) => (
        <Button type="link" size="small" onClick={() => openEdit(record)}>Modifier</Button>
      ),
    },
  ];

  return (
    <Space direction="vertical" size={16} style={{ width: '100%' }}>
      <Row justify="space-between" align="middle">
        <Col>
          <Space>
            <Title level={4} style={{ margin: 0 }}>Entretiens professionnels</Title>
            {overdueCount > 0 && (
              <Tag color="error" icon={<WarningOutlined />}>{overdueCount} en retard</Tag>
            )}
          </Space>
        </Col>
        <Col>
          <Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>Nouvel entretien</Button>
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
          dataSource={interviews}
          columns={columns}
          rowKey="Id"
          loading={loading}
          pagination={{ pageSize: 20 }}
          locale={{ emptyText: 'Recherchez par ID employe.' }}
        />
      </Card>

      <Modal
        title={editing ? 'Modifier l\'entretien' : 'Nouvel entretien professionnel'}
        open={modalOpen}
        onCancel={() => setModalOpen(false)}
        footer={null}
        width={560}
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="EmployeeId" label="ID Employe" rules={[{ required: true }]}>
                <Input placeholder="employee-001" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="ManagerId" label="ID Manager" rules={[{ required: true }]}>
                <Input placeholder="manager-001" />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="Type" label="Type" rules={[{ required: true }]} initialValue="Biennial">
                <Select options={INTERVIEW_TYPE_OPTIONS.map(v => ({ value: v, label: v }))} />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="ScheduledDate" label="Date prevue" rules={[{ required: true }]}>
                <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={submitting} block>
              {editing ? 'Mettre a jour' : 'Creer l\'entretien'}
            </Button>
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
};

export default EntretiensPage;
