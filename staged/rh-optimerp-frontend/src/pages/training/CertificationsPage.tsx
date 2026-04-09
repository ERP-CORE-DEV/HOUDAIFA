import React, { useEffect, useState, useCallback } from 'react';
import { Tabs, Table, Button, Space, Tag, message } from 'antd';
import { PlusOutlined, EditOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import { certificationService } from '../../services/training';
import { CertificationRncp, VaeProject, VaeStatus } from '../../types/training';
import { CertificationModal } from '../../components/training/modals';

const vaeStatusLabels: Record<VaeStatus, string> = {
  Recevabilite: 'Recevabilite',
  Accompagnement: 'Accompagnement',
  Livret2: 'Livret 2',
  Jury: 'Jury',
  ValidationTotale: 'Validation totale',
  ValidationPartielle: 'Validation partielle',
  Refus: 'Refus',
};

const vaeStatusColors: Record<VaeStatus, string> = {
  Recevabilite: 'blue',
  Accompagnement: 'processing',
  Livret2: 'orange',
  Jury: 'purple',
  ValidationTotale: 'success',
  ValidationPartielle: 'warning',
  Refus: 'error',
};

const CertificationsPage: React.FC = () => {
  const [certifications, setCertifications] = useState<CertificationRncp[]>([]);
  const [certLoading, setCertLoading] = useState(true);
  const [certPage, setCertPage] = useState(1);
  const [certTotal, setCertTotal] = useState(0);

  const [vaeProjects, setVaeProjects] = useState<VaeProject[]>([]);
  const [vaeLoading, setVaeLoading] = useState(true);
  const [vaePage, setVaePage] = useState(1);
  const [vaeTotal, setVaeTotal] = useState(0);

  const [isCertModalOpen, setIsCertModalOpen] = useState(false);
  const [editingCert, setEditingCert] = useState<CertificationRncp | null>(null);

  const loadCertifications = useCallback(async () => {
    setCertLoading(true);
    try {
      const result = await certificationService.getPaged(certPage, 20);
      setCertifications(result.Items);
      setCertTotal(result.TotalCount);
    } catch {
      message.error('Erreur lors du chargement des certifications RNCP');
    } finally {
      setCertLoading(false);
    }
  }, [certPage]);

  const loadVaeProjects = useCallback(async () => {
    setVaeLoading(true);
    try {
      const result = await certificationService.getVaeProjects(vaePage, 20);
      setVaeProjects(result.Items);
      setVaeTotal(result.TotalCount);
    } catch {
      message.error('Erreur lors du chargement des projets VAE');
    } finally {
      setVaeLoading(false);
    }
  }, [vaePage]);

  useEffect(() => {
    loadCertifications();
  }, [loadCertifications]);

  useEffect(() => {
    loadVaeProjects();
  }, [loadVaeProjects]);

  const handleOpenCreate = () => {
    setEditingCert(null);
    setIsCertModalOpen(true);
  };

  const handleOpenEdit = (record: CertificationRncp) => {
    setEditingCert(record);
    setIsCertModalOpen(true);
  };

  const handleCertModalClose = () => {
    setIsCertModalOpen(false);
    setEditingCert(null);
  };

  const handleCertModalSuccess = () => {
    setIsCertModalOpen(false);
    setEditingCert(null);
    loadCertifications();
  };

  const certColumns: ColumnsType<CertificationRncp> = [
    { title: 'Code RNCP', dataIndex: 'RncpCode', key: 'rncpCode', width: 120 },
    { title: 'Titre', dataIndex: 'Titre', key: 'titre', ellipsis: true },
    { title: 'Organisme', dataIndex: 'Organisme', key: 'organisme', width: 200, ellipsis: true },
    { title: 'Niveau', dataIndex: 'NiveauQualification', key: 'niveau', width: 120 },
    {
      title: 'Date enregistrement',
      dataIndex: 'DateEnregistrement',
      key: 'dateEnregistrement',
      width: 160,
      render: (date: string) => dayjs(date).format('DD/MM/YYYY'),
    },
    {
      title: 'Echeance',
      dataIndex: 'DateEcheance',
      key: 'dateEcheance',
      width: 120,
      render: (date?: string) => (date ? dayjs(date).format('DD/MM/YYYY') : '—'),
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
      width: 80,
      render: (_, record) => (
        <Space size="small">
          <Button size="small" icon={<EditOutlined />} onClick={() => handleOpenEdit(record)} />
        </Space>
      ),
    },
  ];

  const vaeColumns: ColumnsType<VaeProject> = [
    { title: 'Employe', dataIndex: 'EmployeeId', key: 'employee', width: 180, ellipsis: true },
    {
      title: 'Certification',
      dataIndex: 'CertificationTitre',
      key: 'certification',
      ellipsis: true,
      render: (titre: string | undefined, record: VaeProject) => titre ?? record.CertificationId,
    },
    {
      title: 'Statut',
      dataIndex: 'Status',
      key: 'status',
      width: 160,
      render: (status: VaeStatus) => (
        <Tag color={vaeStatusColors[status]}>{vaeStatusLabels[status]}</Tag>
      ),
    },
    { title: 'Phase', dataIndex: 'Phase', key: 'phase', width: 140 },
    {
      title: 'Debut',
      dataIndex: 'StartDate',
      key: 'startDate',
      width: 120,
      render: (date: string) => dayjs(date).format('DD/MM/YYYY'),
    },
    {
      title: 'Fin prevue',
      dataIndex: 'ExpectedEndDate',
      key: 'expectedEndDate',
      width: 120,
      render: (date?: string) => (date ? dayjs(date).format('DD/MM/YYYY') : '—'),
    },
  ];

  const rncpTab = (
    <>
      <div style={{ display: 'flex', justifyContent: 'flex-end', marginBottom: 16 }}>
        <Button type="primary" icon={<PlusOutlined />} onClick={handleOpenCreate}>
          Ajouter une certification
        </Button>
      </div>
      <Table<CertificationRncp>
        columns={certColumns}
        dataSource={certifications}
        rowKey="Id"
        loading={certLoading}
        pagination={{ current: certPage, total: certTotal, pageSize: 20, onChange: setCertPage }}
      />
    </>
  );

  const vaeTab = (
    <>
      <Table<VaeProject>
        columns={vaeColumns}
        dataSource={vaeProjects}
        rowKey="Id"
        loading={vaeLoading}
        pagination={{ current: vaePage, total: vaeTotal, pageSize: 20, onChange: setVaePage }}
      />
    </>
  );

  const tabItems = [
    { key: 'rncp', label: 'Certifications RNCP', children: rncpTab },
    { key: 'vae', label: 'Projets VAE', children: vaeTab },
  ];

  return (
    <>
      <h2 style={{ marginBottom: 16 }}>Certifications &amp; VAE</h2>

      <Tabs items={tabItems} />

      <CertificationModal
        visible={isCertModalOpen}
        onCancel={handleCertModalClose}
        onSuccess={handleCertModalSuccess}
        editRecord={editingCert}
      />
    </>
  );
};

export default CertificationsPage;
