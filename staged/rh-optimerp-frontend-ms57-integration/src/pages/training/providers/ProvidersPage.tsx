import React, { useEffect, useState, useCallback } from 'react';
import {
  Table, Button, Tag, Space, Typography, Card, Modal, Form, Input, InputNumber,
  message, Popconfirm, Alert, Row, Col, Switch, DatePicker, Rate,
} from 'antd';
import { PlusOutlined, DeleteOutlined, SafetyCertificateOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { providerApi } from '../../../services/trainingApiService';
import { extractApiError } from '../../../services/utils/extractApiError';
import type { TrainingProvider } from '../../../types/training';

const { Title } = Typography;

const ProvidersPage: React.FC = () => {
  const [providers, setProviders] = useState<TrainingProvider[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<TrainingProvider | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [form] = Form.useForm();

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await providerApi.getAll(page, 20);
      setProviders(result.Items);
      setTotalCount(result.TotalCount);
    } catch (err: unknown) {
      setError(extractApiError(err));
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => { load(); }, [load]);

  const openCreate = () => { setEditing(null); form.resetFields(); setModalOpen(true); };
  const openEdit = (record: TrainingProvider) => {
    setEditing(record);
    form.setFieldsValue({
      ...record,
      QualiopiExpirationDate: record.QualiopiExpirationDate ? dayjs(record.QualiopiExpirationDate) : undefined,
    });
    setModalOpen(true);
  };

  const handleSubmit = async (values: Record<string, unknown>) => {
    setSubmitting(true);
    try {
      const dto = {
        ...values,
        QualiopiExpirationDate: (values.QualiopiExpirationDate as dayjs.Dayjs)?.toISOString(),
      };
      if (editing) {
        await providerApi.update(editing.Id, dto as Partial<TrainingProvider>);
        message.success('Organisme mis a jour');
      } else {
        await providerApi.create(dto as Partial<TrainingProvider>);
        message.success('Organisme cree');
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
      await providerApi.delete(id);
      message.success('Organisme supprime');
      load();
    } catch (err: unknown) {
      message.error(extractApiError(err));
    }
  };

  const columns = [
    { title: 'Nom', dataIndex: 'Name', key: 'name', ellipsis: true },
    { title: 'NDA', dataIndex: 'NdaNumber', key: 'nda', width: 130 },
    {
      title: 'Qualiopi',
      dataIndex: 'QualiopiCertified',
      key: 'qualiopi',
      render: (v: boolean) => v ? (
        <Tag color="success" icon={<SafetyCertificateOutlined />}>Certifie</Tag>
      ) : (
        <Tag color="default">Non certifie</Tag>
      ),
    },
    {
      title: 'Expiration Qualiopi',
      dataIndex: 'QualiopiExpirationDate',
      key: 'qexp',
      render: (d: string) => {
        if (!d) return '-';
        const isExpiring = dayjs(d).diff(dayjs(), 'month') < 3;
        return <Tag color={isExpiring ? 'warning' : 'default'}>{dayjs(d).format('DD/MM/YYYY')}</Tag>;
      },
    },
    {
      title: 'Note',
      dataIndex: 'Rating',
      key: 'rating',
      render: (r: number) => r ? <Rate disabled defaultValue={r} count={5} style={{ fontSize: 12 }} /> : '-',
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
      render: (_: unknown, record: TrainingProvider) => (
        <Space size="small">
          <Button type="link" size="small" onClick={() => openEdit(record)}>Modifier</Button>
          <Popconfirm title="Supprimer cet organisme ?" onConfirm={() => handleDelete(record.Id)}>
            <Button type="link" size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <Space direction="vertical" size={16} style={{ width: '100%' }}>
      <Row justify="space-between" align="middle">
        <Col><Title level={4} style={{ margin: 0 }}>Organismes de formation</Title></Col>
        <Col>
          <Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>Nouvel organisme</Button>
        </Col>
      </Row>

      {error && <Alert type="error" message={error} showIcon closable />}

      <Card>
        <Table
          dataSource={providers}
          columns={columns}
          rowKey="Id"
          loading={loading}
          pagination={{
            current: page, total: totalCount, pageSize: 20,
            onChange: setPage, showTotal: (t) => `${t} organismes`,
          }}
          locale={{ emptyText: 'Aucun organisme de formation.' }}
        />
      </Card>

      <Modal
        title={editing ? 'Modifier l\'organisme' : 'Nouvel organisme de formation'}
        open={modalOpen}
        onCancel={() => setModalOpen(false)}
        footer={null}
        width={580}
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <Form.Item name="Name" label="Nom de l'organisme" rules={[{ required: true }]}>
            <Input placeholder="CEGOS Formation" />
          </Form.Item>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="NdaNumber" label="Numero NDA">
                <Input placeholder="11 75 12345 67" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="Rating" label="Note (1-5)">
                <InputNumber style={{ width: '100%' }} min={1} max={5} step={0.5} />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="QualiopiCertified" label="Certifie Qualiopi" valuePropName="checked">
                <Switch />
              </Form.Item>
            </Col>
            <Col span={16}>
              <Form.Item name="QualiopiExpirationDate" label="Expiration Qualiopi">
                <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="IsActive" label="Actif" valuePropName="checked" initialValue={true}>
            <Switch />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={submitting} block>
              {editing ? 'Mettre a jour' : 'Creer l\'organisme'}
            </Button>
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
};

export default ProvidersPage;
