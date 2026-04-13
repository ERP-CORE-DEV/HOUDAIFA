import React, { useEffect, useState, useCallback } from 'react';
import {
  Table, Button, Space, Typography, Card, Modal, Form, Input, InputNumber,
  message, Alert, Row, Col, Tag, Popconfirm,
} from 'antd';
import { PlusOutlined, EuroCircleOutlined } from '@ant-design/icons';
import { cpfApi } from '../../../services/trainingApiService';
import { extractApiError } from '../../../services/utils/extractApiError';
import type { CpfAccount } from '../../../types/training';

const { Title } = Typography;

const CpfPage: React.FC = () => {
  const [accounts, setAccounts] = useState<CpfAccount[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [mobilizeOpen, setMobilizeOpen] = useState(false);
  const [editing, setEditing] = useState<CpfAccount | null>(null);
  const [selected, setSelected] = useState<CpfAccount | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [form] = Form.useForm();
  const [mobilizeForm] = Form.useForm();

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      // No paginated list endpoint — we load by employee in production.
      // For admin view, we show a sample search by employee ID.
      setAccounts([]);
    } catch (err: unknown) {
      setError(extractApiError(err));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  const handleSearchEmployee = async (values: Record<string, unknown>) => {
    setLoading(true);
    setError(null);
    try {
      const account = await cpfApi.getByEmployee(values.employeeId as string);
      setAccounts([account]);
    } catch (err: unknown) {
      setError(extractApiError(err));
      setAccounts([]);
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = async (values: Record<string, unknown>) => {
    setSubmitting(true);
    try {
      await cpfApi.create(values as Partial<CpfAccount>);
      message.success('Compte CPF cree');
      setModalOpen(false);
      form.resetFields();
    } catch (err: unknown) {
      message.error(extractApiError(err));
    } finally {
      setSubmitting(false);
    }
  };

  const handleCreditAnnual = async (id: string) => {
    try {
      await cpfApi.creditAnnual(id);
      message.success('Credit annuel effectue');
      load();
    } catch (err: unknown) {
      message.error(extractApiError(err));
    }
  };

  const handleMobilize = async (values: Record<string, unknown>) => {
    if (!selected) return;
    setSubmitting(true);
    try {
      await cpfApi.mobilize(selected.Id, values.amount as number, values.actionId as string);
      message.success('Mobilisation CPF effectuee');
      setMobilizeOpen(false);
      mobilizeForm.resetFields();
    } catch (err: unknown) {
      message.error(extractApiError(err));
    } finally {
      setSubmitting(false);
    }
  };

  const columns = [
    { title: 'ID Employe', dataIndex: 'EmployeeId', key: 'employeeId', ellipsis: true },
    {
      title: 'Solde (EUR)',
      dataIndex: 'BalanceEuros',
      key: 'balance',
      render: (v: number) => <strong>{v.toLocaleString('fr-FR')} EUR</strong>,
    },
    {
      title: 'Plafond (EUR)',
      dataIndex: 'CeilingEuros',
      key: 'ceiling',
      render: (v: number) => `${v.toLocaleString('fr-FR')} EUR`,
    },
    {
      title: 'Credit annuel (EUR)',
      dataIndex: 'AnnualCreditEuros',
      key: 'annual',
      render: (v: number) => `${v.toLocaleString('fr-FR')} EUR`,
    },
    {
      title: 'Faible qualification',
      dataIndex: 'IsLowQualified',
      key: 'lowQualified',
      render: (v: boolean) => <Tag color={v ? 'orange' : 'default'}>{v ? 'Oui' : 'Non'}</Tag>,
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: unknown, record: CpfAccount) => (
        <Space size="small">
          <Popconfirm title="Crediter le compte annuellement ?" onConfirm={() => handleCreditAnnual(record.Id)}>
            <Button type="link" size="small" icon={<EuroCircleOutlined />}>Credit annuel</Button>
          </Popconfirm>
          <Button type="link" size="small" onClick={() => { setSelected(record); setMobilizeOpen(true); }}>
            Mobiliser
          </Button>
        </Space>
      ),
    },
  ];

  return (
    <Space direction="vertical" size={16} style={{ width: '100%' }}>
      <Row justify="space-between" align="middle">
        <Col><Title level={4} style={{ margin: 0 }}>Comptes CPF</Title></Col>
        <Col>
          <Button type="primary" icon={<PlusOutlined />} onClick={() => { setEditing(null); form.resetFields(); setModalOpen(true); }}>
            Nouveau compte
          </Button>
        </Col>
      </Row>

      <Card>
        <Form layout="inline" onFinish={handleSearchEmployee} style={{ marginBottom: 16 }}>
          <Form.Item name="employeeId" label="ID Employe" rules={[{ required: true }]}>
            <Input placeholder="employee-001" style={{ width: 220 }} />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={loading}>Rechercher</Button>
          </Form.Item>
        </Form>
        {error && <Alert type="error" message={error} showIcon closable style={{ marginBottom: 8 }} />}
        <Table
          dataSource={accounts}
          columns={columns}
          rowKey="Id"
          loading={loading}
          pagination={false}
          locale={{ emptyText: 'Recherchez un compte par ID employe.' }}
        />
      </Card>

      <Modal title="Nouveau compte CPF" open={modalOpen} onCancel={() => setModalOpen(false)} footer={null}>
        <Form form={form} layout="vertical" onFinish={handleCreate}>
          <Form.Item name="EmployeeId" label="ID Employe" rules={[{ required: true }]}>
            <Input placeholder="employee-001" />
          </Form.Item>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="BalanceEuros" label="Solde initial (EUR)" initialValue={0}>
                <InputNumber style={{ width: '100%' }} min={0} />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="CeilingEuros" label="Plafond (EUR)" initialValue={5000}>
                <InputNumber style={{ width: '100%' }} min={0} />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="AnnualCreditEuros" label="Credit annuel (EUR)" initialValue={500}>
            <InputNumber style={{ width: '100%' }} min={0} />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={submitting} block>Creer</Button>
          </Form.Item>
        </Form>
      </Modal>

      <Modal title="Mobiliser CPF" open={mobilizeOpen} onCancel={() => setMobilizeOpen(false)} footer={null}>
        <Form form={mobilizeForm} layout="vertical" onFinish={handleMobilize}>
          <Form.Item name="amount" label="Montant (EUR)" rules={[{ required: true }]}>
            <InputNumber style={{ width: '100%' }} min={1} />
          </Form.Item>
          <Form.Item name="actionId" label="ID Action de formation" rules={[{ required: true }]}>
            <Input placeholder="action-001" />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={submitting} block>Mobiliser</Button>
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
};

export default CpfPage;
