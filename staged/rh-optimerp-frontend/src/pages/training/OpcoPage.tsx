import React, { useEffect, useState, useCallback } from 'react';
import { Table, Button, Tag, Space, Modal, Divider, message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, CheckOutlined, CloseOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { opcoService } from '../../services/training';
import { Opco, FundingRequest } from '../../services/training/opcoService';
import { FundingStatus, PagedResult } from '../../types/training';

const FUNDING_STATUS_COLORS: Record<FundingStatus, string> = {
  Draft: 'default',
  Submitted: 'processing',
  UnderReview: 'orange',
  Approved: 'success',
  Rejected: 'error',
  Paid: 'green',
};

const FUNDING_STATUS_LABELS: Record<FundingStatus, string> = {
  Draft: 'Brouillon',
  Submitted: 'Soumis',
  UnderReview: 'En cours d\'examen',
  Approved: 'Approuve',
  Rejected: 'Refuse',
  Paid: 'Paye',
};

const OpcoPage: React.FC = () => {
  const [opcos, setOpcos] = useState<Opco[]>([]);
  const [fundingRequests, setFundingRequests] = useState<FundingRequest[]>([]);
  const [loadingOpcos, setLoadingOpcos] = useState(true);
  const [loadingFunding, setLoadingFunding] = useState(true);
  const [opcoPage, setOpcoPage] = useState(1);
  const [fundingPage, setFundingPage] = useState(1);
  const [opcoTotal, setOpcoTotal] = useState(0);
  const [fundingTotal, setFundingTotal] = useState(0);

  const loadOpcos = useCallback(async () => {
    setLoadingOpcos(true);
    try {
      const result: PagedResult<Opco> = await opcoService.getAll(opcoPage, 20);
      setOpcos(result.Items);
      setOpcoTotal(result.TotalCount);
    } catch {
      message.error('Erreur lors du chargement des OPCO');
    } finally {
      setLoadingOpcos(false);
    }
  }, [opcoPage]);

  const loadFundingRequests = useCallback(async () => {
    setLoadingFunding(true);
    try {
      const result: PagedResult<FundingRequest> = await opcoService.getAllFundingRequests(fundingPage, 20);
      setFundingRequests(result.Items);
      setFundingTotal(result.TotalCount);
    } catch {
      message.error('Erreur lors du chargement des demandes de financement');
    } finally {
      setLoadingFunding(false);
    }
  }, [fundingPage]);

  useEffect(() => {
    loadOpcos();
  }, [loadOpcos]);

  useEffect(() => {
    loadFundingRequests();
  }, [loadFundingRequests]);

  const handleDeleteOpco = (id: string) => {
    Modal.confirm({
      title: 'Supprimer cet OPCO ?',
      content: 'Cette action est irreversible.',
      okText: 'Supprimer',
      okType: 'danger',
      cancelText: 'Annuler',
      onOk: async () => {
        await opcoService.delete(id);
        message.success('OPCO supprime');
        loadOpcos();
      },
    });
  };

  const handleApprove = async (id: string) => {
    await opcoService.approveFundingRequest(id);
    message.success('Demande approuvee');
    loadFundingRequests();
  };

  const handleReject = async (id: string) => {
    await opcoService.rejectFundingRequest(id);
    message.success('Demande refusee');
    loadFundingRequests();
  };

  const opcoColumns: ColumnsType<Opco> = [
    { title: 'Nom', dataIndex: 'Name', key: 'name', ellipsis: true },
    { title: 'Code', dataIndex: 'Code', key: 'code', width: 100 },
    { title: 'Convention collective', dataIndex: 'ConventionCollective', key: 'convention', ellipsis: true },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      render: (_, record) => (
        <Space size="small">
          <Button size="small" icon={<EditOutlined />} />
          <Button
            size="small"
            danger
            icon={<DeleteOutlined />}
            onClick={() => handleDeleteOpco(record.Id)}
          />
        </Space>
      ),
    },
  ];

  const fundingColumns: ColumnsType<FundingRequest> = [
    { title: 'OPCO', dataIndex: 'OpcoName', key: 'opco', width: 150 },
    {
      title: 'Montant',
      dataIndex: 'Amount',
      key: 'amount',
      width: 120,
      render: (val: number) => `${val.toLocaleString('fr-FR')} EUR`,
    },
    {
      title: 'Statut',
      dataIndex: 'Status',
      key: 'status',
      width: 160,
      render: (status: FundingStatus) => (
        <Tag color={FUNDING_STATUS_COLORS[status]}>{FUNDING_STATUS_LABELS[status]}</Tag>
      ),
    },
    {
      title: 'Date',
      dataIndex: 'RequestDate',
      key: 'date',
      width: 120,
      render: (date: string) => new Date(date).toLocaleDateString('fr-FR'),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 140,
      render: (_, record) => (
        <Space size="small">
          {record.Status === 'Submitted' && (
            <>
              <Button
                size="small"
                type="primary"
                icon={<CheckOutlined />}
                onClick={() => handleApprove(record.Id)}
              />
              <Button
                size="small"
                danger
                icon={<CloseOutlined />}
                onClick={() => handleReject(record.Id)}
              />
            </>
          )}
        </Space>
      ),
    },
  ];

  return (
    <>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <h2>OPCO &amp; Financement</h2>
        <Button type="primary" icon={<PlusOutlined />}>Nouvel OPCO</Button>
      </div>
      <Table<Opco>
        columns={opcoColumns}
        dataSource={opcos}
        rowKey="Id"
        loading={loadingOpcos}
        pagination={{ current: opcoPage, total: opcoTotal, pageSize: 20, onChange: setOpcoPage }}
      />
      <Divider />
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <h3>Demandes de financement</h3>
        <Button icon={<PlusOutlined />}>Nouvelle demande</Button>
      </div>
      <Table<FundingRequest>
        columns={fundingColumns}
        dataSource={fundingRequests}
        rowKey="Id"
        loading={loadingFunding}
        pagination={{ current: fundingPage, total: fundingTotal, pageSize: 20, onChange: setFundingPage }}
      />
    </>
  );
};

export default OpcoPage;
