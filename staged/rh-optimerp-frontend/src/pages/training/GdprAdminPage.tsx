import React, { useEffect, useState } from 'react';
import {
  Button,
  Card,
  Col,
  Divider,
  Input,
  Modal,
  Row,
  Spin,
  Statistic,
  message,
} from 'antd';
import {
  UserDeleteOutlined,
  ExportOutlined,
  FileSearchOutlined,
} from '@ant-design/icons';
import { gdprService } from '../../services/training';
import { DataRetentionReportDto } from '../../services/training/gdprService';

const GdprAdminPage: React.FC = () => {
  const [anonymizeEmployeeId, setAnonymizeEmployeeId] = useState('');
  const [exportEmployeeId, setExportEmployeeId] = useState('');
  const [loadingAnonymize, setLoadingAnonymize] = useState(false);
  const [loadingExport, setLoadingExport] = useState(false);
  const [loadingReport, setLoadingReport] = useState(true);
  const [retentionReport, setRetentionReport] = useState<DataRetentionReportDto | null>(null);

  const loadRetentionReport = async () => {
    setLoadingReport(true);
    try {
      const report = await gdprService.getRetentionReport();
      setRetentionReport(report);
    } catch {
      message.error('Erreur lors du chargement du rapport de retention');
    } finally {
      setLoadingReport(false);
    }
  };

  useEffect(() => {
    loadRetentionReport();
  }, []);

  const handleAnonymize = () => {
    if (!anonymizeEmployeeId.trim()) {
      message.warning('Veuillez saisir un identifiant employe');
      return;
    }
    Modal.confirm({
      title: 'Confirmer l\'anonymisation',
      content: `Toutes les donnees personnelles de l\'employe ${anonymizeEmployeeId} seront anonymisees de facon irreversible. Confirmer ?`,
      okText: 'Anonymiser',
      okType: 'danger',
      cancelText: 'Annuler',
      onOk: async () => {
        setLoadingAnonymize(true);
        try {
          await gdprService.anonymizeEmployee(anonymizeEmployeeId.trim());
          message.success(`Donnees de l\'employe ${anonymizeEmployeeId} anonymisees avec succes`);
          setAnonymizeEmployeeId('');
          loadRetentionReport();
        } catch {
          message.error('Erreur lors de l\'anonymisation');
        } finally {
          setLoadingAnonymize(false);
        }
      },
    });
  };

  const handleExport = async () => {
    if (!exportEmployeeId.trim()) {
      message.warning('Veuillez saisir un identifiant employe');
      return;
    }
    setLoadingExport(true);
    try {
      const exportData = await gdprService.exportEmployeeData(exportEmployeeId.trim());
      const blob = new Blob([JSON.stringify(exportData, null, 2)], {
        type: 'application/json',
      });
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `rgpd-export-${exportEmployeeId.trim()}-${new Date().toISOString().slice(0, 10)}.json`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);
      message.success('Export RGPD telecharge');
    } catch {
      message.error('Erreur lors de l\'export des donnees');
    } finally {
      setLoadingExport(false);
    }
  };

  return (
    <>
      <div style={{ marginBottom: 24 }}>
        <h2>Administration RGPD</h2>
      </div>

      <Row gutter={[24, 24]}>
        <Col xs={24} md={12}>
          <Card
            title={
              <span>
                <UserDeleteOutlined style={{ marginRight: 8, color: '#ff4d4f' }} />
                Anonymisation des donnees
              </span>
            }
            bordered
          >
            <p style={{ color: '#595959', marginBottom: 16 }}>
              Anonymise toutes les donnees personnelles d&apos;un employe conformement au RGPD.
              Cette operation est <strong>irreversible</strong>.
            </p>
            <Input
              placeholder="Identifiant employe (ex: EMP-12345)"
              value={anonymizeEmployeeId}
              onChange={(e) => setAnonymizeEmployeeId(e.target.value)}
              style={{ marginBottom: 12 }}
              allowClear
            />
            <Button
              type="primary"
              danger
              icon={<UserDeleteOutlined />}
              loading={loadingAnonymize}
              onClick={handleAnonymize}
              block
            >
              Anonymiser
            </Button>
          </Card>
        </Col>

        <Col xs={24} md={12}>
          <Card
            title={
              <span>
                <ExportOutlined style={{ marginRight: 8, color: '#1890ff' }} />
                Export RGPD
              </span>
            }
            bordered
          >
            <p style={{ color: '#595959', marginBottom: 16 }}>
              Exporte l&apos;ensemble des donnees personnelles d&apos;un employe au format JSON
              (droit a la portabilite — article 20 RGPD).
            </p>
            <Input
              placeholder="Identifiant employe (ex: EMP-12345)"
              value={exportEmployeeId}
              onChange={(e) => setExportEmployeeId(e.target.value)}
              style={{ marginBottom: 12 }}
              allowClear
            />
            <Button
              type="primary"
              icon={<ExportOutlined />}
              loading={loadingExport}
              onClick={handleExport}
              block
            >
              Exporter
            </Button>
          </Card>
        </Col>
      </Row>

      <Divider />

      <Card
        title={
          <span>
            <FileSearchOutlined style={{ marginRight: 8 }} />
            Rapport de retention des donnees
          </span>
        }
        bordered
        extra={
          <Button size="small" onClick={loadRetentionReport} loading={loadingReport}>
            Rafraichir
          </Button>
        }
      >
        {loadingReport ? (
          <div style={{ textAlign: 'center', padding: 40 }}>
            <Spin size="large" />
          </div>
        ) : retentionReport ? (
          <>
            <Row gutter={[16, 16]}>
              <Col xs={12} sm={8} md={4}>
                <Statistic
                  title="Total employes"
                  value={retentionReport.TotalEmployees}
                />
              </Col>
              <Col xs={12} sm={8} md={4}>
                <Statistic
                  title="Anonymises"
                  value={retentionReport.AnonymizedCount}
                  valueStyle={{ color: '#52c41a' }}
                />
              </Col>
              <Col xs={12} sm={8} md={4}>
                <Statistic
                  title="En attente d'anonymisation"
                  value={retentionReport.PendingAnonymizationCount}
                  valueStyle={{ color: '#faad14' }}
                />
              </Col>
              <Col xs={12} sm={8} md={4}>
                <Statistic
                  title="En retard"
                  value={retentionReport.OverdueCount}
                  valueStyle={{ color: '#ff4d4f' }}
                />
              </Col>
              <Col xs={12} sm={8} md={4}>
                <Statistic
                  title="Duree de retention (ans)"
                  value={retentionReport.RetentionPeriodYears}
                />
              </Col>
              <Col xs={12} sm={8} md={4}>
                <Statistic
                  title="Prochaine revue"
                  value={new Date(retentionReport.NextReviewDate).toLocaleDateString('fr-FR')}
                />
              </Col>
            </Row>
            <p style={{ marginTop: 16, color: '#8c8c8c', fontSize: 12 }}>
              Rapport genere le : {new Date(retentionReport.ReportDate).toLocaleDateString('fr-FR')}
            </p>
          </>
        ) : (
          <p>Aucun rapport disponible.</p>
        )}
      </Card>
    </>
  );
};

export default GdprAdminPage;
