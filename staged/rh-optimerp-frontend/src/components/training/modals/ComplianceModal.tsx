import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, DatePicker, message } from 'antd';
import dayjs from 'dayjs';
import { complianceService } from '../../../services/training';
import {
  ComplianceObligation,
  ComplianceStatus,
  RiskLevel,
  ObligationType,
} from '../../../services/training/complianceService';

const { Option } = Select;
const { TextArea } = Input;

interface ComplianceModalProps {
  visible: boolean;
  onCancel: () => void;
  onSuccess: () => void;
  editRecord?: ComplianceObligation | null;
}

type ComplianceFormValues = {
  Title: string;
  ObligationType: ObligationType;
  Description?: string;
  RiskLevel: RiskLevel;
  Status: ComplianceStatus;
  Echeance: dayjs.Dayjs;
  ResponsableNom?: string;
  ResponsableId?: string;
};

const OBLIGATION_TYPE_OPTIONS: { value: ObligationType; label: string }[] = [
  { value: 'Reglementaire', label: 'Reglementaire (code du travail, decrets...)' },
  { value: 'Conventionnelle', label: 'Conventionnelle (convention collective)' },
  { value: 'Interne', label: 'Interne (reglement, charte entreprise)' },
];

const RISK_LEVEL_OPTIONS: { value: RiskLevel; label: string }[] = [
  { value: 'Faible', label: 'Faible' },
  { value: 'Moyen', label: 'Moyen' },
  { value: 'Eleve', label: 'Eleve' },
  { value: 'Critique', label: 'Critique' },
];

const STATUS_OPTIONS: { value: ComplianceStatus; label: string }[] = [
  { value: 'EnCours', label: 'En cours de traitement' },
  { value: 'Conforme', label: 'Conforme' },
  { value: 'NonConforme', label: 'Non conforme' },
];

const ComplianceModal: React.FC<ComplianceModalProps> = ({
  visible,
  onCancel,
  onSuccess,
  editRecord,
}) => {
  const [form] = Form.useForm<ComplianceFormValues>();
  const [saving, setSaving] = React.useState(false);

  useEffect(() => {
    if (!visible) return;
    if (editRecord) {
      form.setFieldsValue({
        Title: editRecord.Title,
        ObligationType: editRecord.ObligationType,
        Description: editRecord.Description,
        RiskLevel: editRecord.RiskLevel,
        Status: editRecord.Status,
        Echeance: dayjs(editRecord.Echeance),
        ResponsableNom: editRecord.ResponsableNom,
        ResponsableId: editRecord.ResponsableId,
      });
    } else {
      form.resetFields();
      form.setFieldsValue({ Status: 'EnCours', RiskLevel: 'Moyen' });
    }
  }, [visible, editRecord, form]);

  const handleOk = async () => {
    const values = await form.validateFields();
    setSaving(true);
    try {
      const payload: Partial<ComplianceObligation> = {
        Title: values.Title,
        ObligationType: values.ObligationType,
        Description: values.Description,
        RiskLevel: values.RiskLevel,
        Status: values.Status,
        Echeance: values.Echeance.toISOString(),
        ResponsableNom: values.ResponsableNom,
        ResponsableId: values.ResponsableId,
        IsActive: true,
      };
      if (editRecord) {
        await complianceService.update(editRecord.Id, payload);
        message.success('Obligation de conformite mise a jour');
      } else {
        await complianceService.create(payload);
        message.success('Obligation de conformite creee');
      }
      onSuccess();
    } catch {
      message.error("Erreur lors de la sauvegarde de l'obligation");
    } finally {
      setSaving(false);
    }
  };

  const modalTitle = editRecord
    ? "Modifier l'obligation de conformite"
    : 'Nouvelle obligation de conformite';

  return (
    <Modal
      title={modalTitle}
      open={visible}
      onOk={handleOk}
      onCancel={onCancel}
      okText={editRecord ? 'Mettre a jour' : 'Creer'}
      cancelText="Annuler"
      confirmLoading={saving}
      width={580}
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item
          name="Title"
          label="Intitule de l'obligation"
          rules={[{ required: true, message: "L'intitule est obligatoire" }]}
        >
          <Input placeholder="Ex. : Formation securite au travail obligatoire" />
        </Form.Item>

        <Form.Item
          name="ObligationType"
          label="Type d'obligation"
          rules={[{ required: true, message: "Le type d'obligation est obligatoire" }]}
        >
          <Select placeholder="Selectionner le type">
            {OBLIGATION_TYPE_OPTIONS.map((opt) => (
              <Option key={opt.value} value={opt.value}>
                {opt.label}
              </Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item name="Description" label="Description de l'obligation">
          <TextArea
            rows={3}
            placeholder="Detail de l'obligation reglementaire, source juridique, perimetre..."
          />
        </Form.Item>

        <Form.Item
          name="RiskLevel"
          label="Niveau de risque en cas de non-conformite"
          rules={[{ required: true, message: 'Le niveau de risque est obligatoire' }]}
        >
          <Select placeholder="Selectionner le niveau de risque">
            {RISK_LEVEL_OPTIONS.map((opt) => (
              <Option key={opt.value} value={opt.value}>
                {opt.label}
              </Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item
          name="Status"
          label="Statut de conformite"
          rules={[{ required: true, message: 'Le statut est obligatoire' }]}
        >
          <Select placeholder="Selectionner le statut">
            {STATUS_OPTIONS.map((opt) => (
              <Option key={opt.value} value={opt.value}>
                {opt.label}
              </Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item
          name="Echeance"
          label="Date d'echeance"
          rules={[{ required: true, message: "L'echeance est obligatoire" }]}
        >
          <DatePicker
            style={{ width: '100%' }}
            format="DD/MM/YYYY"
            placeholder="Date limite de mise en conformite"
          />
        </Form.Item>

        <Form.Item name="ResponsableNom" label="Responsable de suivi">
          <Input placeholder="Nom du responsable de la mise en conformite" />
        </Form.Item>

        <Form.Item name="ResponsableId" label="Identifiant responsable">
          <Input placeholder="ID du responsable" />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default ComplianceModal;
