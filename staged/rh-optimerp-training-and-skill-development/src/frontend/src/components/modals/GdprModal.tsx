import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, message } from 'antd';
import { gdprService } from '../../services/api/gdprService';

const { Option } = Select;
const { TextArea } = Input;

type GdprActionType = 'Anonymize' | 'Export' | 'RetentionReport';

interface GdprModalProps {
  visible: boolean;
  onCancel: () => void;
  onSuccess: () => void;
  editRecord?: { EmployeeId: string; ActionType: GdprActionType } | null;
}

type GdprFormValues = {
  EmployeeId: string;
  ActionType: GdprActionType;
  Notes?: string;
};

const ACTION_TYPE_OPTIONS: { value: GdprActionType; label: string }[] = [
  { value: 'Anonymize', label: 'Anonymisation des donnees' },
  { value: 'Export', label: 'Export des donnees (portabilite)' },
  { value: 'RetentionReport', label: 'Rapport de retention' },
];

const GdprModal: React.FC<GdprModalProps> = ({
  visible,
  onCancel,
  onSuccess,
  editRecord,
}) => {
  const [form] = Form.useForm<GdprFormValues>();
  const [saving, setSaving] = React.useState(false);

  useEffect(() => {
    if (!visible) return;
    if (editRecord) {
      form.setFieldsValue({
        EmployeeId: editRecord.EmployeeId,
        ActionType: editRecord.ActionType,
      });
    } else {
      form.resetFields();
    }
  }, [visible, editRecord, form]);

  const handleOk = async () => {
    const values = await form.validateFields();
    setSaving(true);
    try {
      if (values.ActionType === 'Anonymize') {
        await gdprService.anonymizeEmployee(values.EmployeeId);
        message.success('Demande d\'anonymisation traitee');
      } else if (values.ActionType === 'Export') {
        await gdprService.exportEmployeeData(values.EmployeeId);
        message.success('Export des donnees effectue');
      } else {
        await gdprService.getRetentionReport();
        message.success('Rapport de retention genere');
      }
      onSuccess();
    } catch {
      message.error('Erreur lors du traitement de la demande RGPD');
    } finally {
      setSaving(false);
    }
  };

  const modalTitle = 'Nouvelle demande RGPD';

  return (
    <Modal
      title={modalTitle}
      open={visible}
      onOk={handleOk}
      onCancel={onCancel}
      okText="Executer"
      cancelText="Annuler"
      confirmLoading={saving}
      width={560}
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item
          name="EmployeeId"
          label="Collaborateur"
          rules={[{ required: true, message: 'Le collaborateur est obligatoire' }]}
        >
          <Input placeholder="Identifiant du collaborateur" />
        </Form.Item>

        <Form.Item
          name="ActionType"
          label="Type de demande RGPD"
          rules={[{ required: true, message: 'Le type de demande est obligatoire' }]}
        >
          <Select placeholder="Selectionner un type">
            {ACTION_TYPE_OPTIONS.map((opt) => (
              <Option key={opt.value} value={opt.value}>
                {opt.label}
              </Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item name="Notes" label="Notes">
          <TextArea rows={3} placeholder="Notes sur la demande RGPD..." />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default GdprModal;
