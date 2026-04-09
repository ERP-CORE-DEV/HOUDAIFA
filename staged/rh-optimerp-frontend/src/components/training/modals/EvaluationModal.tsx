import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, Switch, InputNumber, message } from 'antd';
import { evaluationService } from '../../../services/training';
import { EvaluationRecord } from '../../../services/training/evaluationService';
import { EvaluationLevel } from '../../../types/training';

const { Option } = Select;
const { TextArea } = Input;

interface EvaluationModalProps {
  visible: boolean;
  onCancel: () => void;
  onSuccess: () => void;
  editRecord?: EvaluationRecord | null;
}

type EvaluationFormValues = {
  SessionId: string;
  EvaluateurId: string;
  Level: EvaluationLevel;
  Score: number;
  EvaluationDate: string;
  Comments?: string;
  IsActive: boolean;
};

const LEVEL_OPTIONS: { value: EvaluationLevel; label: string }[] = [
  { value: 'Satisfaction', label: 'Satisfaction' },
  { value: 'Apprentissage', label: 'Apprentissage' },
  { value: 'Transfert', label: 'Transfert' },
  { value: 'Resultats', label: 'Resultats' },
];

const EvaluationModal: React.FC<EvaluationModalProps> = ({
  visible,
  onCancel,
  onSuccess,
  editRecord,
}) => {
  const [form] = Form.useForm<EvaluationFormValues>();
  const [saving, setSaving] = React.useState(false);

  useEffect(() => {
    if (!visible) return;
    if (editRecord) {
      form.setFieldsValue({
        SessionId: editRecord.SessionId,
        EvaluateurId: editRecord.EvaluateurId,
        Level: editRecord.Level,
        Score: editRecord.Score,
        EvaluationDate: editRecord.EvaluationDate,
        Comments: editRecord.Comments,
        IsActive: editRecord.IsActive,
      });
    } else {
      form.resetFields();
      form.setFieldsValue({ IsActive: true, EvaluationDate: new Date().toISOString().split('T')[0] });
    }
  }, [visible, editRecord, form]);

  const handleOk = async () => {
    const values = await form.validateFields();
    setSaving(true);
    try {
      if (editRecord) {
        await evaluationService.update(editRecord.Id, values);
        message.success('Evaluation mise a jour');
      } else {
        await evaluationService.create(values);
        message.success('Evaluation creee');
      }
      onSuccess();
    } catch {
      message.error("Erreur lors de la sauvegarde de l'evaluation");
    } finally {
      setSaving(false);
    }
  };

  const modalTitle = editRecord ? "Modifier l'evaluation" : 'Nouvelle evaluation de formation';

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
          name="SessionId"
          label="Session"
          rules={[{ required: true, message: 'La session est obligatoire' }]}
        >
          <Input placeholder="Identifiant de la session" />
        </Form.Item>

        <Form.Item
          name="EvaluateurId"
          label="Evaluateur"
          rules={[{ required: true, message: "L'evaluateur est obligatoire" }]}
        >
          <Input placeholder="Identifiant de l'evaluateur" />
        </Form.Item>

        <Form.Item
          name="Level"
          label="Type d'evaluation"
          rules={[{ required: true, message: "Le type d'evaluation est obligatoire" }]}
        >
          <Select placeholder="Selectionner un type">
            {LEVEL_OPTIONS.map((opt) => (
              <Option key={opt.value} value={opt.value}>
                {opt.label}
              </Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item
          name="Score"
          label="Score global"
          rules={[{ required: true, message: 'Le score est obligatoire' }]}
        >
          <InputNumber min={0} max={10} step={0.5} style={{ width: '100%' }} placeholder="0-10" />
        </Form.Item>

        <Form.Item
          name="EvaluationDate"
          label="Date d'evaluation"
          rules={[{ required: true, message: "La date d'evaluation est obligatoire" }]}
        >
          <Input type="date" />
        </Form.Item>

        <Form.Item name="Comments" label="Commentaires">
          <TextArea rows={3} placeholder="Commentaires sur l'evaluation..." />
        </Form.Item>

        <Form.Item name="IsActive" label="Actif" valuePropName="checked">
          <Switch />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default EvaluationModal;
