import React, { useEffect } from 'react';
import { Modal, Form, Input, InputNumber, Select, message } from 'antd';
import { trainingPlanService } from '../../../services/training';
import { TrainingPlan, TrainingPlanStatus } from '../../../types/training';

const { Option } = Select;
const { TextArea } = Input;

interface TrainingPlanModalProps {
  visible: boolean;
  onCancel: () => void;
  onSuccess: () => void;
  editRecord?: TrainingPlan | null;
}

type TrainingPlanFormValues = {
  Title: string;
  Year: number;
  BudgetAllocated: number;
  Status: TrainingPlanStatus;
  Description?: string;
};

const STATUS_OPTIONS: { value: TrainingPlanStatus; label: string }[] = [
  { value: 'Draft', label: 'Brouillon' },
  { value: 'PendingApproval', label: 'En attente d\'approbation' },
  { value: 'Approved', label: 'Approuve' },
  { value: 'InExecution', label: 'En cours d\'execution' },
  { value: 'Completed', label: 'Termine' },
  { value: 'Archived', label: 'Archive' },
];

const CURRENT_YEAR = new Date().getFullYear();

const TrainingPlanModal: React.FC<TrainingPlanModalProps> = ({
  visible,
  onCancel,
  onSuccess,
  editRecord,
}) => {
  const [form] = Form.useForm<TrainingPlanFormValues>();
  const [saving, setSaving] = React.useState(false);

  useEffect(() => {
    if (!visible) return;
    if (editRecord) {
      form.setFieldsValue({
        Title: editRecord.Title,
        Year: editRecord.Year,
        BudgetAllocated: editRecord.BudgetAllocated,
        Status: editRecord.Status,
        Description: editRecord.Description,
      });
    } else {
      form.resetFields();
      form.setFieldsValue({ Year: CURRENT_YEAR, Status: 'Draft', BudgetAllocated: 0 });
    }
  }, [visible, editRecord, form]);

  const handleOk = async () => {
    const values = await form.validateFields();
    setSaving(true);
    try {
      if (editRecord) {
        await trainingPlanService.update(editRecord.Id, values);
        message.success('Plan de formation mis a jour');
      } else {
        await trainingPlanService.create({ ...values, IsActive: true });
        message.success('Plan de formation cree');
      }
      onSuccess();
    } catch {
      message.error('Erreur lors de la sauvegarde du plan de formation');
    } finally {
      setSaving(false);
    }
  };

  const modalTitle = editRecord ? 'Modifier le plan de formation' : 'Nouveau plan de formation';

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
          name="Title"
          label="Titre du plan"
          rules={[{ required: true, message: 'Le titre est obligatoire' }]}
        >
          <Input placeholder="Ex. : Plan de formation 2026 — Siege social" />
        </Form.Item>

        <Form.Item
          name="Year"
          label="Annee"
          rules={[
            { required: true, message: "L'annee est obligatoire" },
            { type: 'number', min: 2000, max: 2100, message: "Annee invalide" },
          ]}
        >
          <InputNumber
            style={{ width: '100%' }}
            placeholder={String(CURRENT_YEAR)}
            min={2000}
            max={2100}
          />
        </Form.Item>

        <Form.Item
          name="BudgetAllocated"
          label="Budget alloue (EUR)"
          rules={[
            { required: true, message: 'Le budget est obligatoire' },
            { type: 'number', min: 0, message: 'Le budget doit etre positif' },
          ]}
        >
          <InputNumber
            style={{ width: '100%' }}
            placeholder="Ex. : 50000"
            min={0}
            step={1000}
            formatter={(val) => `${val}`.replace(/\B(?=(\d{3})+(?!\d))/g, ' ')}
          />
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

        <Form.Item name="Description" label="Description">
          <TextArea
            rows={4}
            placeholder="Objectifs, perimetre, contexte du plan de formation..."
          />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default TrainingPlanModal;
