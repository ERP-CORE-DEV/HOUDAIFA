import React, { useState } from 'react';
import {
  Table, Button, Tag, Space, Typography, Card, Modal, Form, Input, InputNumber,
  message, Popconfirm, Alert, Row, Col, DatePicker, Select,
} from 'antd';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { alternanceApi } from '../../../services/trainingApiService';
import { extractApiError } from '../../../services/utils/extractApiError';

const { Title } = Typography;

const CONTRACT_TYPE_OPTIONS = [
  { value: 'Apprentissage', label: 'Contrat d\'apprentissage' },
  { value: 'Professionnalisation', label: 'Contrat de professionnalisation' },
];

interface AlternanceContract {
  Id: string;
  EmployeeId: string;
  ContractType: string;
  StartDate: string;
  EndDate?: string;
  MentorId?: string;
  TrainingOrganismName?: string;
  WeeklyHoursAtWork: number;
  WeeklyHoursAtSchool: number;
  Remuneration?: number;
}

const AlternancePage: React.FC = () => {
  const [contracts, setContracts] = useState<AlternanceContract[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<AlternanceContract | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [form] = Form.useForm();
  const [searchForm] = Form.useForm();

  const handleSearch = async (values: Record<string, unknown>) => {
    setLoading(true);
    setError(null);
    try {
      const result = await alternanceApi.getByEmployee(values.employeeId as string);
      setContracts(Array.isArray(result) ? result : [result]);
    } catch (err: unknown) {
      setError(extractApiError(err));
      setContracts([]);
    } finally {
      setLoading(false);
    }
  };

  const openEdit = (record: AlternanceContract) => {
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
        await alternanceApi.update(editing.Id, dto);
        message.success('Contrat mis a jour');
      } else {
        await alternanceApi.create(dto);
        message.success('Contrat cree');
      }
      setModalOpen(false);
      form.resetFields();
    } catch (err: unknown) {
      message.error(extractApiError(err));
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await alternanceApi.delete(id);
      message.success('Contrat supprime');
      setContracts(prev => prev.filter(c => c.Id !== id));
    } catch (err: unknown) {
      message.error(extractApiError(err));
    }
  };

  const columns = [
    { title: 'ID Employe', dataIndex: 'EmployeeId', key: 'employee', ellipsis: true },
    {
      title: 'Type de contrat',
      dataIndex: 'ContractType',
      key: 'type',
      render: (t: string) => <Tag color="blue">{t}</Tag>,
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
    { title: 'Organisme', dataIndex: 'TrainingOrganismName', key: 'organism', ellipsis: true },
    {
      title: 'Remuneration (EUR)',
      dataIndex: 'Remuneration',
      key: 'remuneration',
      render: (v: number) => v ? `${v.toLocaleString('fr-FR')} EUR` : '-',
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      render: (_: unknown, record: AlternanceContract) => (
        <Space size="small">
          <Button type="link" size="small" onClick={() => openEdit(record)}>Modifier</Button>
          <Popconfirm title="Supprimer ce contrat ?" onConfirm={() => handleDelete(record.Id)}>
            <Button type="link" size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <Space direction="vertical" size={16} style={{ width: '100%' }}>
      <Row justify="space-between" align="middle">
        <Col><Title level={4} style={{ margin: 0 }}>Contrats d\'alternance</Title></Col>
        <Col>
          <Button type="primary" icon={<PlusOutlined />} onClick={() => { setEditing(null); form.resetFields(); setModalOpen(true); }}>
            Nouveau contrat
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
          dataSource={contracts}
          columns={columns}
          rowKey="Id"
          loading={loading}
          pagination={{ pageSize: 20 }}
          locale={{ emptyText: 'Recherchez par ID employe.' }}
        />
      </Card>

      <Modal
        title={editing ? 'Modifier le contrat' : 'Nouveau contrat d\'alternance'}
        open={modalOpen}
        onCancel={() => setModalOpen(false)}
        footer={null}
        width={580}
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="EmployeeId" label="ID Employe" rules={[{ required: true }]}>
                <Input placeholder="employee-001" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="MentorId" label="ID Tuteur">
                <Input placeholder="employee-002" />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="ContractType" label="Type de contrat" rules={[{ required: true }]} initialValue="Apprentissage">
            <Select options={CONTRACT_TYPE_OPTIONS} />
          </Form.Item>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="StartDate" label="Date de debut" rules={[{ required: true }]}>
                <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="EndDate" label="Date de fin">
                <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="TrainingOrganismName" label="Organisme de formation">
            <Input placeholder="CFA Metiers" />
          </Form.Item>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="WeeklyHoursAtWork" label="H/sem entreprise">
                <InputNumber style={{ width: '100%' }} min={0} max={35} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="WeeklyHoursAtSchool" label="H/sem CFA">
                <InputNumber style={{ width: '100%' }} min={0} max={35} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="Remuneration" label="Remuneration (EUR)">
                <InputNumber style={{ width: '100%' }} min={0} />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={submitting} block>
              {editing ? 'Mettre a jour' : 'Creer le contrat'}
            </Button>
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
};

export default AlternancePage;
