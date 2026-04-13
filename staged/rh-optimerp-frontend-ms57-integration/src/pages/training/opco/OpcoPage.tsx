import React, { useEffect, useState, useCallback } from 'react';
import {
  Table, Button, Tag, Space, Typography, Card, Modal, Form, Input, InputNumber,
  message, Popconfirm, Alert, Row, Col, Switch,
} from 'antd';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import { opcoApi } from '../../../services/trainingApiService';
import { extractApiError } from '../../../services/utils/extractApiError';
import type { Opco } from '../../../types/training';

const { Title } = Typography;

const OpcoPage: React.FC = () => {
  const [opcos, setOpcos] = useState<Opco[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<Opco | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [form] = Form.useForm();

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await opcoApi.getAll(page, 20);
      setOpcos(result.Items);
      setTotalCount(result.TotalCount);
    } catch (err: unknown) {
      setError(extractApiError(err));
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => { load(); }, [load]);

  const openCreate = () => { setEditing(null); form.resetFields(); setModalOpen(true); };
  const openEdit = (record: Opco) => {
    setEditing(record);
    form.setFieldsValue(record);
    setModalOpen(true);
  };

  const handleSubmit = async (values: Record<string, unknown>) => {
    setSubmitting(true);
    try {
      if (editing) {
        await opcoApi.update(editing.Id, values as Partial<Opco>);
        message.success('OPCO mis a jour');
      } else {
        await opcoApi.create(values as Partial<Opco>);
        message.success('OPCO cree');
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
      await opcoApi.delete(id);
      message.success('OPCO supprime');
      load();
    } catch (err: unknown) {
      message.error(extractApiError(err));
    }
  };

  const columns = [
    { title: 'Code', dataIndex: 'Code', key: 'code', width: 100 },
    { title: 'Nom', dataIndex: 'Name', key: 'name', ellipsis: true },
    {
      title: 'Taux de contribution',
      dataIndex: 'ContributionRate',
      key: 'rate',
      render: (v: number) => `${(v * 100).toFixed(2)} %`,
    },
    { title: 'Email contact', dataIndex: 'ContactEmail', key: 'email', ellipsis: true },
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
      render: (_: unknown, record: Opco) => (
        <Space size="small">
          <Button type="link" size="small" onClick={() => openEdit(record)}>Modifier</Button>
          <Popconfirm title="Supprimer cet OPCO ?" onConfirm={() => handleDelete(record.Id)}>
            <Button type="link" size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <Space direction="vertical" size={16} style={{ width: '100%' }}>
      <Row justify="space-between" align="middle">
        <Col><Title level={4} style={{ margin: 0 }}>Gestion des OPCO</Title></Col>
        <Col>
          <Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>Nouvel OPCO</Button>
        </Col>
      </Row>

      {error && <Alert type="error" message={error} showIcon closable />}

      <Card>
        <Table
          dataSource={opcos}
          columns={columns}
          rowKey="Id"
          loading={loading}
          pagination={{
            current: page, total: totalCount, pageSize: 20,
            onChange: setPage, showTotal: (t) => `${t} OPCO`,
          }}
          locale={{ emptyText: 'Aucun OPCO enregistre.' }}
        />
      </Card>

      <Modal
        title={editing ? 'Modifier l\'OPCO' : 'Nouvel OPCO'}
        open={modalOpen}
        onCancel={() => setModalOpen(false)}
        footer={null}
        width={560}
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="Code" label="Code" rules={[{ required: true }]}>
                <Input placeholder="OPCO-EP" />
              </Form.Item>
            </Col>
            <Col span={16}>
              <Form.Item name="Name" label="Nom" rules={[{ required: true }]}>
                <Input placeholder="OPCO Entreprises de Proximite" />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="ContactEmail" label="Email contact">
                <Input placeholder="contact@opco.fr" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="ContributionRate" label="Taux de contribution" rules={[{ required: true }]}>
                <InputNumber style={{ width: '100%' }} min={0} max={1} step={0.001} placeholder="0.013" />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="BranchesProfessionnelles" label="Branches professionnelles">
            <Input.TextArea rows={2} placeholder="Batiment, Commerce, ..." />
          </Form.Item>
          <Form.Item name="IsActive" label="Actif" valuePropName="checked" initialValue={true}>
            <Switch />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={submitting} block>
              {editing ? 'Mettre a jour' : 'Creer l\'OPCO'}
            </Button>
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
};

export default OpcoPage;
