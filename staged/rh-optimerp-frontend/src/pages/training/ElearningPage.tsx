import React, { useEffect, useState, useCallback } from 'react';
import { Table, Button, Tag, Space, Modal, Progress, message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { elearningService } from '../../services/training';
import { ElearningCourse } from '../../services/training/elearningService';
import { PagedResult } from '../../types/training';

const COURSE_TYPE_LABELS: Record<string, string> = {
  MOOC: 'MOOC',
  SPOC: 'SPOC',
  VirtualClass: 'Classe virtuelle',
};

const COURSE_TYPE_COLORS: Record<string, string> = {
  MOOC: 'blue',
  SPOC: 'purple',
  VirtualClass: 'cyan',
};

const ElearningPage: React.FC = () => {
  const [courses, setCourses] = useState<ElearningCourse[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);

  const loadCourses = useCallback(async () => {
    setLoading(true);
    try {
      const result: PagedResult<ElearningCourse> = await elearningService.getAll(page, 20);
      setCourses(result.Items);
      setTotal(result.TotalCount);
    } catch {
      message.error('Erreur lors du chargement des formations e-learning');
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => {
    loadCourses();
  }, [loadCourses]);

  const handleDelete = (id: string) => {
    Modal.confirm({
      title: 'Supprimer cette formation e-learning ?',
      content: 'Cette action est irreversible.',
      okText: 'Supprimer',
      okType: 'danger',
      cancelText: 'Annuler',
      onOk: async () => {
        await elearningService.delete(id);
        message.success('Formation supprimee');
        loadCourses();
      },
    });
  };

  const columns: ColumnsType<ElearningCourse> = [
    {
      title: 'Titre',
      dataIndex: 'Title',
      key: 'title',
      ellipsis: true,
    },
    {
      title: 'Type',
      dataIndex: 'CourseType',
      key: 'courseType',
      width: 150,
      render: (type: string) => (
        <Tag color={COURSE_TYPE_COLORS[type] ?? 'default'}>
          {COURSE_TYPE_LABELS[type] ?? type}
        </Tag>
      ),
    },
    {
      title: 'Duree (h)',
      dataIndex: 'DurationHours',
      key: 'duration',
      width: 100,
      render: (val: number) => `${val} h`,
    },
    {
      title: 'Plateforme',
      dataIndex: 'Platform',
      key: 'platform',
      width: 140,
    },
    {
      title: 'Progression',
      dataIndex: 'CompletionPercentage',
      key: 'completion',
      width: 180,
      render: (pct: number) => (
        <Progress
          percent={pct}
          size="small"
          status={pct === 100 ? 'success' : 'active'}
        />
      ),
    },
    {
      title: 'Statut',
      dataIndex: 'IsActive',
      key: 'status',
      width: 100,
      render: (active: boolean) => (
        <Tag color={active ? 'green' : 'red'}>{active ? 'Actif' : 'Inactif'}</Tag>
      ),
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
        <h2>E-Learning &amp; Formation digitale</h2>
        <Button type="primary" icon={<PlusOutlined />}>Nouvelle formation</Button>
      </div>
      <Table<ElearningCourse>
        columns={columns}
        dataSource={courses}
        rowKey="Id"
        loading={loading}
        pagination={{ current: page, total, pageSize: 20, onChange: setPage }}
      />
    </>
  );
};

export default ElearningPage;
