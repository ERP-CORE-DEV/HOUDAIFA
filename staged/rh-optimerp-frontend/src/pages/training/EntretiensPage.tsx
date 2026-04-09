import React, { useEffect, useState, useCallback } from 'react';
import { Table, Button, Space, Tag, message } from 'antd';
import { PlusOutlined, EditOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import { entretienService } from '../../services/training';
import { ProfessionalInterview, InterviewType } from '../../types/training';
import { EntretienModal } from '../../components/training/modals';

const interviewTypeLabels: Record<InterviewType, string> = {
  Biennial: 'Biennal',
  SixYearReview: 'Etat des lieux 6 ans',
  PostAbsence: 'Retour absence',
  Voluntary: 'Volontaire',
};

const statusColors: Record<string, string> = {
  Planifie: 'blue',
  Realise: 'success',
  Annule: 'error',
  EnCours: 'processing',
};

const statusLabels: Record<string, string> = {
  Planifie: 'Planifie',
  Realise: 'Realise',
  Annule: 'Annule',
  EnCours: 'En cours',
};

const EntretiensPage: React.FC = () => {
  const [interviews, setInterviews] = useState<ProfessionalInterview[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingRecord, setEditingRecord] = useState<ProfessionalInterview | null>(null);

  const loadInterviews = useCallback(async () => {
    setLoading(true);
    try {
      const result = await entretienService.getPaged(page, 20);
      setInterviews(result.Items);
      setTotal(result.TotalCount);
    } catch {
      message.error('Erreur lors du chargement des entretiens professionnels');
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => {
    loadInterviews();
  }, [loadInterviews]);

  const handleOpenCreate = () => {
    setEditingRecord(null);
    setIsModalOpen(true);
  };

  const handleOpenEdit = (record: ProfessionalInterview) => {
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
    loadInterviews();
  };

  const columns: ColumnsType<ProfessionalInterview> = [
    {
      title: 'Employe',
      dataIndex: 'EmployeeId',
      key: 'employee',
      width: 180,
      ellipsis: true,
    },
    {
      title: 'Manager',
      dataIndex: 'ManagerId',
      key: 'manager',
      width: 180,
      ellipsis: true,
    },
    {
      title: 'Type',
      dataIndex: 'Type',
      key: 'type',
      width: 180,
      render: (type: InterviewType) => interviewTypeLabels[type] ?? type,
    },
    {
      title: 'Date planifiee',
      dataIndex: 'ScheduledDate',
      key: 'scheduledDate',
      width: 140,
      render: (date: string) => dayjs(date).format('DD/MM/YYYY'),
    },
    {
      title: 'Date realise',
      dataIndex: 'ConductedDate',
      key: 'conductedDate',
      width: 140,
      render: (date?: string) => (date ? dayjs(date).format('DD/MM/YYYY') : '—'),
    },
    {
      title: 'Statut',
      dataIndex: 'Status',
      key: 'status',
      width: 120,
      render: (status: string) => (
        <Tag color={statusColors[status] ?? 'default'}>
          {statusLabels[status] ?? status}
        </Tag>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 80,
      render: (_, record) => (
        <Space size="small">
          <Button size="small" icon={<EditOutlined />} onClick={() => handleOpenEdit(record)} />
        </Space>
      ),
    },
  ];

  return (
    <>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <h2>Entretiens professionnels</h2>
        <Button type="primary" icon={<PlusOutlined />} onClick={handleOpenCreate}>
          Planifier un entretien
        </Button>
      </div>

      <Table<ProfessionalInterview>
        columns={columns}
        dataSource={interviews}
        rowKey="Id"
        loading={loading}
        pagination={{ current: page, total, pageSize: 20, onChange: setPage }}
      />

      <EntretienModal
        visible={isModalOpen}
        onCancel={handleModalClose}
        onSuccess={handleModalSuccess}
        editRecord={editingRecord}
      />
    </>
  );
};

export default EntretiensPage;
