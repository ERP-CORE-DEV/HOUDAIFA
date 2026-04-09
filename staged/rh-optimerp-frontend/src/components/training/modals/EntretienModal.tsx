import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, DatePicker, message } from 'antd';
import dayjs from 'dayjs';
import { entretienService } from '../../../services/training';
import { ProfessionalInterview, InterviewType } from '../../../types/training';

const { Option } = Select;
const { TextArea } = Input;

interface EntretienModalProps {
  visible: boolean;
  onCancel: () => void;
  onSuccess: () => void;
  editRecord?: ProfessionalInterview | null;
}

type EntretienFormValues = {
  EmployeeId: string;
  ManagerId: string;
  Type: InterviewType;
  ScheduledDate: dayjs.Dayjs;
  Status: string;
  Notes?: string;
};

const TYPE_OPTIONS: { value: InterviewType; label: string }[] = [
  { value: 'Biennial', label: 'Entretien professionnel biennal' },
  { value: 'SixYearReview', label: 'Etat des lieux a 6 ans' },
  { value: 'PostAbsence', label: 'Retour de longue absence' },
  { value: 'Voluntary', label: 'Entretien volontaire' },
];

const STATUS_OPTIONS = [
  { value: 'Planifie', label: 'Planifie' },
  { value: 'EnCours', label: 'En cours' },
  { value: 'Realise', label: 'Realise' },
  { value: 'Annule', label: 'Annule' },
];

const EntretienModal: React.FC<EntretienModalProps> = ({
  visible,
  onCancel,
  onSuccess,
  editRecord,
}) => {
  const [form] = Form.useForm<EntretienFormValues>();
  const [saving, setSaving] = React.useState(false);

  useEffect(() => {
    if (!visible) return;
    if (editRecord) {
      form.setFieldsValue({
        EmployeeId: editRecord.EmployeeId,
        ManagerId: editRecord.ManagerId,
        Type: editRecord.Type,
        ScheduledDate: dayjs(editRecord.ScheduledDate),
        Status: editRecord.Status,
      });
    } else {
      form.resetFields();
      form.setFieldsValue({ Status: 'Planifie' });
    }
  }, [visible, editRecord, form]);

  const handleOk = async () => {
    const values = await form.validateFields();
    setSaving(true);
    try {
      const payload: Partial<ProfessionalInterview> = {
        EmployeeId: values.EmployeeId,
        ManagerId: values.ManagerId,
        Type: values.Type,
        ScheduledDate: values.ScheduledDate.toISOString(),
        Status: values.Status,
      };
      if (editRecord) {
        await entretienService.update(editRecord.Id, payload);
        message.success('Entretien mis a jour');
      } else {
        await entretienService.create(payload);
        message.success('Entretien planifie');
      }
      onSuccess();
    } catch {
      message.error("Erreur lors de la sauvegarde de l'entretien");
    } finally {
      setSaving(false);
    }
  };

  const modalTitle = editRecord ? "Modifier l'entretien professionnel" : 'Planifier un entretien professionnel';

  return (
    <Modal
      title={modalTitle}
      open={visible}
      onOk={handleOk}
      onCancel={onCancel}
      okText={editRecord ? 'Mettre a jour' : 'Planifier'}
      cancelText="Annuler"
      confirmLoading={saving}
      width={540}
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item
          name="EmployeeId"
          label="Identifiant employe"
          rules={[{ required: true, message: "L'identifiant employe est obligatoire" }]}
        >
          <Input placeholder="ID de l'employe concerne" />
        </Form.Item>

        <Form.Item
          name="ManagerId"
          label="Identifiant manager conducteur"
          rules={[{ required: true, message: "L'identifiant manager est obligatoire" }]}
        >
          <Input placeholder="ID du manager conducteur" />
        </Form.Item>

        <Form.Item
          name="Type"
          label="Type d'entretien"
          rules={[{ required: true, message: "Le type est obligatoire" }]}
        >
          <Select placeholder="Selectionner le type d'entretien">
            {TYPE_OPTIONS.map((opt) => (
              <Option key={opt.value} value={opt.value}>
                {opt.label}
              </Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item
          name="ScheduledDate"
          label="Date planifiee"
          rules={[{ required: true, message: 'La date planifiee est obligatoire' }]}
        >
          <DatePicker
            style={{ width: '100%' }}
            format="DD/MM/YYYY"
            placeholder="Selectionner une date"
          />
        </Form.Item>

        <Form.Item
          name="Status"
          label="Statut"
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

        <Form.Item name="Notes" label="Notes et observations">
          <TextArea rows={4} placeholder="Observations, points abordes, actions a suivre..." />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default EntretienModal;
