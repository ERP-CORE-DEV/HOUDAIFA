import React, { useEffect, useState, useCallback } from 'react';
import { Table, Button, Space, Modal, Input, Tag, message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { competencyService } from '../../services/training';
import { Competency } from '../../types/training';
import { CompetencyModal } from '../../components/training/modals';

const CompetenciesPage: React.FC = () => {
  const [competencies, setCompetencies] = useState<Competency[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [domainFilter, setDomainFilter] = useState<string>('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingRecord, setEditingRecord] = useState<Competency | null>(null);

  const loadCompetencies = useCallback(async () => {
    setLoading(true);
    try {
      const result = await competencyService.getPaged(page, 20);
      const filtered = domainFilter
        ? result.Items.filter((c) =>
            c.Domain.toLowerCase().includes(domainFilter.toLowerCase()),
          )
        : result.Items;
      setCompetencies(filtered);
      setTotal(result.TotalCount);
    } catch {
      message.error('Erreur lors du chargement du referentiel de competences');
    } finally {
      setLoading(false);
    }
  }, [page, domainFilter]);

  useEffect(() => {
    loadCompetencies();
  }, [loadCompetencies]);

  const handleOpenCreate = () => {
    setEditingRecord(null);
    setIsModalOpen(true);
  };

  const handleOpenEdit = (record: Competency) => {
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
    loadCompetencies();
  };

  const handleDelete = (id: string) => {
    Modal.confirm({
      title: 'Supprimer cette competence ?',
      content: 'Cette action est irreversible.',
      okText: 'Supprimer',
      okType: 'danger',
      cancelText: 'Annuler',
      onOk: async () => {
        await competencyService.delete(id);
        message.success('Competence supprimee');
        loadCompetencies();
      },
    });
  };

  const columns: ColumnsType<Competency> = [
    { title: 'Code', dataIndex: 'Code', key: 'code', width: 100 },
    { title: 'Nom', dataIndex: 'Name', key: 'name', ellipsis: true },
    { title: 'Domaine', dataIndex: 'Domain', key: 'domain', width: 140 },
    { title: 'Famille', dataIndex: 'Family', key: 'family', width: 140 },
    { title: 'Type', dataIndex: 'Type', key: 'type', width: 120 },
    {
      title: 'Critique',
      dataIndex: 'IsCritical',
      key: 'critical',
      width: 90,
      render: (val: boolean) =>
        val ? <Tag color="red">Critique</Tag> : <Tag color="default">Standard</Tag>,
    },
    {
      title: 'Statut',
      dataIndex: 'IsActive',
      key: 'active',
      width: 90,
      render: (val: boolean) =>
        val ? <Tag color="success">Actif</Tag> : <Tag color="default">Inactif</Tag>,
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
        <h2>Referentiel de competences</h2>
        <Button type="primary" icon={<PlusOutlined />} onClick={handleOpenCreate}>
          Ajouter une competence
        </Button>
      </div>

      <div style={{ marginBottom: 16 }}>
        <Input.Search
          placeholder="Filtrer par domaine..."
          allowClear
          style={{ maxWidth: 320 }}
          onSearch={(val) => {
            setDomainFilter(val);
            setPage(1);
          }}
          onChange={(e) => {
            if (!e.target.value) {
              setDomainFilter('');
              setPage(1);
            }
          }}
        />
      </div>

      <Table<Competency>
        columns={columns}
        dataSource={competencies}
        rowKey="Id"
        loading={loading}
        pagination={{ current: page, total, pageSize: 20, onChange: setPage }}
      />

      <CompetencyModal
        visible={isModalOpen}
        onCancel={handleModalClose}
        onSuccess={handleModalSuccess}
        editRecord={editingRecord}
      />
    </>
  );
};

export default CompetenciesPage;
