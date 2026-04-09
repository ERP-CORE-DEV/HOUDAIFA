import React, { useEffect, useState, useCallback } from 'react';
import { Table, Button, Tag, Space, Modal, message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { evaluationService } from '../../services/training';
import { EvaluationRecord } from '../../services/training/evaluationService';
import { EvaluationLevel, PagedResult } from '../../types/training';

const KIRKPATRICK_COLORS: Record<EvaluationLevel, string> = {
  Satisfaction: 'blue',
  Apprentissage: 'cyan',
  Transfert: 'orange',
  Resultats: 'green',
};

const KIRKPATRICK_LABELS: Record<EvaluationLevel, string> = {
  Satisfaction: 'Niveau 1 — Satisfaction',
  Apprentissage: 'Niveau 2 — Apprentissage',
  Transfert: 'Niveau 3 — Transfert',
  Resultats: 'Niveau 4 — Resultats',
};

const EvaluationPage: React.FC = () => {
  const [evaluations, setEvaluations] = useState<EvaluationRecord[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);

  const loadEvaluations = useCallback(async () => {
    setLoading(true);
    try {
      const result: PagedResult<EvaluationRecord> = await evaluationService.getAll(page, 20);
      setEvaluations(result.Items);
      setTotal(result.TotalCount);
    } catch {
      message.error('Erreur lors du chargement des evaluations');
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => {
    loadEvaluations();
  }, [loadEvaluations]);

  const handleDelete = (id: string) => {
    Modal.confirm({
      title: 'Supprimer cette evaluation ?',
      content: 'Cette action est irreversible.',
      okText: 'Supprimer',
      okType: 'danger',
      cancelText: 'Annuler',
      onOk: async () => {
        await evaluationService.delete(id);
        message.success('Evaluation supprimee');
        loadEvaluations();
      },
    });
  };

  const columns: ColumnsType<EvaluationRecord> = [
    {
      title: 'Session',
      dataIndex: 'SessionTitle',
      key: 'session',
      ellipsis: true,
    },
    {
      title: 'Evaluateur',
      dataIndex: 'EvaluateurNom',
      key: 'evaluateur',
      width: 180,
    },
    {
      title: 'Niveau Kirkpatrick',
      dataIndex: 'Level',
      key: 'level',
      width: 220,
      render: (level: EvaluationLevel) => (
        <Tag color={KIRKPATRICK_COLORS[level]}>{KIRKPATRICK_LABELS[level]}</Tag>
      ),
    },
    {
      title: 'Score',
      dataIndex: 'Score',
      key: 'score',
      width: 90,
      render: (score: number) => `${score} / 10`,
    },
    {
      title: 'Date',
      dataIndex: 'EvaluationDate',
      key: 'date',
      width: 120,
      render: (date: string) =>
        new Date(date).toLocaleDateString('fr-FR'),
    },
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
            onClick={() => handleDelete(record.Id)}
          />
        </Space>
      ),
    },
  ];

  return (
    <>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <h2>Evaluations de formation</h2>
        <Button type="primary" icon={<PlusOutlined />}>Nouvelle evaluation</Button>
      </div>
      <Table<EvaluationRecord>
        columns={columns}
        dataSource={evaluations}
        rowKey="Id"
        loading={loading}
        pagination={{ current: page, total, pageSize: 20, onChange: setPage }}
      />
    </>
  );
};

export default EvaluationPage;
