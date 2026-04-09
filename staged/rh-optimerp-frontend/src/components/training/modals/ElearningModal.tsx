import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, Switch, InputNumber, message } from 'antd';
import { elearningService } from '../../../services/training';
import { ElearningCourse } from '../../../services/training/elearningService';

const { Option } = Select;

interface ElearningModalProps {
  visible: boolean;
  onCancel: () => void;
  onSuccess: () => void;
  editRecord?: ElearningCourse | null;
}

type ElearningFormValues = {
  Title: string;
  CourseType: string;
  Platform: string;
  DurationHours: number;
  IsActive: boolean;
};

const COURSE_TYPE_OPTIONS = [
  { value: 'SCORM', label: 'SCORM' },
  { value: 'xAPI', label: 'xAPI' },
  { value: 'Video', label: 'Video' },
  { value: 'Interactive', label: 'Interactif' },
  { value: 'Assessment', label: 'Evaluation' },
];

const ElearningModal: React.FC<ElearningModalProps> = ({
  visible,
  onCancel,
  onSuccess,
  editRecord,
}) => {
  const [form] = Form.useForm<ElearningFormValues>();
  const [saving, setSaving] = React.useState(false);

  useEffect(() => {
    if (!visible) return;
    if (editRecord) {
      form.setFieldsValue({
        Title: editRecord.Title,
        CourseType: editRecord.CourseType,
        Platform: editRecord.Platform,
        DurationHours: editRecord.DurationHours,
        IsActive: editRecord.IsActive,
      });
    } else {
      form.resetFields();
      form.setFieldsValue({ IsActive: true });
    }
  }, [visible, editRecord, form]);

  const handleOk = async () => {
    const values = await form.validateFields();
    setSaving(true);
    try {
      if (editRecord) {
        await elearningService.update(editRecord.Id, values);
        message.success('Cours e-learning mis a jour');
      } else {
        await elearningService.create(values);
        message.success('Cours e-learning cree');
      }
      onSuccess();
    } catch {
      message.error('Erreur lors de la sauvegarde du cours e-learning');
    } finally {
      setSaving(false);
    }
  };

  const modalTitle = editRecord ? 'Modifier le cours e-learning' : 'Nouveau cours e-learning';

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
          label="Titre"
          rules={[{ required: true, message: 'Le titre est obligatoire' }]}
        >
          <Input placeholder="Titre du cours e-learning" />
        </Form.Item>

        <Form.Item
          name="CourseType"
          label="Format"
          rules={[{ required: true, message: 'Le format est obligatoire' }]}
        >
          <Select placeholder="Selectionner un format">
            {COURSE_TYPE_OPTIONS.map((opt) => (
              <Option key={opt.value} value={opt.value}>
                {opt.label}
              </Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item
          name="Platform"
          label="Plateforme"
          rules={[{ required: true, message: 'La plateforme est obligatoire' }]}
        >
          <Input placeholder="Ex. : Moodle, 360Learning, Cornerstone" />
        </Form.Item>

        <Form.Item
          name="DurationHours"
          label="Duree (heures)"
          rules={[{ required: true, message: 'La duree est obligatoire' }]}
        >
          <InputNumber min={0} step={0.5} style={{ width: '100%' }} placeholder="1.5" />
        </Form.Item>

        <Form.Item name="IsActive" label="Actif" valuePropName="checked">
          <Switch />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default ElearningModal;
