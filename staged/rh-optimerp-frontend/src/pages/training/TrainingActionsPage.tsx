import React, { useEffect, useState, useCallback } from 'react';
import { Table, Button, Space, Tag, message, Badge } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import { trainingActionService, trainingSessionService } from '../../services/training';
import { TrainingAction, TrainingSession, SessionStatus, TrainingActionType, TrainingModality } from '../../types/training';
import { TrainingActionModal } from '../../components/training/modals';

const actionTypeLabels: Record<TrainingActionType, string> = {
  Obligatoire: 'Obligatoire',
  Adaptation: 'Adaptation',
  Developpement: 'Developpement',
};

const actionTypeColors: Record<TrainingActionType, string> = {
  Obligatoire: 'red',
  Adaptation: 'blue',
  Developpement: 'green',
};

const modalityLabels: Record<TrainingModality, string> = {
  Presentiel: 'Presentiel',
  Distanciel: 'Distanciel',
  ELearning: 'E-Learning',
  Blended: 'Blended',
  Afest: 'AFEST',
};

const sessionStatusColors: Record<SessionStatus, string> = {
  Planned: 'blue',
  Confirmed: 'cyan',
  InProgress: 'processing',
  Completed: 'success',
  Cancelled: 'error',
  Postponed: 'warning',
};

const sessionStatusLabels: Record<SessionStatus, string> = {
  Planned: 'Planifiee',
  Confirmed: 'Confirmee',
  InProgress: 'En cours',
  Completed: 'Terminee',
  Cancelled: 'Annulee',
  Postponed: 'Reportee',
};

const SessionsSubTable: React.FC<{ actionId: string }> = ({ actionId }) => {
  const [sessions, setSessions] = useState<TrainingSession[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadSessions = async () => {
      try {
        const data = await trainingSessionService.getByActionId(actionId);
        setSessions(data);
      } catch {
        message.error('Erreur lors du chargement des sessions');
      } finally {
        setLoading(false);
      }
    };
    loadSessions();
  }, [actionId]);

  const sessionColumns: ColumnsType<TrainingSession> = [
    {
      title: 'Debut',
      dataIndex: 'StartDate',
      key: 'startDate',
      width: 130,
      render: (date: string) => dayjs(date).format('DD/MM/YYYY'),
    },
    {
      title: 'Fin',
      dataIndex: 'EndDate',
      key: 'endDate',
      width: 130,
      render: (date: string) => dayjs(date).format('DD/MM/YYYY'),
    },
    {
      title: 'Lieu',
      dataIndex: 'Location',
      key: 'location',
      ellipsis: true,
      render: (val?: string) => val ?? '—',
    },
    {
      title: 'Statut',
      dataIndex: 'Status',
      key: 'status',
      width: 120,
      render: (status: SessionStatus) => (
        <Badge
          status={sessionStatusColors[status] as 'success' | 'processing' | 'error' | 'warning' | 'default'}
          text={sessionStatusLabels[status]}
        />
      ),
    },
    {
      title: 'Inscrits / Max',
      key: 'capacity',
      width: 120,
      render: (_, record) => `${record.EnrolledCount} / ${record.MaxCapacity}`,
    },
  ];

  return (
    <Table<TrainingSession>
      columns={sessionColumns}
      dataSource={sessions}
      rowKey="Id"
      loading={loading}
      pagination={false}
      size="small"
      style={{ margin: '8px 0' }}
    />
  );
};

const TrainingActionsPage: React.FC = () => {
  const [actions, setActions] = useState<TrainingAction[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingRecord, setEditingRecord] = useState<TrainingAction | null>(null);

  const loadActions = useCallback(async () => {
    setLoading(true);
    try {
      const result = await trainingActionService.getAll(page, 20);
      setActions(result.Items);
      setTotal(result.TotalCount);
    } catch {
      message.error('Erreur lors du chargement des actions de formation');
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => {
    loadActions();
  }, [loadActions]);

  const handleOpenCreate = () => {
    setEditingRecord(null);
    setIsModalOpen(true);
  };

  const handleOpenEdit = (record: TrainingAction) => {
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
    loadActions();
  };

  const handleDelete = (id: string) => {
    import('antd').then(({ Modal: AntModal }) => {
      AntModal.confirm({
        title: 'Supprimer cette action de formation ?',
        content: 'Cette action est irreversible.',
        okText: 'Supprimer',
        okType: 'danger',
        cancelText: 'Annuler',
        onOk: async () => {
          await trainingActionService.delete(id);
          message.success('Action supprimee');
          loadActions();
        },
      });
    });
  };

  const columns: ColumnsType<TrainingAction> = [
    { title: 'Titre', dataIndex: 'Title', key: 'title', ellipsis: true },
    {
      title: 'Type',
      dataIndex: 'Type',
      key: 'type',
      width: 130,
      render: (type: TrainingActionType) => (
        <Tag color={actionTypeColors[type]}>{actionTypeLabels[type]}</Tag>
      ),
    },
    {
      title: 'Modalite',
      dataIndex: 'Modality',
      key: 'modality',
      width: 120,
      render: (modality: TrainingModality) => modalityLabels[modality],
    },
    {
      title: 'Duree (h)',
      dataIndex: 'DurationHours',
      key: 'duration',
      width: 100,
      render: (val: number) => `${val} h`,
    },
    {
      title: 'Cout',
      dataIndex: 'Cost',
      key: 'cost',
      width: 120,
      render: (val: number) => `${val.toLocaleString('fr-FR')} EUR`,
    },
    {
      title: 'Participants max',
      dataIndex: 'MaxParticipants',
      key: 'maxParticipants',
      width: 140,
    },
    {
      title: 'Obligatoire',
      dataIndex: 'IsObligatory',
      key: 'obligatory',
      width: 110,
      render: (val: boolean) =>
        val ? <Tag color="red">Oui</Tag> : <Tag color="default">Non</Tag>,
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 100,
      render: (_, record) => (
        <Space size="small">
          <Button
            size="small"
            icon={<EditOutlined />}
            onClick={() => handleOpenEdit(record)}
          />
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
        <h2>Actions de formation</h2>
        <Button type="primary" icon={<PlusOutlined />} onClick={handleOpenCreate}>
          Nouvelle action
        </Button>
      </div>

      <Table<TrainingAction>
        columns={columns}
        dataSource={actions}
        rowKey="Id"
        loading={loading}
        pagination={{ current: page, total, pageSize: 20, onChange: setPage }}
        expandable={{
          expandedRowRender: (record) => <SessionsSubTable actionId={record.Id} />,
          rowExpandable: () => true,
        }}
      />

      <TrainingActionModal
        visible={isModalOpen}
        onCancel={handleModalClose}
        onSuccess={handleModalSuccess}
        editRecord={editingRecord}
      />
    </>
  );
};

export default TrainingActionsPage;
