import React, { useEffect, useState, useCallback } from 'react';
import {
  Table, Button, Tag, Space, Typography, Card, Modal, Form, Input, InputNumber,
  Select, message, Popconfirm, Alert, Row, Col, Switch,
} from 'antd';
import { PlusOutlined, DeleteOutlined, PlayCircleOutlined } from '@ant-design/icons';
import { elearningApi } from '../../../services/trainingApiService';
import { extractApiError } from '../../../services/utils/extractApiError';
import type { ElearningCourse, ElearningFormat } from '../../../types/training';

const { Title } = Typography;

const FORMAT_OPTIONS: ElearningFormat[] = ['SCORM', 'xAPI', 'Video', 'Interactive', 'Assessment'];

const FORMAT_COLORS: Record<ElearningFormat, string> = {
  SCORM: 'blue',
  xAPI: 'purple',
  Video: 'cyan',
  Interactive: 'green',
  Assessment: 'orange',
};

const ElearningPage: React.FC = () => {
  const [courses, setCourses] = useState<ElearningCourse[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<ElearningCourse | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [form] = Form.useForm();

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await elearningApi.getAll(page, 20);
      setCourses(result.Items);
      setTotalCount(result.TotalCount);
    } catch (err: unknown) {
      setError(extractApiError(err));
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => { load(); }, [load]);

  const openCreate = () => { setEditing(null); form.resetFields(); setModalOpen(true); };
  const openEdit = (record: ElearningCourse) => {
    setEditing(record);
    form.setFieldsValue(record);
    setModalOpen(true);
  };

  const handleSubmit = async (values: Record<string, unknown>) => {
    setSubmitting(true);
    try {
      if (editing) {
        await elearningApi.update(editing.Id, values as Partial<ElearningCourse>);
        message.success('Cours mis a jour');
      } else {
        await elearningApi.create(values as Partial<ElearningCourse>);
        message.success('Cours cree');
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
      await elearningApi.delete(id);
      message.success('Cours supprime');
      load();
    } catch (err: unknown) {
      message.error(extractApiError(err));
    }
  };

  const columns = [
    { title: 'Titre', dataIndex: 'Title', key: 'title', ellipsis: true },
    {
      title: 'Format',
      dataIndex: 'Format',
      key: 'format',
      render: (f: ElearningFormat) => <Tag color={FORMAT_COLORS[f]}>{f}</Tag>,
    },
    {
      title: 'Duree (min)',
      dataIndex: 'DurationMinutes',
      key: 'duration',
      width: 110,
    },
    {
      title: 'Obligatoire',
      dataIndex: 'IsMandatory',
      key: 'mandatory',
      render: (v: boolean) => <Tag color={v ? 'error' : 'default'}>{v ? 'Oui' : 'Non'}</Tag>,
    },
    {
      title: 'Actif',
      dataIndex: 'IsActive',
      key: 'active',
      render: (v: boolean) => <Tag color={v ? 'success' : 'default'}>{v ? 'Actif' : 'Inactif'}</Tag>,
    },
    {
      title: 'URL',
      dataIndex: 'Url',
      key: 'url',
      ellipsis: true,
      render: (url: string) => url ? (
        <a href={url} target="_blank" rel="noopener noreferrer" style={{ maxWidth: 150, display: 'inline-block', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', verticalAlign: 'middle' }}>
          <PlayCircleOutlined /> Ouvrir
        </a>
      ) : '-',
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      render: (_: unknown, record: ElearningCourse) => (
        <Space size="small">
          <Button type="link" size="small" onClick={() => openEdit(record)}>Modifier</Button>
          <Popconfirm title="Supprimer ce cours ?" onConfirm={() => handleDelete(record.Id)}>
            <Button type="link" size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <Space direction="vertical" size={16} style={{ width: '100%' }}>
      <Row justify="space-between" align="middle">
        <Col><Title level={4} style={{ margin: 0 }}>Catalogue E-Learning</Title></Col>
        <Col>
          <Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>Nouveau cours</Button>
        </Col>
      </Row>

      {error && <Alert type="error" message={error} showIcon closable />}

      <Card>
        <Table
          dataSource={courses}
          columns={columns}
          rowKey="Id"
          loading={loading}
          pagination={{
            current: page, total: totalCount, pageSize: 20,
            onChange: setPage, showTotal: (t) => `${t} cours`,
          }}
          locale={{ emptyText: 'Aucun cours e-learning.' }}
        />
      </Card>

      <Modal
        title={editing ? 'Modifier le cours' : 'Nouveau cours e-learning'}
        open={modalOpen}
        onCancel={() => setModalOpen(false)}
        footer={null}
        width={600}
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <Form.Item name="Title" label="Titre" rules={[{ required: true }]}>
            <Input placeholder="Introduction au RGPD" />
          </Form.Item>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="Format" label="Format" rules={[{ required: true }]} initialValue="SCORM">
                <Select options={FORMAT_OPTIONS.map(v => ({ value: v, label: v }))} />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="DurationMinutes" label="Duree (minutes)" rules={[{ required: true }]}>
                <InputNumber style={{ width: '100%' }} min={1} />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="Url" label="URL du cours">
            <Input placeholder="https://lms.example.com/courses/001" />
          </Form.Item>
          <Form.Item name="Description" label="Description">
            <Input.TextArea rows={3} />
          </Form.Item>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="IsMandatory" label="Obligatoire" valuePropName="checked">
                <Switch />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="IsActive" label="Actif" valuePropName="checked" initialValue={true}>
                <Switch />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={submitting} block>
              {editing ? 'Mettre a jour' : 'Creer le cours'}
            </Button>
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
};

export default ElearningPage;
