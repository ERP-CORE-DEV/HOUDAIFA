import React, { useEffect, useState, useCallback } from 'react';
import { Table, Button, Tag, Space, Modal, Progress, message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, RightOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { bilanService } from '../../services/training';
import { BilanCompetences, BilanPhase, BilanStatus } from '../../services/training/bilanService';
import { PagedResult } from '../../types/training';

const PHASE_COLORS: Record<BilanPhase, string> = {
  Preliminaire: 'blue',
  Investigation: 'orange',
  Conclusion: 'green',
};

const STATUS_COLORS: Record<BilanStatus, string> = {
  EnCours: 'processing',
  Suspendu: 'warning',
  Termine: 'success',
  Abandonne: 'error',
};

const STATUS_LABELS: Record<BilanStatus, string> = {
  EnCours: 'En cours',
  Suspendu: 'Suspendu',
  Termine: 'Termine',
  Abandonne: 'Abandonne',
};

const MAX_HOURS = 24;

const BilanPage: React.FC = () => {
  const [bilans, setBilans] = useState<BilanCompetences[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);

  const loadBilans = useCallback(async () => {
    setLoading(true);
    try {
      const result: PagedResult<BilanCompetences> = await bilanService.getAll(page, 20);
      setBilans(result.Items);
      setTotal(result.TotalCount);
    } catch {
      message.error('Erreur lors du chargement des bilans de competences');
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => {
    loadBilans();
  }, [loadBilans]);

  const handleDelete = (id: string) => {
    Modal.confirm({
      title: 'Supprimer ce bilan de competences ?',
      content: 'Cette action est irreversible.',
      okText: 'Supprimer',
      okType: 'danger',
      cancelText: 'Annuler',
      onOk: async () => {
        await bilanService.delete(id);
        message.success('Bilan supprime');
        loadBilans();
      },
    });
  };

  const handleAdvancePhase = async (id: string) => {
    await bilanService.advancePhase(id);
    message.success('Phase avancee');
    loadBilans();
  };

  const columns: ColumnsType<BilanCompetences> = [
    {
      title: 'Employe',
      dataIndex: 'EmployeeNom',
      key: 'employee',
      width: 200,
    },
    {
      title: 'Phase',
      dataIndex: 'Phase',
      key: 'phase',
      width: 140,
      render: (phase: BilanPhase) => (
        <Tag color={PHASE_COLORS[phase]}>{phase}</Tag>
      ),
    },
    {
      title: 'Statut',
      dataIndex: 'Status',
      key: 'status',
      width: 120,
      render: (status: BilanStatus) => (
        <Tag color={STATUS_COLORS[status]}>{STATUS_LABELS[status]}</Tag>
      ),
    },
    {
      title: `Heures utilisees / ${MAX_HOURS}h max`,
      key: 'hours',
      width: 220,
      render: (_, record) => {
        const pct = Math.min(Math.round((record.UsedHours / MAX_HOURS) * 100), 100);
        return (
          <Space direction="vertical" size={0} style={{ width: '100%' }}>
            <span style={{ fontSize: 12 }}>
              {record.UsedHours}h / {MAX_HOURS}h
            </span>
            <Progress
              percent={pct}
              size="small"
              status={pct >= 100 ? 'exception' : 'active'}
            />
          </Space>
        );
      },
    },
    {
      title: 'Debut',
      dataIndex: 'StartDate',
      key: 'start',
      width: 110,
      render: (date: string) => new Date(date).toLocaleDateString('fr-FR'),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 140,
      render: (_, record) => (
        <Space size="small">
          <Button size="small" icon={<EditOutlined />} />
          {record.Status === 'EnCours' && (
            <Button
              size="small"
              type="primary"
              icon={<RightOutlined />}
              onClick={() => handleAdvancePhase(record.Id)}
              title="Avancer la phase"
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
        <h2>Bilan de competences</h2>
        <Button type="primary" icon={<PlusOutlined />}>Nouveau bilan</Button>
      </div>
      <Table<BilanCompetences>
        columns={columns}
        dataSource={bilans}
        rowKey="Id"
        loading={loading}
        pagination={{ current: page, total, pageSize: 20, onChange: setPage }}
      />
    </>
  );
};

export default BilanPage;
