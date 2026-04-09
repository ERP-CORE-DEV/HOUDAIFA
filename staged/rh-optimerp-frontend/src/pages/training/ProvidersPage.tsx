import React, { useEffect, useState, useCallback } from 'react';
import { Table, Button, Tag, Space, Modal, Rate, message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { providerService } from '../../services/training';
import { Provider } from '../../services/training/providerService';
import { PagedResult } from '../../types/training';

const ProvidersPage: React.FC = () => {
  const [providers, setProviders] = useState<Provider[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);

  const loadProviders = useCallback(async () => {
    setLoading(true);
    try {
      const result: PagedResult<Provider> = await providerService.getAll(page, 20);
      setProviders(result.Items);
      setTotal(result.TotalCount);
    } catch {
      message.error('Erreur lors du chargement des organismes de formation');
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => {
    loadProviders();
  }, [loadProviders]);

  const handleDelete = (id: string) => {
    Modal.confirm({
      title: 'Supprimer cet organisme de formation ?',
      content: 'Cette action est irreversible.',
      okText: 'Supprimer',
      okType: 'danger',
      cancelText: 'Annuler',
      onOk: async () => {
        await providerService.delete(id);
        message.success('Organisme supprime');
        loadProviders();
      },
    });
  };

  const columns: ColumnsType<Provider> = [
    {
      title: 'Nom',
      dataIndex: 'Name',
      key: 'name',
      ellipsis: true,
    },
    {
      title: 'SIRET',
      dataIndex: 'Siret',
      key: 'siret',
      width: 160,
    },
    {
      title: 'Certification Qualiopi',
      dataIndex: 'QualiopiCertified',
      key: 'qualiopi',
      width: 180,
      render: (certified: boolean) =>
        certified ? (
          <Tag color="green">Certifie</Tag>
        ) : (
          <Tag color="red">Non certifie</Tag>
        ),
    },
    {
      title: 'Domaines',
      dataIndex: 'Domaines',
      key: 'domaines',
      render: (domaines: string[]) => (
        <>
          {domaines.map((d) => (
            <Tag key={d}>{d}</Tag>
          ))}
        </>
      ),
    },
    {
      title: 'Note moyenne',
      dataIndex: 'AverageRating',
      key: 'rating',
      width: 160,
      render: (rating: number) => (
        <Rate disabled allowHalf defaultValue={rating} style={{ fontSize: 14 }} />
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
        <h2>Organismes de formation</h2>
        <Button type="primary" icon={<PlusOutlined />}>Nouvel organisme</Button>
      </div>
      <Table<Provider>
        columns={columns}
        dataSource={providers}
        rowKey="Id"
        loading={loading}
        pagination={{ current: page, total, pageSize: 20, onChange: setPage }}
      />
    </>
  );
};

export default ProvidersPage;
