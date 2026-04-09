import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, message } from 'antd';
import { bilanService, BilanCompetences, BilanStatus, BilanPhase } from '../../services/api/bilanService';

const { Option } = Select;

interface BilanModalProps {
  visible: boolean;
  onCancel: () => void;
  onSuccess: () => void;
  editRecord?: BilanCompetences | null;
}

type BilanFormValues = {
  EmployeeId: string;
  EmployeeNom: string;
  Phase: BilanPhase;
  Status: BilanStatus;
  StartDate: string;
  EndDate?: string;
  TotalDurationHours: number;
  MaxHours: number;
  OrganismeNom?: string;
};

const PHASE_OPTIONS: { value: BilanPhase; label: string }[] = [
  { value: 'Preliminaire', label: 'Preliminaire' },
  { value: 'Investigation', label: 'Investigation' },
  { value: 'Conclusion', label: 'Conclusion' },
];

const STATUS_OPTIONS: { value: BilanStatus; label: string }[] = [
  { value: 'EnCours', label: 'En cours' },
  { value: 'Suspendu', label: 'Suspendu' },
  { value: 'Termine', label: 'Termine' },
  { value: 'Abandonne', label: 'Abandonne' },
];

const BilanModal: React.FC<BilanModalProps> = ({
  visible,
  onCancel,
  onSuccess,
  editRecord,
}) => {
  const [form] = Form.useForm<BilanFormValues>();
  const [saving, setSaving] = React.useState(false);

  useEffect(() => {
    if (!visible) return;
    if (editRecord) {
      form.setFieldsValue({
        EmployeeId: editRecord.EmployeeId,
        EmployeeNom: editRecord.EmployeeNom,
        Phase: editRecord.Phase,
        Status: editRecord.Status,
        StartDate: editRecord.StartDate,
        EndDate: editRecord.EndDate,
        TotalDurationHours: editRecord.TotalDurationHours,
        MaxHours: editRecord.MaxHours,
        OrganismeNom: editRecord.OrganismeNom,
      });
    } else {
      form.resetFields();
      form.setFieldsValue({ Status: 'EnCours', Phase: 'Preliminaire' });
    }
  }, [visible, editRecord, form]);

  const handleOk = async () => {
    const values = await form.validateFields();
    setSaving(true);
    try {
      if (editRecord) {
        await bilanService.update(editRecord.Id, values);
        message.success('Bilan de competences mis a jour');
      } else {
        await bilanService.create({ ...values, UsedHours: 0 });
        message.success('Bilan de competences cree');
      }
      onSuccess();
    } catch {
      message.error('Erreur lors de la sauvegarde du bilan de competences');
    } finally {
      setSaving(false);
    }
  };

  const modalTitle = editRecord ? 'Modifier le bilan de competences' : 'Nouveau bilan de competences';

  return (
    <Modal
      title={modalTitle}
      open={visible}
      onOk={handleOk}
      onCancel={onCancel}
      okText={editRecord ? 'Mettre a jour' : 'Creer'}
      cancelText="Annuler"
      confirmLoading={saving}
      width={560}
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item
          name="EmployeeId"
          label="Identifiant collaborateur"
          rules={[{ required: true, message: 'Le collaborateur est obligatoire' }]}
        >
          <Input placeholder="Identifiant du collaborateur" />
        </Form.Item>

        <Form.Item
          name="EmployeeNom"
          label="Nom du collaborateur"
          rules={[{ required: true, message: 'Le nom du collaborateur est obligatoire' }]}
        >
          <Input placeholder="Nom complet" />
        </Form.Item>

        <Form.Item
          name="Phase"
          label="Phase"
          rules={[{ required: true, message: 'La phase est obligatoire' }]}
        >
          <Select placeholder="Selectionner une phase">
            {PHASE_OPTIONS.map((opt) => (
              <Option key={opt.value} value={opt.value}>
                {opt.label}
              </Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item
          name="Status"
          label="Statut"
          rules={[{ required: true, message: 'Le statut est obligatoire' }]}
        >
          <Select placeholder="Selectionner un statut">
            {STATUS_OPTIONS.map((opt) => (
              <Option key={opt.value} value={opt.value}>
                {opt.label}
              </Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item
          name="StartDate"
          label="Date de debut"
          rules={[{ required: true, message: 'La date de debut est obligatoire' }]}
        >
          <Input type="date" />
        </Form.Item>

        <Form.Item name="EndDate" label="Date de fin">
          <Input type="date" />
        </Form.Item>

        <Form.Item name="OrganismeNom" label="Organisme prestataire">
          <Input placeholder="Nom de l'organisme" />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default BilanModal;
