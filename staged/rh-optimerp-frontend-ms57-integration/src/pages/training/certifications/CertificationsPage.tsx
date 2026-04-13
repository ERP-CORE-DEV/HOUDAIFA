import React, { useEffect, useState, useCallback } from 'react';
import {
  Table, Button, Tag, Space, Typography, Card, Modal, Form, Input,
  message, Popconfirm, Alert, Row, Col, DatePicker, Switch, Tabs,
} from 'antd';
import { PlusOutlined, DeleteOutlined, WarningOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { certificationApi, vaeApi } from '../../../services/trainingApiService';
import { extractApiError } from '../../../services/utils/extractApiError';
import type { CertificationRncp, VaeProject, VaeStatus } from '../../../types/training';

const { Title } = Typography;

const VAE_STATUS_COLORS: Record<VaeStatus, string> = {
  Recevabilite: 'processing',
  Accompagnement: 'blue',
  Livret2: 'cyan',
  Jury: 'orange',
  ValidationTotale: 'success',
  ValidationPartielle: 'gold',
  Refus: 'error',
};

const CertificationsPage: React.FC = () => {
  const [certifications, setCertifications] = useState<CertificationRncp[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<CertificationRncp | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [form] = Form.useForm();

  const [vaeProjects, setVaeProjects] = useState<VaeProject[]>([]);
  const [vaeModalOpen, setVaeModalOpen] = useState(false);
  const [vaeForm] = Form.useForm();

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await certificationApi.getAll(page, 20);
      setCertifications(result.Items);
      setTotalCount(result.TotalCount);
    } catch (err: unknown) {
      setError(extractApiError(err));
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => { load(); }, [load]);

  const handleSubmit = async (values: Record<string, unknown>) => {
    setSubmitting(true);
    try {
      const dto = {
        ...values,
        DateEnregistrement: (values.DateEnregistrement as dayjs.Dayjs)?.toISOString(),
        DateEcheance: (values.DateEcheance as dayjs.Dayjs)?.toISOString(),
      };
      if (editing) {
        await certificationApi.update(editing.Id, dto as Partial<CertificationRncp>);
        message.success('Certification mise a jour');
      } else {
        await certificationApi.create(dto as Partial<CertificationRncp>);
        message.success('Certification creee');
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

  const handleDelete = async (id: string) => {
    try {
      await certificationApi.delete(id);
      message.success('Certification supprimee');
      load();
    } catch (err: unknown) {
      message.error(extractApiError(err));
    }
  };

  const handleVaeSearch = async (employeeId: string) => {
    try {
      const results = await vaeApi.getByEmployee(employeeId);
      setVaeProjects(results);
    } catch (err: unknown) {
      message.error(extractApiError(err));
    }
  };

  const handleCreateVae = async (values: Record<string, unknown>) => {
    setSubmitting(true);
    try {
      const dto = {
        ...values,
        StartDate: (values.StartDate as dayjs.Dayjs)?.toISOString(),
      };
      await vaeApi.create(dto as Partial<VaeProject>);
      message.success('Projet VAE cree');
      setVaeModalOpen(false);
      vaeForm.resetFields();
    } catch (err: unknown) {
      message.error(extractApiError(err));
    } finally {
      setSubmitting(false);
    }
  };

  const certColumns = [
    { title: 'Code RNCP', dataIndex: 'RncpCode', key: 'code', width: 110 },
    { title: 'Titre', dataIndex: 'Titre', key: 'titre', ellipsis: true },
    { title: 'Organisme', dataIndex: 'Organisme', key: 'organisme' },
    { title: 'Niveau', dataIndex: 'NiveauQualification', key: 'niveau' },
    {
      title: 'Echeance',
      dataIndex: 'DateEcheance',
      key: 'echeance',
      render: (d: string) => {
        if (!d) return '-';
        const isExpiring = dayjs(d).diff(dayjs(), 'month') < 3;
        return <Tag color={isExpiring ? 'warning' : 'default'}>{dayjs(d).format('DD/MM/YYYY')}</Tag>;
      },
    },
    {
      title: 'Actif',
      dataIndex: 'IsActive',
      key: 'active',
      render: (v: boolean) => <Tag color={v ? 'success' : 'default'}>{v ? 'Actif' : 'Inactif'}</Tag>,
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: unknown, record: CertificationRncp) => (
        <Space size="small">
          <Button type="link" size="small" onClick={() => { setEditing(record); form.setFieldsValue({ ...record, DateEnregistrement: dayjs(record.DateEnregistrement), DateEcheance: record.DateEcheance ? dayjs(record.DateEcheance) : undefined }); setModalOpen(true); }}>
            Modifier
          </Button>
          <Popconfirm title="Supprimer ?" onConfirm={() => handleDelete(record.Id)}>
            <Button type="link" size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  const vaeColumns = [
    { title: 'ID Employe', dataIndex: 'EmployeeId', key: 'emp', ellipsis: true },
    { title: 'Certification', dataIndex: 'CertificationTitre', key: 'cert', ellipsis: true },
    {
      title: 'Statut',
      dataIndex: 'Status',
      key: 'status',
      render: (s: VaeStatus) => <Tag color={VAE_STATUS_COLORS[s]}>{s}</Tag>,
    },
    { title: 'Phase', dataIndex: 'Phase', key: 'phase' },
    {
      title: 'Debut',
      dataIndex: 'StartDate',
      key: 'start',
      render: (d: string) => dayjs(d).format('DD/MM/YYYY'),
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: unknown, record: VaeProject) => (
        <Button type="link" size="small" onClick={async () => { await vaeApi.advancePhase(record.Id); message.success('Phase avancee'); handleVaeSearch(record.EmployeeId); }}>
          Avancer phase
        </Button>
      ),
    },
  ];

  return (
    <Space direction="vertical" size={16} style={{ width: '100%' }}>
      <Row justify="space-between" align="middle">
        <Col><Title level={4} style={{ margin: 0 }}>Certifications / VAE</Title></Col>
        <Col>
          <Space>
            <Button onClick={() => setVaeModalOpen(true)}>Nouveau projet VAE</Button>
            <Button type="primary" icon={<PlusOutlined />} onClick={() => { setEditing(null); form.resetFields(); setModalOpen(true); }}>
              Nouvelle certification
            </Button>
          </Space>
        </Col>
      </Row>

      {error && <Alert type="error" message={error} showIcon closable />}

      <Tabs
        items={[
          {
            key: 'certifications',
            label: 'Certifications RNCP/RS',
            children: (
              <Card>
                <Table
                  dataSource={certifications}
                  columns={certColumns}
                  rowKey="Id"
                  loading={loading}
                  pagination={{
                    current: page, total: totalCount, pageSize: 20,
                    onChange: setPage, showTotal: (t) => `${t} certifications`,
                  }}
                  locale={{ emptyText: 'Aucune certification enregistree.' }}
                />
              </Card>
            ),
          },
          {
            key: 'vae',
            label: 'Projets VAE',
            children: (
              <Card>
                <Form layout="inline" onFinish={(v) => handleVaeSearch(v.employeeId)} style={{ marginBottom: 16 }}>
                  <Form.Item name="employeeId" label="ID Employe" rules={[{ required: true }]}>
                    <Input placeholder="employee-001" style={{ width: 220 }} />
                  </Form.Item>
                  <Form.Item>
                    <Button type="primary" htmlType="submit">Rechercher</Button>
                  </Form.Item>
                </Form>
                <Table
                  dataSource={vaeProjects}
                  columns={vaeColumns}
                  rowKey="Id"
                  pagination={{ pageSize: 20 }}
                  locale={{ emptyText: 'Recherchez par ID employe.' }}
                />
              </Card>
            ),
          },
        ]}
      />

      <Modal
        title={editing ? 'Modifier la certification' : 'Nouvelle certification'}
        open={modalOpen}
        onCancel={() => setModalOpen(false)}
        footer={null}
        width={600}
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="RncpCode" label="Code RNCP" rules={[{ required: true }]}>
                <Input placeholder="RNCP37822" />
              </Form.Item>
            </Col>
            <Col span={16}>
              <Form.Item name="Titre" label="Titre" rules={[{ required: true }]}>
                <Input placeholder="Manager de projet" />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="Organisme" label="Organisme" rules={[{ required: true }]}>
                <Input placeholder="France Competences" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="NiveauQualification" label="Niveau" rules={[{ required: true }]}>
                <Input placeholder="Niveau 6 (Bac+3/4)" />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="DateEnregistrement" label="Date d'enregistrement" rules={[{ required: true }]}>
                <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="DateEcheance" label="Date d'echeance">
                <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="IsActive" label="Active" valuePropName="checked" initialValue={true}>
            <Switch />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={submitting} block>
              {editing ? 'Mettre a jour' : 'Creer'}
            </Button>
          </Form.Item>
        </Form>
      </Modal>

      <Modal title="Nouveau projet VAE" open={vaeModalOpen} onCancel={() => setVaeModalOpen(false)} footer={null}>
        <Form form={vaeForm} layout="vertical" onFinish={handleCreateVae}>
          <Form.Item name="EmployeeId" label="ID Employe" rules={[{ required: true }]}>
            <Input placeholder="employee-001" />
          </Form.Item>
          <Form.Item name="CertificationId" label="ID Certification" rules={[{ required: true }]}>
            <Input placeholder="cert-001" />
          </Form.Item>
          <Form.Item name="StartDate" label="Date de debut" rules={[{ required: true }]}>
            <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
          </Form.Item>
          <Form.Item name="Phase" label="Phase initiale" initialValue="Recevabilite">
            <Input />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={submitting} block>Creer le projet VAE</Button>
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
};

export default CertificationsPage;
