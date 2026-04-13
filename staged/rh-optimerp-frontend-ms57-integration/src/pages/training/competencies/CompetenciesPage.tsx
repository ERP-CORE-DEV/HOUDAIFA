import React, { useEffect, useState, useCallback } from 'react';
import {
  Table, Button, Tag, Space, Typography, Card, Modal, Form, Input,
  Select, message, Popconfirm, Alert, Row, Col, Switch,
} from 'antd';
import { PlusOutlined, DeleteOutlined, SearchOutlined } from '@ant-design/icons';
import { competencyApi } from '../../../services/trainingApiService';
import { extractApiError } from '../../../services/utils/extractApiError';
import type { Competency } from '../../../types/training';

const { Title } = Typography;

const CompetenciesPage: React.FC = () => {
  const [competencies, setCompetencies] = useState<Competency[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<Competency | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [form] = Form.useForm();

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      if (searchQuery.trim()) {
        const results = await competencyApi.search(searchQuery);
        setCompetencies(results);
        setTotalCount(results.length);
      } else {
        const result = await competencyApi.getAll(page, 20);
        setCompetencies(result.Items);
        setTotalCount(result.TotalCount);
      }
    } catch (err: unknown) {
      setError(extractApiError(err));
    } finally {
      setLoading(false);
    }
  }, [page, searchQuery]);

  useEffect(() => { load(); }, [load]);

  const openCreate = () => { setEditing(null); form.resetFields(); setModalOpen(true); };
  const openEdit = (record: Competency) => {
    setEditing(record);
    form.setFieldsValue(record);
    setModalOpen(true);
  };

  const handleSubmit = async (values: Record<string, unknown>) => {
    setSubmitting(true);
    try {
      if (editing) {
        await competencyApi.update(editing.Id, values as Partial<Competency>);
        message.success('Competence mise a jour');
      } else {
        await competencyApi.create(values as Partial<Competency>);
        message.success('Competence creee');
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
      await competencyApi.delete(id);
      message.success('Competence supprimee');
      load();
    } catch (err: unknown) {
      message.error(extractApiError(err));
    }
  };

  const columns = [
    { title: 'Code', dataIndex: 'Code', key: 'code', width: 100 },
    { title: 'Nom', dataIndex: 'Name', key: 'name', ellipsis: true },
    { title: 'Domaine', dataIndex: 'Domain', key: 'domain' },
    { title: 'Famille', dataIndex: 'Family', key: 'family' },
    { title: 'Type', dataIndex: 'Type', key: 'type' },
    {
      title: 'Critique',
      dataIndex: 'IsCritical',
      key: 'critical',
      render: (v: boolean) => v ? <Tag color="error">Critique</Tag> : <Tag>Standard</Tag>,
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
      width: 120,
      render: (_: unknown, record: Competency) => (
        <Space size="small">
          <Button type="link" size="small" onClick={() => openEdit(record)}>Modifier</Button>
          <Popconfirm title="Supprimer cette competence ?" onConfirm={() => handleDelete(record.Id)}>
            <Button type="link" size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <Space direction="vertical" size={16} style={{ width: '100%' }}>
      <Row justify="space-between" align="middle">
        <Col><Title level={4} style={{ margin: 0 }}>Referentiel de competences</Title></Col>
        <Col>
          <Space>
            <Input.Search
              placeholder="Rechercher..."
              allowClear
              style={{ width: 220 }}
              onSearch={(v) => { setSearchQuery(v); setPage(1); }}
              prefix={<SearchOutlined />}
            />
            <Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>Nouvelle competence</Button>
          </Space>
        </Col>
      </Row>

      {error && <Alert type="error" message={error} showIcon closable />}

      <Card>
        <Table
          dataSource={competencies}
          columns={columns}
          rowKey="Id"
          loading={loading}
          pagination={{
            current: page, total: totalCount, pageSize: 20,
            onChange: setPage, showTotal: (t) => `${t} competences`,
          }}
          locale={{ emptyText: 'Aucune competence dans le referentiel.' }}
        />
      </Card>

      <Modal
        title={editing ? 'Modifier la competence' : 'Nouvelle competence'}
        open={modalOpen}
        onCancel={() => setModalOpen(false)}
        footer={null}
        width={600}
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="Code" label="Code" rules={[{ required: true }]}>
                <Input placeholder="COMP-001" />
              </Form.Item>
            </Col>
            <Col span={16}>
              <Form.Item name="Name" label="Nom" rules={[{ required: true }]}>
                <Input placeholder="Gestion de projet" />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="Domain" label="Domaine" rules={[{ required: true }]}>
                <Input placeholder="Management" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="Family" label="Famille" rules={[{ required: true }]}>
                <Input placeholder="Competences transversales" />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="Type" label="Type" rules={[{ required: true }]}>
                <Input placeholder="Savoir-faire" />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item name="IsCritical" label="Critique" valuePropName="checked">
                <Switch />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item name="IsActive" label="Actif" valuePropName="checked" initialValue={true}>
                <Switch />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="Description" label="Description">
            <Input.TextArea rows={3} />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={submitting} block>
              {editing ? 'Mettre a jour' : 'Creer la competence'}
            </Button>
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
};

export default CompetenciesPage;
