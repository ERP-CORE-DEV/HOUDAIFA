import React, { useEffect, useState, useCallback } from 'react';
import { Table, Button, Tag, Space, Modal, message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, CheckOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { trainingPlanService } from '../../services/training';
import { TrainingPlan, TrainingPlanStatus } from '../../types/training';
import { TrainingPlanModal } from '../../components/training/modals';

const statusColors: Record<TrainingPlanStatus, string> = {
  Draft: 'default',
  PendingApproval: 'processing',
  Approved: 'success',
  InExecution: 'blue',
  Completed: 'green',
  Archived: 'grey',
};

const statusLabels: Record<TrainingPlanStatus, string> = {
  Draft: 'Brouillon',
  PendingApproval: 'En attente',
  Approved: 'Approuve',
  InExecution: 'En cours',
  Completed: 'Termine',
  Archived: 'Archive',
};

const TrainingPlansPage: React.FC = () => {
  const [plans, setPlans] = useState<TrainingPlan[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingRecord, setEditingRecord] = useState<TrainingPlan | null>(null);

  const loadPlans = useCallback(async () => {
    setLoading(true);
    try {
      const result = await trainingPlanService.getAll(page, 20);
      setPlans(result.Items);
      setTotal(result.TotalCount);
    } catch {
      message.error('Erreur lors du chargement des plans de formation');
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => {
    loadPlans();
  }, [loadPlans]);

  const handleOpenCreate = () => {
    setEditingRecord(null);
    setIsModalOpen(true);
  };

  const handleOpenEdit = (record: TrainingPlan) => {
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
    loadPlans();
  };

  const handleDelete = async (id: string) => {
    Modal.confirm({
      title: 'Supprimer ce plan de formation ?',
      content: 'Cette action est irreversible.',
      okText: 'Supprimer',
      okType: 'danger',
      cancelText: 'Annuler',
      onOk: async () => {
        await trainingPlanService.delete(id);
        message.success('Plan supprime');
        loadPlans();
      },
    });
  };

  const handleApprove = async (id: string) => {
    await trainingPlanService.approve(id);
    message.success('Plan approuve');
    loadPlans();
  };

  const columns: ColumnsType<TrainingPlan> = [
    { title: 'Titre', dataIndex: 'Title', key: 'title', ellipsis: true },
    { title: 'Annee', dataIndex: 'Year', key: 'year', width: 80 },
    {
      title: 'Statut',
      dataIndex: 'Status',
      key: 'status',
      width: 120,
      render: (status: TrainingPlanStatus) => (
        <Tag color={statusColors[status]}>{statusLabels[status]}</Tag>
      ),
    },
    {
      title: 'Budget alloue',
      dataIndex: 'BudgetAllocated',
      key: 'budget',
      width: 140,
      render: (val: number) => `${val.toLocaleString('fr-FR')} EUR`,
    },
    {
      title: 'Consomme',
      dataIndex: 'BudgetConsumed',
      key: 'consumed',
      width: 140,
      render: (val: number) => `${val.toLocaleString('fr-FR')} EUR`,
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 160,
      render: (_, record) => (
        <Space size="small">
          <Button
            size="small"
            icon={<EditOutlined />}
            onClick={() => handleOpenEdit(record)}
          />
          {record.Status === 'PendingApproval' && (
            <Button
              size="small"
              type="primary"
              icon={<CheckOutlined />}
              onClick={() => handleApprove(record.Id)}
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
        <h2>Plans de formation</h2>
        <Button type="primary" icon={<PlusOutlined />} onClick={handleOpenCreate}>
          Nouveau plan
        </Button>
      </div>
      <Table<TrainingPlan>
        columns={columns}
        dataSource={plans}
        rowKey="Id"
        loading={loading}
        pagination={{ current: page, total, pageSize: 20, onChange: setPage }}
      />

      <TrainingPlanModal
        visible={isModalOpen}
        onCancel={handleModalClose}
        onSuccess={handleModalSuccess}
        editRecord={editingRecord}
      />
    </>
  );
};

export default TrainingPlansPage;
