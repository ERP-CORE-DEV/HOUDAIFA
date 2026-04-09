import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, Switch, message } from 'antd';
import { competencyService } from '../../../services/training';
import { Competency } from '../../../types/training';

const { Option } = Select;
const { TextArea } = Input;

interface CompetencyModalProps {
  visible: boolean;
  onCancel: () => void;
  onSuccess: () => void;
  editRecord?: Competency | null;
}

type CompetencyFormValues = {
  Code: string;
  Name: string;
  Domain: string;
  Family: string;
  Type: string;
  IsCritical: boolean;
  Description?: string;
};

const TYPE_OPTIONS = [
  { value: 'Technique', label: 'Technique' },
  { value: 'Comportementale', label: 'Comportementale (Savoir-etre)' },
  { value: 'Manageriale', label: 'Manageriale' },
  { value: 'Savoir', label: 'Savoir (connaissance theorique)' },
  { value: 'Savoir-faire', label: 'Savoir-faire (competence pratique)' },
];

const CompetencyModal: React.FC<CompetencyModalProps> = ({
  visible,
  onCancel,
  onSuccess,
  editRecord,
}) => {
  const [form] = Form.useForm<CompetencyFormValues>();
  const [saving, setSaving] = React.useState(false);

  useEffect(() => {
    if (!visible) return;
    if (editRecord) {
      form.setFieldsValue({
        Code: editRecord.Code,
        Name: editRecord.Name,
        Domain: editRecord.Domain,
        Family: editRecord.Family,
        Type: editRecord.Type,
        IsCritical: editRecord.IsCritical,
        Description: editRecord.Description,
      });
    } else {
      form.resetFields();
      form.setFieldsValue({ IsCritical: false });
    }
  }, [visible, editRecord, form]);

  const handleOk = async () => {
    const values = await form.validateFields();
    setSaving(true);
    try {
      if (editRecord) {
        await competencyService.update(editRecord.Id, values);
        message.success('Competence mise a jour');
      } else {
        await competencyService.create({ ...values, IsActive: true });
        message.success('Competence creee');
      }
      onSuccess();
    } catch {
      message.error('Erreur lors de la sauvegarde de la competence');
    } finally {
      setSaving(false);
    }
  };

  const modalTitle = editRecord ? 'Modifier la competence' : 'Nouvelle competence';

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
          name="Code"
          label="Code de competence"
          rules={[{ required: true, message: 'Le code est obligatoire' }]}
        >
          <Input placeholder="Ex. : COMP-001" />
        </Form.Item>

        <Form.Item
          name="Name"
          label="Intitule"
          rules={[{ required: true, message: "L'intitule est obligatoire" }]}
        >
          <Input placeholder="Nom de la competence" />
        </Form.Item>

        <Form.Item
          name="Domain"
          label="Domaine"
          rules={[{ required: true, message: 'Le domaine est obligatoire' }]}
        >
          <Input placeholder="Ex. : Technique, Management, Relation client" />
        </Form.Item>

        <Form.Item
          name="Family"
          label="Famille"
          rules={[{ required: true, message: 'La famille est obligatoire' }]}
        >
          <Input placeholder="Ex. : Developpement logiciel, Leadership" />
        </Form.Item>

        <Form.Item
          name="Type"
          label="Type"
          rules={[{ required: true, message: 'Le type est obligatoire' }]}
        >
          <Select placeholder="Selectionner un type">
            {TYPE_OPTIONS.map((opt) => (
              <Option key={opt.value} value={opt.value}>
                {opt.label}
              </Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item name="IsCritical" label="Competence critique" valuePropName="checked">
          <Switch />
        </Form.Item>

        <Form.Item name="Description" label="Description">
          <TextArea rows={3} placeholder="Description detaillee de la competence..." />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default CompetencyModal;
