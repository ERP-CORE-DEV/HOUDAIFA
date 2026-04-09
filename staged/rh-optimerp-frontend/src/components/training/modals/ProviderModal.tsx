import React, { useEffect } from 'react';
import { Modal, Form, Input, Switch, InputNumber, DatePicker, message } from 'antd';
import dayjs from 'dayjs';
import { providerService } from '../../../services/training';
import { Provider } from '../../../services/training/providerService';

interface ProviderModalProps {
  visible: boolean;
  onCancel: () => void;
  onSuccess: () => void;
  editRecord?: Provider | null;
}

type ProviderFormValues = {
  Name: string;
  Siret: string;
  QualiopiCertified: boolean;
  QualiopiExpirationDate?: dayjs.Dayjs;
  AverageRating: number;
  IsActive: boolean;
};

const ProviderModal: React.FC<ProviderModalProps> = ({
  visible,
  onCancel,
  onSuccess,
  editRecord,
}) => {
  const [form] = Form.useForm<ProviderFormValues>();
  const [saving, setSaving] = React.useState(false);

  useEffect(() => {
    if (!visible) return;
    if (editRecord) {
      form.setFieldsValue({
        Name: editRecord.Name,
        Siret: editRecord.Siret,
        QualiopiCertified: editRecord.QualiopiCertified,
        QualiopiExpirationDate: editRecord.QualiopiExpirationDate
          ? dayjs(editRecord.QualiopiExpirationDate)
          : undefined,
        AverageRating: editRecord.AverageRating,
        IsActive: editRecord.IsActive,
      });
    } else {
      form.resetFields();
      form.setFieldsValue({ QualiopiCertified: false, IsActive: true, AverageRating: 0 });
    }
  }, [visible, editRecord, form]);

  const handleOk = async () => {
    const values = await form.validateFields();
    setSaving(true);
    try {
      const payload = {
        ...values,
        QualiopiExpirationDate: values.QualiopiExpirationDate
          ? values.QualiopiExpirationDate.toISOString()
          : undefined,
        Domaines: editRecord?.Domaines ?? [],
      };
      if (editRecord) {
        await providerService.update(editRecord.Id, payload);
        message.success("Organisme de formation mis a jour");
      } else {
        await providerService.create(payload);
        message.success("Organisme de formation cree");
      }
      onSuccess();
    } catch {
      message.error("Erreur lors de la sauvegarde de l'organisme de formation");
    } finally {
      setSaving(false);
    }
  };

  const modalTitle = editRecord
    ? "Modifier l'organisme de formation"
    : 'Nouvel organisme de formation';

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
          name="Name"
          label="Nom"
          rules={[{ required: true, message: "Le nom de l'organisme est obligatoire" }]}
        >
          <Input placeholder="Nom de l'organisme de formation" />
        </Form.Item>

        <Form.Item
          name="Siret"
          label="Numero de declaration d'activite (SIRET)"
          rules={[{ required: true, message: 'Le numero SIRET est obligatoire' }]}
        >
          <Input placeholder="Ex. : 11 75 XXXXX 75" />
        </Form.Item>

        <Form.Item name="QualiopiCertified" label="Certification Qualiopi" valuePropName="checked">
          <Switch />
        </Form.Item>

        <Form.Item name="QualiopiExpirationDate" label="Date d'expiration Qualiopi">
          <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
        </Form.Item>

        <Form.Item name="AverageRating" label="Note moyenne">
          <InputNumber min={0} max={5} step={0.5} style={{ width: '100%' }} placeholder="0-5" />
        </Form.Item>

        <Form.Item name="IsActive" label="Actif" valuePropName="checked">
          <Switch />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default ProviderModal;
