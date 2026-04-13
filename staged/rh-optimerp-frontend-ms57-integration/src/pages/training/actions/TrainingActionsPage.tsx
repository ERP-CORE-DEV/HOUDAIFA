import React, { useEffect, useState, useCallback } from 'react';
import {
  Table, Button, Tag, Space, Typography, Card, Modal, Form, Input, InputNumber,
  Select, message, Popconfirm, Alert, Row, Col, Switch,
} from 'antd';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import { trainingActionApi } from '../../../services/trainingApiService';
import { extractApiError } from '../../../services/utils/extractApiError';
import type { TrainingAction, TrainingActionType, TrainingModality } from '../../../types/training';

const { Title } = Typography;

const ACTION_TYPE_OPTIONS: TrainingActionType[] = ['Obligatoire', 'Adaptation', 'Developpement'];
const MODALITY_OPTIONS: TrainingModality[] = ['Presentiel', 'Distanciel', 'ELearning', 'Blended', 'Afest'];

const MODALITY_COLORS: Record<TrainingModality, string> = {
  Presentiel: 'blue',
  Distanciel: 'cyan',
  ELearning: 'purple',
  Blended: 'geekblue',
  Afest: 'orange',
};

const TrainingActionsPage: React.FC = () => {
  const [actions, setActions] = useState<TrainingAction[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<TrainingAction | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [form] = Form.useForm();

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await trainingActionApi.getAll(page, 20);
      setActions(result.Items);
      setTotalCount(result.TotalCount);
    } catch (err: unknown) {
      setError(extractApiError(err));
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => { load(); }, [load]);

  const openCreate = () => { setEditing(null); form.resetFields(); setModalOpen(true); };
  const openEdit = (record: TrainingAction) => {
    setEditing(record);
    form.setFieldsValue(record);
    setModalOpen(true);
  };

  const handleSubmit = async (values: Record<string, unknown>) => {
    setSubmitting(true);
    try {
      if (editing) {
        await trainingActionApi.update(editing.Id, values as Partial<TrainingAction>);
        message.success('Action mise a jour');
      } else {
        await trainingActionApi.create(values as Partial<TrainingAction>);
        message.success('Action creee');
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
      await trainingActionApi.delete(id);
      message.success('Action supprimee');
      load();
    } catch (err: unknown) {
      message.error(extractApiError(err));
    }
  };

  const columns = [
    { title: 'Titre', dataIndex: 'Title', key: 'title', ellipsis: true },
    {
      title: 'Type',
      dataIndex: 'Type',
      key: 'type',
      render: (t: TrainingActionType) => <Tag color={t === 'Obligatoire' ? 'error' : 'default'}>{t}</Tag>,
    },
    {
      title: 'Modalite',
      dataIndex: 'Modality',
      key: 'modality',
      render: (m: TrainingModality) => <Tag color={MODALITY_COLORS[m]}>{m}</Tag>,
    },
    { title: 'Duree (h)', dataIndex: 'DurationHours', key: 'duration', width: 90 },
    {
      title: 'Cout (EUR)',
      dataIndex: 'Cost',
      key: 'cost',
      render: (v: number) => v.toLocaleString('fr-FR'),
    },
    {
      title: 'Obligatoire',
      dataIndex: 'IsObligatory',
      key: 'obligatory',
      render: (v: boolean) => <Tag color={v ? 'error' : 'default'}>{v ? 'Oui' : 'Non'}</Tag>,
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 130,
      render: (_: unknown, record: TrainingAction) => (
        <Space size="small">
          <Button type="link" size="small" onClick={() => openEdit(record)}>Modifier</Button>
          <Popconfirm title="Supprimer cette action ?" onConfirm={() => handleDelete(record.Id)}>
            <Button type="link" size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <Space direction="vertical" size={16} style={{ width: '100%' }}>
      <Row justify="space-between" align="middle">
        <Col><Title level={4} style={{ margin: 0 }}>Actions de formation</Title></Col>
        <Col>
          <Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>Nouvelle action</Button>
        </Col>
      </Row>

      {error && <Alert type="error" message={error} showIcon closable />}

      <Card>
        <Table
          dataSource={actions}
          columns={columns}
          rowKey="Id"
          loading={loading}
          pagination={{
            current: page, total: totalCount, pageSize: 20,
            onChange: setPage, showTotal: (t) => `${t} actions`,
          }}
          locale={{ emptyText: 'Aucune action de formation.' }}
        />
      </Card>

      <Modal
        title={editing ? 'Modifier l\'action' : 'Nouvelle action de formation'}
        open={modalOpen}
        onCancel={() => setModalOpen(false)}
        footer={null}
        width={620}
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <Form.Item name="Title" label="Titre" rules={[{ required: true }]}>
            <Input placeholder="Formation RGPD" />
          </Form.Item>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="Type" label="Type" rules={[{ required: true }]} initialValue="Adaptation">
                <Select options={ACTION_TYPE_OPTIONS.map(v => ({ value: v, label: v }))} />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="Modality" label="Modalite" rules={[{ required: true }]} initialValue="Presentiel">
                <Select options={MODALITY_OPTIONS.map(v => ({ value: v, label: v }))} />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="DurationHours" label="Duree (heures)" rules={[{ required: true }]}>
                <InputNumber style={{ width: '100%' }} min={0.5} step={0.5} />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="Cost" label="Cout (EUR)">
                <InputNumber style={{ width: '100%' }} min={0} />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="MaxParticipants" label="Participants max">
                <InputNumber style={{ width: '100%' }} min={1} />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="IsObligatory" label="Obligatoire" valuePropName="checked">
                <Switch />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="Description" label="Description">
            <Input.TextArea rows={3} />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={submitting} block>
              {editing ? 'Mettre a jour' : 'Creer l\'action'}
            </Button>
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
};

export default TrainingActionsPage;
