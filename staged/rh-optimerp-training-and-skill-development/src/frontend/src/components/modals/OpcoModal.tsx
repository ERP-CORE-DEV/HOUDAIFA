import React, { useEffect } from 'react';
import { Modal, Form, Input, Switch, message } from 'antd';
import { opcoService, Opco } from '../../services/api/opcoService';

interface OpcoModalProps {
  visible: boolean;
  onCancel: () => void;
  onSuccess: () => void;
  editRecord?: Opco | null;
}

type OpcoFormValues = {
  Code: string;
  Name: string;
  ConventionCollective: string;
  IsActive: boolean;
};

const OpcoModal: React.FC<OpcoModalProps> = ({
  visible,
  onCancel,
  onSuccess,
  editRecord,
}) => {
  const [form] = Form.useForm<OpcoFormValues>();
  const [saving, setSaving] = React.useState(false);

  useEffect(() => {
    if (!visible) return;
    if (editRecord) {
      form.setFieldsValue({
        Code: editRecord.Code,
        Name: editRecord.Name,
        ConventionCollective: editRecord.ConventionCollective,
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
        await opcoService.update(editRecord.Id, values);
        message.success('OPCO mis a jour');
      } else {
        await opcoService.create(values);
        message.success('OPCO cree');
      }
      onSuccess();
    } catch {
      message.error("Erreur lors de la sauvegarde de l'OPCO");
    } finally {
      setSaving(false);
    }
  };

  const modalTitle = editRecord ? "Modifier l'organisme OPCO" : 'Nouvel organisme OPCO';

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
          label="Code OPCO"
          rules={[{ required: true, message: 'Le code OPCO est obligatoire' }]}
        >
          <Input placeholder="Ex. : OPCO-EP" />
        </Form.Item>

        <Form.Item
          name="Name"
          label="Nom"
          rules={[{ required: true, message: 'Le nom est obligatoire' }]}
        >
          <Input placeholder="Nom de l'OPCO" />
        </Form.Item>

        <Form.Item
          name="ConventionCollective"
          label="Convention collective"
          rules={[{ required: true, message: 'La convention collective est obligatoire' }]}
        >
          <Input placeholder="Ex. : IDCC 1486 - Commerce de detail" />
        </Form.Item>

        <Form.Item name="IsActive" label="Actif" valuePropName="checked">
          <Switch />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default OpcoModal;
