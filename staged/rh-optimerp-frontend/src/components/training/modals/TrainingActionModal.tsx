import React, { useEffect } from 'react';
import { Modal, Form, Input, InputNumber, Select, Switch, message } from 'antd';
import { trainingActionService } from '../../../services/training';
import { TrainingAction, TrainingActionType, TrainingModality } from '../../../types/training';

const { Option } = Select;
const { TextArea } = Input;

interface TrainingActionModalProps {
  visible: boolean;
  onCancel: () => void;
  onSuccess: () => void;
  editRecord?: TrainingAction | null;
}

type TrainingActionFormValues = {
  Title: string;
  Type: TrainingActionType;
  Modality: TrainingModality;
  DurationHours: number;
  Cost: number;
  MaxParticipants: number;
  IsObligatory: boolean;
  Description?: string;
};

const TYPE_OPTIONS: { value: TrainingActionType; label: string }[] = [
  { value: 'Obligatoire', label: 'Obligatoire' },
  { value: 'Adaptation', label: 'Adaptation au poste' },
  { value: 'Developpement', label: 'Developpement des competences' },
];

const MODALITY_OPTIONS: { value: TrainingModality; label: string }[] = [
  { value: 'Presentiel', label: 'Presentiel' },
  { value: 'Distanciel', label: 'Distanciel' },
  { value: 'ELearning', label: 'E-Learning' },
  { value: 'Blended', label: 'Blended (mixte)' },
  { value: 'Afest', label: 'AFEST (Action de Formation En Situation de Travail)' },
];

const TrainingActionModal: React.FC<TrainingActionModalProps> = ({
  visible,
  onCancel,
  onSuccess,
  editRecord,
}) => {
  const [form] = Form.useForm<TrainingActionFormValues>();
  const [saving, setSaving] = React.useState(false);

  useEffect(() => {
    if (!visible) return;
    if (editRecord) {
      form.setFieldsValue({
        Title: editRecord.Title,
        Type: editRecord.Type,
        Modality: editRecord.Modality,
        DurationHours: editRecord.DurationHours,
        Cost: editRecord.Cost,
        MaxParticipants: editRecord.MaxParticipants,
        IsObligatory: editRecord.IsObligatory,
        Description: editRecord.Description,
      });
    } else {
      form.resetFields();
      form.setFieldsValue({ IsObligatory: false, Cost: 0, MaxParticipants: 1 });
    }
  }, [visible, editRecord, form]);

  const handleOk = async () => {
    const values = await form.validateFields();
    setSaving(true);
    try {
      if (editRecord) {
        await trainingActionService.update(editRecord.Id, values);
        message.success('Action de formation mise a jour');
      } else {
        await trainingActionService.create({ ...values, IsActive: true });
        message.success('Action de formation creee');
      }
      onSuccess();
    } catch {
      message.error("Erreur lors de la sauvegarde de l'action de formation");
    } finally {
      setSaving(false);
    }
  };

  const modalTitle = editRecord
    ? "Modifier l'action de formation"
    : 'Nouvelle action de formation';

  return (
    <Modal
      title={modalTitle}
      open={visible}
      onOk={handleOk}
      onCancel={onCancel}
      okText={editRecord ? 'Mettre a jour' : 'Creer'}
      cancelText="Annuler"
      confirmLoading={saving}
      width={600}
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item
          name="Title"
          label="Intitule de l'action"
          rules={[{ required: true, message: "L'intitule est obligatoire" }]}
        >
          <Input placeholder="Ex. : Formation securite incendie" />
        </Form.Item>

        <Form.Item
          name="Type"
          label="Categorie"
          rules={[{ required: true, message: 'La categorie est obligatoire' }]}
        >
          <Select placeholder="Selectionner une categorie">
            {TYPE_OPTIONS.map((opt) => (
              <Option key={opt.value} value={opt.value}>
                {opt.label}
              </Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item
          name="Modality"
          label="Modalite"
          rules={[{ required: true, message: 'La modalite est obligatoire' }]}
        >
          <Select placeholder="Selectionner une modalite">
            {MODALITY_OPTIONS.map((opt) => (
              <Option key={opt.value} value={opt.value}>
                {opt.label}
              </Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item
          name="DurationHours"
          label="Duree (heures)"
          rules={[
            { required: true, message: 'La duree est obligatoire' },
            { type: 'number', min: 1, message: 'La duree minimale est 1 heure' },
          ]}
        >
          <InputNumber
            style={{ width: '100%' }}
            placeholder="Ex. : 14"
            min={1}
            step={1}
            addonAfter="h"
          />
        </Form.Item>

        <Form.Item
          name="Cost"
          label="Cout (EUR)"
          rules={[
            { required: true, message: 'Le cout est obligatoire' },
            { type: 'number', min: 0, message: 'Le cout doit etre positif' },
          ]}
        >
          <InputNumber
            style={{ width: '100%' }}
            placeholder="Ex. : 1200"
            min={0}
            step={100}
            formatter={(val) => `${val}`.replace(/\B(?=(\d{3})+(?!\d))/g, ' ')}
          />
        </Form.Item>

        <Form.Item
          name="MaxParticipants"
          label="Participants maximum"
          rules={[
            { required: true, message: 'Le nombre de participants est obligatoire' },
            { type: 'number', min: 1, message: 'Minimum 1 participant' },
          ]}
        >
          <InputNumber
            style={{ width: '100%' }}
            placeholder="Ex. : 15"
            min={1}
            step={1}
          />
        </Form.Item>

        <Form.Item name="IsObligatory" label="Formation obligatoire" valuePropName="checked">
          <Switch />
        </Form.Item>

        <Form.Item name="Description" label="Description">
          <TextArea rows={3} placeholder="Objectifs pedagogiques, contenu, public cible..." />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default TrainingActionModal;
