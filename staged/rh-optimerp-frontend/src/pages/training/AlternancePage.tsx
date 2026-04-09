import React, { useEffect, useState, useCallback } from 'react';
import { Table, Button, Tag, Space, Modal, message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { alternanceService } from '../../services/training';
import {
  AlternanceContract,
  AlternanceContractType,
  AlternanceStatus,
} from '../../services/training/alternanceService';
import { PagedResult } from '../../types/training';
import { AlternanceModal } from '../../components/training/modals';

const CONTRACT_TYPE_COLORS: Record<AlternanceContractType, string> = {
  Apprentissage: 'blue',
  Professionnalisation: 'purple',
};

const STATUS_COLORS: Record<AlternanceStatus, string> = {
  EnCours: 'processing',
  Termine: 'success',
  Rompu: 'error',
  Suspendu: 'warning',
};

const STATUS_LABELS: Record<AlternanceStatus, string> = {
  EnCours: 'En cours',
  Termine: 'Termine',
  Rompu: 'Rompu',
  Suspendu: 'Suspendu',
};

const AGE_MIN = 16;
const AGE_MAX = 29;

const AlternancePage: React.FC = () => {
  const [contracts, setContracts] = useState<AlternanceContract[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingRecord, setEditingRecord] = useState<AlternanceContract | null>(null);

  const loadContracts = useCallback(async () => {
    setLoading(true);
    try {
      const result: PagedResult<AlternanceContract> = await alternanceService.getAll(page, 20);
      setContracts(result.Items);
      setTotal(result.TotalCount);
    } catch {
      message.error("Erreur lors du chargement des contrats d'alternance");
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => {
    loadContracts();
  }, [loadContracts]);

  const handleOpenCreate = () => {
    setEditingRecord(null);
    setIsModalOpen(true);
  };

  const handleOpenEdit = (record: AlternanceContract) => {
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
    loadContracts();
  };

  const handleDelete = (id: string) => {
    Modal.confirm({
      title: "Supprimer ce contrat d'alternance ?",
      content: 'Cette action est irreversible.',
      okText: 'Supprimer',
      okType: 'danger',
      cancelText: 'Annuler',
      onOk: async () => {
        await alternanceService.delete(id);
        message.success('Contrat supprime');
        loadContracts();
      },
    });
  };

  const columns: ColumnsType<AlternanceContract> = [
    {
      title: 'Apprenti',
      dataIndex: 'ApprentiNom',
      key: 'apprenti',
      width: 180,
    },
    {
      title: 'Age',
      dataIndex: 'ApprentiAge',
      key: 'age',
      width: 80,
      render: (age: number) => {
        const valid = age >= AGE_MIN && age <= AGE_MAX;
        return (
          <Tag color={valid ? 'green' : 'red'}>{age} ans</Tag>
        );
      },
    },
    {
      title: "Maitre d'apprentissage",
      dataIndex: 'MaitreApprentissageNom',
      key: 'maitre',
      width: 200,
    },
    {
      title: 'Type de contrat',
      dataIndex: 'ContractType',
      key: 'contractType',
      width: 180,
      render: (type: AlternanceContractType) => (
        <Tag color={CONTRACT_TYPE_COLORS[type]}>{type}</Tag>
      ),
    },
    {
      title: 'Debut',
      dataIndex: 'StartDate',
      key: 'start',
      width: 110,
      render: (date: string) => new Date(date).toLocaleDateString('fr-FR'),
    },
    {
      title: 'Fin',
      dataIndex: 'EndDate',
      key: 'end',
      width: 110,
      render: (date: string) => new Date(date).toLocaleDateString('fr-FR'),
    },
    {
      title: 'Remuneration',
      dataIndex: 'Remuneration',
      key: 'remuneration',
      width: 130,
      render: (val: number) => `${val.toLocaleString('fr-FR')} EUR`,
    },
    {
      title: 'Statut',
      dataIndex: 'Status',
      key: 'status',
      width: 110,
      render: (status: AlternanceStatus) => (
        <Tag color={STATUS_COLORS[status]}>{STATUS_LABELS[status]}</Tag>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 100,
      render: (_, record) => (
        <Space size="small">
          <Button size="small" icon={<EditOutlined />} onClick={() => handleOpenEdit(record)} />
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
        <h2>Contrats d&apos;alternance</h2>
        <Button type="primary" icon={<PlusOutlined />} onClick={handleOpenCreate}>
          Nouveau contrat
        </Button>
      </div>
      <Table<AlternanceContract>
        columns={columns}
        dataSource={contracts}
        rowKey="Id"
        loading={loading}
        pagination={{ current: page, total, pageSize: 20, onChange: setPage }}
        scroll={{ x: 1200 }}
      />

      <AlternanceModal
        visible={isModalOpen}
        onCancel={handleModalClose}
        onSuccess={handleModalSuccess}
        editRecord={editingRecord}
      />
    </>
  );
};

export default AlternancePage;
