import React, { useEffect, useState, useCallback } from 'react';
import { Table, Button, Tag, Space, Modal, message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, CheckOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { complianceService } from '../../services/training';
import {
  ComplianceObligation,
  ComplianceStatus,
  RiskLevel,
  ObligationType,
} from '../../services/training/complianceService';
import { PagedResult } from '../../types/training';
import { ComplianceModal } from '../../components/training/modals';

const STATUS_COLORS: Record<ComplianceStatus, string> = {
  Conforme: 'success',
  NonConforme: 'error',
  EnCours: 'processing',
};

const STATUS_LABELS: Record<ComplianceStatus, string> = {
  Conforme: 'Conforme',
  NonConforme: 'Non conforme',
  EnCours: 'En cours',
};

const RISK_COLORS: Record<RiskLevel, string> = {
  Faible: 'green',
  Moyen: 'orange',
  Eleve: 'red',
  Critique: 'red',
};

const OBLIGATION_TYPE_LABELS: Record<ObligationType, string> = {
  Reglementaire: 'Reglementaire',
  Conventionnelle: 'Conventionnelle',
  Interne: 'Interne',
};

const CompliancePage: React.FC = () => {
  const [obligations, setObligations] = useState<ComplianceObligation[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingRecord, setEditingRecord] = useState<ComplianceObligation | null>(null);

  const loadObligations = useCallback(async () => {
    setLoading(true);
    try {
      const result: PagedResult<ComplianceObligation> = await complianceService.getAll(page, 20);
      setObligations(result.Items);
      setTotal(result.TotalCount);
    } catch {
      message.error('Erreur lors du chargement des obligations de conformite');
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => {
    loadObligations();
  }, [loadObligations]);

  const handleOpenCreate = () => {
    setEditingRecord(null);
    setIsModalOpen(true);
  };

  const handleOpenEdit = (record: ComplianceObligation) => {
    setEditingRecord(record);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setEditingRecord(null);
  };

  const handleModalSuccess = () => {
    setIsModalOpen(false);
    setEditingRecord(null);
    loadObligations();
  };

  const handleDelete = (id: string) => {
    Modal.confirm({
      title: 'Supprimer cette obligation ?',
      content: 'Cette action est irreversible.',
      okText: 'Supprimer',
      okType: 'danger',
      cancelText: 'Annuler',
      onOk: async () => {
        await complianceService.delete(id);
        message.success('Obligation supprimee');
        loadObligations();
      },
    });
  };

  const handleMarkConforme = async (id: string) => {
    await complianceService.markConforme(id);
    message.success('Obligation marquee conforme');
    loadObligations();
  };

  const columns: ColumnsType<ComplianceObligation> = [
    {
      title: 'Obligation',
      dataIndex: 'Title',
      key: 'title',
      ellipsis: true,
    },
    {
      title: 'Type',
      dataIndex: 'ObligationType',
      key: 'type',
      width: 160,
      render: (type: ObligationType) => (
        <Tag>{OBLIGATION_TYPE_LABELS[type]}</Tag>
      ),
    },
    {
      title: 'Echeance',
      dataIndex: 'Echeance',
      key: 'echeance',
      width: 120,
      render: (date: string) => new Date(date).toLocaleDateString('fr-FR'),
    },
    {
      title: 'Statut',
      dataIndex: 'Status',
      key: 'status',
      width: 140,
      render: (status: ComplianceStatus) => (
        <Tag color={STATUS_COLORS[status]}>{STATUS_LABELS[status]}</Tag>
      ),
    },
    {
      title: 'Niveau de risque',
      dataIndex: 'RiskLevel',
      key: 'risk',
      width: 140,
      render: (risk: RiskLevel) => (
        <Tag color={RISK_COLORS[risk]}>{risk}</Tag>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 140,
      render: (_, record) => (
        <Space size="small">
          <Button size="small" icon={<EditOutlined />} onClick={() => handleOpenEdit(record)} />
          {record.Status !== 'Conforme' && (
            <Button
              size="small"
              type="primary"
              icon={<CheckOutlined />}
              onClick={() => handleMarkConforme(record.Id)}
              title="Marquer conforme"
            />
          )}
          <Button
            size="small"
            danger
            icon={<DeleteOutlined />}
            onClick={() => handleDelete(record.Id)}
          />
        </Space>
      ),
    },
  ];

  return (
    <>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <h2>Conformite reglementaire</h2>
        <Button type="primary" icon={<PlusOutlined />} onClick={handleOpenCreate}>
          Nouvelle obligation
        </Button>
      </div>
      <Table<ComplianceObligation>
        columns={columns}
        dataSource={obligations}
        rowKey="Id"
        loading={loading}
        pagination={{ current: page, total, pageSize: 20, onChange: setPage }}
      />

      <ComplianceModal
        visible={isModalOpen}
        onCancel={handleModalClose}
        onSuccess={handleModalSuccess}
        editRecord={editingRecord}
      />
    </>
  );
};

export default CompliancePage;
