import React, { useState } from 'react';
import { Card, Input, Descriptions, Button, Statistic, Tag, Alert, Spin, message, Row, Col } from 'antd';
import { BankOutlined, EuroCircleOutlined, ArrowUpOutlined } from '@ant-design/icons';
import { cpfService } from '../../services/training';
import { CpfAccount } from '../../types/training';

const { Search } = Input;

const CpfPage: React.FC = () => {
  const [account, setAccount] = useState<CpfAccount | null>(null);
  const [loading, setLoading] = useState(false);
  const [creditLoading, setCreditLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [searchedId, setSearchedId] = useState<string>('');

  const handleSearch = async (employeeId: string) => {
    if (!employeeId.trim()) return;
    setLoading(true);
    setError(null);
    setAccount(null);
    setSearchedId(employeeId.trim());
    try {
      const data = await cpfService.getByEmployeeId(employeeId.trim());
      setAccount(data);
    } catch {
      setError(`Aucun compte CPF trouve pour l'employe : ${employeeId.trim()}`);
    } finally {
      setLoading(false);
    }
  };

  const handleApplyAnnualCredit = async () => {
    if (!account) return;
    setCreditLoading(true);
    try {
      const updated = await cpfService.applyAnnualCredit(account.Id);
      setAccount(updated);
      message.success('Credit annuel applique avec succes');
    } catch {
      message.error('Erreur lors de l\'application du credit annuel');
    } finally {
      setCreditLoading(false);
    }
  };

  const utilisationPercent =
    account && account.CeilingEuros > 0
      ? Math.round((account.BalanceEuros / account.CeilingEuros) * 100)
      : 0;

  return (
    <>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 24 }}>
        <h2>Compte CPF</h2>
      </div>

      <Card style={{ marginBottom: 24 }}>
        <Search
          placeholder="Saisir l'identifiant employe..."
          enterButton="Rechercher"
          size="large"
          onSearch={handleSearch}
          loading={loading}
          style={{ maxWidth: 480 }}
        />
      </Card>

      {loading && (
        <div style={{ textAlign: 'center', padding: 64 }}>
          <Spin size="large" />
        </div>
      )}

      {error && !loading && (
        <Alert type="warning" message={error} showIcon style={{ marginBottom: 16 }} />
      )}

      {account && !loading && (
        <>
          <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
            <Col xs={24} sm={8}>
              <Card>
                <Statistic
                  title="Solde"
                  value={account.BalanceEuros}
                  suffix="EUR"
                  prefix={<EuroCircleOutlined />}
                  precision={2}
                  valueStyle={{ color: '#3f8600' }}
                />
              </Card>
            </Col>
            <Col xs={24} sm={8}>
              <Card>
                <Statistic
                  title="Plafond"
                  value={account.CeilingEuros}
                  suffix="EUR"
                  prefix={<BankOutlined />}
                  precision={2}
                />
              </Card>
            </Col>
            <Col xs={24} sm={8}>
              <Card>
                <Statistic
                  title="Credit annuel"
                  value={account.AnnualCreditEuros}
                  suffix="EUR"
                  prefix={<ArrowUpOutlined />}
                  precision={2}
                  valueStyle={{ color: '#1890ff' }}
                />
              </Card>
            </Col>
          </Row>

          <Card
            title="Detail du compte CPF"
            extra={
              <Button
                type="primary"
                icon={<ArrowUpOutlined />}
                loading={creditLoading}
                onClick={handleApplyAnnualCredit}
              >
                Appliquer le credit annuel
              </Button>
            }
          >
            <Descriptions bordered column={2}>
              <Descriptions.Item label="Identifiant employe">{account.EmployeeId}</Descriptions.Item>
              <Descriptions.Item label="Identifiant compte">{account.Id}</Descriptions.Item>
              <Descriptions.Item label="Solde actuel">
                {account.BalanceEuros.toLocaleString('fr-FR', { style: 'currency', currency: 'EUR' })}
              </Descriptions.Item>
              <Descriptions.Item label="Plafond">
                {account.CeilingEuros.toLocaleString('fr-FR', { style: 'currency', currency: 'EUR' })}
              </Descriptions.Item>
              <Descriptions.Item label="Credit annuel">
                {account.AnnualCreditEuros.toLocaleString('fr-FR', { style: 'currency', currency: 'EUR' })}
              </Descriptions.Item>
              <Descriptions.Item label="Taux d'utilisation">
                {utilisationPercent} %
              </Descriptions.Item>
              <Descriptions.Item label="Travailleur peu qualifie">
                <Tag color={account.IsLowQualified ? 'orange' : 'default'}>
                  {account.IsLowQualified ? 'Oui — credit majore' : 'Non'}
                </Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Employe recherche">{searchedId}</Descriptions.Item>
            </Descriptions>
          </Card>
        </>
      )}
    </>
  );
};

export default CpfPage;
